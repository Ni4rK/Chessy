using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Core {

	private BoardEvaluator board_evaluator;
	private BoardManager board_manager;
	private Referee referee;
	private Node root;
	private Piece.Team root_team;
	private int index_board;
	private const int max_depth = 2; // NEEDS TO BE GREATER (or equal) THAN 1

	private volatile bool is_thinking;
	private volatile bool is_stopped;
	private Thread ai_thread;



	// Use this for initialization
	public Core (BoardManager board_manager, Referee referee) {
		this.board_evaluator = new BoardEvaluator ();
		this.board_manager = board_manager;
		this.referee = referee;
		this.root = new Node ();
		this.root.board = 0;
		this.board_manager.create (0);
		this.root_team = Piece.Team.NULL;
		this.index_board = 1;
		this.is_thinking = false;
		this.is_stopped = false;
		this.ai_thread = null;
	}

	public bool think() {
		if (this.is_thinking) {
			return false;
		}
		this.is_thinking = true;
		this.is_stopped = false;
		this.ai_thread = new Thread (this.thinking);
		this.ai_thread.Start ();
		return true;
	}

	public void stop() {
		this.is_stopped = true;
	}

	public bool isThinking() {
		return this.is_thinking;
	}

	public int[] decision() {
		if (!this.root.evaluated) {
			return new int[] { 0, 0 };
		}
		return this.root.decision;
	}

	private void thinking() {
		this.index_board = 1;
		this.board_manager.get (0).copy (this.board_manager.get ("default"));
		this.root.evaluated = false;
		this.root.children.Clear ();
		this.root_team = this.board_manager.get (0).getCurrentTeam ();
		if (!this.createTreeRecursively (this.root, Core.max_depth) || !this.evaluateTreeRecursively (this.root) || !this.setBestDecision ()) {
			Debug.Log ((this.is_stopped) ? "AI stopped thinking" : "AI failed");
		}
		this.is_thinking = false;
	}

	private bool createTreeRecursively(Node parent_node, int current_depth) {
		if (current_depth == 0) {
			return true;
		}
		Node working_node = new Node ();
		working_node.board = this.index_board;
		this.board_manager.create (working_node.board);
		this.board_manager.get (working_node.board).copy (this.board_manager.get (parent_node.board));

		for (int index_from = 0; index_from < 64; index_from++) {
			for (int index_to = 0; index_to < 64; index_to++) {
				if (this.is_stopped) {
					return false;
				}
				if (index_to == index_from) {
					continue;
				}
				if (this.referee.move (index_from, index_to, working_node.board)) {
					Board working_board = this.board_manager.get (working_node.board);
					while (working_board.doLastEvent (index_from, index_to) != GameEvent.NONE) {
					}
					working_board.swapCurrentTeam ();
					working_node.decision [0] = index_from;
					working_node.decision [1] = index_to;
					parent_node.children.AddLast (working_node);
					this.index_board += 1;
					if (!this.createTreeRecursively (working_node, current_depth - 1)) {
						return false;
					}
					Node new_node = new Node ();
					new_node.board = index_board;
					working_node = new_node;
					this.board_manager.create (working_node.board);
					this.board_manager.get (working_node.board).copy (this.board_manager.get (parent_node.board));
				}
			}
		}
		return true;
	}

	private bool evaluateTreeRecursively(Node parent_node) {
		if (parent_node.children.Count == 0) {
			int number_moves = 0;
			for (int index_from = 0; index_from < 64; index_from++) {
				for (int index_to = 0; index_to < 64; index_to++) {
					if (this.is_stopped) {
						return false;
					}
					if (index_to == index_from) {
						continue;
					}
					if (this.referee.move (index_from, index_to, parent_node.board)) {
						number_moves += 1;
					}
				}
			}
			parent_node.value = this.board_evaluator.evaluate (this.board_manager.get (parent_node.board), number_moves);
			return true;
		}
		Piece.Team parent_team = this.board_manager.get (parent_node.board).getCurrentTeam ();
		foreach (Node child_node in parent_node.children) {
			if (this.is_stopped) {
				return false;
			}
			if (!this.evaluateTreeRecursively (child_node)) {
				return false;
			}
			if (!parent_node.evaluated ||
				parent_team == this.root_team && child_node.value < parent_node.value ||
				parent_team != this.root_team && child_node.value > parent_node.value) {
				parent_node.evaluated = true;
				parent_node.value = child_node.value;
			}
		}
		return true;
	}

	private bool setBestDecision() {
		foreach (Node child_node in this.root.children) {
			if (child_node.value == this.root.value) {
				this.root.decision = child_node.decision;
				return true;
			}
		}
		return false;
	}
}
