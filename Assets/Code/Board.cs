using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;
using UnityEngine;

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

    public int Enpassant, MoveCounter, turnColour, enemyColour, captures;
    public int[] squares = new int[64];
    public bool inCheck;
    public bool[] Castling = new bool[4];
    public ulong ZobristKey;
    public Stack<ulong> gameKeys;
    public Stack<GameState> gameStates;
    public Stack<Move> gameMoves;
    public int[] kings;
    public PieceList[] pawnsList, knightsList, rooksList, bishopsList, queensList, allLists;
    public ulong[] pawnsBoard, knightsBoard, rooksBoard, bishopsBoard, queensBoard, kingsBoard, allBoards;
    public ulong[] bitArray;

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
        gameStates = new Stack<GameState>();
        gameMoves = new Stack<Move>();
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
        allBoards = new ulong[] { pawnsBoard[0], knightsBoard[0], rooksBoard[0], bishopsBoard[0], queensBoard[0], kingsBoard[0],
            pawnsBoard[1], knightsBoard[1], rooksBoard[1], bishopsBoard[1], queensBoard[1], kingsBoard[1] };

        LoadInfo info = FEN.LoadNewFEN(fen);

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
        Castling = info.castling;
        Enpassant = info.enpassant;
        turnColour = info.turnColour;
        enemyColour = turnColour == 0 ? 1 : 0;
        ZobristKey = Zobrist.GetZobristHash(this);

    }
    bool BitboardContains(ulong board, int sq) {
        return ((board >> sq) & 1) != 0;
    }
    public List<Move> GenerateMoves() {
        

        return moveGenerator.GenerateMoves(this);
    }

    public void MovePiece(Move move) {

        if (gameLogic.show) Console.Write(gameLogic.Log(move));

        int movingFrom = move.StartSquare;
        int movingTo = move.EndSquare;
        int capturedPiece = squares[movingTo];
        bool[] castleCopy = (bool[])Castling.Clone();
        int oldE = Enpassant;
        GameState oldGameState = new GameState(squares, castleCopy, oldE, capturedPiece);
        gameStates.Push(oldGameState);
        int capturedPieceType = Piece.Type(squares[movingTo]);
        int moveingPieceType = Piece.Type(squares[movingFrom]);
        int movingPiece = squares[movingFrom];
        int moveFlag = move.MoveFlag;
        int newPieceType = 0;


        
        

        //if you captured a piece remove it from the list
        if (capturedPieceType != 0) {
            PieceList pieceList = GetList(capturedPieceType, enemyColour);
            if (pieceList == null) {
                if (capturedPieceType == 1) {
                    Debug.Log("tried to capture king type:" + moveingPieceType + " from:" + movingFrom + " to:" + movingTo + " col1:" + Piece.Colour(movingPiece) + " col2:" + Piece.Colour(capturedPiece));
                } else {
                    Debug.Log("null piecelist type:" + moveingPieceType + " from:" + movingFrom + " to:" + movingTo + " col1:" + Piece.Colour(movingPiece) + " col2:" + Piece.Colour(capturedPiece));
                }

            }
            pieceList.Remove(movingTo);
            RemoveBit(capturedPieceType, enemyColour, movingTo);
            ZobristKey ^= Zobrist.zPieces[enemyColour, capturedPieceType, movingTo];
        }
        //move pieces around in the lists
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
            PieceList pieceList = GetList(moveingPieceType, turnColour);
            if (pieceList == null) {
                Debug.Log(moveingPieceType + " ");
            }
            pieceList.Move(movingFrom, movingTo);
        }
        //castling
        if (moveFlag == Move.Flag.Castling) {
            bool kingSide = movingTo == 6 || movingTo == 62;
            int rookFrom = kingSide ? movingTo + 1 : movingTo - 2;
            int rookTo = kingSide ? movingTo - 1 : movingTo + 1;
            squares[rookTo] = squares[rookFrom];
            squares[rookFrom] = 0;
            PieceList rookList = rooksList[turnColour];
            rookList.Move(rookFrom, rookTo);

            RemoveBit(Piece.Rook, turnColour, rookFrom);
            AddBit(Piece.Rook, turnColour, rookTo);
            ZobristKey ^= Zobrist.zPieces[turnColour, Piece.Rook, rookFrom];
            ZobristKey ^= Zobrist.zPieces[turnColour, Piece.Rook, rookTo];
        }
        //promotion
        if (move.IsPromotion) {
            int col = turnColour == 0 ? Piece.White : Piece.Black;
            switch (moveFlag) {
                case Move.Flag.PromotionBishop: newPieceType = Piece.Bishop; bishopsList[turnColour].Push(movingTo); break;
                case Move.Flag.PromotionKnight: newPieceType = Piece.Knight; knightsList[turnColour].Push(movingTo); break;
                case Move.Flag.PromotionQueen: newPieceType = Piece.Queen; queensList[turnColour].Push(movingTo); break;
                case Move.Flag.PromotionRook: newPieceType = Piece.Rook; rooksList[turnColour].Push(movingTo); break;
            }
            int PromotionPiece = newPieceType | col;
            pawnsList[turnColour].Remove(movingTo); ;
            movingPiece = PromotionPiece;
        }
        //Enpassant captures
        if (moveFlag == Move.Flag.EnPassantCapture) {
            int EnPawnSquare = (Enpassant >= 16 && Enpassant <= 23) ? Enpassant + 8 : Enpassant - 8;
            squares[EnPawnSquare] = 0;
            PieceList pawnList = pawnsList[enemyColour];
            pawnList.Remove(EnPawnSquare);

            RemoveBit(Piece.Pawn, enemyColour, EnPawnSquare);
            ZobristKey ^= Zobrist.zPieces[enemyColour, Piece.Pawn, EnPawnSquare];
        }

        //actual move in array
        RemoveBit(moveingPieceType, turnColour, movingFrom);
        ZobristKey ^= Zobrist.zPieces[turnColour, moveingPieceType, movingFrom];
        if (move.IsPromotion) {
            AddBit(newPieceType, turnColour, movingTo);
            ZobristKey ^= Zobrist.zPieces[turnColour, newPieceType, movingTo];
        } else {
            AddBit(moveingPieceType, turnColour, movingTo);
            ZobristKey ^= Zobrist.zPieces[turnColour, moveingPieceType, movingTo];
        }

        
        squares[movingTo] = movingPiece;
        squares[movingFrom] = 0;

        //if enpassant, remove Enpassant
        if (Enpassant != 99) {
            ZobristKey ^= Zobrist.zEnpassant[Enpassant % 8];
            Enpassant = 99;
        }
        //if double move, reAdd Enpassant
        if (moveFlag == Move.Flag.PawnDoubleMove) {
            int i = movingFrom - movingTo == 16 ? -8 : 8;
            Enpassant = (movingFrom + i);
            ZobristKey ^= Zobrist.zEnpassant[Enpassant % 8];
        }
        //set castling false if required pieces move
        if (movingFrom == 7 || movingTo == 7) Castling[0] = false;
        if (movingFrom == 0 || movingTo == 0) Castling[1] = false;
        if (movingFrom == 63 || movingTo == 63) Castling[2] = false;
        if (movingFrom == 56 || movingTo == 56) Castling[3] = false;
        //if castling was changed, update key
        for (int castle = 0; castle < 4; castle++) {
            if (Castling[castle] != castleCopy[castle]) {
                ZobristKey ^= Zobrist.zCastling[castle];
            }
        }
        ZobristKey ^= Zobrist.zTurnColour;
        //store the move
        turnColour = turnColour == 0 ? 1 : 0;
        enemyColour = enemyColour == 0 ? 1 : 0;
        //if (ZobristKey != Zobrist.GetZobristHash(this)) Console.Write("Zobrist key error\n"+ZobristKey+"\n"+ Zobrist.GetZobristHash(this));
        if (gameLogic.show) gameLogic.EndTurn();
    }

    public void CtrlZ(Move move) {
        //roll back turn
        turnColour = turnColour == 0 ? 1 : 0;
        enemyColour = enemyColour == 0 ? 1 : 0;
        ZobristKey ^= Zobrist.zTurnColour;
        GameState restoreState = gameStates.Pop();
        int capturedPiece = restoreState.CapturedPiece;
        int movingTo = move.EndSquare;
        int movingFrom = move.StartSquare;
        int moveFlag = move.MoveFlag;
        int moveingPieceType = Piece.Type(squares[movingTo]);
        int movedPieceType = move.IsPromotion == true ? Piece.Pawn : moveingPieceType;

        //restore castling 
        for (int castle = 0; castle < 4; castle++) {
            if (Castling[castle] != restoreState.Castling[castle]) {
                ZobristKey ^= Zobrist.zCastling[castle];
            }
        }
        Castling = restoreState.Castling;
        //restore Enpassant
        if (Enpassant != 99) ZobristKey ^= Zobrist.zEnpassant[Enpassant % 8];
        Enpassant = restoreState.Enpassant;
        if (Enpassant != 99) ZobristKey ^= Zobrist.zEnpassant[Enpassant % 8];



        //move peice back
        if (moveingPieceType == Piece.King) {
            kings[turnColour] = movingFrom;
        } else {
            PieceList pieceList = GetList(moveingPieceType, turnColour);
            pieceList.Move(movingTo, movingFrom);
            //pieceList[pieceList.IndexOf(movingTo)] = movingFrom;
        }
        RemoveBit(moveingPieceType, turnColour, movingTo);
        AddBit(movedPieceType, turnColour, movingFrom);
        ZobristKey ^= Zobrist.zPieces[turnColour, moveingPieceType, movingTo];
        ZobristKey ^= Zobrist.zPieces[turnColour, movedPieceType, movingFrom];
        squares[movingFrom] = squares[movingTo];
        //replace taken piece
        if (capturedPiece == 0) {
            squares[movingTo] = 0;
        } else {
            AddBit(Piece.Type(capturedPiece), enemyColour, movingTo);
            ZobristKey ^= Zobrist.zPieces[enemyColour, Piece.Type(capturedPiece), movingTo];
            PieceList pieceList = GetList(Piece.Type(capturedPiece), enemyColour);
            pieceList.Push(movingTo);
            squares[movingTo] = capturedPiece;
        }
        //if promotion change piece back to pawn
        if (move.IsPromotion) {
            PieceList pieceList = GetList(moveingPieceType, turnColour);
            pieceList.Remove(movingFrom);
            int col = turnColour == 0 ? Piece.White : Piece.Black;
            pawnsList[turnColour].Push(movingFrom);
            squares[movingFrom] = Piece.Pawn | col;
        }
        //add back pawn if empassment capture
        if (moveFlag == Move.Flag.EnPassantCapture) {
            int EnPawnSquare = (movingTo >= 16 && movingTo <= 23) ? movingTo + 8 : movingTo - 8;
            int col = enemyColour == 0 ? Piece.White : Piece.Black;
            squares[EnPawnSquare] = col | Piece.Pawn;
            pawnsList[enemyColour].Push(EnPawnSquare);
            AddBit(Piece.Pawn, enemyColour, EnPawnSquare);
            ZobristKey ^= Zobrist.zPieces[enemyColour, Piece.Pawn, EnPawnSquare];
        }
        //move back rook if castle
        if (moveFlag == Move.Flag.Castling) {
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
        // if (ZobristKey != Zobrist.GetZobristHash(this)) Console.Write("Zobrist key error\n" + ZobristKey + "\n" + Zobrist.GetZobristHash(this));
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
            case Piece.Pawn:    pawnsBoard[col] |= bitArray[i]; break;
            case Piece.Knight:  knightsBoard[col] |= bitArray[i]; break;
            case Piece.Rook:    rooksBoard[col] |= bitArray[i]; break;
            case Piece.Bishop:  bishopsBoard[col] |= bitArray[i]; break;
            case Piece.Queen:   queensBoard[col] |= bitArray[i]; break;
            case Piece.King:    kingsBoard[col] |= bitArray[i]; break;
        }
    }

    public void RemoveBit(int type, int col, int i) {
        switch (type) {
            case Piece.Pawn:    pawnsBoard[col] ^= bitArray[i]; break;
            case Piece.Knight:  knightsBoard[col] ^= bitArray[i]; break;
            case Piece.Rook:    rooksBoard[col] ^= bitArray[i]; break;
            case Piece.Bishop:  bishopsBoard[col] ^= bitArray[i]; break;
            case Piece.Queen:   queensBoard[col] ^= bitArray[i]; break;
            case Piece.King:    kingsBoard[col] ^= bitArray[i]; break;
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
