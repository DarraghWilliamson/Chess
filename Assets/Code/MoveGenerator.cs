using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;

using static PreComputedMoves;
public class MoveGenerator {
    Board board;
    List<Move> moves = new List<Move>();

    const int whiteColourIndex = 0;
    int turnColour, enemyColour, enemyPieceCol, myKing;
    int[] squares;
    bool inCheck;
    bool inDoubleCheck;
    bool pinExists;
    bool[] Castling;

    public List<int> bitmatSquares = new List<int>(); //public for test method
    Dictionary<int, List<int>> pinnedPieces;
    int[] pins;
    ulong[] pinBitboard;
    ulong SlidingAttackBitmap, KnightAttackBitmap, PawnAttackBitmap, enemyAttackBitmapNoPawns, enemyAttackBitmap, checkBitmap;

    public List<Move> GenerateMoves(Board board) {
        inCheck = false;
        inDoubleCheck = false;
        pinExists = false;
        this.board = board;
        pinnedPieces = new Dictionary<int, List<int>>();
        inCheck = false;
        inDoubleCheck = false;
        moves = new List<Move>();
        pins = new int[8];
        Castling = board.Castling;
        turnColour = board.turnColour;
        enemyColour = turnColour == 0 ? 1 : 0;
        enemyPieceCol = (enemyColour == whiteColourIndex) ? Piece.White : Piece.Black;
        squares = board.squares;
        myKing = board.kings[turnColour];
        GenAttackBitboards();
        GetKingMoves();
        GetPawnMoves();
        GetKnightMoves();
        RookBishopQueen();

        board.inCheck = inCheck;

        bitmatSquares.Clear();
        for (int i = 0; i < 64; i++) {
            if (BitboardContains(checkBitmap, i)) {
                bitmatSquares.Add(i);
            }
        }

        return moves;
    }

    bool IsEnemy(int piece, int sq) {
        return Piece.Colour(piece) != sq;
    }

    void GetKingMoves() {
        int sq = board.kings[turnColour];
        for (int m = 0; m < kingMoves[sq].Length; m++) {
            int to = kingMoves[sq][m];
            if (squares[to] != 0) {
                if (IsEnemy(squares[to], turnColour)) {
                    if (CheckSquare(to)) {
                        moves.Add(new Move(sq, to));
                    }
                    continue;
                }
            } else {
                if (CheckSquare(to)) {
                    moves.Add(new Move(sq, to));
                }
            }
        }
        //Castling logic
        if (!inCheck) {
            if (turnColour == whiteColourIndex) {
                if (Castling[0] && Piece.Empty(squares[5]) && Piece.Empty(squares[6])) {
                    if (CheckSquare(6) && CheckSquare(5)) moves.Add(new Move(sq, 6, 2));
                }
                if (Castling[1] && Piece.Empty(squares[3]) && Piece.Empty(squares[2]) && Piece.Empty(squares[1])) {
                    if (CheckSquare(2) && CheckSquare(3)) moves.Add(new Move(sq, 2, 2));
                }
            } else {
                if (Castling[2] && Piece.Empty(squares[61]) && Piece.Empty(squares[62])) {
                    if (CheckSquare(62) && CheckSquare(61)) moves.Add(new Move(sq, 62, 2));
                }
                if (Castling[3] && Piece.Empty(squares[59]) && Piece.Empty(squares[58]) && Piece.Empty(squares[57])) {
                    if (CheckSquare(58) && CheckSquare(59)) moves.Add(new Move(sq, 58, 2));
                }
            }
        }
    }

    //returns true if square is safe
    bool CheckSquare(int sq) {
        return !BitboardContains(enemyAttackBitmap, sq);
    }

    void GetPawnMoves() {
        int offset = (turnColour == whiteColourIndex) ? 8 : -8;
        //{ 8, -8, -1, 1, 7, 9, -9, -7 };
        int[] diagnols = (turnColour == whiteColourIndex) ? new int[] { 6, 7 } : new int[] { 4, 5 };
        int[] diagnolOffset = (turnColour == whiteColourIndex) ? new int[] { 7, 9 } : new int[] { -9, -7, };
        List<int> pawnList = board.pawns[turnColour];
        for (int i = 0; i < pawnList.Count; i++) {
            int pawnSquare = pawnList[i];
            bool doubleMove = distances[pawnSquare][enemyColour] == 1;
            bool promotionMove = distances[pawnSquare][turnColour] == 1;
            int moveTo = pawnSquare + offset;
            //if sqare empty
            if (squares[moveTo] == 0) {
                //if not pinned or is moving in pin
                if (!pinExists || (!pinnedPieces.ContainsKey(pawnSquare) || pinnedPieces[pawnSquare].Contains(moveTo))) {
                    //if not in check or is moving to stop check
                    if (!inCheck || InCheckMap(moveTo)) {
                        if (promotionMove) {
                            moves.Add(new Move(pawnSquare, moveTo, 4));
                            moves.Add(new Move(pawnSquare, moveTo, 5));
                            moves.Add(new Move(pawnSquare, moveTo, 6));
                            moves.Add(new Move(pawnSquare, moveTo, 7));
                        } else {
                            moves.Add(new Move(pawnSquare, moveTo));
                        }
                    }
                    //if can move double and sq empty
                    if (doubleMove && squares[moveTo + offset] == 0) {
                        //recheck check, no need to recheck pin
                        if (!inCheck || InCheckMap(moveTo + offset)) {
                            moves.Add(new Move(pawnSquare, moveTo + offset, Move.Flag.PawnDoubleMove));
                        }
                    }
                }
            }
            //check pawn captures
            for (int d = 0; d < 2; d++) {
                moveTo = pawnSquare + diagnolOffset[d];
                if (distances[pawnSquare][diagnols[d]] > 0) {
                    //recheck pins
                    if (!pinnedPieces.ContainsKey(pawnSquare) || pinnedPieces[pawnSquare].Contains(moveTo)) {
                        //recheck check
                        int eset = (turnColour == whiteColourIndex) ? -8 : 8;
                        if (BitboardContains(checkBitmap, board.Enpassant + eset)) {
                            //Enpass capture
                            if (moveTo == board.Enpassant && ValidateEnpassant(pawnSquare, moveTo)) {
                                moves.Add(new Move(pawnSquare, moveTo, Move.Flag.EnPassantCapture));
                            }
                        }
                        if (!inCheck || InCheckMap(moveTo)) {
                            //Enpass capture
                            if (board.Enpassant != 0 && moveTo == board.Enpassant && ValidateEnpassant(pawnSquare, moveTo)) {
                                moves.Add(new Move(pawnSquare, moveTo, Move.Flag.EnPassantCapture));
                            }
                            //regular capture
                            if (squares[moveTo] != 0 && Piece.Colour(squares[moveTo]) != turnColour) {
                                if (promotionMove) {
                                    moves.Add(new Move(pawnSquare, moveTo, 4));
                                    moves.Add(new Move(pawnSquare, moveTo, 5));
                                    moves.Add(new Move(pawnSquare, moveTo, 6));
                                    moves.Add(new Move(pawnSquare, moveTo, 7));
                                } else {
                                    moves.Add(new Move(pawnSquare, moveTo));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    //check Enpassant capture is actualy legal
    bool ValidateEnpassant(int moveFrom, int moveTo) {
        int Enpassant = board.Enpassant;
        int EnPawnSquare = (Enpassant >= 16 && Enpassant <= 23) ? Enpassant + 8 : Enpassant - 8;
        //create board clone to test on
        int[] copy = (int[])squares.Clone();
        copy[EnPawnSquare] = 0;
        copy[moveTo] = copy[moveFrom];
        copy[moveFrom] = 0;
        //test if move puts player in check
        int sq = board.kings[turnColour];
        int enemyRook = enemyPieceCol | Piece.Rook;
        int enemyBishop = enemyPieceCol | Piece.Bishop;
        int enemyQueen = enemyPieceCol | Piece.Queen;
        for (int d = 0; d < 8; d++) {
            for (int i = 1; i < distances[sq][d] + 1; i++) {
                int to = sq + (offset[d] * i);
                if (copy[to] == 0) {
                    continue;
                }
                if (Piece.Colour(copy[to]) == turnColour) {
                    break;
                } else {
                    if ((d <= 3) && copy[to] == enemyRook) {
                        return false;
                    }
                    if ((d >= 4) && copy[to] == enemyBishop) {
                        return false;
                    }
                    if (copy[to] == enemyQueen) {
                        return false;
                    }
                    break;
                }
            }
        }
        return true;
    }
    void GetKnightMoves() {
        List<int> KnightM = board.knights[turnColour];
        for (int i = 0; i < KnightM.Count; i++) {
            int knight = KnightM[i];
            if (IsPinned(knight)) {
                continue;
            }

            for (int kn = 0; kn < knightMoves[knight].Length; kn++) {
                int to = knightMoves[knight][kn];
                if (!inCheck || InCheckMap(to)) {
                    if (squares[to] == 0 || Piece.Colour(squares[to]) != turnColour) {
                        moves.Add(new Move(knight, to));
                    }
                }
            }



        }
    }
    //get sliding piece moves
    void RookBishopQueen() {
        List<int> RookM = board.rooks[turnColour];
        List<int> BishopM = board.bishops[turnColour];
        List<int> QueenM = board.queens[turnColour];

        foreach (int sq in RookM) Slide(sq, 0, 4);
        foreach (int sq in BishopM) Slide(sq, 4, 8);
        foreach (int sq in QueenM) Slide(sq, 0, 8);
    }
    //get attacks for sliding pieces
    void Slide(int sq, int start, int end) {
        if (inCheck && pinnedPieces.ContainsKey(sq)) {
            return;
        }


        for (int d = start; d < end; d++) {
            for (int i = 1; i < distances[sq][d] + 1; i++) {
                int moveTo = sq + (offset[d] * i);
                //if blocked by friendly, stop searching in this direction
                if (squares[moveTo] != 0 && Piece.Colour(squares[moveTo]) == turnColour) {
                    break;
                }
                //if is pinned and move is not in pin
                if (pinnedPieces.ContainsKey(sq) && !pinnedPieces[sq].Contains(moveTo)) {
                    continue;
                }
                //if in check and this move dosnt remove check
                if (inCheck && !InCheckMap(moveTo)) {
                    continue;
                }
                //square empty
                if (squares[moveTo] == 0) {
                    moves.Add(new Move(sq, moveTo));
                    continue;
                } else {
                    //if not empty is blocked by enemy blocked by enemy
                    moves.Add(new Move(sq, moveTo));
                    break;

                }
            }
        }

    }

    void GenAttackBitboards() {
        checkBitmap = 0;
        GenBitboardSlide();
        GenBitboardPins();
        GenBitboardKnight();
        GenBitboardPawn();
        enemyAttackBitmapNoPawns = SlidingAttackBitmap | KnightAttackBitmap | kingAttackBitboards[board.kings[enemyColour]];
        enemyAttackBitmap = enemyAttackBitmapNoPawns | PawnAttackBitmap;
    }

    void GenBitboardPins() {
        pinBitboard = new ulong[8];
        int enemyRook = enemyPieceCol | Piece.Rook;
        int enemyBishop = enemyPieceCol | Piece.Bishop;
        int enemyQueen = enemyPieceCol | Piece.Queen;
        for (int d = 0; d < 8; d++) {
            List<int> pinTemp = new List<int>();
            int pinnedPos = 0;
            ulong tempBitmap = 0;
            int friendCount = 0;
            bool dangerPiece = false;

            for (int i = 1; i < distances[myKing][d] + 1; i++) {
                int newSquare = myKing + (offset[d] * i);
                if (squares[newSquare] == 0) {
                    pinTemp.Add(newSquare);
                    tempBitmap |= 1ul << newSquare;
                    continue;
                }
                if (Piece.Colour(squares[newSquare]) == turnColour) {
                    friendCount++;
                    pinnedPos = newSquare;
                    if (friendCount > 1) {
                        break;
                    }
                } else {
                    int enemyPiece = squares[newSquare];
                    if ((d <= 3) && enemyPiece == enemyRook || (d >= 4) && enemyPiece == enemyBishop || enemyPiece == enemyQueen) {
                        pinTemp.Add(newSquare);
                        tempBitmap |= 1ul << newSquare;
                        dangerPiece = true;
                    }
                    break;
                }
            }
            if (dangerPiece && friendCount == 0) {
                checkBitmap |= tempBitmap;
                inDoubleCheck = inCheck;
                inCheck = true;
            }
            if (dangerPiece && friendCount == 1) {
                pinExists = true;
                pinnedPieces.Add(pinnedPos, pinTemp);
            }
            if (inDoubleCheck) {
                break;
            }
        }
    }

    void GenBitboardPawn() {
        List<int> PawnM = board.pawns[enemyColour];
        PawnAttackBitmap = 0;
        for (int i = 0; i < PawnM.Count; i++) {
            PawnAttackBitmap |= pawnAttackBitboards[PawnM[i]][enemyColour];
            if (BitboardContains(pawnAttackBitboards[PawnM[i]][enemyColour], myKing)) {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkBitmap |= 1ul << PawnM[i];
            }
        }
    }

    void GenBitboardKnight() {
        List<int> KnightM = board.knights[enemyColour];
        KnightAttackBitmap = 0;
        for (int i = 0; i < KnightM.Count; i++) {
            KnightAttackBitmap |= knightAttackBitboards[KnightM[i]];
            if (BitboardContains(knightAttackBitboards[KnightM[i]], myKing)) {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkBitmap |= 1ul << KnightM[i];
            }
        }
    }

    void GenBitboardSlide() {
        SlidingAttackBitmap = 0;
        List<int> RookM = board.rooks[enemyColour];
        List<int> BishopM = board.bishops[enemyColour];
        List<int> QueenM = board.queens[enemyColour];
        foreach (int sq in RookM) SlideAttacks(sq, 0, 4);
        foreach (int sq in BishopM) SlideAttacks(sq, 4, 8);
        foreach (int sq in QueenM) SlideAttacks(sq, 0, 8);
    }

    void SlideAttacks(int sq, int start, int end) {
        for (int d = start; d < end; d++) {
            for (int i = 1; i < distances[sq][d] + 1; i++) {
                int moveTo = sq + (offset[d] * i);
                SlidingAttackBitmap |= 1ul << moveTo;
                if (moveTo != myKing) {
                    if (board.squares[moveTo] != 0) {
                        break;
                    }
                }
            }
        }
    }

    bool IsPinned(int square) {
        return pinExists && pinnedPieces.ContainsKey(square);
    }

    bool InCheckMap(int square) {
        return inCheck && ((checkBitmap >> square) & 1) != 0;
    }
    bool BitboardContains(ulong board, int sq) {
        return ((board >> sq) & 1) != 0;
    }






}
