using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI : MonoBehaviour {
    public TMP_Text team, turn, check, emp;
    GameLogic gamelogic;

    void Start() {
        gamelogic = GameLogic.instance;
        gamelogic.onTurnEnd += UpdateUI;
        gamelogic.onCheck += UpdateCheck;
        UpdateUI();
    }

    public void UpdateUI() {
        if (gamelogic.board == null) {
            team.text = "null";
            return;
        }
        if (gamelogic.board.turnColour == 0) team.text = "White's move"; else team.text = "Blacks's move";
        if (gamelogic.board.turnColour == gamelogic.playerColour) turn.text = "Your move"; else turn.text = "Enemy move";
        if (gamelogic.board.Enpassant != 99) emp.text = (" " + gamelogic.board.Enpassant); else emp.text = "";
    }

    public void UpdateCheck() {
        if (gamelogic.board.inCheck) { check.text = "In Check"; } else { check.text = ""; return; }
        if (gamelogic.checkmate) { check.text = "Checkmate."; return; }
    }

    public void TEST() {
        List<int> sq = GameLogic.instance.board.bitmatSquares;
        foreach(int i in sq) {
            GameDisplay.instance.tiles[i].ShowMoveable();
        }
    }
    public void Fen() {
        FEN.ExportFen(GameLogic.instance.board);
    }
    public void Tests() {
        GameLogic.instance.Tests();
    }

    public void ToggleAi() {
        GameLogic.instance.board.CtrlZ(GameLogic.instance.board.gameMoves.Peek());
    }

    public void TurnChange() {
        gamelogic.board.TurnSkip();
        UpdateUI();
    }
    public void TeamChange() {
        gamelogic.ChangeTeam();
        UpdateUI();
    }
}

