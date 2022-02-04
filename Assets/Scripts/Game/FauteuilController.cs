using UnityEngine;

public class FauteuilController : WalkController {

	public GameObject placeholder;

	private SeatController seat;

	private new void Start() {
		base.Start();
		seat = placeholder.GetComponent<SeatController>();
	}


	// highly dependent on WalkController
	// not really clean but work...
	private new void FixedUpdate() {
		if (rb2D.velocity.sqrMagnitude > (0.1 * 0.1)) {
			direction = AngleToDirection(rb2D.velocity);
		} else {
			if (hasStoppedDirection) {
				direction = stoppedDirection;
				hasStoppedDirection = false;
			}
		}
		if (seat.isHolding) {
			seat.goHeld.GetComponent<WalkController>().direction = direction;
		}
	}

}
