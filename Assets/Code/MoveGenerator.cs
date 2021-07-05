using System.Collections.Generic;
using UnityEngine;
using static PreComputedMoves;
using static Utils;

public class MoveGenerator {
    private Board board;
    private List<int> shortMoves;

    private int turnColour, enemyColour;
    private int myKing;
    private int[] squares;
    private int castling, enpass;
    private bool inCheck, inDoubleCheck, pinExists;
    private const ulong debruijn64 = (0x03f79d71b4cb0a89);

    //const ulong deBruijn64 = 0x03f79d71b4cb0a89UL;
    public static readonly byte[] index64 = {
         0, 47,  1, 56, 48, 27,  2, 60,
         57, 49, 41, 37, 28, 16,  3, 61,
         54, 58, 35, 52, 50, 42, 21, 44,
         38, 32, 29, 23, 17, 11,  4, 62,
         46, 55, 26, 59, 40, 36, 15, 53,
         34, 51, 20, 43, 31, 22, 10, 45,
         25, 39, 14, 33, 19, 30,  9, 24,
         13, 18,  8, 12,  7,  6,  5, 63
    };

    private ulong slidingAttackBitmap, knightAttackBitmap, pawnAttackBitmap, enemyAttackBitmapNoPawns,
        enemyAttackBitmap, checkBitmap, turnPieces, enemyPieces, pinBitboard;

    public List<int> GenerateMoves(Board board) {
        shortMoves = new List<int>();
        this.board = board;
        inCheck = false;
        inDoubleCheck = false;
        pinExists = false;
        enpass = ((board.currentGameState >> 4) & 15);
        castling = board.currentGameState & 15;
        turnColour = board.turnColour;
        squares = board.squares;
        enemyColour = turnColour == 0 ? 1 : 0;
        myKing = (ushort)board.kings[turnColour];

        GenerateBitboards();
        GetMoves();

        board.inCheck = inCheck;
        return shortMoves;
    }

    private void GenerateBitboards() {
        enemyPieces = 0;
        turnPieces = 0;
        checkBitmap = 0;
        enemyPieces = board.pawnsBoard[enemyColour] | board.knightsBoard[enemyColour] | board.bishopsBoard[enemyColour] | board.rooksBoard[enemyColour] | board.queensBoard[enemyColour] | board.kingsBoard[enemyColour];
        turnPieces = board.pawnsBoard[turnColour] | board.knightsBoard[turnColour] | board.bishopsBoard[turnColour] | board.rooksBoard[turnColour] | board.queensBoard[turnColour] | board.kingsBoard[turnColour];

        GenBitboardSlide();
        GenBitboardKnight();
        GenBitboardPawn();
        enemyAttackBitmapNoPawns = slidingAttackBitmap | knightAttackBitmap | kingAttackBitboards[board.kings[enemyColour]];
        enemyAttackBitmap = enemyAttackBitmapNoPawns | pawnAttackBitmap;
    }

    private void GetMoves() {
        GetKingMoves();
        if (inDoubleCheck) return; //if in double check only king moves are valid
        GetPawnMoves();
        GetKnightMoves();
        RookBishopQueen();
    }

    private void GenBitboardSlide() {
        slidingAttackBitmap = 0;
        pinBitboard = 0;
        PieceList rookPieces = board.rooksList[enemyColour];
        PieceList bishopPieces = board.bishopsList[enemyColour];
        PieceList queenPieces = board.queensList[enemyColour];

        for (int i = 0; i < rookPieces.length; i++) AddToBitboardSlide(rookPieces[i], 0, 4);
        for (int i = 0; i < bishopPieces.length; i++) AddToBitboardSlide(bishopPieces[i], 4, 8);
        for (int i = 0; i < queenPieces.length; i++) AddToBitboardSlide(queenPieces[i], 0, 8);
    }

    //creates slide attack, pin and check pitboards
    //shoud be re-done later for better efficency
    private void AddToBitboardSlide(int startSq, int start, int end) {
        for (int dir = start; dir < end; dir++) {
            ulong ray = rays[startSq][dir];

            if (ray == 0) continue;
            if ((ray & board.kingsBoard[turnColour]) != 0) {
                ulong kingRay = ray & rays[myKing][inverse[dir]];
                if ((kingRay & enemyPieces) == 0) {
                    //if no pieces between them, set check
                    if ((kingRay & turnPieces) == 0) {
                        inDoubleCheck = inCheck;
                        inCheck = true;
                        kingRay |= 1ul << startSq;
                        checkBitmap |= kingRay;
                    } else {
                        //if only a friendly piece, set pin
                        if (BitScan(kingRay & turnPieces).Length == 1) {
                            int pin;
                            if (dir == 0 || dir == 3 || dir == 4 || dir == 5) { //assending index
                                pin = (int)BitScan(kingRay)[0];
                            } else { //decending index
                                byte[] t = BitScan(kingRay);
                                pin = (int)t[t.Length - 1];
                            }
                            ulong pinRay = kingRay & rays[pin][inverse[dir]];
                            pinExists = true;
                            kingRay |= 1ul << startSq;
                            pinRay |= 1ul << startSq;
                            pinBitboard |= kingRay;
                            slidingAttackBitmap |= pinRay;
                            continue;
                        }
                    }
                }
            }
            ulong blocks = ray & (turnPieces | enemyPieces);
            blocks &= ~board.kingsBoard[turnColour];
            //if no blocks add moves
            if (blocks == 0) {
                byte[] path = BitScan(ray);
                for (int i = 0; i < path.Length; i++) {
                    slidingAttackBitmap |= 1ul << (int)path[i];
                }
                continue;
            }
            int block;
            //if block exists
            if (dir == 0 || dir == 3 || dir == 4 || dir == 5) { //assending index
                block = (int)BitScan(blocks)[0];
            } else { //decending index
                byte[] t = BitScan(blocks);
                block = (int)t[t.Length - 1];
            }
            //create ray from block to attacker and add moves
            ulong attackRay = ray & rays[block][inverse[dir]];
            attackRay |= 1ul << block;
            slidingAttackBitmap |= attackRay;
        }
    }

    private void GenBitboardKnight() {
        PieceList knightPieces = board.knightsList[enemyColour];
        knightAttackBitmap = 0;
        for (int i = 0; i < knightPieces.length; i++) {
            knightAttackBitmap |= knightAttackBitboards[knightPieces.pieces[i]];
            if (BitExists(knightAttackBitboards[knightPieces.pieces[i]], myKing)) {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkBitmap |= 1ul << knightPieces.pieces[i];
            }
        }
    }

    private void GenBitboardPawn() {
        PieceList pawnPieces = board.pawnsList[enemyColour];
        pawnAttackBitmap = 0;
        for (int i = 0; i < pawnPieces.length; i++) {
            pawnAttackBitmap |= pawnAttackBitboards[pawnPieces.pieces[i]][enemyColour];
            if (BitExists(pawnAttackBitboards[pawnPieces.pieces[i]][enemyColour], myKing)) {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkBitmap |= 1ul << pawnPieces.pieces[i];
            }
        }
    }

    private void GetKingMoves() {
        ulong kingPiece = kingAttackBitboards[myKing];
        kingPiece &= ~turnPieces;
        kingPiece &= ~enemyAttackBitmap;
        byte[] jumps = BitScan(kingPiece);
        if (kingPiece != 0) {
            for (int i = 0; i < jumps.Length; i++) {
                shortMoves.Add(CreateMoveShort(myKing, jumps[i]));
            }
        }
        if (!inCheck) {
            if (turnColour == 0) {
                //ushort castle = GetCastling();
                if (CanCastle(castling, 0) && (squares[5] + squares[6] == 0)) {
                    if (CheckSquare(6) && CheckSquare(5)) {
                        shortMoves.Add(CreateMoveShort(myKing, 6, 2));
                    }
                }
                if (CanCastle(castling, 1) && (squares[3] + squares[2] + squares[1] == 0)) {
                    if (CheckSquare(2) && CheckSquare(3)) {
                        shortMoves.Add(CreateMoveShort(myKing, 2, 2));
                    }
                }
            } else {
                if (CanCastle(castling, 2) && (squares[61] + squares[62] == 0)) {
                    if (CheckSquare(62) && CheckSquare(61)) {
                        shortMoves.Add(CreateMoveShort(myKing, 62, 2));
                    }
                }
                if (CanCastle(castling, 3) && (squares[59] + squares[58] + squares[57] == 0)) {
                    if (CheckSquare(58) && CheckSquare(59)) {
                        shortMoves.Add(CreateMoveShort(myKing, 58, 2));
                    }
                }
            }
        }
    }

    private void GetPawnMoves() {
        int pawnOffset = offset[turnColour];
        int[] diagnols = (turnColour == 0) ? new int[] { 6, 7 } : new int[] { 4, 5 };
        int[] diagnolOffset = (turnColour == 0) ? new int[] { 4, 5 } : new int[] { 6, 7, };
        PieceList pawnPieces = board.pawnsList[turnColour];

        for (int i = 0; i < pawnPieces.length; i++) {
            int startSq = pawnPieces.pieces[i];
            if (Piece.Type(squares[startSq]) != 2) {
                Debug.Log(Piece.Type(squares[startSq]));
            }
            bool IsDoubleMove = distances[startSq][enemyColour] == 1;
            bool promotionMove = distances[startSq][turnColour] == 1;
            int endSq = startSq + pawnOffset;

            //Pawn Push
            if (squares[endSq] == 0) {
                if (!IsPinned(startSq) || IsMovingAlongRay(startSq, turnColour)) {
                    if (!inCheck || InCheckMap(endSq)) {
                        if (promotionMove) { // 0:Knight, 1:bishop, 2:rook, 3:queen
                            shortMoves.Add(CreatePromotionMoveShort(startSq, endSq, 0));
                            shortMoves.Add(CreatePromotionMoveShort(startSq, endSq, 1));
                            shortMoves.Add(CreatePromotionMoveShort(startSq, endSq, 2));
                            shortMoves.Add(CreatePromotionMoveShort(startSq, endSq, 3));
                        } else {
                            shortMoves.Add(CreateMoveShort(startSq, endSq));
                        }
                    }
                    //if can move double and sq empty
                    if (IsDoubleMove && squares[endSq + pawnOffset] == 0) {
                        endSq += pawnOffset;
                        if (!inCheck || InCheckMap(endSq)) {
                            shortMoves.Add(CreateMoveShort(startSq, endSq));
                        }
                    }
                }
            }
            //Pawn Captures
            for (int d = 0; d < 2; d++) {
                endSq = startSq + offset[diagnolOffset[d]];
                if (distances[startSq][diagnols[d]] > 0) {
                    if (!IsPinned(startSq) || IsMovingAlongRay(startSq, diagnolOffset[d])) {
                        if (!inCheck || InCheckMap(endSq)) {
                            EnpassCapture(startSq, endSq);
                            if (squares[endSq] != 0 && Piece.Colour(squares[endSq]) != turnColour) {
                                if (promotionMove) {
                                    shortMoves.Add(CreatePromotionMoveShort(startSq, endSq, 0));
                                    shortMoves.Add(CreatePromotionMoveShort(startSq, endSq, 1));
                                    shortMoves.Add(CreatePromotionMoveShort(startSq, endSq, 2));
                                    shortMoves.Add(CreatePromotionMoveShort(startSq, endSq, 3));
                                } else {
                                    shortMoves.Add(CreateMoveShort(startSq, endSq));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void EnpassCapture(int startSq, int endSq) {
        if (enpass == 0) return;
        if (distances[startSq][turnColour] != 3) return;
        if (enpass - 1 != (GetFile(endSq))) return;
        if (ValidateEnpassant(startSq)) {
            shortMoves.Add(CreateMoveShort((ushort)startSq, (ushort)endSq, 1));
        }
    }

    private bool ValidateEnpassant(int moveFrom) {
        bool valid = true;
        int enemyPieceCol = (enemyColour == 0) ? Piece.White : Piece.Black;
        int enemyRook = enemyPieceCol | Piece.Rook;
        int enemyQueen = enemyPieceCol | Piece.Queen;
        int EnPawnSquare = (turnColour == 0) ? enpass + 31 : enpass + 23;
        int from = squares[moveFrom];
        int enp = squares[EnPawnSquare];
        squares[moveFrom] = 0;
        squares[EnPawnSquare] = 0;
        //test if move reveals a check
        for (int d = 2; d <= 3; d++) {
            if ((rays[moveFrom][d] & board.kingsBoard[turnColour]) != 0) {
                int dist = distances[myKing][inverse[d]];
                int off = offset[inverse[d]];
                for (int x = 1; x < dist + 1; x++) {
                    int addition = (off * x);
                    if (squares[myKing + addition] != 0) {
                        if (squares[myKing + addition] == enemyRook || squares[myKing + addition] == enemyQueen) {
                            valid = false;
                            break;
                        } else {
                            squares[moveFrom] = from;
                            squares[EnPawnSquare] = enp;
                            return valid;
                        }
                    }
                }
            }
        }
        squares[moveFrom] = from;
        squares[EnPawnSquare] = enp;
        return valid;
    }

    private void GetKnightMoves() {
        PieceList KnightM = board.knightsList[turnColour];
        for (int i = 0; i < KnightM.length; i++) {
            int knight = KnightM.pieces[i];

            if (IsPinned(knight)) continue;
            ulong nMoves = knightAttackBitboards[KnightM[i]];
            nMoves &= ~turnPieces;
            if (inCheck) nMoves &= checkBitmap;
            byte[] kM = knightMoves[knight];
            for (int move = 0; move < kM.Length; move++) {
                if (BitExists(nMoves, kM[move])) {
                    shortMoves.Add(CreateMoveShort(knight, kM[move]));
                }
            }
        }
    }

    private void RookBishopQueen() {
        //if (inDoubleCheck) return;
        PieceList RookM = board.rooksList[turnColour];
        PieceList BishopM = board.bishopsList[turnColour];
        PieceList QueenM = board.queensList[turnColour];
        for (int i = 0; i < RookM.length; i++) GetSlideMoves(RookM.pieces[i], 0, 4);
        for (int i = 0; i < BishopM.length; i++) GetSlideMoves(BishopM.pieces[i], 4, 8);
        for (int i = 0; i < QueenM.length; i++) GetSlideMoves(QueenM.pieces[i], 0, 8);
    }

    private void GetSlideMoves(int startSq, int start, int end) {
        for (int dir = start; dir < end; dir++) {
            for (int i = 1; i < distances[startSq][dir] + 1; i++) {
                int moveTo = startSq + (offset[dir] * i);

                //piece exists
                if (squares[moveTo] != 0) {
                    //if piece is a enemy piece
                    if (Piece.Colour(squares[moveTo]) != turnColour) {
                        //if not in check or this move removes check
                        if (!inCheck || InCheckMap(moveTo)) {
                            //if not in pinned or moving along pin
                            if (!IsPinned(startSq) || IsMovingAlongRay(startSq, dir)) {
                                shortMoves.Add(CreateMoveShort((ushort)startSq, (ushort)moveTo));
                            }
                        }
                    }
                    break;
                } else {
                    //if not in check or this move removes check
                    if (!inCheck || InCheckMap(moveTo)) {
                        //if not in pinned or moving along pin
                        if (!IsPinned(startSq) || IsMovingAlongRay(startSq, dir)) {
                            shortMoves.Add(CreateMoveShort(startSq, moveTo));
                        }
                    }
                }
            }
        }
    }

    private bool CheckSquare(int sq) {
        return !BitExists(enemyAttackBitmap, sq);
    }

    private bool IsMovingAlongRay(int square, int dir) {
        ulong ray = rays[square][dir] | rays[square][inverse[dir]];
        ulong kingInRay = board.kingsBoard[turnColour] & ray;
        return kingInRay != 0;
    }

    private bool IsPinned(int square) {
        return pinExists && ((pinBitboard >> square) & 1) != 0;
    }

    private bool InCheckMap(int square) {
        return inCheck && ((checkBitmap >> square) & 1) != 0;
    }

    //takes a bitboard and returns a list of bytes for each non zero entry
    //https://stackoverflow.com/questions/24798499/chess-bitscanning?rq=1
    public static byte[] BitScan(ulong bitboard) {
        if (bitboard == 0) return null;
        var indices = new byte[28];
        var index = 0;
        while (bitboard != 0) {
            indices[index++] = index64[((bitboard ^ (bitboard - 1)) * debruijn64) >> 58];
            bitboard &= bitboard - 1;
        }
        //if no result return null to
        byte[] a = new byte[index];
        for (int i = 0; i < index; i++) a[i] = indices[i];
        return a;
    }
}