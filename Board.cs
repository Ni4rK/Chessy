using System;
using System.Collections;
using System.Collections.Generic;


public class Board {

	private Piece[] pieces;
	private Piece.Team current_team;
	private Queue<GameEvent> game_events;
	private Dictionary<GameEvent, Func<int, int, bool>> event_to_handler;
	private bool check_blacks;
	private bool check_whites;
	private bool win_blacks;
	private bool win_whites;

	public Board() {
		this.pieces = new Piece[64];
		for (int i = 0; i < 64; i++) {
			this.pieces [i] = new Piece ();
		}
		this.current_team = Piece.Team.WHITES;
		this.game_events = new Queue<GameEvent> ();
		this.event_to_handler = new Dictionary<GameEvent, Func<int, int, bool>> ();
		this.event_to_handler [GameEvent.MOVE] = this.handleEventMove;
		this.event_to_handler [GameEvent.ATTACK] = this.handleEventMove;
		this.event_to_handler [GameEvent.CASTLE_KINGSIDE] = this.handleEventCastle;
		this.event_to_handler [GameEvent.CASTLE_QUEENSIDE] = this.handleEventCastle;
		this.event_to_handler [GameEvent.CHECK] = this.handleEventCheck;
		this.event_to_handler [GameEvent.CHECKMATE] = this.handleEventCheckMate;
		this.check_blacks = false;
		this.check_whites = false;
		this.win_blacks = false;
		this.win_whites = false;
	}

	public void copy(Board source) {
		for (int i = 0; i < 64; i++) {
			this.pieces [i] = new Piece (source.getPiece(i).type, source.getPiece(i).team);
		}
		this.current_team = source.getCurrentTeam ();
		this.game_events.Clear ();
		this.check_blacks = source.isCheckBlacks ();
		this.check_whites = source.isCheckWhites ();
		this.win_blacks = source.hasWinBlacks ();
		this.win_whites = source.hasWinWhites ();
	}

	public void reset() {
		for (int i = 0; i < 64; i++) {
			this.pieces [i] = new Piece ();
		}
		for (int i = 0; i < 16; i++) {
			this.pieces [i] = new Piece (Piece.Type.PAWN, Piece.Team.WHITES);
			this.pieces [64 - 1 - i] = new Piece (Piece.Type.PAWN, Piece.Team.BLACKS);
		}
		this.pieces [0].type = Piece.Type.ROOK;
		this.pieces [1].type = Piece.Type.KNIGHT;
		this.pieces [2].type = Piece.Type.BISHOP;
		this.pieces [3].type = Piece.Type.QUEEN;
		this.pieces [4].type = Piece.Type.KING;
		this.pieces [5].type = Piece.Type.BISHOP;
		this.pieces [6].type = Piece.Type.KNIGHT;
		this.pieces [7].type = Piece.Type.ROOK;
		this.pieces [64 - 1 - 0].type = Piece.Type.ROOK;
		this.pieces [64 - 1 - 1].type = Piece.Type.KNIGHT;
		this.pieces [64 - 1 - 2].type = Piece.Type.BISHOP;
		this.pieces [64 - 1 - 3].type = Piece.Type.KING;
		this.pieces [64 - 1 - 4].type = Piece.Type.QUEEN;
		this.pieces [64 - 1 - 5].type = Piece.Type.BISHOP;
		this.pieces [64 - 1 - 6].type = Piece.Type.KNIGHT;
		this.pieces [64 - 1 - 7].type = Piece.Type.ROOK;
		this.current_team = Piece.Team.WHITES;
		this.game_events.Clear ();
		this.check_blacks = false;
		this.check_whites = false;
		this.win_blacks = false;
		this.win_whites = false;
	}

	public Piece getPiece(int index) {
		return this.pieces [index];
	}

	public Piece.Team getCurrentTeam() {
		return this.current_team;
	}

	public void swapCurrentTeam() {
		this.current_team = (this.current_team == Piece.Team.BLACKS) ? Piece.Team.WHITES : Piece.Team.BLACKS;
	}

	public GameEvent doLastEvent(int index_from, int index_to) {
		if (this.game_events.Count == 0)
			return GameEvent.NONE;
		GameEvent game_event = this.game_events.Dequeue ();
		if (this.event_to_handler.ContainsKey (game_event)) {
			this.event_to_handler [game_event].Invoke (index_from, index_to);
		}
		return game_event;
	}

	public void pushEvent(GameEvent game_event) {
		this.game_events.Enqueue (game_event);
	}

	public bool isCheckBlacks() {
		return this.check_blacks;
	}

	public bool isCheckWhites() {
		return this.check_whites;
	}

	public bool hasWinBlacks() {
		return this.win_blacks;
	}

	public bool hasWinWhites() {
		return this.win_whites;
	}

	private bool handleEventMove(int index_from, int index_to) {
		this.pieces [index_to].team = this.pieces [index_from].team;
		this.pieces [index_to].type = this.pieces [index_from].type;
		this.pieces [index_from].team = Piece.Team.NULL;
		this.pieces [index_from].type = Piece.Type.NULL;
		return true;
	}

	private bool handleEventCastle(int index_1, int index_2) {
		Piece.Team team_index_2 = this.pieces [index_1].team;
		Piece.Type type_index_2 = this.pieces [index_1].type;
		this.pieces [index_1].team = this.pieces [index_2].team;
		this.pieces [index_1].type = this.pieces [index_2].type;
		this.pieces [index_2].team = team_index_2;
		this.pieces [index_2].type = type_index_2;
		return true;
	}

	private bool handleEventCheck(int index_from, int index_to) {
		if (this.current_team == Piece.Team.BLACKS) {
			this.check_blacks = true;
		} else {
			this.check_whites = true;
		}
		return true;
	}

	private bool handleEventCheckMate(int index_from, int index_to) {
		if (this.current_team == Piece.Team.BLACKS) {
			this.win_blacks = true;
		} else {
			this.win_whites = true;
		}
		return true;
	}
}
