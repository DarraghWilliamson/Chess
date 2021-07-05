using System.Collections.Generic;
using UnityEngine;

public class Setup : MonoBehaviour {
    private GameLogic gameLogic = new GameLogic();
    private GameDisplay gameDisplay = new GameDisplay();

    private readonly Dictionary<int, string> dictString = new Dictionary<int, string>() {
        [Piece.Pawn] = "Pawn",
        [Piece.Bishop] = "Bishop",
        [Piece.Knight] = "Knight",
        [Piece.Rook] = "Rook",
        [Piece.King] = "King",
        [Piece.Queen] = "Queen"
    };

    private void Start() {
        gameLogic.gameDisplay = gameDisplay;
        gameDisplay.tiles = SetUpTiles();
        PlacePieces();
        gameLogic.Start();
    }

    public Tile[] SetUpTiles() {
        GameObject Tiles = new GameObject("Tiles");
        Tile[] tiles = new Tile[64];
        string[] abc = { "A", "B", "C", "D", "E", "F", "G", "H" };
        GameObject tile_ = Resources.Load<GameObject>("Peices/Tile");
        int t = 0;
        Vector3 pos = new Vector3(-70, 0, 70);
        for (int i = 0; i < 8; i++) {
            for (int i2 = 0; i2 < 8; i2++) {
                GameObject tile = Instantiate(tile_, Tiles.transform);
                tile.GetComponent<Tile>().gameDisplay = gameDisplay;
                tile.GetComponent<Tile>().gameLogic = gameLogic;
                tile.transform.position = pos;
                pos.z -= 20;
                tile.name = abc[i2] + (i + 1);
                tiles[t] = tile.GetComponent<Tile>();
                tiles[t].num = t;
                t++;
            }
            pos.z = 70;
            pos.x += 20;
        }

        return tiles;
    }

    public void PlacePieces() {
        Tile[] tiles = GameDisplay.instance.tiles;
        int[] defultPieces = new int[] {
            14, 14, 14, 14, 14, 14, 14, 14,
            11,11,11,11,11,11,
            13,13, 13, 13, 13, 13,
            15,15,15,15,15,15,
            10,10,10,10,10,10,10,10,
            18,18,18,18,18,18,18,18,
            22,22, 22,22,22,22,22,22,
            19,19,19,19,19,19,19,19,
            21, 21,21, 21,21, 21,21, 21,
            23,23,23,23,23,23,23,23,
            17,9,
        };
        int[] promotionPieces = new int[] { 14, 11, 13, 15 };
        int[] promorionCords = new int[] { 30, 10, -10, -30 };

        GameObject[] kings = new GameObject[2];
        List<GameObject>[] pawns = { new List<GameObject>(), new List<GameObject>() };
        List<GameObject>[] knights = { new List<GameObject>(), new List<GameObject>() };
        List<GameObject>[] rooks = { new List<GameObject>(), new List<GameObject>() };
        List<GameObject>[] bishops = { new List<GameObject>(), new List<GameObject>() };
        List<GameObject>[] queens = { new List<GameObject>(), new List<GameObject>() };
        List<GameObject>[] all = { pawns[0], knights[0], rooks[0], bishops[0], queens[0], pawns[1], knights[1], rooks[1], bishops[1], queens[1] };

        GameObject PromoWhite = new GameObject("PromotionWhite");
        GameObject PromoBlack = new GameObject("PromotionBlack");
        List<GameObject> PromotionWhite = new List<GameObject>();
        List<GameObject> PromotionBlack = new List<GameObject>();
        for (int p = 0; p < promotionPieces.Length; p++) {
            GameObject pieceW = Instantiate(Resources.Load<GameObject>("Peices/Promotion/" + dictString[Piece.Type(promotionPieces[p])] + "White"), PromoWhite.transform);
            pieceW.transform.position = new Vector3(110, 0, promorionCords[p]);
            pieceW.GetComponent<PieceObject>().gameLogic = gameLogic;
            pieceW.GetComponent<PieceObject>().gameDisplay = gameDisplay;
            pieceW.GetComponent<PieceObject>().isPromotionPiece = true;
            PromotionWhite.Add(pieceW);
            pieceW.SetActive(false);
            pieceW.transform.rotation = Quaternion.Euler(0, 180, 0);
            GameObject pieceB = Instantiate(Resources.Load<GameObject>("Peices/Promotion/" + dictString[Piece.Type(promotionPieces[p])] + "Black"), PromoBlack.transform);
            pieceB.transform.position = new Vector3(-110, 0, promorionCords[p]);
            pieceB.GetComponent<PieceObject>().isPromotionPiece = true;
            PromotionBlack.Add(pieceB);
            pieceB.GetComponent<PieceObject>().gameLogic = gameLogic;
            pieceB.GetComponent<PieceObject>().gameDisplay = gameDisplay;
            pieceB.SetActive(false);
        }

        GameObject White = new GameObject("White");
        GameObject Black = new GameObject("Black");
        for (int i = 0; i < defultPieces.Length; i++) {
            if (defultPieces[i] != '\0' && defultPieces[i] != 'e') {
                string colour;
                GameObject parent;
                if (Piece.IsColour(defultPieces[i], Piece.White)) {
                    colour = "White";
                    parent = White;
                } else {
                    colour = "black";
                    parent = Black;
                }

                GameObject piece = Instantiate(Resources.Load<GameObject>("Peices/" + dictString[Piece.Type(defultPieces[i])] + colour), parent.transform);

                if (colour == "black") piece.transform.rotation = Quaternion.Euler(0, 180, 0);
                piece.name = colour + dictString[Piece.Type(defultPieces[i])];
                PieceObject pObject = piece.GetComponent<PieceObject>();
                pObject.tiles = tiles;
                pObject.type = defultPieces[i];
                pObject.gameLogic = gameLogic;
                pObject.gameDisplay = gameDisplay;

                int col = colour == "White" ? 0 : 1;
                switch (Piece.Type(defultPieces[i])) {
                    case Piece.King: kings[col] = piece; break;
                    case Piece.Pawn: pawns[col].Add(piece); break;
                    case Piece.Knight: knights[col].Add(piece); break;
                    case Piece.Rook: rooks[col].Add(piece); break;
                    case Piece.Bishop: bishops[col].Add(piece); break;
                    case Piece.Queen: queens[col].Add(piece); break;
                }
            }
        }

        GameDisplay.instance.pawns = pawns;
        GameDisplay.instance.knights = knights;
        GameDisplay.instance.rooks = rooks;
        GameDisplay.instance.bishops = bishops;
        GameDisplay.instance.queens = queens;
        GameDisplay.instance.kings = kings;
        GameDisplay.instance.allPieces = all;
        GameDisplay.instance.PromotionBlack = PromotionBlack;
        GameDisplay.instance.PromotionWhite = PromotionWhite;

        GameObject temp = Instantiate(Resources.Load<GameObject>("Peices/PawnGrey"), White.transform);
        temp.GetComponent<MeshCollider>().enabled = false;
        temp.SetActive(false);
        GameDisplay.instance.Enp = temp;
    }
}