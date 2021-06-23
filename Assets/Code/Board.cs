using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class Board {
    public MoveGenerator moveGenerator = new MoveGenerator();
    public GameLogic gameLogic;

    public const int WhiteIndex = 0;
    public const int BlackIndex = 1;
    public const int pawnIndex = 0;
    public const int knightIndex = 1;
    public const int rookIndex = 2;
    public const int bishopIndex = 3;
    public const int queenIndex = 4;

    public int Enpassant, MoveCounter, turnColour, enemyColour, captures, checks;
    public int[] squares, kings;
    public bool[] Castling;
    public bool inCheck;

    public Stack<GameState> gameStates = new Stack<GameState>();
    public Stack<Move> gameMoves = new Stack<Move>();

    public List<int>[] pawns, knights, rooks, bishops, queens, allLists;

    public List<Move> GenerateMoves() {
        return moveGenerator.GenerateMoves(this);
    }

    public void Testing() {
        foreach (int i in pawns[0]) {
            GameDisplay.instance.tiles[i].ShowBlocked();
        }
    }

    public List<int> GetList(int type, int turn) {
        int i = turn == 0 ? 0 : 5;
        if (type == Piece.Pawn) return allLists[0 + i];
        if (type == Piece.Knight) return allLists[1 + i];
        if (type == Piece.Rook) return allLists[2 + i];
        if (type == Piece.Bishop) return allLists[3 + i];
        if (type == Piece.Queen) return allLists[4 + i];
        return null;
    }

    List<Move> ssss;
    #region tests
    //Perft testing methods, one for whole board, one divided into moves for debugging
    public void MoveTestSplit(int ina) {
        StringBuilder sb = new StringBuilder();
        uint total = 0;
        gameLogic.show = false;
        captures = 0;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        List<Move> allMoves = GenerateMoves();
        for (int i = 0; i < allMoves.Count; i++) {
            uint moves = MoveGenTestSplt(ina, allMoves[i]);
            total += moves;
            sb.Append(gameLogic.GetBoardRep(allMoves[i].StartSquare) + gameLogic.GetBoardRep(allMoves[i].EndSquare) + ": " + moves + "\n");
        }
        sb.Append(total + " total moves" + " in " + watch.ElapsedMilliseconds + " ms");
        Debug.Log(sb.ToString());
        watch.Stop();
    }
    uint MoveGenTestSplt(int depth, Move move) {
        if (depth == 0) return 1;
        uint numPos = 0;
        MovePiece(move);
        numPos += MoveGenTest(depth - 1);
        CtrlZ(move);
        return numPos;
    }
    
    public void MoveTest(int ina) {
        ssss = GenerateMoves();
        gameLogic.show = false;
        captures = 0;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        uint moves = MoveGenTest(ina);
        watch.Stop();
        MonoBehaviour.print(moves + " moves, " + captures + " captures, " + checks + " checks in " + watch.ElapsedMilliseconds + " ms");
        if (GenerateMoves().Count != ssss.Count) Debug.LogError("CountOff");
    }
    uint MoveGenTest(int depth) {
        if (depth == 0) return 1;
        List<Move> allMoves = GenerateMoves();
        uint numPos = 0;
        captures = 0;
        foreach (Move m in allMoves) {
            if (squares[m.EndSquare] != 0) captures++;
            MovePiece(m);
            numPos += MoveGenTest(depth - 1);
            CtrlZ(m);
        }
        return numPos;
    }
    #endregion

    public void MovePiece(Move move) {

        int movingFrom = move.StartSquare;
        int movingTo = move.EndSquare;
        int capturedPiece = squares[movingTo];
        bool[] copy = (bool[])Castling.Clone();
        int oldE = Enpassant;
        GameState oldGameState = new GameState(squares, copy, oldE, capturedPiece, allLists);
        gameStates.Push(oldGameState);
        int capturedPieceType = Piece.Type(squares[movingTo]);
        int moveingPieceType = Piece.Type(squares[movingFrom]);
        int movingPiece = squares[movingFrom];
        int moveFlag = move.MoveFlag;

        if (inCheck) checks++;


        if (capturedPieceType != 0) {

            List<int> pieceList = GetList(capturedPieceType, enemyColour);
            if (pieceList == null || !pieceList.Contains(movingTo)) {
                Debug.LogError("Captured Piece type not recognised:" + capturedPieceType);
            }
            pieceList.Remove(movingTo);
        }

        if (moveingPieceType == Piece.King) {
            kings[turnColour] = movingTo;
            if (turnColour == 0) {
                Castling[0] = false;
                Castling[1] = false;
            } else {
                Castling[2] = false;
                Castling[3] = false;
            }
        } else {
            List<int> pieceList = GetList(moveingPieceType, turnColour);
            pieceList[pieceList.IndexOf(movingFrom)] = movingTo;
        }
        //castline
        if (moveFlag == Move.Flag.Castling) {
            bool kingSide = movingTo == 6 || movingTo == 62;
            int rookFrom = kingSide ? movingTo + 1 : movingTo - 2;
            int rookTo = kingSide ? movingTo - 1 : movingTo + 1;
            squares[rookTo] = squares[rookFrom];
            squares[rookFrom] = 0;
            List<int> rookList = rooks[turnColour];
            rookList[rookList.IndexOf(rookFrom)] = rookTo;
        }
        //promotion
        if (move.IsPromotion) {
            int col = turnColour == 0 ? Piece.White : Piece.Black;
            int newPieceType = 0;
            switch (moveFlag) {
                case 5: newPieceType = Piece.Bishop; bishops[turnColour].Add(movingTo); break;
                case 6: newPieceType = Piece.Knight; knights[turnColour].Add(movingTo); break;
                case 7: newPieceType = Piece.Queen; queens[turnColour].Add(movingTo); break;
                case 4: newPieceType = Piece.Rook; rooks[turnColour].Add(movingTo); break;
            }
            pawns[turnColour].Remove(movingTo); ;
            movingPiece = newPieceType | col;
        }

        if (moveFlag == Move.Flag.EnPassantCapture) {
            int EnPawnSquare = (Enpassant >= 16 && Enpassant <= 23) ? Enpassant + 8 : Enpassant - 8;
            squares[EnPawnSquare] = 0;
            List<int> pawnList = pawns[enemyColour];
            pawnList.Remove(EnPawnSquare);
        }

        squares[movingTo] = movingPiece;
        squares[movingFrom] = 0;

        if (moveFlag == Move.Flag.PawnDoubleMove) {
            int i = movingFrom - movingTo == 16 ? -8 : 8;
            Enpassant = (movingFrom + i);
        } else {
            Enpassant = 99;
        }
        if (movingFrom == 7 || movingTo == 7) Castling[0] = false;
        if (movingFrom == 0 || movingTo == 0) Castling[1] = false;
        if (movingFrom == 63 || movingTo == 63) Castling[2] = false;
        if (movingFrom == 56 || movingTo == 56) Castling[3] = false;

        gameMoves.Push(move);

        ChangeTurn();
        gameLogic.EndTurn(move);
    }

    void ChangeTurn() {
        turnColour = turnColour == 0 ? 1 : 0;
        enemyColour = enemyColour == 0 ? 1 : 0;
    }

    public void CtrlZ(Move move) {
        //roll back turn
        ChangeTurn();
        //restore from gamestate
        GameState restoreState = gameStates.Pop();
        Enpassant = restoreState.Enpassant;
        Castling = restoreState.Castling;
        int capturedPiece = restoreState.CapturedPiece;
        int movingTo = move.EndSquare;
        int moveingFrom = move.StartSquare;
        int moveFlag = move.MoveFlag;
        int moveingPieceType = Piece.Type(squares[movingTo]);

        //move peice back
        if (moveingPieceType == Piece.King) {
            kings[turnColour] = moveingFrom;
        } else {
            List<int> pieceList = GetList(moveingPieceType, turnColour);
            pieceList[pieceList.IndexOf(movingTo)] = moveingFrom;
        }
        squares[moveingFrom] = squares[movingTo];
        //replace taken piece
        if (capturedPiece == 0) {
            squares[movingTo] = 0;
        } else {
            List<int> pieceList = GetList(Piece.Type(capturedPiece), enemyColour);
            pieceList.Add(movingTo);
            squares[movingTo] = capturedPiece;
        }
        //if promotion change piece back to pawn
        if (move.IsPromotion) {
            int col = turnColour == 0 ? Piece.White : Piece.Black;
            switch (moveFlag) {
                case 5: bishops[turnColour].Remove(moveingFrom); break;
                case 6: knights[turnColour].Remove(moveingFrom); break;
                case 7: queens[turnColour].Remove(moveingFrom); break;
                case 4: rooks[turnColour].Remove(moveingFrom); break;
            }
            pawns[turnColour].Add(moveingFrom);
            squares[moveingFrom] = Piece.Pawn | col;
        }
        //add back pawn if empassment capture
        if (moveFlag == Move.Flag.EnPassantCapture) {
            int EnPawnSquare = (movingTo >= 16 && movingTo <= 23) ? movingTo + 8 : movingTo - 8;
            int col = enemyColour == 0 ? Piece.White : Piece.Black;
            squares[EnPawnSquare] = col | Piece.Pawn;
            pawns[enemyColour].Add(EnPawnSquare);
        }
        //move back rook if castle
        if (moveFlag == Move.Flag.Castling) {
            bool kingSide = movingTo == 6 || movingTo == 62;
            int rookFrom = kingSide ? movingTo + 1 : movingTo - 2;
            int rookTo = kingSide ? movingTo - 1 : movingTo + 1;
            squares[rookFrom] = squares[rookTo];
            squares[rookTo] = 0;
            List<int> pieceList = rooks[turnColour];
            pieceList[pieceList.IndexOf(rookTo)] = rookFrom;
        }
        gameMoves.Pop();
    }
    //skip turn method for dubugging
    public void TurnSkip() {
        turnColour = turnColour == 0 ? 1 : 0;
        enemyColour = enemyColour == 0 ? 1 : 0;
        gameLogic.EndTurn();
    }







}
