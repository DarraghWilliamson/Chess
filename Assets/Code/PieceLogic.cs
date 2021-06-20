using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PieceLogic {
    GameLogic gameLogic;
    Board board;
    public static PieceLogic instance;
    char ori;

    public PieceLogic() {
        instance = this;
        gameLogic = GameLogic.instance;
        board = gameLogic.board;
    }


  }


