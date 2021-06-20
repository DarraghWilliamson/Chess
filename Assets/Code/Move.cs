using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public readonly struct Move {
    public readonly struct Flag {
        public const int None = 0;
        public const int EnPassantCapture = 1;
        public const int Castling = 2;
        public const int PawnDoubleMove = 3;
        public const int Promotion = 4;
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

public enum Colour {
    White,
    Black
};



public struct GameState {
    private int[] sq;
    public int[] Squares { get { return sq; } set { sq = value; } }
    private bool[] cas;
    public bool[] Castling { get { return cas; } set { cas = value; } }
    private int enpa;
    public int Enpassant { get { return enpa; } set { enpa = value; } }
    private int cap;
    public int CapturedPiece { get { return cap; } set { cap = value; } }

    public GameState(int[] c, bool[] cas,int enpa, int cap) {
        this.sq = c;
        this.cas = cas;
        this.enpa = enpa;
        this.cap = cap;
    }
}