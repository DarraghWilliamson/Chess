using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ArtificialPlayer {
    Colour AiColour;
    PieceLogic pieceLogic;
    GameLogic gameLogic;
    GameDisplay gameDisplay;
    System.Random rand = new System.Random();

    public ArtificialPlayer(Colour colour) {
        AiColour = colour;
        pieceLogic = PieceLogic.instance;
        gameLogic = GameLogic.instance;
        gameDisplay = GameDisplay.instance;
    }

    public void TakeTurn() {
        Dictionary<int, List<int>> moves = gameLogic.possableMoves;
        MoveRandom(moves);
    }
    
    public void MoveRandom(Dictionary<int, List<int>> moves) {
        List<int> keys = new List<int>(moves.Keys);
        int from = keys[rand.Next(keys.Count)];
        List<int> options = moves[from];
        int to = options[rand.Next(options.Count)];
        gameLogic.MovePeice(from, to);
    }

    


}
