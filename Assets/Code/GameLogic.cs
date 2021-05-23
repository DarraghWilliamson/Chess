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

    bool CheckCheck(Colour colour, char[] b) {
        char king = colour == Colour.White ? 'K' : 'k';
        int ind = Array.IndexOf(b, king);
        Colour enemy = colour == Colour.White ? Colour.Black : Colour.White;
        Dictionary<int, List<int>> temp = PieceLogic.instance.GetAllMoves(enemy, b);
        foreach (KeyValuePair<int, List<int>> x in temp) {
                foreach (int m in x.Value) {
                    if (m == ind) return true;
                }
        }
        return false;
    }

    Dictionary<int, List<int>> CheckCheckmate(Colour colour) {
        Dictionary<int, List<int>> secsessfulMoves = new Dictionary<int, List<int>>();
        //create array of boards where every available move was made
        Dictionary<int, List<int>> myMoves = PieceLogic.instance.GetAllMoves(colour, board);
        foreach (KeyValuePair<int, List<int>> piece in myMoves) {
            foreach(int move in piece.Value) {
                char[] boardClone = (char[])board.Clone();
                boardClone[move] = board[piece.Key];
                boardClone[piece.Key] = '\0';
                //see if in any of those boards check was escaped
                if (CheckCheck(colour, boardClone) != true) {
                    if (secsessfulMoves.ContainsKey(piece.Key)) {
                        secsessfulMoves[piece.Key].Add(move);
                    } else {
                        List<int> ina = new List<int>() { move };
                        secsessfulMoves.Add(piece.Key, ina);
                    }
                }
            }
        }
        return secsessfulMoves;
    }

    public Dictionary<int, List<int>> GetPossible(Colour colour) {
        //get all moves
        Dictionary<int, List<int>> Moves = PieceLogic.instance.GetAllMoves(turn, board);
        foreach (KeyValuePair<int, List<int>> piece in Moves) {
            List<int> temp = new List<int>();
            foreach (int move in piece.Value) {
                char[] boardClone = (char[])board.Clone();
                boardClone[move] = board[piece.Key];
                boardClone[piece.Key] = '\0';
                //if they put you in check, remove

                char king = colour == Colour.White ? 'K' : 'k';
                int ind = Array.IndexOf(boardClone, king);
                Colour enemy = colour == Colour.White ? Colour.Black : Colour.White;
                Dictionary<int, List<int>> temppp = PieceLogic.instance.GetAllMoves(enemy, boardClone);


                if (CheckCheck(turn, boardClone) == true) {
                    if (piece.Value.Count == 1) {
                        Moves.Remove(piece.Key);
                    } else {
                        temp.Add(move);
                    }
                }
            }
            if (temp.Count != 0) {
                foreach (int x in temp) {
                    piece.Value.Remove(x);
                }
            }
        }
        return Moves;
    }

        public void StartTurn() {
        possableMoves = null;
        //check if in check, if true, check checkmate
        check = false;
        if (CheckCheck(turn, board)) {
            possableMoves = CheckCheckmate(turn);
            if (possableMoves.Count == 0) {
                checkmate = true;
                onCheck?.Invoke();
            } else {
                check = true;
                onCheck?.Invoke();
            }
        } else {
            check = false;
            onCheck?.Invoke();
            possableMoves = GetPossible(turn);
        }
        
        //if (turn != playerColour) artificialPlayer.TakeTurn();
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
