using System;
using System.Collections;
using System.Collections.Generic;


public class BoardManager {

	private Dictionary<string, Board> name_to_board;
	private Dictionary<int, Board> numero_to_board;

	public BoardManager() {
		this.name_to_board = new Dictionary<string, Board> ();
		this.numero_to_board = new Dictionary<int, Board> ();
	}


	/*
	 * creates a new Board (a basic and empty one) binded to a name[string] or a numero[int]
	 */
	public bool create(string name) {
		if (this.name_to_board.ContainsKey (name)) {
			return false;
		}
		this.name_to_board [name] = new Board ();
		return true;
	}

	public bool create(int numero) {
		if (this.numero_to_board.ContainsKey (numero)) {
			return false;
		}
		this.numero_to_board [numero] = new Board ();
		return true;
	}


	/*
	 * returns the board according to its name[string] or numero[int]
	 */
	public Board get(string name) {
		if (!this.name_to_board.ContainsKey (name)) {
			return null;
		}
		return this.name_to_board [name];
	}

	public Board get(int numero) {
		if (!this.numero_to_board.ContainsKey (numero)) {
			return null;
		}
		return this.numero_to_board [numero];
	}


	/*
	 * deletes an existing board
	 */
	public bool delete(string name) {
		if (!this.name_to_board.ContainsKey (name)) {
			return false;
		}
		this.name_to_board.Remove (name);
		return true;
	}

	public bool delete(int numero) {
		if (!this.numero_to_board.ContainsKey (numero)) {
			return false;
		}
		this.numero_to_board.Remove (numero);
		return true;
	}
}
