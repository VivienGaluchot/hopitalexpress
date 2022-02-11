using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.IO;

public class GameLoader : MonoBehaviour {

	[SerializeField] private Dropdown FileNamesDropdown;
	[SerializeField] private string path;

	[SerializeField] private Transform FloorParent;
	[SerializeField] private Transform WallsParent;
	[SerializeField] private Transform LevelObjectsParent;
	[SerializeField] private GameObject[] FloorPrefabs;
	[SerializeField] private GameObject[] WallPrefabs;
	[SerializeField] private Sprite[] WallSprites;

	public bool instantLoad, loadLevel, loadSpawns, loadDiseases;

	private GameController gc;

	private void Start() {
		gc = GetComponent<GameController>();
		path = Path.Combine(Application.dataPath, path);
		FetchFilesNames();

		if(instantLoad)
			LoadLevel(true);
	}

	private void FetchFilesNames() {
		string[] paths = Directory.GetFiles(path);
		List<string> pathsList = new List<string>();
		foreach (string s in paths) {
			if (!s.EndsWith(".meta"))
				pathsList.Add(Path.GetFileNameWithoutExtension(s));
		}
		FileNamesDropdown.ClearOptions();
		FileNamesDropdown.AddOptions(pathsList);
	}

	public void LoadLevel(bool random = false) {
		FileNamesDropdown.transform.parent.parent.gameObject.SetActive(false);

		string filename = FileNamesDropdown.options[FileNamesDropdown.value].text + ".json";
		if (random) {
			filename = FileNamesDropdown.options[Random.Range(0, FileNamesDropdown.options.Count)].text + ".json";
			Debug.Log("load : " + filename);
		}
		LevelData Data = JsonUtility.FromJson<LevelData>(ReadFromFile(Path.Combine(path, filename)));

		List<GameObject> WelcomeSeats = new List<GameObject>();

		if (loadLevel) {
			foreach (CellData cell in Data.floorCells) {
				GameObject newGO = Instantiate(FloorPrefabs[cell.value - 1], FloorParent);
				newGO.GetComponent<SpriteRenderer>().sortingOrder = -2;
				newGO.transform.position = new Vector3(cell.y / LevelEditorController.size, -cell.x / LevelEditorController.size, 0f);
			}
			foreach (CellData cell in Data.wallCells) {
				GameObject newGO = Instantiate(WallPrefabs[(cell.value - 1)%16], WallsParent);
				newGO.GetComponent<SpriteRenderer>().sprite = WallSprites[cell.value - 1];
				newGO.transform.position = new Vector3(cell.y / LevelEditorController.size, -cell.x / LevelEditorController.size, 0f);
			}

			foreach (LevelObject lo in Data.loContainer.LevelObjects) {
				GameObject newGO = Instantiate(Resources.Load<GameObject>(lo.path), lo.pos, Quaternion.identity, LevelObjectsParent);
				if (newGO.GetComponent<SeatController>() && lo.isWelcomeSeat) WelcomeSeats.Add(newGO);

				// Load childs
				if(lo.childs != null && lo.childs.Count > 0) {
					newGO.AddComponent<SortingGroup>();
					newGO.transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder = -10000;
					foreach (LevelObject child in lo.childs) {
						GameObject newChild = Instantiate(Resources.Load<GameObject>(child.path), child.pos, Quaternion.identity, newGO.transform);
						if (newChild.GetComponent<SeatController>() && child.isWelcomeSeat) WelcomeSeats.Add(newChild);
						newChild.transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder = Mathf.RoundToInt(newChild.transform.position.y * -100);
					}
				}
			}

			Camera.main.transform.position = new Vector3(Data.camX, Data.camY, -10f);
			Camera.main.orthographicSize = Data.camSize;
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
		
		if(loadSpawns)
			GetComponent<GameController>().StartGame(Data.playerSpawn, Data.patientSpawn, Data.patientSpawnDirection, Data.patientQueueSize, Data.levelTime, WelcomeSeats);
		else
			GetComponent<GameController>().StartGame();
	}

	private Step ComputeStep(StepData stepData) {
		(float, Step)[] next = new (float, Step)[stepData.NextsList.Count];
		for (int i = 0; i < stepData.NextsList.Count; i++) {
			next[i] = (stepData.NextsList[i].proba, ComputeStep(stepData.NextsList[i].nextStep));
		}

		return new Step(stepData.name, Resources.Load<GameObject>(stepData.path), stepData.time, next);
	}

	private StepContainer ReadNextStep(string s) {
		return JsonUtility.FromJson<StepContainer>(ReadFromFile(Path.Combine(Application.dataPath, "MyEditor/Data/Treatment/" + s + ".json")));
	}

	private string ReadFromFile(string path) {
		StreamReader sr = new StreamReader(path);
		return sr.ReadToEnd();
	}
}
