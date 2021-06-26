using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class Zobrist {
    static int seed = 2846602;
    static System.Random random = new System.Random(seed);

    public static ulong[,,] zPieces = new ulong[2,8,64];
    public static ulong[] zEnpassant = new ulong[8];
    public static ulong[] zCastling = new ulong[4];
    public static ulong zTurnColour;

    public static void FillzProperties() {
        for(int col = 0; col < 2; col++) {
            for(int type = 0; type < 8; type++) {
                for (int squares = 0; squares < 64; squares++) {
                    zPieces[col,type,squares] = MakeRandom64Bit();
                }
            }
        }
        for (int Enp = 0; Enp < 8; Enp++) {
            zEnpassant[Enp] = MakeRandom64Bit();
        }
        for (int cast = 0; cast < 4; cast++) {
            zCastling[cast] = MakeRandom64Bit();
        }
        zTurnColour = MakeRandom64Bit();
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
            if (board.Enpassant != 99) {
                zKey ^= zEnpassant[board.Enpassant % 8];
            }
            
            for(int i = 0; i < 4; i++) {
                if (!board.Castling[i]) zKey ^= zCastling[i];
            }
        if(board.turnColour == 1) {
                zKey ^= zTurnColour;
            }

        

        return zKey;
    }


    public static ulong MakeRandom64Bit() {
        byte[] bytes = new byte[8];
        random.NextBytes(bytes);
        return System.BitConverter.ToUInt64(bytes, 0);
    }




}
