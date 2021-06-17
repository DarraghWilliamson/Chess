using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Board {
    public const int WhiteIn = 0;
    public const int BlackIn = 1;

    public char[] board;
    public bool[] Castling;

    public bool isWhitesMove;
    public int playerColour;
    public int turnColour;

    public bool En;
    public int Enpassant;
    public GameLogic gameLogic;

    public void ChangeTurn() {
        isWhitesMove = !isWhitesMove;
        turnColour = turnColour == 0 ? 1 : 0;
    }

    public void MovePeiceCheck(Move move) {
        int from = move.StartSquare;
        int to = move.EndSquare;

        //set Enpassant when pawn moved 2 squares
        if (char.ToLower(board[from]) == 'p' && Mathf.Abs(from - to) == 16) {
            int i = from - to == 16 ? -8 : 8;
            //if enpass last turn remove
            if (En) if (char.ToLower(board[Enpassant]) == 'e') board[Enpassant] = '\0';
            //set enpass
            if (char.IsUpper(board[from])) {
                board[from + i] = 'E';
            } else {
                board[from + i] = 'e';
            }
            En = true;
            Enpassant = from + i;
        } else {
            if (En) {
                En = false;
                if (char.ToLower(board[Enpassant]) == 'e') board[Enpassant] = '\0';
            }
        }
        //Castling
        if (move.IsCastle) {
            if (to == 62) ActualMove(63, 61);
            if (to == 58) ActualMove(56, 59);
            if (to == 6) ActualMove(7, 5);
            if (to == 2) ActualMove(0, 3);
        }
        //set castling flase when king moves
        if (char.ToLower(board[from]) == 'k') {
            if (char.IsUpper(board[from])) {
                Castling[0] = false;
                Castling[1] = false;
            } else {
                Castling[2] = false;
                Castling[3] = false;
            }
        }
        //set castling to false when rook moves
        if (char.ToLower(board[from]) == 'r') {
            if (from == 63) Castling[0] = false;
            if (from == 56) Castling[1] = false;
            if (from == 7) Castling[2] = false;
            if (from == 0) Castling[3] = false;
        }
        //kill pawn when Enpassant tile is taken
        if (char.ToLower(board[to]) == 'e') {
            if (to >= 16 && to <= 23) {
                board[to + 8] = '\0';
                //if (show) gameDisplay.tiles[to + 8].piece.Die();
            } else {
                board[to - 8] = '\0';
                //if (show) gameDisplay.tiles[to - 8].piece.Die();
            }
        }
        //if (show) Log(from, to);
        ActualMove(from, to);
        gameLogic.EndTurn();
    }

    void ActualMove(int from, int to) {
        board[to] = board[from];
        board[from] = '\0';
    }
    
    //creates a clone board where a move was made
    char[] GenBoard(int from, int to, char[] b) {
        char[] boardClone = (char[])b.Clone();
        boardClone[to] = boardClone[from];
        boardClone[from] = '\0';
        return boardClone;
    }

    public void MoveTest(int ina) {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        int moves = MoveGenTest(ina, board);
        watch.Stop();
        MonoBehaviour.print(moves + " in " + watch.ElapsedMilliseconds + "ms");
    }
    int MoveGenTest(int depth, char[] b) {
        if (depth == 0) return 1;
        List<Move> allMoves = GetLegalMoves(turnColour, board);

        int numPos = 0;
        foreach (Move m in allMoves) {
            //GameState s = new GameState(board, turn, Castling, Enpassant, En);
            //MovePeiceCheck(m);
            numPos += MoveGenTest(depth - 1, GenBoard(m.StartSquare, m.EndSquare, b));
            //RestoreState(s);
        }
        return numPos;
    }


    public bool CheckCheck(int colour, char[] b) {
        char king = colour == 0 ? 'K' : 'k';
        int kingPos = Array.IndexOf(b, king);
        int enemy = colour == 0 ? 1 : 0;
        List<Move> temp = GetSideMoves(enemy, b);
        foreach (Move x in temp) {
            if (x.EndSquare == kingPos) return true;
        }
        return false;
    }

    //return true if sideColour is in checkmate
    public bool CheckCheckmate(int colour, char[] b) {
        List<Move> attemptedMoves = GetLegalMoves(colour, b);
        if (attemptedMoves == null || attemptedMoves.Count == 0) {
            if (attemptedMoves == null) {
                MonoBehaviour.print("moves null");
            } else {
                MonoBehaviour.print(attemptedMoves.Count);
            }
            return true;
        } else {
            return false;
        }
    }

    //get all moves that dont place you in check if made
    public List<Move> GetLegalMoves(int colour, char[] b) {
        List<Move> allowedmoves = new List<Move>();
        List<Move> allMoves = GetSideMoves(colour, b);
        foreach (Move move in allMoves) {
            char[] boardClone = GenBoard(move.StartSquare, move.EndSquare, board);
            //if they dont put you in check, add
            if (!CheckCheck(turnColour, boardClone)) {
                allowedmoves.Add(move);
            }
        }
        return allowedmoves;
    }

    public List<Move> GetLegalMove(int c) {
        List<Move> allowedmoves = new List<Move>();
        List<Move> allMoves = PieceLogic.instance.GetMoves(c);
        if (allMoves == null) return null;
        foreach (Move move in allMoves) {
            char[] boardClone = GenBoard(move.StartSquare, move.EndSquare, board);
            //if they dont put you in check, add
            if (!CheckCheck(turnColour, boardClone)) {
                allowedmoves.Add(move);
            }
        }
        return allowedmoves;
    }

    //gets moves for colour, not filtered
    public List<Move> GetSideMoves(int colour, char[] board) {
        bool col = colour == 0 ? true : false;
        List<Move> moves = new List<Move>();
        for (int i = 0; i < 64; i++) {
            if (char.IsLetter(board[i]) && (char.IsUpper(board[i]) == col)) {
                List<Move> temp = PieceLogic.instance.GetMoves(i);
                if (temp != null) {
                    foreach (Move move in temp) {
                        moves.Add(move);
                    }
                }
            }
        }
        return moves;
    }


    void Log(int from, int to) {
        string notation = "";
        if (char.ToLower(board[from]) != 'p') {
            notation += board[from];
        } else {
            if (char.IsLetter(board[to])) {
                notation += GetBoardRep(from)[0];
            }
        }
        if (char.IsLetter(board[to])) {
            notation += "x";
        }
        notation += GetBoardRep(to);
        if (char.ToLower(board[from]) == 'k' && Math.Abs(from - to) == 2) {
            if (to == 62 || to == 6) {
                notation = "0-0";
            } else {
                notation = "0-0-0";
            }
        }
        Debug.Log(notation);
    }
    string GetBoardRep(int sq) {
        int rank = (sq / 8) + 1;
        int t = sq % 8;
        char file = (char)(t + 65);
        string s = file + "" + rank;
        return s;
    }
}
