using System;
using System.Collections.Generic;

public class ArtificialPlayer {
    private int AiColour;
    private GameLogic gameLogic;
    private System.Random rand = new System.Random();

    public ArtificialPlayer(int colour, GameLogic gameLogic) {
        AiColour = colour;
        this.gameLogic = gameLogic;
    }

    public void TakeTurn() {
        List<int> moves = gameLogic.board.GenerateMoves();
        if (moves.Count == 0) {
            Console.Write("AI has no moves");
            return;
        }
        MoveRandom(moves);
    }

    public void MoveRandom(List<int> moves) {
        int a = rand.Next(moves.Count);
        gameLogic.board.MovePiece(moves[a]);
    }
}