using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disease {

	private PatientController patient;

	public readonly struct Infos {
		public Infos(string name, float lifespan) {
			_name = name;
			_lifespan = lifespan;
		}

		public readonly string _name;
		public readonly float _lifespan;
	}

	private static Infos[] Diseases = new Infos[3] {
		new Infos("Rhume", 60),
		new Infos("Grippe", 90),
		new Infos("Covid", 45)
	};

	public Infos myInfos { get; private set; }
	public Sprite sickFace { get; private set; }

	public readonly struct Step {
		public Step(string name, float time) {
			_name= name;
			_time = time;
		}
		public readonly string _name;
		public readonly float _time;
	}

	private static Dictionary<string, Step[]> DiseaseSteps = new Dictionary<string, Step[]> {
		{ "Rhume", new Step[1] { new Step("BluePill", 0) } },
		{ "Grippe", new Step[2] { new Step("BluePill", 0), new Step("GreenPill", 0) } },
		{ "Covid", new Step[3] { new Step("PCR", 0), new Step("Scanner", 10), new Step("BluePill", 0) } }
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

	//void Update() {
	//	if(!isOver) {
	//		lifetime -= Time.deltaTime;
	//		if (lifetime < 0f) {
	//			patient.DiseaseLifetimeElapsed();
	//			isOver = true;
	//		}
	//	}
	//}

	public Sprite GetNeedSprite() {
		string path = "Sprites/Medoc/" + Steps[currentStep]._name;
		Sprite needSprite = Resources.Load<Sprite>(path);

		return needSprite;
	}

	public void TakeItem(string item) {
		if (!isOver) {
            if (Steps[currentStep]._name == item) {
                currentStep++;
                if (currentStep == Steps.Length) {
                    // it was the last step, patient cured!
                    isOver = true;
                    patient.DiseaseCured();
                } else {
                    patient.DisplayNextNeed();
                }
            }
        }
	}
}
