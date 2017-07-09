using System;
using System.Collections;
using System.Collections.Generic;


public class BoardEvaluator {

	private static readonly int[] PawnTable = new int[]
	{
		0,  0,  0,  0,  0,  0,  0,  0,
		50, 50, 50, 50, 50, 50, 50, 50,
		10, 10, 20, 30, 30, 20, 10, 10,
		5,  5, 10, 27, 27, 10,  5,  5,
		0,  0,  0, 25, 25,  0,  0,  0,
		5, -5,-10,  0,  0,-10, -5,  5,
		5, 10, 10,-25,-25, 10, 10,  5,
		0,  0,  0,  0,  0,  0,  0,  0
	};

	private static readonly int[] KnightTable = new int[]
	{
		-50,-40,-30,-30,-30,-30,-40,-50,
		-40,-20,  0,  0,  0,  0,-20,-40,
		-30,  0, 10, 15, 15, 10,  0,-30,
		-30,  5, 15, 20, 20, 15,  5,-30,
		-30,  0, 15, 20, 20, 15,  0,-30,
		-30,  5, 10, 15, 15, 10,  5,-30,
		-40,-20,  0,  5,  5,  0,-20,-40,
		-50,-40,-20,-30,-30,-20,-40,-50,
	};

	private static readonly int[] BishopTable = new int[]
	{
		-20,-10,-10,-10,-10,-10,-10,-20,
		-10,  0,  0,  0,  0,  0,  0,-10,
		-10,  0,  5, 10, 10,  5,  0,-10,
		-10,  5,  5, 10, 10,  5,  5,-10,
		-10,  0, 10, 10, 10, 10,  0,-10,
		-10, 10, 10, 10, 10, 10, 10,-10,
		-10,  5,  0,  0,  0,  0,  5,-10,
		-20,-10,-40,-10,-10,-40,-10,-20,
	};

	private static readonly int[] KingTable = new int[]
	{
		-30, -40, -40, -50, -50, -40, -40, -30,
		-30, -40, -40, -50, -50, -40, -40, -30,
		-30, -40, -40, -50, -50, -40, -40, -30,
		-30, -40, -40, -50, -50, -40, -40, -30,
		-20, -30, -30, -40, -40, -30, -30, -20,
		-10, -20, -20, -20, -20, -20, -20, -10, 
		20,  20,   0,   0,   0,   0,  20,  20,
		20,  30,  10,   0,   0,  10,  30,  20
	};

	private Dictionary<Piece.Type, int> type_to_value;
	private Dictionary<Piece.Type, int[]> type_to_position_value;



	public BoardEvaluator() {
		this.type_to_value = new Dictionary<Piece.Type, int> ();
		this.type_to_value [Piece.Type.NULL] = 0;
		this.type_to_value [Piece.Type.ROOK] = 5000;
		this.type_to_value [Piece.Type.KNIGHT] = 320;
		this.type_to_value [Piece.Type.BISHOP] = 325;
		this.type_to_value [Piece.Type.QUEEN] = 975;
		this.type_to_value [Piece.Type.KING] = 32767;
		this.type_to_value [Piece.Type.PAWN] = 100;
		this.type_to_position_value = new Dictionary<Piece.Type, int[]> ();
		this.type_to_position_value [Piece.Type.PAWN] = new int[]
		{
			0,  0,  0,  0,  0,  0,  0,  0,
			50, 50, 50, 50, 50, 50, 50, 50,
			10, 10, 20, 30, 30, 20, 10, 10,
			5,  5, 10, 27, 27, 10,  5,  5,
			0,  0,  0, 25, 25,  0,  0,  0,
			5, -5,-10,  0,  0,-10, -5,  5,
			5, 10, 10,-25,-25, 10, 10,  5,
			0,  0,  0,  0,  0,  0,  0,  0
		};
		this.type_to_position_value [Piece.Type.KNIGHT] = new int[]
		{
			-50,-40,-30,-30,-30,-30,-40,-50,
			-40,-20,  0,  0,  0,  0,-20,-40,
			-30,  0, 10, 15, 15, 10,  0,-30,
			-30,  5, 15, 20, 20, 15,  5,-30,
			-30,  0, 15, 20, 20, 15,  0,-30,
			-30,  5, 10, 15, 15, 10,  5,-30,
			-40,-20,  0,  5,  5,  0,-20,-40,
			-50,-40,-20,-30,-30,-20,-40,-50,
		};
		this.type_to_position_value [Piece.Type.BISHOP] = new int[]
		{
			-20,-10,-10,-10,-10,-10,-10,-20,
			-10,  0,  0,  0,  0,  0,  0,-10,
			-10,  0,  5, 10, 10,  5,  0,-10,
			-10,  5,  5, 10, 10,  5,  5,-10,
			-10,  0, 10, 10, 10, 10,  0,-10,
			-10, 10, 10, 10, 10, 10, 10,-10,
			-10,  5,  0,  0,  0,  0,  5,-10,
			-20,-10,-40,-10,-10,-40,-10,-20,
		};
		this.type_to_position_value [Piece.Type.KING] = new int[]
		{
			-30, -40, -40, -50, -50, -40, -40, -30,
			-30, -40, -40, -50, -50, -40, -40, -30,
			-30, -40, -40, -50, -50, -40, -40, -30,
			-30, -40, -40, -50, -50, -40, -40, -30,
			-20, -30, -30, -40, -40, -30, -30, -20,
			-10, -20, -20, -20, -20, -20, -20, -10, 
			20,  20,   0,   0,   0,   0,  20,  20,
			20,  30,  10,   0,   0,  10,  30,  20
		};
	}


	public int evaluate(Board working_board, int number_moves) {
		int value = 0;
		Piece current_piece = new Piece ();
		for (int index = 0; index < 64; index++) {
			current_piece = working_board.getPiece (index);
			value += this.type_to_value [current_piece.type];
			if (this.type_to_position_value.ContainsKey (current_piece.type)) {
				value += this.type_to_position_value [current_piece.type] [(current_piece.team == Piece.Team.BLACKS) ? 63 - index : index];
			}
		}
		value += number_moves;
		return value;
	}
}
