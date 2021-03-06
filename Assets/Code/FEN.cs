using System.Collections.Generic;
using UnityEngine;

public static class FEN {
    public readonly static string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public readonly static string Kiwipete = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -";
    public readonly static string twoMil = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";

    public readonly static string[] FenArray = new string[] {
        "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1",
        "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1",
        "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8",
        "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10"
    };

    private static readonly Dictionary<char, int> dictInt = new Dictionary<char, int>() {
        ['p'] = Piece.Pawn,
        ['b'] = Piece.Bishop,
        ['n'] = Piece.Knight,
        ['r'] = Piece.Rook,
        ['k'] = Piece.King,
        ['q'] = Piece.Queen,
    };

    public static LoadInfo LoadNewFEN(string FEN) {
        int[] origin = new int[64];
        int turn, half, full;
        bool[] Castling = new bool[4];
        int Enpassant = 0;
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
                origin[(rank * 8) + file] = col | type;
                file++;
            }
        }
        turn = split[1][0] == 'w' ? 0 : 1;
        int castle = 0;
        foreach (char ch in split[2]) {
            if (ch == '-') continue;
            if (ch == 'K') castle |= 1;
            if (ch == 'Q') castle |= 2;
            if (ch == 'k') castle |= 4;
            if (ch == 'q') castle |= 8;
        }
        if (split[3][0] != '-') {
            Enpassant = ((int)char.ToUpper(split[3][0])) - 64;
        }
        half = 0;
        full = 0;
        if (split.Length > 4) {
            if (split[4] != "") {
                half = int.Parse(split[4]);
                if (split.Length >= 6) {
                    full = int.Parse(split[5]);
                }
            }
        }

        int state = ((Enpassant << 4) + castle);
        return new LoadInfo { squares = origin, state = state, turnColour = turn, turnCount = full };
    }

    public static void ExportFen(Board board) {
        string FEN = "";
        int[] squares = board.squares;
        for (int file = 7; file >= 0; file--) {
            int emptyCount = 0;
            for (int rank = 0; rank < 8; rank++) {
                int i = file * 8 + rank;
                if (squares[i] == 0) {
                    emptyCount++;
                } else {
                    if (emptyCount != 0) {
                        FEN += emptyCount.ToString();
                        emptyCount = 0;
                    }
                    char pieceChar = ' ';
                    int pieceType = Piece.Type(squares[i]);
                    switch (pieceType) {
                        case Piece.King: pieceChar = 'k'; break;
                        case Piece.Pawn: pieceChar = 'p'; break;
                        case Piece.Rook: pieceChar = 'r'; break;
                        case Piece.Knight: pieceChar = 'n'; break;
                        case Piece.Bishop: pieceChar = 'b'; break;
                        case Piece.Queen: pieceChar = 'q'; break;
                    }
                    if (Piece.IsColour(squares[i], Piece.White)) {
                        FEN += char.ToUpper(pieceChar);
                    } else {
                        FEN += pieceChar;
                    }
                }
            }
            if (emptyCount != 0) {
                FEN += emptyCount.ToString();
            }
            if (file != 0) FEN += '/';
        }
        //turn
        FEN += board.turnColour == 0 ? " w " : " b ";
        //castling
        string castling = "";
        int castle = board.currentGameState & 15;
        castling += ((castle >> 0) & 1) != 0 ? "K" : "";
        castling += ((castle >> 1) & 1) != 0 ? "Q" : "";
        castling += ((castle >> 2) & 1) != 0 ? "k" : "";
        castling += ((castle >> 3) & 1) != 0 ? "q" : "";
        if (castling == "") {
            FEN += "- ";
        } else {
            FEN += castling + " ";
        }
        //Enpassant
        int e = ((board.currentGameState >> 4) & 15);
        if (e != 0) {
            int add = board.turnColour == 1 ? 15 : 39;
            FEN += GetBoardRep(e + add) + " ";
        } else {
            FEN += "- ";
        }
        //moves
        FEN += "0 ";
        FEN += "0 ";
        Debug.Log(FEN);
    }

    private static string GetBoardRep(int sq) {
        int rank = (sq / 8) + 1;
        int t = sq % 8;
        char file = (char)(t + 65);
        string s = char.ToLower(file) + "" + rank;
        return s;
    }
}