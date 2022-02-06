using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatController : MonoBehaviour {


	// LAYER 6 = SEATS
	private static int seatLayer = 6;

	[SerializeField] private Transform PlacerHolder;
	public GameObject goHeld { get; protected set; }
	public bool isHolding { get; protected set; }
	private RigidbodyType2D holdRBType;
	private bool holdIsSimulated;

	protected static Vector3 epsilonY = new Vector3(0f, .0001f, 0f);


	protected virtual void Start() {
		gameObject.layer = seatLayer;
		isHolding = false;
	}

    public virtual bool ReceiveHold(GameObject target) {
		if(!isHolding) {
			isHolding = true;
			goHeld = target;

			goHeld.transform.parent = PlacerHolder;
			goHeld.transform.position = PlacerHolder.position;
			goHeld.transform.rotation = PlacerHolder.rotation;
			Rigidbody2D r2d = goHeld.GetComponent<Rigidbody2D>();
			holdRBType = r2d.bodyType;
			r2d.bodyType = RigidbodyType2D.Static;
			holdIsSimulated = r2d.simulated;
			r2d.simulated = false;
			
			var targetWc = target.GetComponent<WalkController>();
			if (targetWc) {
				if (GetComponent<WalkController>()) {
					targetWc.direction = GetComponent<WalkController>().direction;
				} else {
					targetWc.direction = WalkController.Dir.Down;
				}
				targetWc.isSeated = true;
			}

			return true;
		}

		return false;
	}

	// TODO set direction
	public virtual GameObject GiveHold() {
		if(isHolding) {
			Rigidbody2D r2d = goHeld.GetComponent<Rigidbody2D>();
			r2d.bodyType = holdRBType;
			r2d.simulated = holdIsSimulated;
			
			var targetWc = goHeld.GetComponent<WalkController>();
			if (targetWc) {
				targetWc.isSeated = false;
			}

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
