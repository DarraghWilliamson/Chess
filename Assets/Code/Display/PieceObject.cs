using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class PieceObject : MonoBehaviour {
    public bool inDanger, selected, isPromotionPiece;
    public int colour = 0;
    public int type = 0;
    public Tile tile;
    public Material standard, greenOutline, redOutline;
    public Tile[] tiles;
    public GameDisplay gameDisplay;
    public GameLogic gameLogic;
    public List<int> posibilities = new List<int>();
    public bool promotion = false;
    public int assignedMove;

    public void OnMouseDown() {
        if (isPromotionPiece) {
            gameLogic.board.MovePiece(assignedMove);
            gameDisplay.WasPromotion();
            return;
        }

        if (!gameLogic.MyTurn()) {
            return;
        }
        if (gameDisplay.SelectedPeice != null && (assignedMove != 0 || promotion)) {
            if (promotion) {
                SelectPromote();
                return;
            }
            gameLogic.board.MovePiece(assignedMove);
            return;
        }
        if (!selected && (gameLogic.playerColour == colour)) {
            selected = true;
            gameDisplay.SelectNew(this);
        }
    }

    public void SetTile(Tile tile) {
        this.tile = tile;
    }

    private void SelectPromote() {
        List<GameObject> con;
        con = gameLogic.board.turnColour == 0 ? gameDisplay.PromotionWhite : gameDisplay.PromotionBlack;
        for (int i = 0; i < con.Count; i++) con[i].SetActive(true);
        gameDisplay.showingPromotionOptions = true;
        gameDisplay.promotingPawn = this.gameObject;

        foreach (ushort m in posibilities) {
            switch (GetPromotionType(m)) { // 0:Knight, 1:bishop, 2:rook, 3:queen
                case 3: con[3].GetComponent<PieceObject>().assignedMove = m; con[3].GetComponent<PieceObject>().isPromotionPiece = true; break;
                case 1: con[2].GetComponent<PieceObject>().assignedMove = m; con[2].GetComponent<PieceObject>().isPromotionPiece = true; break;
                case 2: con[0].GetComponent<PieceObject>().assignedMove = m; con[0].GetComponent<PieceObject>().isPromotionPiece = true; break;
                case 0: con[1].GetComponent<PieceObject>().assignedMove = m; con[1].GetComponent<PieceObject>().isPromotionPiece = true; break;
            }
        }
    }

    public bool IsEnemy(PieceObject p) {
        if (p.colour == this.colour) return false; else return true;
    }

    private void OnMouseOver() {
        if (inDanger) return;
        if (gameLogic == null)
            if (gameLogic.playerColour != colour) return;
        GetComponent<Renderer>().material = greenOutline;
    }

    private void OnMouseExit() {
        if (inDanger) return;
        if (!selected) GetComponent<Renderer>().material = standard;
    }

    public void Select() {
        selected = true;
        transform.position = transform.position + new Vector3(0, 5, 0);
        GetComponent<Renderer>().material = greenOutline;
        gameDisplay.ShowMoves(tile.num);
    }

    public void Unselect() {
        tile.Hide();
        List<GameObject> con = gameLogic.board.turnColour == 0 ? gameDisplay.PromotionWhite : gameDisplay.PromotionBlack;
        foreach (GameObject g in con) g.SetActive(false);

        selected = false;
        GetComponent<Renderer>().material = standard;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        foreach (Tile tile in gameDisplay.activatedTiles) tile.Hide();
        gameDisplay.activatedTiles.Clear();
        foreach (PieceObject piece in gameDisplay.activatedPieces) piece.OutDanger();
        gameDisplay.activatedPieces.Clear();
    }

    public void InDanger() {
        gameDisplay.activatedPieces.Add(this);
        GetComponent<Renderer>().material = redOutline;
        inDanger = true;
    }

    public void OutDanger() {
        GetComponent<Renderer>().material = standard;
        inDanger = false;
    }

    public void Die() {
        tile.piece = null;
        GetComponent<MeshCollider>().enabled = false;
        if (colour == 0) {
            if (gameDisplay == null) print("D");
            transform.position = GameDisplay.instance.deathWhite[GameDisplay.instance.deadWhite];
            gameDisplay.deadWhite++;
        } else {
            transform.position = GameDisplay.instance.deathBlack[GameDisplay.instance.deadBlack];
            gameDisplay.deadBlack++;
        }
    }
}