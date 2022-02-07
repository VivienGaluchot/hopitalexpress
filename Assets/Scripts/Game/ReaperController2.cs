using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReaperController2 : MonoBehaviour {

	private Rigidbody2D rb2D;
	private PatientController target;

	private float minFound;

	private void Awake() {
		rb2D = GetComponent<Rigidbody2D>();
	}

    private void Update() {
		if(target) {
			if ((transform.position - target.transform.position).sqrMagnitude < .1f) {
				target.timeEffect = 10f;
			} else {
				target.timeEffect = 1f;
			}
		}
    }

    private void FixedUpdate() {
		if(target)
			rb2D.velocity = (target.transform.position - transform.position) * .25f * (GameController.instance.isPaused ? .1f : 1f);
		CheckForDirection();
	}

	private void CheckForDirection() {
		foreach (PatientController pc in GameController.instance.patientsList) {
			if (pc != null && pc.state == PatientController.States.sick && 
				(!target || target.state != PatientController.States.sick || pc.lifetime < target.lifetime)) {
				if (target) target.timeEffect = 1f;
				target = pc;
			}
		}
	}
}
