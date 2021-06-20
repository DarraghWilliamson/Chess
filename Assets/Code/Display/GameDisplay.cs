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


    public void DebugSetMoves(Move m) {
        int s = m.StartSquare;
        if (tiles[s].piece.pos.Contains(m.EndSquare)) return; else tiles[s].piece.pos.Add(m.EndSquare);
    }

    void OnTurnEnd() {
        if (gameLogic.board.En) {
            Enp.transform.position = tiles[gameLogic.board.Enpassant].transform.position;
            Enp.SetActive(true);
        } else {
            Enp.SetActive(false);
        }
    }

    public void UpdateDisplay(Move move) {
        if (move.MoveFlag == Move.Flag.Castling) {
            bool kingSide = move.EndSquare == 6 || move.EndSquare == 62;
            int rookFrom = kingSide ? move.EndSquare + 1 : move.EndSquare - 2;
            int rookTo = kingSide ? move.EndSquare - 1 : move.EndSquare + 1;
            MovePieces(rookFrom, rookTo);
        }
        if(move.MoveFlag == Move.Flag.EnPassantCapture) {
            int Enpassant = move.EndSquare;
            int EnpCap = (Enpassant >= 16 && Enpassant <= 23) ? Enpassant + 8 : Enpassant - 8;
            print(EnpCap);
            tiles[EnpCap].piece.Die();
        }
        MovePieces(move.StartSquare, move.EndSquare);
    }

    public void MovePieces(int from, int to) {
        Unselect();
        PieceObject p = tiles[from].piece;
            if (tiles[to].piece != null) tiles[to].piece.Die();
            tiles[from].piece = null;
            tiles[to].PlacePiece(p);
            p.transform.position = tiles[to].transform.position;
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
