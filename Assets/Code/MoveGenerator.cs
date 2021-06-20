using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveGenerator {
    List<Move> moves = new List<Move>();
    const int whiteColourIndex = 0;
    int turnColour;
    bool isWhitesMove;
    Board board;

    int[] squares;

    public List<Move> GenerateMoves(Board board) {
        moves = new List<Move>();
        this.board = board;
        isWhitesMove = board.turnColour == 0;
        turnColour = board.turnColour;
        squares = board.squares;

        GetKingMoves();
        GetPawnMoves();
        GetKnightMoves();
        RookBishopQueen();
        
        return moves;
    }
    
    //returns a int[] with distances to edge in each direction - n,s,e,w,ne,nw,se,sw
    //south,north,west,east, sw,se,nw,ne
    public int[] GetDistance(int loc) {
        int[] nsew = new int[8];
        nsew[0] = loc / 8;
        nsew[1] = 7 - nsew[0];
        loc += 8;
        nsew[3] = loc % 8;
        nsew[2] = 7 - nsew[3];
        nsew[4] = Math.Min(nsew[0], nsew[2]);
        nsew[5] = Math.Min(nsew[0], nsew[3]);
        nsew[6] = Math.Min(nsew[1], nsew[2]);
        nsew[7] = Math.Min(nsew[1], nsew[3]);
        return nsew;
    }
    
    //returns true is there is not a friendly piece at pos
    bool CheckNotFriendly(int pos, int[] board) {
        int t = board[pos];
        if (t == 0 || (Piece.Colour(t) != turnColour)) return true;
        return false;
    }

    void RookBishopQueen() {
        List<int> RookM = board.rooks[turnColour];
        List<int> BishopM = board.bishops[turnColour];
        List<int> QueenM = board.queens[turnColour];
        foreach (int sq in RookM) {
            GetRookMoves(sq);
        }
        foreach (int sq in BishopM) {
            GetBishopMoves(sq);
        }
        foreach (int sq in QueenM) {
            GetBishopMoves(sq);
            GetRookMoves(sq);
        }
    }

    void GetKingMoves() {
        int sq = board.kings[turnColour];
        int[] distances = GetDistance(sq);

        if (distances[0] > 0 && CheckNotFriendly(sq - 8, squares)) moves.Add(new Move(sq, sq - 8));
        if (distances[1] > 0 && CheckNotFriendly(sq + 8, squares)) moves.Add(new Move(sq, sq + 8));
        if (distances[2] > 0 && CheckNotFriendly(sq + 1, squares)) moves.Add(new Move(sq, sq + 1));
        if (distances[3] > 0 && CheckNotFriendly(sq - 1, squares)) moves.Add(new Move(sq, sq - 1));
        if (distances[4] > 0 && CheckNotFriendly(sq - 7, squares)) moves.Add(new Move(sq, sq - 7));
        if (distances[5] > 0 && CheckNotFriendly(sq - 9, squares)) moves.Add(new Move(sq, sq - 9));
        if (distances[6] > 0 && CheckNotFriendly(sq + 9, squares)) moves.Add(new Move(sq, sq + 9));
        if (distances[7] > 0 && CheckNotFriendly(sq + 7, squares)) moves.Add(new Move(sq, sq + 7));
        //Castling logic

        bool[] c = board.Castling;
        if (turnColour == whiteColourIndex) {
            if (c[0] && Piece.Empty(squares[5]) && Piece.Empty(squares[6])) {
                moves.Add(new Move(sq, 6, 2));
            }
            if (c[1] && Piece.Empty(squares[3]) && Piece.Empty(squares[2]) && Piece.Empty(squares[1])) {
                moves.Add(new Move(sq, 2, 2));
            }
        } else {
            if (c[2] && Piece.Empty(squares[61]) && Piece.Empty(squares[62])) {
                moves.Add(new Move(sq, 62, 2));
            }
            if (c[3] && Piece.Empty(squares[59]) && Piece.Empty(squares[58]) && Piece.Empty(squares[57])) {
                moves.Add(new Move(sq, 58, 2));
            }

        }
    }
    void GetPawnMoves() {
        List<int> pawnM = board.pawns[turnColour];
        foreach (int sq in pawnM) {
            int[] distances = GetDistance(sq);
            if (turnColour == whiteColourIndex) {
                if (distances[1] > 0) {
                    if (Piece.Empty(squares[sq + 8])) {
                        moves.Add(new Move(sq, sq + 8));
                        if (distances[0] == 1 && Piece.Empty(squares[sq + 16])) {
                            moves.Add(new Move(sq, sq + 16, Move.Flag.PawnDoubleMove));
                        }
                    }
                }
                if (distances[6] > 0) {
                    PawnDiagonalCheck(sq, sq + 9);
                }
                if (distances[7] > 0) {
                    PawnDiagonalCheck(sq, sq + 7);
                }
            } else {
                if (distances[0] > 0) {
                    if (Piece.Empty(squares[sq - 8])) {
                        moves.Add(new Move(sq, sq - 8));
                        if (distances[1] == 1 && Piece.Empty(squares[sq - 16])) {
                            moves.Add(new Move(sq, sq - 16, Move.Flag.PawnDoubleMove));
                        }
                    }
                }
                if (distances[4] > 0) {
                    PawnDiagonalCheck(sq, sq - 7);
                }
                if (distances[5] > 0) {
                    PawnDiagonalCheck(sq, sq - 9);
                }
            }
        }
    }
    
    //check if a tile contains a enemy for GetPawnMoves
    void PawnDiagonalCheck(int from, int to) {
        
        if(to == board.Enpassant) {
            moves.Add(new Move(from, to, Move.Flag.EnPassantCapture));
            return;
        }
        if (squares[to] == 0) return;
        if (Piece.Colour(squares[to]) != turnColour) {
            moves.Add(new Move(from, to));
        }
    }

    void GetKnightMoves() {
        List<int> KnightM = board.knights[turnColour];
        foreach (int sq in KnightM) {
            int[] distances = GetDistance(sq);
            if (distances[0] >= 2) {
                if (distances[2] >= 1 && CheckNotFriendly(sq - 15, squares)) moves.Add(new Move(sq, sq - 15));
                if (distances[3] >= 1 && CheckNotFriendly(sq - 17, squares)) moves.Add(new Move(sq, sq - 17));
            }
            if (distances[1] >= 2) {
                if (distances[2] >= 1 && CheckNotFriendly(sq + 17, squares)) moves.Add(new Move(sq, sq + 17));
                if (distances[3] >= 1 && CheckNotFriendly(sq + 15, squares)) moves.Add(new Move(sq, sq + 15));
            }
            if (distances[2] >= 2) {
                if (distances[1] >= 1 && CheckNotFriendly(sq + 10, squares)) moves.Add(new Move(sq, sq + 10));
                if (distances[0] >= 1 && CheckNotFriendly(sq - 6, squares)) moves.Add(new Move(sq, sq - 6));
            }
            if (distances[3] >= 2) {
                if (distances[1] >= 1 && CheckNotFriendly(sq + 6, squares)) moves.Add(new Move(sq, sq + 6));
                if (distances[0] >= 1 && CheckNotFriendly(sq - 10, squares)) moves.Add(new Move(sq, sq - 10));
            }
        }
    }

    void Rook() {
        foreach (int sq in board.rooks[turnColour]) {

            int[] distances = GetDistance(sq);
            int[] offset = new int[] { -8, 8, 1, -1 };
            for (int d = 0; d < 4; d++) {
                for (int i = 0; i < distances[0]; i++) {
                    if (CheckNotFriendly(sq + (offset[i] * d), squares)) {
                        if (Piece.Empty(squares[sq + (offset[d] * i)])) {
                            moves.Add(new Move(sq, sq + (offset[i] * d)));
                        } else {
                            moves.Add(new Move(sq, sq + (offset[i] * d)));
                            break;
                        }
                    } else {
                        break;
                    }
                }
            }
        }
    }
    int[] offsets = new int[] { -8, 8, -1, 1 };

    void GetRookMoves(int sq) {
        int[] distances = GetDistance(sq);
        List<int> destinations = new List<int>();
        for (int i = 1; i < distances[0] + 1; i++) {
            if (!Piece.Empty(squares[sq - (8 * i)])) {
                if (CheckNotFriendly(sq - (8 * i), squares)) destinations.Add(sq - (8 * i));
                break;
            } else {
                destinations.Add(sq - (8 * i));
            }
        }
        for (int i = 1; i < distances[1] + 1; i++) {
            if (!Piece.Empty(squares[sq + (8 * i)])) {
                if (CheckNotFriendly(sq + (8 * i), squares)) destinations.Add(sq + (8 * i));
                break;
            } else {
                destinations.Add(sq + (8 * i));
            }
        }
        for (int i = 1; i < distances[2] + 1; i++) {
            if (!Piece.Empty(squares[sq + i])) {
                if (CheckNotFriendly(sq + i, squares)) destinations.Add(sq + i);
                break;
            } else {
                destinations.Add(sq + i);
            }
        }
        for (int i = 1; i < distances[3] + 1; i++) {
            if (!Piece.Empty(squares[sq - i])) {
                if (CheckNotFriendly(sq - i, squares)) destinations.Add(sq - i);
                break;
            } else {
                destinations.Add(sq - i);
            }
        }
        foreach (int destination in destinations) {
            moves.Add(new Move(sq, destination));
        }

    }
    //south,north,west,east, sw,se,nw,ne

    void GetBishopMoves(int sq) {
        int[] distances = GetDistance(sq);
        List<int> destinations = new List<int>();
        for (int i = 1; i < distances[6] + 1; i++) {
            if (!Piece.Empty(squares[sq + (9 * i)])) {
                if (CheckNotFriendly(sq + (9 * i), squares)) moves.Add(new Move(sq, sq + (9 * i)));
                break;
            } else {
                moves.Add(new Move(sq, sq + (9 * i))); 
            }
        }
        for (int i = 1; i < distances[7] + 1; i++) {
            if (!Piece.Empty(squares[sq + (7 * i)])) {
                if (CheckNotFriendly(sq + (7 * i), squares)) moves.Add(new Move(sq, sq + (7 * i)));
                break;
            } else {
                moves.Add(new Move(sq, sq + (7 * i)));
            }
        }
        for (int i = 1; i < distances[4] + 1; i++) {
            if (!Piece.Empty(squares[sq - (7 * i)])) {
                if (CheckNotFriendly(sq - (7 * i), squares)) moves.Add(new Move(sq, sq - (7 * i)));
                break;
            } else {
                moves.Add(new Move(sq, sq - (7 * i)));
            }
        }
        for (int i = 1; i < distances[5] + 1; i++) {
            if (!Piece.Empty(squares[sq - (9 * i)])) {
                if (CheckNotFriendly(sq - (9 * i), squares)) moves.Add(new Move(sq, sq - (9 * i)));
                break;
            } else {
                moves.Add(new Move(sq, sq - (9 * i)));
            }
        }

    }

}
