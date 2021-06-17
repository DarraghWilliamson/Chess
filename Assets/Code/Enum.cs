using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Colour { 
        White, 
        Black 
    };

public readonly struct Move {

    public readonly struct Flag {
        public const int None = 0;
        public const int EnPassantCapture = 1;
        public const int Castling = 2;
        public const int PawnDoubleMove = 3;
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

public struct Piece {
    public int location;
    public List<Move> moves;
    public Piece(int l, List<Move> m) {
        this.location = l;
        this.moves = m;
    }
}

public struct GameState {
    private readonly char[] c;
    public char[] Board { get { return c; } }
    private readonly Colour t;
    public Colour Turn { get { return t; } }
    private readonly bool[] cas;
    public bool[] Castling { get { return cas; } }
    private readonly int enpa;
    public int Enpassant { get { return enpa; } }
    private readonly bool e;
    public bool En { get { return e; } }

    public GameState(char[] c, Colour t, bool[] cas,int enpa, bool e) {
        this.c = c;
        this.t = t;
        this.cas = cas;
        this.enpa = enpa;
        this.e = e;
    }
}