using System;
using System.Collections.Generic;

public static class PreComputedMoves {

    
    public static readonly int[][] distances;
    static readonly int[] knightOffsets = new int[] { 15, 17, -17, -15, -10, 6, -6, 10 };
    public static readonly int[] offset = new int[] { 8, -8, -1, 1, 7, 9, -9, -7 };
    public static readonly int[][] pawnAttackOffsets = { new int[] { 4, 6 }, new int[] { 7, 5 } };

    public static readonly int[][] pawnAttackWhite;
    public static readonly int[][] pawnAttackBlack;
    public static readonly ulong[][] pawnAttackBitboards;
    public static readonly ulong[] kingAttackBitboards;
    public static readonly ulong[] knightAttackBitboards;
    
    public static readonly byte[][] knightMoves;
    public static readonly byte[][] kingMoves;
    public static readonly ulong[] rookMoves;
    public static readonly ulong[] bishopMoves;
    public static readonly ulong[] queenMoves;

    

    static PreComputedMoves() {

        pawnAttackWhite = new int[64][];
        pawnAttackBlack = new int[64][];
        distances = new int[64][];
        knightMoves = new byte[64][];
        kingMoves = new byte[64][];

        rookMoves = new ulong[64];
        bishopMoves = new ulong[64];
        queenMoves = new ulong[64];
        knightAttackBitboards = new ulong[64];
        kingAttackBitboards = new ulong[64];
        pawnAttackBitboards = new ulong[64][];
        
        for (int sq = 0; sq < 64; sq++) {
            GetDistances(sq);
            GetKnightMoves(sq);
            GetKingMoves(sq);
            PawnAttacks(sq);
            RookMoves(sq);
            BishopMoves(sq);
            QueenMoves(sq);
        }
    
    }

    //north, south, east, west, north east, north west, south east, south west
    static void GetDistances(int sq) {
        int file = sq / 8;
        int rank = (sq + 8) % 8;
        int north = 7 - file;
        int south = file;
        int east = rank;
        int west = 7 - rank;
        distances[sq] = new int[8];
        distances[sq][0] = north;
        distances[sq][1] = south;
        distances[sq][2] = east;
        distances[sq][3] = west;
        distances[sq][4] = Math.Min(north, east);
        distances[sq][5] = Math.Min(north, west);
        distances[sq][6] = Math.Min(south, east);
        distances[sq][7] = Math.Min(south, west);
    }

    static void GetKnightMoves(int sq) {
        //probably better ways of doing this
        int[] c1 = new int[] { 0, 0, 1, 1, 2, 2, 3, 3 };
        int[] c2 = new int[] { 2, 3, 2, 3, 1, 0, 1, 0 };
        var legalMoves = new List<byte>();
        ulong knightBitboard = 0;
        for (int d = 0; d < 8; d++) {
            if (distances[sq][c1[d]] >= 2 && distances[sq][c2[d]] >= 1) {
                int knightMoveSq = sq + knightOffsets[d];
                legalMoves.Add((byte)knightMoveSq);
                knightBitboard |= 1ul << knightMoveSq;
            }
        }
        knightMoves[sq] = legalMoves.ToArray();
        knightAttackBitboards[sq] = knightBitboard;
    }

    static void GetKingMoves(int sq) {
        var legalMoves = new List<byte>();
        for (int d = 0; d < 8; d++) {
            if (distances[sq][d] >= 1) {
                int kingMoveSq = sq + offset[d];
                legalMoves.Add((byte)kingMoveSq);
                kingAttackBitboards[sq] |= 1ul << kingMoveSq;
            }
        }
        kingMoves[sq] = legalMoves.ToArray();
    }
    
    static void PawnAttacks(int sq) {
        List<int> legalAttackWhite = new List<int>();
        List<int> legalAttackBlack = new List<int>();
        pawnAttackBitboards[sq] = new ulong[2];
        for(int c = 4; c < 6; c++) {
            if (distances[sq][c] > 0) {
                legalAttackWhite.Add(sq + offset[c]);
                pawnAttackBitboards[sq][0] |= 1ul << sq + offset[c];
            }
        }
        for (int c = 6; c < 8; c++) {
            if (distances[sq][c] > 0) {
                legalAttackBlack.Add(sq+offset[c]);
                pawnAttackBitboards[sq][1] |= 1ul << sq+offset[c];
            }
        }
        pawnAttackBlack[sq] = legalAttackBlack.ToArray();
        pawnAttackWhite[sq] = legalAttackWhite.ToArray();
    }

    static void RookMoves(int sq) {
        for (int d = 0; d < 4; d++) {
            for (int n = 0; n < distances[sq][d]; n++) {
                int legalMove = sq + d * (n + 1);
                rookMoves[sq] |= 1ul << legalMove;
            }
        }
    }
    static void BishopMoves(int sq) {
        for (int d = 4; d < 8; d++) {
            for (int n = 0; n < distances[sq][d]; n++) {
                int legalMove = sq + d * (n + 1);
                bishopMoves[sq] |= 1ul << legalMove;
            }
        }
    }
    static void QueenMoves(int sq) {
        queenMoves[sq] = rookMoves[sq] | bishopMoves[sq];
    }


}
