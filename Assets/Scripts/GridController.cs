using System;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;

public class GridController : MonoBehaviour
{
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

    private void Awake()
    {
        _mainCamera = Camera.main;

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

        for (int r = 0; r < M; r++)
        {
            for (int c = 0; c < N; c++)
            {
                byte randomColorIndex = (byte)Random.Range(0, K);
                _boardData[r, c] = randomColorIndex;

                //Todo: Object Pooling
                BlockVisual newBlock = Instantiate(blockPrefab, boardContainer);
                newBlock.transform.position = new Vector3(c, r, 0);
                newBlock.Init(r, c);

                _visualBoard[r, c] = newBlock;
            }
        }
    }

    private void UpdateVisual()
    {
        analyzer.AnalyzeGrid(_boardData, M, N, ApplyCorrectSprite);
    }

    private void ApplyCorrectSprite(int r, int c, int iconType)
    {
        byte colorIndex = _boardData[r, c];
        Sprite correctSprite = theme.GetSprite(colorIndex, iconType);

        if (_visualBoard[r, c] != null)
        {
            _visualBoard[r, c].UpdateSprite(correctSprite);
        }
    }
    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = _positionAction.ReadValue<Vector2>();

        Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition);

        Vector3 localPos = boardContainer.InverseTransformPoint(worldPosition);

        int c = Mathf.RoundToInt(localPos.x); 
        int r = Mathf.RoundToInt(localPos.y);

        if (r >= 0 && r < M && c >= 0 && c < N)
        {
            Debug.Log($"Clicked: {r},{c}");
            // Blast(r, c); 
        }
    }
}