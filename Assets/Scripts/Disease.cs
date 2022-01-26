using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disease : MonoBehaviour {

	private PatientController patient;

	public string _name;
	public Sprite sickFace;
	public float lifespan;

	public string[] Steps;
	private int currentStep;

	public float lifetime { get; private set; }
	private bool isOver;

	public void Initialize(PatientController parent) {
		_name = gameObject.name;
		patient = parent;
		isOver = false;
		lifetime = lifespan;
		currentStep = 0;

		if (Steps.Length == 0) {
			isOver = true;
			patient.DiseaseCured();
		}
	}

	void Update() {
		if(!isOver) {
			lifetime -= Time.deltaTime;
			if (lifetime < 0f) {
				patient.DiseaseLifetimeElapsed();
				isOver = true;
			}
		}
	}

	public void TakeItem(string item) {
		if (!isOver) {
			if(Steps[currentStep] == item) {
				currentStep++;
				if (currentStep == Steps.Length) {
					// it was the last step, patient cured!
					isOver = true;
					patient.DiseaseCured();
				}
			}
        }
	}
}
