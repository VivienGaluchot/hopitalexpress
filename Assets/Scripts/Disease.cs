using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disease {

	private PatientController patient;
	
	// A struct to store disease informations
	public readonly struct Infos {
		public Infos(string name, float lifespan, int points) {
			_name = name;
			_lifespan = lifespan;
			_points = points;
		}

		public readonly string _name;
		public readonly float _lifespan;
		public readonly int _points;
	}

	// Store informations about the different diseases possible
	private static Infos[] Diseases = new Infos[3] {
		new Infos("Rhume", 75f, 50),
		new Infos("Grippe", 90f, 100),
		new Infos("Covid", 60f, 200)
	};

	public Infos myInfos { get; private set; }
	public Sprite sickFace { get; private set; }

	// A struct to store informations about a treatment step
	// We need "name" during "time" seconds to go to next step
	// if time == 0, it's instantaneous
	public readonly struct Step {
		public Step(string name, float time) {
			_name= name;
			_time = time;
		}
		public readonly string _name;
		public readonly float _time;
	}

	// Store informations about treatment concerning the existing diseases
	// Maybe add some random in treaments
	private static Dictionary<string, Step[]> DiseaseSteps = new Dictionary<string, Step[]> {
		{ "Rhume", new Step[1] { new Step("BluePill", 0) } },
		{ "Grippe", new Step[2] { new Step("GreenPill", 0), new Step("BluePill", 0) } },
		{ "Covid", new Step[3] { new Step("PCR", 0), new Step("Scanner", 10), new Step("GreenPill", 0) } }
	};

	private Step[] Steps;
	private int currentStep;

	public float lifetime { get; private set; }
	private bool isOver;

	public Disease(PatientController parent) {
		patient = parent;
		isOver = false;

		myInfos = Diseases[Random.Range(0, Diseases.Length)];
		lifetime = myInfos._lifespan;
		sickFace = Resources.Load<Sprite>("Sprites/Faces/" + myInfos._name + "Face");
		DiseaseSteps.TryGetValue(myInfos._name, out Steps);
		currentStep = 0;

		if (Steps.Length == 0) {
			isOver = true;
			patient.DiseaseCured();
		}
	}

	// Return the sprite to display the current need
	public Sprite GetNeedSprite() {
		string path = "Sprites/Medoc/" + Steps[currentStep]._name;
		Sprite needSprite = Resources.Load<Sprite>(path);

		if(needSprite == null) {
			path = "Sprites/Machines/" + Steps[currentStep]._name;
			needSprite = Resources.Load<Sprite>(path);
		}

		return needSprite;
	}

	// Say that we took this item
	public void TakeItem(string item) {
		if (!isOver)
            if (Steps[currentStep]._name == item)
				NextStep();
	}

	// Say that we use the machine during some time
	public void UsedMachine(string machine, float time) {
		if (!isOver)
			if (Steps[currentStep]._name == machine && time >= Steps[currentStep]._time)
				NextStep();
    }

	// Check whether we go to next step or if we're done here 
	private void NextStep() {
		currentStep++;
		if (currentStep == Steps.Length) {
			// it was the last step, patient cured!
			isOver = true;
			patient.DiseaseCured();
		} else {
			patient.DisplayNextNeed();
		}
	}

	public (string name, float time) GetCurrentStep() {
		return (Steps[currentStep]._name, Steps[currentStep]._time);
    }
}
