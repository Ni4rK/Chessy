using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

public class InputManager {

	public enum InputType {
		NONE,
		MOUSE_CLICK,
		MOUSE_MOVE
	}

	public struct InputHit {
		public RaycastHit hit;
		public Vector3 position;
		public GameObject gameobject;
	}



	private InputHit input_hit;
	private GestureRecognizer gestures;
	private GameObject old_focused_object;
	private GameObject current_focused_object;
	private Vector3 head_position;
	private Vector3 gaze_direction;
	private DisplayMode mode;
	private bool has_hololens_click;
	private InputType current_input;
	private Dictionary<DisplayMode, Func<bool>> mode_to_handler;



	public InputManager(DisplayMode mode) {
		this.input_hit = new InputHit ();
		this.input_hit.hit = new RaycastHit ();
		this.input_hit.position = new Vector3 (0, 0, 0);
		this.input_hit.gameobject = null;
		this.gestures = null;
		this.current_focused_object = null;
		this.old_focused_object = null;
		this.mode = mode;
		this.has_hololens_click = false;
		if (mode == DisplayMode.HOLOLENS) {
			this.gestures = new GestureRecognizer ();
			this.gestures.TappedEvent += (source, tapCount, ray) => {
				if (this.current_focused_object != null) {
					this.has_hololens_click = true;
				}
			};
			this.gestures.StartCapturingGestures();
		}
		this.current_input = InputType.NONE;
		this.mode_to_handler = new Dictionary<DisplayMode, Func<bool>> ();
		this.mode_to_handler [DisplayMode.COMPUTER] = this.handleComputer;
		this.mode_to_handler [DisplayMode.HOLOLENS] = this.handleHololens;
	}

	public DisplayMode getMode() {
		return this.mode;
	}

	public InputType getNext() {
		this.mode_to_handler [this.mode].Invoke ();
		return this.current_input;
	}

	private bool handleComputer() {
		this.old_focused_object = this.current_focused_object;
		if (!Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out this.input_hit.hit)) {
			this.input_hit.gameobject = null;
			this.current_focused_object = null;
			this.current_input = InputType.NONE;
			return false;
		}
		if (Input.GetMouseButtonUp (0)) {
			this.current_input = InputType.MOUSE_CLICK;
		} else {
			this.current_input = InputType.MOUSE_MOVE;
		}
		this.input_hit.position = this.input_hit.hit.point;
		this.input_hit.gameobject = this.input_hit.hit.collider.gameObject;
		this.current_focused_object = this.input_hit.hit.collider.gameObject;
		return true;
	}

	private bool handleHololens() {
		if (this.has_hololens_click) {
			this.has_hololens_click = false;
			Vector3 cam = Camera.main.transform.position;
			this.current_input = InputType.MOUSE_CLICK;
			return true;
		}
		this.old_focused_object = this.current_focused_object;
		this.head_position = Camera.main.transform.position;
		this.gaze_direction = Camera.main.transform.forward;
		if (Physics.Raycast (this.head_position, this.gaze_direction, out this.input_hit.hit)) {
			this.input_hit.position = this.input_hit.hit.point;
			this.input_hit.gameobject = this.input_hit.hit.collider.gameObject;
			this.current_focused_object = this.input_hit.hit.collider.gameObject;
			this.current_input = InputType.MOUSE_MOVE;
		} else {
			this.input_hit.gameobject = null;
			this.current_focused_object = null;
			this.current_input = InputType.NONE;
		}
		if (this.old_focused_object != this.current_focused_object)
		{
			this.gestures.CancelGestures();
			this.gestures.StartCapturingGestures();
		}
		return (this.current_input != InputType.NONE);
	}

	public InputHit getHit() {
		return this.input_hit;
	}
}
