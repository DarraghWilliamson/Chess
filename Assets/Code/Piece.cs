using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Piece {

	public const int None = 0;
	public const int King = 1;
	public const int Pawn = 2;
	public const int Knight = 3;
	public const int Bishop = 5;
	public const int Rook = 6;
	public const int Queen = 7;
	public const int White = 8;
	public const int Black = 16;

	public static bool IsColour(int piece, int colour) {
		int i = (piece / 16) == 0 ? 8 : 16;
		return i == colour;
	}

	public static int Colour(int piece) {
		return piece / 16;
	}
	public static bool Empty(int piece) {
		return piece == 0;
    }

	public static int Type(int piece) {
		return piece % 8 ;
	}
}