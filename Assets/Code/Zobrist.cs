using static Random;

public static class Zobrist {
    public static ulong[,,] zPieces = new ulong[2, 8, 64];
    public static ulong[] zEnpassant = new ulong[9];
    public static ulong[] zCastling = new ulong[16];
    public static ulong zTurnColour;

    public static void FillzProperties() {
        for (int col = 0; col < 2; col++) {
            for (int type = 0; type < 8; type++) {
                for (int squares = 0; squares < 64; squares++) {
                    zPieces[col, type, squares] = GetRandom64();
                }
            }
        }
        for (int Enp = 0; Enp < 9; Enp++) {
            zEnpassant[Enp] = GetRandom64();
        }
        for (int cast = 0; cast < 16; cast++) {
            zCastling[cast] = GetRandom64();
        }
        zTurnColour = GetRandom64();
    }

    public static ulong GetZobristHash(Board board) {
        ulong zKey = 0;

        for (int sq = 0; sq < 64; sq++) {
            if (board.squares[sq] != 0) {
                int type = Piece.Type(board.squares[sq]);
                int col = Piece.Colour(board.squares[sq]);
                zKey ^= zPieces[col, type, sq];
            }
        }

        int EnpassantFile = ((board.currentGameState >> 4) & 15);
        if (EnpassantFile != 0) {
            zKey ^= zEnpassant[EnpassantFile];
        }

        int castle = board.currentGameState & 15;
        zKey ^= zCastling[castle];

        if (board.turnColour == 1) {
            zKey ^= zTurnColour;
        }

        return zKey;
    }
}