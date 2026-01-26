using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;

public class GridController : MonoBehaviour
{
    private const byte EMPTY = 255;


    [Header("Settings")] 
    [SerializeField] private int M;
    [SerializeField] private int N; // M row N column
    [SerializeField] private BlockTheme theme;

    [Header("References")] [SerializeField]
    private GridAnalyzer analyzer;

    [SerializeField] private BlockVisual blockPrefab;
    [SerializeField] private Transform boardContainer;
    
    private InputAction _clickAction;
    private InputAction _positionAction;
    private Camera _mainCamera;

    private byte[,] _boardData;
    private BlockVisual[,] _visualBoard;

    #region Unity Event Functions
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
        analyzer.Initialize(M, N, theme.thresholds);

        SetupGrid();
    }
    #endregion

    #region Initialization
    public void SetupGrid()
    {
        _boardData = new byte[M, N];
        _visualBoard = new BlockVisual[M, N];

        GenerateInitialBoard();

        UpdateVisual();
    }

    private void GenerateInitialBoard()
    {
        int K = theme.colors.Length;
        boardContainer.position = new Vector3(-N / 2f + 0.5f, -M / 2f + 0.5f, 0);

        for (int r = 0; r < M; r++)
        {
            for (int c = 0; c < N; c++)
            {
                byte randomColorIndex = (byte)Random.Range(0, K);
                _boardData[r, c] = randomColorIndex;

                //Todo: Object Pooling
                BlockVisual newBlock = Instantiate(blockPrefab, boardContainer);
                newBlock.transform.localPosition = new Vector3(c, r, 0);
                newBlock.Init(r, c);

                _visualBoard[r, c] = newBlock;
            }
        }
    }
    #endregion

    #region Visual Updates
    private void UpdateVisual()
    {
        analyzer.AnalyzeGrid(_boardData, M, N, ApplyCorrectSprite);
    }

    // Function to call from analyzer
    private void ApplyCorrectSprite(int r, int c, int iconType)
    {
        if (_boardData[r, c] == EMPTY) return;

        byte colorIndex = _boardData[r, c];
        Sprite correctSprite = theme.GetSprite(colorIndex, iconType);

        if (_visualBoard[r, c] != null)
        {
            _visualBoard[r, c].UpdateSprite(correctSprite);
        }
    }
    #endregion

    #region Interaction
    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = _positionAction.ReadValue<Vector2>();

        Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition);

        Vector3 localPos = boardContainer.InverseTransformPoint(worldPosition);

        int c = Mathf.RoundToInt(localPos.x);
        int r = Mathf.RoundToInt(localPos.y);

        if (r >= 0 && r < M && c >= 0 && c < N)
        {
#if UNITY_EDITOR
            Debug.Log($"Clicked: r:{r}, c:{c}");
#endif
            Blast(r, c);
        }
    }

    private void Blast(int r, int c)
    {
        if (_boardData[r, c] == EMPTY) return;

        var group = analyzer.GetConnectedGroup(r, c, _boardData, M, N);

        if (group.Count < 2) return;

        foreach (Vector2Int pos in group)
        {
            if (_visualBoard[pos.x, pos.y] != null)
            {
                // TODO: Object pooling
                Destroy(_visualBoard[pos.x, pos.y].gameObject);
                _visualBoard[pos.x, pos.y] = null;
            }

            _boardData[pos.x, pos.y] = EMPTY;
        }

        ApplyGravityAndRefill();

        UpdateVisual();
    }

    private void ApplyGravityAndRefill()
    {
        // Gravity
        int K = theme.colors.Length;
        for (int c = 0; c < N; c++)
        {
            // Next empty row
            int writeRow = 0;

            for (int r = 0; r < M; r++)
            {
                // Row is empty. Look next.
                if (_boardData[r, c] == EMPTY) continue;
                
                // If r == write row, already filled. Increase both
                if (r != writeRow)
                {
                    // Drop nonempty block
                    _boardData[writeRow, c] = _boardData[r, c];
                    _boardData[r, c] = EMPTY;

                    BlockVisual block = _visualBoard[r, c];
                    _visualBoard[writeRow, c] = block;
                    _visualBoard[r, c] = null;

                    
                    // TODO: drop animation
                    if (block != null)
                    {
                        block.gridPosition = new Vector2Int(writeRow, c);
                        block.transform.localPosition = new Vector3(c, writeRow, 0);
                    }
                }

                writeRow++;
            }

            // Refill
            // Add new blocks from writeRow to M-1
            for (int r = writeRow; r < M; r++)
            {
                //TODO: Object pooling
                byte newColor = (byte)Random.Range(0, K);
                _boardData[r, c] = newColor;

                BlockVisual newBlock = Instantiate(blockPrefab, boardContainer);

                //TODO: Animation
                newBlock.transform.localPosition = new Vector3(c, r, 0);
                newBlock.Init(r, c);

                _visualBoard[r, c] = newBlock;
            }
        }
    }
    #endregion
}