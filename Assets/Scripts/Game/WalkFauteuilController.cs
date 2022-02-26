using UnityEngine;

public class WalkFauteuilController : WalkController {

	public float releaseImpulse = 1;

	private WalkController holder;
	private FixedJoint2D holderJoint;

	private Transform prevParent;

	public SeatController seat { get; protected set; }

	protected override void Start() {
		base.Start();
		seat = GetComponent<SeatController>();
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
			if (holder != null) {
				holder = null;
				holderJoint.connectedBody = null;
				holderJoint.enabled = false;
				holderJoint = null;
				rb2D.velocity += DirToVect(direction) * releaseImpulse;
			}
		}
	}

	private void FixedUpdate() {
		if (holder) {
			holderJoint.anchor = new Vector2(.6f, .3f) * DirToVect(holder.direction);
		}
	}

	protected override void Update() {
		if (holder) {
			direction = holder.direction;
		} else {
			base.Update();
		}
		if (seat.holder.IsHolding()) {
			seat.holder.held.GetComponent<WalkController>().direction = direction;
		}
	}

}
