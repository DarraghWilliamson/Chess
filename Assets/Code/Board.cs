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

    public int Enpassant, MoveCounter, turnColour, enemyColour, captures;
    public int[] squares = new int[64];
    public bool inCheck;
    public bool[] Castling = new bool[4];
    public ulong ZobristKey;
    public Stack<ulong> gameKeys;
    public Stack<GameState> gameStates;
    public Stack<Move> gameMoves;

    public int[] kings;
    public List<int>[] pawns;
    public List<int>[] knights;
    public List<int>[] rooks;
    public List<int>[] bishops;
    public List<int>[] queens;
    public List<int>[] allLists;

    public Board(GameLogic game) {
        gameLogic = game;
    }
    
    public void LoadInfo(string fen) {
        squares = new int[64];
        kings = new int[2];
        ZobristKey = 0;
        gameKeys = new Stack<ulong>();
        gameStates = new Stack<GameState>();
        gameMoves = new Stack<Move>();
        pawns = new List<int>[] { new List<int>(), new List<int>() };
        knights = new List<int>[] { new List<int>(), new List<int>() };
        rooks = new List<int>[] { new List<int>(), new List<int>() };
        bishops = new List<int>[] { new List<int>(), new List<int>() };
        queens = new List<int>[] { new List<int>(), new List<int>() };
        allLists = new List<int>[] { pawns[0], knights[0], rooks[0], bishops[0], queens[0], pawns[1], knights[1], rooks[1], bishops[1], queens[1] };
        LoadInfo info = FEN.LoadNewFEN(fen);

        for (int i = 0; i < 64; i++) {
            squares[i] = info.squares[i];
            if (info.squares[i] != 0) {
                int type = Piece.Type(squares[i]);
                int col = Piece.Colour(squares[i]);
                switch (type) {
                    case Piece.Pawn: pawns[col].Add(i); break;
                    case Piece.Rook: rooks[col].Add(i); break;
                    case Piece.Bishop: bishops[col].Add(i); break;
                    case Piece.Knight: knights[col].Add(i); break;
                    case Piece.Queen: queens[col].Add(i); break;
                    case Piece.King: kings[col] = i; break;
                }
            }
        }
        Castling = info.castling;
        Enpassant = info.enpassant;
        turnColour = info.turnColour;
        enemyColour = turnColour == 0 ? 1 : 0;
        ZobristKey = Zobrist.GetZobristHash(this);
    }

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
    
    public void MovePiece(Move move) {
        if (gameLogic.show) Debug.Log(gameLogic.Log(move));
        int movingFrom = move.StartSquare;
        int movingTo = move.EndSquare;
        int capturedPiece = squares[movingTo];
        bool[] castleCopy = (bool[])Castling.Clone();
        int oldE = Enpassant;
        GameState oldGameState = new GameState(squares, castleCopy, oldE, capturedPiece, allLists);
        gameStates.Push(oldGameState);
        int capturedPieceType = Piece.Type(squares[movingTo]);
        int moveingPieceType = Piece.Type(squares[movingFrom]);
        int movingPiece = squares[movingFrom];
        int moveFlag = move.MoveFlag;
        int newPieceType = 0;
        //if you captured a piece remove it from the list
        if (capturedPieceType != 0) {
            List<int> pieceList = GetList(capturedPieceType, enemyColour);
            pieceList.Remove(movingTo);
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
            List<int> pieceList = GetList(moveingPieceType, turnColour);
            pieceList[pieceList.IndexOf(movingFrom)] = movingTo;
        }
        //castling
        if (moveFlag == Move.Flag.Castling) {
            bool kingSide = movingTo == 6 || movingTo == 62;
            int rookFrom = kingSide ? movingTo + 1 : movingTo - 2;
            int rookTo = kingSide ? movingTo - 1 : movingTo + 1;
            squares[rookTo] = squares[rookFrom];
            squares[rookFrom] = 0;
            List<int> rookList = rooks[turnColour];
            rookList[rookList.IndexOf(rookFrom)] = rookTo;
            ZobristKey ^= Zobrist.zPieces[turnColour, Piece.Rook, rookFrom];
            ZobristKey ^= Zobrist.zPieces[turnColour, Piece.Rook, rookTo];
        }
        //promotion
        
        if (move.IsPromotion) {
            int col = turnColour == 0 ? Piece.White : Piece.Black;
            switch (moveFlag) {
                case Move.Flag.PromotionBishop: newPieceType = Piece.Bishop; bishops[turnColour].Add(movingTo); break;
                case Move.Flag.PromotionKnight: newPieceType = Piece.Knight; knights[turnColour].Add(movingTo); break;
                case Move.Flag.PromotionQueen: newPieceType = Piece.Queen; queens[turnColour].Add(movingTo); break;
                case Move.Flag.PromotionRook: newPieceType = Piece.Rook; rooks[turnColour].Add(movingTo); break;
            }
            int PromotionPiece = newPieceType | col;
            pawns[turnColour].Remove(movingTo); ;
            movingPiece = PromotionPiece;
        }
        //Enpassant captures
        if (moveFlag == Move.Flag.EnPassantCapture) {
            int EnPawnSquare = (Enpassant >= 16 && Enpassant <= 23) ? Enpassant + 8 : Enpassant - 8;
            squares[EnPawnSquare] = 0;
            List<int> pawnList = pawns[enemyColour];
            pawnList.Remove(EnPawnSquare);
            ZobristKey ^= Zobrist.zPieces[enemyColour, Piece.Pawn, EnPawnSquare];
        }
        //actual move in array
        ZobristKey ^= Zobrist.zPieces[turnColour, moveingPieceType, movingFrom];
        if (move.IsPromotion) {
            ZobristKey ^= Zobrist.zPieces[turnColour, newPieceType, movingTo];
        } else {
            ZobristKey ^= Zobrist.zPieces[turnColour, moveingPieceType, movingTo];
        }
        
        
        squares[movingTo] = movingPiece;
        squares[movingFrom] = 0;
        
        //if enpassant, remove Enpassant
        if (Enpassant != 99) {
            ZobristKey ^= Zobrist.zEnpassant[Enpassant%8];
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
        gameMoves.Push(move);
        turnColour = turnColour == 0 ? 1 : 0;
        enemyColour = enemyColour == 0 ? 1 : 0;
        if (ZobristKey != Zobrist.GetZobristHash(this)) {
            Debug.LogError("Zobrist key error\n"+ZobristKey+"\n"+ Zobrist.GetZobristHash(this));
            
        }
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
        if(Enpassant != 99) ZobristKey ^= Zobrist.zEnpassant[Enpassant % 8];
        Enpassant = restoreState.Enpassant;
        if (Enpassant != 99) ZobristKey ^= Zobrist.zEnpassant[Enpassant % 8];



        //move peice back
        if (moveingPieceType == Piece.King) {
            kings[turnColour] = movingFrom;
        } else {
            List<int> pieceList = GetList(moveingPieceType, turnColour);
            pieceList[pieceList.IndexOf(movingTo)] = movingFrom;
        }
        ZobristKey ^= Zobrist.zPieces[turnColour, moveingPieceType, movingTo];
        ZobristKey ^= Zobrist.zPieces[turnColour, movedPieceType, movingFrom];
        squares[movingFrom] = squares[movingTo];
        //replace taken piece
        if (capturedPiece == 0) {
            squares[movingTo] = 0;
        } else {
            ZobristKey ^= Zobrist.zPieces[enemyColour, Piece.Type(capturedPiece), movingTo];
            List<int> pieceList = GetList(Piece.Type(capturedPiece), enemyColour);
            pieceList.Add(movingTo);
            squares[movingTo] = capturedPiece;
        }
        //if promotion change piece back to pawn
        if (move.IsPromotion) {
            List<int> pieceList = GetList(moveingPieceType, turnColour);
            pieceList.Remove(movingFrom);
            int col = turnColour == 0 ? Piece.White : Piece.Black;
            pawns[turnColour].Add(movingFrom);
            squares[movingFrom] = Piece.Pawn | col;
        }
        //add back pawn if empassment capture
        if (moveFlag == Move.Flag.EnPassantCapture) {
            int EnPawnSquare = (movingTo >= 16 && movingTo <= 23) ? movingTo + 8 : movingTo - 8;
            int col = enemyColour == 0 ? Piece.White : Piece.Black;
            squares[EnPawnSquare] = col | Piece.Pawn;
            pawns[enemyColour].Add(EnPawnSquare);
            ZobristKey ^= Zobrist.zPieces[enemyColour, Piece.Pawn, EnPawnSquare];
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
            ZobristKey ^= Zobrist.zPieces[turnColour, Piece.Rook, rookTo];
            ZobristKey ^= Zobrist.zPieces[turnColour, Piece.Rook, rookFrom];
        }
        if (ZobristKey != Zobrist.GetZobristHash(this)) {
            Debug.LogError("Zobrist key error\n" + ZobristKey + "\n" + Zobrist.GetZobristHash(this));
        }
        if (gameMoves.Count!=0)gameMoves.Pop();
        if (gameLogic.show) gameLogic.EndTurn();
    }
    //skip turn method for dubugging
    public void TurnSkip() {
        ZobristKey ^= Zobrist.zTurnColour;
        turnColour = turnColour == 0 ? 1 : 0;
        enemyColour = enemyColour == 0 ? 1 : 0;
        gameLogic.EndTurn();
    }







}
