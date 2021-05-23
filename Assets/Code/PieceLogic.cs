using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PieceLogic {
    GameLogic gameLogic;
    public static PieceLogic instance;
    char ori;

    public PieceLogic() {
        instance = this;
        gameLogic = GameLogic.instance;
    }

    public Dictionary<int, List<int>> GetMoves(int i, Colour colour) {
        return GetMovesMethod(i, colour, gameLogic.board);
    }
    public Dictionary<int, List<int>> GetMoves(int i, Colour colour, char[] b) {
        return GetMovesMethod(i, colour, b);
    }
    
    public Dictionary<int, List<int>> GetMovesMethod(int i, Colour colour, char[] board) {
        Dictionary<int, List<int>> moves = null;
        ori = board[i];
        if (char.ToLower(ori) == 'p') moves = GetPawnMoves(colour, i, board);
        if (char.ToLower(ori) == 'n') moves = GetKnightMoves(i, board);
        if (char.ToLower(ori) == 'r') moves = GetRookMoves(i, board);
        if (char.ToLower(ori) == 'b') moves = GetBishopMoves(i, board);
        if (char.ToLower(ori) == 'q') moves = GetQueenMoves(i, board);
        if (char.ToLower(ori) == 'k') moves = GetKingMoves(colour, i, board);
        //if in check disallow any moves that dont get you out of check
        if (gameLogic.check) {
            Dictionary<int, List<int>> allowedMoves = gameLogic.possableMoves;
            if (moves == null) return moves;
            foreach (KeyValuePair<int, List<int>> move in moves) {
                if (!allowedMoves.ContainsKey(move.Key)){
                    if (moves.Keys.Count == 1) {
                        return null;
                    } else {
                        moves.Remove(move.Key);
                    }
                } else {
                    moves[move.Key].Clear();
                    foreach(int x in allowedMoves[move.Key]) {
                        moves[move.Key].Add(x);
                    }
                }
            }
        }
        return moves;
    }

    public Dictionary<int, List<int>> GetAllMoves(Colour colour, char[] board) {
        Dictionary<int, List<int>> moves = new Dictionary<int, List<int>>();
        Dictionary<int, List<int>> temp;
        int i = 0;
        foreach (char c in board) {
            if (char.IsUpper(c) == (colour == Colour.White)) {
                temp = GetMoves(i, colour, board);
                if (temp == null) continue;
                foreach (KeyValuePair<int, List<int>> x in temp) {
                    if (x.Value.Count > 0) {
                        moves.Add(x.Key, x.Value);
                    }
                }
            }
            i++;
        }
        return moves;
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

    bool CheckNotFriendly(int i, char[] board) {
        char t = board[i];
        if (!char.IsLetter(t) || (char.IsUpper(t) != char.IsUpper(ori))) return true;
        return false;
    }

    

    public Dictionary<int, List<int>> GetPawnMoves(Colour colour, int i, char[] board) {
        int[] distances = GetDistance(i);
        List<int> m = new List<int>();
        if (colour == Colour.Black) {
            if (distances[1] > 0) {
                if (!char.IsLetter(board[i + 8])) {
                    m.Add(i + 8);
                    if (distances[0] == 1 && !char.IsLetter(board[i + 16])) {
                        m.Add(i + 16);
                    }
                }
            }
            if (distances[6] > 0) {
                if (CheckEnemy(i + 9, board)) m.Add(i + 9);
            }
            if (distances[7] > 0) {
                if (CheckEnemy(i + 7, board)) m.Add(i + 9);
            }
        } else {
            if (distances[0] > 0) {
                if (!char.IsLetter(board[i - 8])) {
                    m.Add(i - 8);
                    if (distances[1] == 1 && !char.IsLetter(board[i - 16])) {
                        m.Add(i - 16);
                    }
                }
            }
            if (distances[4] > 0) {
                if (CheckEnemy(i - 7, board)) m.Add(i - 7);
            }
            if (distances[5] > 0) {
                if (CheckEnemy(i - 9, board)) m.Add(i - 9);
            }
        }
        Dictionary<int, List<int>> moves = new Dictionary<int, List<int>>() { { i, m } };
        return moves;
    }

    public Dictionary<int, List<int>> GetKnightMoves(int i, char[] board) {
        int[] distances = GetDistance(i);
        List<int> m = new List<int>();
        if (distances[0] >= 2) {
            if (distances[2] >= 1 && CheckNotFriendly(i - 15, board)) m.Add(i - 15);
            if (distances[3] >= 1 && CheckNotFriendly(i - 17, board)) m.Add(i - 17);
        }
        if (distances[1] >= 2) {
            if (distances[2] >= 1 && CheckNotFriendly(i + 17, board)) m.Add(i + 17);
            if (distances[3] >= 1 && CheckNotFriendly(i + 15, board)) m.Add(i + 15);
        }
        if (distances[3] >= 2) {
            if (distances[0] >= 1 && CheckNotFriendly(i - 10, board)) m.Add(i - 10);
            if (distances[1] >= 1 && CheckNotFriendly(i + 6, board)) m.Add(i + 6);
        }
        if (distances[4] >= 2) {
            if (distances[0] >= 1 && CheckNotFriendly(i - 6, board)) m.Add(i - 6);
            if (distances[1] >= 1 && CheckNotFriendly(i + 10, board)) m.Add(i + 10);
        }
        Dictionary<int, List<int>> moves = new Dictionary<int, List<int>>() { { i, m } };
        return moves;
    }

    public Dictionary<int, List<int>> GetRookMoves(int t, char[] board) {
        int[] distances = GetDistance(t);
        List<int> m = new List<int>();
        for (int i = 1; i < distances[0] + 1; i++) {
            if (char.IsLetter(board[t - (8 * i)])) {
                if (CheckNotFriendly(t - (8 * i), board)) m.Add(t - (8 * i));
                break;
            } else {
                m.Add(t - (8 * i));
            }
        }
        for (int i = 1; i < distances[1] + 1; i++) {
            if (char.IsLetter(board[t + (8 * i)])) {
                if (CheckNotFriendly(t + (8 * i), board)) m.Add(t + (8 * i));
                break;
            } else {
                m.Add(t + (8 * i));
            }
        }
        for (int i = 1; i < distances[2] + 1; i++) {
            if (char.IsLetter(board[t + i])) {
                if (CheckNotFriendly(t + i, board)) m.Add(t + i);
                break;
            } else {
                m.Add(t + i);
            }
        }
        for (int i = 1; i < distances[3] + 1; i++) {
            if (char.IsLetter(board[t - i])) {
                if (CheckNotFriendly(t - i, board)) m.Add(t - i);
                break;
            } else {
                m.Add(t - i);
            }
        }
        Dictionary<int, List<int>> moves = new Dictionary<int, List<int>>() { { t, m } };
        return moves;
    }

    public Dictionary<int, List<int>> GetBishopMoves(int t, char[] board) {
        int[] distances = GetDistance(t);
        List<int> m = new List<int>();
        for (int i = 1; i < distances[4] + 1; i++) {
            if (char.IsLetter(board[t - (7 * i)])) {
                if (CheckNotFriendly(t - (7 * i), board)) m.Add(t - (7 * i));
                break;
            } else {
                m.Add(t - (7 * i));
            }
        }
        for (int i = 1; i < distances[5] + 1; i++) {
            if (char.IsLetter(board[t - (9 * i)])) {
                if (CheckNotFriendly(t - (9 * i), board)) m.Add(t - (9 * i));
                break;
            } else {
                m.Add(t - (9 * i));
            }
        }
        for (int i = 1; i < distances[6] + 1; i++) {
            if (char.IsLetter(board[t + (9 * i)])) {
                if (CheckNotFriendly(t + (9 * i), board)) m.Add(t + (9 * i));
                break;
            } else {
                m.Add(t + (9 * i));
            }
        }
        for (int i = 1; i < distances[7] + 1; i++) {
            if (char.IsLetter(board[t + (7 * i)])) {
                if (CheckNotFriendly(t + (7 * i), board)) m.Add(t + (7 * i));
                break;
            } else {
                m.Add(t + (7 * i));
            }
        }
        Dictionary<int, List<int>> moves = new Dictionary<int, List<int>>() { { t, m } };
        return moves;
    }

    public Dictionary<int, List<int>> GetQueenMoves(int t, char[] board) {
        Dictionary<int, List<int>> moves = GetRookMoves(t, board);
        Dictionary<int, List<int>> temp = GetBishopMoves(t, board);
        if (temp != null) {
            foreach (KeyValuePair<int, List<int>> i in temp) {
                if (moves.ContainsKey(i.Key)) {
                    foreach (int x in i.Value) {
                        moves[i.Key].Add(x);
                    }
                } else {
                    moves.Add(i.Key, i.Value);
                }
            }
        }
        return moves;
    }

    public Dictionary<int, List<int>> GetKingMoves(Colour colour, int t, char[] board) {
        int[] distances = GetDistance(t);
        List<int> m = new List<int>();
        if (distances[0] > 0 && CheckNotFriendly(t - 8, board)) m.Add(t - 8);
        if (distances[1] > 0 && CheckNotFriendly(t + 8, board)) m.Add(t + 8);
        if (distances[2] > 0 && CheckNotFriendly(t + 1, board)) m.Add(t + 1);
        if (distances[3] > 0 && CheckNotFriendly(t - 1, board)) m.Add(t - 1);
        if (distances[4] > 0 && CheckNotFriendly(t - 7, board)) m.Add(t - 7);
        if (distances[5] > 0 && CheckNotFriendly(t - 9, board)) m.Add(t - 9);
        if (distances[6] > 0 && CheckNotFriendly(t + 9, board)) m.Add(t + 9);
        if (distances[7] > 0 && CheckNotFriendly(t + 7, board)) m.Add(t + 7);
        //Castling logic
        bool[] c = gameLogic.Castling;
        if(colour == Colour.White) {
            if (c[0]) {
                if(!char.IsLetter(board[61]) && !char.IsLetter(board[62])) {
                    m.Add(62);
                }
            }
            if (c[1]) {
                if (!char.IsLetter(board[59]) && !char.IsLetter(board[58]) && !char.IsLetter(board[57])) {
                    m.Add(58);
                }
            }
        } 


        Dictionary<int, List<int>> moves = new Dictionary<int, List<int>>() { { t, m } };
        return moves;
    }

}
