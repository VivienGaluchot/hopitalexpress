using UnityEngine;

public class PersoAnimator : MonoBehaviour {

	public float diseaseAnimationPeriod;


    public enum Dir {
        Down, Up, Left, Right
    }

    public Dir direction = Dir.Down;


	private Rigidbody2D rb2D;

	private Animator animator;

	private float periodWithoutDiseaseAnimation;

	private void Start() {
		rb2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		periodWithoutDiseaseAnimation = 0;
    }

	private void FixedUpdate() {
		float angle = Vector2.SignedAngle(new Vector2(-1, -1), this.rb2D.velocity);
		angle = angle - (angle + 360) % 90;

		animator.SetBool("isWalking", this.rb2D.velocity.sqrMagnitude > (0.1 * 0.1));

		if (this.rb2D.velocity.sqrMagnitude > (0.1 * 0.1)) {
			switch (angle) {
				case 0f:
					direction = Dir.Down;
					break;
				case 90f:
					direction = Dir.Right;
					break;
				case -90f:
					direction = Dir.Left;
					break;
				default:
					direction = Dir.Up;
					break;
			}
		}

		if (diseaseAnimationPeriod > 0) {
			periodWithoutDiseaseAnimation += Time.fixedDeltaTime;
			if (periodWithoutDiseaseAnimation > diseaseAnimationPeriod) {
				periodWithoutDiseaseAnimation = 0;
				animator.SetTrigger("diseasing");
			}
		}
	}
}
