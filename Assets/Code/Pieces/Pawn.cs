public class Pawn : PieceObject {

    /*
    public override void ShowMoves() { //bounds check
        base.ShowMoves();
        if (colour== Colour.White) {
            HighlightMove(x - 1, y);
            HighlightTake(x - 1, y - 1);
            HighlightTake(x - 1, y + 1);
            if (firstMove) HighlightMove(x - 2, y);
        } else {
            HighlightMove(x + 1, y);
            HighlightTake(x + 1, y + 1);
            HighlightTake(x + 1, y - 1);
            if (firstMove) HighlightMove(x + 2, y);
        }
    }

    public void HighlightMove(int x,int y) {
        if (!Check(x, y)) return;
        if (tiles[x][y].piece != null) {
            tiles[x][y].ShowBlocked();
        } else {
            tiles[x][y].ShowMoveable();
        }
    }

    public void HighlightTake(int x, int y) {
        if (!Check(x, y)) return;

        if (tiles[x][y].piece != null) {
            if (tiles[x][y].piece.colour != colour) {
                tiles[x][y].ShowTakeable();
            }
        }
    }
    */

}
