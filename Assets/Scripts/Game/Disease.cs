using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DiseaseTypes {
	Rhume,
	Grippe,
	Covid
}

public class Disease {

	// Skins to select for each disease (depends on order or sprite sheets)
	static private Dictionary<DiseaseTypes, int> faceSkinIndex = new Dictionary<DiseaseTypes, int>() {
		{ DiseaseTypes.Rhume, 0 },
		{ DiseaseTypes.Grippe, 2 },
		{ DiseaseTypes.Covid, 4 }
	};

	private PatientController patient;
	
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
		public Step(Items name, string icon, float time = 0f, (float, Step)[] next = null) {
			_name = name.ToString();
			_path = "Illustrations/Pills/" + icon;
			_time = time;
			_next = next;
		}

		public Step(MachineTypes name, string icon, float time = 0f, (float, Step)[] next = null) {
			_name = name.ToString();
			_path = "Illustrations/Objets/" + icon;
			_time = time;
			_next = next;
		}

		public readonly string _name;
		public readonly string _path;
		public readonly float _time;
		public readonly (float, Step)[] _next;
	}

	// Store informations about the different diseases possible
	private static Infos[] Diseases = new Infos[3] {
		new Infos(DiseaseTypes.Rhume, 75f, 50, 
			new Step(MachineTypes.Diagnostable, "Diagnostable", 0f, 
				new (float, Step)[1] {(1f, new Step(Items.PiluleBleue, "bleu"))}
			)
		),
		new Infos(DiseaseTypes.Grippe, 90f, 100, 
			new Step(MachineTypes.Diagnostable, "Diagnostable", 0f, 
				new (float, Step)[1] {(1f, new Step(Items.PiluleVerte, "vert", 0f,
					new (float, Step)[2] {(.75f, new Step(Items.PiluleVerte, "vert")),
										(.25f, new Step(Items.PiluleBleue, "bleu"))
					})
				)}
			)
		),
		new Infos(DiseaseTypes.Covid, 60f, 200,
			new Step(MachineTypes.Diagnostable, "Diagnostable", 0f,
				// TODO PCR
				new (float, Step)[1] {(1f, new Step(Items.PCR, "bleu", 0f,
					new (float, Step)[1] {(1f, new Step(MachineTypes.Scanner, "scanner", 10f, 
						new (float, Step) [2] {(.5f, new Step(Items.SeringueRouge, "Fiolerouge")),
							(.5f, new Step(Items.SeringueJaune, "Fiolejaune"))
						})
					)}
				))}
			)
		),
	};

	// The current Disease, take from the array Diseases
	public Infos myInfos { get; private set; }
	public Sprite sickFace { get; private set; }
	private Step currentStep;

	public float lifetime { get; private set; }
	private bool isOver;

	public Disease(PatientController parent) {
		patient = parent;
		isOver = false;

		myInfos = Diseases[Random.Range(0, Diseases.Length)];
		lifetime = myInfos._lifespan;
		sickFace = Resources.Load<Sprite>("Sprites/Faces/" + myInfos._name + "Face");
		currentStep = myInfos._firstStep;
	}

	public int GetFaceSkinIndex() {
		return faceSkinIndex[myInfos._type];
	}

	// Return the sprite to display the current need
	public Sprite GetNeedSprite() {
		return Resources.Load<Sprite>(currentStep._path);
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
		if (currentStep._next != null) {
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
