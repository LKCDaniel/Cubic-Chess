using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Mathematics;
using static UnityEngine.Mathf;
using System.Collections;
using System;
using System.Linq;

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
    public GameState CurrentState { get; private set; }

    // Animation durations
    [Header("Animation Durations")]
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
    private MoveableObject[,,] chessBoard; // 4*4*4, X from L to R, Y from B to T, Z from F to B
    private MoveableObject pointedPiece, selectedPiece;
    private Cube pointedCube;
    private bool isWhiteTurn, isRevolvedWhite, isRevolvedDark; // Normally, the player starts from the bottom
    public bool isWhiteOnTop => isWhiteTurn && isRevolvedWhite || !isWhiteTurn && !isRevolvedDark;
    private int whiteEats, darkEats;
    public bool inTransition { get; private set; } // Is a piece currently moving

    // keep a record of every move
    private class MoveRecord
    {
        public int3 fromPosition, toPosition;
        public MoveableObject movedPiece, eatenPiece, promotedPiece;

        public MoveRecord(int3 from, int3 to, MoveableObject moved, MoveableObject eaten = null, MoveableObject promoted = null)
        {
            fromPosition = from;
            toPosition = to;
            movedPiece = moved;
            eatenPiece = eaten;
            promotedPiece = promoted;
        }
    }
    private List<MoveRecord> records = new List<MoveRecord>();
    public int currentStep { get; private set; }

    // chess pieces
    private MoveableObject
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
        KingW = Instantiate(Resources.Load<GameObject>("Prefabs/King White")).GetComponent<MoveableObject>();
        KingD = Instantiate(Resources.Load<GameObject>("Prefabs/King Dark")).GetComponent<MoveableObject>();
        QueenW = Instantiate(Resources.Load<GameObject>("Prefabs/Queen White")).GetComponent<MoveableObject>();
        QueenD = Instantiate(Resources.Load<GameObject>("Prefabs/Queen Dark")).GetComponent<MoveableObject>();
        RookW1 = Instantiate(Resources.Load<GameObject>("Prefabs/Rook White")).GetComponent<MoveableObject>();
        RookW2 = Instantiate(Resources.Load<GameObject>("Prefabs/Rook White")).GetComponent<MoveableObject>();
        RookD1 = Instantiate(Resources.Load<GameObject>("Prefabs/Rook Dark")).GetComponent<MoveableObject>();
        RookD2 = Instantiate(Resources.Load<GameObject>("Prefabs/Rook Dark")).GetComponent<MoveableObject>();
        BishopW1 = Instantiate(Resources.Load<GameObject>("Prefabs/Bishop White")).GetComponent<MoveableObject>();
        BishopW2 = Instantiate(Resources.Load<GameObject>("Prefabs/Bishop White")).GetComponent<MoveableObject>();
        BishopD1 = Instantiate(Resources.Load<GameObject>("Prefabs/Bishop Dark")).GetComponent<MoveableObject>();
        BishopD2 = Instantiate(Resources.Load<GameObject>("Prefabs/Bishop Dark")).GetComponent<MoveableObject>();
        KnightW1 = Instantiate(Resources.Load<GameObject>("Prefabs/Knight White")).GetComponent<MoveableObject>();
        KnightW2 = Instantiate(Resources.Load<GameObject>("Prefabs/Knight White")).GetComponent<MoveableObject>();
        KnightD1 = Instantiate(Resources.Load<GameObject>("Prefabs/Knight Dark")).GetComponent<MoveableObject>();
        KnightD2 = Instantiate(Resources.Load<GameObject>("Prefabs/Knight Dark")).GetComponent<MoveableObject>();
        PawnW1 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn White")).GetComponent<MoveableObject>();
        PawnW2 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn White")).GetComponent<MoveableObject>();
        PawnW3 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn White")).GetComponent<MoveableObject>();
        PawnW4 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn White")).GetComponent<MoveableObject>();
        PawnW5 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn White")).GetComponent<MoveableObject>();
        PawnW6 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn White")).GetComponent<MoveableObject>();
        PawnW7 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn White")).GetComponent<MoveableObject>();
        PawnW8 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn White")).GetComponent<MoveableObject>();
        PawnD1 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn Dark")).GetComponent<MoveableObject>();
        PawnD2 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn Dark")).GetComponent<MoveableObject>();
        PawnD3 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn Dark")).GetComponent<MoveableObject>();
        PawnD4 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn Dark")).GetComponent<MoveableObject>();
        PawnD5 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn Dark")).GetComponent<MoveableObject>();
        PawnD6 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn Dark")).GetComponent<MoveableObject>();
        PawnD7 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn Dark")).GetComponent<MoveableObject>();
        PawnD8 = Instantiate(Resources.Load<GameObject>("Prefabs/Pawn Dark")).GetComponent<MoveableObject>();

    }

    void Start()
    {
        isWhiteTurn = true; // white starts first
        chessBoard = new MoveableObject[4, 4, 4]{ // initialize the chessboard: x, y, z
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
            pawn.isPawnDark2White = true;

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
                ShiftCamera(initRadius, initTheta, initPhi, entryTime, () =>
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
                UIManager.Instance.SetShade(0.5f); // show the shade
                break;

            case GameState.Running:
                Debug.Log("Game is running");
                Time.timeScale = 1; // resume the game
                UIManager.Instance.SetShade(0f); // show the shade
                break;

            case GameState.End:
                Debug.Log("Game over!");
                UIManager.Instance.GameFinish(!isWhiteTurn);
                break;
        }

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

        MoveCamera();

    }

    private void PointerControl()
    {
        // find the pointing cube/piece, set highlights
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
            selectedPiece.SetHighLight(!inTransition);

        if (!inTransition)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()); // use a ray at the pointer's position
            if (Physics.Raycast(ray, out RaycastHit hit)) // Pointing to piece or cube
            {
                if (hit.collider.CompareTag("Green") || hit.collider.CompareTag("Red")) // if pointing to a cube
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

        // Camera dragging or selecting piece
        if (isDraggingCamera) // is dragging the camera
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            Vector2 delta = currentMousePosition - lastMousePosition;
            theta -= delta.x * mouseSensitivity;
            phi += delta.y * mouseSensitivity;
            MoveCamera();
            lastMousePosition = currentMousePosition;
        }
        else if (Mouse.current.leftButton.wasPressedThisFrame) // just pressed the left button
        {
            isDraggingCamera = true;
            lastMousePosition = initialMousePosition = Mouse.current.position.ReadValue();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame) // just released the left button
        {
            isDraggingCamera = false;
            float mouseDragDistance = Vector2.Distance(initialMousePosition, Mouse.current.position.ReadValue());
            if (mouseDragDistance <= clickTolerance && !inTransition) // this is a click event
            {
                if (selectedPiece != null)
                    selectedPiece.SetHighLight(false);

                if (pointedPiece != null) // if a piece is clicked, and if the color is right, select the new piece
                {
                    if (pointedPiece.CompareTag("White") && isWhiteTurn || pointedPiece.CompareTag("Dark") && !isWhiteTurn)
                    {
                        selectedPiece = pointedPiece;
                        GetPotentialMoves(selectedPiece);
                    }
                }
                else // click at a cube or nothing
                {
                    if (pointedCube != null) // if a cube is clicked, make a move
                    {
                        Move(pointedCube.chessPosition.x, pointedCube.chessPosition.y, pointedCube.chessPosition.z,
                            selectedPiece.name.Split(" ")[0] == "Pawn" &&
                            (selectedPiece.isPawnDark2White && pointedCube.chessPosition.y == 0 || !selectedPiece.isPawnDark2White && pointedCube.chessPosition.y == 3));
                    }
                    else if (selectedPiece != null)
                        selectedPiece = null;
                    CubeManager.Instance.ClearMoveableCubes();
                }
            }
        }

    }

    // ------------------------------------ Executions -------------------------------------------------------------------

    private void GetPotentialMoves(MoveableObject piece, HashSet<int3> storeMoveables = null, HashSet<int3> storeEatables = null, bool showCubes = true)
    {
        HashSet<int3> moveables = new HashSet<int3>();
        HashSet<int3> eatables = new HashSet<int3>();
        string type = piece.name.Split(' ')[0];

        if (type == "Rook" || type == "Queen")
            FindVerticalMoves(chessBoard, piece, moveables, eatables);
        if (type == "Bishop" || type == "Queen")
            FindDiagonalMoves(chessBoard, piece, moveables, eatables);
        else
            FindNonBlockableMoves(chessBoard, piece, moveables, eatables);

        foreach (var move in moveables.ToList())
        {
            MoveableObject[,,] board = (MoveableObject[,,])chessBoard.Clone();
            int3 storePosition = piece.chessPosition; // store the original position
            board[piece.chessPosition.x, piece.chessPosition.y, piece.chessPosition.z] = null;
            board[move.x, move.y, move.z] = piece; // set the piece to the new position
            piece.chessPosition = move;

            if (!IsKingSafe(board))
                moveables.Remove(move); // if the king is threatened, remove this move
            piece.chessPosition = storePosition; // restore the original position
        }

        foreach (var move in eatables.ToList())
        {
            MoveableObject[,,] board = (MoveableObject[,,])chessBoard.Clone();
            int3 storePosition = piece.chessPosition; // store the original position
            board[piece.chessPosition.x, piece.chessPosition.y, piece.chessPosition.z] = null;
            board[move.x, move.y, move.z] = piece; // set the piece to the new position
            piece.chessPosition = move;

            if (!IsKingSafe(board))
                eatables.Remove(move); // if the king is threatened, remove this eat movement
            piece.chessPosition = storePosition; // restore the original position
        }

        storeMoveables?.UnionWith(moveables);
        storeEatables?.UnionWith(eatables);
        if (showCubes)
            CubeManager.Instance.SetCubes(moveables, eatables);

    }

    private void Move(int x, int y, int z, bool isPromotion = false)
    {
        CubeManager.Instance.ClearWarningCube();
        int3 fromPosition = selectedPiece.chessPosition;

        // move the eaten piece to side
        inTransition = true;
        MoveableObject targetPiece = chessBoard[x, y, z];
        if (targetPiece != null)
        {
            Vector3 sidePosition;
            sidePosition.y = -4.5f;
            if (isWhiteTurn)
            {
                sidePosition.x = (darkEats < 8) ? (4 * separation) : (4.5f * separation);
                sidePosition.z = (-3.5f + whiteEats % 8) * separation / 2;
                whiteEats++;
            }
            else
            {
                sidePosition.x = (darkEats < 8) ? (-4 * separation) : (-4.5f * separation);
                sidePosition.z = (3.5f - darkEats % 8) * separation / 2;
                darkEats++;
            }
            targetPiece.PieceEaten(sidePosition);
        }

        MoveableObject promotedPiece = null;
        chessBoard[selectedPiece.chessPosition.x, selectedPiece.chessPosition.y, selectedPiece.chessPosition.z] = null; // update the chess board
        selectedPiece.SetHighLight(false);
        if (isPromotion) // if a pawn reaches edge, promote it
        {
            selectedPiece.MoveTo(new int3(x, y, z), () =>
                StartCoroutine(UIManager.Instance.SetPromotionTypeCoroutine((newType) =>
                {
                    selectedPiece.gameObject.SetActive(false); // hide the pawn
                    promotedPiece = Instantiate(Resources.Load<GameObject>($"Prefabs/{newType + (isWhiteTurn ? " White" : " Dark")}")).GetComponent<MoveableObject>();
                    promotedPiece.SetChessPosition(new int3(x, y, z));
                    chessBoard[x, y, z] = promotedPiece;
                    SwitchTurn();
                })));
        }
        else
        {
            chessBoard[x, y, z] = selectedPiece;
            selectedPiece.MoveTo(new int3(x, y, z), SwitchTurn);
        }


        // switch player's turn
        void SwitchTurn()
        {
            // save record first
            MoveRecord record = new MoveRecord(
                fromPosition,
                new int3(x, y, z),
                selectedPiece,
                targetPiece,
                promotedPiece
            );
            if (currentStep < records.Count)
                records[currentStep] = record;
            else
                records.Add(record);
            currentStep++;
            selectedPiece = null;

            // if no possible moves, game over
            isWhiteTurn = !isWhiteTurn;
            HashSet<int3> allMoves = new HashSet<int3>();
            HashSet<int3> allEats = new HashSet<int3>();
            foreach (var piece in chessBoard)
            {
                if (piece != null && piece.CompareTag(isWhiteTurn ? "White" : "Dark"))
                    GetPotentialMoves(piece, allMoves, allEats, false);
            }
            if (allMoves.Count == 0 && allEats.Count == 0)
            {
                ChangeState(GameState.End);
                return;
            }

            // transition
            if (isRevolvedWhite == isRevolvedDark)
                RevolveBoard();
            else
            {
                inTransition = false;
                // if king is threatened
                if (isWhiteTurn && !IsKingSafeAtPosition(KingW.chessPosition) || !isWhiteTurn && !IsKingSafeAtPosition(KingD.chessPosition))
                    CubeManager.Instance.SetWarningCube(isWhiteTurn ? KingW.chessPosition : KingD.chessPosition);
            }
            ShiftCamera(storedRadius, storedTheta, storedPhi, cameraMoveTime);
        }

    }

    public void UndoStep()
    {
        if (currentStep == 0)
            return;
        isWhiteTurn = !isWhiteTurn;
        MoveRecord previous = records[--currentStep];
        Debug.Log($"Undoing step {currentStep}: {previous.movedPiece.name} from {previous.fromPosition} to {previous.toPosition}");

        if (previous.promotedPiece != null) // if a piece was promoted, restore the pawn
        {
            Destroy(previous.promotedPiece.gameObject); // destroy the promoted piece
            previous.movedPiece.gameObject.SetActive(true); // activate the pawn
        }
        if (previous.eatenPiece != null) // if the eaten piece exists, restore its position
            previous.eatenPiece.SetChessPosition(previous.toPosition);

        previous.movedPiece.SetChessPosition(previous.fromPosition); // move the piece back to the original position
        chessBoard[previous.fromPosition.x, previous.fromPosition.y, previous.fromPosition.z] = previous.movedPiece;
        chessBoard[previous.toPosition.x, previous.toPosition.y, previous.toPosition.z] = previous.eatenPiece;

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

        // reset the camera
        float r = radius = storedRadius;
        float t = theta = storedTheta;
        float p = phi = storedPhi;
        MoveCamera();
        storedRadius = r;
        storedTheta = t;
        storedPhi = p;

    }

    public void RevolveBoard(bool fromUser = false)
    {
        if (fromUser)
        {
            if (isWhiteTurn)
                isRevolvedWhite = !isRevolvedWhite;
            else
                isRevolvedDark = !isRevolvedDark;
        }
        inTransition = true;
        CubeManager.Instance.ClearMoveableCubes();
        CubeManager.Instance.ClearWarningCube();
        GridManager.Instance.Revolve();

        int numPiece = 0;
        int numFinish = 0;
        foreach (var piece in chessBoard)
        {
            if (piece != null)
            {
                numPiece++;
                piece.RevolveAlongAxisZ(() =>
                {
                    numFinish++;
                    if (numFinish == numPiece)
                    {
                        inTransition = false;
                        // if king is threatened
                        if (isWhiteTurn && !IsKingSafeAtPosition(KingW.chessPosition) || !isWhiteTurn && !IsKingSafeAtPosition(KingD.chessPosition))
                            CubeManager.Instance.SetWarningCube(isWhiteTurn ? KingW.chessPosition : KingD.chessPosition);
                        if (selectedPiece != null)
                            GetPotentialMoves(selectedPiece);
                    }
                });
            }
        }



    }

    // ------------------------------------ Camera Movements -------------------------------------------------------------------

    private void ShiftCamera(float targetRadius, float targetTheta, float targetPhi, float duration, Action onComplete = null)
    {
        StartCoroutine(MoveCameraCoroutine(targetRadius, targetTheta, targetPhi, duration, onComplete));
        storedRadius = radius;
        storedTheta = theta;
        storedPhi = phi;

        IEnumerator MoveCameraCoroutine(float targetRadius, float targetTheta, float targetPhi, float duration, Action onComplete)
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
                MoveCamera();
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            radius = targetRadius;
            theta = targetTheta;
            phi = targetPhi;
            MoveCamera();
            onComplete?.Invoke();
        }

    }

    private void MoveCamera()
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

    // ------------------------------------ Find Possible Moves ------------------------------------------------------------------------

    private bool IsKingSafeAtPosition(int3 pos)
    {
        MoveableObject[,,] board = (MoveableObject[,,])chessBoard.Clone();
        MoveableObject currentKing = isWhiteTurn ? KingW : KingD;
        int3 storedPosition = currentKing.chessPosition; // store the original position
        board[currentKing.chessPosition.x, currentKing.chessPosition.y, currentKing.chessPosition.z] = null;
        board[pos.x, pos.y, pos.z] = currentKing;
        currentKing.chessPosition = pos; // set the piece to the new position
        bool isSafe = IsKingSafe(board);
        currentKing.chessPosition = storedPosition;
        return isSafe;

    }

    private bool IsKingSafe(MoveableObject[,,] board) // if the position is threatened by any opponent piece
    {
        foreach (var piece in board)
        {
            if (piece == null || piece.CompareTag("White") && isWhiteTurn || piece.CompareTag("Dark") && !isWhiteTurn)
                continue;

            HashSet<int3> pieceMoveables = new HashSet<int3>();
            HashSet<int3> pieceEatables = new HashSet<int3>();
            string type = piece.name.Split(' ')[0];
            if (type == "Rook" || type == "Queen")
                FindVerticalMoves(board, piece, pieceMoveables, pieceEatables);
            if (type == "Bishop" || type == "Queen")
                FindDiagonalMoves(board, piece, pieceMoveables, pieceEatables);
            else
                FindNonBlockableMoves(board, piece, pieceMoveables, pieceEatables, true);

            int3 kingPosition = isWhiteTurn ? KingW.chessPosition : KingD.chessPosition;
            if (pieceEatables.Contains(kingPosition))
                return false; // if the position is threatened by any opponent piece
        }
        return true;

    }

    private void FindVerticalMoves(MoveableObject[,,] board, MoveableObject fromPiece, HashSet<int3> moveables, HashSet<int3> eatables)
    {
        int3 from = fromPiece.chessPosition;
        // x+ move
        for (int i = 1; i < 4; i++)
        {
            int x = from.x + i;
            if (x > 3) break;

            if (board[x, from.y, from.z] == null)
                moveables.Add(new int3(x, from.y, from.z));
            else
            {
                if (!board[x, from.y, from.z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(x, from.y, from.z));
                break;
            }
        }

        // x- move
        for (int i = 1; i < 4; i++)
        {
            int x = from.x - i;
            if (x < 0) break;

            if (board[x, from.y, from.z] == null)
                moveables.Add(new int3(x, from.y, from.z));
            else
            {
                if (!board[x, from.y, from.z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(x, from.y, from.z));
                break;
            }
        }

        // y+ move
        for (int i = 1; i < 4; i++)
        {
            int y = from.y + i;
            if (y > 3) break;

            if (board[from.x, y, from.z] == null)
                moveables.Add(new int3(from.x, y, from.z));
            else
            {
                if (!board[from.x, y, from.z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(from.x, y, from.z));
                break;
            }
        }

        // y- move
        for (int i = 1; i < 4; i++)
        {
            int y = from.y - i;
            if (y < 0) break;

            if (board[from.x, y, from.z] == null)
                moveables.Add(new int3(from.x, y, from.z));
            else
            {
                if (!board[from.x, y, from.z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(from.x, y, from.z));
                break;
            }
        }

        // z+ move
        for (int i = 1; i < 4; i++)
        {
            int z = from.z + i;
            if (z > 3) break;

            if (board[from.x, from.y, z] == null)
                moveables.Add(new int3(from.x, from.y, z));
            else
            {
                if (!board[from.x, from.y, z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(from.x, from.y, z));
                break;
            }
        }

        // z- move
        for (int i = 1; i < 4; i++)
        {
            int z = from.z - i;
            if (z < 0) break;

            if (board[from.x, from.y, z] == null)
                moveables.Add(new int3(from.x, from.y, z));
            else
            {
                if (!board[from.x, from.y, z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(from.x, from.y, z));
                break;
            }
        }
    }

    private void FindDiagonalMoves(MoveableObject[,,] board, MoveableObject fromPiece, HashSet<int3> moveables, HashSet<int3> eatables)
    {
        int3 from = fromPiece.chessPosition;
        // x+y+ move
        for (int i = 1; i < 4; i++)
        {
            int x = from.x + i;
            int y = from.y + i;
            if (x > 3 || y > 3) break;

            if (board[x, y, from.z] == null)
                moveables.Add(new int3(x, y, from.z));
            else
            {
                if (!board[x, y, from.z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(x, y, from.z));
                break;
            }
        }

        // x+y- move
        for (int i = 1; i < 4; i++)
        {
            int x = from.x + i;
            int y = from.y - i;
            if (x > 3 || y < 0) break;

            if (board[x, y, from.z] == null)
                moveables.Add(new int3(x, y, from.z));
            else
            {
                if (!board[x, y, from.z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(x, y, from.z));
                break;
            }
        }

        // x-y+ move
        for (int i = 1; i < 4; i++)
        {
            int x = from.x - i;
            int y = from.y + i;
            if (x < 0 || y > 3) break;

            if (board[x, y, from.z] == null)
                moveables.Add(new int3(x, y, from.z));
            else
            {
                if (!board[x, y, from.z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(x, y, from.z));
                break;
            }
        }

        // x-y- move
        for (int i = 1; i < 4; i++)
        {
            int x = from.x - i;
            int y = from.y - i;
            if (x < 0 || y < 0) break;

            if (board[x, y, from.z] == null)
                moveables.Add(new int3(x, y, from.z));
            else
            {
                if (!board[x, y, from.z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(x, y, from.z));
                break;
            }
        }

        // x+z+ move
        for (int i = 1; i < 4; i++)
        {
            int x = from.x + i;
            int z = from.z + i;
            if (x > 3 || z > 3) break;

            if (board[x, from.y, z] == null)
                moveables.Add(new int3(x, from.y, z));
            else
            {
                if (!board[x, from.y, z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(x, from.y, z));
                break;
            }
        }

        // x+z- move
        for (int i = 1; i < 4; i++)
        {
            int x = from.x + i;
            int z = from.z - i;
            if (x > 3 || z < 0) break;

            if (board[x, from.y, z] == null)
                moveables.Add(new int3(x, from.y, z));
            else
            {
                if (!board[x, from.y, z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(x, from.y, z));
                break;
            }
        }

        // x-z+ move
        for (int i = 1; i < 4; i++)
        {
            int x = from.x - i;
            int z = from.z + i;
            if (x < 0 || z > 3) break;

            if (board[x, from.y, z] == null)
                moveables.Add(new int3(x, from.y, z));
            else
            {
                if (!board[x, from.y, z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(x, from.y, z));
                break;
            }
        }

        // x-z- move
        for (int i = 1; i < 4; i++)
        {
            int x = from.x - i;
            int z = from.z - i;
            if (x < 0 || z < 0) break;

            if (board[x, from.y, z] == null)
                moveables.Add(new int3(x, from.y, z));
            else
            {
                if (!board[x, from.y, z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(x, from.y, z));
                break;
            }
        }

        // y+z+ move
        for (int i = 1; i < 4; i++)
        {
            int y = from.y + i;
            int z = from.z + i;
            if (y > 3 || z > 3) break;

            if (board[from.x, y, z] == null)
                moveables.Add(new int3(from.x, y, z));
            else
            {
                if (!board[from.x, y, z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(from.x, y, z));
                break;
            }
        }

        // y+z- move
        for (int i = 1; i < 4; i++)
        {
            int y = from.y + i;
            int z = from.z - i;
            if (y > 3 || z < 0) break;

            if (board[from.x, y, z] == null)
                moveables.Add(new int3(from.x, y, z));
            else
            {
                if (!board[from.x, y, z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(from.x, y, z));
                break;
            }
        }

        // y-z+ move
        for (int i = 1; i < 4; i++)
        {
            int y = from.y - i;
            int z = from.z + i;
            if (y < 0 || z > 3) break;

            if (board[from.x, y, z] == null)
                moveables.Add(new int3(from.x, y, z));
            else
            {
                if (!board[from.x, y, z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(from.x, y, z));
                break;
            }
        }

        // y-z- move
        for (int i = 1; i < 4; i++)
        {
            int y = from.y - i;
            int z = from.z - i;
            if (y < 0 || z < 0) break;

            if (board[from.x, y, z] == null)
                moveables.Add(new int3(from.x, y, z));
            else
            {
                if (!board[from.x, y, z].CompareTag(fromPiece.tag))
                    eatables.Add(new int3(from.x, y, z));
                break;
            }
        }
    }

    private void FindNonBlockableMoves(MoveableObject[,,] board, MoveableObject fromPiece,
        HashSet<int3> moveables, HashSet<int3> eatables, bool ignoreThreat = false)
    {
        int3 from = fromPiece.chessPosition;
        string type = fromPiece.name.Split(' ')[0];
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = 0; z < 4; z++)
                {
                    switch (type)
                    {
                        case "King":
                            if (Abs(x - from.x) <= 1 && Abs(y - from.y) <= 1 && Abs(z - from.z) <= 1)
                            {
                                if (!ignoreThreat && !IsKingSafeAtPosition(new int3(x, y, z))) // king cannot move to a threatened position
                                    continue;
                                if (board[x, y, z] == null)
                                    moveables.Add(new int3(x, y, z));
                                else if (!board[x, y, z].CompareTag(fromPiece.tag))
                                    eatables.Add(new int3(x, y, z));
                            }
                            break;

                        case "Knight":
                            if ((Abs(x - from.x) == 2 && Abs(y - from.y) == 1 && z == from.z) ||
                                (Abs(x - from.x) == 1 && Abs(y - from.y) == 2 && z == from.z) ||
                                (Abs(z - from.z) == 2 && Abs(x - from.x) == 1 && y == from.y) ||
                                (Abs(z - from.z) == 1 && Abs(x - from.x) == 2 && y == from.y) ||
                                (Abs(y - from.y) == 2 && Abs(z - from.z) == 1 && x == from.x) ||
                                (Abs(y - from.y) == 1 && Abs(z - from.z) == 2 && x == from.x))
                            {
                                if (board[x, y, z] == null)
                                    moveables.Add(new int3(x, y, z));
                                else if (!board[x, y, z].CompareTag(fromPiece.tag))
                                    eatables.Add(new int3(x, y, z));
                            }
                            break;

                        case "Pawn":
                            int yMatch = fromPiece.isPawnDark2White ? y + 1 : y - 1; // white is at button
                            if (yMatch == from.y)
                            {
                                if (x == from.x && z == from.z)
                                {
                                    if (board[x, y, z] == null)
                                        moveables.Add(new int3(x, y, z));
                                }
                                else if ((Abs(x - from.x) == 1 && z == from.z || Abs(z - from.z) == 1 && x == from.x) &&
                                        board[x, y, z] != null &&
                                        !board[x, y, z].CompareTag(fromPiece.tag))
                                    eatables.Add(new int3(x, y, z));
                            }
                            break;
                    }
                }
            }
        }

    }



}