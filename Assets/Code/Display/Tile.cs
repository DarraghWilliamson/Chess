using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Utils;

public class Tile : MonoBehaviour {
    Material moveMat, selectMat, blockedMat, takeMat;
    public bool showingMoveable, showingBlocked, showingTakeable;
    public int num;
    public PieceObject piece;
    public GameDisplay gameDisplay;
    public GameLogic gameLogic;

    public bool promotion = false;
    public int assignedMove;

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
        if(piece == null || piece.colour != gameLogic.board.turnColour) {
            Debug.Log(this);
        }
        //if theres a move assigned, do that
        if (assignedMove != 0) {
            gameLogic.board.MovePiece(assignedMove);
        } else {
            if (piece != null) {
                piece.OnMouseDown();
                return;
            } else {
                gameDisplay.Unselect();
            }
        }
        //if theres a piece, refer to the piece logic
        if (piece != null) {
            piece.OnMouseDown();
            return;
        }
        //else log tile
        Debug.Log(this);
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
        assignedMove = 0;
        if (piece != null) piece.assignedMove = 0;
        GetComponent<Renderer>().material = selectMat;
        GetComponent<Renderer>().enabled = false;
        showingMoveable = false;
        showingBlocked = false;
        showingTakeable = false;
    }
    public void ShowTakeable() {
        piece.InDanger();
        GetComponent<Renderer>().material = takeMat;
        GetComponent<Renderer>().enabled = true;
        showingTakeable = true;
        gameDisplay.activatedTiles.Add(this);
    }

}