public class PieceList {
    public int[] pieces;
    public int[] map;
    public int length;

    public PieceList() {
        pieces = new int[16];
        map = new int[64];
        length = 0;
    }

    public void Push(int ind) {
        map[ind] = length;
        pieces[length] = ind;
        length++;
    }

    public void Remove(int ind) {
        length--;  //reduce pieces in list
        int pieceLoc = map[ind]; //get the square the index is on
        pieces[pieceLoc] = pieces[length]; //move the last entry in list to the pos of the removed entry
        map[pieces[pieceLoc]] = pieceLoc;  //update the map to the ajusted location
    }

    public int Count() {
        return length;
    }

    public void Move(int from, int to) {
        int moveing = map[from];
        pieces[moveing] = to;
        map[to] = moveing;
    }

    //public int this[int index] => pieces[index];
    public int this[int index] {
        get {
            return pieces[index];
        }
    }
}