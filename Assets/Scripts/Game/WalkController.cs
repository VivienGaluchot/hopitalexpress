using UnityEngine;

public class WalkController : MonoBehaviour {

	public enum Dir {
		Down, Up, Left, Right
	}

	public Dir direction = Dir.Down;
	
	public bool isSeated = false;

	// direction to apply when the perso is not moving
	protected Dir stoppedDirection = Dir.Down;
	protected bool hasStoppedDirection = false;

	protected Rigidbody2D rb2D;

	protected Animator animator;

	protected virtual void Start() {
		rb2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
	}

	protected virtual void Update() {
		if (!isSeated) {
			if (rb2D.velocity.sqrMagnitude > (0.1 * 0.1)) {
				if (animator) {
					animator.SetBool("isWalking", true);
				}
				SetDirection(AngleToDirection(rb2D.velocity));
			} else {
				if (animator) {
					animator.SetBool("isWalking", false);
				}
				if (hasStoppedDirection) {
					SetDirection(stoppedDirection);
					hasStoppedDirection = false;
				}
			}
		}
	}

	public void SetStoppedDirection(Vector2 dir) {
		stoppedDirection = AngleToDirection(dir);
		hasStoppedDirection = true;
	}

	protected void SetDirection(Dir newDirection) {
		if (animator) {
			animator.SetBool("isUp", newDirection == Dir.Up);
			animator.SetBool("isRight", newDirection == Dir.Right);
			animator.SetBool("isLeft", newDirection == Dir.Left);
		}
		direction = newDirection;
	}

	protected Dir AngleToDirection(Vector2 dir) {
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
}
