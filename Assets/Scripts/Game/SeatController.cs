using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class Holder {

	public GameObject held { get; protected set; } = null;

	private Action<GameObject> onGive;
	private Action<GameObject> onReceive;

	public Holder(Action<GameObject> onReceive, Action<GameObject> onGive) {
		this.onGive = onGive;
		this.onReceive = onReceive;
	}

	public bool IsHolding() {
		return held != null;
	}

	public bool Receive(GameObject target) {
		if(!IsHolding()) {
			held = target;
			if (held.GetComponent<WalkController>()) {
				held.GetComponent<WalkController>().holderObject = this;
			}
			onReceive(target);
			return true;
		}
		return false;
	}

	public GameObject Give() {
		if(IsHolding()) {
			GameObject target = held;
			held = null;
			if (target.GetComponent<WalkController>()) {
				target.GetComponent<WalkController>().holderObject = null;
			}
			onGive(target);
			return target;
		}
		return null;
	}

	public bool TryTansfertTo(Holder other) {
		if (IsHolding() && !other.IsHolding()) {
			other.Receive(Give());
			return true;
		}
		return false;
	}

}


public class SeatController : MonoBehaviour {

	// LAYER 6 = SEATS
	private static int seatLayer = 6;

	[SerializeField] private Transform PlacerHolder;
	[SerializeField] private bool isSeatingPerso;

	protected static Vector3 epsilonY = new Vector3(0f, .0001f, 0f);

	private FixedJoint2D joint;

	private Transform goParent;

	public Holder holder { get; protected set; }

	protected virtual void Start() {
		gameObject.layer = seatLayer;
		joint = GetComponent<FixedJoint2D>();
		holder = new Holder(OnReceive, OnGive);
	}

	protected virtual void OnReceive(GameObject target) {
		goParent = target.transform.parent;
		joint.connectedBody = target.GetComponent<Rigidbody2D>();
		joint.enabled = true; 
		joint.anchor = PlacerHolder.localPosition; 
		joint.connectedAnchor = Vector2.zero;
		target.transform.parent = PlacerHolder;

		var targetWc = target.GetComponent<WalkController>();
		if (targetWc) {
			if (GetComponent<WalkController>()) {
				targetWc.SetStoppedDirection(GetComponent<WalkController>().direction);
			} else {
				targetWc.SetStoppedDirection(WalkController.Dir.Down);
			}
			if (isSeatingPerso) {
				targetWc.isSeated = true;
			}
		}
	}

	protected virtual void OnGive(GameObject target) {
		joint.connectedBody = null;
		joint.enabled = false;
		target.transform.parent = goParent;
		goParent = null;
		
		if (isSeatingPerso) {
			var targetWc = target.GetComponent<WalkController>();
			if (targetWc) {
				targetWc.isSeated = false;
				targetWc.SetStoppedDirection(Vector2.down);
			}
			targetWc.direction = WalkController.Dir.Down;
		}
	}
}
