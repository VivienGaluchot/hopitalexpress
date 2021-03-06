using System.Collections.Generic;
using UnityEngine;
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

public class TreatmentDataController : DataController {

	private TreatmentEditorController tec;

	void Start() { tec = TreatmentEditorController.instance; }

	public override void SaveData() {
		TreatmentItemController starter = null;
		foreach (TreatmentItemController item in tec.TreatmentItems) {
			if (item.endingLines.Count == 0) {
				starter = item;
				break;
			}
		}

		if (starter)
			WriteToFile(JsonUtility.ToJson(TreatmentItemToStepDataContainer(starter)));
		else
			Debug.Log("NO STARTER FOUND -- Un objet ne doit pas avoir de parent pour ?tre le point de d?part du traitement");

		FetchFilesNamesToLoad();
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

	private List<List<GameObject>> nodes;
	private List<LineController> lines;

	public override void LoadData() {
		if (FilesDropdown.options.Count == 0) {
			Debug.LogWarning("Aucun fichier ? charger");
			return;
		}

		tec.ClearScreen();
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
		OrganizeTree(starter);
	}

	public GameObject CreateGameObjectsFromStepData(StepData data, int layer = 0) {
		if (nodes.Count < layer + 1)
			nodes.Add(new List<GameObject>());

		GameObject newGo = Instantiate(tec.TreatmentItemPrefab, transform);
		TreatmentItemController tic = newGo.GetComponent<TreatmentItemController>();
		tic.path = data.path;
		tec.TreatmentItems.Add(tic);
		tic.valueField.text = data.time.ToString();

		Transform SpriteContainer = newGo.transform.Find("Sprite");
		SpriteRenderer sr = SpriteContainer.GetComponent<SpriteRenderer>();

		GameObject loaded = Resources.Load<GameObject>(data.path);

		sr.sprite = loaded.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;
		sr.drawMode = SpriteDrawMode.Sliced;
		if (sr.size.x > sr.size.y) sr.size = new Vector2(.6f, .6f * sr.size.y / sr.size.x);
		else sr.size = new Vector2(.6f * sr.size.x / sr.size.y, .6f);

		// Adapt position depending on pivot
		Vector2 pivot = new Vector2(sr.sprite.pivot.x / sr.sprite.rect.width, sr.sprite.pivot.y / sr.sprite.rect.height);
		Vector2 offset = pivot - new Vector2(.5f, .5f);
		SpriteContainer.transform.localPosition += (Vector3)offset;

		foreach(NextStepData nextStep in data.NextsList) {
			GameObject nextItem = CreateGameObjectsFromStepData(nextStep.nextStep, layer + 1);
			GameObject myLine = Instantiate(tec.LinePrefab, tec.transform);
			LineController lc = myLine.GetComponent<LineController>();
			lc.Init();
			lines.Add(lc);
			lc.inputField.text = nextStep.proba.ToString();

			tic.TryAddNext(lc, nextItem.GetComponent<TreatmentItemController>());
		}

		nodes[layer].Add(newGo);

		return newGo;
	}

	private void OrganizeTree(StepData first) {
        for (int i = 0; i < nodes.Count; i++) {
            for (int j = 0; j < nodes[i].Count; j++) {
                nodes[i][j].transform.position = new Vector3(-nodes[i].Count + 2 * j, 4 - 2 * i, 0f);
            }
        }

        foreach (LineController lc in lines)
			lc.UpdateMeshAndPosition();
	}
}
