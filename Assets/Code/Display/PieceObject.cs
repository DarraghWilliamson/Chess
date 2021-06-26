using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PieceObject : MonoBehaviour {

    public bool inDanger, selected;
    public int colour = 0;
    public int type = 0;
    public bool isPromotionPiece;
    public Tile tile;
    public Material standard, greenOutline, redOutline;
    public Tile[] tiles;
    public GameDisplay gameDisplay;
    public GameLogic gameLogic;
    public bool force = false;
    public Move ForceMove;

    public void OnMouseDown() {
        Debug.Log(this.name);
        if (!gameLogic.MyTurn()) return;
        if (force) {
            GameDisplay.instance.AddNewPiece(ForceMove);
            gameLogic.board.MovePiece(ForceMove);
            return;
        }
        if ((gameLogic.playerColour != colour) && inDanger) {
            List<Move> posibilities = new List<Move>();
            foreach (Move move in gameLogic.possableMoves) {
                if (move.StartSquare == gameDisplay.SelectedPeice.tile.num) {
                    if (move.EndSquare == tile.num) {
                        posibilities.Add(move);
                    }
                }
            }
            if (posibilities.Count != 1) {
                SelectPromote(posibilities);
                return;
            } else {
                Move m = posibilities[0];
                gameLogic.board.MovePiece(m);
                return;
            }
        }
        if (!selected && (gameLogic.playerColour == colour) && !isPromotionPiece) {
            selected = true;
            gameDisplay.SelectNew(this);
        }
    }
    void SelectPromote(List<Move> posibilities) {
        List<GameObject> con;
        if(gameLogic.board.turnColour == 0) {
            con = gameDisplay.PromotionWhite;
        } else {
            con = gameDisplay.PromotionBlack;
        }
        for(int i = 0; i<con.Count;i++) {
            con[i].SetActive(true);
        }
        foreach(Move m in posibilities) {
            switch (m.MoveFlag) {
                case Move.Flag.PromotionQueen: con[3].GetComponent<PieceObject>().ForceMove = m; con[3].GetComponent<PieceObject>().force = true; break;
                case Move.Flag.PromotionBishop:con[2].GetComponent<PieceObject>().ForceMove = m; con[2].GetComponent<PieceObject>().force = true; break;
                case Move.Flag.PromotionRook: con[0].GetComponent<PieceObject>().ForceMove = m; con[0].GetComponent<PieceObject>().force = true; break;
                case Move.Flag.PromotionKnight: con[1].GetComponent<PieceObject>().ForceMove = m; con[1].GetComponent<PieceObject>().force = true; break;
            }
        }
    }

    public bool IsEnemy(PieceObject p) {
        if (p.colour == this.colour) return false; else return true;
    }

    void OnMouseOver() {
        if (inDanger) return;
        if (gameLogic == null) Debug.Log("asd");
        if (gameLogic.playerColour != colour) return;
        GetComponent<Renderer>().material = greenOutline;
    }

    void OnMouseExit() {
        if (inDanger) return;
        if (!selected) GetComponent<Renderer>().material = standard;
    }

    public void Select() {
        selected = true;
        transform.position = transform.position + new Vector3(0, 5, 0);
        GetComponent<Renderer>().material = greenOutline;
        ShowMoves();
    }

    string CheckTile(int i) {
        int t = gameLogic.board.squares[i];
        if (t==0) {
            return "move";
        } else {
            if (Piece.IsColour(gameLogic.board.squares[i], Piece.White) != (colour == 1)) {
                return "block";
            } else {
                return "take";
            }
        }
    }

    void ShowMoves() {
        List<Move> moves = gameLogic.possableMoves;
        foreach(Move move in moves) {
            if (move.StartSquare == tile.num) {
                int end = move.EndSquare;
                if (Piece.Type(gameLogic.board.squares[move.StartSquare]) == Piece.Pawn) {
                    if((move.EndSquare-move.StartSquare)%8 != 0) {
                        tiles[end].ShowTakeable();
                        continue;
                    }
                }
                if (CheckTile(end) == "move") tiles[end].ShowMoveable();
                if (CheckTile(end) == "take") tiles[end].ShowTakeable();
                if (CheckTile(end) == "block") tiles[end].ShowBlocked();
            }
        }
    }

    public void Unselect() {
        tile.Hide();
        List<GameObject> con;
        if (gameLogic.board.turnColour == 0) {
            con = gameDisplay.PromotionWhite;
        } else {
            con = gameDisplay.PromotionBlack;
        }
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
