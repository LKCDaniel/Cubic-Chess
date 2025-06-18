using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.Mathematics;
using static UnityEngine.Mathf;
using System.Collections;

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
    private UIManager uiManager;
    // Game states
    public enum GameState { Entry, Paused, Running, End }
    public GameState CurrentState;

    // Animation times
    [Header("Animation Times")]
    public float pieceMoveTime;
    public float entryTime, cameraMoveTime;

    // Interaction variables
    [Header("Camera Control Settings")]
    public float mouseSensitivity;
    public float scrollSensitivity, clickTolerance;
    public float keyZoomSpeed, keyThetaSpeed, keyPhiSpeed;
    public float minPhi, maxPhi, minRadius, maxRadius;
    private bool isDraggingCamera = false;
    private Vector2 lastMousePosition, initialMousePosition;

    // Sphere coordinate variables
    [Header("Initial Camera Position")]
    public float initRadius;
    public float initTheta, initPhi; // initial horizontal angle 0-360, vertical angle
    private float radius, theta, phi; // current camera angles
    private float storedRadius, storedTheta, storedPhi; // store white and dark camera angles

    // chess board, game variables
    [Header("Chess Board Settings")]
    public float separation; // separation between chess pieces
    ChessPiece[,,] chessBoard; // 4*4*4, X from L to R, Y from B to T, Z from F to B
    private ChessPiece pointedPiece, selectedPiece;
    private Cube pointedCube;
    private List<int3> moveables = new List<int3>(); // List of possible moves for the selected piece
    private List<int3> eatables = new List<int3>(); // List of possible eats for the selected piece
    private bool isWhiteTurn = true;
    private bool inTransition; // Is a piece currently moving

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
        uiManager = UIManager.Instance;

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

    }

    void Start()
    {
        chessBoard = new ChessPiece[4, 4, 4]{ // x, y, z
        {
            { PawnD1, PawnD2, PawnD3, PawnD4}, // x0, y0, z0-z3
            { KnightD1, BishopD1, BishopD2, KnightD2}, // x0, y1, z0-z3
            { PawnD5, PawnD6, PawnD7, PawnD8}, // x0, y2, z0-z3
            { RookD1, RookD2, QueenD, KingD} // x0, y3, z0-z3
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
            { KingW, QueenW, RookW1, RookW2},
            { PawnW1, PawnW2, PawnW3, PawnW4},
            { KnightW1, BishopW1, BishopW2, KnightW2},
            { PawnW5, PawnW6, PawnW7, PawnW8}
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

        foreach (var pawn in new[] { PawnD1, PawnD2, PawnD3, PawnD4, PawnD5, PawnD6, PawnD7, PawnD8 })
            pawn.isL2R = true;

        ChangeState(GameState.Entry);

    }

    void Update()
    {
        KeyboardControl();
        if (CurrentState == GameState.Running)
            PointerControl();
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        switch (CurrentState)
        {
            case GameState.Entry:
                Debug.Log("Game is starting");
                inTransition = true;
                uiManager.FadeShade();
                MoveCamera(initRadius, initTheta, initPhi, entryTime, () =>
                {
                    inTransition = false;
                    radius = storedRadius = initRadius;
                    theta = storedTheta = initTheta;
                    phi = storedPhi = initPhi;
                    ChangeState(GameState.Running);
                });
                break;

            case GameState.Paused:
                Debug.Log("Game is paused");
                Time.timeScale = 0; // pause the game
                break;

            case GameState.Running:
                Debug.Log("Game is running");
                Time.timeScale = 1; // resume the game
                break;

            case GameState.End:
                Debug.Log("Game over!");
                break;
        }

    }

    private void Win(bool isWhite)
    {

    }

    private void PointerControl()
    {
        // No action if pointing at UI, or moving a piece
        if (inTransition) return;

        // Highlighting piece or cube
        if (pointedPiece != null)
        {
            pointedPiece.SetHighLight(false);
            pointedPiece = null;
        }
        if (pointedCube != null)
        {
            pointedCube.SetEnlargeCube(false);
            pointedCube = null;
        }
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit)) // Pointing to piece or cube
        {
            if (hit.collider.CompareTag("Green") || hit.collider.CompareTag("Red"))
            {
                pointedCube = hit.collider.GetComponent<Cube>();
                pointedCube.SetEnlargeCube(true);
            }
            else if (hit.collider.CompareTag("White") && isWhiteTurn || hit.collider.CompareTag("Dark") && !isWhiteTurn)
            {
                pointedPiece = hit.collider.GetComponent<ChessPiece>();
                pointedPiece.SetHighLight(true);
            }
        }

        // Camera dragging selection
        if (isDraggingCamera)
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            Vector2 delta = currentMousePosition - lastMousePosition;
            theta -= delta.x * mouseSensitivity;
            phi += delta.y * mouseSensitivity;
            UpdateCamera();
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
            float mouseDragDistance = Vector2.Distance(initialMousePosition, Mouse.current.position.ReadValue());
            if (mouseDragDistance <= clickTolerance) // click event
            {
                if (pointedPiece != null)
                {
                    selectedPiece = pointedPiece;
                    ClickPiece();
                }
                else if (pointedCube != null)
                {
                    Move(pointedCube.chessPosition.x, pointedCube.chessPosition.y, pointedCube.chessPosition.z);
                    cubeManager.ClearCubes();
                }
            }
        }

    }

    private void KeyboardControl()
    {
        // onclick pause button
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            uiManager.PauseOnClick();

        if (Keyboard.current == null || CurrentState != GameState.Running) return;

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

        UpdateCamera();

    }

    private void ClickPiece()
    {
        moveables.Clear();
        eatables.Clear();
        if (selectedPiece == null)
        {
            cubeManager.ClearCubes();
            return;
        }

        string type = selectedPiece.name.Split(' ')[0];
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 4; z++)
                {
                    int3 selPiecePos = selectedPiece.chessPosition;
                    int sameAxis = (x == selPiecePos.x ? 1 : 0) + (y == selPiecePos.y ? 1 : 0) + (z == selPiecePos.z ? 1 : 0);
                    switch (type)
                    {
                        case "King":
                            if (Abs(x - selPiecePos.x) <= 1 && Abs(y - selPiecePos.y) <= 1 && Abs(z - selPiecePos.z) <= 1)
                                AddMoveable(x, y, z);
                            break;

                        case "Queen":
                            if (x == selPiecePos.x && Abs(y - selPiecePos.y) == Abs(z - selPiecePos.z) ||
                                y == selPiecePos.y && Abs(x - selPiecePos.x) == Abs(z - selPiecePos.z) ||
                                z == selPiecePos.z && Abs(x - selPiecePos.x) == Abs(y - selPiecePos.y) ||
                                sameAxis == 2)
                                AddMoveable(x, y, z);
                            break;

                        case "Rook":
                            if (sameAxis == 2)
                                AddMoveable(x, y, z);
                            break;

                        case "Bishop":
                            if (x == selPiecePos.x && Abs(y - selPiecePos.y) == Abs(z - selPiecePos.z) ||
                                y == selPiecePos.y && Abs(x - selPiecePos.x) == Abs(z - selPiecePos.z) ||
                                z == selPiecePos.z && Abs(x - selPiecePos.x) == Abs(y - selPiecePos.y))
                                AddMoveable(x, y, z);
                            break;

                        case "Knight":
                            if ((Abs(x - selPiecePos.x) == 2 && Abs(y - selPiecePos.y) == 1 && z == selPiecePos.z) ||
                                (Abs(x - selPiecePos.x) == 1 && Abs(y - selPiecePos.y) == 2 && z == selPiecePos.z) ||
                                (Abs(z - selPiecePos.z) == 2 && Abs(x - selPiecePos.x) == 1 && y == selPiecePos.y) ||
                                (Abs(z - selPiecePos.z) == 1 && Abs(x - selPiecePos.x) == 2 && y == selPiecePos.y) ||
                                (Abs(y - selPiecePos.y) == 2 && Abs(z - selPiecePos.z) == 1 && x == selPiecePos.x) ||
                                (Abs(y - selPiecePos.y) == 1 && Abs(z - selPiecePos.z) == 2 && x == selPiecePos.x))
                                AddMoveable(x, y, z);
                            break;

                        case "Pawn":
                            int xMatch = selectedPiece.isL2R ? x - 1 : x + 1;
                            if (xMatch == selPiecePos.x)
                            {
                                if (y == selPiecePos.y && z == selPiecePos.z)
                                {
                                    if (chessBoard[x, y, z] == null)
                                        moveables.Add(new int3(x, y, z));
                                }
                                else if ((Abs(y - selPiecePos.y) == 1 && z == selPiecePos.z || Abs(z - selPiecePos.z) == 1 && y == selPiecePos.y) &&
                                        chessBoard[x, y, z] != null &&
                                        !chessBoard[x, y, z].CompareTag(selectedPiece.tag))
                                    eatables.Add(new int3(x, y, z));
                            }
                            break;
                    }
                }
            }
        }
        cubeManager.SetCubes(moveables, eatables);

        // helper method
        void AddMoveable(int x, int y, int z)
        {
            if (chessBoard[x, y, z] == null)
                moveables.Add(new int3(x, y, z));
            else if (!chessBoard[x, y, z].CompareTag(selectedPiece.tag))
                eatables.Add(new int3(x, y, z));
        }

    }

    private void Move(int x, int y, int z)
    {
        inTransition = true;
        ChessPiece targetPiece = chessBoard[x, y, z];

        if (targetPiece != null)
            targetPiece.Eaten(new(0, 0, 5));

        if (selectedPiece.isL2R && x == 3 || !selectedPiece.isL2R && x == 0) // Pawn promotion / reverse in direction
            selectedPiece.isL2R = !selectedPiece.isL2R;
        else if (targetPiece == KingW) // Dark win
            Win(false);
        else if (targetPiece == KingD) // White win
            Win(true);

        chessBoard[selectedPiece.chessPosition.x, selectedPiece.chessPosition.y, selectedPiece.chessPosition.z] = null;
        selectedPiece.MoveTo(new int3(x, y, z), SwitchTurn);
        chessBoard[x, y, z] = selectedPiece;

        void SwitchTurn()
        {
            MoveCamera(storedRadius, storedTheta, storedPhi, cameraMoveTime, () => { inTransition = false; });
            storedRadius = radius;
            storedTheta = theta;
            storedPhi = phi;
            isWhiteTurn = !isWhiteTurn;
        }

    }

    private void UpdateCamera()
    {
        radius = Clamp(radius, minRadius, maxRadius);
        theta %= 360;
        phi = Clamp(phi, minPhi, maxPhi);
        float x = radius * Sin(phi * Deg2Rad) * Cos(theta * Deg2Rad);
        float y = radius * Cos(phi * Deg2Rad);
        float z = radius * Sin(phi * Deg2Rad) * Sin(theta * Deg2Rad);
        Camera.main.transform.position = new Vector3(x, y, z);
        Camera.main.transform.LookAt(Vector3.zero);
    }

    private void MoveCamera(float targetRadius, float targetTheta, float targetPhi, float duration, System.Action onComplete = null)
    {
        StartCoroutine(MoveCameraCoroutine(targetRadius, targetTheta, targetPhi, duration, onComplete));

        IEnumerator MoveCameraCoroutine(float targetRadius, float targetTheta, float targetPhi, float duration, System.Action onComplete)
        {
            Vector3 startPosition = Camera.main.transform.position;
            float elapsedTime = 0;
            float beginRadius = radius;
            float beginTheta = theta;
            float beginPhi = phi;

            float deltaTheta = targetTheta - theta;
            if (deltaTheta > 180) deltaTheta -= 360;
            else if (deltaTheta < -180) deltaTheta += 360;

            while (elapsedTime < duration)
            {
                float time = SmoothStep(0, 1, elapsedTime / duration);

                radius = Lerp(beginRadius, targetRadius, time);
                theta = Lerp(0, deltaTheta, time) + beginTheta;
                phi = Lerp(beginPhi, targetPhi, time);
                UpdateCamera();
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            radius = targetRadius;
            theta = targetTheta;
            phi = targetPhi;
            UpdateCamera();
            onComplete?.Invoke();
        }

    }


}
