using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Serialization;

public class GridController : MonoBehaviour
{
    #region Fields
    [Header("Settings")] private int m;
    private int n; // M row N column
    [SerializeField] private BlockTheme theme;

    [Header("References")] [SerializeField]
    private GridAnalyzer analyzer;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BlockVisual blockPrefab;
    [SerializeField] private Transform boardContainer;
    [SerializeField] private BlockPoolManager poolManager;

    
    private float clusteringChance = 0.4f;

    private InputAction _clickAction;
    private InputAction _positionAction;
    private Camera _mainCamera;

    private byte[,] _boardData;
    private BlockVisual[,] _visualBoard;

    private bool _isAnimating = false;
    private readonly float _fallDuration = 0.5f;
    private readonly float _bounceDelay = 0.05f;
    
    #endregion

    
    
    #region Unity Lifecycle
    private void Awake()
    {
        _mainCamera = Camera.main;

        // Set and bind inputs
        _clickAction = new InputAction(type: InputActionType.Button);
        _clickAction.AddBinding("<Mouse>/leftButton");
        _clickAction.AddBinding("<Touchscreen>/primaryTouch/press");

        _positionAction = new InputAction(type: InputActionType.Value, expectedControlType: "Vector2");
        _positionAction.AddBinding("<Mouse>/position");
        _positionAction.AddBinding("<Touchscreen>/primaryTouch/position");

        m = theme.rows;
        n = theme.columns;
        clusteringChance = theme.clusterChance;
    }

    private void OnEnable()
    {
        _clickAction.performed += OnClickPerformed;

        _clickAction.Enable();
        _positionAction.Enable();
    }

    private void OnDisable()
    {
        _clickAction.performed -= OnClickPerformed;

        _clickAction.Disable();
        _positionAction.Disable();
    }

    private void Start()
    {
        analyzer.Initialize(m, n, theme.thresholds);

        SetupGrid();
    }
    #endregion

    
    
    #region Initialization / Setup
    public void SetupGrid()
    {
        _boardData = new byte[m, n];
        _visualBoard = new BlockVisual[m, n];

        GenerateInitialBoard();

        UpdateVisual();
    }

    private void GenerateInitialBoard()
    {
        boardContainer.position = new Vector3(-n / 2f + 0.5f, -m / 2f + 0.5f, 0);

        for (int r = 0; r < m; r++)
        {
            for (int c = 0; c < n; c++)
            {
                _boardData[r, c] = GetWeightedRandomColor(r, c);

                BlockVisual newBlock = poolManager.GetBlock(boardContainer, new Vector3(c, r, 0));
                newBlock.Init(r, c);

                _visualBoard[r, c] = newBlock;
            }
        }
    }
    #endregion
    
    
     
    #region Visual
    private void UpdateVisual()
    {
        bool isDeadlocked = analyzer.AnalyzeGrid(_boardData, m, n, ApplyCorrectSprite);
        if (isDeadlocked)
        {
#if UNITY_EDITOR
            Debug.Log("Deadlock detected. Reshuffling with clustering");
#endif
            ShuffleBoard();
        }
    }
    
    private void ApplyCorrectSprite(int r, int c, int iconType)
    {
        if (_boardData[r, c] == GameConstants.EMPTY) return;

        byte colorIndex = _boardData[r, c];
        Sprite correctSprite = theme.GetSprite(colorIndex, iconType);

        if (_visualBoard[r, c] != null)
        {
            _visualBoard[r, c].UpdateSprite(correctSprite);
        }
    }
    #endregion
    
    private void ShuffleBoard()
    {
        // TODO: add a game over check for small game tables
        List<byte> blockPool = new List<byte>();

        for (int r = 0; r < m; r++)
        {
            for (int c = 0; c < n; c++)
            {
                if (_boardData[r, c] != GameConstants.EMPTY)
                {
                    blockPool.Add(_boardData[r, c]);
                }
            }
        }

        for (int i = blockPool.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            (blockPool[i], blockPool[rnd]) = (blockPool[rnd], blockPool[i]);
        }

        for (int r = 0; r < m; r++)
        {
            for (int c = 0; c < n; c++)
            {
                if (_boardData[r, c] == GameConstants.EMPTY) continue;

                _boardData[r, c] = GetClusteredBlockFromPool(r, c, blockPool);
            }
        }

        UpdateVisual();
    }
    
    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (_isAnimating) return;
        
        if (gameManager != null && gameManager._isGamePaused) return;
        
        Vector2 screenPosition = _positionAction.ReadValue<Vector2>();

        Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition);

        Vector3 localPos = boardContainer.InverseTransformPoint(worldPosition);

        int c = Mathf.RoundToInt(localPos.x);
        int r = Mathf.RoundToInt(localPos.y);

        if (r >= 0 && r < m && c >= 0 && c < n)
        {
#if UNITY_EDITOR
            Debug.Log($"Clicked: r:{r}, c:{c}");
#endif
            StartCoroutine(Blast(r, c));
        }
    }

    private IEnumerator Blast(int r, int c)
    {
        if (_boardData[r, c] == GameConstants.EMPTY) yield break;

        var group = analyzer.GetConnectedGroup(r, c, _boardData, m, n);

        if (group.Count < 2) yield break;

        _isAnimating = true;

        foreach (Vector2Int pos in group)
        {
            if (_visualBoard[pos.x, pos.y])
            {
                poolManager.ReturnBlock(_visualBoard[pos.x, pos.y]);
                _visualBoard[pos.x, pos.y] = null;
            }

            _boardData[pos.x, pos.y] = GameConstants.EMPTY;
        }

        yield return new WaitForSeconds(0.05f);

        ApplyGravityAndRefill();

        yield return new WaitForSeconds(_fallDuration + 0.1f);

        UpdateVisual();

        _isAnimating = false;
    }

    private void ApplyGravityAndRefill()
    {
        // Gravity
        int k = theme.activeColorCount;
        for (int c = 0; c < n; c++)
        {
            // Next empty row
            int writeRow = 0;

            for (int r = 0; r < m; r++)
            {
                // Row is empty. Look next.
                if (_boardData[r, c] == GameConstants.EMPTY) continue;

                // If r == write row, already filled. Increase both
                if (r != writeRow)
                {
                    // Drop nonempty block
                    _boardData[writeRow, c] = _boardData[r, c];
                    _boardData[r, c] = GameConstants.EMPTY;

                    BlockVisual block = _visualBoard[r, c];
                    _visualBoard[writeRow, c] = block;
                    _visualBoard[r, c] = null;


                    if (block)
                    {
                        block.gridPosition = new Vector2Int(writeRow, c);
                        Vector3 targetPos = new Vector3(c, writeRow, 0);
                        block.MovePos(targetPos, _fallDuration);
                    }
                }

                writeRow++;
            }

            // Refill
            // Add new blocks from writeRow to M-1
            for (int r = writeRow; r < m; r++)
            {
                byte newColor = (byte)Random.Range(0, k);
                _boardData[r, c] = newColor;

                float startY = n + 2;
                BlockVisual newBlock = poolManager.GetBlock(boardContainer, new Vector3(c, startY, 0));
                newBlock.UpdateSprite(theme.GetSprite(newColor, 0));
                newBlock.Init(r, c);

                _visualBoard[r, c] = newBlock;

                Vector3 targetPos = new Vector3(c, r, 0);
                newBlock.MovePos(targetPos, _fallDuration + (c*_bounceDelay));
            }
        }
    }

    
    private byte GetWeightedRandomColor(int r, int c)
    {
        int k = theme.activeColorCount;
        byte colorIndex;
        
        // Check is there any adjacent colored block
        byte leftColor = (c > 0) ? _boardData[r, c - 1] : GameConstants.EMPTY;
        byte bottomColor = (r > 0) ? _boardData[r - 1, c] : GameConstants.EMPTY;

        // Are we going to use neighbors color?
        bool useNeighbor = Random.value < clusteringChance && (leftColor != GameConstants.EMPTY || bottomColor != GameConstants.EMPTY);

        if (useNeighbor)
        {
            if (leftColor != GameConstants.EMPTY && bottomColor != GameConstants.EMPTY)
            {
                // If both of them exists, use randomly
                colorIndex = (Random.value > 0.5f) ? leftColor : bottomColor;
            }
            else if (leftColor != GameConstants.EMPTY)
            {
                colorIndex = leftColor;
            }
            else
            {
                colorIndex = bottomColor;
            }
        }
        else
        {
            // Use random color
            colorIndex = (byte)Random.Range(0, k);
        }

        return colorIndex;
    }
    
    private byte GetClusteredBlockFromPool(int r, int c, List<byte> pool)
    {
        byte leftColor = (c > 0) ? _boardData[r, c - 1] : GameConstants.EMPTY;
        byte bottomColor = (r > 0) ? _boardData[r - 1, c] : GameConstants.EMPTY;

        // Add ccandidates to a list
        List<byte> candidates = new List<byte>();
        if (leftColor != GameConstants.EMPTY) candidates.Add(leftColor);
        if (bottomColor != GameConstants.EMPTY) candidates.Add(bottomColor);

        if (candidates.Count > 0 && Random.value < clusteringChance)
        {
            byte preferredColor = candidates[Random.Range(0, candidates.Count)];
            
            // If pool contains preferred color, return it
            if (pool.Contains(preferredColor))
            {
                pool.Remove(preferredColor);
                return preferredColor;
            }
        }
        //else return last element of pool
        int lastIndex = pool.Count - 1;
        byte selectedColor = pool[lastIndex];
        pool.RemoveAt(lastIndex);

        return selectedColor;
    }

}