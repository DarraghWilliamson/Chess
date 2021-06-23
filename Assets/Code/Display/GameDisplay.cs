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
    public List<GameObject> Pieces;

    public GameObject[] kings;
    public List<GameObject>[] pawns, knights, rooks, bishops, queens, all;

    void Awake() { instance = this; }

    private void Start() {
        gameLogic = GameLogic.instance;
        deathBlack = CreateDeathVectors(new Vector3(70, 0, -110), -20, 70);
        deathWhite = CreateDeathVectors(new Vector3(-70, 0, 110), 20, -70);
    }
    
    //probbaly better ways of doing this 
    public void RefreshDisplay() {
        deadBlack = 0;
        deadWhite = 0;
        Clear();
        Unselect();
        List<int>[] allLists = gameLogic.board.allLists;
        int[] kingsb = gameLogic.board.kings;
        for (int i = 0; i < kings.Length; i++){
            tiles[kingsb[i]].PlacePiece(kings[i].GetComponent<PieceObject>());
            kings[i].transform.position = tiles[kingsb[i]].transform.position;
        }
        for(int x = 0; x < all.Length; x++) {
            for(int i = 0; i < all[x].Count; i++) {
                try {
                    tiles[allLists[x][i]].PlacePiece(all[x][i].GetComponent<PieceObject>());
                    all[x][i].transform.position = tiles[allLists[x][i]].transform.position;
                } catch(System.ArgumentOutOfRangeException) {
                    all[x][i].GetComponent<PieceObject>().Die();
                }
            }
        }
    }

    public void Clear() {
        foreach(Tile t in tiles) {
            t.piece = null;
        }
    }


    public void DebugSetMoves(Move m) {
        int s = m.StartSquare;
        if (tiles[s].piece.pos.Contains(m.EndSquare)) return; else tiles[s].piece.pos.Add(m.EndSquare);
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
    
    public Vector3[] CreateDeathVectors(Vector3 pos, int x, int y) {
        Vector3[] vector3s = new Vector3[16];
        int v = 0;
        for (int i = 0; i < 2; i++) {
            for (int i2 = 0; i2 < 8; i2++) {
                vector3s[v] = pos;
                v++;
                pos.x += x;
            }
            pos.x = y;
            pos.z += x;
        }
        return vector3s;
    }

}
