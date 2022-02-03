using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DiseaseTypes {
	Rhume,
	Grippe,
	Covid
}

// A struct to store disease informations
public readonly struct Infos {
	public Infos(DiseaseTypes name, float lifespan, int points, Step firstStep) {
		_type = name;
		_name = name.ToString();
		_lifespan = lifespan;
		_points = points;
		_firstStep = firstStep;
	}

	public readonly DiseaseTypes _type;
	public readonly string _name;
	public readonly float _lifespan;
	public readonly int _points;
	public readonly Step _firstStep;
}

// A struct to store informations about a treatment step
// We need "name" during "time" seconds to go to next step
// if time == 0, it's instantaneous
public readonly struct Step {
	public Step(Items name, string path, float time = 0f, (float, Step)[] next = null) {
		_name = name.ToString();
		_path = path;
		_time = time;
		_next = next;
	}

	public Step(MachineTypes name, string path, float time = 0f, (float, Step)[] next = null) {
		_name = name.ToString();
		_path = path;
		_time = time;
		_next = next;
	}

	public Step(string name, string path, float time = 0f, (float, Step)[] next = null) {
		_name = name.ToString();
		_path = path;
		_time = time;
		_next = next;
	}

	public readonly string _name;
	public readonly string _path;
	public readonly float _time;
	public readonly (float, Step)[] _next;
}

public class Disease {

	// Skins to select for each disease (depends on order or sprite sheets)
	static private Dictionary<DiseaseTypes, int> faceSkinIndex = new Dictionary<DiseaseTypes, int>() {
		{ DiseaseTypes.Rhume, 0 },
		{ DiseaseTypes.Grippe, 1 },
		{ DiseaseTypes.Covid, 2 }
	};

	private PatientController patient;

	// The current Disease, take from the array Diseases
	public Infos myInfos { get; private set; }
	public Sprite sickFace { get; private set; }
	private Step currentStep;

	public float lifetime { get; private set; }
	private bool isOver;

	public Disease(PatientController parent) {
		patient = parent;
		isOver = false;

		myInfos = patient.gc.DiseasesAvailable[Random.Range(0, patient.gc.DiseasesAvailable.Length)];

		lifetime = myInfos._lifespan;
		sickFace = Resources.Load<Sprite>("Sprites/Faces/" + myInfos._name + "Face");
		currentStep = myInfos._firstStep;
	}

	public int GetFaceSkinIndex() {
		return faceSkinIndex[myInfos._type];
	}

	// Return the sprite to display the current need
	public Sprite GetNeedSprite() {
		GameObject tmp = Resources.Load<GameObject>(currentStep._path);
		if(tmp != null) {
			SpriteRenderer sr;
			if (tmp.TryGetComponent<SpriteRenderer>(out sr))
				return sr.sprite;

			sr = tmp.GetComponentInChildren<SpriteRenderer>();
			if (sr != null)
				return sr.sprite;

			Debug.LogError("UNABLE TO GET NEED SPRITE FROM " + currentStep._path);
			return null;
		} else {
			Debug.LogError("UNABLE TO GET GAMEOBJECT AT " + currentStep._path);
			return null;
		}
	}

	// Say that we took this item
	public void TakeItem(string item) {
		if (!isOver)
			if (currentStep._name == item)
				NextStep();
	}

	// Say that we use the machine during some time
	public void UsedMachine(string machine, float time) {
		if (!isOver)
			if (currentStep._name == machine && time >= currentStep._time)
				NextStep();
	}

	// Check whether we go to next step or if we're done here 
	private void NextStep() {
		if (currentStep._next.Length > 0) {
			// Chose random next step
			float mySum = 0f, randomValue = Random.Range(0f, 1f);
			bool foundNext = false;
			for (int i = 0; i < currentStep._next.Length; i++) {
				if(currentStep._next[i].Item1 + mySum > randomValue) {
					// We want this one!
					foundNext = true;
					currentStep = currentStep._next[i].Item2;
					break;
                } else {
					mySum += currentStep._next[i].Item1;
				}
            }

			// To be sure we didn't f*cked up the proba, in doubt take first
			if (!foundNext)
				currentStep = currentStep._next[0].Item2;

			patient.DisplayNextNeed();
		} else {
			// it was the last step, patient cured!
			isOver = true;
			patient.DiseaseCured();
		}
	}

	public (string name, float time) GetCurrentStep() {
		return (currentStep._name, currentStep._time);
	}
}
