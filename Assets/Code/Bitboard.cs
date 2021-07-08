public class Bitboard {
    public ulong[] bitboards;

    public Bitboard(ulong[] bitboards) {
        this.bitboards = bitboards;
    }

    public void AddBit(int board, int index) {
        bitboards[board] |= (1ul << index);
    }

    public void RemoveBit(int board, int index) {
        bitboards[board] ^= (1ul << index);
    }

    public bool BitExists(ulong board, int bit) {
        return ((bitboards[board] >> bit) & 1) != 0;
    }

    public ulong this[int index] {
        get {
            return bitboards[index];
        }
        set {
            bitboards[index] = value;
        }
    }
}