using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.InputSystem;

public class GameManager_Robot : MonoBehaviour
{
    public static GameManager_Robot Instance;
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    // Game states
    public enum GameState { Entry, Paused, PlayerTurn, RobotTurn, End }
    public GameState CurrentState { get; private set; }
    [HideInInspector]
    public int outcome; // 0: Dark, 1: White, 2: Draw

    // Interaction variables
    public float mouseSensitivity;
    public float scrollSensitivity, clickTolerance;
    public float keyZoomSpeed, keyThetaSpeed, keyPhiSpeed;
    private bool isDraggingCamera;
    private Vector2 lastMousePosition, initialMousePosition;

    // Game variables
    public float RobotResponseTime; // Time for the robot to think
    private MoveableObject pointedPiece;
    private Cube pointedCube;
    private string[] promoteTypes = { "Queen", "Rook", "Bishop", "Knight" };

    void OnEnable()
    {
        if (PlayerPrefs.GetInt("PlayerWhite") == 1)
            BoardManager.Instance.isRevolvedDark = true;
        else
            BoardManager.Instance.isRevolvedWhite = true;
    }

    void Start()
    {
        ChangeState(GameState.Entry);
    }

    void Update()
    {
        if (BoardManager.Instance.isGameOver && CurrentState != GameState.End)
            ChangeState(GameState.End);

        if (CurrentState == GameState.PlayerTurn)
        {
            KeyboardControl();
            PointerControl();
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        switch (CurrentState)
        {
            case GameState.Entry:
                Debug.Log("Game is starting");
                BoardManager.Instance.inTransition = true;
                UIManager_Robot.Instance.FadeShade();
                CameraManager.Instance.EntryCamera(BoardManager.Instance.entryTime, () =>
                {
                    BoardManager.Instance.inTransition = false;
                    if (PlayerPrefs.GetInt("PlayerWhite") == 1)
                        ChangeState(GameState.PlayerTurn);
                    else
                        ChangeState(GameState.RobotTurn);
                });
                break;

            case GameState.Paused:
                Debug.Log("Game is paused");
                Time.timeScale = 0; // pause the game
                UIManager_Robot.Instance.SetShade(0.5f); // show the shade
                break;

            case GameState.PlayerTurn:
                Debug.Log("Player's turn");
                Time.timeScale = 1; // resume the game
                UIManager_Robot.Instance.SetShade(0f); // show the shade
                break;

            case GameState.RobotTurn:
                Debug.Log("Robot's turn");
                BoardManager.Instance.inTransition = true;
                StartCoroutine(WaitAndDo(RobotResponseTime, () =>
                {
                    switch (PlayerPrefs.GetInt("RobotLevel"))
                    {
                        case 0:
                            RobotRandom();
                            break;
                        case 1:
                            RobotPrioritizeEat();
                            break;
                        case 2:
                            RobotHard();
                            break;
                    }
                }));
                break;

            case GameState.End:
                Debug.Log("Game over!");
                if (outcome != 2)
                    outcome = BoardManager.Instance.isWhiteTurn ? 0 : 1;
                UIManager_Robot.Instance.GameFinish(outcome);
                break;
        }

        IEnumerator WaitAndDo(float waitTime, System.Action action)
        {
            yield return new WaitForSeconds(waitTime);
            action?.Invoke();
        }
    }

    private void KeyboardControl()
    {
        // onclick pause button
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            UIManager_Robot.Instance.PauseOnClick();

        if (Keyboard.current == null) return;

        // zoom camera: R, F
        if (Keyboard.current.rKey.isPressed)
            CameraManager.Instance.radius -= keyZoomSpeed * Time.deltaTime;

        if (Keyboard.current.fKey.isPressed)
            CameraManager.Instance.radius += keyZoomSpeed * Time.deltaTime;

        // rotate camera: A, D, W, S or Arrow keys
        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            CameraManager.Instance.theta -= keyThetaSpeed * Time.deltaTime;

        if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            CameraManager.Instance.theta += keyThetaSpeed * Time.deltaTime;

        if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
            CameraManager.Instance.phi -= keyPhiSpeed * Time.deltaTime;

        if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
            CameraManager.Instance.phi += keyPhiSpeed * Time.deltaTime;

        CameraManager.Instance.UpdateCamera();
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
        if (BoardManager.Instance.selectedPiece != null)
            BoardManager.Instance.selectedPiece.SetHighLight(!BoardManager.Instance.inTransition);

        if (!BoardManager.Instance.inTransition)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()); // use a ray at the pointer's position
            if (Physics.Raycast(ray, out RaycastHit hit)) // Pointing to piece or cube
            {
                if (hit.collider.CompareTag("Green") || hit.collider.CompareTag("Red")) // if pointing to a cube
                {
                    pointedCube = hit.collider.GetComponent<Cube>();
                    pointedCube.SetEnlargeCube(true);
                }
                else if (hit.collider.CompareTag("White") && BoardManager.Instance.isWhiteTurn ||
                    hit.collider.CompareTag("Dark") && !BoardManager.Instance.isWhiteTurn)
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
            CameraManager.Instance.theta -= delta.x * mouseSensitivity;
            CameraManager.Instance.phi += delta.y * mouseSensitivity;
            CameraManager.Instance.UpdateCamera();
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
            if (mouseDragDistance <= clickTolerance && !BoardManager.Instance.inTransition) // this is a click event
            {
                if (BoardManager.Instance.selectedPiece != null)
                    BoardManager.Instance.selectedPiece.SetHighLight(false);

                if (pointedPiece != null) // if a piece is clicked, and if the color is right, select the new piece
                {
                    if (pointedPiece.CompareTag("White") && BoardManager.Instance.isWhiteTurn ||
                        pointedPiece.CompareTag("Dark") && !BoardManager.Instance.isWhiteTurn)
                    {
                        BoardManager.Instance.selectedPiece = pointedPiece;
                        BoardManager.Instance.GetPiecePotentialMoves(BoardManager.Instance.selectedPiece);
                    }
                }
                else // click at a cube or nothing
                {
                    if (pointedCube != null) // if a cube is clicked, make a move
                    {
                        BoardManager.Instance.Move(pointedCube.chessPosition.x, pointedCube.chessPosition.y, pointedCube.chessPosition.z,
                            BoardManager.Instance.selectedPiece.name.Split(" ")[0] == "Pawn" &&
                            (BoardManager.Instance.selectedPiece.isPawnDark2White && pointedCube.chessPosition.y == 0 ||
                            !BoardManager.Instance.selectedPiece.isPawnDark2White && pointedCube.chessPosition.y == 3), null,
                            () => ChangeState(GameState.RobotTurn));
                    }
                    else if (BoardManager.Instance.selectedPiece != null)
                        BoardManager.Instance.selectedPiece = null;
                    CubeManager.Instance.ClearMoveableCubes();
                }
            }
        }
    }

    private void RobotRandom() // Randomly select a piece, then randomly move
    {
        List<MoveableObject> pieces = new List<MoveableObject>();
        List<HashSet<int3>> potentialMoves = new List<HashSet<int3>>();
        foreach (var piece in BoardManager.Instance.chessBoard)
        {
            if (piece != null && !piece.CompareTag(PlayerPrefs.GetInt("PlayerWhite") == 1 ? "White" : "Dark"))
            {
                HashSet<int3> Moves = new HashSet<int3>();
                BoardManager.Instance.GetPiecePotentialMoves(piece, Moves, Moves, false);
                if (Moves.Count > 0)
                {
                    pieces.Add(piece);
                    potentialMoves.Add(Moves);
                }
            }
        }

        int i = UnityEngine.Random.Range(0, pieces.Count);
        MoveableObject selectedPiece = pieces[i];
        BoardManager.Instance.selectedPiece = selectedPiece;
        int3 move = potentialMoves[i].ElementAt(UnityEngine.Random.Range(0, potentialMoves[i].Count));

        BoardManager.Instance.Move(move.x, move.y, move.z, selectedPiece.name.Split(" ")[0] == "Pawn" &&
            (selectedPiece.isPawnDark2White && move.y == 0 || !selectedPiece.isPawnDark2White && move.y == 3),
            promoteTypes[UnityEngine.Random.Range(0, 4)], () => ChangeState(GameState.PlayerTurn));
    }

    private void RobotPrioritizeEat()
    {
        List<MoveableObject> moveablePieces = new List<MoveableObject>();
        List<MoveableObject> eatablePieces = new List<MoveableObject>();
        List<HashSet<int3>> potentialMoves = new List<HashSet<int3>>();
        List<HashSet<int3>> PotentialEats = new List<HashSet<int3>>();
        foreach (var piece in BoardManager.Instance.chessBoard)
        {
            if (piece != null && !piece.CompareTag(PlayerPrefs.GetInt("PlayerWhite") == 1 ? "White" : "Dark"))
            {
                HashSet<int3> Moves = new HashSet<int3>();
                HashSet<int3> Eats = new HashSet<int3>();
                BoardManager.Instance.GetPiecePotentialMoves(piece, Moves, Eats, false);
                if (Moves.Count > 0)
                {
                    moveablePieces.Add(piece);
                    potentialMoves.Add(Moves);
                }
                if (Eats.Count > 0)
                {
                    eatablePieces.Add(piece);
                    PotentialEats.Add(Eats);
                }
            }
        }

        if (eatablePieces.Count > 0)
        {
            int i = UnityEngine.Random.Range(0, eatablePieces.Count);
            MoveableObject selectedPiece = eatablePieces[i];
            BoardManager.Instance.selectedPiece = selectedPiece;
            int3 move = PotentialEats[i].ElementAt(UnityEngine.Random.Range(0, PotentialEats[i].Count));

            BoardManager.Instance.Move(move.x, move.y, move.z, selectedPiece.name.Split(" ")[0] == "Pawn" &&
                (selectedPiece.isPawnDark2White && move.y == 0 || !selectedPiece.isPawnDark2White && move.y == 3),
                promoteTypes[UnityEngine.Random.Range(0, 4)], () => ChangeState(GameState.PlayerTurn));
            return;
        }
        else
        {
            int i = UnityEngine.Random.Range(0, moveablePieces.Count);
            MoveableObject selectedPiece = moveablePieces[i];
            BoardManager.Instance.selectedPiece = selectedPiece;
            int3 move = potentialMoves[i].ElementAt(UnityEngine.Random.Range(0, potentialMoves[i].Count));

            BoardManager.Instance.Move(move.x, move.y, move.z, selectedPiece.name.Split(" ")[0] == "Pawn" &&
                (selectedPiece.isPawnDark2White && move.y == 0 || !selectedPiece.isPawnDark2White && move.y == 3),
                promoteTypes[UnityEngine.Random.Range(0, 4)], () => ChangeState(GameState.PlayerTurn));
        }
    }

    private void RobotHard()
    {
        RobotRandom(); // Debug
    }


}