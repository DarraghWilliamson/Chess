public class Knight : PieceObject {
    /*
    public override void ShowMoves() {
        base.ShowMoves();

        MoveTake(x + 2, y + 1);
        MoveTake(x + 2, y - 1);

        MoveTake(x - 2, y + 1);
        MoveTake(x - 2, y - 1);

        MoveTake(x - 1, y + 2);
        MoveTake(x + 1, y + 2);

        MoveTake(x - 1, y - 2);
        MoveTake(x + 1, y - 2);
    }

    public void MoveTake(int x, int y) {
        if (!Check(x, y)) return;
        if (tiles[x][y].piece != null) {
            if (IsEnemy(tiles[x][y].piece)) {
                tiles[x][y].ShowTakeable();
            } else {
                tiles[x][y].ShowBlocked();
            }

        } else {
            tiles[x][y].ShowMoveable();
        }
    */
    

}
