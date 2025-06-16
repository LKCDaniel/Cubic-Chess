using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    // Game Variables -----------------------------------------------------------

    // Game states
    public enum GameState { GameEntry, GamePause, WhiteTurn, BlackTurn, GameEnd }
    private GameState CurrentState;

    // Camera
    private bool isDraggingCamera = false;
    private Vector2 lastMousePosition;

    [Header("Camera Spherical Coordinates")]
    public float radius;
    public float theta, phi; // horizontal angle 0-360, vertical angle

    [Header("Camera Control Settings")]
    public float mouseSensitivity;
    public float scrollSensitivity;
    public float keyZoomSpeed, keyThetaSpeed, keyPhiSpeed;
    public float minPhi, maxPhi, minRadius, maxRadius;

    // chess board
    // 4*4*4, X from L to R, Y from B to T, Z from F to B, same as Unity's coordinate system
    ChessPiece[,,] chessBoard;
    [Header("Chess Board Settings")]
    public float separation; // separation between chess pieces
    private ChessPiece pointedPiece;

    // chess pieces
    ChessPiece KingW, KingD,
    QueenW, QueenD,
    RookW1, RookW2, RookD1, RookD2,
    BishopW1, BishopW2, BishopD1, BishopD2,
    KnightW1, KnightW2, KnightD1, KnightD2,
    PawnW1, PawnW2, PawnW3, PawnW4, PawnW5, PawnW6, PawnW7, PawnW8,
    PawnD1, PawnD2, PawnD3, PawnD4, PawnD5, PawnD6, PawnD7, PawnD8;

    // --------------------------------------------------------------------------


    void OnEnable()
    {
        KingW = GameObject.Find("King White").GetComponent<ChessPiece>();
        KingD = GameObject.Find("King Dark").GetComponent<ChessPiece>();
        QueenW = GameObject.Find("Queen White").GetComponent<ChessPiece>();
        QueenD = GameObject.Find("Queen Dark").GetComponent<ChessPiece>();
        RookW1 = GameObject.Find("Rook White 1").GetComponent<ChessPiece>();
        RookW2 = GameObject.Find("Rook White 2").GetComponent<ChessPiece>();
        RookD1 = GameObject.Find("Rook Dark 1").GetComponent<ChessPiece>();
        RookD2 = GameObject.Find("Rook Dark 2").GetComponent<ChessPiece>();
        BishopW1 = GameObject.Find("Bishop White 1").GetComponent<ChessPiece>();
        BishopW2 = GameObject.Find("Bishop White 2").GetComponent<ChessPiece>();
        BishopD1 = GameObject.Find("Bishop Dark 1").GetComponent<ChessPiece>();
        BishopD2 = GameObject.Find("Bishop Dark 2").GetComponent<ChessPiece>();
        KnightW1 = GameObject.Find("Knight White 1").GetComponent<ChessPiece>();
        KnightW2 = GameObject.Find("Knight White 2").GetComponent<ChessPiece>();
        KnightD1 = GameObject.Find("Knight Dark 1").GetComponent<ChessPiece>();
        KnightD2 = GameObject.Find("Knight Dark 2").GetComponent<ChessPiece>();
        PawnW1 = GameObject.Find("Pawn White 1").GetComponent<ChessPiece>();
        PawnW2 = GameObject.Find("Pawn White 2").GetComponent<ChessPiece>();
        PawnW3 = GameObject.Find("Pawn White 3").GetComponent<ChessPiece>();
        PawnW4 = GameObject.Find("Pawn White 4").GetComponent<ChessPiece>();
        PawnW5 = GameObject.Find("Pawn White 5").GetComponent<ChessPiece>();
        PawnW6 = GameObject.Find("Pawn White 6").GetComponent<ChessPiece>();
        PawnW7 = GameObject.Find("Pawn White 7").GetComponent<ChessPiece>();
        PawnW8 = GameObject.Find("Pawn White 8").GetComponent<ChessPiece>();
        PawnD1 = GameObject.Find("Pawn Dark 1").GetComponent<ChessPiece>();
        PawnD2 = GameObject.Find("Pawn Dark 2").GetComponent<ChessPiece>();
        PawnD3 = GameObject.Find("Pawn Dark 3").GetComponent<ChessPiece>();
        PawnD4 = GameObject.Find("Pawn Dark 4").GetComponent<ChessPiece>();
        PawnD5 = GameObject.Find("Pawn Dark 5").GetComponent<ChessPiece>();
        PawnD6 = GameObject.Find("Pawn Dark 6").GetComponent<ChessPiece>();
        PawnD7 = GameObject.Find("Pawn Dark 7").GetComponent<ChessPiece>();
        PawnD8 = GameObject.Find("Pawn Dark 8").GetComponent<ChessPiece>();

        chessBoard = new ChessPiece[4, 4, 4]{ // x, y, z
        {
            {PawnD1, PawnD2, PawnD3, PawnD4},
            {PawnD5, PawnD6, PawnD7, PawnD8},
            {KnightD1, BishopD1, BishopD2, KnightD2},
            {RookD1, QueenD, KingD, RookD2}
        }, {
            {null, null, null, null},
            {null, null, null, null},
            {null, null, null, null},
            {null, null, null, null}
        }, {
            {null, null, null, null},
            {null, null, null, null},
            {null, null, null, null},
            {null, null, null, null}
        }, {
            {PawnW1, PawnW2, PawnW3, PawnW4},
            {PawnW5, PawnW6, PawnW7, PawnW8},
            {KnightW1, BishopW1, BishopW2, KnightW2},
            {RookW1, KingW, QueenW, RookW2}
        },};

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 4; z++)
                {
                    if (chessBoard[x, y, z] != null)
                    {
                        Vector3 p = new Vector3(
                            x * separation - 1.5f * separation,
                            y * separation - 1.5f * separation,
                            z * separation - 1.5f * separation
                        );
                        chessBoard[x, y, z].SetPosition(p);
                        chessBoard[x, y, z].chessX = x;
                        chessBoard[x, y, z].chessY = y;
                        chessBoard[x, y, z].chessZ = z;
                    }
                }
            }
        }
    }

    void Start()
    {
        ChangeState(GameState.GameEntry);
    }

    void Update()
    {
        PointerControl();
        KeyboardControl();
    }

    private void PointerControl()
    {
        // Pointing at UI
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        // Pointing to a chess piece
        if (Physics.Raycast(ray, out hit))
        {
            // Highlight the piece
            ChessPiece hitPiece = hit.collider.GetComponent<ChessPiece>();
            if (pointedPiece != hitPiece && pointedPiece != null)
                pointedPiece.SetHighLight(false);
            pointedPiece = hitPiece;
            hitPiece.SetHighLight(true);
        }
        else // pointing to empty space
        {
            if (pointedPiece != null)
            {
                pointedPiece.SetHighLight(false);
                pointedPiece = null;
            }

            if (isDraggingCamera)
            {
                Vector2 currentMousePosition = Mouse.current.position.ReadValue();
                Vector2 delta = currentMousePosition - lastMousePosition;

                theta -= delta.x * mouseSensitivity;
                theta %= 360;
                phi += delta.y * mouseSensitivity;
                phi = Mathf.Clamp(phi, minPhi, maxPhi);

                SetCamera(radius, theta, phi);

                lastMousePosition = currentMousePosition;
            }
            else if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                isDraggingCamera = true;
                lastMousePosition = Mouse.current.position.ReadValue();
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                isDraggingCamera = false;
            }
        }
    }

    private void KeyboardControl()
    {
        if (Keyboard.current == null) return;

        // onclick pause button
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {

        }

        // zoom camera: R, F
        if (Keyboard.current.rKey.isPressed)
            radius -= keyZoomSpeed * Time.deltaTime;

        if (Keyboard.current.fKey.isPressed)
            radius += keyZoomSpeed * Time.deltaTime;

        // rotate camera: A, D, W, S or Arrow keys
        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            theta -= keyThetaSpeed * Time.deltaTime;

        if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            theta += keyThetaSpeed * Time.deltaTime;

        if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
            phi -= keyPhiSpeed * Time.deltaTime;

        if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
            phi += keyPhiSpeed * Time.deltaTime;

        radius = Mathf.Clamp(radius, minRadius, maxRadius);
        theta %= 360;
        phi = Mathf.Clamp(phi, minPhi, maxPhi);

        SetCamera(radius, theta, phi);
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        Debug.Log($"Changing game state from {CurrentState} to {newState}");
        CurrentState = newState;

        switch (CurrentState)
        {
            case GameState.GameEntry:
                Debug.Log("Game is starting...");
                SetupGame();
                break;
            case GameState.GamePause:
                Debug.Log("Game is paused");
                break;
            case GameState.WhiteTurn:
                Debug.Log("White's turn to play");
                break;
            case GameState.BlackTurn:
                Debug.Log("Black's turn to play");
                break;
            case GameState.GameEnd:
                Debug.Log("Game over!");
                break;
        }
    }

    private void SetupGame()
    {
        // Let the camera sweep across the board
        SetCamera(radius, theta, phi);
        ChangeState(GameState.WhiteTurn);
    }

    private void SetCamera(float radius, float theta, float phi)
    {
        phi *= Mathf.Deg2Rad;
        theta *= Mathf.Deg2Rad;
        float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
        float y = radius * Mathf.Cos(phi);
        float z = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
        Camera.main.transform.position = new Vector3(x, y, z);
        Camera.main.transform.LookAt(Vector3.zero);
    }


}
