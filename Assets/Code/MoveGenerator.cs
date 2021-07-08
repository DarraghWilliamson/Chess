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
    private int castling, EnpassantFile;
    private bool inCheck, inDoubleCheck, pinExists;
    private const ulong debruijn64 = (0x03f79d71b4cb0a89);

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
        enemyAttackBitmap, checkBitmap, turnPieces, enemyPieces, pinBitboard, allPieces;

    public List<int> GenerateMoves(Board board) {
        shortMoves = new List<int>();
        this.board = board;
        inCheck = false;
        inDoubleCheck = false;
        pinExists = false;
        EnpassantFile = ((board.currentGameState >> 4) & 15);
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
        pinBitboard = 0;
        enemyPieces = 0;
        turnPieces = 0;
        checkBitmap = 0;
        allPieces = 0;
        slidingAttackBitmap = 0;
        enemyPieces = board.pawnsBoard[enemyColour] | board.knightsBoard[enemyColour] | board.bishopsBoard[enemyColour] | board.rooksBoard[enemyColour] | board.queensBoard[enemyColour] | board.kingsBoard[enemyColour];
        turnPieces = board.pawnsBoard[turnColour] | board.knightsBoard[turnColour] | board.bishopsBoard[turnColour] | board.rooksBoard[turnColour] | board.queensBoard[turnColour] | board.kingsBoard[turnColour];
        allPieces = enemyPieces | turnPieces;
        GenChecksAndPins();
        GetSlideAttack();
        GetKnightAttack();
        GetPawnAttack();
        enemyAttackBitmapNoPawns = slidingAttackBitmap | knightAttackBitmap | kingAttackBitboards[board.kings[enemyColour]];
        enemyAttackBitmap = enemyAttackBitmapNoPawns | pawnAttackBitmap;

        board.bitmatSquares.Clear();
        for (int i = 0; i < 64; i++) {
            if (BitExists(slidingAttackBitmap, i)) {
                board.bitmatSquares.Add(i);
            }
        }
    }

    private void GenChecksAndPins() {
        GetChecksAndPins(0, 4, (board.queensBoard[enemyColour] | board.rooksBoard[enemyColour]));
        GetChecksAndPins(4, 8, (board.queensBoard[enemyColour] | board.bishopsBoard[enemyColour]));
    }

    private void GetChecksAndPins(int start, int end, ulong enemyBoard) {
        //look for enemy
        for (int dir = start; dir < end; dir++) {
            ulong kingRay = rays[myKing][dir];
            //if enemy in direction
            ulong sliders = kingRay & enemyBoard;
            if (sliders != 0) {
                int slidersCount = CountBits(sliders);
                for (int s = 0; s < slidersCount; s++) {
                    int sliderPos = LsfbIndex(sliders);
                    sliders = RemoveBit(sliders, sliderPos);
                    ulong ray = kingRay & rays[sliderPos][inverse[dir]];
                    //if ray blocked by enemy piece continue
                    if ((ray & enemyPieces) != 0) {
                        continue;
                    }
                    if ((ray & turnPieces) != 0) {
                        //if blocked by single friendly piece
                        if (CountBits(ray & turnPieces) == 1) {
                            pinExists = true;
                            ray |= 1ul << sliderPos;
                            pinBitboard |= ray;
                        }
                        continue;
                    }
                    //if not blocked
                    inDoubleCheck = inCheck;
                    inCheck = true;
                    int behindKing = myKing + offset[inverse[dir]];
                    ray |= 1ul << sliderPos;
                    slidingAttackBitmap |= 1ul << behindKing;
                    checkBitmap |= ray;
                }
            }
        }
    }

    private void GetMoves() {
        GetKingMoves();
        if (inDoubleCheck) return; //if in double check only king moves are valid
        GetPawnMoves();
        GetKnightMoves();
        GetSlidingMoves();
    }

    private void GetSlideAttack() {
        PieceList rookPieces = board.rooksList[enemyColour];
        PieceList bishopPieces = board.bishopsList[enemyColour];
        PieceList queenPieces = board.queensList[enemyColour];
        for (int i = 0; i < rookPieces.length; i++) {
            slidingAttackBitmap |= GetRookAttacks(rookPieces[i], allPieces);
        }
        for (int i = 0; i < bishopPieces.length; i++) {
            slidingAttackBitmap |= GetBishopAttacks(bishopPieces[i], allPieces);
        }
        for (int i = 0; i < queenPieces.length; i++) {
            slidingAttackBitmap |= (GetRookAttacks(queenPieces[i], allPieces) | GetBishopAttacks(queenPieces[i], allPieces));
        }
    }

    private void GetKnightAttack() {
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

    private void GetPawnAttack() {
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
                shortMoves.Add(CreateMove(myKing, jumps[i]));
            }
        }
        if (!inCheck) {
            if (turnColour == 0) {
                if (CanCastle(0) && (squares[5] + squares[6] == 0)) {
                    if (CheckSquare(6) && CheckSquare(5)) {
                        shortMoves.Add(CreateMoveShort(myKing, 6, 2));
                    }
                }
                if (CanCastle(1) && (squares[3] + squares[2] + squares[1] == 0)) {
                    if (CheckSquare(2) && CheckSquare(3)) {
                        shortMoves.Add(CreateMoveShort(myKing, 2, 2));
                    }
                }
            } else {
                if (CanCastle(2) && (squares[61] + squares[62] == 0)) {
                    if (CheckSquare(62) && CheckSquare(61)) {
                        shortMoves.Add(CreateMoveShort(myKing, 62, 2));
                    }
                }
                if (CanCastle(3) && (squares[59] + squares[58] + squares[57] == 0)) {
                    if (CheckSquare(58) && CheckSquare(59)) {
                        shortMoves.Add(CreateMoveShort(myKing, 58, 2));
                    }
                }
            }
        }
    }

    private bool CanCastle(int pos) {
        return ((castling >> pos) & 1) != 0;
    }

    private ulong ShiftBoard(ulong board, int shift) {
        switch (shift) {
            case 8: return board << 8;
            case 16: return board << 16;
            case 1: return board << 1;
            case 9: return board << 9;
            case 7: return board << 7;
            case -8: return board >> 8;
            case -16: return board >> 16;
            case -1: return board >> 1;
            case -9: return board >> 9;
            case -7: return board >> 7;
            default: return board;
        }
    }

    private void MakePromotionMoves(int from, int to) {
        shortMoves.Add(CreatePromotionMoveShort(from, to, 0));
        shortMoves.Add(CreatePromotionMoveShort(from, to, 1));
        shortMoves.Add(CreatePromotionMoveShort(from, to, 2));
        shortMoves.Add(CreatePromotionMoveShort(from, to, 3));
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
                            shortMoves.Add(CreateMove(startSq, endSq));
                        }
                    }
                    //if can move double and sq empty
                    if (IsDoubleMove && squares[endSq + pawnOffset] == 0) {
                        if (!inCheck || InCheckMap(endSq + pawnOffset)) {
                            shortMoves.Add(CreateMove(startSq, endSq + pawnOffset));
                        }
                    }
                }
            }
            //Pawn Captures
            for (int d = 0; d < 2; d++) {
                endSq = startSq + offset[diagnolOffset[d]];
                if (distances[startSq][diagnols[d]] > 0) {
                    if (!IsPinned(startSq) || IsMovingAlongRay(startSq, diagnolOffset[d])) {
                        EnpassCapture(startSq, endSq);
                        if (!inCheck || InCheckMap(endSq)) {
                            if (squares[endSq] != 0 && Piece.Colour(squares[endSq]) != turnColour) {
                                if (promotionMove) {
                                    shortMoves.Add(CreatePromotionMoveShort(startSq, endSq, 0));
                                    shortMoves.Add(CreatePromotionMoveShort(startSq, endSq, 1));
                                    shortMoves.Add(CreatePromotionMoveShort(startSq, endSq, 2));
                                    shortMoves.Add(CreatePromotionMoveShort(startSq, endSq, 3));
                                } else {
                                    shortMoves.Add(CreateMove(startSq, endSq));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void EnpassCapture(int startSq, int endSq) {
        if (EnpassantFile == 0) return;
        if (distances[startSq][turnColour] != 3) return;
        if (EnpassantFile - 1 != (GetFile(endSq))) return;
        if (ValidateEnpassant(startSq)) {
            shortMoves.Add(CreateMoveShort((ushort)startSq, (ushort)endSq, 1));
        }
    }

    private bool ValidateEnpassant(int moveFrom) {
        bool valid = true;
        int enemyPieceCol = (enemyColour == 0) ? Piece.White : Piece.Black;
        int enemyRook = enemyPieceCol | Piece.Rook;
        int enemyQueen = enemyPieceCol | Piece.Queen;
        int EnPawnSquare = (turnColour == 0) ? EnpassantFile + 31 : EnpassantFile + 23;
        int from = squares[moveFrom];
        int enp = squares[EnPawnSquare];
        if (inCheck && !InCheckMap(EnPawnSquare)) return false;
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
                    shortMoves.Add(CreateMove(knight, kM[move]));
                }
            }
        }
    }

    private void GetSlidingMoves() {
        PieceList RookM = board.rooksList[turnColour];
        PieceList BishopM = board.bishopsList[turnColour];
        PieceList QueenM = board.queensList[turnColour];
        for (int i = 0; i < RookM.length; i++) SlideAttackMaps(RookM.pieces[i], Piece.Rook);
        for (int i = 0; i < BishopM.length; i++) SlideAttackMaps(BishopM.pieces[i], Piece.Bishop);
        for (int i = 0; i < QueenM.length; i++) SlideAttackMaps(QueenM.pieces[i], Piece.Queen);
    }

    private void SlideAttackMaps(int sq, int type) {
        ulong moveMap = 0;
        switch (type) {
            case Piece.Bishop: moveMap = GetBishopAttacks(sq, allPieces); break;
            case Piece.Rook: moveMap = GetRookAttacks(sq, allPieces); break;
            case Piece.Queen: moveMap = (GetRookAttacks(sq, allPieces) | GetBishopAttacks(sq, allPieces)); break;
        }
        if (IsPinned(sq)) moveMap &= GetPinMovementRay(sq);
        if (inCheck) moveMap &= checkBitmap;
        if (moveMap == 0) return;
        moveMap &= ~turnPieces;
        int moveCount = CountBits(moveMap);
        for (int c = 0; c < moveCount; c++) {
            int moveSq = LsfbIndex(moveMap);
            moveMap = RemoveBit(moveMap, moveSq);
            shortMoves.Add(CreateMove(sq, moveSq));
        }
    }

    private ulong GetPinMovementRay(int sq) {
        bool kingFound = false;
        ulong ray = 0;
        for (int dir = 0; dir < 8; dir++) {
            ray = rays[sq][dir];
            if ((ray & board.kingsBoard[turnColour]) != 0) {
                ray |= rays[sq][inverse[dir]];
                kingFound = true;
                break;
            }
        }
        if (kingFound) {
            return ray;
        } else {
            return 0;
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