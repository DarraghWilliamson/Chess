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
    GameDisplay gameDisplay;
    bool show = true;
    public Piece[] pieces;
    public Board board;

    public delegate void OnTurnEnd();
    public OnTurnEnd onTurnEnd;

    public delegate void OnCheck();
    public OnCheck onCheck;

    public GameLogic() { instance = this; }

    public void Setup(Board _board, Colour _turn, int _playerColour, int _halfMove, int _fullMove, ArtificialPlayer _artificialPlayer, bool _AiOn) {
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
        if (board.CheckCheck(board.turnColour, board.board)) {
            if (board.CheckCheckmate(board.turnColour, board.board)) {
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


        possableMoves = board.GetLegalMoves(board.turnColour, board.board);
        for (int i = 0; i < pieces.Length; i++) {
            pieces[i].moves = board.GetLegalMove(pieces[i].location);
        }

        if (board.turnColour != playerColour && AiOn == true) {
            artificialPlayer.TakeTurn();
        }
    }

    public void EndTurn() {
        //if(show) gameDisplay.MovePieceObject(from, to);
        board.ChangeTurn();
        onTurnEnd?.Invoke();
        StartTurn();
    }
    
    //converts current board to FEN format and prints
    public void ExportFen() {
        board.MoveTest(1);
        board.MoveTest(2);
        board.MoveTest(3);
        //MoveTest(4);

        string FEN = "";
        int rank = 0;
        int emptyCount = 0;
        //board
        foreach (char square in board.board) {
            if (rank == 8) {
                if (emptyCount != 0) {
                    FEN += emptyCount.ToString();
                    emptyCount = 0;
                }
                FEN += "/";
                rank = 0;
            }
            if (char.IsLetter(square)) {
                if (emptyCount != 0) {
                    FEN += emptyCount.ToString();
                }
                emptyCount = 0;
                FEN += square;
            } else {
                emptyCount++;
            }
            rank++;
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
        if (En) {
            FEN += Enpassant + " ";
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

}