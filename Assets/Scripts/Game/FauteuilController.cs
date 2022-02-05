using UnityEngine;

public class FauteuilController : WalkController {

	public GameObject placeholder;

	private WalkController holder;
	private FixedJoint2D holderJoint;

	private Transform prevParent;

	private SeatController seat;

	private new void Start() {
		base.Start();
		seat = placeholder.GetComponent<SeatController>();
	}

	// Maybe reverse the logic by setting the FixedJoint2D in the player
	public void SetHolder(GameObject target) {
		if (target) {
			if (holder  == null) {
				holder = target.GetComponent<WalkController>();
				holderJoint = target.GetComponent<FixedJoint2D>();
				holderJoint.connectedBody = rb2D;
				holderJoint.enabled = true;
			}
		} else {
			if (holder  != null) {
				holder = null;
				holderJoint.connectedBody = null;
				holderJoint.enabled = false;
				holderJoint = null;
			}
		}
	}
	private void FixedUpdate() {
		if (holder) {
			switch (holder.direction) {
				case Dir.Up:
					holderJoint.anchor = .3f * Vector3.up;
					break;
				case Dir.Down:
					holderJoint.anchor = .3f * Vector3.down;
					break;
				case Dir.Left:
					holderJoint.anchor = .6f * Vector3.left;
					break;
				case Dir.Right:
					holderJoint.anchor = .6f * Vector3.right;
					break;
			}
		}
	}

	private new void Update() {
		if (holder) {
			direction = holder.direction;
		} else {
			base.Update();
		}
		if (seat.isHolding) {
			seat.goHeld.GetComponent<WalkController>().direction = direction;
		}
	}

	public bool IsHolding() {
		return seat.isHolding;
	}
	public GameObject GetPatient() {
		return seat.goHeld;
	}

	public bool ReceivePatient(GameObject patient) {
		bool result = seat.ReceiveHold(patient);
		if (result) {
			patient.GetComponent<WalkController>().direction = GetComponent<WalkController>().direction;
		}
		return result;
	}

	public GameObject GivePatient() {
		var target = seat.GiveHold();
		if (target) {
			target.GetComponent<WalkController>().direction = WalkController.Dir.Down;
		}
		return target;
	}

}
