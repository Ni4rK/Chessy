using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour {

	private bool is_rotating;
	private float rotation_multiplier;
	private float rotation_divider;
	private float rotation_rest;

	// Use this for initialization
	void Start () {
		this.is_rotating = false;
		this.rotation_multiplier = 1.0f;
		this.rotation_divider = 20.0f;
		this.rotation_rest = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
		this.is_rotating = (this.rotation_rest != 0.0f);
		if (this.is_rotating) {
			Quaternion rotation_q = this.transform.rotation;
			Vector3 rotation_v = rotation_q.eulerAngles;
			rotation_v.y += this.rotation_multiplier * (180.0f / this.rotation_divider);
			rotation_q.eulerAngles = rotation_v;
			this.transform.rotation = rotation_q;
			this.rotation_rest -= 1.0f;
		}
	}

	public void rotate() {
		this.is_rotating = true;
		this.rotation_multiplier *= -1.0f;
		this.rotation_rest = this.rotation_divider;
	}

	public bool isRotating() {
		return this.is_rotating;
	}

	public bool isRotated() {
		return (this.rotation_multiplier == -1.0f);
	}
}
