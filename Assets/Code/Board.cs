using System;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class Board {
    public MoveGenerator moveGenerator = new MoveGenerator();
    public GameLogic gameLogic;
    public Stack<int> gameStates;

    // A gamestate uses 16 bits
    // 0-3      Castling | 0:Wk 1:Wq 2:Bk 3:Bq
    // 4-7      Enpass file starting at 0
    // 8-10     CapturedPieceType
    // 11-15    counter
    public Stack<int> gameMoves;

    // A move uses 16 bits
    // 0-5      startSq
    // 6-11     endSq
    // 12-13    moveType | 0:none, 1:Enpassant, 2:castle, 3:promotion
    // 14-15    promotionType | 0:Knight, 1:bishop, 2:rook, 3:queen
    public Stack<ulong> gameKeys;

    public int Enpassant, MoveCounter, turnColour, enemyColour, captures, currentGameState;
    public int[] kings, squares;
    public bool inCheck;
    public ulong[] pawnsBoard, knightsBoard, rooksBoard, bishopsBoard, queensBoard, kingsBoard, bitArray;
    public PieceList[] pawnsList, knightsList, rooksList, bishopsList, queensList, allLists;
    public ulong ZobristKey;

    public List<int> bitmatSquares = new List<int>(); //public for test method

    public Board(GameLogic game) {
        gameLogic = game;
    }

    public void LoadInfo(string fen) {
        bitArray = new ulong[64];
        squares = new int[64];
        kings = new int[2];
        ZobristKey = 0;
        gameKeys = new Stack<ulong>();
        gameStates = new Stack<int>();
        gameMoves = new Stack<int>();
        pawnsList = new PieceList[] { new PieceList(), new PieceList() };
        knightsList = new PieceList[] { new PieceList(), new PieceList() };
        rooksList = new PieceList[] { new PieceList(), new PieceList() };
        bishopsList = new PieceList[] { new PieceList(), new PieceList() };
        queensList = new PieceList[] { new PieceList(), new PieceList() };
        allLists = new PieceList[] { pawnsList[0], knightsList[0], rooksList[0], bishopsList[0], queensList[0], pawnsList[1], knightsList[1], rooksList[1], bishopsList[1], queensList[1] };
        pawnsBoard = new ulong[] { 0, 0 };
        knightsBoard = new ulong[] { 0, 0 };
        rooksBoard = new ulong[] { 0, 0 };
        bishopsBoard = new ulong[] { 0, 0 };
        queensBoard = new ulong[] { 0, 0 };
        kingsBoard = new ulong[] { 0, 0 };

        LoadInfo info = FEN.LoadNewFEN(fen);
        //build bitboards and piecelists
        for (int i = 0; i < 64; i++) {
            squares[i] = info.squares[i];
            if (info.squares[i] != 0) {
                int type = Piece.Type(squares[i]);
                int col = Piece.Colour(squares[i]);
                switch (type) {
                    case Piece.Pawn: pawnsList[col].Push(i); pawnsBoard[col] |= 1ul << i; break;
                    case Piece.Rook: rooksList[col].Push(i); rooksBoard[col] |= 1ul << i; break;
                    case Piece.Bishop: bishopsList[col].Push(i); bishopsBoard[col] |= 1ul << i; break;
                    case Piece.Knight: knightsList[col].Push(i); knightsBoard[col] |= 1ul << i; break;
                    case Piece.Queen: queensList[col].Push(i); queensBoard[col] |= 1ul << i; break;
                    case Piece.King: kings[col] = i; kingsBoard[col] |= 1ul << i; break;
                }
            }
            ulong t = 0;
            t |= 1ul << i;
            bitArray[i] = t;
        }
        currentGameState = info.state;
        turnColour = info.turnColour;
        enemyColour = turnColour == 0 ? 1 : 0;
        ZobristKey = Zobrist.GetZobristHash(this);
    }

    public List<int> GenerateMoves() {
        return moveGenerator.GenerateMoves(this);
    }

    public void MovePiece(int move) {
        gameMoves.Push(move);
        if (gameLogic.show) Console.Write(Log(move));
        int movingFrom = GetStartSquare(move);
        int movingTo = GetEndSquare(move);
        int moveType = GetMoveType(move);
        int capturedPieceType = Piece.Type(squares[movingTo]);
        int moveingPieceType = Piece.Type(squares[movingFrom]);
        bool IsPromotion = moveType == 3;
        bool IsDoubleMove = moveingPieceType == Piece.Pawn && (Math.Abs(movingFrom - movingTo) > 10);
        int oldEnpassantFile = ((currentGameState >> 4) & 15);
        int oldCastling = currentGameState &= 15;
        int oldState = ((capturedPieceType << 8) + (oldEnpassantFile << 4) + oldCastling);
        gameStates.Push(oldState);
        int Castling = oldCastling;
        currentGameState = 0;

        //if you captured a piece remove it from the list
        if (capturedPieceType != 0) {
            PieceList l = GetList(capturedPieceType, enemyColour);
            if (l == null) {
                Debug.Log(PrintMove(move));
            }
            l.Remove(movingTo);
            RemoveBit(capturedPieceType, enemyColour, movingTo);
            ZobristKey ^= Zobrist.zPieces[enemyColour, capturedPieceType, movingTo];
            currentGameState |= (capturedPieceType << 8);
        }

        //move pieces around in the lists
        if (moveingPieceType == Piece.King) {
            kings[turnColour] = movingTo;
        } else {
            GetList(moveingPieceType, turnColour).Move(movingFrom, movingTo);
        }
        //other piece movement
        RemoveBit(moveingPieceType, turnColour, movingFrom);
        AddBit(moveingPieceType, turnColour, movingTo);
        ZobristKey ^= Zobrist.zPieces[turnColour, moveingPieceType, movingFrom];
        ZobristKey ^= Zobrist.zPieces[turnColour, moveingPieceType, movingTo];
        squares[movingTo] = squares[movingFrom];
        squares[movingFrom] = 0;

        //promotion
        if (IsPromotion) {
            //remove old pawn
            pawnsList[turnColour].Remove(movingTo);
            RemoveBit(Piece.Pawn, turnColour, movingTo);
            ZobristKey ^= Zobrist.zPieces[turnColour, Piece.Pawn, movingTo];
            squares[movingTo] = 0;
            //get new promotion piece
            int newPieceType = 0;
            switch (GetPromotionType(move)) { // 0:Knight, 1:bishop, 2:rook, 3:queen
                case 0: newPieceType = Piece.Knight; break;
                case 1: newPieceType = Piece.Bishop; break;
                case 2: newPieceType = Piece.Rook; break;
                case 3: newPieceType = Piece.Queen; break;
            }

            //Add promotion piece back to square
            GetList(newPieceType, turnColour).Push(movingTo);
            AddBit(newPieceType, turnColour, movingTo);

            int col = turnColour == 0 ? Piece.White : Piece.Black;
            squares[movingTo] = col | newPieceType;
            ZobristKey ^= Zobrist.zPieces[turnColour, newPieceType, movingTo];
        }

        //castling
        if (moveType == 2) {
            bool kingSide = movingTo == 6 || movingTo == 62;
            int rookFrom = kingSide ? movingTo + 1 : movingTo - 2;
            int rookTo = kingSide ? movingTo - 1 : movingTo + 1;
            squares[rookTo] = squares[rookFrom];
            squares[rookFrom] = 0;
            rooksList[turnColour].Move(rookFrom, rookTo);
            RemoveBit(Piece.Rook, turnColour, rookFrom);
            AddBit(Piece.Rook, turnColour, rookTo);
            ZobristKey ^= Zobrist.zPieces[turnColour, Piece.Rook, rookFrom];
            ZobristKey ^= Zobrist.zPieces[turnColour, Piece.Rook, rookTo];
        }
        //Enpassant captures
        if (moveType == 1) {
            int EnPawnSquare = (turnColour == 0) ? oldEnpassantFile + 31 : oldEnpassantFile + 23;
            squares[EnPawnSquare] = 0;
            pawnsList[enemyColour].Remove(EnPawnSquare);
            RemoveBit(Piece.Pawn, enemyColour, EnPawnSquare);
            ZobristKey ^= Zobrist.zPieces[enemyColour, Piece.Pawn, EnPawnSquare];
        }
        //enpass existed, remove it
        if (oldEnpassantFile != 0) {
            ZobristKey ^= Zobrist.zEnpassant[oldEnpassantFile];
        }//if double move, Add Enpassant
        if (IsDoubleMove) {
            int enpassantFile = (movingFrom % 8) + 1;
            currentGameState |= (enpassantFile << 4);
            ZobristKey ^= Zobrist.zEnpassant[enpassantFile];
        }

        //set new castling states if castling isnt already inpossable
        if (oldCastling != 0) {
            if (moveingPieceType == Piece.King) Castling &= turnColour == 0 ? ~3 : ~12;
            if (movingFrom == 7 || movingTo == 7) Castling &= ~1;
            if (movingFrom == 0 || movingTo == 0) Castling &= ~2;
            if (movingFrom == 63 || movingTo == 63) Castling &= ~4;
            if (movingFrom == 56 || movingTo == 56) Castling &= ~8;
        }
        //if castling was changed, update key
        if (Castling != oldCastling) {
            ZobristKey ^= Zobrist.zCastling[oldCastling];
            ZobristKey ^= Zobrist.zCastling[Castling];
        }

        currentGameState |= Castling;

        ZobristKey ^= Zobrist.zTurnColour;
        turnColour = turnColour == 0 ? 1 : 0;
        enemyColour = enemyColour == 0 ? 1 : 0;
        /*if (ZobristKey != Zobrist.GetZobristHash(this)) {
            Debug.Log("Zobrist key error\n" + ZobristKey + "\n" + Zobrist.GetZobristHash(this));
            Debug.Log(PrintMove(move)+" P:"+moveingPieceType);
        }*/
        if (gameLogic.show) gameLogic.EndTurn();
    }

    public void CtrlZ(int move) {
        //roll back turn
        turnColour = turnColour == 0 ? 1 : 0;
        enemyColour = enemyColour == 0 ? 1 : 0;
        int enemyPieceCol = enemyColour == 0 ? Piece.White : Piece.Black;
        ZobristKey ^= Zobrist.zTurnColour;
        //get old states
        int oldState = gameStates.Pop();
        int capturedPieceType = (oldState >> 8);
        int capturedPiece = capturedPieceType == 0 ? 0 : capturedPieceType | enemyPieceCol;
        //get move data
        int movingTo = GetEndSquare(move);
        int movingFrom = GetStartSquare(move);
        int moveType = GetMoveType(move);
        int moveingPieceType = Piece.Type(squares[movingTo]);
        bool IsPromotion = moveType == 3;

        //enpass exists, remove it
        int Enpass = (currentGameState >> 4) & 15;
        if (Enpass != 0) {
            ZobristKey ^= Zobrist.zEnpassant[Enpass];
        }
        //if old enpass, restore
        int oldEnpass = (oldState >> 4) & 15;
        if (oldEnpass != 0) {
            ZobristKey ^= Zobrist.zEnpassant[oldEnpass];
        }
        int oldCastleing = oldState & 15;
        int Castling = currentGameState & 15;
        if (Castling != oldCastleing) {
            ZobristKey ^= Zobrist.zCastling[Castling];
            ZobristKey ^= Zobrist.zCastling[oldCastleing];
        }

        //move peice back
        if (moveingPieceType == Piece.King) {
            kings[turnColour] = movingFrom;
        } else {
            GetList(moveingPieceType, turnColour).Move(movingTo, movingFrom);
        }
        RemoveBit(moveingPieceType, turnColour, movingTo);
        AddBit(moveingPieceType, turnColour, movingFrom);
        ZobristKey ^= Zobrist.zPieces[turnColour, moveingPieceType, movingTo];
        ZobristKey ^= Zobrist.zPieces[turnColour, moveingPieceType, movingFrom];
        squares[movingFrom] = squares[movingTo];
        squares[movingTo] = 0;

        //replace taken piece
        if (capturedPiece != 0) {
            AddBit(Piece.Type(capturedPiece), enemyColour, movingTo);
            ZobristKey ^= Zobrist.zPieces[enemyColour, Piece.Type(capturedPiece), movingTo];
            PieceList pieceList = GetList(Piece.Type(capturedPiece), enemyColour);
            pieceList.Push(movingTo);
            squares[movingTo] = capturedPiece;
        }

        //move back rook if castle
        if (moveType == 2) {
            bool kingSide = movingTo == 6 || movingTo == 62;
            int rookFrom = kingSide ? movingTo + 1 : movingTo - 2;
            int rookTo = kingSide ? movingTo - 1 : movingTo + 1;
            squares[rookFrom] = squares[rookTo];
            squares[rookTo] = 0;
            PieceList rookList = rooksList[turnColour];
            rookList.Move(rookTo, rookFrom);
            RemoveBit(Piece.Rook, turnColour, rookTo);
            AddBit(Piece.Rook, turnColour, rookFrom);
            ZobristKey ^= Zobrist.zPieces[turnColour, Piece.Rook, rookTo];
            ZobristKey ^= Zobrist.zPieces[turnColour, Piece.Rook, rookFrom];
        }

        //if promotion change piece back to pawn
        if (IsPromotion) {
            RemoveBit(moveingPieceType, turnColour, movingFrom);
            GetList(moveingPieceType, turnColour).Remove(movingFrom);
            ZobristKey ^= Zobrist.zPieces[turnColour, moveingPieceType, movingFrom];

            AddBit(Piece.Pawn, turnColour, movingFrom);
            pawnsList[turnColour].Push(movingFrom);
            ZobristKey ^= Zobrist.zPieces[turnColour, Piece.Pawn, movingFrom];

            int col = turnColour == 0 ? Piece.White : Piece.Black;
            squares[movingFrom] = Piece.Pawn | col;
        }

        //add back pawn if empassment capture
        if (moveType == 1) {
            int EnPawnSquare = (movingTo >= 16 && movingTo <= 23) ? movingTo + 8 : movingTo - 8;
            int col = enemyColour == 0 ? Piece.White : Piece.Black;
            squares[EnPawnSquare] = col | Piece.Pawn;
            pawnsList[enemyColour].Push(EnPawnSquare);
            AddBit(Piece.Pawn, enemyColour, EnPawnSquare);
            ZobristKey ^= Zobrist.zPieces[enemyColour, Piece.Pawn, EnPawnSquare];
        }

        currentGameState = oldState;
        //if (ZobristKey != Zobrist.GetZobristHash(this)) Debug.Log("Zobrist key error\n" + ZobristKey + "\n" + Zobrist.GetZobristHash(this));

        if (gameMoves.Count != 0) gameMoves.Pop();
        if (gameLogic.show) gameLogic.EndTurn();
    }

    public PieceList GetList(int type, int turn) {
        int i = turn == 0 ? 0 : 5;
        if (type == Piece.Pawn) return allLists[0 + i];
        if (type == Piece.Knight) return allLists[1 + i];
        if (type == Piece.Rook) return allLists[2 + i];
        if (type == Piece.Bishop) return allLists[3 + i];
        if (type == Piece.Queen) return allLists[4 + i];
        return null;
    }

    public void AddBit(int type, int col, int i) {
        switch (type) {
            case Piece.Pawn: pawnsBoard[col] |= bitArray[i]; break;
            case Piece.Knight: knightsBoard[col] |= bitArray[i]; break;
            case Piece.Rook: rooksBoard[col] |= bitArray[i]; break;
            case Piece.Bishop: bishopsBoard[col] |= bitArray[i]; break;
            case Piece.Queen: queensBoard[col] |= bitArray[i]; break;
            case Piece.King: kingsBoard[col] |= bitArray[i]; break;
        }
    }

    public void RemoveBit(int type, int col, int i) {
        switch (type) {
            case Piece.Pawn: pawnsBoard[col] ^= bitArray[i]; break;
            case Piece.Knight: knightsBoard[col] ^= bitArray[i]; break;
            case Piece.Rook: rooksBoard[col] ^= bitArray[i]; break;
            case Piece.Bishop: bishopsBoard[col] ^= bitArray[i]; break;
            case Piece.Queen: queensBoard[col] ^= bitArray[i]; break;
            case Piece.King: kingsBoard[col] ^= bitArray[i]; break;
        }
    }

    //skip turn method for dubugging
    public void TurnSkip() {
        ZobristKey ^= Zobrist.zTurnColour;
        turnColour = turnColour == 0 ? 1 : 0;
        enemyColour = enemyColour == 0 ? 1 : 0;
        gameLogic.EndTurn();
    }
}