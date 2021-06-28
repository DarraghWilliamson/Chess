using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameDisplay {

    public static GameDisplay instance;
    public int deadBlack = 0;
    public int deadWhite = 0;
    public Vector3[] deathBlack, deathWhite;
    public Tile[] tiles;
    public List<Tile> activatedTiles = new List<Tile>();
    public PieceObject SelectedPeice;
    public List<PieceObject> activatedPieces = new List<PieceObject>();
    public GameObject Enp;
    public GameObject[] kings;
    public List<GameObject>[] pawns, knights, rooks, bishops, queens, allPieces;
    public List<GameObject> Pieces, PromotionBlack, PromotionWhite;

    public GameDisplay() {
        instance = this;
        deathBlack = CreateDeathVectors(new Vector3(70, 0, -110), -20, 70);
        deathWhite = CreateDeathVectors(new Vector3(-70, 0, 110), 20, -70);
    }

    readonly Dictionary<int, string> dictString = new Dictionary<int, string>() {
        [Piece.Pawn] = "Pawn",
        [Piece.Bishop] = "Bishop",
        [Piece.Knight] = "Knight",
        [Piece.Rook] = "Rook",
        [Piece.King] = "King",
        [Piece.Queen] = "Queen"
    };

    public void AddNewPiece(Move m) {
        int type = 0;
        int square = m.EndSquare;
        switch (m.MoveFlag) {
            case Move.Flag.PromotionQueen: type = Piece.Queen; break;
            case Move.Flag.PromotionBishop: type = Piece.Bishop; break;
            case Move.Flag.PromotionRook: type = Piece.Rook; break;
            case Move.Flag.PromotionKnight: type = Piece.Knight; break;
        }
        int col = GameLogic.instance.board.turnColour;
        string colour = col == 0 ? "White" : "Black";
        GameObject piece = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Peices/" + dictString[type] + colour));
        Tile tile = tiles[square];
        tile.piece = piece.GetComponent<PieceObject>();
        tile.GetComponent<Tile>().piece = (piece.GetComponent<PieceObject>());
        piece.transform.position = tile.gameObject.transform.position;
        if (colour == "black") piece.transform.rotation = Quaternion.Euler(0, 180, 0);
        piece.name = colour + dictString[type];
        piece.GetComponent<PieceObject>().tiles = tiles;
        piece.GetComponent<PieceObject>().type = type;
        tile.piece.gameLogic = GameLogic.instance;
        tile.piece.gameDisplay = this;
        
        switch (type) {
            case Piece.King: kings[col] = piece; break;
            case Piece.Pawn: pawns[col].Add(piece); break;
            case Piece.Knight: knights[col].Add(piece); break;
            case Piece.Rook: rooks[col].Add(piece); break;
            case Piece.Bishop: bishops[col].Add(piece); break;
            case Piece.Queen: queens[col].Add(piece); break;

        }
    }

    //probbaly better ways of doing this 
    public void RefreshDisplay(Board board) {
        deadBlack = 0;
        deadWhite = 0;
        Unselect();
        Clear(board);
        
        for (int i = 0; i < 2; i++) {
            PieceObject king = kings[i].GetComponent<PieceObject>();
            int ind = board.kings[i];
            king.SetTile(tiles[ind]);
            tiles[ind].SetPiece(king);
            kings[i].transform.position = tiles[ind].transform.position;
        }

        //cycle through piece onjects, place piece down where thers a piece on the board
        //if no place is found kill the piece
        List<int>[] allLists = board.allLists;
        for (int objList = 0; objList < allLists.Length; objList++) {
            for (int obj = 0; obj < allLists[objList].Count; obj++) {



                PieceObject piece = allPieces[objList][obj].GetComponent<PieceObject>();
                int ind = allLists[objList][obj];
                piece.SetTile(tiles[ind]);
                tiles[ind].SetPiece(piece);
                piece.gameObject.SetActive(true);
                allPieces[objList][obj].transform.position = tiles[ind].transform.position;


            }
        }


        /*
            for (int objList = 0; objList < allPieces.Length; objList++) {
            for (int obj = 0; obj < allPieces[objList].Count; obj++) {
                PieceObject piece = allPieces[objList][obj].GetComponent<PieceObject>();
                try {
                    allPieces[objList][obj].SetActive(true);
                    int sq = allLists[objList][obj];
                    piece.SetTile(tiles[sq]);
                    tiles[sq].SetPiece(piece);
                    piece.gameObject.SetActive(true);
                    allPieces[objList][obj].transform.position = tiles[sq].transform.position;
                } catch (System.ArgumentOutOfRangeException) {
                    piece.Die();
                    piece.gameObject.SetActive(false);
                    continue;
                }

            }



        }
            */
    }

    void Clear(Board board) {
        for (int i = 0; i < 64; i++) {
            tiles[i].piece = null;
        }
        List<int>[] allLists = board.allLists;
        for (int objList = 0; objList < allPieces.Length; objList++) {
            for (int obj = 0; obj < allPieces[objList].Count; obj++) {
                allPieces[objList][obj].SetActive(false);
            }
        }

            }

    public void SelectNew(PieceObject peice) {
        if (SelectedPeice != null) {
            SelectedPeice.Unselect();
        }
        peice.Select();
        SelectedPeice = peice;
    }

    public void Unselect() {
        if (SelectedPeice != null) {
            SelectedPeice.Unselect();
            SelectedPeice = null;
        }
    }

    public Vector3[] CreateDeathVectors(Vector3 pos, int x, int y) {
        Vector3[] vector3s = new Vector3[16];
        int v = 0;
        for (int i = 0; i < 2; i++) {
            for (int i2 = 0; i2 < 8; i2++) {
                vector3s[v] = pos;
                v++;
                pos.x += x;
            }
            pos.x = y;
            pos.z += x;
        }
        return vector3s;
    }

}
