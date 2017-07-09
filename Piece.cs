public class Piece {
	
	public enum Type {
		NULL,
		PAWN,
		ROOK,
		KNIGHT,
		BISHOP,
		QUEEN,
		KING
	}

	public enum Team {
		NULL,
		WHITES,
		BLACKS
	}



	public Type type;
	public Team team;



	public Piece() {
		this.type = Type.NULL;
		this.team = Team.NULL;
	}

	public Piece(Type type, Team team) {
		this.type = type;
		this.team = team;
	}

}
