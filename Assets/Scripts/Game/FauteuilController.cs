using UnityEngine;

public class FauteuilController : WalkController {

	public GameObject placeholder;

	private WalkController holder;

	private Transform prevParent;

	private SeatController seat;

	private new void Start() {
		base.Start();
		seat = placeholder.GetComponent<SeatController>();
	}

	public void SetHolder(GameObject target) {
		if (target) {
			if (holder  == null) {
				holder = target.GetComponent<WalkController>();
				rb2D.simulated = false;
			}
		} else {
			if (holder  != null) {
				holder = null;
				rb2D.simulated = true;
			}
		}
	}

	private void Update() {
		if (holder) {
			switch (holder.direction) {
				case Dir.Up:
					transform.position = holder.transform.position + .6f * Vector3.up;
					break;
				case Dir.Down:
					transform.position = holder.transform.position + .2f * Vector3.down;
					break;
				case Dir.Left:
					transform.position = holder.transform.position + .6f * Vector3.left;
					break;
				case Dir.Right:
					transform.position = holder.transform.position + .6f * Vector3.right;
					break;
			}
		}
	}

	// highly dependent on WalkController
	// not really clean but work...
	private new void FixedUpdate() {
		if (holder) {
			direction = holder.direction;
		} else {
			if (rb2D.velocity.sqrMagnitude > (0.1 * 0.1)) {
				direction = AngleToDirection(rb2D.velocity);
			} else {
				if (hasStoppedDirection) {
					direction = stoppedDirection;
					hasStoppedDirection = false;
				}
			}
		}
		if (seat.isHolding) {
			seat.goHeld.GetComponent<WalkController>().direction = direction;
		}
	}

}
