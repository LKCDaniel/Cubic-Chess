using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using static UnityEngine.Mathf;
using System.Linq;
using System;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    // ------------------------------------ Game Variables -------------------------------------------------------------------

    // Animation durations
    [Header("Animation Durations")]
    public float pieceMoveTime;
    public float entryTime, cameraMoveTime;

    // chess board, game variables
    [Header("Chess Board Settings")]
    public float separation; // separation between chess pieces
    public MoveableObject[,,] chessBoard; // 4*4*4, X from L to R, Y from B to T, Z from F to B
    [HideInInspector]
    public MoveableObject selectedPiece;
    [HideInInspector]
    public bool isLocal2P; // is it a local 2-player game
    public bool isGameOver { get; private set; } // is the game over
    public bool isWhiteTurn { get; private set; } // is it white's turn
    [HideInInspector]
    public bool isRevolvedWhite, isRevolvedDark; // Normally, the player starts from the bottom
    public bool isWhiteOnTop => isWhiteTurn && isRevolvedWhite || !isWhiteTurn && !isRevolvedDark;
    private int whiteEats, darkEats;
    [HideInInspector]
    public bool inTransition; // Is a piece currently moving

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

    }

    public void GetPiecePotentialMoves(MoveableObject piece, HashSet<int3> storeMoveables = null, HashSet<int3> storeEatables = null, bool showCubes = true)
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

    public void Move(int x, int y, int z, bool isPromotion, string promoteType = null, Action onComplete = null)
    {
        Debug.Log($"Step {currentStep}: Moving piece from {selectedPiece.chessPosition} to {new int3(x, y, z)}");
        if (isPromotion)
            Debug.Log($"Promoting to {promoteType}");
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
                sidePosition.x = (whiteEats < 8) ? (4 * separation) : (4.5f * separation);
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
            {
                if (promoteType == null) // if it's a local 2-player game, ask the user to select a promotion type
                {
                    StartCoroutine(UIManager_Local2P.Instance.SetPromotionTypeCoroutine((newType) =>
                    {
                        selectedPiece.gameObject.SetActive(false); // hide the pawn
                        promotedPiece = Instantiate(Resources.Load<GameObject>($"Prefabs/{newType + (isWhiteTurn ? " White" : " Dark")}")).GetComponent<MoveableObject>();
                        promotedPiece.SetChessPosition(new int3(x, y, z));
                        chessBoard[x, y, z] = promotedPiece;
                        SwitchTurn();
                    }));
                }
                else
                {
                    selectedPiece.gameObject.SetActive(false); // hide the pawn
                    promotedPiece = Instantiate(Resources.Load<GameObject>($"Prefabs/{promoteType + (isWhiteTurn ? " White" : " Dark")}")).GetComponent<MoveableObject>();
                    promotedPiece.SetChessPosition(new int3(x, y, z));
                    chessBoard[x, y, z] = promotedPiece;
                    SwitchTurn();
                }
            });
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
                    GetPiecePotentialMoves(piece, allMoves, allEats, false);
            }
            if (allMoves.Count == 0 && allEats.Count == 0)
            {
                isGameOver = true;
                return;
            }

            // transition
            if (isRevolvedWhite == isRevolvedDark && isLocal2P)
                RevolveBoard();
            else
            {
                inTransition = false;
                // if king is threatened
                if (isWhiteTurn && !IsKingSafeAtPosition(KingW.chessPosition) || !isWhiteTurn && !IsKingSafeAtPosition(KingD.chessPosition))
                    CubeManager.Instance.SetWarningCube(isWhiteTurn ? KingW.chessPosition : KingD.chessPosition);
            }
            onComplete?.Invoke();
            if (isLocal2P)
                CameraManager.Instance.SwitchCamera(cameraMoveTime);
        }

    }

    public void Undo(int step = 1)
    {
        for (int i = 0; i < step; i++)
        {
            isWhiteTurn = !isWhiteTurn;
            MoveRecord previous = records[--currentStep];
            Debug.Log($"Undoing step {currentStep}: {previous.movedPiece.name} from {previous.fromPosition} to {previous.toPosition}");

            if (previous.promotedPiece != null) // if a piece was promoted, restore the pawn
            {
                Destroy(previous.promotedPiece.gameObject); // destroy the promoted piece
                previous.movedPiece.gameObject.SetActive(true); // activate the pawn
            }
            if (previous.eatenPiece != null) // if the eaten piece exists, restore its position
            {
                previous.eatenPiece.SetChessPosition(previous.toPosition);
                if (isWhiteTurn)
                    whiteEats--;
                else
                    darkEats--;
            }

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
        }
        // if king is threatened
        CubeManager.Instance.ClearMoveableCubes();
        if (isWhiteTurn && !IsKingSafeAtPosition(KingW.chessPosition) || !isWhiteTurn && !IsKingSafeAtPosition(KingD.chessPosition))
            CubeManager.Instance.SetWarningCube(isWhiteTurn ? KingW.chessPosition : KingD.chessPosition);

        if (isLocal2P && step % 2 == 1)
            CameraManager.Instance.SwitchCamera(cameraMoveTime / 2); // switch camera to the previous position

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
                            GetPiecePotentialMoves(selectedPiece);
                    }
                });
            }
        }

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