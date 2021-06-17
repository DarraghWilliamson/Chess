using UnityEngine;
using System;
using System.Collections.Generic;

public class Setup : MonoBehaviour {

    int playerColour;
    public Tile[] tiles = new Tile[64];
    public char[] origin;
    readonly string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    readonly string startFENj = "rnbqkb2/1ppppprp/p5pn/8/2B2P2/4PQ1N/PPPP2PP/RNB1K2R w KQkq - 0 0";
    readonly string startFENa = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
    public GameLogic gameLogic = new GameLogic();
    ArtificialPlayer artificialPlayer;

    void Start() {
        playerColour = 0;
        origin = new char[64];
        artificialPlayer = new ArtificialPlayer(1);
        LoadFEN(startFEN);
        tiles = SetUpTiles();
        new PieceLogic();
        PlacePieces();
        GameDisplay.instance.tiles = tiles;
        GameDisplay.instance.deathBlack = CreateDeathVectors(new Vector3(70, 0, -110), -20, 70);
        GameDisplay.instance.deathWhite = CreateDeathVectors(new Vector3(-70, 0, 110), 20, -70);
        gameLogic.StartTurn();
    }

    public void LoadFEN(string FEN) {
        Colour turn;
        bool[] Castling = new bool[4];
        int Enpassant = 0;
        int half;
        int full;
        bool en;

        string[] orig = FEN.Split(new char[] { ' ' });

        int rank = 7;
        int file = 0;
        foreach (char ch in orig[0]) {
            if (char.IsDigit(ch)) {
                file += (int)char.GetNumericValue(ch);
            } else {
                if (ch == '/') {
                    rank--;
                    file = 0;
                    continue;
                }
                origin[(rank * 8) + file] = ch;
                file++;
            }
        }

        turn = orig[1][0] == 'w' ? Colour.White : Colour.Black;
        foreach (char ch in orig[2]) {
            if (ch == '-') continue;
            if (ch == 'K') Castling[0] = true;
            if (ch == 'Q') Castling[1] = true;
            if (ch == 'k') Castling[2] = true;
            if (ch == 'q') Castling[3] = true;
        }
        if (orig[3][0] != '-') {
            en = true;
            int temp1 = ((int)char.ToUpper(orig[3][0])) - 65;
            int temp2 = 64 - (((int)char.GetNumericValue(orig[3][1])) * 8);
            Enpassant = (temp1 + temp2);
        } else {
            en = false;
        }
        half = Int32.Parse(orig[4]);
        full = Int32.Parse(orig[5]);
        Board b = new Board();
        b.board = origin;
        b.Castling = Castling;
        b.gameLogic = gameLogic;
        b.Enpassant = Enpassant;
        b.En = en;
        gameLogic.Setup(b, turn, playerColour, half, full, artificialPlayer, true);

    }

    public static Tile[] SetUpTiles() {
        Tile[] tiles = new Tile[64];
        string[] abc = { "A", "B", "C", "D", "E", "F", "G", "H" };
        GameObject tile_ = Resources.Load<GameObject>("Peices/Tile");
        GameObject Tiles = new GameObject("Tiles");
        int t = 0;
        Vector3 pos = new Vector3(-70, 0, 70);
        for (int i = 0; i < 8; i++) {
            for (int i2 = 0; i2 < 8; i2++) {
                GameObject tile = Instantiate(tile_, Tiles.transform);
                tile.transform.position = pos;
                pos.z -= 20;
                tile.name = abc[i2] + (i+1);
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
        Dictionary<char, string> dict = new Dictionary<char, string>() {
            ['p'] = "Pawn",
            ['b'] = "Bishop",
            ['n'] = "Knight",
            ['r'] = "Rook",
            ['k'] = "King",
            ['q'] = "Queen"
        };
        GameObject White = new GameObject("White");
        GameObject Black = new GameObject("Black");
        List<int> tempList = new List<int>();

        for (int i = 0; i < 64; i++) {
            if (origin[i] != '\0' && origin[i] != 'e') {
                tempList.Add(i);
                string colour;
                GameObject parent;
                if (char.IsUpper(origin[i])) {
                    colour = "White";
                    parent = White;
                } else {
                    colour = "black";
                    parent = Black;
                }
                GameObject piece = Instantiate(Resources.Load<GameObject>("Peices/" + dict[char.ToLower(origin[i])] + colour), parent.transform);

                Tile tile = tiles[i];
                tile.piece = piece.GetComponent<PieceObject>();
                tile.GetComponent<Tile>().PlacePiece(piece.GetComponent<PieceObject>());
                piece.transform.position = tile.gameObject.transform.position;
                if (colour == "black") piece.transform.rotation = Quaternion.Euler(0, 180, 0);
                piece.name = colour + dict[char.ToLower(origin[i])];
                piece.GetComponent<PieceObject>().tiles = tiles;
            }
        }
        GameObject temp = Instantiate(Resources.Load<GameObject>("Peices/PawnGrey"), White.transform);
        temp.GetComponent<MeshCollider>().enabled = false;
        temp.SetActive(false);
        GameDisplay.instance.Enp = temp;
        Piece[] pieces = new Piece[tempList.Count];
        for (int i = 0; i < tempList.Count; i++) {
            pieces[i] = new Piece(tempList[i], null);
        }
        gameLogic.pieces = pieces;

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
