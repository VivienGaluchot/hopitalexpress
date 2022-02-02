using UnityEngine;

public class PersoAnimator : MonoBehaviour {

	public enum Dir {
		Down, Up, Left, Right
	}

	public Dir direction = Dir.Down;

	// direction to apply when the perso is not moving
	public Dir stoppedDirection = Dir.Down;


	private Rigidbody2D rb2D;

	private Animator animator;

	private void Start() {
		rb2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
	}

	private void FixedUpdate() {
		if (rb2D.velocity.sqrMagnitude > (0.1 * 0.1)) {
			animator.SetBool("isWalking", true);
			SetDirection(AngleToDirection(rb2D.velocity));
		} else {
			animator.SetBool("isWalking", false);
			SetDirection(stoppedDirection);
		}
	}

	public void SetStoppedDirection(Vector2 dir) {
		stoppedDirection = AngleToDirection(dir);
	}

	private Dir AngleToDirection(Vector2 dir) {
		float angle = Vector2.SignedAngle(new Vector2(-1, -1), dir);
		angle = angle - (angle + 360) % 90;
		switch (angle) {
			case 0f:
				return Dir.Down;
			case 90f:
				return Dir.Right;
			case -90f:
				return Dir.Left;
			default:
				return Dir.Up;
		}
	}

	private void SetDirection(Dir newDirection) {
		animator.SetBool("isUp", newDirection == Dir.Up);
		animator.SetBool("isRight", newDirection == Dir.Right);
		animator.SetBool("isLeft", newDirection == Dir.Left);
		direction = newDirection;
	}
}
