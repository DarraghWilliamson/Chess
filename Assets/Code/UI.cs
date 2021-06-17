using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI : MonoBehaviour {
    public TMP_Text team, turn, check;
    GameLogic gamelogic;

    void Start() {
        gamelogic = GameLogic.instance;
        gamelogic.onTurnEnd += UpdateUI;
        gamelogic.onCheck += UpdateCheck;
        UpdateUI();
    }

    public void UpdateUI() {
        if (gamelogic.board.turnColour == 0) team.text = "White's move"; else team.text = "Blacks's move";
        if (gamelogic.board.turnColour == gamelogic.playerColour) turn.text = "Your move"; else turn.text = "Enemy move";
    }

    public void UpdateCheck() {
        if (gamelogic.check) { check.text = "In Check"; return; }
        if (gamelogic.checkmate) { check.text = "Checkmate."; return; }
        check.text = "";
    }

    public void FEN() {
        GameLogic.instance.ExportFen();
    }

    public void ToggleAi() {
        GameLogic.instance.ToggleAi();
    }

    public void TurnChange() {
        gamelogic.EndTurn();
        UpdateUI();
    }
    public void TeamChange() {
        gamelogic.ChangeTeam();
        UpdateUI();
    }
}
