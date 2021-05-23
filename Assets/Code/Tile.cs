using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Tile : MonoBehaviour {
    Material moveMat, selectMat, blockedMat, takeMat;
    public bool showingMoveable, showingBlocked, showingTakeable, showingEnpas;
    public int num;
    public PieceObject piece;
    GameDisplay gameDisplay;
    GameLogic gameLogic;

    private void Start() {
        gameLogic = GameLogic.instance;
        gameDisplay = GameDisplay.instance;
        selectMat = Resources.Load<Material>("Materials/Select");
        moveMat = Resources.Load<Material>("Materials/Move");
        blockedMat = Resources.Load<Material>("Materials/Block");
        takeMat = Resources.Load<Material>("Materials/Take");
    }

    public void PlacePiece(PieceObject peice_) {
        piece = peice_;
        peice_.GetComponent<PieceObject>().tile = this;
    }

    public override string ToString() {
        string r = this.name +" " + this.num + gameLogic.board[num];
        if (piece != null) r = r + " - " + piece.name;
        return r;
    }

    public void OnMouseDown() {
        if (showingBlocked) return;
        if (showingTakeable || showingMoveable) {
            gameLogic.MovePeice(gameDisplay.SelectedPeice.tile.num, num);
            return;
        }
        if (piece == null) {
            gameDisplay.Unselect();
            Debug.Log(this);
        } else {
            Debug.Log(this);
        }
    }

    void OnMouseOver() {
        GetComponent<Renderer>().enabled = true;
    }

    void OnMouseExit() {
        if (!showingMoveable && !showingBlocked && !showingTakeable) {
            GetComponent<Renderer>().enabled = false;
        }
    }

    public void ShowMoveable(bool enpas) {
        if (enpas) {
            showingEnpas = true;
        }
        if (this.piece != null) ShowTakeable();
        GetComponent<Renderer>().material = moveMat;
        GetComponent<Renderer>().enabled = true;
        showingMoveable = true;
        gameDisplay.activatedTiles.Add(this);
    }

    public void Hide() {
        GetComponent<Renderer>().material = selectMat;
        GetComponent<Renderer>().enabled = false;
        showingMoveable = false;
        showingBlocked = false;
        showingTakeable = false;
        showingEnpas = false;
    }

    public void ShowBlocked() {
        GetComponent<Renderer>().material = blockedMat;
        GetComponent<Renderer>().enabled = true;
        showingBlocked = true;
        gameDisplay.activatedTiles.Add(this);
    }

    public void ShowTakeable() {
        GetComponent<Renderer>().material = takeMat;
        GetComponent<Renderer>().enabled = true;
        if(char.ToLower(gameLogic.board[num])!='e') piece.InDanger();
        showingTakeable = true;
        gameDisplay.activatedTiles.Add(this);
    }

}