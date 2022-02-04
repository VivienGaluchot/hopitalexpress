using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Globalization;

[Serializable]
public class StepData {
	public StepData(bool first, string name, string path, float time, List<NextStepData> NextsList) { this.first = first;  this.name = name; this.path = path; this.time = time; this.NextsList = NextsList; }

	public bool first;
	public string name;
	public string path;
	public float time;
	[NonSerialized] public List<NextStepData> NextsList;
}

public class NextStepData {
	public NextStepData(float proba, StepData nextStep) { this.proba = proba; this.nextStep = nextStep; }
	public float proba;
	public StepData nextStep;
}

[Serializable]
public class StepContainer : ISerializationCallbackReceiver {
	public List<StepData> allSteps;
	public List<serializationInfo> serializationInfos;

	public void OnBeforeSerialize() {
		serializationInfos = new List<serializationInfo>();
		foreach(StepData step in allSteps) {
			serializationInfo info = new serializationInfo();
			info.parentIndex = allSteps.FindIndex(e => e == step);
			info.childs = new List<Child>();
			foreach (NextStepData nextStep in step.NextsList) {
				Child child = new Child { proba = nextStep.proba, index = allSteps.FindIndex(e => e == nextStep.nextStep) };
				info.childs.Add(child);
			}
			serializationInfos.Add(info);
		}
	}

	public void OnAfterDeserialize() {
		foreach(serializationInfo info in serializationInfos) {
			allSteps[info.parentIndex].NextsList = new List<NextStepData>();
			foreach(Child child in info.childs) {
				allSteps[info.parentIndex].NextsList.Add(new NextStepData(child.proba, allSteps[child.index]));
			}
		}
	}

	[Serializable]
	public struct serializationInfo {
		public int parentIndex;
		public List<Child> childs;
	}

	[Serializable]
	public struct Child {
		public float proba;
		public int index;
	}
}

public class TreatmentDataController : MonoBehaviour {

	[SerializeField] private Dropdown FilesDropdown;
	[SerializeField] private InputField FileNameInputField;
	[SerializeField] private string path;

	private bool clickedDelete;

	private void Start() {
		path = Path.Combine(Application.dataPath, path);
		FetchDDOptions();
	}

	public void DeleteSave(Text text) {
		if (!clickedDelete) {
			clickedDelete = true;
			text.text = "Sure?";
		} else {
			text.text = "Delete";
			clickedDelete = false;
			string filePath = Path.Combine(path, FilesDropdown.options[FilesDropdown.value].text + ".json");
			if (File.Exists(filePath))
				File.Delete(filePath);
			FetchDDOptions();
		}
	}

	private void FetchDDOptions() {
		string[] paths = System.IO.Directory.GetFiles(path);
		List<string> pathsList = new List<string>();
		foreach (string s in paths) {
			if (!s.EndsWith(".meta"))
				pathsList.Add(Path.GetFileNameWithoutExtension(s));
		}
		FilesDropdown.ClearOptions();
		FilesDropdown.AddOptions(pathsList);
	}

	public void SaveData() {
		TreatmentItemController starter = null;
		foreach (TreatmentItemController item in TreatmentEditorController.instance.TreatmentItems) {
			if (item.endingLines.Count == 0) {
				starter = item;
				break;
			}
		}

		if (starter)
			WriteToFile(JsonUtility.ToJson(TreatmentItemToStepDataContainer(starter)));
		else
			Debug.Log("NO STARTER FOUND -- Un objet ne doit pas avoir de parent pour être le point de départ du traitement");

		FetchDDOptions();
	}

	private StepContainer TreatmentItemToStepDataContainer(TreatmentItemController starter) {

		StepContainer container = new StepContainer();
		container.allSteps = new List<StepData>();
		TICTOStepData(starter, container, true);

		return container;
	}

	private StepData TICTOStepData(TreatmentItemController tic, StepContainer container, bool first = false) {
		string name = Path.GetFileNameWithoutExtension(tic.path);
		string path = tic.path;
		float time = tic.TimeDisplayedValue();
		List<NextStepData> nextList = new List<NextStepData>();

		float[] proba = new float[tic.startingLines.Count];
		float probaSum = 0;
		for(int i = 0; i < tic.startingLines.Count; i++) {
			proba[i] = GetFloat(tic.startingLines[i].inputField.text);
			probaSum += proba[i];
		}
		for(int i = 0; i < tic.startingLines.Count; i++) {
			nextList.Add(new NextStepData(proba[i] / probaSum, TICTOStepData(tic.startingLines[i].ender, container)));
		}

		StepData stepData = new StepData(first, name, path, time, nextList);
		container.allSteps.Add(stepData);

		return stepData;
	}

	private float GetFloat(string value) {
		string proba = value != "" ? value : "1";
		proba = proba.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
		proba = proba.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
		return float.Parse(proba);
	}

	private void WriteToFile(string content) {
		StreamWriter sw = new StreamWriter(Path.Combine(path, FileNameInputField.text + ".json"));
		sw.WriteLine(content);
		sw.Close();
	}

	private List<List<GameObject>> nodes;
	private List<LineController> lines;

	public void LoadData() {
		TreatmentEditorController.instance.ClearScreen();
		string filename = FilesDropdown.options[FilesDropdown.value].text + ".json";
		StepContainer Data = JsonUtility.FromJson<StepContainer>(ReadFromFile(filename));
		FileNameInputField.text = Path.GetFileNameWithoutExtension(filename);
		nodes = new List<List<GameObject>>();
		lines = new List<LineController>();

		StepData starter = null;
		foreach (StepData step in Data.allSteps) {
			if(step.first) {
				starter = step;
				break;
            }
		}

		CreateGameObjectsFromStepData(starter);
		OrganizeTree();
	}


	private string ReadFromFile(string fileName) {
		StreamReader sr = new StreamReader(Path.Combine(path, fileName));
		return sr.ReadToEnd();
	}

	public GameObject CreateGameObjectsFromStepData(StepData data, int layer = 0) {
		if (nodes.Count < layer + 1)
			nodes.Add(new List<GameObject>());

		GameObject newItem = Instantiate(TreatmentEditorController.instance.TreatmentItemPrefab, TreatmentEditorController.instance.transform);
		TreatmentItemController tic = newItem.GetComponent<TreatmentItemController>();
		tic.path = data.path;
		newItem.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Resources.Load<GameObject>(data.path).GetComponent<SpriteRenderer>().sprite;
		TreatmentEditorController.instance.TreatmentItems.Add(tic);
		tic.valueField.text = data.time.ToString();

		foreach(NextStepData nextStep in data.NextsList) {
			GameObject nextItem = CreateGameObjectsFromStepData(nextStep.nextStep, layer + 1);
			GameObject myLine = Instantiate(TreatmentEditorController.instance.LinePrefab, TreatmentEditorController.instance.transform);
			LineController lc = myLine.GetComponent<LineController>();
			lc.Init();
			lines.Add(lc);
			lc.inputField.text = nextStep.proba.ToString();

			tic.TryAddNext(lc, nextItem.GetComponent<TreatmentItemController>());
		}

		nodes[layer].Add(newItem);

		return newItem;
	}

	private void OrganizeTree() {
		for (int i = 0; i < nodes.Count; i++) {
			for (int j = 0; j < nodes[i].Count; j++) {
                nodes[i][j].transform.position = new Vector3(-nodes[i].Count + 2*j, 4 - 2 * i, 0f);
            }
		}

		foreach(LineController lc in lines)
            lc.UpdateMeshAndPosition();
	}
}
