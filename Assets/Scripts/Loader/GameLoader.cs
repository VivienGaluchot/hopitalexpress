using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GameLoader : MonoBehaviour {

	[SerializeField] private Dropdown dd;
	[SerializeField] private string path;

	[SerializeField] private Transform FloorParent;
	[SerializeField] private Transform WallsParent;
	[SerializeField] private GameObject[] Walls;
	public bool useTest;
	public GameObject[] WallsTest;
	[SerializeField] private GameObject[] Floor;

	public bool instantLoad, loadLevel, loadSpawns, loadDiseases;

	private Transform[] PrefabsParents;
	private GameObject[][] Prefabs;

	private GameController gc;

	private void Start() {
		gc = GetComponent<GameController>();
		PrefabsParents = new Transform[2] { FloorParent, WallsParent };
		Prefabs = new GameObject[2][] { Floor, useTest ? WallsTest : Walls };
		path = Path.Combine(Application.dataPath, path);
		FetchDDOptions();

		if(instantLoad) {
			LoadLevel();
		}
	}

	private void FetchDDOptions() {
		string[] paths = System.IO.Directory.GetFiles(path);
		List<string> pathsList = new List<string>();
		foreach (string s in paths) {
			if (!s.EndsWith(".meta"))
				pathsList.Add(Path.GetFileNameWithoutExtension(s));
		}
		dd.ClearOptions();
		dd.AddOptions(pathsList);
	}

	public void LoadLevel() {

		dd.transform.parent.parent.gameObject.SetActive(false);

		string filename = dd.options[dd.value].text + ".json";
		LevelData Data = JsonUtility.FromJson<LevelData>(ReadFromFile(Path.Combine(path, filename)));
		
		if(loadLevel) {
			for (int i = 0; i < Data.layers.Count; i++) {
				foreach (CellData cell in Data.layers[i].cells) {
					GameObject newGO = Instantiate(Prefabs[i][cell.value - 1], PrefabsParents[i]);
					newGO.GetComponent<SpriteRenderer>().sortingOrder = i - 2;
					newGO.transform.position = new Vector3(cell.y / Data.size, -cell.x / Data.size, 0f);
				}
			}
			Camera.main.transform.position = new Vector3((Data.columns - 1) / 2f / Data.size, (1 - Data.rows) / 2f / Data.size, -10f);
		}

		if(loadDiseases) {
			// LOAD DISEASES
			gc.DiseasesAvailable = new Infos[Data.diseases.Count];
			for (int i = 0; i < Data.diseases.Count; i++) {
				DiseaseData diseaseData = JsonUtility.FromJson<DiseaseData>(ReadFromFile(Path.Combine(Application.dataPath, "MyEditor/Data/Disease/" + Data.diseases[i] + ".json")));

				StepContainer container = ReadNextStep(diseaseData.treatment);

                StepData starter = null;
                foreach (StepData step in container.allSteps) {
                    if (step.first) {
                        starter = step;
                        break;
                    }
                }

                Step firstStep = ComputeStep(starter);
				DiseaseTypes dt = (DiseaseTypes)diseaseData.faceID;
				gc.DiseasesAvailable[i] = new Infos(dt, diseaseData.lifespan, (int)diseaseData.points, firstStep);
			}
		}
		
		if(loadSpawns) {
			GetComponent<GameController>().StartGame(Data.playerSpawn, Data.patientSpawn, Data.patientSpawnDirection);
		} else {
			GetComponent<GameController>().StartGame();
		}
	}

	private Step ComputeStep(StepData stepData) {
		(float, Step)[] next = new (float, Step)[stepData.NextsList.Count];
		for (int i = 0; i < stepData.NextsList.Count; i++) {
			next[i] = (stepData.NextsList[i].proba, ComputeStep(stepData.NextsList[i].nextStep));
		}

		return new Step(stepData.name, stepData.path, stepData.time, next);
	}

	private StepContainer ReadNextStep(string s) {
		return JsonUtility.FromJson<StepContainer>(ReadFromFile(Path.Combine(Application.dataPath, "MyEditor/Data/Treatment/" + s + ".json")));
	}

	private string ReadFromFile(string path) {
		StreamReader sr = new StreamReader(path);
		return sr.ReadToEnd();
	}
}
