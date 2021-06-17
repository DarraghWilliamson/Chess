using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PieceLogic {
    GameLogic gameLogic;
    Board board;
    public static PieceLogic instance;
    char ori;

    public PieceLogic() {
        instance = this;
        gameLogic = GameLogic.instance;
        board = gameLogic.board;
    }

    
    
    public List<Move> GetMoves(int i) {
        Colour colour = char.IsUpper(board.board[i]) ? Colour.White : Colour.Black;
        return GetMovesMethod(i, colour, board.board);
    }
    public List<Move> GetMoves(int i, Colour colour, char[] b) {
        return GetMovesMethod(i, colour, b);
    }

    public List<Move> GetMovesMethod(int i, Colour colour, char[] board) {
        ori = board[i];
        if (char.ToLower(ori) == 'p') return GetPawnMoves(colour, i, board);
        if (char.ToLower(ori) == 'n') return GetKnightMoves(i, board);
        if (char.ToLower(ori) == 'r') return GetRookMoves(i, board);
        if (char.ToLower(ori) == 'b') return GetBishopMoves(i, board);
        if (char.ToLower(ori) == 'q') return GetQueenMoves(i, board);
        if (char.ToLower(ori) == 'k') return GetKingMoves(colour, i, board);
        return null;
    }
    //returns a int[] with distances to edge in each direction - n,s,e,w,ne,nw,se,sw
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
    //check if a tile contains a enemy for GetPawnMoves
    bool CheckEnemy(int i, char[] board) {
        char t = board[i];
        if (char.IsLetter(t) && char.IsUpper(t) != char.IsUpper(ori)) return true;
        return false;
    }
    //returns true is there is not a friendly piece at pos
    bool CheckNotFriendly(int pos, char[] board) {
        char t = board[pos];
        if (!char.IsLetter(t) || (char.IsUpper(t) != char.IsUpper(ori))) return true;
        return false;
    }

    public List<Move> GetPawnMoves(Colour colour, int sq, char[] board) {
        int[] distances = GetDistance(sq);
        List<Move> moves = new List<Move>();
        if (colour == Colour.White) {
            if (distances[1] > 0) {
                if (!char.IsLetter(board[sq + 8])) {
                    moves.Add(new Move(sq, sq + 8));
                    if (distances[0] == 1 && !char.IsLetter(board[sq + 16])) {
                        moves.Add(new Move(sq, sq + 16, 3));
                    }
                }
            }
            if (distances[6] > 0) {
                if (CheckEnemy(sq + 9, board)) moves.Add(new Move(sq, sq + 9));
            }
            if (distances[7] > 0) {
                if (CheckEnemy(sq + 7, board)) moves.Add(new Move(sq, sq + 7));
            }
        } else {
            if (distances[0] > 0) {
                if (!char.IsLetter(board[sq - 8])) {
                    moves.Add(new Move(sq, sq - 8));
                    if (distances[1] == 1 && !char.IsLetter(board[sq - 16])) {
                        moves.Add(new Move(sq, sq - 16, 3));
                    }
                }
            }
            if (distances[4] > 0) {
                if (CheckEnemy(sq - 7, board)) moves.Add(new Move(sq, sq - 7));
            }
            if (distances[5] > 0) {
                if (CheckEnemy(sq - 9, board)) moves.Add(new Move(sq, sq - 9));
            }
        }
        return moves;
    }
    
    public List<Move> GetKnightMoves(int sq, char[] board) {
        int[] distances = GetDistance(sq);
        List<int> m = new List<int>();
        if (distances[0] >= 2) {
            if (distances[2] >= 1 && CheckNotFriendly(sq - 15, board)) m.Add(sq - 15);
            if (distances[3] >= 1 && CheckNotFriendly(sq - 17, board)) m.Add(sq - 17);
        }
        if (distances[1] >= 2) {
            if (distances[2] >= 1 && CheckNotFriendly(sq + 17, board)) m.Add(sq + 17);
            if (distances[3] >= 1 && CheckNotFriendly(sq + 15, board)) m.Add(sq + 15);
        }
        if (distances[3] >= 2) {
            if (distances[0] >= 1 && CheckNotFriendly(sq - 10, board)) m.Add(sq - 10);
            if (distances[1] >= 1 && CheckNotFriendly(sq + 6, board)) m.Add(sq + 6);
        }
        if (distances[4] >= 2) {
            if (distances[0] >= 1 && CheckNotFriendly(sq - 6, board)) m.Add(sq - 6);
            if (distances[1] >= 1 && CheckNotFriendly(sq + 10, board)) m.Add(sq + 10);
        }

        List<Move> moves = new List<Move>();
        foreach (int destination in m) {
            moves.Add(new Move(sq, destination));
        }
        return moves;
    }

    public List<Move> GetRookMoves(int sq, char[] board) {
        int[] distances = GetDistance(sq);
        List<int> destinations = new List<int>();
        for (int i = 1; i < distances[0] + 1; i++) {
            if (char.IsLetter(board[sq - (8 * i)])) {
                if (CheckNotFriendly(sq - (8 * i), board)) destinations.Add(sq - (8 * i));
                break;
            } else {
                destinations.Add(sq - (8 * i));
            }
        }
        for (int i = 1; i < distances[1] + 1; i++) {
            if (char.IsLetter(board[sq + (8 * i)])) {
                if (CheckNotFriendly(sq + (8 * i), board)) destinations.Add(sq + (8 * i));
                break;
            } else {
                destinations.Add(sq + (8 * i));
            }
        }
        for (int i = 1; i < distances[2] + 1; i++) {
            if (char.IsLetter(board[sq + i])) {
                if (CheckNotFriendly(sq + i, board)) destinations.Add(sq + i);
                break;
            } else {
                destinations.Add(sq + i);
            }
        }
        for (int i = 1; i < distances[3] + 1; i++) {
            if (char.IsLetter(board[sq - i])) {
                if (CheckNotFriendly(sq - i, board)) destinations.Add(sq - i);
                break;
            } else {
                destinations.Add(sq - i);
            }
        }
        List<Move> moves = new List<Move>();
        foreach (int destination in destinations) {
            moves.Add(new Move(sq, destination));
        }
        return moves;
    }

    public List<Move> GetBishopMoves(int sq, char[] board) {
        int[] distances = GetDistance(sq);
        List<int> destinations = new List<int>();
        for (int i = 1; i < distances[4] + 1; i++) {
            if (char.IsLetter(board[sq - (7 * i)])) {
                if (CheckNotFriendly(sq - (7 * i), board)) destinations.Add(sq - (7 * i));
                break;
            } else {
                destinations.Add(sq - (7 * i));
            }
        }
        for (int i = 1; i < distances[5] + 1; i++) {
            if (char.IsLetter(board[sq - (9 * i)])) {
                if (CheckNotFriendly(sq - (9 * i), board)) destinations.Add(sq - (9 * i));
                break;
            } else {
                destinations.Add(sq - (9 * i));
            }
        }
        for (int i = 1; i < distances[6] + 1; i++) {
            if (char.IsLetter(board[sq + (9 * i)])) {
                if (CheckNotFriendly(sq + (9 * i), board)) destinations.Add(sq + (9 * i));
                break;
            } else {
                destinations.Add(sq + (9 * i));
            }
        }
        for (int i = 1; i < distances[7] + 1; i++) {
            if (char.IsLetter(board[sq + (7 * i)])) {
                if (CheckNotFriendly(sq + (7 * i), board)) destinations.Add(sq + (7 * i));
                break;
            } else {
                destinations.Add(sq + (7 * i));
            }
        }
        List<Move> moves = new List<Move>();
        foreach (int destination in destinations) {
            moves.Add(new Move(sq, destination));
        }
        return moves;
    }

    public List<Move> GetQueenMoves(int t, char[] board) {
        List<Move> moves = GetRookMoves(t, board);
        List<Move> temp = GetBishopMoves(t, board);
        if (temp != null) {
            foreach (Move move in temp) {
                moves.Add(move);
            }
        }
        return moves;
    }

    public List<Move> GetKingMoves(Colour colour, int sq, char[] b) {
        int[] distances = GetDistance(sq);
        List<Move> moves = new List<Move>();
        if (distances[0] > 0 && CheckNotFriendly(sq - 8, b)) moves.Add(new Move(sq, sq - 8));
        if (distances[1] > 0 && CheckNotFriendly(sq + 8, b)) moves.Add(new Move(sq, sq + 8));
        if (distances[2] > 0 && CheckNotFriendly(sq + 1, b)) moves.Add(new Move(sq, sq + 1));
        if (distances[3] > 0 && CheckNotFriendly(sq - 1, b)) moves.Add(new Move(sq, sq - 1));
        if (distances[4] > 0 && CheckNotFriendly(sq - 7, b)) moves.Add(new Move(sq, sq - 7));
        if (distances[5] > 0 && CheckNotFriendly(sq - 9, b)) moves.Add(new Move(sq, sq - 9));
        if (distances[6] > 0 && CheckNotFriendly(sq + 9, b)) moves.Add(new Move(sq, sq + 9));
        if (distances[7] > 0 && CheckNotFriendly(sq + 7, b)) moves.Add(new Move(sq, sq + 7));
        //Castling logic
        
        bool[] c = board.Castling;
        if (colour == Colour.White) {
            if (c[0] && !char.IsLetter(b[5]) && !char.IsLetter(b[6])) {
                
                moves.Add(new Move(sq, 6, 2));
            }
            if (c[1] && !char.IsLetter(b[3]) && !char.IsLetter(b[2]) && !char.IsLetter(b[1])) {
                moves.Add(new Move(sq, 2, 2));
            }
        } else {
            if (c[2] && !char.IsLetter(b[61]) && !char.IsLetter(b[62])) {
                moves.Add(new Move(sq, 62, 2));

            }
            if (c[3] && !char.IsLetter(b[59]) && !char.IsLetter(b[58]) && !char.IsLetter(b[57])) {
                moves.Add(new Move(sq, 58, 2));
            }

        }
        return moves;
    }

}
