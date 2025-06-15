using UnityEngine;


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
    public GameState CurrentState { get; private set; }

    // chess pieces
    ChessPiece KingW, KingD,
    QueenW, QueenD,
    RookW1, RookW2, RookD1, RookD2,
    BishopW1, BishopW2, BishopD1, BishopD2,
    KnightW1, KnightW2, KnightD1, KnightD2,
    PawnW1, PawnW2, PawnW3, PawnW4, PawnW5, PawnW6, PawnW7, PawnW8,
    PawnD1, PawnD2, PawnD3, PawnD4, PawnD5, PawnD6, PawnD7, PawnD8;

    // chess board
    // 4*4*4, X from L to R, Y from B to T, Z from F to B, same as Unity's coordinate system
    ChessPiece[,,] chessBoard;
    public float separation = 2.5f; // separation between chess pieces

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
                    }
                }
            }
        }
    }

    void Start()
    {
        ChangeState(GameState.GameEntry);
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

        ChangeState(GameState.WhiteTurn);
    }


}
