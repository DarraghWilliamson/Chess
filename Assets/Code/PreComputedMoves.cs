using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Random;
using static Utils;

public static class PreComputedMoves {

    private static readonly int[] rookOccupancyBits = new int[64] {
    12, 11, 11, 11, 11, 11, 11, 12,
    11, 10, 10, 10, 10, 10, 10, 11,
    11, 10, 10, 10, 10, 10, 10, 11,
    11, 10, 10, 10, 10, 10, 10, 11,
    11, 10, 10, 10, 10, 10, 10, 11,
    11, 10, 10, 10, 10, 10, 10, 11,
    11, 10, 10, 10, 10, 10, 10, 11,
    12, 11, 11, 11, 11, 11, 11, 12
    };

    private static readonly int[] bishopOccupancyBits = new int[64] {
    6, 5, 5, 5, 5, 5, 5, 6,
    5, 5, 5, 5, 5, 5, 5, 5,
    5, 5, 7, 7, 7, 7, 5, 5,
    5, 5, 7, 9, 9, 7, 5, 5,
    5, 5, 7, 9, 9, 7, 5, 5,
    5, 5, 7, 7, 7, 7, 5, 5,
    5, 5, 5, 5, 5, 5, 5, 5,
    6, 5, 5, 5, 5, 5, 5, 6
    };

    public static readonly ulong[] RookMagicNumbers = new ulong[64] {
        9979994641325359136,
90072129987412032,
180170925814149121,
72066458867205152,
144117387368072224,
216203568472981512,
9547631759814820096,
2341881152152807680,
140740040605696,
2316046545841029184,
72198468973629440,
81205565149155328,
146508277415412736,
703833479054336,
2450098939073003648,
576742228899270912,
36033470048378880,
72198881818984448,
1301692025185255936,
90217678106527746,
324684134750365696,
9265030608319430912,
4616194016369772546,
2199165886724,
72127964931719168,
2323857549994496000,
9323886521876609,
9024793588793472,
562992905192464,
2201179128832,
36038160048718082,
36029097666947201,
4629700967774814240,
306244980821723137,
1161084564161792,
110340390163316992,
5770254227613696,
2341876206435041792,
82199497949581313,
144120019947619460,
324329544062894112,
1152994210081882112,
13545987550281792,
17592739758089,
2306414759556218884,
144678687852232706,
9009398345171200,
2326183975409811457,
72339215047754240,
18155273440989312,
4613959945983951104,
145812974690501120,
281543763820800,
147495088967385216,
2969386217113789440,
19215066297569792,
180144054896435457,
2377928092116066437,
9277424307650174977,
4621827982418248737,
563158798583922,
5066618438763522,
144221860300195844,
281752018887682,
    };

    public static readonly ulong[] BishopMagicNumbers = new ulong[64] {
    18018831494946945,
1134767471886336,
2308095375972630592,
27308574661148680,
9404081239914275072,
4683886618770800641,
216245358743802048,
9571253153235970,
27092002521253381,
1742811846410792,
8830470070272,
9235202921558442240,
1756410529322199040,
1127005325142032,
1152928124311179269,
2377913937382869017,
2314850493043704320,
4684324174200832257,
77688339246880000,
74309421802472544,
8649444578941734912,
4758897525753456914,
18168888584831744,
2463750540959940880,
9227893366251856128,
145276341141897348,
292821938185734161,
5190965918678714400,
2419567834477633538,
2308272929927873024,
18173279030480900,
612771170333492228,
4611976426970161409,
2270508834359424,
9223442681551127040,
144117389281722496,
1262208579542270208,
13988180992906560530,
4649975687305298176,
9809420809726464128,
1153222256471056394,
2901448468860109312,
40690797321924624,
4504295814726656,
299204874469892,
594838215186186752,
7210408796106130432,
144405467744964672,
145390656058359810,
1153203537948246016,
102002796048417802,
9243919728426124800,
2455024885924167748,
72066815467061280,
325424741529814049,
1175584649085829253,
18720594346444812,
584352516473913920,
1441151883179198496,
4919056693802862608,
1161950831810052608,
2464735771073020416,
54610562058947072,
580611413180448,
};

    public static readonly int[][] distances;
    public static readonly int[] offset = new int[] { 8, -8, -1, 1, 7, 9, -9, -7 };
    private static readonly int[] knightOffsets = new int[] { 15, 17, -17, -15, -10, 6, -6, 10 };
    public static readonly int[] inverse = new int[] { 1, 0, 3, 2, 7, 6, 5, 4 };
    public static readonly int[][] pawnAttackOffsets = { new int[] { 4, 6 }, new int[] { 7, 5 } };
    public static readonly ulong[][] rays;

    public static readonly int[][] pawnAttackWhite;
    public static readonly int[][] pawnAttackBlack;
    public static readonly byte[][] knightMoves;
    public static readonly byte[][] kingMoves;

    public static readonly ulong[][] pawnAttackBitboards;
    public static readonly ulong[] kingAttackBitboards;
    public static readonly ulong[] knightAttackBitboards;

    private static ulong[] rookMasks;
    private static ulong[] bishopMasks;
    private static ulong[,] rookAttacksTable;
    private static ulong[,] bishopAttacksTable;

    static PreComputedMoves() {
        pawnAttackWhite = new int[64][];
        pawnAttackBlack = new int[64][];
        distances = new int[64][];
        knightMoves = new byte[64][];
        kingMoves = new byte[64][];

        rookMasks = new ulong[64];
        bishopMasks = new ulong[64];
        rookAttacksTable = new ulong[64, 4096];
        bishopAttacksTable = new ulong[64, 512];

        knightAttackBitboards = new ulong[64];
        kingAttackBitboards = new ulong[64];
        pawnAttackBitboards = new ulong[64][];

        rays = new ulong[64][];

        for (int sq = 0; sq < 64; sq++) {
            GetDistances(sq);
            CalculateRays(sq);
            GetKnightMoves(sq);
            GetKingMoves(sq);
            PawnAttacks(sq);
        }

        SliderAttacks(true);
        SliderAttacks(false);

        /*ulong occ = 0ul;
        occ |= 1ul << a2;
        occ |= 1ul << 30;
        occ |= 1ul << 27;
        occ |= 1ul << 28;
        occ |= 1ul << 20;
        UnityEngine.Debug.Log(BitboardToString(GetRookAttacks(44, occ)));
        UnityEngine.Debug.Log(BitboardToString(GetBishopAttacks(27, occ)));*/
    }

    public static ulong GetBishopAttacks(int sq, ulong occupancy) {
        occupancy &= bishopMasks[sq];
        occupancy *= BishopMagicNumbers[sq];
        occupancy >>= 64 - bishopOccupancyBits[sq];
        return bishopAttacksTable[sq, occupancy];
    }

    public static ulong GetRookAttacks(int sq, ulong occupancy) {
        occupancy &= rookMasks[sq];
        occupancy *= RookMagicNumbers[sq];
        occupancy >>= 64 - rookOccupancyBits[sq];
        return rookAttacksTable[sq, occupancy];
    }

    private static void SliderAttacks(bool isBishop) {
        for (int sq = 0; sq < 64; sq++) {
            bishopMasks[sq] = BishopAttackMap(sq);
            rookMasks[sq] = RookAttackMap(sq);
            ulong attackMask = isBishop ? bishopMasks[sq] : rookMasks[sq];
            int bits = CountBits(attackMask);
            int occupancyIndex = (1 << bits);
            for (int i = 0; i < occupancyIndex; i++) {
                if (isBishop) {
                    ulong occupancy = GetOccuupancy(i, bits, attackMask);
                    ulong magicIndex = (occupancy * BishopMagicNumbers[sq]) >> (64 - bishopOccupancyBits[sq]);
                    bishopAttacksTable[sq, magicIndex] = BishopAttacks(sq, occupancy);
                } else {
                    ulong occupancy = GetOccuupancy(i, bits, attackMask);
                    ulong magicIndex = (occupancy * RookMagicNumbers[sq]) >> (64 - rookOccupancyBits[sq]);
                    rookAttacksTable[sq, magicIndex] = RookAttacks(sq, occupancy);
                }
            }
        }
    }

    private static void GenMagicNumbers() {
        Stopwatch s = new Stopwatch(); s.Start();
        string st = "";
        for (int i = 0; i < 64; i++) {
            st += (GenMagicNumbers(i, bishopOccupancyBits[i], true) + ",\n");
        }
        s.Stop();
        UnityEngine.Debug.Log(st);
        UnityEngine.Debug.Log(s.ElapsedMilliseconds);
    }

    private static ulong GenMagicNumbers(int sq, int bits, bool isBishop) {
        ulong attackMask;
        ulong[] occupancies = new ulong[4096];
        ulong[] attacks = new ulong[4096];
        ulong[] usedAttacks = new ulong[4096];
        ulong magicCandidate;
        int magicIndex, i;
        bool fail;

        attackMask = isBishop ? BishopAttackMap(sq) : RookAttackMap(sq);
        int attackBits = CountBits(attackMask);

        for (i = 0; i < (1 << attackBits); i++) {
            occupancies[i] = GetOccuupancy(i, attackBits, attackMask);
            attacks[i] = isBishop ? BishopAttacks(sq, occupancies[i]) : RookAttacks(sq, occupancies[i]);
        }
        for (int count = 0; count < 100000000; count++) {
            magicCandidate = GenMagicCandidate();
            if (CountBits((attackMask * magicCandidate) & 0xFF00000000000000UL) < 6) continue;
            for (i = 0; i < 4096; i++) usedAttacks[i] = 0UL;
            for (i = 0, fail = false; !fail && i < (1 << attackBits); i++) {
                magicIndex = (int)((occupancies[i] * magicCandidate) >> (64 - bits));
                if (usedAttacks[magicIndex] == 0UL) {
                    usedAttacks[magicIndex] = attacks[i];
                } else {
                    if (usedAttacks[magicIndex] != attacks[i]) {
                        fail = true;
                    }
                }
            }
            if (!fail) return magicCandidate;
        }
        Console.WriteLine("***Failed***\n");
        return 0UL;
    }

    private static ulong GenMagicCandidate() {
        return (GetRandom64() & GetRandom64() & GetRandom64());
    }

    private static ulong GetOccuupancy(int index, int bits, ulong attackMap) {
        ulong occupancy = 0;
        for (int c = 0; c < bits; c++) {
            int sq = LsfbIndex(attackMap);
            attackMap = RemoveBit(attackMap, sq);
            if ((index & (1U << c)) != 0) {
                occupancy |= (1ul << sq);
            }
        }
        return occupancy;
    }

    private static ulong RookAttackMap(int startSq) {
        ulong rookOccupancyMask = 0;
        for (int dir = 0; dir < 4; dir++) {
            int dist = distances[startSq][dir];
            int off = offset[dir];
            for (int s = 1; s < dist; s++) {
                rookOccupancyMask |= (1ul << (startSq + off * s));
            }
        }
        return rookOccupancyMask;
    }

    private static ulong BishopAttackMap(int startSq) {
        ulong bishopOccupancyMask = 0;
        for (int dir = 4; dir < 8; dir++) {
            int dist = distances[startSq][dir];
            int off = offset[dir];
            for (int s = 1; s < dist; s++) {
                bishopOccupancyMask |= (1ul << (startSq + off * s));
            }
        }
        return bishopOccupancyMask;
    }

    private static ulong BishopAttacks(int startSq, ulong block) {
        ulong mask = 0;
        for (int dir = 4; dir < 8; dir++) {
            int dist = distances[startSq][dir];
            int off = offset[dir];
            for (int s = 1; s < dist + 1; s++) {
                int to = (startSq + off * s);
                mask |= (1ul << to);
                if (BitExists(block, to)) break;
            }
        }
        return mask;
    }

    private static ulong RookAttacks(int startSq, ulong block) {
        ulong mask = 0;
        for (int dir = 0; dir < 4; dir++) {
            int dist = distances[startSq][dir];
            int off = offset[dir];
            for (int s = 1; s < dist + 1; s++) {
                int to = (startSq + off * s);
                mask |= (1ul << to);
                if (BitExists(block, to)) break;
            }
        }
        return mask;
    }

    private static void GetDistances(int sq) {
        int file = sq / 8;
        int rank = (sq + 8) % 8;
        int north = 7 - file;
        int south = file;
        int east = rank;
        int west = 7 - rank;
        distances[sq] = new int[8];
        distances[sq][0] = north;
        distances[sq][1] = south;
        distances[sq][2] = east;
        distances[sq][3] = west;
        distances[sq][4] = Math.Min(north, east);
        distances[sq][5] = Math.Min(north, west);
        distances[sq][6] = Math.Min(south, east);
        distances[sq][7] = Math.Min(south, west);
    }

    private static void CalculateRays(int sq) {
        ulong[] raysq = new ulong[8];
        for (int dir = 0; dir < 8; dir++) {
            ulong ray = 0;
            for (int dist = 1; dist < distances[sq][dir] + 1; dist++) {
                ray |= 1ul << sq + (offset[dir] * dist);
            }
            raysq[dir] = ray;
        }
        rays[sq] = raysq;
    }

    private static void GetKnightMoves(int sq) {
        //probably better ways of doing this
        int[] c1 = new int[] { 0, 0, 1, 1, 2, 2, 3, 3 };
        int[] c2 = new int[] { 2, 3, 2, 3, 1, 0, 1, 0 };
        var legalMoves = new List<byte>();
        ulong knightBitboard = 0;
        for (int d = 0; d < 8; d++) {
            if (distances[sq][c1[d]] >= 2 && distances[sq][c2[d]] >= 1) {
                int knightMoveSq = sq + knightOffsets[d];
                legalMoves.Add((byte)knightMoveSq);
                knightBitboard |= 1ul << knightMoveSq;
            }
        }
        knightMoves[sq] = legalMoves.ToArray();
        knightAttackBitboards[sq] = knightBitboard;
    }

    private static void GetKingMoves(int sq) {
        var legalMoves = new List<byte>();
        for (int d = 0; d < 8; d++) {
            if (distances[sq][d] >= 1) {
                int kingMoveSq = sq + offset[d];
                legalMoves.Add((byte)kingMoveSq);
                kingAttackBitboards[sq] |= 1ul << kingMoveSq;
            }
        }
        kingMoves[sq] = legalMoves.ToArray();
    }

    private static void PawnAttacks(int sq) {
        List<int> legalAttackWhite = new List<int>();
        List<int> legalAttackBlack = new List<int>();
        pawnAttackBitboards[sq] = new ulong[2];
        for (int c = 4; c < 6; c++) {
            if (distances[sq][c] > 0) {
                legalAttackWhite.Add(sq + offset[c]);
                pawnAttackBitboards[sq][0] |= 1ul << sq + offset[c];
            }
        }
        for (int c = 6; c < 8; c++) {
            if (distances[sq][c] > 0) {
                legalAttackBlack.Add(sq + offset[c]);
                pawnAttackBitboards[sq][1] |= 1ul << sq + offset[c];
            }
        }
        pawnAttackBlack[sq] = legalAttackBlack.ToArray();
        pawnAttackWhite[sq] = legalAttackWhite.ToArray();
    }
}