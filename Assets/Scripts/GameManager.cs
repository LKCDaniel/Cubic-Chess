using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.Mathematics;
using static System.Math;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        Instance = this;
    }


    // Managers
    private CubeManager cubeManager;

    // Game states
    public enum GameState { GameEntry, GamePause, WhiteTurn, BlackTurn, GameEnd }
    private GameState CurrentState;

    // Interaction variables
    private bool isDraggingCamera = false;
    private Vector2 lastMousePosition, initialMousePosition;

    [Header("Camera Spherical Coordinates")]
    public float radius;
    public float theta, phi; // horizontal angle 0-360, vertical angle

    [Header("Camera Control Settings")]
    public float mouseSensitivity;
    public float scrollSensitivity, clickTolerance;
    public float keyZoomSpeed, keyThetaSpeed, keyPhiSpeed;
    public float minPhi, maxPhi, minRadius, maxRadius;

    // chess board, game variables
    [Header("Chess Board Settings")]
    ChessPiece[,,] chessBoard; // 4*4*4, X from L to R, Y from B to T, Z from F to B
    public float separation; // separation between chess pieces
    private ChessPiece selectedPiece;
    private List<int3> moveables = new List<int3>(); // List of possible moves for the selected piece
    private List<int3> eatables = new List<int3>(); // List of possible eats for the selected piece
    private bool[] pawnReversed = new bool[8]; // Track if pawns have been reversed
    private bool isWhiteTurn = true;

    // chess pieces
    ChessPiece
        KingW, KingD,
        QueenW, QueenD,
        RookW1, RookW2, RookD1, RookD2,
        BishopW1, BishopW2, BishopD1, BishopD2,
        KnightW1, KnightW2, KnightD1, KnightD2,
        PawnW1, PawnW2, PawnW3, PawnW4, PawnW5, PawnW6, PawnW7, PawnW8,
        PawnD1, PawnD2, PawnD3, PawnD4, PawnD5, PawnD6, PawnD7, PawnD8;

    // -------------------- Game Variables -----------------------------------------------------------


    void OnEnable()
    {
        cubeManager = CubeManager.Instance;

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
            { PawnD5, PawnD6, PawnD7, PawnD8},
            { KnightD1, BishopD1, BishopD2, KnightD2},
            { RookD1, QueenD, KingD, RookD2}
        }, {
            { null, null, null, null},
            { null, null, null, null},
            { null, null, null, null},
            { null, null, null, null}
        }, {
            { null, null, null, null},
            { null, null, null, null},
            { null, null, null, null},
            { null, null, null, null}
        }, {
            { PawnW1, PawnW2, PawnW3, PawnW4},
            { PawnW5, PawnW6, PawnW7, PawnW8},
            { KnightW1, BishopW1, BishopW2, KnightW2},
            { RookW1, KingW, QueenW, RookW2}
        },};

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 4; z++)
                {
                    if (chessBoard[x, y, z] != null)
                    {
                        chessBoard[x, y, z].SetPosition(new int3(x, y, z));
                        chessBoard[x, y, z].chessPosition = new int3(x, y, z);
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
        foreach (ChessPiece piece in chessBoard)
        {
            if (piece != null)
                piece.SetHighLight(false); // Clear all highlights
        }
        PointerControl();
        KeyboardControl();
    }

    private void PointerControl()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return; // Pointing at UI, no action

        // Piece highlighting
        ChessPiece pointedPiece = null;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) // Pointing to a chess piece
        {
            ChessPiece hitPiece = hit.collider.GetComponent<ChessPiece>();

            // If it is the color's turn, highlight the pointed piece
            if ((hitPiece.CompareTag("White") && isWhiteTurn) || (hitPiece.CompareTag("Dark") && !isWhiteTurn))
            {
                pointedPiece = hitPiece;
                pointedPiece.SetHighLight(true);
            }
        }

        // Camera dragging, piece selection
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
            lastMousePosition = initialMousePosition = Mouse.current.position.ReadValue();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDraggingCamera = false;

            // click event
            float mouseDragDistance = Vector2.Distance(initialMousePosition, Mouse.current.position.ReadValue());
            if (mouseDragDistance <= clickTolerance)
            {
                setSelectedPiece(pointedPiece);
            }
        }

    }

    private void setSelectedPiece(ChessPiece piece)
    {
        selectedPiece = piece;
        moveables.Clear();
        eatables.Clear();
        if (selectedPiece == null)
        {
            cubeManager.ClearCubes();
            return;
        }

        string type = piece.name.Split(' ')[0];
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 4; z++)
                {
                    int3 pieceP = piece.chessPosition;
                    int sameAxis = (x == pieceP.x ? 1 : 0) + (y == pieceP.y ? 1 : 0) + (z == pieceP.z ? 1 : 0);
                    switch (type)
                    {
                        case "King":
                            if (Abs(x - pieceP.x) <= 1 && Abs(y - pieceP.y) <= 1 && Abs(z - pieceP.z) <= 1)
                                AddMoveable(x, y, z, piece);
                            break;

                        case "Queen":
                            if (x == pieceP.x && Abs(y - pieceP.y) == Abs(z - pieceP.z) ||
                                y == pieceP.y && Abs(x - pieceP.x) == Abs(z - pieceP.z) ||
                                z == pieceP.z && Abs(x - pieceP.x) == Abs(y - pieceP.y) ||
                                sameAxis == 2)
                                AddMoveable(x, y, z, piece);
                            break;

                        case "Rook":
                            if (sameAxis == 2)
                                AddMoveable(x, y, z, piece);
                            break;

                        case "Bishop":
                            if (x == pieceP.x && Abs(y - pieceP.y) == Abs(z - pieceP.z) ||
                                y == pieceP.y && Abs(x - pieceP.x) == Abs(z - pieceP.z) ||
                                z == pieceP.z && Abs(x - pieceP.x) == Abs(y - pieceP.y))
                                AddMoveable(x, y, z, piece);
                            break;

                        case "Knight":
                            if ((Abs(x - pieceP.x) == 2 && Abs(y - pieceP.y) == 1 && z == pieceP.z) ||
                                (Abs(x - pieceP.x) == 1 && Abs(y - pieceP.y) == 2 && z == pieceP.z) ||
                                (Abs(z - pieceP.z) == 2 && Abs(x - pieceP.x) == 1 && y == pieceP.y) ||
                                (Abs(z - pieceP.z) == 1 && Abs(x - pieceP.x) == 2 && y == pieceP.y) ||
                                (Abs(y - pieceP.y) == 2 && Abs(z - pieceP.z) == 1 && x == pieceP.x) ||
                                (Abs(y - pieceP.y) == 1 && Abs(z - pieceP.z) == 2 && x == pieceP.x))
                                AddMoveable(x, y, z, piece);
                            break;

                        case "Pawn":
                            bool isPawnL2R = isWhiteTurn == pawnReversed[int.Parse(piece.name.Split(' ')[2]) - 1]; // is pawn moving from left to right
                            int xMatch = isPawnL2R ? x - 1 : x + 1;
                            if (xMatch == pieceP.x)
                            {
                                if (y == pieceP.y && z == pieceP.z)
                                {
                                    if (chessBoard[x, y, z] == null)
                                        moveables.Add(new int3(x, y, z));
                                }
                                else if ((Abs(y - pieceP.y) == 1 && z == pieceP.z || Abs(z - pieceP.z) == 1 && y == pieceP.y) &&
                                        chessBoard[x, y, z] != null &&
                                        chessBoard[x, y, z].CompareTag(piece.CompareTag("White") == isWhiteTurn ? "Dark" : "White"))
                                    eatables.Add(new int3(x, y, z));
                            }
                            break;
                    }

                }
            }
        }

        cubeManager.SetCubes(moveables, eatables);

    }

    private void AddMoveable(int x, int y, int z, ChessPiece piece)
    {
        if (chessBoard[x, y, z] == null)
            moveables.Add(new int3(x, y, z));
        else if (chessBoard[x, y, z].CompareTag(piece.CompareTag("White") == isWhiteTurn ? "Dark" : "White"))
            eatables.Add(new int3(x, y, z));
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
