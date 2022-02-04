using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Globalization;

[Serializable]
public class StepData {
	public StepData(string name, string path, float time, NextStepsListData Nexts) { this.name = name; this.path = path; this.time = time; this.Nexts = Nexts; }

	public string name;
	public string path;
	public float time;
	public NextStepsListData Nexts;
}
[Serializable]
public class NextStepsListData {
	public NextStepsListData(List<NextStepData> Nexts) { this.Nexts = Nexts; }
	public List<NextStepData> Nexts;
}
[Serializable]
public class NextStepData {
	public NextStepData(float proba, StepData next) { this.proba = proba; this.next = next; }
	public float proba;
	public StepData next;
}

public class TreatmentDataController : MonoBehaviour {

	[SerializeField] private Dropdown FilesDropdown;
	[SerializeField] private InputField FileNameInputField;
	[SerializeField] private string path;
    private TreatmentEditorController ec;

	private bool clickedDelete;

    private void Start() {
        ec = GetComponent<TreatmentEditorController>();
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

    //public void SaveData() {
    //    PrefabItem starter = null;
    //    foreach (PrefabItem prefab in ec.MyPrefabs) {
    //        if (!prefab.isNexted) {
    //            starter = prefab;
    //            break;
    //        }
    //    }

    //    if (starter)
    //        WriteToFile(JsonUtility.ToJson(PrefabItemToStepData(starter)));
    //    else
    //        Debug.Log("NO STARTER FOUND -- Un objet ne doit pas avoir de parent pour être le point de départ du traitement");

    //    FetchDDOptions();
    //}

    //public StepData PrefabItemToStepData(PrefabItem item) {
    //	List<NextStepData> nextDataList = new List<NextStepData>();
    //	NextStepsListData nsData = new NextStepsListData(nextDataList);

    //	foreach (PrefabItem.Next next in item.Nexts) {
    //		string proba = next.proba.text != "" ? next.proba.text : (1f/item.Nexts.Count).ToString();
    //		proba = proba.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
    //		proba = proba.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

    //		nsData.Nexts.Add(new NextStepData(float.Parse(proba), PrefabItemToStepData(next.item)));
    //	}

    //	return new StepData(Path.GetFileNameWithoutExtension(item.path), item.path, item.TimeDisplayedValue(), nsData);
    //}

    private void WriteToFile(string content) {
		StreamWriter sw = new StreamWriter(Path.Combine(path, FileNameInputField.text + ".json"));
		sw.WriteLine(content);
		sw.Close();
	}

	private List<List<GameObject>> nodes;

	//public void LoadData() {
	//	ec.ClearScreen();
	//	string filename = FilesDropdown.options[FilesDropdown.value].text + ".json";
	//	StepData Data = JsonUtility.FromJson<StepData>(ReadFromFile(filename));
 //       FileNameInputField.text = Path.GetFileNameWithoutExtension(filename);
 //       nodes = new List<List<GameObject>>();
 //       CreateGameObjectsFromStepData(Data);
 //       OrganizeTree();
 //   }
	

	private string ReadFromFile(string fileName) {
		StreamReader sr = new StreamReader(Path.Combine(path, fileName));
		return sr.ReadToEnd();
	}

	//public GameObject CreateGameObjectsFromStepData(StepData data, int layer = 0) {
	//	if (nodes.Count < layer + 1)
	//		nodes.Add(new List<GameObject>());

	//	GameObject newItem = Instantiate(Resources.Load<GameObject>(data.path));
	//	ec.everyObjects.Add(newItem);
	//	newItem.AddComponent<PrefabItem>();
	//	PrefabItem pi = newItem.GetComponent<PrefabItem>();
	//	ec.MyPrefabs.Add(pi);
	//	pi.path = data.path;

	//	GameObject displayer = Instantiate(ec.ValueDisplayer, newItem.transform.position, Quaternion.identity, newItem.transform);
	//	newItem.transform.Find("ValueDisplayer(Clone)/InputField").gameObject.GetComponent<InputField>().text = data.time.ToString();

	//	List<PrefabItem.Next> PrefabItemNexts = new List<PrefabItem.Next>();
	//	foreach (NextStepData n in data.Nexts.Nexts) {
	//		GameObject nextItem = CreateGameObjectsFromStepData(n.next, layer+1);
	//		GameObject myLine = Instantiate(ec.LinePrefab);
	//		ec.everyObjects.Add(myLine);
	//		LineRenderer myLineLR = myLine.GetComponent<LineRenderer>();
	//		myLineLR.SetPositions(new Vector3[] { newItem.transform.position, nextItem.transform.position });
	//		myLine.GetComponent<LineController>().DisplayCanvas();

	//		InputField probaInputField = myLine.GetComponent<LineController>().ProbaDisplayedValue();
	//		probaInputField.text = n.proba.ToString();
	//		PrefabItemNexts.Add(new PrefabItem.Next(nextItem.GetComponent<PrefabItem>(), probaInputField));
			
	//		pi.AddToStartingLine(myLineLR);
	//		nextItem.GetComponent<PrefabItem>().AddToEndingLine(myLineLR);

	//	}
	//	pi.Nexts = PrefabItemNexts;

	//	nodes[layer].Add(newItem);

	//	return newItem;
	//}

	private void OrganizeTree() {
		for (int i = 0; i < nodes.Count; i++)
			for (int j = 0; j < nodes[i].Count; j++)
				nodes[i][j].transform.position = new Vector3(-nodes[i].Count / 2f + j, 4 - 2 * i, 0f);
	}
}
