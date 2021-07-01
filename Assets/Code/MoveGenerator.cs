﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;
using UnityEngine;
using System.Numerics;

using static PreComputedMoves;
public class MoveGenerator {
    Board board;
    List<Move> moves = new List<Move>();

    int turnColour, enemyColour, myKing;
    int[] squares;
    bool inCheck, inDoubleCheck, pinExists;
    bool[] castling;
    const ulong debruijn64 = (0x03f79d71b4cb0a89);
    //const ulong deBruijn64 = 0x03f79d71b4cb0a89UL;
    public static readonly byte[] index64 = {
         0, 47,  1, 56, 48, 27,  2, 60,
         57, 49, 41, 37, 28, 16,  3, 61,
         54, 58, 35, 52, 50, 42, 21, 44,
         38, 32, 29, 23, 17, 11,  4, 62,
         46, 55, 26, 59, 40, 36, 15, 53,
         34, 51, 20, 43, 31, 22, 10, 45,
         25, 39, 14, 33, 19, 30,  9, 24,
         13, 18,  8, 12,  7,  6,  5, 63
    };
    ulong slidingAttackBitmap, knightAttackBitmap, pawnAttackBitmap, enemyAttackBitmapNoPawns,
        enemyAttackBitmap, checkBitmap, turnPieces, enemyPieces, pinBitboard;
    
    ulong newmap;

    public List<Move> GenerateMoves(Board board) {
        moves = new List<Move>();
        this.board = board;
        inCheck = false;
        inDoubleCheck = false;
        pinExists = false;
        
        castling = board.Castling;
        turnColour = board.turnColour;
        squares = board.squares;
        enemyColour = turnColour == 0 ? 1 : 0;
        myKing = board.kings[turnColour];

        GenerateBitboards();
        GenerateMoves();


        board.bitmatSquares.Clear();
        for (int i = 0; i < 64; i++) {
            if (BitboardContains(pinBitboard, i)) {
                board.bitmatSquares.Add(i);
            }
        }
        
        board.inCheck = inCheck;
        return moves;
    }
    
    void GenerateBitboards() {
        checkBitmap = 0;        
        enemyPieces = board.pawnsBoard[enemyColour] | board.knightsBoard[enemyColour] | board.bishopsBoard[enemyColour] | board.rooksBoard[enemyColour] | board.queensBoard[enemyColour] | board.kingsBoard[enemyColour];
        turnPieces = board.pawnsBoard[turnColour] | board.knightsBoard[turnColour] | board.bishopsBoard[turnColour] | board.rooksBoard[turnColour] | board.queensBoard[turnColour] | board.kingsBoard[turnColour];

        GenBitboardSlide();
        GenBitboardKnight();
        GenBitboardPawn();
        enemyAttackBitmapNoPawns = slidingAttackBitmap | knightAttackBitmap | kingAttackBitboards[board.kings[enemyColour]];
        enemyAttackBitmap = enemyAttackBitmapNoPawns | pawnAttackBitmap;
    }

    void GenerateMoves() {
        GetKingMoves();
        if (inDoubleCheck) return; //if in double check only king moves are valid
        GetPawnMoves();
        GetKnightMoves();
        RookBishopQueen();
    }
    
    void GenBitboardSlide() {
        slidingAttackBitmap = 0;
        pinBitboard = 0;
        PieceList rookPieces = board.rooksList[enemyColour];
        PieceList bishopPieces = board.bishopsList[enemyColour];
        PieceList queenPieces = board.queensList[enemyColour];

        for (int i = 0; i < rookPieces.length; i++) AddToBitboardSlide(rookPieces[i], 0, 4);
        for (int i = 0; i < bishopPieces.length; i++) AddToBitboardSlide(bishopPieces[i], 4, 8);
        for (int i = 0; i < queenPieces.length; i++) AddToBitboardSlide(queenPieces[i], 0, 8);
    }

    //takes a bitboard and returns a list of bytes for each non zero entry
    //https://stackoverflow.com/questions/24798499/chess-bitscanning?rq=1
    public static byte[] BitScan(ulong bitboard) {
        if (bitboard == 0) return null;
        var indices = new byte[28];
        var index = 0;
        while (bitboard != 0) {
            indices[index++] = index64[((bitboard ^ (bitboard - 1)) * debruijn64) >> 58];
            bitboard &= bitboard - 1;
        }
        //if no result return null to
        byte[] a = new byte[index];
        for (int i = 0; i < index; i++) a[i] = indices[i];
        return a;
    }
    
    //creates slide attack, pin and check pitboards
    //shoud be re-done later for better efficency
    void AddToBitboardSlide(int startSq, int start, int end) {
        for (int dir = start; dir < end; dir++) {
            ulong ray = rays[startSq][dir];

            if (ray == 0) continue;
            if ((ray & board.kingsBoard[turnColour]) != 0) {
                ulong kingRay = ray & rays[myKing][inverse[dir]];
                if ((kingRay & enemyPieces) == 0) {
                    //if no pieces between them, set check
                    if ((kingRay & turnPieces) == 0) {
                        inDoubleCheck = inCheck;
                        inCheck = true;
                        kingRay |= 1ul << startSq;
                        checkBitmap |= kingRay;
                    } else {
                        //if only a friendly piece, set pin
                        if (BitScan(kingRay & turnPieces).Length == 1) {
                            int pin;
                            if (dir == 0 || dir == 3 || dir == 4 || dir == 5) { //assending index
                                pin = (int)BitScan(kingRay)[0];
                            } else { //decending index
                                byte[] t = BitScan(kingRay);
                                pin = (int)t[t.Length - 1];
                            }
                            ulong pinRay = kingRay & rays[pin][inverse[dir]];
                            pinExists = true;
                            kingRay |= 1ul << startSq;
                            pinRay |= 1ul << startSq;
                            pinBitboard |= kingRay;
                            slidingAttackBitmap |= pinRay;
                            continue;
                        }
                    }
                }
            }
            ulong blocks = ray & (turnPieces | enemyPieces);
            blocks &= ~board.kingsBoard[turnColour];
            //if no blocks add moves 
            if (blocks == 0) {
                byte[] path = BitScan(ray);
                for(int i = 0; i < path.Length; i++) {
                    slidingAttackBitmap |= 1ul << (int)path[i];
                }
                continue;
            }
            int block;
           //if block exists
            if (dir == 0 || dir == 3 || dir == 4 || dir == 5) { //assending index
                block = (int)BitScan(blocks)[0];
            } else { //decending index
                byte[] t = BitScan(blocks);
                block = (int)t[t.Length-1];
            }
            //create ray from block to attacker and add moves
            ulong attackRay = ray & rays[block][inverse[dir]];
            //if(BitboardContains(turnPieces,block)) 
                attackRay |= 1ul << block;
            slidingAttackBitmap |= attackRay;
        }
    }

    void GenBitboardKnight() {
        PieceList knightPieces = board.knightsList[enemyColour];
        knightAttackBitmap = 0;
        for (int i = 0; i < knightPieces.length; i++) {
            knightAttackBitmap |= knightAttackBitboards[knightPieces.pieces[i]];
            if (BitboardContains(knightAttackBitboards[knightPieces.pieces[i]], myKing)) {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkBitmap |= 1ul << knightPieces.pieces[i];
            }
        }
    }
    void GenBitboardPawn() {
        PieceList pawnPieces = board.pawnsList[enemyColour];
        pawnAttackBitmap = 0;
        for (int i = 0; i < pawnPieces.length; i++) {
            pawnAttackBitmap |= pawnAttackBitboards[pawnPieces.pieces[i]][enemyColour];
            if (BitboardContains(pawnAttackBitboards[pawnPieces.pieces[i]][enemyColour], myKing)) {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkBitmap |= 1ul << pawnPieces.pieces[i];
            }
        }
    }

    void GetKingMoves() {
        ulong kingPiece = kingAttackBitboards[myKing];
        kingPiece &= ~turnPieces;
        kingPiece &= ~enemyAttackBitmap;
        byte[] jumps = BitScan(kingPiece);
        if (kingPiece != 0) {
            for (int i = 0; i < jumps.Length; i++) {
                moves.Add(new Move(myKing, (int)jumps[i]));
            }
        }
        if (!inCheck) {
            if (turnColour == 0) {
                if (castling[0] && (squares[5] + squares[6] == 0)) {
                    if (CheckSquare(6) && CheckSquare(5)) moves.Add(new Move(myKing, 6, 2));
                }
                if (castling[1] && (squares[3] + squares[2] + squares[1] == 0)) {
                    if (CheckSquare(2) && CheckSquare(3)) moves.Add(new Move(myKing, 2, 2));
                }
            } else {
                if (castling[2] && (squares[61] + squares[62] == 0)) {
                    if (CheckSquare(62) && CheckSquare(61)) moves.Add(new Move(myKing, 62, 2));
                }
                if (castling[3] && (squares[59] + squares[58] + squares[57] == 0)) {
                    if (CheckSquare(58) && CheckSquare(59)) moves.Add(new Move(myKing, 58, 2));
                }
            }
        }
    }

    void GetPawnMoves() {
        int pawnOffset = offset[turnColour]; 
        int[] diagnols = (turnColour == 0) ? new int[] { 6, 7 } : new int[] { 4, 5 };
        int[] diagnolOffset = (turnColour == 0) ? new int[] { 4, 5 } : new int[] { 6, 7, };
        PieceList pawnPieces = board.pawnsList[turnColour];
        for (int i = 0; i < pawnPieces.length; i++) {
            int startSq = pawnPieces.pieces[i];
            bool IsDoubleMove = distances[startSq][enemyColour] == 1;
            bool promotionMove = distances[startSq][turnColour] == 1;
            int endSq = startSq + pawnOffset;


            //if sqare empty
            if (squares[endSq] == 0) {
                //if not pinned or is moving in pin
                if (!IsPinned(startSq) || IsMovingAlongRay(startSq, turnColour)) {                
                    //if (!pinExists || (!pinnedPieces.ContainsKey(pawnSquare) || pinnedPieces[pawnSquare].Contains(moveTo))) {
                    //if not in check or is moving to stop check
                    if (!inCheck || InCheckMap(endSq)) {
                        if (promotionMove) {
                            moves.Add(new Move(startSq, endSq, 4));
                            moves.Add(new Move(startSq, endSq, 5));
                            moves.Add(new Move(startSq, endSq, 6));
                            moves.Add(new Move(startSq, endSq, 7));
                        } else {
                            moves.Add(new Move(startSq, endSq));
                        }
                    }
                    //if can move double and sq empty
                    if (IsDoubleMove && squares[endSq + pawnOffset] == 0) {
                        //recheck check, no need to recheck pin
                        if (!inCheck || InCheckMap(endSq + pawnOffset)) {
                            moves.Add(new Move(startSq, endSq + pawnOffset, Move.Flag.PawnDoubleMove));
                        }
                    }
                }
            }
            //check pawn captures
            for (int d = 0; d < 2; d++) {
                endSq = startSq + offset[diagnolOffset[d]];
                if (distances[startSq][diagnols[d]] > 0) {
                    //recheck pins
                    if (!IsPinned(startSq) || IsMovingAlongRay(startSq, diagnolOffset[d])) {
                        //if (!pinnedPieces.ContainsKey(pawnSquare) || pinnedPieces[pawnSquare].Contains(moveTo)) {
                        //recheck check
                        int eset = (turnColour == 0) ? -8 : 8;
                        if (BitboardContains(checkBitmap, board.Enpassant + eset)) {
                            //Enpass capture
                            if (endSq == board.Enpassant && ValidateEnpassant(startSq, endSq)) {
                                moves.Add(new Move(startSq, endSq, Move.Flag.EnPassantCapture));
                            }
                        }
                        if (!inCheck || InCheckMap(endSq)) {
                            //Enpass capture
                            if (board.Enpassant != 0 && endSq == board.Enpassant && ValidateEnpassant(startSq, endSq)) {
                                moves.Add(new Move(startSq, endSq, Move.Flag.EnPassantCapture));
                            }
                            //regular capture
                            if (squares[endSq] != 0 && Piece.Colour(squares[endSq]) != turnColour) {
                                if (promotionMove) {
                                    moves.Add(new Move(startSq, endSq, 4));
                                    moves.Add(new Move(startSq, endSq, 5));
                                    moves.Add(new Move(startSq, endSq, 6));
                                    moves.Add(new Move(startSq, endSq, 7));
                                } else {
                                    moves.Add(new Move(startSq, endSq));
                                }
                            }
                        }
                    }
                }
            }

        }
    }
    bool ValidateEnpassant(int moveFrom, int moveTo) {
        int Enpassant = board.Enpassant;
        int EnPawnSquare = (Enpassant >= 16 && Enpassant <= 23) ? Enpassant + 8 : Enpassant - 8;
        //create board clone to test on
        int[] copy = (int[])squares.Clone();
        copy[EnPawnSquare] = 0;
        copy[moveTo] = copy[moveFrom];
        copy[moveFrom] = 0;
        //test if move puts player in check
        int sq = board.kings[turnColour];
        int enemyPieceCol = (enemyColour == 0) ? Piece.White : Piece.Black;
        int enemyRook = enemyPieceCol | Piece.Rook;
        int enemyBishop = enemyPieceCol | Piece.Bishop;
        int enemyQueen = enemyPieceCol | Piece.Queen;
        for (int d = 0; d < 8; d++) {
            for (int i = 1; i < distances[sq][d] + 1; i++) {
                int to = sq + (offset[d] * i);
                if (copy[to] == 0) {
                    continue;
                }
                if (Piece.Colour(copy[to]) == turnColour) {
                    break;
                } else {
                    if ((d <= 3) && copy[to] == enemyRook) {
                        return false;
                    }
                    if ((d >= 4) && copy[to] == enemyBishop) {
                        return false;
                    }
                    if (copy[to] == enemyQueen) {
                        return false;
                    }
                    break;
                }
            }
        }
        return true;
    }

    void GetKnightMoves() {
        PieceList KnightM = board.knightsList[turnColour];
        for (int i = 0; i < KnightM.length; i++) {
            int knight = KnightM.pieces[i];
            if (IsPinned(knight)) continue;
            ulong nMoves = knightAttackBitboards[KnightM[i]];
            nMoves &= ~turnPieces;
            if (inCheck) nMoves &= checkBitmap;
            for (int move = 0; move < knightMoves[knight].Length; move++) {
                if (BitboardContains(nMoves, knightMoves[knight][move])) {
                    moves.Add(new Move(knight, knightMoves[knight][move]));
                }
            }
        }
    }
    void RookBishopQueen() {
        //if (inDoubleCheck) return;
        PieceList RookM = board.rooksList[turnColour];
        PieceList BishopM = board.bishopsList[turnColour];
        PieceList QueenM = board.queensList[turnColour];
        for (int i = 0; i < RookM.length; i++) GetSlideMoves(RookM.pieces[i], 0, 4);
        for (int i = 0; i < BishopM.length; i++) GetSlideMoves(BishopM.pieces[i], 4, 8);
        for (int i = 0; i < QueenM.length; i++) GetSlideMoves(QueenM.pieces[i], 0, 8);
    }
    void GetSlideMoves(int startSq, int start, int end) {
        for (int dir = start; dir < end; dir++) {
            for (int i = 1; i < distances[startSq][dir] + 1; i++) {
                int moveTo = startSq + (offset[dir] * i);

                //piece exists
                if (squares[moveTo] != 0) {
                    //if piece is a enemy piece
                    if (Piece.Colour(squares[moveTo]) != turnColour) {
                        //if not in check or this move removes check
                        if (!inCheck || InCheckMap(moveTo)) {
                            //if not in pinned or moving along pin
                            if (!IsPinned(startSq) || IsMovingAlongRay(startSq, dir)) {
                                moves.Add(new Move(startSq, moveTo));
                            }
                        }
                    }
                    break;
                } else {
                    //if not in check or this move removes check
                    if (!inCheck || InCheckMap(moveTo)) {
                        //if not in pinned or moving along pin
                        if (!IsPinned(startSq) || IsMovingAlongRay(startSq, dir)) {
                            // if (!pinnedPieces.ContainsKey(startSq) || pinnedPieces[startSq].Contains(moveTo)) {
                            moves.Add(new Move(startSq, moveTo));
                        }
                    }
                }
            }
        }
    }

    bool CheckSquare(int sq) {
        return !BitboardContains(enemyAttackBitmap, sq);
    }
    bool IsMovingAlongRay(int square, int dir) {
        ulong ray = rays[square][dir] | rays[square][inverse[dir]];
        ulong kingInRay = board.kingsBoard[turnColour] & ray;
        return kingInRay != 0;
    }
    bool IsPinned(int square) {
        return pinExists && ((pinBitboard >> square) & 1) != 0;
    }
    bool InCheckMap(int square) {
        return inCheck && ((checkBitmap >> square) & 1) != 0;
    }
    bool BitboardContains(ulong board, int sq) {
        return ((board >> sq) & 1) != 0;
    }

}
