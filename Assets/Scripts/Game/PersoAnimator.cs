using UnityEngine;

public class PersoAnimator : MonoBehaviour {

    public enum Dir {
        Down, Up, Left, Right
    }

    public Dir direction = Dir.Down;


	private Rigidbody2D rb2D;

	private Animator animator;

	private void Start() {
		rb2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
    }

	private void FixedUpdate() {
		float angle = Vector2.SignedAngle(new Vector2(-1, -1), this.rb2D.velocity);
		angle = angle - (angle + 360) % 90;

		animator.SetBool("isWalking", this.rb2D.velocity.sqrMagnitude > (0.1 * 0.1));

		if (this.rb2D.velocity.sqrMagnitude > (0.1 * 0.1)) {
			switch (angle) {
				case 0f:
					animator.SetBool("isUp", false);
					animator.SetBool("isRight", false);
					animator.SetBool("isLeft", false);
					break;
				case 90f:
					animator.SetBool("isUp", false);
					animator.SetBool("isRight", true);
					animator.SetBool("isLeft", false);
					break;
				case -90f:
					animator.SetBool("isUp", false);
					animator.SetBool("isRight", false);
					animator.SetBool("isLeft", true);
					break;
				default:
					animator.SetBool("isUp", true);
					animator.SetBool("isRight", false);
					animator.SetBool("isLeft", false);
					break;
			}
		}
	}
}
