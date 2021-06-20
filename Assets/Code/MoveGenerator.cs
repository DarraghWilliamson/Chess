using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveGenerator {
    List<Move> moves = new List<Move>();
    const int whiteColourIndex = 0;
    int turnColour;
    int enemyColour;
    int enemyPieceCol;
    bool isWhitesMove;
    bool inCheck;
    Board board;
    public List<int> SquaresUnderAttack = new List<int>();
    int king;
    int[] squares;
    List<int> checkMoves = new List<int>();

    public List<Move> GenerateMoves(Board board) {
        checkMoves = new List<int>();
        moves = new List<Move>();
        this.board = board;
        isWhitesMove = board.turnColour == 0;
        turnColour = board.turnColour;
        enemyColour = turnColour == 0 ? 1 : 0;
        enemyPieceCol = (enemyColour == whiteColourIndex) ? Piece.White : Piece.Black;
        squares = board.squares;
        int PieceCol = (turnColour == whiteColourIndex) ? Piece.White : Piece.Black;
        king = PieceCol | Piece.King;
        inCheck = CheckSquare(board.kings[turnColour]);
        board.inCheck = inCheck;
        Dictionary<int, List<int>> pinned = GetPinned();

        GetKingMoves();
        GetPawnMoves();
        GetKnightMoves();
        RookBishopQueen();



        List<Move> filteredMoves = new List<Move>();
        foreach(Move move in moves) {
            if (pinned.ContainsKey(move.StartSquare)) {
                if (pinned[move.StartSquare].Contains(move.EndSquare)) {
                    filteredMoves.Add(move);
                }
            } else {
                filteredMoves.Add(move);
            }
        }
        if (inCheck) {
            List<Move> filteredMoves2 = new List<Move>();
            foreach (Move move in moves) {
                if(checkMoves.Contains(move.EndSquare)) {
                    filteredMoves2.Add(move);
                }
                if(move.StartSquare == board.kings[turnColour]) {
                    filteredMoves2.Add(move);
                }
            }
            return filteredMoves2;
            }
        return filteredMoves;
    }

    //returns a int[] with distances to edge in each direction - n,s,e,w,ne,nw,se,sw
    //south,north,west,east, sw,se,nw,ne
    int[] GetDistance(int loc) {
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

    //returns true if square is under attack
    bool CheckSquare(int sq) {
        SquaresUnderAttack.Clear();
        GetAttacks();
        if (IsUnderAttack(sq)) return true;
        if (SquaresUnderAttack.Contains(sq)) return true;
        return false;
    }

    //check to see if a square is under attack from non-sliding piece
    public bool IsUnderAttack(int sq) {
        int[] distances = GetDistance(sq);
        int[] offset = new int[] { -8, 8, 1, -1, -7, -9, 9, 7 };

        //check for attacking pawns
        if (turnColour == whiteColourIndex) {
            if (distances[6] > 0 && squares[sq + 9] == 18) {
                checkMoves.Add(sq + 9);
                return true;
            }
            if (distances[7] > 0 && squares[sq + 7] == 18) { 
                checkMoves.Add(sq + 7); 
                return true; 
            }
        } else {
            if (distances[4] > 0 && squares[sq - 7] == 10) {
                checkMoves.Add(sq - 7);
                return true;
            }
            if (distances[5] > 0 && squares[sq - 9] == 10) {
                checkMoves.Add(sq - 9);
                return true;
            }
        }
        int enemyKnight = enemyPieceCol | Piece.Knight;
        int enemyKing = enemyPieceCol | Piece.King;
        //check for attacking knight
        if (distances[0] >= 2) {
            if (distances[2] >= 1) { if (squares[sq - 15] == enemyKnight) { checkMoves.Add(sq - 15); return true; } }
            if (distances[3] >= 1) { if (squares[sq - 17] == enemyKnight) { checkMoves.Add(sq - 17); return true; } }
        }
        if (distances[1] >= 2) {
            if (distances[2] >= 1) { if (squares[sq + 17] == enemyKnight) { checkMoves.Add(sq + 17); return true; } }
            if (distances[3] >= 1) { if (squares[sq + 15] == enemyKnight) { checkMoves.Add(sq + 15); return true; } }
        }
        if (distances[2] >= 2) {
            if (distances[1] >= 1) { if (squares[sq + 10] == enemyKnight) { checkMoves.Add(sq + 10); return true; } }
            if (distances[0] >= 1) { if (squares[sq - 6] == enemyKnight) { checkMoves.Add(sq - 6); return true; } }
        }
        if (distances[3] >= 2) {
            if (distances[1] >= 1) { if (squares[sq + 6] == enemyKnight) { checkMoves.Add(sq + 6); return true; } }
            if (distances[0] >= 1) { if (squares[sq - 10] == enemyKnight) { checkMoves.Add(sq - 10); return true; } }
        }
        //check for attacking king
        for (int d = 0; d < 8; d++) {
            if (distances[d] > 1) distances[d] = 1;
            for (int i = 1; i < distances[d] + 1; i++) {
                if (squares[sq + (offset[d] * i)] == enemyKing) {
                    return true;
                }
            }
        }
        return false;
    }

    void GetAttacks() {
        List<int> RookM = board.rooks[enemyColour];
        List<int> BishopM = board.bishops[enemyColour];
        List<int> QueenM = board.queens[enemyColour];
        foreach (int sq in RookM) UnderSlideAttack(sq, 0, 4);
        foreach (int sq in BishopM) UnderSlideAttack(sq, 4, 8);
        foreach (int sq in QueenM) UnderSlideAttack(sq, 0, 8);
    }
    void UnderSlideAttack(int sq, int start, int end) {
        int[] distances = GetDistance(sq);
        int[] offset = new int[] { -8, 8, 1, -1, -7, -9, 9, 7 };
        for (int d = start; d < end; d++) {
            for (int i = 1; i < distances[d] + 1; i++) {
                if (squares[sq + (offset[d] * i)] != 0) {

                    if (squares[sq + (offset[d] * i)] != king) {
                        break;
                    }
                    
                }
                SquaresUnderAttack.Add(sq + (offset[d] * i));
            }
        }
    }

    Dictionary<int, List<int>> GetPinned() {
        Dictionary<int, List<int>> pinned = new Dictionary<int, List<int>>();
        int sq = board.kings[turnColour];
        int enemyRook = enemyPieceCol | Piece.Rook;
        int enemyBishop = enemyPieceCol | Piece.Bishop;
        int enemyQueen = enemyPieceCol | Piece.Queen;

        int[] distances = GetDistance(sq);
        int[] offset = new int[] { -8, 8, 1, -1, -7, -9, 9, 7 };
        for (int d = 0; d < 8; d++) {
            List<int> Temp = new List<int>();
            int pinnedPos = 0;
            int friendCount = 0;
            bool slider = false;
            for (int i = 1; i < distances[d] + 1; i++) {
                int to = sq + (offset[d] * i);
                if (squares[to] == 0) {
                    Temp.Add(to);
                    continue;
                }
                if (Piece.Colour(squares[to]) == turnColour) {
                    friendCount++;
                    pinnedPos = to;
                    if (friendCount > 1) {
                        break;
                    }
                } else {
                    if (squares[to] == enemyBishop || squares[to] == enemyRook || squares[to] == enemyQueen) {
                        Temp.Add(to);
                        slider = true;
                    }
                    break;
                }
            }
            if(slider && friendCount == 0) {
                checkMoves.AddRange(Temp);
            }
            if (slider && friendCount == 1) {
                pinned.Add(pinnedPos, Temp);
            }
        }
        return pinned;
    }

    //returns true is there is not a friendly piece at pos
    bool NotFriendly(int pos) {
        int t = squares[pos];
        if (t == 0 || (Piece.Colour(t) != turnColour)) return true;
        return false;
    }

    void RookBishopQueen() {
        List<int> RookM = board.rooks[turnColour];
        List<int> BishopM = board.bishops[turnColour];
        List<int> QueenM = board.queens[turnColour];
        foreach (int sq in RookM) Slide(sq, 0, 4);
        foreach (int sq in BishopM) Slide(sq, 4, 8);
        foreach (int sq in QueenM) Slide(sq, 0, 8);
    }

    void GetKingMoves() {
        int sq = board.kings[turnColour];
        int[] distances = GetDistance(sq);
        int[] offset = new int[] { -8, 8, 1, -1, -7, -9, 9, 7 };
        for (int d = 0; d < 8; d++) {
            if (distances[d] > 1) distances[d] = 1;
            for (int i = 1; i < distances[d] + 1; i++) {
                if (squares[sq + (offset[d] * i)] != 0) {
                    if (Piece.Colour(squares[sq + (offset[d] * i)]) != turnColour) {
                        if (!CheckSquare(sq + (offset[d] * i))) {
                        moves.Add(new Move(sq, sq + (offset[d] * i)));
                        }
                    }
                    break;
                } else {
                    if (!CheckSquare(sq + (offset[d] * i))) {
                    moves.Add(new Move(sq, sq + (offset[d] * i)));
                    }
                }
            }
        }
        //Castling logic
        bool[] c = board.Castling;
        if (turnColour == whiteColourIndex) {
            if (c[0] && Piece.Empty(squares[5]) && Piece.Empty(squares[6])) {
                if (!CheckSquare(6)) moves.Add(new Move(sq, 6, 2));
            }
            if (c[1] && Piece.Empty(squares[3]) && Piece.Empty(squares[2]) && Piece.Empty(squares[1])) {
                if (!CheckSquare(2)) moves.Add(new Move(sq, 2, 2));
            }
        } else {
            if (c[2] && Piece.Empty(squares[61]) && Piece.Empty(squares[62])) {
                if (!CheckSquare(62)) moves.Add(new Move(sq, 62, 2));
            }
            if (c[3] && Piece.Empty(squares[59]) && Piece.Empty(squares[58]) && Piece.Empty(squares[57])) {
                if (!CheckSquare(58)) moves.Add(new Move(sq, 58, 2));
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
                        if (distances[1] == 1) {
                            PawnPromotion(sq, sq + 8);
                        } else {
                            moves.Add(new Move(sq, sq + 8));
                        }
                        if (distances[0] == 1 && Piece.Empty(squares[sq + 16])) {
                            moves.Add(new Move(sq, sq + 16, Move.Flag.PawnDoubleMove));
                        }
                    }
                }
                if (distances[6] > 0) PawnDiagonalCheck(sq, sq + 9);
                if (distances[7] > 0) PawnDiagonalCheck(sq, sq + 7);
            } else {
                if (distances[0] > 0) {
                    if (Piece.Empty(squares[sq - 8])) {
                        if (distances[0] == 1) {
                            PawnPromotion(sq, sq - 8);
                        } else {
                            moves.Add(new Move(sq, sq - 8));
                        }
                        if (distances[1] == 1 && Piece.Empty(squares[sq - 16])) {
                            moves.Add(new Move(sq, sq - 16, Move.Flag.PawnDoubleMove));
                        }
                    }
                }
                if (distances[4] > 0) PawnDiagonalCheck(sq, sq - 7);
                if (distances[5] > 0) PawnDiagonalCheck(sq, sq - 9);
            }
        }
    }
    //make the moves for promoting pawns;
    void PawnPromotion(int from, int to) {
        moves.Add(new Move(from, to, Move.Flag.Promotion));
    }
    //check if a tile contains a enemy for GetPawnMoves
    void PawnDiagonalCheck(int from, int to) {
        if (to == board.Enpassant) {
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
                if (distances[2] >= 1 && NotFriendly(sq - 15)) moves.Add(new Move(sq, sq - 15));
                if (distances[3] >= 1 && NotFriendly(sq - 17)) moves.Add(new Move(sq, sq - 17));
            }
            if (distances[1] >= 2) {
                if (distances[2] >= 1 && NotFriendly(sq + 17)) moves.Add(new Move(sq, sq + 17));
                if (distances[3] >= 1 && NotFriendly(sq + 15)) moves.Add(new Move(sq, sq + 15));
            }
            if (distances[2] >= 2) {
                if (distances[1] >= 1 && NotFriendly(sq + 10)) moves.Add(new Move(sq, sq + 10));
                if (distances[0] >= 1 && NotFriendly(sq - 6)) moves.Add(new Move(sq, sq - 6));
            }
            if (distances[3] >= 2) {
                if (distances[1] >= 1 && NotFriendly(sq + 6)) moves.Add(new Move(sq, sq + 6));
                if (distances[0] >= 1 && NotFriendly(sq - 10)) moves.Add(new Move(sq, sq - 10));
            }
        }
    }

    void Slide(int sq, int start, int end) {
        int[] distances = GetDistance(sq);
        if (sq == board.kings[turnColour]) {
            for (int x = 0; x < distances.Length; x++) {
                if (distances[x] > 1) distances[x] = 1;
            }
        }
        int[] offset = new int[] { -8, 8, 1, -1, -7, -9, 9, 7 };
        for (int d = start; d < end; d++) {
            for (int i = 1; i < distances[d] + 1; i++) {
                if (squares[sq + (offset[d] * i)] != 0) {
                    if (Piece.Colour(squares[sq + (offset[d] * i)]) != turnColour) {
                        moves.Add(new Move(sq, sq + (offset[d] * i)));
                    }
                    break;
                } else {
                    moves.Add(new Move(sq, sq + (offset[d] * i)));
                }
            }
        }

    }


}
