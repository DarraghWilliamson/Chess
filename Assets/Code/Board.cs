using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Board {
    public const int WhiteIndex = 0;
    public const int BlackIndex = 1;

    public const int pawnIndex = 0;
    public const int knightIndex = 1;
    public const int rookIndex = 2;
    public const int bishopIndex = 3;
    public const int queenIndex = 4;

    public int[] squares;
    public bool[] Castling;
    public bool inCheck, isWhitesMove;
    public int turnColour, enemyColour;

    public Move lastMove;
    public Stack<GameState> gameStates = new Stack<GameState>();
    int captures, checks;
    public int Enpassant, MoveCounter;
    public GameLogic gameLogic;
    public MoveGenerator moveGenerator = new MoveGenerator();

    public int[] kings;
    public List<int>[] pawns, knights, rooks, bishops, queens, allLists;

    public List<Move> GeneratMoves() {
        return moveGenerator.GenerateMoves(this);
    }
    public List<Move> GeneratEnemyMoves() {
        return null;
        //return moveGenerator.GenerateEnemyMoves(this);
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

    public void MovePiece(Move move) {

        GameState oldState = new GameState() ;
        int Enpass = Enpassant;
        oldState.Enpassant = Enpass;
        oldState.Castling = Castling;
        oldState.Squares = squares;
        int movingFrom = move.StartSquare;
        int movingTo = move.EndSquare;

        int capturedPieceType = Piece.Type(squares[movingTo]);
        int moveingPieceType = Piece.Type(squares[movingFrom]);
        int movingPiece = squares[movingFrom];

        int moveFlag = move.MoveFlag;

        if (inCheck)  checks++;

        oldState.CapturedPiece = squares[movingTo];
        if (capturedPieceType != 0) {
            captures++;
            List<int> pieceList = GetList(capturedPieceType, enemyColour);
            if (pieceList == null || !pieceList.Contains(movingTo) )  {
                MonoBehaviour.print("S");
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
            if (pieceList == null) MonoBehaviour.print(moveingPieceType);
            pieceList[pieceList.IndexOf(movingFrom)] = movingTo;
        }
        if (moveFlag == Move.Flag.Castling) {
            bool kingSide = movingTo == 6 || movingTo == 62;
            int rookFrom = kingSide ? movingTo + 1 : movingTo - 2;
            int rookTo = kingSide ? movingTo - 1 : movingTo + 1;
            squares[rookTo] = squares[rookFrom];
            squares[rookFrom] = 0;
            List<int> pieceList = rooks[turnColour];
            pieceList[pieceList.IndexOf(rookFrom)] = rookTo;
        }
        if (moveFlag == Move.Flag.Promotion) {
            //add code
        }
        if (moveFlag == Move.Flag.EnPassantCapture) {
            int EnPawnSquare = (Enpassant >= 16 && Enpassant <= 23) ? Enpassant + 8 : Enpassant - 8;
            squares[EnPawnSquare] = 0;
            List<int> pieceList = pawns[enemyColour];
            pieceList.Remove(EnPawnSquare);
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

        gameStates.Push(oldState);
        lastMove = move;

        isWhitesMove = !isWhitesMove;
        turnColour = turnColour == 0 ? 1 : 0;
        enemyColour = enemyColour == 0 ? 1 : 0;
        gameLogic.EndTurn(move);
    }

    public void CtrlZ(Move move) {
        //roll back turn
        isWhitesMove = !isWhitesMove;
        turnColour = turnColour == 0 ? 1 : 0;
        enemyColour = enemyColour == 0 ? 1 : 0;
        //restore from gamestate
        GameState restoreState = gameStates.Pop();
        Enpassant = restoreState.Enpassant;
        Castling = restoreState.Castling;
        int capturedPiece = restoreState.CapturedPiece;
        int movingTo = move.EndSquare;
        int moveingFrom = move.StartSquare;
        int moveFlag = move.MoveFlag;
        int moveingPieceType = Piece.Type(squares[movingTo]);


        //move piece back
        if (moveingPieceType == Piece.King) {
            kings[turnColour] = moveingFrom;
            if (turnColour == 0) {
                Castling[0] = false;
                Castling[1] = false;
            } else {
                Castling[2] = false;
                Castling[3] = false;
            }
        } else {
            List<int> pieceList = GetList(moveingPieceType, turnColour);
            pieceList[pieceList.IndexOf(movingTo)] = moveingFrom;
        }
        squares[moveingFrom] = squares[movingTo];
        squares[movingTo] = 0;
        //replace taken piece
        if (capturedPiece != 0) {
            int capturedPieceType = Piece.Type(capturedPiece);
            List<int> pieceList = GetList(capturedPieceType, enemyColour);
            pieceList.Add(movingTo);
            squares[movingTo] = capturedPiece;
        }
        //add back pawn if empassment
        if(moveFlag == Move.Flag.EnPassantCapture) {
            int EnPawnSquare = (movingTo >= 16 && movingTo <= 23) ? movingTo + 8 : movingTo - 8;
            int col = enemyColour == 0 ? Piece.White : Piece.Black;
            int pawn = col | Piece.Pawn;
            squares[EnPawnSquare] = pawn ;
            List<int> pieceList = pawns[enemyColour];
            pieceList.Add(EnPawnSquare);
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


        if (gameLogic.show) {
            gameLogic.gameDisplay.UpdateDisplay(new Move(move.EndSquare, move.StartSquare));
            gameLogic.EndTurn();
        }
        
    }



    public void TurnSkip() {
        isWhitesMove = !isWhitesMove;
        turnColour = turnColour == 0 ? 1 : 0;
        enemyColour = enemyColour == 0 ? 1 : 0;
        gameLogic.EndTurn();
    }

    //creates a clone board where a move was made
    int[] GenBoard(int from, int to, int[] b) {
        int[] boardClone = (int[])b.Clone();
        boardClone[to] = boardClone[from];
        boardClone[from] = '\0';
        return boardClone;
    }


    
    public void MoveTest(int ina) {
        gameLogic.show = false;
        captures = 0;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        int moves = MoveGenTest(ina);
        watch.Stop();
        MonoBehaviour.print(moves + " moves, " + captures + " captures, "+checks+" checks in " + watch.ElapsedMilliseconds + " ms");
    }
    int MoveGenTest(int depth) {
        if (depth == 0) return 1;
        List<Move> allMoves = GeneratMoves();
        int numPos = 0;
        foreach (Move m in allMoves) {
            MovePiece(m);
            numPos += MoveGenTest(depth - 1);
            CtrlZ(m);
        }
        return numPos;
    }


    public bool CheckCheck(int colour, int[] b) {
        int kingPos = kings[turnColour];
        List<Move> temp = GeneratEnemyMoves();
        if (temp == null) return false;
        foreach (Move x in temp) {
            if (x.EndSquare == kingPos) return true;
        }
        return false;
    }



}
