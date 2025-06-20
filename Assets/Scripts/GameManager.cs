using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Mathematics;
using static UnityEngine.Mathf;
using System.Collections;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    // ------------------------------------ Game Variables -------------------------------------------------------------------

    // Game states
    public enum GameState { Entry, Paused, Running, End }
    [HideInInspector]
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
    private bool isDraggingCamera;
    private Vector2 lastMousePosition, initialMousePosition;

    // Sphere coordinate variables
    [Header("Initial Camera Position")]
    public float initRadius;
    public float initTheta, initPhi; // initial horizontal angle 0-360, vertical angle
    public float cameraYOffset; // camera height offset
    private float radius, theta, phi; // current camera angles
    private float storedRadius, storedTheta, storedPhi; // store white and dark camera angles

    // chess board, game variables
    [Header("Chess Board Settings")]
    public float separation; // separation between chess pieces
    MoveableObject[,,] chessBoard; // 4*4*4, X from L to R, Y from B to T, Z from F to B
    private MoveableObject pointedPiece, selectedPiece;
    private Cube pointedCube;
    private List<int3> moveables = new List<int3>(); // List of possible moves for the selected piece
    private List<int3> eatables = new List<int3>(); // List of possible eats for the selected piece
    [HideInInspector]
    public bool isWhiteTurn = true;
    private bool inTransition; // Is a piece currently moving

    // chess pieces
    MoveableObject
        KingW, KingD,
        QueenW, QueenD,
        RookW1, RookW2, RookD1, RookD2,
        BishopW1, BishopW2, BishopD1, BishopD2,
        KnightW1, KnightW2, KnightD1, KnightD2,
        PawnW1, PawnW2, PawnW3, PawnW4, PawnW5, PawnW6, PawnW7, PawnW8,
        PawnD1, PawnD2, PawnD3, PawnD4, PawnD5, PawnD6, PawnD7, PawnD8;

    // ------------------------------------ Initializations -------------------------------------------------------------------

    void OnEnable()
    {
        KingW = GameObject.Find("King White").GetComponent<MoveableObject>();
        KingD = GameObject.Find("King Dark").GetComponent<MoveableObject>();
        QueenW = GameObject.Find("Queen White").GetComponent<MoveableObject>();
        QueenD = GameObject.Find("Queen Dark").GetComponent<MoveableObject>();
        RookW1 = GameObject.Find("Rook White 1").GetComponent<MoveableObject>();
        RookW2 = GameObject.Find("Rook White 2").GetComponent<MoveableObject>();
        RookD1 = GameObject.Find("Rook Dark 1").GetComponent<MoveableObject>();
        RookD2 = GameObject.Find("Rook Dark 2").GetComponent<MoveableObject>();
        BishopW1 = GameObject.Find("Bishop White 1").GetComponent<MoveableObject>();
        BishopW2 = GameObject.Find("Bishop White 2").GetComponent<MoveableObject>();
        BishopD1 = GameObject.Find("Bishop Dark 1").GetComponent<MoveableObject>();
        BishopD2 = GameObject.Find("Bishop Dark 2").GetComponent<MoveableObject>();
        KnightW1 = GameObject.Find("Knight White 1").GetComponent<MoveableObject>();
        KnightW2 = GameObject.Find("Knight White 2").GetComponent<MoveableObject>();
        KnightD1 = GameObject.Find("Knight Dark 1").GetComponent<MoveableObject>();
        KnightD2 = GameObject.Find("Knight Dark 2").GetComponent<MoveableObject>();
        PawnW1 = GameObject.Find("Pawn White 1").GetComponent<MoveableObject>();
        PawnW2 = GameObject.Find("Pawn White 2").GetComponent<MoveableObject>();
        PawnW3 = GameObject.Find("Pawn White 3").GetComponent<MoveableObject>();
        PawnW4 = GameObject.Find("Pawn White 4").GetComponent<MoveableObject>();
        PawnW5 = GameObject.Find("Pawn White 5").GetComponent<MoveableObject>();
        PawnW6 = GameObject.Find("Pawn White 6").GetComponent<MoveableObject>();
        PawnW7 = GameObject.Find("Pawn White 7").GetComponent<MoveableObject>();
        PawnW8 = GameObject.Find("Pawn White 8").GetComponent<MoveableObject>();
        PawnD1 = GameObject.Find("Pawn Dark 1").GetComponent<MoveableObject>();
        PawnD2 = GameObject.Find("Pawn Dark 2").GetComponent<MoveableObject>();
        PawnD3 = GameObject.Find("Pawn Dark 3").GetComponent<MoveableObject>();
        PawnD4 = GameObject.Find("Pawn Dark 4").GetComponent<MoveableObject>();
        PawnD5 = GameObject.Find("Pawn Dark 5").GetComponent<MoveableObject>();
        PawnD6 = GameObject.Find("Pawn Dark 6").GetComponent<MoveableObject>();
        PawnD7 = GameObject.Find("Pawn Dark 7").GetComponent<MoveableObject>();
        PawnD8 = GameObject.Find("Pawn Dark 8").GetComponent<MoveableObject>();

    }

    void Start()
    {
        // Button-Top arrangement
        chessBoard = new MoveableObject[4, 4, 4]{ // x, y, z
        {
            { RookW1, PawnW1, KnightW1, PawnW2}, // x0, y0, z0-z3
            { null, null, null, null}, // x0, y1, z0-z3
            { null, null, null, null}, // x0, y2, z0-z3
            { PawnD1, KnightD1, PawnD2, KingD} // x0, y3, z0-z3
        }, {
            { RookW2, PawnW3, BishopW1, PawnW4},
            { null, null, null, null},
            { null, null, null, null},
            { PawnD3, BishopD1, PawnD4, QueenD}
        }, {
            { QueenW, PawnW5, BishopW2, PawnW6},
            { null, null, null, null},
            { null, null, null, null},
            { PawnD5, BishopD2, PawnD6, RookD1}
        }, {
            { KingW, PawnW7, KnightW2, PawnW8},
            { null, null, null, null},
            { null, null, null, null},
            { PawnD7, KnightD2, PawnD8, RookD2}
        },};

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 4; z++)
                {
                    if (chessBoard[x, y, z] != null)
                        chessBoard[x, y, z].SetChessPosition(new int3(x, y, z));
                }
            }
        }

        foreach (var pawn in new[] { PawnD1, PawnD2, PawnD3, PawnD4, PawnD5, PawnD6, PawnD7, PawnD8 })
            pawn.isDark2White = true;

        ChangeState(GameState.Entry);

    }

    // ------------------------------------ Game Flow -------------------------------------------------------------------

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
                UIManager.Instance.FadeShade();
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

    // ------------------------------------ Inputs -------------------------------------------------------------------

    private void KeyboardControl()
    {
        // onclick pause button
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            UIManager.Instance.PauseOnClick();

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

    private void PointerControl()
    {
        // Highlighting pieces or cube
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
        if (selectedPiece != null)
            selectedPiece.SetHighLight(true);

        if (!inTransition)
        {
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
                    pointedPiece = hit.collider.GetComponent<MoveableObject>();
                    pointedPiece.SetHighLight(true);
                }
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
                if (selectedPiece != null)
                    selectedPiece.SetHighLight(false);

                if (pointedPiece != null)
                {
                    selectedPiece = pointedPiece;
                    selectedPiece.SetHighLight(true);
                    ClickPiece();
                }
                else if (pointedCube != null)
                {
                    Move(pointedCube.chessPosition.x, pointedCube.chessPosition.y, pointedCube.chessPosition.z);
                    selectedPiece.SetHighLight(false);
                    selectedPiece = null;
                    CubeManager.Instance.ClearCubes();
                }
            }
        }

    }

    // ------------------------------------ Executions -------------------------------------------------------------------

    private void ClickPiece()
    {
        moveables.Clear();
        eatables.Clear();
        string type = selectedPiece.name.Split(' ')[0];
        int3 pos = selectedPiece.chessPosition;

        if (type == "Rook" || type == "Queen")
            FindVerticalMoves(pos);
        if (type == "Bishop" || type == "Queen")
            FindDiagonalMoves(pos);
        else
            FindNoneBlockableMoves(type, pos);

        CubeManager.Instance.SetCubes(moveables, eatables);

    }

    private void Move(int x, int y, int z)
    {
        inTransition = true;
        MoveableObject targetPiece = chessBoard[x, y, z];

        if (targetPiece != null)
            targetPiece.PieceEaten(new Vector3((isWhiteTurn ? 4 : -4) * separation, 0, 0)); // Move the eaten piece to side

        // Pawn promotion / reverse in direction
        if (selectedPiece.name.Split(" ")[0] == "Pawn" &&
            (selectedPiece.isDark2White && y == 0 || !selectedPiece.isDark2White && y == 3))
        {
            selectedPiece.isDark2White = !selectedPiece.isDark2White;
            selectedPiece.UpsideDown();
        }

        if (targetPiece == KingW) // Dark win
            Win(false);
        else if (targetPiece == KingD) // White win
            Win(true);

        chessBoard[selectedPiece.chessPosition.x, selectedPiece.chessPosition.y, selectedPiece.chessPosition.z] = null;
        selectedPiece.MoveTo(new int3(x, y, z), SwitchTurn);
        chessBoard[x, y, z] = selectedPiece;

        void SwitchTurn()
        {
            GridManager.Instance.Revolve();
            foreach (var piece in chessBoard)
            {
                if (piece != null)
                    piece.RevolveAlongAxisZ();
            }
            MoveCamera(storedRadius, storedTheta, storedPhi, cameraMoveTime, () =>
            {
                inTransition = false;
                isWhiteTurn = !isWhiteTurn;
            });
            storedRadius = radius;
            storedTheta = theta;
            storedPhi = phi;
        }

    }

    // ------------------------------------ Camera Movements -------------------------------------------------------------------

    private void UpdateCamera()
    {
        radius = Clamp(radius, minRadius, maxRadius);
        theta %= 360;
        phi = Clamp(phi, minPhi, maxPhi);
        float x = radius * Sin(phi * Deg2Rad) * Cos(theta * Deg2Rad);
        float y = radius * Cos(phi * Deg2Rad);
        float z = radius * Sin(phi * Deg2Rad) * Sin(theta * Deg2Rad);
        Camera.main.transform.position = new Vector3(x, y + cameraYOffset, z);
        Camera.main.transform.LookAt(new Vector3(0, cameraYOffset, 0));
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

    // ------------------------------------ Find Possible Moves ------------------------------------------------------------------------

    void FindVerticalMoves(int3 pos)
    {
        // x+ move
        for (int i = 1; i < 4; i++)
        {
            int x = pos.x + i;
            if (x > 3) break;

            if (chessBoard[x, pos.y, pos.z] == null)
                moveables.Add(new int3(x, pos.y, pos.z));
            else if (!chessBoard[x, pos.y, pos.z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(x, pos.y, pos.z));
                break;
            }
            else
                break;
        }

        // x- move
        for (int i = 1; i < 4; i++)
        {
            int x = pos.x - i;
            if (x < 0) break;

            if (chessBoard[x, pos.y, pos.z] == null)
                moveables.Add(new int3(x, pos.y, pos.z));
            else if (!chessBoard[x, pos.y, pos.z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(x, pos.y, pos.z));
                break;
            }
            else
                break;
        }

        // y+ move
        for (int i = 1; i < 4; i++)
        {
            int y = pos.y + i;
            if (y > 3) break;

            if (chessBoard[pos.x, y, pos.z] == null)
                moveables.Add(new int3(pos.x, y, pos.z));
            else if (!chessBoard[pos.x, y, pos.z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(pos.x, y, pos.z));
                break;
            }
            else
                break;
        }

        // y- move
        for (int i = 1; i < 4; i++)
        {
            int y = pos.y - i;
            if (y < 0) break;

            if (chessBoard[pos.x, y, pos.z] == null)
                moveables.Add(new int3(pos.x, y, pos.z));
            else if (!chessBoard[pos.x, y, pos.z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(pos.x, y, pos.z));
                break;
            }
            else
                break;
        }

        // z+ move
        for (int i = 1; i < 4; i++)
        {
            int z = pos.z + i;
            if (z > 3) break;

            if (chessBoard[pos.x, pos.y, z] == null)
                moveables.Add(new int3(pos.x, pos.y, z));
            else if (!chessBoard[pos.x, pos.y, z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(pos.x, pos.y, z));
                break;
            }
            else
                break;
        }

        // z- move
        for (int i = 1; i < 4; i++)
        {
            int z = pos.z - i;
            if (z < 0) break;

            if (chessBoard[pos.x, pos.y, z] == null)
                moveables.Add(new int3(pos.x, pos.y, z));
            else if (!chessBoard[pos.x, pos.y, z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(pos.x, pos.y, z));
                break;
            }
            else
                break;
        }
    }

    void FindDiagonalMoves(int3 pos)
    {
        // x+y+ move
        for (int i = 1; i < 4; i++)
        {
            int x = pos.x + i;
            int y = pos.y + i;
            if (x > 3 || y > 3) break;

            if (chessBoard[x, y, pos.z] == null)
                moveables.Add(new int3(x, y, pos.z));
            else if (!chessBoard[x, y, pos.z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(x, y, pos.z));
                break;
            }
            else
                break;
        }

        // x+y- move
        for (int i = 1; i < 4; i++)
        {
            int x = pos.x + i;
            int y = pos.y - i;
            if (x > 3 || y < 0) break;

            if (chessBoard[x, y, pos.z] == null)
                moveables.Add(new int3(x, y, pos.z));
            else if (!chessBoard[x, y, pos.z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(x, y, pos.z));
                break;
            }
            else
                break;
        }

        // x-y+ move
        for (int i = 1; i < 4; i++)
        {
            int x = pos.x - i;
            int y = pos.y + i;
            if (x < 0 || y > 3) break;

            if (chessBoard[x, y, pos.z] == null)
                moveables.Add(new int3(x, y, pos.z));
            else if (!chessBoard[x, y, pos.z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(x, y, pos.z));
                break;
            }
            else
                break;
        }

        // x-y- move
        for (int i = 1; i < 4; i++)
        {
            int x = pos.x - i;
            int y = pos.y - i;
            if (x < 0 || y < 0) break;

            if (chessBoard[x, y, pos.z] == null)
                moveables.Add(new int3(x, y, pos.z));
            else if (!chessBoard[x, y, pos.z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(x, y, pos.z));
                break;
            }
            else
                break;
        }

        // x+z+ move
        for (int i = 1; i < 4; i++)
        {
            int x = pos.x + i;
            int z = pos.z + i;
            if (x > 3 || z > 3) break;

            if (chessBoard[x, pos.y, z] == null)
                moveables.Add(new int3(x, pos.y, z));
            else if (!chessBoard[x, pos.y, z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(x, pos.y, z));
                break;
            }
            else
                break;
        }

        // x+z- move
        for (int i = 1; i < 4; i++)
        {
            int x = pos.x + i;
            int z = pos.z - i;
            if (x > 3 || z < 0) break;

            if (chessBoard[x, pos.y, z] == null)
                moveables.Add(new int3(x, pos.y, z));
            else if (!chessBoard[x, pos.y, z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(x, pos.y, z));
                break;
            }
            else
                break;
        }

        // x-z+ move
        for (int i = 1; i < 4; i++)
        {
            int x = pos.x - i;
            int z = pos.z + i;
            if (x < 0 || z > 3) break;

            if (chessBoard[x, pos.y, z] == null)
                moveables.Add(new int3(x, pos.y, z));
            else if (!chessBoard[x, pos.y, z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(x, pos.y, z));
                break;
            }
            else
                break;
        }

        // x-z- move
        for (int i = 1; i < 4; i++)
        {
            int x = pos.x - i;
            int z = pos.z - i;
            if (x < 0 || z < 0) break;

            if (chessBoard[x, pos.y, z] == null)
                moveables.Add(new int3(x, pos.y, z));
            else if (!chessBoard[x, pos.y, z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(x, pos.y, z));
                break;
            }
            else
                break;
        }

        // y+z+ move
        for (int i = 1; i < 4; i++)
        {
            int y = pos.y + i;
            int z = pos.z + i;
            if (y > 3 || z > 3) break;

            if (chessBoard[pos.x, y, z] == null)
                moveables.Add(new int3(pos.x, y, z));
            else if (!chessBoard[pos.x, y, z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(pos.x, y, z));
                break;
            }
            else
                break;
        }

        // y+z- move
        for (int i = 1; i < 4; i++)
        {
            int y = pos.y + i;
            int z = pos.z - i;
            if (y > 3 || z < 0) break;

            if (chessBoard[pos.x, y, z] == null)
                moveables.Add(new int3(pos.x, y, z));
            else if (!chessBoard[pos.x, y, z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(pos.x, y, z));
                break;
            }
            else
                break;
        }

        // y-z+ move
        for (int i = 1; i < 4; i++)
        {
            int y = pos.y - i;
            int z = pos.z + i;
            if (y < 0 || z > 3) break;

            if (chessBoard[pos.x, y, z] == null)
                moveables.Add(new int3(pos.x, y, z));
            else if (!chessBoard[pos.x, y, z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(pos.x, y, z));
                break;
            }
            else
                break;
        }

        // y-z- move
        for (int i = 1; i < 4; i++)
        {
            int y = pos.y - i;
            int z = pos.z - i;
            if (y < 0 || z < 0) break;

            if (chessBoard[pos.x, y, z] == null)
                moveables.Add(new int3(pos.x, y, z));
            else if (!chessBoard[pos.x, y, z].CompareTag(selectedPiece.tag))
            {
                eatables.Add(new int3(pos.x, y, z));
                break;
            }
            else
                break;
        }
    }

    void FindNoneBlockableMoves(string type, int3 pos)
    {
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 4; z++)
                {
                    switch (type)
                    {
                        case "King":
                            if (Abs(x - pos.x) <= 1 && Abs(y - pos.y) <= 1 && Abs(z - pos.z) <= 1)
                                if (chessBoard[x, y, z] == null)
                                    moveables.Add(new int3(x, y, z));
                                else if (!chessBoard[x, y, z].CompareTag(selectedPiece.tag))
                                    eatables.Add(new int3(x, y, z)); ;
                            break;

                        case "Knight":
                            if ((Abs(x - pos.x) == 2 && Abs(y - pos.y) == 1 && z == pos.z) ||
                                (Abs(x - pos.x) == 1 && Abs(y - pos.y) == 2 && z == pos.z) ||
                                (Abs(z - pos.z) == 2 && Abs(x - pos.x) == 1 && y == pos.y) ||
                                (Abs(z - pos.z) == 1 && Abs(x - pos.x) == 2 && y == pos.y) ||
                                (Abs(y - pos.y) == 2 && Abs(z - pos.z) == 1 && x == pos.x) ||
                                (Abs(y - pos.y) == 1 && Abs(z - pos.z) == 2 && x == pos.x))
                                if (chessBoard[x, y, z] == null)
                                    moveables.Add(new int3(x, y, z));
                                else if (!chessBoard[x, y, z].CompareTag(selectedPiece.tag))
                                    eatables.Add(new int3(x, y, z)); ;
                            break;

                        case "Pawn":
                            // Button-Top arrangement
                            int yMatch = selectedPiece.isDark2White ? y + 1 : y - 1; // white is at button
                            if (yMatch == pos.y)
                            {
                                if (x == pos.x && z == pos.z)
                                {
                                    if (chessBoard[x, y, z] == null)
                                        moveables.Add(new int3(x, y, z));
                                }
                                else if ((Abs(x - pos.x) == 1 && z == pos.z || Abs(z - pos.z) == 1 && x == pos.x) &&
                                        chessBoard[x, y, z] != null &&
                                        !chessBoard[x, y, z].CompareTag(selectedPiece.tag))
                                    eatables.Add(new int3(x, y, z));
                            }
                            break;
                    }
                }
            }
        }
    }



}
