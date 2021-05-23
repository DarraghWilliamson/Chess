using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDisplay : MonoBehaviour {

    public static GameDisplay instance;
    public PieceObject SelectedPeice;
    public List<Tile> activatedTiles;
    public Tile[] tiles;
    public List<PieceObject> activatedPieces;
    public Vector3[] deathBlack, deathWhite;
    public int deadBlack = 0;
    public int deadWhite = 0;
    GameLogic gameLogic;
    public GameObject Enp;

    void Awake() { instance = this; }

    private void Start() {
        gameLogic = GameLogic.instance;
        gameLogic.onTurnEnd += OnTurnEnd;
    }

    void OnTurnEnd() {
        if (gameLogic.En) {
            Enp.transform.position = tiles[gameLogic.Enpassant].transform.position;
            Enp.SetActive(true);
        } else {
            Enp.SetActive(false);
        }
    }

    public void MoveObjectAI(int t, int nt) {
        Tile tile = tiles[t];
        Tile newTile = tiles[nt];

        PieceObject p = tile.piece;
        if (newTile.piece != null) newTile.piece.Die();
        tile.piece = null;
        newTile.PlacePiece(p);
        p.transform.position = newTile.transform.position;
    }

    public void MovePiece(Tile tile, Tile newTile) {
        PieceObject p = tile.piece;
        if (newTile.piece != null) newTile.piece.Die();
        tile.piece = null;
        newTile.PlacePiece(p);
        p.transform.position = newTile.transform.position;
        Unselect();
    }

    public void SelectNew(PieceObject peice) {
        if (SelectedPeice != null) {
            SelectedPeice.Unselect();
        }
        peice.Select();
        SelectedPeice = peice;
    }

    public void Unselect() {
        if (SelectedPeice != null) {
            SelectedPeice.Unselect();
            SelectedPeice = null;
        }
    }

}
