using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranspositionTable{
    public Entry[] entries;
    public readonly ulong size;
    Board board;

    public TranspositionTable(Board board,int size) {
        this.board = board;
        this.size = (ulong)size;
        entries = new Entry[size];
    }
    public ulong Index {
        get {
            return board.ZobristKey % size;
        }
    }

    public uint GetEnrtyNodes() {
        Entry entry = entries[Index];
        if (entry.zKey == board.ZobristKey) return entry.nodes;
        return 0;
    }

    public void StoreEntry(uint nodes) {
        entries[Index] = new Entry(board.ZobristKey, nodes);
    }
}

public struct Entry {
    public readonly ulong zKey;
    //public readonly Move move;
    //public readonly int depth;
    public readonly uint nodes;
    public Entry(ulong zKey, uint nodes) {
        //this.depth = depth;
        this.zKey = zKey;
        //this.move = move; 
        this.nodes = nodes;
    }

}
