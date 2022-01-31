using UnityEngine;

public class WalkAnimator : MonoBehaviour {

	private Rigidbody2D rb2D;


	private Animator animator;
	private float animatorSpeed;

	private enum WalkDirections {
		down, 
		left, 
		up, 
		right, 
		idle
    }

	private WalkDirections direction = WalkDirections.idle;


	private void Start() {
		rb2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		animatorSpeed = animator.speed;
    }

	private void FixedUpdate() {
		float angle = Vector2.SignedAngle(new Vector2(-1, -1), this.rb2D.velocity);
		angle = angle - (angle + 360) % 90;
		if (this.rb2D.velocity.sqrMagnitude > (0.1 * 0.1)) {
			// I don't like it to be hardcoded, but we'll see some day.........
			// ................................. :<
			switch (angle) {
				case 0f:
					if (direction != WalkDirections.down) {
						animator.SetTrigger("walkDown");

						// 1
						animator.speed = animatorSpeed;
						direction = WalkDirections.down;
					}
					break;
				case 90f:
					if (direction != WalkDirections.right) {
						animator.SetTrigger("walkRight");

						// 2
						if(direction == WalkDirections.idle)
							animator.speed = animatorSpeed;
						direction = WalkDirections.right;
					}
					break;
				case -90f:
					if (direction != WalkDirections.left) {
						animator.SetTrigger("walkLeft");
						animator.speed = animatorSpeed;
						direction = WalkDirections.left;
					}
					break;
				default:
					if (direction != WalkDirections.up) {
						animator.SetTrigger("walkUp");
						animator.speed = animatorSpeed;
						direction = WalkDirections.up;
					}
					break;
			}
		} else if (direction != WalkDirections.idle) {
			animatorSpeed = animator.speed;
			animator.speed = 0f;
			direction = WalkDirections.idle;
		}
	}
}
