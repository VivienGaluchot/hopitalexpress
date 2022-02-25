using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatController : MonoBehaviour {

	// LAYER 6 = SEATS
	private static int seatLayer = 6;

	[SerializeField] private Transform PlacerHolder;
	[SerializeField] private bool isSeatingPerso;

	public GameObject goHeld { get; protected set; }
	public bool isHolding { get; protected set; }

	protected static Vector3 epsilonY = new Vector3(0f, .0001f, 0f);

	private FixedJoint2D joint;

	private Transform goParent;


	protected virtual void Start() {
		gameObject.layer = seatLayer;
		isHolding = false;
		joint = GetComponent<FixedJoint2D>();
	}

    public virtual bool ReceiveHold(GameObject target) {
		if(!isHolding) {
			isHolding = true;
			goHeld = target;

			goParent = goHeld.transform.parent;
			joint.connectedBody = goHeld.GetComponent<Rigidbody2D>();
			joint.enabled = true; 
			joint.anchor = PlacerHolder.localPosition; 
			joint.connectedAnchor = Vector2.zero;
			goHeld.transform.parent = PlacerHolder;
			
			var targetWc = target.GetComponent<WalkController>();
			if (targetWc) {
				if (GetComponent<WalkController>()) {
					targetWc.direction = GetComponent<WalkController>().direction;
				} else {
					targetWc.direction = WalkController.Dir.Down;
				}
				if (isSeatingPerso) {
					targetWc.isSeated = true;
				}
			}

			return true;
		}

		return false;
	}

	public virtual GameObject GiveHold() {
		if(isHolding) {
			joint.connectedBody = null;
			joint.enabled = false;
			
			if (isSeatingPerso) {
				var targetWc = goHeld.GetComponent<WalkController>();
				if (targetWc) {
					targetWc.isSeated = false;
					targetWc.SetStoppedDirection(Vector2.down);
				}
				targetWc.direction = WalkController.Dir.Down;
			}
			goHeld.transform.parent = goParent;

			GameObject returnGO = goHeld;
			goHeld = null;
			isHolding = false;

			return returnGO;
		}

		return null;
	}

	public bool TryTansfertTo(SeatController other) {
		if (isHolding && !other.isHolding) {
			other.ReceiveHold(GiveHold());
			return true;
		}
		return false;
	}
}
