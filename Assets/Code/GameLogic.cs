using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GameLogic {

    public static GameLogic instance;
    public int playerColour = 0;

    public int halfMove, fullMove;
    public bool check, checkmate, AiOn;
    public List<Move> possableMoves;
    ArtificialPlayer artificialPlayer;
    public GameDisplay gameDisplay;
    public bool show = true;
    public Board board;

    public delegate void OnTurnEnd();
    public OnTurnEnd onTurnEnd;

    public delegate void OnCheck();
    public OnCheck onCheck;

    public GameLogic() { instance = this; }

    public void Setup(Board _board, int _playerColour, int _halfMove, int _fullMove, ArtificialPlayer _artificialPlayer, bool _AiOn) {
        board = _board;
        playerColour = _playerColour;
        halfMove = _halfMove;
        fullMove = _halfMove;
        artificialPlayer = _artificialPlayer;
        gameDisplay = GameDisplay.instance;
        AiOn = _AiOn;

    }

    

    public void StartTurn() {
        possableMoves = null;
        //check if in check, if true, check checkmate
        check = false;
        //check code?
        possableMoves = board.GeneratMoves();

        if (board.turnColour != playerColour && AiOn == true) {
            artificialPlayer.TakeTurn();
        }
    }


    public void EndTurn(Move move) {
        if (show) gameDisplay.UpdateDisplay(move);
        if (show) Log(move);
        if (show) onTurnEnd?.Invoke();
        StartTurn();
    }
    public void EndTurn() {
        onTurnEnd?.Invoke();
        StartTurn();
    }

    //converts current board to FEN format and prints
    public void ExportFen() {
        //Test();
        //board.MoveTest(1);
        //board.MoveTest(2);
        board.MoveTest(3);
        board.MoveTest(4);

        string FEN = "";
        
        int emptyCount = 0;
        int[] squares = board.squares;
        for(int file = 7;file >= 0; file--) {
            emptyCount = 0;
            for(int rank = 0; rank < 8; rank++) {
                int i = file * 8 + rank;
                if (squares[i] == 0) {
                    emptyCount++;
                } else {
                    if (emptyCount != 0) {
                        FEN += emptyCount.ToString();
                        emptyCount = 0;
                    }
                    char pieceChar = ' ';
                    int pieceType = Piece.Type(squares[i]);
                    switch (pieceType) {
                        case Piece.King: pieceChar = 'k'; break;
                        case Piece.Pawn: pieceChar = 'p'; break;
                        case Piece.Rook: pieceChar = 'r'; break;
                        case Piece.Knight: pieceChar = 'n'; break;
                        case Piece.Bishop: pieceChar = 'b'; break;
                        case Piece.Queen: pieceChar = 'q'; break;
                    }
                    if (Piece.IsColour(squares[i], Piece.White)) {
                        FEN += char.ToUpper(pieceChar);
                    } else {
                        FEN += pieceChar;
                    }
                }
            }
            if(emptyCount!=0) {
                FEN += emptyCount.ToString();
            }
            if (file != 0) FEN += '/';
        }
        //turn
        FEN += board.turnColour == 0 ? " w " : " b ";
        //castling
        string castling = "";
        castling += board.Castling[0] ? "K" : "";
        castling += board.Castling[1] ? "Q" : "";
        castling += board.Castling[2] ? "k" : "";
        castling += board.Castling[3] ? "q" : "";
        if (castling == "") {
            FEN += "- ";
        } else {
            FEN += castling + " ";
        }
        //Enpassant
        if (board.Enpassant != 99) {
            FEN += GetBoardRep(board.Enpassant) + " ";
        } else {
            FEN += "- ";
        }
        //moves
        FEN += "0 ";
        FEN += "0 ";
        MonoBehaviour.print(FEN);
    }

    public void ToggleAi() {
        AiOn = AiOn ? false : true;
    }

    public bool MyTurn() {
        return playerColour == board.turnColour;
    }

    public int GetTeam() {
        return playerColour;
    }

    public void ChangeTeam() {
        if (playerColour == 0) playerColour = 1; else playerColour = 0;
    }

    void Log(Move move) {
        int from = move.StartSquare;
        int to = move.EndSquare;
        int[] squares = board.squares;
        string notation = "";
        if (Piece.Type(squares[from]) != Piece.Pawn) {
            notation += squares[from];
        } else {
            if (squares[to] != 0) {
                notation += GetBoardRep(from)[0];
            }
        }
        if (squares[to] != 0) {
            notation += "x";
        }
        notation += GetBoardRep(to);
        if (Piece.Type(squares[from]) != Piece.King && Math.Abs(from - to) == 2) {
            if (to == 62 || to == 6) {
                notation = "0-0";
            } else {
                notation = "0-0-0";
            }
        }
        //Debug.Log(notation);
    }
    string GetBoardRep(int sq) {
        int rank = (sq / 8) + 1;
        int t = sq % 8;
        char file = (char)(t + 65);
        string s = file + "" + rank;
        return s;
    }

}