using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager_Local2P : MonoBehaviour
{
    public static GameManager_Local2P Instance;
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1; // reset time scale when the game manager is destroyed
    }

    // Game states
    public enum GameState { Entry, Paused, Running, End }
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
    private MoveableObject pointedPiece;
    private Cube pointedCube;

    void Start()
    {
        BoardManager.Instance.isLocal2P = true; // set the game mode to local 2-player
        ChangeState(GameState.Entry);
    }

    void Update()
    {
        if (BoardManager.Instance.isGameOver && CurrentState != GameState.End)
            ChangeState(GameState.End);
        if (CurrentState == GameState.Running)
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
                UIManager_Local2P.Instance.FadeShade();
                CameraManager.Instance.EntryCamera(BoardManager.Instance.entryTime, () =>
                {
                    BoardManager.Instance.inTransition = false;
                    ChangeState(GameState.Running);
                });
                break;

            case GameState.Paused:
                Debug.Log("Game is paused");
                Time.timeScale = 0; // pause the game
                UIManager_Local2P.Instance.SetShade(0.5f); // show the shade
                break;

            case GameState.Running:
                Debug.Log("Game is running");
                Time.timeScale = 1; // resume the game
                UIManager_Local2P.Instance.SetShade(0f); // show the shade
                break;

            case GameState.End:
                Debug.Log("Game over!");
                if (outcome != 2)
                    outcome = BoardManager.Instance.isWhiteTurn ? 0 : 1; // set the outcome based on the current turn
                UIManager_Local2P.Instance.GameFinish(outcome);
                break;
        }
    }

    private void KeyboardControl()
    {
        // onclick pause button
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            UIManager_Local2P.Instance.PauseOnClick();

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
        MoveableObject selectedPiece = BoardManager.Instance.selectedPiece;
        if (selectedPiece != null)
            selectedPiece.SetHighLight(!BoardManager.Instance.inTransition);

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
                if (selectedPiece != null)
                    selectedPiece.SetHighLight(false);

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
                            selectedPiece.name.Split(" ")[0] == "Pawn" &&
                            (selectedPiece.isPawnDark2White && pointedCube.chessPosition.y == 0 ||
                            !selectedPiece.isPawnDark2White && pointedCube.chessPosition.y == 3));
                    }
                    else if (selectedPiece != null)
                        BoardManager.Instance.selectedPiece = null;
                    CubeManager.Instance.ClearMoveableCubes();
                }
            }
        }
    }



}