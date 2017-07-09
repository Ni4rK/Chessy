using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 /*
 ** Animation handling
 */
public class PieceController : MonoBehaviour {
    bool is_moving = false;
    float stepX;
    float stepZ;
    int count;
    private Dictionary<Piece.Type, Func<float, float, bool>> firstMove;
    private Dictionary<Piece.Type, Func<float, float, bool>> secondMove;
    private float x;
    private float z;
    private Vector3 origin;
    private Piece.Type type = Piece.Type.NULL;
    public void setPieceType(Piece.Type type)
    {
        this.type = type;
    }
    void Start () {
		is_moving = false;
        stepX = 0;
        stepZ = 0;
        count = 0;
        firstMove = new Dictionary<Piece.Type, Func<float, float, bool>>();
        secondMove = new Dictionary<Piece.Type, Func<float, float, bool>>();
        firstMove.Add(Piece.Type.NULL, null);
        firstMove.Add(Piece.Type.PAWN, pawnFirstMove);
        firstMove.Add(Piece.Type.ROOK, rookFirstMove);
        firstMove.Add(Piece.Type.KNIGHT, knightFirstMove);
        firstMove.Add(Piece.Type.BISHOP, bishopFirstMove);
        firstMove.Add(Piece.Type.QUEEN, queenFirstMove);
        firstMove.Add(Piece.Type.KING, kingFirstMove);

        secondMove.Add(Piece.Type.NULL, null);
        secondMove.Add(Piece.Type.PAWN, null);
        secondMove.Add(Piece.Type.ROOK, null);
        secondMove.Add(Piece.Type.KNIGHT, knightSecondMove);
        secondMove.Add(Piece.Type.BISHOP, null);
        secondMove.Add(Piece.Type.QUEEN, null);
        secondMove.Add(Piece.Type.KING, null);

		this.hide ();
    }

    void Update () {
		if (is_moving && firstMove != null && secondMove != null)
        {
            if (firstMove[type] != null)
                firstMove[type].Invoke(x, z);
            if (secondMove[type] != null)
                secondMove[type].Invoke(x, z);
            if (count <= 0)
				is_moving = false;
            count -= 1;
        }
		if (is_moving == false && count <= 0)
            count = 9;
	}

	public void hide() {
		if (this.GetComponent<MeshRenderer> () == null) {
			foreach (MeshRenderer render in this.GetComponentsInChildren<MeshRenderer> ())
				render.enabled = false;
			foreach (Collider render in this.GetComponentsInChildren<Collider> ())
				render.enabled = false;
		} else {
			this.GetComponent<MeshRenderer> ().enabled = false;
			this.GetComponent<Collider> ().enabled = false;
		}
	}

	public void show() {
		if (this.GetComponent<MeshRenderer> () == null) {
			foreach (MeshRenderer render in this.GetComponentsInChildren<MeshRenderer> ())
				render.enabled = true;
			foreach (Collider render in this.GetComponentsInChildren<Collider> ())
				render.enabled = true;
		} else {
			this.GetComponent<MeshRenderer> ().enabled = true;
			this.GetComponent<Collider> ().enabled = true;
		}
	}

	public void move(float x, float z)
	{
		origin = transform.position;
		is_moving = true;
		stepX = (x - origin.x) / 10;
		stepZ = (z - origin.z) / 10;
		count = 9;
		this.x = x;
		this.z = z;
	}

	public bool isMoving()
	{
		return this.is_moving;
	}


	/*
     * First move animation by pieceType
     */

    private bool pawnFirstMove(float x, float z)
	{
        Vector3 newPosition = transform.position;

        newPosition.x += (stepX);
        newPosition.z += (stepZ);
        transform.position = newPosition;
        return (true);
    }
    private bool rookFirstMove(float x, float z)
    {
        Vector3 newPosition = transform.position;

        newPosition.x += (stepX);
        newPosition.z += (stepZ);
        transform.position = newPosition;
        return (true);
    }
    public bool knightFirstMove(float x, float z)
    {
        Vector3 newPosition = transform.position;

        newPosition.x += (stepX);
        transform.position = newPosition;
        return (true);
    }
    private bool bishopFirstMove(float x, float z)
    {
        Vector3 newPosition = transform.position;

        newPosition.x += (stepX);
        newPosition.z += (stepZ);
        transform.position = newPosition;
        return (true);
    }
    private bool queenFirstMove(float x, float z)
    {
        Vector3 newPosition = transform.position;

        newPosition.x += (stepX);
        newPosition.z += (stepZ);
        transform.position = newPosition;
        return (true);
    }
    private bool kingFirstMove(float x, float z)
    {
        Vector3 newPosition = transform.position;

        newPosition.x += (stepX);
        newPosition.z += (stepZ);
        transform.position = newPosition;
        return (true);
    }

    /*
     * Second move animation  by pieceType
     */
    private bool knightSecondMove(float x, float z)
    {
        Vector3 newPosition = transform.position;

        newPosition.z += (stepZ);
        transform.position = newPosition;
        return (true);
    }
}
