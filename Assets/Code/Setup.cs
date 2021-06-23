using UnityEngine;
using System;
using System.Collections.Generic;

public class Setup : MonoBehaviour {

    int playerColour;
    public Tile[] tiles = new Tile[64];
    public int[] origin;
    readonly string startFEN1 = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    readonly string startFEN2 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 0";
    readonly string startFEN3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1";
    readonly string startFEN4 = "r3k2r/p1ppqpb1/bn2P1p1/4N3/1p2n3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 0";
    public GameLogic gameLogic = new GameLogic();
    ArtificialPlayer artificialPlayer;
    Board board = new Board();

    readonly Dictionary<int, string> dictString = new Dictionary<int, string>() {
        [Piece.Pawn] = "Pawn",
        [Piece.Bishop] = "Bishop",
        [Piece.Knight] = "Knight",
        [Piece.Rook] = "Rook",
        [Piece.King] = "King",
        [Piece.Queen] = "Queen"
    };
    readonly Dictionary<char, int> dictInt = new Dictionary<char, int>() {
        ['p'] = Piece.Pawn,
        ['b'] = Piece.Bishop,
        ['n'] = Piece.Knight,
        ['r'] = Piece.Rook,
        ['k'] = Piece.King,
        ['q'] = Piece.Queen,
    };

    void Start() {
        playerColour = 0;
        artificialPlayer = new ArtificialPlayer(1);
        origin = new int[64];
        LoadFEN(startFEN2);
        tiles = SetUpTiles();
        PlacePieces();
        GameDisplay.instance.tiles = tiles;
        
        gameLogic.StartTurn();
    }

    public void LoadFEN(string FEN) {
        int turn;
        bool[] Castling = new bool[4];
        int Enpassant = 0;
        int half;
        int full;

        string[] split = FEN.Split(new char[] { ' ' });

        int rank = 7;
        int file = 0;
        foreach (char ch in split[0]) {
            if (char.IsDigit(ch)) {
                file += (int)char.GetNumericValue(ch);
            } else {
                if (ch == '/') {
                    rank--;
                    file = 0;
                    continue;
                }
                int col = char.IsUpper(ch) ? Piece.White : Piece.Black;
                int type = dictInt[char.ToLower(ch)];

                origin[(rank * 8) + file] = col|type;
                file++;
            }
        }

        turn = split[1][0] == 'w' ? 0 : 1;
        foreach (char ch in split[2]) {
            if (ch == '-') continue;
            if (ch == 'K') Castling[0] = true;
            if (ch == 'Q') Castling[1] = true;
            if (ch == 'k') Castling[2] = true;
            if (ch == 'q') Castling[3] = true;
        }
        if (split[3][0] != '-') {
            int temp1 = ((int)char.ToUpper(split[3][0])) - 65;
            int temp2 = 64 - (((int)char.GetNumericValue(split[3][1])) * 8);
            Enpassant = (temp1 + temp2);
        }
        half = Int32.Parse(split[4]);
        full = Int32.Parse(split[5]);


        int[] kings = new int[2];
        List<int>[] pawns = { new List<int>(), new List<int>() };
        List<int>[] knights = { new List<int>(), new List<int>() };
        List<int>[] rooks = { new List<int>(), new List<int>() };
        List<int>[] bishops = { new List<int>(), new List<int>() };
        List<int>[] queens = { new List<int>(), new List<int>() };
        for (int i = 0; i < 64; i++) {
            if (origin[i] == Piece.None) continue;
            int col = Piece.Colour(origin[i]);
            if (Piece.Type(origin[i]) == Piece.King) kings[col] = i;
            if (Piece.Type(origin[i]) == Piece.Pawn) pawns[col].Add(i);
            if (Piece.Type(origin[i]) == Piece.Knight) knights[col].Add(i);
            if (Piece.Type(origin[i]) == Piece.Bishop) bishops[col].Add(i);
            if (Piece.Type(origin[i]) == Piece.Rook) rooks[col].Add(i);
            if (Piece.Type(origin[i]) == Piece.Queen) queens[col].Add(i);
        }
        board.kings = kings;
        board.pawns = pawns;
        board.knights = knights;
        board.rooks = rooks;
        board.bishops = bishops;
        board.queens = queens;
        List<int>[] all = { pawns[0], knights[0], rooks[0],bishops[0],queens[0], pawns[1], knights[1], rooks[1], bishops[1], queens[1] };
        board.allLists = all;
        board.squares = origin;
        board.Castling = Castling;
        board.gameLogic = gameLogic;
        board.Enpassant = Enpassant;
        board.turnColour = turn;
        board.enemyColour = turn == 0 ? 1 : 0;
        board.MoveCounter = full;
        gameLogic.Setup(board, playerColour, half, full, artificialPlayer, false);

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
        GameObject[] kings = new GameObject[2];
        List<GameObject>[] pawns = { new List<GameObject>(), new List<GameObject>() };
        List<GameObject>[] knights = { new List<GameObject>(), new List<GameObject>() };
        List<GameObject>[] rooks = { new List<GameObject>(), new List<GameObject>() };
        List<GameObject>[] bishops = { new List<GameObject>(), new List<GameObject>() };
        List<GameObject>[] queens = { new List<GameObject>(), new List<GameObject>() };
        List<GameObject>[] all = { pawns[0], knights[0], rooks[0], bishops[0], queens[0], pawns[1], knights[1], rooks[1], bishops[1], queens[1] };
        GameObject White = new GameObject("White");
        GameObject Black = new GameObject("Black");

        for (int i = 0; i < 64; i++) {
            if (origin[i] != '\0' && origin[i] != 'e') {
                string colour;
                GameObject parent;
                if(Piece.IsColour(origin[i],Piece.White)) {
                    colour = "White";
                    parent = White;
                } else {
                    colour = "black";
                    parent = Black;
                }

                GameObject piece = Instantiate(Resources.Load<GameObject>("Peices/" + dictString[Piece.Type( origin[i])] + colour), parent.transform);

                Tile tile = tiles[i];
                tile.piece = piece.GetComponent<PieceObject>();
                tile.GetComponent<Tile>().PlacePiece(piece.GetComponent<PieceObject>());
                piece.transform.position = tile.gameObject.transform.position;
                if (colour == "black") piece.transform.rotation = Quaternion.Euler(0, 180, 0);
                piece.name = colour + dictString[Piece.Type(origin[i])];
                piece.GetComponent<PieceObject>().tiles = tiles;
                piece.GetComponent<PieceObject>().type = origin[i];

                int col = colour == "White" ? 0 : 1;
                switch (Piece.Type(origin[i])) {
                    case Piece.King: kings[col] =  piece;break;
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
        GameDisplay.instance.all = all;


        GameObject temp = Instantiate(Resources.Load<GameObject>("Peices/PawnGrey"), White.transform);
        temp.GetComponent<MeshCollider>().enabled = false;
        temp.SetActive(false);
        GameDisplay.instance.Enp = temp;
    }

    

}
