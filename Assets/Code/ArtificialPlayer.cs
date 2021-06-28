using System.Collections;
using System.Collections.Generic;
using System;

public class ArtificialPlayer {
    int AiColour;
    GameLogic gameLogic;
    System.Random rand = new System.Random();

    public ArtificialPlayer(int colour, GameLogic gameLogic) {
        AiColour = colour;
        this.gameLogic = gameLogic;
    }

    public void TakeTurn() {
        //Dictionary<int, List<int>> moves = gameLogic.possableMoves;
        List<Move> moves = gameLogic.board.GenerateMoves();
        if (moves.Count==0) {
            Console.Write("AI has no moves");
            return;
        }
        MoveRandom(moves);
    }
    
    public void MoveRandom(List<Move> moves) {
        int a = rand.Next(moves.Count);
        gameLogic.board.MovePiece(moves[a]);
    }

    


}
