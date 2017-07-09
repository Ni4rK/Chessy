using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class Referee {

	private BoardManager board_manager;
	private Board working_board;
	private Dictionary<Piece.Type, Func<int, int, bool>> type_to_referee;
    private const int DIRECTION_NONE = 0;
    private const int FORWARD_LEFT = 1;
    private const int FORWARD = 2;
    private const int FORWARD_RIGHT = 3;
    private const int LEFT = 4;
    private const int RIGHT = 5;
    private const int BACKWARD_LEFT = 6;
    private const int BACKWARD = 7;
    private const int BACKWARD_RIGHT = 8;
    private int[] directions = new int[9];

	public Referee(BoardManager board_manager) {
		this.board_manager = board_manager;
		this.type_to_referee = new Dictionary<Piece.Type, Func<int, int, bool>> ();
		this.type_to_referee [Piece.Type.PAWN] = this.refereePawn;
		this.type_to_referee [Piece.Type.ROOK] = this.refereeRook;
		this.type_to_referee [Piece.Type.KNIGHT] = this.refereeKnight;
		this.type_to_referee [Piece.Type.BISHOP] = this.refereeBishop;
		this.type_to_referee [Piece.Type.QUEEN] = this.refereeQueen;
		this.type_to_referee [Piece.Type.KING] = this.refereeKing;
        directions[DIRECTION_NONE] = 0;
        directions[FORWARD_LEFT] = 7;
        directions[FORWARD_RIGHT] = 9;
        directions[FORWARD] = 8;
        directions[LEFT] = -1;
        directions[RIGHT] = 1;
        directions[BACKWARD_LEFT] = -9;
        directions[BACKWARD_RIGHT] = -7;
        directions[BACKWARD] = -8;
	}

	public bool move(int index_from, int index_to, string board = "default") {
		this.working_board = this.board_manager.get (board);
		return this.move (index_from, index_to);
	}

	public bool move(int index_from, int index_to, int board) {
		this.working_board = this.board_manager.get (board);
		return this.move (index_from, index_to);
	}

	private bool move(int index_from, int index_to) {
		bool result = false;
		if (this.working_board.getPiece (index_from).team != this.working_board.getCurrentTeam ()) {
			return result;
		}
		if (this.type_to_referee.ContainsKey (this.working_board.getPiece (index_from).type)) {
			result = this.type_to_referee [this.working_board.getPiece (index_from).type].Invoke (index_from, index_to);
			if (result) {
				if (!boardContainsTeamKing ())
					MonoBehaviour.print ("team " + this.working_board.getCurrentTeam () + " a perdu!");
			}
		}
        return result;
	}

    private bool boardContainsTeamKing()
	{
		for (int i = 0; i < 64; i++) {
			if (this.working_board.getPiece (i).type == Piece.Type.KING && this.working_board.getPiece (i).team == this.working_board.getCurrentTeam ()) {
				return true;
			}
		}
		return false;
	}
    private bool checkEnemyType(Piece.Type type, int direction, int distance)
    {
        if (type == Piece.Type.BISHOP
            && 
            (direction == FORWARD_LEFT 
            || direction == FORWARD_RIGHT 
            || direction == BACKWARD_LEFT 
            || direction == BACKWARD_RIGHT))
            return (true);
        else if (type == Piece.Type.PAWN
            && 
            (direction == FORWARD_LEFT 
            || direction == FORWARD_RIGHT)
            && distance == 1)
            return true;
        else if (type == Piece.Type.QUEEN)
            return true;
        else if (type == Piece.Type.ROOK
            && 
            (direction == FORWARD 
            || direction == BACKWARD 
            || direction == LEFT 
            || direction == RIGHT))
            return true;
        return false;
    }
    public bool isCheck() {
        int king_index = 0;
        int index_to = 0;
        int cur_direction = 0;

        for (int i = 0; i < 64; i++)
        {
			if (this.working_board.getPiece (i).type == Piece.Type.KING && this.working_board.getPiece (i).team == this.working_board.getCurrentTeam ()) {
				king_index = i;
				break;
			}
        }
        for (int i = 1; i < 9; i++)
        {
            index_to = getMaxIndexTo(king_index, i);
            cur_direction = directions[i];
            if (cur_direction < 0)
                cur_direction = -cur_direction;
            if (index_to != -1)
            {
				if (this.working_board.getPiece (index_to).type != Piece.Type.NULL &&
				    this.working_board.getPiece (index_to).team != this.working_board.getPiece (king_index).team &&
				    checkEnemyType (this.working_board.getPiece (index_to).type, i, levelDifference (king_index, index_to, cur_direction))) {
					return (true);
				}
            }
        }
		return false;
	}

	private bool isCheckMate() {
		return false;
	}

    private int levelDifference(int index_from, int index_to, int direction) {
        int distance = 0;

        if (direction == LEFT || direction == RIGHT)
            distance = index_to - index_from;
        else
            distance = (index_to / 8) - (index_from / 8);
        if (distance < 0)
            distance = -distance;
        return distance;
    }
    private int getDirection(int index_from, int index_to) {
        int direction = DIRECTION_NONE;
        int movement = index_to - index_from;
        int levelDiff = levelDifference(index_from, index_to, direction);

        if (levelDiff == 0)
        {
			if (movement > 0)
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? RIGHT : LEFT;
            else
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? LEFT : RIGHT;
        }
        else if ((movement) % 7 == 0)
        {
            if (index_to - index_from > 0)
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? FORWARD_LEFT : BACKWARD_RIGHT;
            else
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? BACKWARD_RIGHT : FORWARD_LEFT;
        }
        else if ((movement) % 8 == 0)
        {
            if (index_to - index_from > 0)
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? FORWARD : BACKWARD;
            else
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? BACKWARD : FORWARD;
        }
        else if ((movement) % 9 == 0)
        {
            if (index_to - index_from > 0)
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? FORWARD_RIGHT : BACKWARD_LEFT;
            else
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? BACKWARD_LEFT : FORWARD_RIGHT;
        }
        return direction;
    }
    private int getKnightDirection(int index_from, int index_to)
    {
        int direction = DIRECTION_NONE;
        int movement = index_to - index_from;
        int levelDiff = levelDifference(index_from, index_to, direction);

        if (levelDiff == 1)
        {
            if (movement == 10)
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? FORWARD_RIGHT : BACKWARD_LEFT;
            else if (movement == 6)
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? FORWARD_LEFT : BACKWARD_RIGHT;
            else if (movement == -10)
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? BACKWARD_LEFT : FORWARD_RIGHT;
            else if (movement == -6)
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? BACKWARD_RIGHT : FORWARD_LEFT;
        }
        else if (levelDiff == 2)
        {
            if (movement == 15)
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? FORWARD_LEFT : BACKWARD_RIGHT;
            else if (movement == 17)
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? FORWARD_RIGHT: BACKWARD_LEFT;
            else if (movement == -15)
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? BACKWARD_RIGHT: FORWARD_LEFT;
            else if (movement == -17)
				direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? BACKWARD_LEFT: FORWARD_RIGHT;
        }
        return direction;
    }
    private bool hasReachedBorder(int index)
    {
        int index_level;

        index_level = index / 8;
        if (index == (index_level * 8)  - 1 //right border
            || index == (index_level * 8) - 7 //left border
            || index_level == 7  //up
            || index_level == 0) //down
            return true;
        return false;
    }
    private int getMaxIndexTo(int index_from, int direction)
    {
        int id = index_from;
        int temp_direction = directions[direction];
        int index_level = 0;
		temp_direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? temp_direction : -temp_direction;

        index_level = index_from / 8;
        do
        {
            if (id + temp_direction < 0 || id + temp_direction > 64)
                return (-1);
            id += temp_direction;
			if (this.working_board.getPiece (id).type != Piece.Type.NULL)
                return id;
            index_level++;
        } while (!hasReachedBorder(id));
        return (id);
    }
    private int isLineFree(int index_from, int index_to, int direction)
    {
        int distance = levelDifference(index_from, index_to, direction);
        int id = index_from;
        int temp_direction = directions[direction];

		if (this.working_board.getPiece (index_from).type == Piece.Type.KNIGHT)
        {
			if (this.working_board.getPiece (index_to).team == Piece.Team.NULL)
                return 1;
			else if (this.working_board.getPiece (index_to).team != this.working_board.getPiece (index_from).team)
                return 2;
            return 0;
        }
		temp_direction = (this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? temp_direction : -temp_direction;
        if (distance == 0 && (direction == 5 || direction == 4))
        {
            distance = index_to - index_from;
            if (distance < 0)
                distance = -distance;
        }
        for (int i = 0; i < distance; i++)
        {
            id += temp_direction;
            if (id > 64 || id < 0)
                id -= temp_direction;
			if (this.working_board.getPiece (id).type != Piece.Type.NULL)
            {
				if (id == index_to && this.working_board.getPiece (id).team != this.working_board.getPiece (index_from).team)
                {
					if (this.working_board.getPiece (id).type == Piece.Type.KING)
                        return (3); // current_team won !
                    return (2); // 2 means you need to eat it. we should make that function !
                }
                return 0;
            }
        }
        return 1;
    }
    private bool refereePawn(int index_from, int index_to) {
        bool result = false;
        int direction = DIRECTION_NONE;
        int distance = 0;
        int game_event_type = 0;

        direction = getDirection(index_from, index_to);
        distance = levelDifference(index_from, index_to, direction);
        if (direction == FORWARD 
            && distance == 1)
        {
            game_event_type = isLineFree(index_from, index_to, direction);
            if (game_event_type != 1)
                result = false;
            else
            {
                result = true;
				this.working_board.pushEvent (GameEvent.MOVE);
            }
        }
        else if (direction == FORWARD 
            && distance == 2 
			&& index_from / 8 == ((this.working_board.getCurrentTeam () == Piece.Team.WHITES) ? 1 : 6))
        {
            game_event_type = isLineFree(index_from, index_to, direction);
            if (game_event_type == 1)
            {
                result = true;
				this.working_board.pushEvent (GameEvent.MOVE);
            }
        }
        else if ((direction == FORWARD_RIGHT || direction == FORWARD_LEFT) 
            && distance == 1)
        {
            game_event_type = isLineFree(index_from, index_to, direction);
            if (game_event_type != 2 && game_event_type != 3)
                result = false;
            else if (game_event_type == 3)
            {
                result = true;
				if (this.working_board.getCurrentTeam () == Piece.Team.WHITES)
					this.working_board.pushEvent (GameEvent.WIN_WHITES);
                else
					this.working_board.pushEvent (GameEvent.WIN_BLACKS);
            }
            else
            {
                result = true;
				this.working_board.pushEvent (GameEvent.ATTACK);
            }
        }
        //else if (direction == FORWARD && distance == 2 && current_board[index_from].hasMoved) { result = true };
        return result;
	}

	private bool refereeRook(int index_from, int index_to) {
        bool result = false;
        int direction = DIRECTION_NONE;
        int game_event_type = 0;

        direction = getDirection(index_from, index_to);
        if ((direction == FORWARD 
            || direction == BACKWARD 
            || direction == LEFT 
            || direction == RIGHT))
        {
            game_event_type = isLineFree(index_from, index_to, direction);
            if (game_event_type != 0)
                result = true;
            if (game_event_type == 1)
				this.working_board.pushEvent (GameEvent.MOVE);
            else if (game_event_type == 2)
				this.working_board.pushEvent (GameEvent.ATTACK);
            else if (game_event_type == 3)
            {
				if (this.working_board.getCurrentTeam () == Piece.Team.WHITES)
					this.working_board.pushEvent (GameEvent.WIN_WHITES);
                else
					this.working_board.pushEvent (GameEvent.WIN_BLACKS);
            }
        }
        return result;
    }

    private bool refereeKnight(int index_from, int index_to) {
        bool result = false;
        int direction = DIRECTION_NONE;
        int game_event_type = 0;

        direction = getKnightDirection(index_from, index_to);
        if ((direction == FORWARD_RIGHT
            || direction == FORWARD_LEFT
            || direction == BACKWARD_RIGHT
            || direction == BACKWARD_LEFT))
        {
            game_event_type = isLineFree(index_from, index_to, direction);
            if (game_event_type != 0)
                result = true;
            if (game_event_type == 1)
				this.working_board.pushEvent (GameEvent.MOVE);
            else if (game_event_type == 2)
				this.working_board.pushEvent (GameEvent.ATTACK); // Here change to GameEvent.ATTACK;
            else if (game_event_type == 3)
            {
				if (this.working_board.getCurrentTeam () == Piece.Team.WHITES)
					this.working_board.pushEvent (GameEvent.WIN_WHITES);
                else
					this.working_board.pushEvent (GameEvent.WIN_BLACKS);
            }
        }
        return result;
    }

    private bool refereeBishop(int index_from, int index_to) {
        bool result = false;
        int direction = DIRECTION_NONE;
        int game_event_type = 0;

        direction = getDirection(index_from, index_to);
        if ((direction == FORWARD_RIGHT 
            || direction == FORWARD_LEFT
            || direction == BACKWARD_RIGHT
            || direction == BACKWARD_LEFT ))
        {
            game_event_type = isLineFree(index_from, index_to, direction);
            if (game_event_type != 0)
                result = true;
            if (game_event_type == 1)
				this.working_board.pushEvent (GameEvent.MOVE);
            else if (game_event_type == 2)
				this.working_board.pushEvent (GameEvent.ATTACK); // Here change to GameEvent.ATTACK;
            else if (game_event_type == 3)
            {
				if (this.working_board.getCurrentTeam () == Piece.Team.WHITES)
					this.working_board.pushEvent (GameEvent.WIN_WHITES);
                else
					this.working_board.pushEvent (GameEvent.WIN_BLACKS);
            }
        }
        return result;
    }

    private bool refereeQueen(int index_from, int index_to) {
        bool result = false;
        int direction = DIRECTION_NONE;
        int game_event_type = 0;

        direction = getDirection(index_from, index_to);
        if (direction != DIRECTION_NONE)
        {
            game_event_type = isLineFree(index_from, index_to, direction);
            if (game_event_type != 0)
                result = true;
			if (game_event_type == 1)
				this.working_board.pushEvent (GameEvent.MOVE);
			else if (game_event_type == 2)
				this.working_board.pushEvent (GameEvent.ATTACK); // Here change to GameEvent.ATTACK;
            else if (game_event_type == 3)
            {
				if (this.working_board.getCurrentTeam () == Piece.Team.WHITES)
					this.working_board.pushEvent (GameEvent.WIN_WHITES);
                else
					this.working_board.pushEvent (GameEvent.WIN_BLACKS);
            }
        }
        return result;
	}

	private bool refereeKing(int index_from, int index_to) {
        bool result = false;
        int direction = DIRECTION_NONE;
        int distance = 0;
        int game_event_type = 0;

        direction = getDirection(index_from, index_to);
        distance = levelDifference(index_from, index_to, direction);
        if (direction != DIRECTION_NONE && distance == 1)
        {
            game_event_type = isLineFree(index_from, index_to, direction);
            if (game_event_type != 0)
                result = true;
			if (game_event_type == 1)
				this.working_board.pushEvent (GameEvent.MOVE);
			else if (game_event_type == 2)
				this.working_board.pushEvent (GameEvent.ATTACK); // Here change to GameEvent.ATTACK;
            else if (game_event_type == 3)
            {
				if (this.working_board.getCurrentTeam () == Piece.Team.WHITES)
					this.working_board.pushEvent (GameEvent.WIN_WHITES);
                else
					this.working_board.pushEvent (GameEvent.WIN_BLACKS);
            }
        }
        return result;
    }
}
