using System.Collections.Generic;

public class Node {
	public int board;
	public int value;
	public bool evaluated;
	public int[] decision;
	public LinkedList<Node> children;

	public Node() {
		this.board = -1;
		this.value = 0;
		this.evaluated = false;
		this.decision = new int[2];
		this.decision [0] = 0;
		this.decision [1] = 0;
		this.children = new LinkedList<Node> ();
	}
}
