using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ArtificialPlayer {
    int AiColour;
    PieceLogic pieceLogic;
    GameLogic gameLogic;
    GameDisplay gameDisplay;
    System.Random rand = new System.Random();

    public ArtificialPlayer(int colour) {
        AiColour = colour;
        pieceLogic = PieceLogic.instance;
        gameLogic = GameLogic.instance;
        gameDisplay = GameDisplay.instance;
    }

    public void TakeTurn() {
        //Dictionary<int, List<int>> moves = gameLogic.possableMoves;
        List<Move> moves = gameLogic.board.GeneratMoves();
        if (moves.Count==0) {
            Debug.Log("AI has no moves");
            return;
        }
        MoveRandom(moves);
    }
    
    public void MoveRandom(List<Move> moves) {
        int a = rand.Next(moves.Count);
        gameLogic.board.MovePiece(moves[a]);
    }

    


}
