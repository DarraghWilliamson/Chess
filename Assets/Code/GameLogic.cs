using System.Collections.Generic;

public class GameLogic {
    public static GameLogic instance;
    public int halfMove, fullMove, playerColour;
    public bool check, checkmate, AiOn, show = true;
    public List<int> possableMoves;
    private ArtificialPlayer artificialPlayer;
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

    public void Start() {
        board = new Board(this);
        artificialPlayer = new ArtificialPlayer(1, this);
        table = new TranspositionTable(board, 64000);
        Zobrist.FillzProperties();
        AiOn = false;
        LoadFen(FEN.startFen);
    }

    public void LoadFen(string i) {
        board.LoadInfo(i);
        playerColour = board.turnColour;
        if (show) gameDisplay.RefreshDisplay(board);
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
        if (show) gameDisplay.RefreshDisplay(board);
        onTurnEnd?.Invoke();
        StartTurn();
    }

    public void Tests() {
        Perft.MoveTestSplit(7);
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
}