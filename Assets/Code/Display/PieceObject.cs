using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PieceObject : MonoBehaviour {

    public bool inDanger, selected;
    public int colour = 0;
    public Tile tile;
    public Material standard, greenOutline, redOutline;
    public Tile[] tiles;
    GameDisplay gameDisplay;
    GameLogic gameLogic;
    Board board;
    public int[] inBoard;


    public List<int> pos;

    private void Start() {
        gameDisplay = GameDisplay.instance;
        gameLogic = GameLogic.instance;
        board = gameLogic.board;
        inBoard = board.squares;
    }

    private void OnMouseDown() {
        if (!gameLogic.MyTurn()) return;
        if ((gameLogic.playerColour != colour) && inDanger) {
            List<Move> posibilities = new List<Move>();
            foreach (Move move in gameLogic.possableMoves) {
                if (move.StartSquare == gameDisplay.SelectedPeice.tile.num) {
                    if (move.EndSquare == tile.num) {
                        posibilities.Add(move);
                    }
                }
            }
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
        int t = board.squares[i];
        if (t==0) {
            return "move";
        } else {
            if (Piece.IsColour(board.squares[i], Piece.White) != (colour == 1)) {
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
                if (Piece.Type(board.squares[move.StartSquare]) == Piece.Pawn) {
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
        if (colour == 0) {
            transform.position = gameDisplay.deathWhite[gameDisplay.deadWhite];
            gameDisplay.deadWhite++;
        } else {
            transform.position = gameDisplay.deathBlack[gameDisplay.deadBlack];
            gameDisplay.deadBlack++;
        }
    }


}
