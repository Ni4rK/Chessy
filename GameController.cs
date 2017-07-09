using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	[SerializeField] private Material[] theming;
	[SerializeField] private GameObject highlightor;
	[SerializeField] private GameObject selector;
	[SerializeField] private GameObject board;
	[SerializeField] private GameObject[] board_objects;
	[SerializeField] private GameObject options_menu;
	[SerializeField] private GameObject promote_menu;
	[SerializeField] private GameObject information;
	[SerializeField] private GameObject information_ai;
	[SerializeField] private DisplayMode display_mode;
	[SerializeField] private Vector3 board_origin;
	[SerializeField] private float square_size;

	private InputManager input_manager;
	private BoardManager board_manager;
	private Referee referee;
	private Core jarvis;

	private int option_theming;
	private bool option_rotation;
	private bool is_rotating;
	private bool option_ai;
	private bool is_ai_thinking;
	private bool is_ai_moving;

	private int moving_index;
	private int selected_index;
	private int[] board_indexes;

	private Dictionary<InputManager.InputType, Func<bool>> input_to_handler;
	private Dictionary<GameEvent, Func<int, int, bool>> event_to_handler;
	private Dictionary<string, Func<InputManager.InputHit, bool>> click_to_handler;
	private Dictionary<string, Func<InputManager.InputHit, bool>> move_to_handler;



	public void Start () {
		this.option_theming = 0;
		this.option_rotation = false;
		this.is_rotating = false;
		this.option_ai = false;
		this.is_ai_thinking = false;
		this.is_ai_moving = false;
		this.moving_index = -1;
		this.selected_index = -1;
		this.board_indexes = new int[64];
		for (int i = 0; i < 64; i++) {
			this.board_indexes [i] = -1;
		}
		this.input_manager = new InputManager (this.display_mode);
		this.board_manager = new BoardManager ();
		this.referee = new Referee (this.board_manager);
		this.jarvis = new Core (this.board_manager, this.referee);
		this.board_manager.create ("default");
		this.input_to_handler = new Dictionary<InputManager.InputType, Func<bool>> ();
		this.input_to_handler [InputManager.InputType.NONE] = this.handlerNull;
		this.input_to_handler [InputManager.InputType.MOUSE_CLICK] = this.handleMouseClick;
		this.input_to_handler [InputManager.InputType.MOUSE_MOVE] = this.handleMouseMove;
		this.event_to_handler = new Dictionary<GameEvent, Func<int, int, bool>> ();
		this.event_to_handler [GameEvent.NONE] = this.handlerNull;
		this.event_to_handler [GameEvent.MOVE] = this.handleEventMove;
		this.event_to_handler [GameEvent.ATTACK] = this.handleEventAttack;
		this.event_to_handler [GameEvent.CASTLE_KINGSIDE] = this.handleEventCastleKingSide;
		this.event_to_handler [GameEvent.CASTLE_QUEENSIDE] = this.handleEventCastleQueenSide;
		this.event_to_handler [GameEvent.PROMOTE] = this.handleEventPromote;
		this.event_to_handler [GameEvent.CHECK] = this.handleEventCheck;
		this.event_to_handler [GameEvent.CHECKMATE] = this.handleEventCheckMate;
		this.event_to_handler [GameEvent.WIN_BLACKS] = this.handleEventWinBlacks;
		this.event_to_handler [GameEvent.WIN_WHITES] = this.handleEventWinWhites;
		this.event_to_handler [GameEvent.DRAW] = this.handleEventDraw;
		this.click_to_handler = new Dictionary<string, Func<InputManager.InputHit, bool>> ();
		this.click_to_handler ["board"] = this.handleClickOnBoard;
		this.click_to_handler ["newgame"] = this.handleClickOnNewGame;
		this.click_to_handler ["options"] = this.handleClickOnOptions;
		this.click_to_handler ["quit"] = this.handleClickOnQuit;
		this.click_to_handler ["option_ai"] = this.handleClickOnOptionAI;
		this.click_to_handler ["option_rotate"] = this.handleClickOnOptionRotate;
		this.click_to_handler ["option_theme"] = this.handleClickOnOptionTheme;
		this.click_to_handler ["promote_rook"] = this.handleClickOnPromoteRook;
		this.click_to_handler ["promote_knight"] = this.handleClickOnPromoteKnight;
		this.click_to_handler ["promote_bishop"] = this.handleClickOnPromoteBishop;
		this.click_to_handler ["promote_queen"] = this.handleClickOnPromoteQueen;
		this.move_to_handler = new Dictionary<string, Func<InputManager.InputHit, bool>> ();
		this.move_to_handler ["board"] = this.handleMoveOnBoard;
		this.move_to_handler ["newgame"] = this.handleMoveOnMenu;
		this.move_to_handler ["options"] = this.handleMoveOnMenu;
		this.move_to_handler ["quit"] = this.handleMoveOnMenu;
		this.move_to_handler ["option_ai"] = this.handleMoveOnMenu;
		this.move_to_handler ["option_rotate"] = this.handleMoveOnMenu;
		this.move_to_handler ["option_theme"] = this.handleMoveOnMenu;
		this.move_to_handler ["promote_rook"] = this.handleMoveOnPromote;
		this.move_to_handler ["promote_knight"] = this.handleMoveOnPromote;
		this.move_to_handler ["promote_bishop"] = this.handleMoveOnPromote;
		this.move_to_handler ["promote_queen"] = this.handleMoveOnPromote;
	}

	public void Update () {
		if (this.moving_index != -1 && !this.board_objects [this.moving_index].GetComponent<PieceController> ().isMoving ()) {
			this.moving_index = -1;
			if (this.option_rotation) {
				this.board.GetComponent<BoardController> ().rotate ();
				this.is_rotating = true;
			} else if (this.option_ai && !this.is_ai_moving) {
				this.is_ai_thinking = this.jarvis.think ();
				this.information_ai.GetComponent<TextMesh> ().text = "AI is thinking...";
			} else if (this.is_ai_moving) {
				this.is_ai_moving = false;
			}
		}
		if (this.is_rotating && !this.board.GetComponent<BoardController> ().isRotating ()) {
			this.is_rotating = false;
			if (this.option_ai && !this.is_ai_moving) {
				this.is_ai_thinking = this.jarvis.think ();
				this.information_ai.GetComponent<TextMesh> ().text = "AI is thinking...";
			} else if (this.is_ai_moving) {
				this.is_ai_moving = false;
			}
		}
		if (this.is_ai_thinking && !this.jarvis.isThinking ()) {
			this.is_ai_thinking = false;
			this.information_ai.GetComponent<TextMesh> ().text = "";
			int[] ai_decision = this.jarvis.decision ();
			this.is_ai_moving = true;
			if (this.referee.move (ai_decision[0], ai_decision[1])) {
				this.is_ai_moving = true;
				this.information.GetComponent<TextMesh>().text = "";
				Board default_board = this.board_manager.get ("default");
				GameEvent game_event;
				while ((game_event = default_board.doLastEvent (ai_decision [0], ai_decision [1])) != GameEvent.NONE) {
					this.event_to_handler [game_event].Invoke (ai_decision [0], ai_decision [1]);
				}
				default_board.swapCurrentTeam ();
			}
		}
		InputManager.InputType current_input = this.input_manager.getNext ();
		this.input_to_handler [current_input].Invoke ();
	}

	private bool handleMouseClick() {
		InputManager.InputHit hit = this.input_manager.getHit ();
		if (this.click_to_handler.ContainsKey (hit.gameobject.tag)) {
			return this.click_to_handler [hit.gameobject.tag].Invoke (hit);
		}
		return false;
	}

	private bool handleClickOnBoard(InputManager.InputHit hit) {
		int clicked_index = this.positionToIndex (hit.position);
		if (this.selected_index == clicked_index) {
			this.selected_index = -1;
			this.selector.SetActive (false);
		} else if (!this.is_ai_thinking && this.moving_index == -1 && !this.is_rotating && this.selected_index != -1) {
			if (this.referee.move (selected_index, clicked_index)) {
				this.information.GetComponent<TextMesh>().text = "";
				Board default_board = this.board_manager.get ("default");
				GameEvent game_event;
				while ((game_event = default_board.doLastEvent (selected_index, clicked_index)) != GameEvent.NONE) {
					this.event_to_handler [game_event].Invoke (selected_index, clicked_index);
				}
				default_board.swapCurrentTeam ();
			}
			this.selected_index = -1;
			this.selector.SetActive (false);
		} else if (this.board_indexes [clicked_index] != -1) {
			this.selected_index = clicked_index;
			this.selector.SetActive (true);
			this.selector.transform.position = this.indexToPosition (clicked_index);
			return true;
		} else {
			return false;
		}
		return true;
	}

	private bool handleClickOnNewGame(InputManager.InputHit hit) {
		this.board_manager.get ("default").reset ();
		this.jarvis.stop ();
		this.is_ai_thinking = false;
		this.information.GetComponent<TextMesh>().text = "";
		this.information_ai.GetComponent<TextMesh>().text = "";
		for (int i = 0; i < 64; i++) {
			this.board_indexes [i] = -1;
		}
		this.selector.SetActive (false);
		this.selected_index = -1;
		Vector3 position;
		for (int i = 0; i < 16; i++) {
			this.board_indexes [i] = i;
			position = this.indexToPosition (i);
			this.board_objects [i].GetComponent<PieceController> ().show ();
			this.board_objects [i].GetComponent<PieceController> ().setPieceType (Piece.Type.PAWN);
			this.board_objects [i].GetComponent<PieceController> ().move (position.x, position.z);
			this.board_indexes [64 - 1 - i] = 16 + i;
			position = this.indexToPosition (64 - 1 - i);
			this.board_objects [16 + i].GetComponent<PieceController> ().show ();
			this.board_objects [16 + i].GetComponent<PieceController> ().setPieceType (Piece.Type.PAWN);
			this.board_objects [16 + i].GetComponent<PieceController> ().move (position.x, position.z);
		}
		return true;
	}

	private bool handleClickOnOptions(InputManager.InputHit hit) {
		this.options_menu.SetActive (!this.options_menu.activeSelf);
		return true;
	}

	private bool handleClickOnQuit(InputManager.InputHit hit) {
		this.jarvis.stop ();
		this.information.GetComponent<TextMesh> ().text = "Waiting jarvis to stop";
		while (this.jarvis.isThinking ()) {
		}
		this.theming [0].SetColor ("_Color", Color.black);
		this.theming [0].SetFloat ("_Metallic", 0.21f);
		this.theming [0].SetFloat ("_Glossiness", 0.46f);
		this.theming [0].SetColor ("_EmissionColor", new Color (0.1f, 0.1f, 0.1f));
		this.theming [1].SetColor ("_Color", Color.white);
		this.theming [1].SetFloat ("_Metallic", 0.21f);
		this.theming [1].SetFloat ("_Glossiness", 0.46f);
		this.theming [1].SetColor ("_EmissionColor", new Color (0.1f, 0.1f, 0.1f));
		this.theming [2].SetFloat ("_Metallic", 1);
		this.theming [2].SetFloat ("_Glossiness", 0.5f);
		this.theming [2].SetColor ("_EmissionColor", new Color (0.2f, 0.2f, 0.2f));
		this.theming [3].SetColor ("_Color", new Color(0, 0.506f, 0.667f));
		this.theming [3].SetFloat ("_Metallic", 1);
		this.theming [3].SetFloat ("_Glossiness", 0);
		this.theming [3].SetColor ("_EmissionColor", new Color (0.2f, 0.2f, 0.2f));
		this.information.GetComponent<TextMesh> ().text = "Successfully Quit";
		Application.Quit ();
		return true;
	}

	private bool handleClickOnOptionAI(InputManager.InputHit hit) {
		this.option_ai = !this.option_ai;
		hit.gameobject.transform.parent.GetComponentInChildren<TextMesh>().text = (this.option_ai) ? "Deactivate AI" : "Activate AI";
		return true;
	}

	private bool handleClickOnOptionRotate(InputManager.InputHit hit) {
		this.option_rotation = !this.option_rotation;
		hit.gameobject.transform.parent.GetComponentInChildren<TextMesh>().text = (this.option_rotation) ? "Deactivate rotation" : "Activate rotation";
		return true;
	}

	private bool handleClickOnOptionTheme(InputManager.InputHit hit) {
		if (this.option_theming % 3 == 0) { // gold
			this.theming [0].SetColor ("_Color", new Color(0.694f, 0, 0, 1));
			this.theming [0].SetFloat ("_Metallic", 1);
			this.theming [0].SetFloat ("_Glossiness", 0.52f);
			this.theming [0].SetColor ("_EmissionColor", Color.black);
			this.theming [1].SetColor ("_Color", new Color(1, 0.914f, 0.522f));
			this.theming [1].SetFloat ("_Metallic", 1);
			this.theming [1].SetFloat ("_Glossiness", 0.43f);
			this.theming [1].SetColor ("_EmissionColor", new Color (0.1f, 0.1f, 0.1f));
			this.theming [2].SetFloat ("_Metallic", 1);
			this.theming [2].SetFloat ("_Glossiness", 0.5f);
			this.theming [2].SetColor ("_EmissionColor", new Color (1, 1, 1));
			this.theming [3].SetColor ("_Color", new Color(0.8f, 0.8f, 0.8f));
			this.theming [3].SetFloat ("_Metallic", 1);
			this.theming [3].SetFloat ("_Glossiness", 0);
			this.theming [3].SetColor ("_EmissionColor", new Color (0, 0, 0));
		} else if (this.option_theming % 3 == 1) { // wood
			this.theming [0].SetColor ("_Color", new Color(0.380f, 0.435f, 0.471f));
			this.theming [0].SetFloat ("_Metallic", 1);
			this.theming [0].SetFloat ("_Glossiness", 0);
			this.theming [0].SetColor ("_EmissionColor", new Color (0, 0, 0));
			this.theming [1].SetColor ("_Color", new Color (0.871f, 0.722f, 0.529f));
			this.theming [1].SetFloat ("_Metallic", 0.59f);
			this.theming [1].SetFloat ("_Glossiness", 0);
			this.theming [1].SetColor ("_EmissionColor", new Color (0.1f, 0.1f, 0.1f));
			this.theming [2].SetFloat ("_Metallic", 1);
			this.theming [2].SetFloat ("_Glossiness", 0);
			this.theming [2].SetColor ("_EmissionColor", new Color (0, 0, 0));
			this.theming [3].SetColor ("_Color", new Color(0, 0.506f, 0.667f));
			this.theming [3].SetFloat ("_Metallic", 1);
			this.theming [3].SetFloat ("_Glossiness", 0);
			this.theming [3].SetColor ("_EmissionColor", new Color (0, 0, 0));
		} else if (this.option_theming % 3 == 2) { // normal 194 255 170
			this.theming [0].SetColor ("_Color", Color.black);
			this.theming [0].SetFloat ("_Metallic", 0.21f);
			this.theming [0].SetFloat ("_Glossiness", 0.46f);
			this.theming [0].SetColor ("_EmissionColor", new Color (0.1f, 0.1f, 0.1f));
			this.theming [1].SetColor ("_Color", Color.white);
			this.theming [1].SetFloat ("_Metallic", 0.21f);
			this.theming [1].SetFloat ("_Glossiness", 0.46f);
			this.theming [1].SetColor ("_EmissionColor", new Color (0.1f, 0.1f, 0.1f));
			this.theming [2].SetFloat ("_Metallic", 1);
			this.theming [2].SetFloat ("_Glossiness", 0.5f);
			this.theming [2].SetColor ("_EmissionColor", new Color (0.2f, 0.2f, 0.2f));
			this.theming [3].SetColor ("_Color", new Color(0, 0.506f, 0.667f));
			this.theming [3].SetFloat ("_Metallic", 1);
			this.theming [3].SetFloat ("_Glossiness", 0);
			this.theming [3].SetColor ("_EmissionColor", new Color (0.2f, 0.2f, 0.2f));
		}
		this.option_theming += 1;
		return true;
	}

	private bool handleClickOnPromoteRook(InputManager.InputHit hit) {
		return true;
	}

	private bool handleClickOnPromoteKnight(InputManager.InputHit hit) {
		return true;
	}

	private bool handleClickOnPromoteBishop(InputManager.InputHit hit) {
		return true;
	}

	private bool handleClickOnPromoteQueen(InputManager.InputHit hit) {
		return true;
	}

	private bool handleMouseMove() {
		InputManager.InputHit hit = this.input_manager.getHit ();
		if (this.move_to_handler.ContainsKey (hit.gameobject.tag)) {
			return this.move_to_handler [hit.gameobject.tag].Invoke (hit);
		}
		return false;
	}

	private bool handleMoveOnBoard(InputManager.InputHit hit) {
		int clicked_index = this.positionToIndex (hit.position);
		this.highlight (false, this.selected_index != clicked_index, false, this.indexToPosition (clicked_index));
		return true;
	}

	private bool handleMoveOnMenu(InputManager.InputHit hit) {
		this.highlight (true, false, false, hit.gameobject.transform.position);
		return true;
	}

	private bool handleMoveOnPromote(InputManager.InputHit hit) {
		this.highlight (false, false, true, hit.gameobject.transform.position);
		return true;
	}

	private bool handleEventMove(int index_from, int index_to) {
		this.moving_index = this.board_indexes [index_from];
		if (this.moving_index != -1) {
			Vector3 position_to = this.indexToPosition (index_to);
			this.board_objects [this.moving_index].GetComponent<PieceController> ().move (position_to.x, position_to.z);
			this.board_indexes [index_to] = this.moving_index;
			this.board_indexes [index_from] = -1;
		}
		return true;
	}

	private bool handleEventAttack(int index_from, int index_to) {
		if (this.board_indexes [index_to] != -1) {
			this.board_objects [this.board_indexes [index_to]].GetComponent<PieceController> ().hide ();
			this.handleEventMove (index_from, index_to);
		}
		return true;
	}

	private bool handleEventCastleKingSide(int index_from, int index_to) {
		return true;
	}

	private bool handleEventCastleQueenSide(int index_from, int index_to) {
		return true;
	}

	private bool handleEventPromote(int index_from, int index_to) {
		return true;
	}

	private bool handleEventCheck(int index_from, int index_to) {
		this.information.GetComponent<TextMesh>().text = "Check !";
		return true;
	}

	private bool handleEventCheckMate(int index_from, int index_to) {
		this.information.GetComponent<TextMesh>().text = "Check Mate !";
		return true;
	}

	private bool handleEventWinBlacks(int index_from, int index_to) {
		this.information.GetComponent<TextMesh>().text = "Blacks win !";
		return true;
	}

	private bool handleEventWinWhites(int index_from, int index_to) {
		this.information.GetComponent<TextMesh>().text = "Whites win !";
		return true;
	}

	private bool handleEventDraw(int index_from, int index_to) {
		return true;
	}

	private void highlight(bool is_menu, bool is_board, bool is_promote, Vector3 position) {
		this.highlightor.SetActive (is_menu || is_board || is_promote);
		if (is_menu) {
			this.highlightor.transform.localScale = new Vector3 (1f, this.highlightor.transform.localScale.y, 0.15f);
			this.highlightor.transform.position = position;
		} else if (is_board) {
			this.highlightor.transform.localScale = new Vector3 (0.17f, this.highlightor.transform.localScale.y, 0.17f);
			this.highlightor.transform.position = position;
		} else if (is_promote) {
			this.highlightor.transform.localScale = new Vector3 (0.205f, this.highlightor.transform.localScale.y, 0.205f);
			this.highlightor.transform.position = position;
		}
	}

	private Vector3 indexToPosition(int index) {
		Vector3 result = new Vector3 ();
		result.x = this.board_origin.x + (index % 8) * this.square_size + this.square_size / 2;
		result.y = 0;
		result.z = this.board_origin.z + (index / 8) * this.square_size + this.square_size / 2;
		if (this.board.GetComponent<BoardController> ().isRotated ()) {
			result.x = this.board_origin.x + this.square_size * 7 - result.x + this.square_size / 2;
			result.z = this.board_origin.z + this.square_size * 7 - result.z + this.square_size / 2;
		}
		return result;
	}

	private int positionToIndex(Vector3 position) {
		int index_x = (int)(((position.x - this.board_origin.x) - (position.x - this.board_origin.x) % this.square_size) / this.square_size);
		int index_z = (int)(((position.z - this.board_origin.z) - (position.z - this.board_origin.z)% this.square_size) / this.square_size);
		if (this.board.GetComponent<BoardController> ().isRotated ()) {
			index_x = 7 - index_x;
			index_z = 7 - index_z;
		}
		return index_x + 8 * index_z;
	}

	private bool handlerNull() {
		return true;
	}

	private bool handlerNull(int a, int b) {
		return true;
	}
}
