using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveGenerator {
    List<Move> moves = new List<Move>();
    const int whiteColourIndex = 0;
    int turnColour, enemyColour, turnPieceCol, enemyPieceCol, king;
    int[] squares;
    bool inCheck;
    bool[] Castling;
    Board board;
    List<int> checkMoves = new List<int>();
    public List<int> SquaresUnderAttack = new List<int>(); //public for test method
    Dictionary<int, List<int>> pinnedPieces = new Dictionary<int, List<int>>();
    readonly int[] offset = new int[] { -8, 8, 1, -1, -7, -9, 9, 7 };
    

    public List<Move> GenerateMoves(Board board) {
        checkMoves = new List<int>();
        moves = new List<Move>();
        this.board = board;
        Castling = board.Castling;
        turnColour = board.turnColour;
        enemyColour = turnColour == 0 ? 1 : 0;
        enemyPieceCol = (enemyColour == whiteColourIndex) ? Piece.White : Piece.Black;
        squares = board.squares;
        turnPieceCol = (turnColour == whiteColourIndex) ? Piece.White : Piece.Black;
        king = turnPieceCol | Piece.King;
        GetSlideDangerSquares();
        inCheck = InCheckNonSlide();
        pinnedPieces = GetPinned();

        GetKingMoves();
        GetPawnMoves();
        GetKnightMoves();
        RookBishopQueen();

        if(inCheck) {
            if (checkMoves.Count == 0) {
                board.gameLogic.Check();
            } else {
                board.gameLogic.Checkmate();
            }
            
        }
        return moves;
    }
    //returns a int[] with distances to edge in each direction
    //south,north,west,east, sw,se,nw,ne
    int[] GetDistance(int loc) {
        int[] nsew = new int[8];
        nsew[0] = loc / 8;
        nsew[1] = 7 - nsew[0];
        loc += 8;
        nsew[3] = loc % 8;
        nsew[2] = 7 - nsew[3];
        nsew[4] = Math.Min(nsew[0], nsew[2]);
        nsew[5] = Math.Min(nsew[0], nsew[3]);
        nsew[6] = Math.Min(nsew[1], nsew[2]);
        nsew[7] = Math.Min(nsew[1], nsew[3]);
        return nsew;
    }
    //check if in check from non sliding piece
    bool InCheckNonSlide() {
        bool check = false;
        int sq = board.kings[turnColour];
        int[] distances = GetDistance(sq);
        //check for attacking pawns
        if (turnColour == whiteColourIndex) {
            if (distances[6] > 0 && squares[sq + 9] == 18) { checkMoves.Add(sq + 9); check = true; }
            if (distances[7] > 0 && squares[sq + 7] == 18) { checkMoves.Add(sq + 7); check = true; }
        } else {
            if (distances[4] > 0 && squares[sq - 7] == 10) { checkMoves.Add(sq -7); check = true; }
            if (distances[5] > 0 && squares[sq - 9] == 10) { checkMoves.Add(sq -9); check = true; }
        }
        //check for attacking knight
        int enemyKnight = enemyPieceCol | Piece.Knight;
        List<int> tiles = GetKnightTiles(sq);
        foreach (int to in tiles) {
            if (squares[to] == enemyKnight) {
                checkMoves.Add(to);
                check = true;
            }
        }
        if (SquaresUnderAttack.Contains(sq)) {
            check = true;
        }
        return check;
    }
    Dictionary<int, List<int>> GetPinned() {
        Dictionary<int, List<int>> pinned = new Dictionary<int, List<int>>();
        int sq = board.kings[turnColour];
        int enemyRook = enemyPieceCol | Piece.Rook;
        int enemyBishop = enemyPieceCol | Piece.Bishop;
        int enemyQueen = enemyPieceCol | Piece.Queen;

        int[] distances = GetDistance(sq);
        int[] offset = new int[] { -8, 8, 1, -1, -7, -9, 9, 7 };
        for (int d = 0; d < 8; d++) {
            List<int> Temp = new List<int>();
            int pinnedPos = 0;
            int friendCount = 0;
            bool slider = false;
            for (int i = 1; i < distances[d] + 1; i++) {
                int to = sq + (offset[d] * i);
                if (squares[to] == 0) {
                    Temp.Add(to);
                    continue;
                }
                if (Piece.Colour(squares[to]) == turnColour) {
                    friendCount++;
                    pinnedPos = to;
                    if (friendCount > 1) {
                        break;
                    }
                } else {
                    if ((d <= 3) && squares[to] == enemyRook) {
                        Temp.Add(to);
                        slider = true;
                    }
                    if ((d >= 4) && squares[to] == enemyBishop) {
                        Temp.Add(to);
                        slider = true;
                    }
                    if (squares[to] == enemyQueen) {
                        Temp.Add(to);
                        slider = true;
                    }
                    break;
                }
            }
            if (slider && friendCount == 0) {
                inCheck = true;
                checkMoves.AddRange(Temp);
            }
            if (slider && friendCount == 1) {
                pinned.Add(pinnedPos, Temp);
            }
        }
        return pinned;
    }



    void GetKingMoves() {
        int sq = board.kings[turnColour];
        int[] distances = GetDistance(sq);
        int[] offset = new int[] { -8, 8, 1, -1, -7, -9, 9, 7 };
        for (int d = 0; d < 8; d++) {
            if (distances[d] > 1) distances[d] = 1;
            for (int i = 1; i < distances[d] + 1; i++) {
                int to = sq + (offset[d] * i);
                if (squares[to] != 0) {
                    if (Piece.Colour(squares[to]) != turnColour) {
                        if (CheckSquare(to)) {
                            moves.Add(new Move(sq, to));
                        }
                    }
                    break;
                } else {
                    if (CheckSquare(to)) {
                        moves.Add(new Move(sq, to));
                    }
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

    //check square for king move
    bool CheckSquare(int sq) {
        if (SafeMove(sq) && !SquaresUnderAttack.Contains(sq)) {
            return true;
        }
        return false;
    }
    //is this sqare safe from non slide attack?
    bool SafeMove(int sq) {
        int[] distances = GetDistance(sq);
        //check for attacking pawns
        if (turnColour == whiteColourIndex) {
            if (distances[6] > 0 && squares[sq + 9] == 18) return false;
            if (distances[7] > 0 && squares[sq + 7] == 18) return false;
        } else {
            if (distances[4] > 0 && squares[sq - 7] == 10) return false;
            if (distances[5] > 0 && squares[sq - 9] == 10) return false;
        }
        //check for attacking knight
        int enemyKnight = enemyPieceCol | Piece.Knight;
        List<int> tiles = GetKnightTiles(sq);
        foreach (int to in tiles) {
            if (squares[to] == enemyKnight) {
                return false;
            }
        }
        //check for adjacent king
        int enemyKing = enemyPieceCol | Piece.King;
        for (int d = 0; d < 8; d++) {
            if (distances[d] > 1) distances[d] = 1;
            for (int i = 1; i < distances[d] + 1; i++) {
                if (squares[sq + (offset[d] * i)] == enemyKing) {
                    return false;
                }
            }
        }
        return true;
    }
    //get the tiles that are threatened by sliding pieces
    void GetSlideDangerSquares() {
        SquaresUnderAttack.Clear();
        foreach (int s in board.rooks[enemyColour]) SlideAttackSquares(s, 0, 4);
        foreach (int s in board.bishops[enemyColour]) SlideAttackSquares(s, 4, 8);
        foreach (int s in board.queens[enemyColour]) SlideAttackSquares(s, 0, 8);
    }
    //get the tiles that are threatened by sliding pieces
    void SlideAttackSquares(int sq, int start, int end) {
        int[] distances = GetDistance(sq);
        int[] offset = new int[] { -8, 8, 1, -1, -7, -9, 9, 7 };
        for (int d = start; d < end; d++) {
            for (int i = 1; i < distances[d] + 1; i++) {
                if (squares[sq + (offset[d] * i)] != 0) {
                    if (squares[sq + (offset[d] * i)] == king) {
                        SquaresUnderAttack.Add(sq + (offset[d] * i));
                        continue;
                    } else {
                        if (Piece.Colour(squares[sq + (offset[d] * i)]) != turnColour) {
                            SquaresUnderAttack.Add(sq + (offset[d] * i));
                        }
                        break;
                    }
                }
                SquaresUnderAttack.Add(sq + (offset[d] * i));
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

    void GetPawnMoves() {
        int offset = (turnColour == whiteColourIndex) ? 8 : -8;
        int[] diagnols = (turnColour == whiteColourIndex) ? new int[] { 6, 7 } : new int[] { 4, 5 };
        int[] diagnolOffset = (turnColour == whiteColourIndex) ? new int[] { 9, 7 } : new int[] { -7, -9 };
        List<int> pawnM = board.pawns[turnColour];
        foreach (int sq in pawnM) {
            int[] distances = GetDistance(sq);
            bool doubleMove = distances[turnColour] == 1;
            bool promotionMove = distances[enemyColour] == 1;
            int moveTo = sq + offset;
            //if sqare empty
            if (squares[moveTo] == 0) {
                //if not pinned or is moving in pin
                if (!pinnedPieces.ContainsKey(sq) || pinnedPieces[sq].Contains(sq + offset)) {
                    //if not in check or is moving to stop check
                    if (!inCheck || checkMoves.Contains(moveTo)) {
                        if (promotionMove) {
                            moves.Add(new Move(sq, moveTo, 4));
                            moves.Add(new Move(sq, moveTo, 5));
                            moves.Add(new Move(sq, moveTo, 6));
                            moves.Add(new Move(sq, moveTo, 7));
                        } else {
                            moves.Add(new Move(sq, moveTo));
                        }
                    }
                    //if can move double and sq empty
                    if (doubleMove && squares[moveTo + offset] == 0) {
                        //recheck check, no need to recheck pin
                        if (!inCheck || checkMoves.Contains(moveTo + offset)) {
                            moves.Add(new Move(sq, moveTo + offset, Move.Flag.PawnDoubleMove));
                        }
                    }
                }
            }
            //check pawn captures
            for (int d = 0; d < 2; d++) {
                moveTo = sq + diagnolOffset[d];
                if (distances[diagnols[d]] > 0) {
                    //recheck pins
                    if (!pinnedPieces.ContainsKey(sq) || pinnedPieces[sq].Contains(moveTo)) {
                        //recheck check
                        int eset = (turnColour == whiteColourIndex) ? -8 : 8;
                        if (checkMoves.Contains(board.Enpassant + eset)) {
                            //Enpass capture
                            if (moveTo == board.Enpassant && ValidateEnpassant(sq, moveTo)) {
                                moves.Add(new Move(sq, moveTo, Move.Flag.EnPassantCapture));
                            }
                        }
                        if (!inCheck || checkMoves.Contains(moveTo)) {
                            //Enpass capture
                            if (board.Enpassant!=0 && moveTo == board.Enpassant && ValidateEnpassant(sq, moveTo)) {
                                moves.Add(new Move(sq, moveTo, Move.Flag.EnPassantCapture));
                            }
                            //regular capture
                            if (squares[moveTo] != 0 && Piece.Colour(squares[moveTo]) != turnColour) {
                                if (promotionMove) {
                                    moves.Add(new Move(sq, moveTo, 4));
                                    moves.Add(new Move(sq, moveTo, 5));
                                    moves.Add(new Move(sq, moveTo, 6));
                                    moves.Add(new Move(sq, moveTo, 7));
                                } else {
                                    moves.Add(new Move(sq, moveTo));
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
        int[] distances = GetDistance(sq);
        int[] offset = new int[] { -8, 8, 1, -1, -7, -9, 9, 7 };
        for (int d = 0; d < 8; d++) {
            for (int i = 1; i < distances[d] + 1; i++) {
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
        foreach (int sq in KnightM) {
            if (pinnedPieces.ContainsKey(sq)) {
                continue;
            }
            List<int> tiles = GetKnightTiles(sq);
            foreach (int t in tiles) {
                if (inCheck & !checkMoves.Contains(t)) {
                    continue;
                }
                if (squares[t] != 0 && Piece.Colour(squares[t]) == turnColour) {
                    continue;
                }
                moves.Add(new Move(sq, t));
            }
        }
    }
    //return the tiles around a piece that a knight could attack 
    List<int> GetKnightTiles(int sq) {
        List<int> tiles = new List<int>();
        int[] distances = GetDistance(sq);
        if (distances[0] >= 2) {
            if (distances[2] >= 1) tiles.Add(sq - 15);
            if (distances[3] >= 1) tiles.Add(sq - 17);
        }
        if (distances[1] >= 2) {
            if (distances[2] >= 1) tiles.Add(sq + 17);
            if (distances[3] >= 1) tiles.Add(sq + 15);
        }
        if (distances[2] >= 2) {
            if (distances[1] >= 1) tiles.Add(sq + 10);
            if (distances[0] >= 1) tiles.Add(sq - 6);
        }
        if (distances[3] >= 2) {
            if (distances[1] >= 1) tiles.Add(sq + 6);
            if (distances[0] >= 1) tiles.Add(sq - 10);
        }

        return tiles;
    }
    //get attacks for sliding pieces
    void Slide(int sq, int start, int end) {
        int[] distances = GetDistance(sq);
        if (sq == board.kings[turnColour]) {
            for (int x = 0; x < distances.Length; x++) {
                if (distances[x] > 1) distances[x] = 1;
            }
        }
        int[] offset = new int[] { -8, 8, 1, -1, -7, -9, 9, 7 };
        for (int d = start; d < end; d++) {
            for (int i = 1; i < distances[d] + 1; i++) {
                int moveTo = sq + (offset[d] * i);
                if (squares[moveTo] != 0) {
                    if (Piece.Colour(squares[moveTo]) != turnColour) {
                        //if not pinned or is moving in pin
                        if (!pinnedPieces.ContainsKey(sq) || pinnedPieces[sq].Contains(moveTo)) {
                            //if not in check or is moving to stop check
                            if (!inCheck || checkMoves.Contains(moveTo)) {
                                moves.Add(new Move(sq, moveTo));
                            }
                        }
                    }
                    break;
                } else {
                    if (!pinnedPieces.ContainsKey(sq) || pinnedPieces[sq].Contains(moveTo)) {
                        if (!inCheck || checkMoves.Contains(moveTo)) {
                            moves.Add(new Move(sq, moveTo));
                        }
                    }
                }
            }
        }

    }


}
