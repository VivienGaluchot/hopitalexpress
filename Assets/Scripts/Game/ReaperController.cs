using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReaperController : MonoBehaviour
{

	private Rigidbody2D r2d;

	private float decisionPeriod = 2;

	private float counter = 0;

	private Vector2 currentDecision;

	void Start() {
		r2d = GetComponent<Rigidbody2D>();
		currentDecision = new Vector2(0, 0);
	}

	void FixedUpdate() {
		r2d.velocity = currentDecision;
	}

	void Update() {
		counter += Time.deltaTime;
		if (counter > decisionPeriod) {
			NewDecision();
			counter = 0;
		}
	}

	private void NewDecision() {
		int random = Random.Range(0, 10);
		switch (random) {
			case 0:
				currentDecision.x = 1;
				break;
			case 1:
				currentDecision.x = -1;
				break;
			case 2:
				currentDecision.y = 1;
				break;
			case 3:
				currentDecision.y = -1;
				break;
			case 4:
				currentDecision.Set(0, 0);
				break;
	   }
	   currentDecision = Vector2.ClampMagnitude(currentDecision, 1);
	}
}
