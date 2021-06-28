using System.Collections;
using System.Collections.Generic;
using System;

public readonly struct Move {
    public readonly struct Flag {
        public const int None = 0;
        public const int EnPassantCapture = 1;
        public const int Castling = 2;
        public const int PawnDoubleMove = 3;
        public const int PromotionRook = 4;
        public const int PromotionBishop = 5;
        public const int PromotionKnight = 6;
        public const int PromotionQueen = 7;
    }

    private readonly int flag;
    public int MoveFlag { get { return flag; } }

    private readonly int from;
    public int StartSquare { get { return from; } }

    private readonly int to;
    public int EndSquare { get { return to; } }
    
    public Move(int from, int to) {
        this.from = from;
        this.to = to;
        this.flag = 0;
    }

    public Move(int f, int t,int flag) {
        this.from = f;
        this.to = t;
        this.flag = flag;
    }
    public bool IsPromotion {
        get {
            return flag == 4 || flag == 5 || flag == 6 || flag == 7;
        }
    }

    public bool IsCastle {
        get {
            return flag == Flag.Castling;
        }
    }

    public bool IsDoublePawnMove {
        get {
            return flag == Flag.PawnDoubleMove;
        }
    }

}

public struct LoadInfo{
    public int[] squares;
    public bool[] castling;
    public int enpassant;
    public int turnColour;
    public int turnCount;
}

public struct GameState {
    private int[] sq;
    public int[] Squares { get { return sq; }  }
    private bool[] cas;
    public bool[] Castling { get { return cas; }}
    private int enpa;
    public int Enpassant { get { return enpa; } }
    private int cap;
    public int CapturedPiece { get { return cap; } }
    private List<int>[] li;
    public List<int>[] Lists { get { return li; } }

    public GameState(int[] c, bool[] cas,int enpa, int cap, List<int>[]li) {
        this.sq = c;
        this.cas = cas;
        this.enpa = enpa;
        this.cap = cap;
        this.li = li;
    }
}