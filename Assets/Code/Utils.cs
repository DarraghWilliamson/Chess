using System.Collections.Generic;
using System.Text;

public static class Utils {

    // A move uses 16 bits
    // 0-5      startSq
    // 6-11     endSq
    // 12-13    moveType | 0:none, 1:Enpassant, 2:castle, 3:promotion
    // 14-15    promotionType | 0:Knight, 1:bishop, 2:rook, 3:queen
    public static int CreatePromotionMoveShort(int from, int to, int promotion) {
        return ((promotion << 14) + (0x03 << 12) + (from << 6) + to);
    }

    public static int CreateMoveShort(int from, int to, int type) {
        return (((type << 12) + (from << 6) + to));
    }

    public static int CreateMoveShort(int from, int to) {
        return (((from << 6) + to));
    }

    public static int GetStartSquare(int m) {
        return ((m >> 6) & 0x3F);
    }

    public static int GetEndSquare(int m) {
        return (m & 0x3F);
    }

    public static int GetMoveType(int m) {
        return ((m >> 12) & 0x03);
    }

    public static int GetPromotionType(int m) {
        return ((m >> 14) & 0x03);
    }

    // A gamestate uses 16 bits
    // 0-3      Castling | 0:Kw 1:Qw 2:Kb 3:Qk
    // 4        Enpass?
    // 5-7      Enpass file
    // 8-10     CapturedPieceType
    // 11-15    counter
    public static int GetCapturedPieceType(ushort m) {
        return ((m >> 8) & 0x03);
    }

    public static bool BitExists(ulong board, int bit) {
        return ((board >> bit) & 1) != 0;
    }

    public static bool CanCastle(int castleShort, int pos) {
        return ((castleShort >> pos) & 1) != 0;
    }

    //logs moves to conosle
    public static string Log(int move) {
        Dictionary<int, char> lerretDict = new Dictionary<int, char> {
            { Piece.King,'K' },
            { Piece.Queen,'Q' },
            { Piece.Knight,'N' },
            { Piece.Bishop,'B' },
            { Piece.Rook,'R' }
        };
        int from = GetStartSquare(move);
        int to = GetEndSquare(move);
        int[] squares = GameLogic.instance.board.squares;

        if (GetMoveType(move) == 2) {
            if (to == 62 || to == 6) {
                return "0-0";
            } else {
                return "0-0-0";
            }
        }
        StringBuilder log = new StringBuilder();
        if (Piece.Type(squares[from]) == Piece.Pawn) {
            if (squares[to] != 0) log.Append(GetBoardRep(squares[from])[0] + "x");
        } else {
            log.Append(lerretDict[Piece.Type(squares[from])]);
            if (squares[to] != 0) log.Append("x");
        }
        log.Append(GetBoardRep(squares[from]));
        return log.ToString();
    }

    public static int GetRank(int sq) {
        return (sq / 8) + 1;
    }

    public static int GetFile(int sq) {
        return sq % 8;
    }

    public static string PrintMove(int move) {
        string s = GetStartSquare(move) + " " + GetEndSquare(move);
        s += " " + GetMoveType(move) + " " + GetPromotionType(move);

        return (s);
    }

    public static string PrintMoveRep(int move) {
        return (GetBoardRep(GetStartSquare(move)) + "" + GetBoardRep(GetEndSquare(move)));
    }

    public static string GetBoardRep(int sq) {
        int rank = (sq / 8) + 1;
        int t = sq % 8;
        char file = (char)(t + 65);
        string s = char.ToLower(file) + "" + rank;
        return s;
    }
}

public struct LoadInfo {
    public int[] squares;
    public int state;
    public int turnColour;
    public int turnCount;
}

public struct GameState {
    private bool[] cas;
    public bool[] Castling { get { return cas; } }
    private int enpa;
    public int Enpassant { get { return enpa; } }
    private int cap;
    public int CapturedPiece { get { return cap; } }

    public GameState(bool[] cas, int enpa, int cap) {
        this.cas = cas;
        this.enpa = enpa;
        this.cap = cap;
    }
}