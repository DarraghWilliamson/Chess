using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameLogic {

    public static GameLogic instance;
    public Colour turn = Colour.White;
    public Colour playerColour = Colour.White;
    public char[] board;
    public bool[] Castling;
    public int Enpassant, halfMove, fullMove;
    public bool En, check, checkmate;
    public Dictionary<int, List<int>> possableMoves;
    ArtificialPlayer artificialPlayer;


    GameDisplay gameDisplay;


    public delegate void OnTurnEnd();
    public OnTurnEnd onTurnEnd;

    public delegate void OnCheck();
    public OnCheck onCheck;

    public GameLogic() { instance = this; }

    public void Setup(char[] _board, Colour _turn, Colour _playerColour, bool[] _castling, int _Enpassant, bool _en, int _halfMove, int _fullMove, ArtificialPlayer _artificialPlayer) {
        board = _board;
        turn = _turn;
        playerColour = _playerColour;
        Castling = _castling;
        Enpassant = _Enpassant;
        En = _en;
        halfMove = _halfMove;
        fullMove = _halfMove;
        artificialPlayer = _artificialPlayer;
        gameDisplay = GameDisplay.instance;
    }



    public Colour GetTurn() {
        return turn;
    }

    public void MovePeice(int from, int to) {
        //set Enpassant when pawn moved 2 squares
        if (char.ToLower(board[from]) == 'p' && Mathf.Abs(from - to) == 16) {
            int i = from - to == 16 ? -8 : 8;
            gameDisplay.MovePiece(gameDisplay.tiles[from], gameDisplay.tiles[to]);
            board[to] = board[from];
            board[from] = '\0';
            EndTurn(from + i, char.IsUpper(board[from]));
            return;
        }
        //Castling
        if (char.ToLower(board[from]) == 'k') {
            if (Mathf.Abs(from - to) == 2) {
                if (to == 62) {
                    MovePeice(63, 61);
                    gameDisplay.MoveObjectAI(63, 61);
                }
                if (to == 58) {
                    MovePeice(56, 59);
                    gameDisplay.MoveObjectAI(56, 59);
                }
                if (to == 6) {
                    MovePeice(7, 5);
                    gameDisplay.MoveObjectAI(7, 5);
                }
                if (to == 2) {
                    MovePeice(0, 3);
                    gameDisplay.MoveObjectAI(0, 3);
                }
            }
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
                gameDisplay.tiles[to + 8].piece.Die();
            } else {
                board[to - 8] = '\0';
                gameDisplay.tiles[to - 8].piece.Die();
            }
        }
        gameDisplay.MovePiece(gameDisplay.tiles[from], gameDisplay.tiles[to]);
        board[to] = board[from];
        board[from] = '\0';
        EndTurn();
    }

    public void EndTurn(int Enp, bool up) {
        if (En) {
            if (char.ToLower(board[Enpassant]) == 'e') board[Enpassant] = '\0';
        }
        if (up) {
            board[Enp] = 'E';
        } else {
            board[Enp] = 'e';
        }

        En = true;
        Enpassant = Enp;

        if (turn == 0) turn = (Colour)1; else turn = (Colour)0;
        onTurnEnd?.Invoke();
        StartTurn();
    }

    public void EndTurn() {
        if (En) {
            En = false;
            if (char.ToLower(board[Enpassant]) == 'e') board[Enpassant] = '\0';
        }
        if (turn == 0) turn = (Colour)1; else turn = (Colour)0;
        onTurnEnd?.Invoke();
        StartTurn();
    }

    //return true if sideColour is in check
    bool CheckCheck(Colour sideColour, char[] b) {
        char king = sideColour == Colour.White ? 'K' : 'k';
        int kingPos = Array.IndexOf(b, king);
        Colour enemy = sideColour == Colour.White ? Colour.Black : Colour.White;
        Dictionary<int, List<int>> temp = PieceLogic.instance.GetAllMoves(enemy, b);
        foreach (KeyValuePair<int, List<int>> x in temp) {
                foreach (int m in x.Value) {
                    if (m == kingPos) return true;
                }
        }
        return false;
    }

    //return true if sideColour is in checkmate
    bool CheckCheckmate(Colour colour, char[] b) {
        Dictionary<int, List<int>> attemptedMoves = GetAllPossible(colour, b);
        foreach (KeyValuePair<int, List<int>> piece in attemptedMoves) {
            foreach (int move in piece.Value) {
                char[] boardClone = (char[])b.Clone();
                boardClone[move] = boardClone[piece.Key];
                boardClone[piece.Key] = '\0';
                //if in any of those moves check was escaped, not checkmate
                if (CheckCheck(colour, boardClone) == false) {
                    return false;
                }
            }
        }
        return true;
    }

    //get all moves that dont place you in check if made
    public Dictionary<int, List<int>> GetAllPossible(Colour colour, char[] b) {
        Dictionary<int, List<int>> moves = new Dictionary<int, List<int>>();
        Dictionary<int, List<int>> temp = PieceLogic.instance.GetAllMoves(turn, b);

        foreach (KeyValuePair<int, List<int>> piece in temp) {
            foreach (int move in piece.Value) {
                char[] boardClone = (char[])b.Clone();
                boardClone[move] = boardClone[piece.Key];
                boardClone[piece.Key] = '\0';
                //if they dont put you in check, add
                Colour enemy = colour == Colour.White ? Colour.Black : Colour.White;
                if (!CheckCheck(colour, boardClone)) {
                    if (moves.ContainsKey(piece.Key)) {
                        moves[piece.Key].Add(move);
                    } else {
                        moves.Add(piece.Key, new List<int>() { move });
                    }
                }
            }
        }
        return moves;
    }

        public void StartTurn() {
        possableMoves = null;
        //check if in check, if true, check checkmate
        check = false;
        if (CheckCheck(turn, board)) {
            if (CheckCheckmate(turn, board)) {
                checkmate = true;
                onCheck?.Invoke();
            } else {
                check = true;
                onCheck?.Invoke();
            }
        } else {
            check = false;
            onCheck?.Invoke();
        }
        possableMoves = GetAllPossible(turn, board);
        if (turn != playerColour) artificialPlayer.TakeTurn();
    }

    public bool MyTurn() {
        return playerColour == turn;
    }

    public Colour GetTeam() {
        return playerColour;
    }

    public void ChangeTeam() {
        if (playerColour == 0) playerColour = (Colour)1; else playerColour = (Colour)0;
    }

}
