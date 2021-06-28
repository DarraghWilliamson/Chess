using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Tile : MonoBehaviour {
    Material moveMat, selectMat, blockedMat, takeMat;
    public bool showingMoveable, showingBlocked, showingTakeable;
    public int num;
    public PieceObject piece;
    public GameDisplay gameDisplay;
    public GameLogic gameLogic;
    readonly Dictionary<int, string> dictString = new Dictionary<int, string>() {
        [Piece.Pawn] = "Pawn",
        [Piece.Bishop] = "Bishop",
        [Piece.Knight] = "Knight",
        [Piece.Rook] = "Rook",
        [Piece.King] = "King",
        [Piece.Queen] = "Queen"
    };

    private void Start() {
        selectMat = Resources.Load<Material>("Materials/Select");
        moveMat = Resources.Load<Material>("Materials/Move");
        blockedMat = Resources.Load<Material>("Materials/Block");
        takeMat = Resources.Load<Material>("Materials/Take");
    }
    
    public override string ToString() {
        string r = this.name +" " + this.num;
        int p = gameLogic.board.squares[num];
        r = r + " - " + gameLogic.board.squares[num];
        if (p != 0) {
            r += Piece.Colour(p) == 0 ? " White" : " Black";
            r += dictString[Piece.Type(p)];
        }
        return r;
    }

    public void OnMouseDown() {

        if (showingBlocked) return;
        //if takable refer to piece
        if (showingTakeable) {
            piece.OnMouseDown();
        }
        //if moveable fine the move
        if (showingMoveable) {
            List<Move> posibilities = new List<Move>();
            foreach(Move move in gameLogic.possableMoves) {
                if(move.StartSquare == gameDisplay.SelectedPeice.tile.num) {
                    if(move.EndSquare == num) {
                        posibilities.Add(move);
                    }
                }
            }
            if (posibilities.Count != 1) print("mult");
            gameLogic.board.MovePiece(posibilities[0]);
            return;
        }
        //if no piece/move, log
        if (piece == null) {
            gameDisplay.Unselect();
            Debug.Log(this);
        } else {
            Debug.Log(this);
        }
    }

    public void SetPiece(PieceObject piece) {
        this.piece = piece;
    }

    void OnMouseOver() {
        GetComponent<Renderer>().enabled = true;
    }

    void OnMouseExit() {
        if (!showingMoveable && !showingBlocked && !showingTakeable) {
            GetComponent<Renderer>().enabled = false;
        }
    }
    public void ShowBlocked() {
        GetComponent<Renderer>().material = blockedMat;
        GetComponent<Renderer>().enabled = true;
        showingBlocked = true;
        gameDisplay.activatedTiles.Add(this);
    }
    public void ShowMoveable() {
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
    }
    public void ShowTakeable() {
        if (piece != null) {
            piece.InDanger();
        }
        GetComponent<Renderer>().material = takeMat;
        GetComponent<Renderer>().enabled = true;
        showingTakeable = true;
        gameDisplay.activatedTiles.Add(this);
    }

}