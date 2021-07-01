using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;

public class GameLogic {
    public static GameLogic instance;
    public int halfMove, fullMove, playerColour;
    public bool check, checkmate, AiOn, show = true;
    public List<Move> possableMoves;
    ArtificialPlayer artificialPlayer;
    public TranspositionTable table;
    public GameDisplay gameDisplay;
    public Board board;

    public delegate void OnTurnEnd();
    public OnTurnEnd onTurnEnd;
    public delegate void OnCheck();
    public OnCheck onCheck;

    public GameLogic() {
        instance = this;
    }

    public void Start(string startFen) {
        board = new Board(this);
        artificialPlayer = new ArtificialPlayer(1,this);
        table = new TranspositionTable(board, 64000);
        Zobrist.FillzProperties();
        AiOn = false;
        //LoadFen(FEN.FenArray[1]);
        LoadFen(FEN.twoMil);
    }

    public void LoadFen(string i) {
        board.LoadInfo(i);
        playerColour = board.turnColour;
        if(show) gameDisplay.RefreshDisplay(board);
        if (show) onTurnEnd?.Invoke();
        if (show) StartTurn();
    }
    
    public void StartTurn() {
        possableMoves = board.GenerateMoves();
        onCheck?.Invoke();
        if (board.turnColour != playerColour && AiOn == true) {
            artificialPlayer.TakeTurn();
        }
    }

    public void EndTurn() {
        if(show) gameDisplay.RefreshDisplay(board);
        onTurnEnd?.Invoke();
        StartTurn();
    }

    public void Tests() {
        //Perft.MoveTester(FEN.FenArray[2], 4);
        //Perft.MoveTestSplit(FEN.twoMil, 5);
        //Perft.MoveTester(FEN.startFen, 5);
        Perft.MoveTester(4);
        //Perft.RunTests();
    }
    public void ToggleAi() {
        AiOn = AiOn ? false : true;
    }
    public bool MyTurn() {
        return playerColour == board.turnColour;
    }
    public int GetTeam() {
        return playerColour;
    }
    public void Check() {
        onCheck?.Invoke();
    }
    public void Checkmate() {
        onCheck?.Invoke();
    }

    public void ChangeTeam() {
        if (playerColour == 0) playerColour = 1; else playerColour = 0;
    }
    //logs moves to conosle
    public string Log(Move move) {
        Dictionary<int, char> lerretDict = new Dictionary<int, char> {
            { Piece.King,'K' },
            { Piece.Queen,'Q' },
            { Piece.Knight,'N' },
            { Piece.Bishop,'B' },
            { Piece.Rook,'R' }
        };
        int from = move.StartSquare;
        int to = move.EndSquare;
        int[] squares = board.squares;

        if(move.MoveFlag == Move.Flag.Castling) {
            if (to == 62 || to == 6) {
                return "0-0";
            } else {
                return "0-0-0";
            }
        }
        StringBuilder log = new StringBuilder();
        if (Piece.Type(squares[from]) == Piece.Pawn) {
            if (squares[to] != 0)  log.Append(GetBoardRep(squares[from])[0] + "x");
        } else {
            log.Append(lerretDict[Piece.Type(squares[from])]);
            if (squares[to] != 0) log.Append("x");
        }
        log.Append(GetBoardRep(squares[from]));
        return log.ToString();
    }
    public string GetBoardRep(int sq) {
        int rank = (sq / 8) + 1;
        int t = sq % 8;
        char file = (char)(t + 65);
        string s = char.ToLower(file) + "" + rank;
        return s;

    }

}