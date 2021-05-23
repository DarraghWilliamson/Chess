using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PieceObject : MonoBehaviour {

    public bool inDanger, selected;
    public Colour colour = Colour.White;
    public Tile tile;
    public Material standard, greenOutline, redOutline;
    public Tile[] tiles;
    GameDisplay gameDisplay;
    GameLogic gameLogic;
    PieceLogic peiceLogic;
    public char[] board;

    private void Start() {
        peiceLogic = PieceLogic.instance;
        gameDisplay = GameDisplay.instance;
        gameLogic = GameLogic.instance;
        board = gameLogic.board;
    }

    private void OnMouseDown() {
        if (!gameLogic.MyTurn()) return;
        if ((gameLogic.playerColour != colour) && inDanger) {
            //gameDisplay.MovePiece(gameDisplay.SelectedPeice.tile, tile);
            gameLogic.MovePeice(gameDisplay.SelectedPeice.tile.num, tile.num);
            
            return;
        }
        if (!selected && (gameLogic.playerColour == colour)) {
            selected = true;
            gameDisplay.SelectNew(this);
        }
    }
    
    public bool IsEnemy(PieceObject p) {
        if (p.colour == this.colour) return false; else return true;
    }

    void OnMouseOver() {
        if (inDanger) return;
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
        char t = GameLogic.instance.board[i];
        if (!char.IsLetter(t)) {
            return "move";
        } else {
            if (char.IsUpper(board[i]) != (colour == Colour.Black)) {
                return "block";
            } else {
                return "take";
            }
        }
    }

    void ShowMoves() {
        int c = Array.IndexOf(tiles, tile);
        Dictionary<int, List<int>> moves = peiceLogic.GetMoves(c, colour);
        if (moves != null && moves[c].Count != 0) {
            List<int> move = moves[c];
            foreach (int i in move) {
                if (CheckTile(i) == "enpas") {
                    tiles[i].ShowMoveable(true);
                }
                if (CheckTile(i) == "move") {
                    if (tiles[i].piece == null) {
                        tiles[i].ShowMoveable(false);
                    } else {
                        tiles[i].ShowBlocked();
                    }
                }
                if (CheckTile(i) == "take") tiles[i].ShowTakeable();
                if (CheckTile(i) == "block") tiles[i].ShowBlocked();
            }
        }
    }

    public void Unselect() {
        selected = false;
        GetComponent<Renderer>().material = standard;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        foreach (Tile tile in gameDisplay.activatedTiles) tile.Hide();
        gameDisplay.activatedTiles.Clear();
        foreach (PieceObject piece in gameDisplay.activatedPieces) piece.OutDanger();
        gameDisplay.activatedTiles.Clear();
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
        if (colour == Colour.White) {
            transform.position = gameDisplay.deathWhite[gameDisplay.deadWhite];
            gameDisplay.deadWhite++;
        } else {
            transform.position = gameDisplay.deathBlack[gameDisplay.deadBlack];
            gameDisplay.deadBlack++;
        }
    }


}
