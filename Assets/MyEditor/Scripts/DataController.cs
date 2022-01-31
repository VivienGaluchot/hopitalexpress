using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class DataController : MonoBehaviour {

    private EditorController ec;

    private void Start() {
        ec = GetComponent<EditorController>();
    }

	public class PrefabsData {
		public List<PrefabData> myPrefabs;
	}

	[Serializable]
	public class PrefabData {
		public PrefabData(string path, float time, NextsData Nexts) { this.path = path; this.time = time; this.Nexts = Nexts; }

		public string path;
		public float time;
		public NextsData Nexts;
	}
	[Serializable]
	public class NextsData {
		public NextsData(List<NextData> Nexts) { this.Nexts = Nexts; }
		public List<NextData> Nexts;
	}

	[Serializable]
	public class NextData {
		public NextData(float proba, PrefabData next) { this.proba = proba; this.next = next; }
		public float proba;
		public PrefabData next;
	}

	public void SaveData() {
		PrefabItem starter = null;
		foreach (PrefabItem prefab in ec.MyPrefabs) {
			if (!prefab.isNexted) {
				starter = prefab;
				break;
			}
		}

		if (starter)
			WriteToFile(JsonUtility.ToJson(PrefabItemToPrefabData(starter)));
		else
			Debug.Log("NO STARTER FOUND -- Un objet ne doit pas avoir de parent pour être le point de départ du traitement");
	}

	public PrefabData PrefabItemToPrefabData(PrefabItem item) {
		List<NextData> nextDataList = new List<NextData>();
		NextsData nsData = new NextsData(nextDataList);

		foreach (PrefabItem.Next next in item.Nexts) {
			nsData.Nexts.Add(new NextData(next.proba, PrefabItemToPrefabData(next.item)));
		}

		return new PrefabData(item.path, item.myTime, nsData);
	}

	private void WriteToFile(string content) {
		StreamWriter sw = new StreamWriter(Application.dataPath + "/MyEditor/DataTest.txt");
		sw.WriteLine(content);
		sw.Close();
	}

	private List<List<GameObject>> nodes;

	public void LoadData() {
		PrefabData Data = JsonUtility.FromJson<PrefabData>(ReadFromFile(Application.dataPath + "/MyEditor/DataTest.txt"));
		nodes = new List<List<GameObject>>();
		CreateGameObjectsFromPrefabData(Data);
		OrganizeTree();
	}

	private string ReadFromFile(string path) {
		StreamReader sr = new StreamReader(path);
		return sr.ReadToEnd();
	}

	public GameObject CreateGameObjectsFromPrefabData(PrefabData data, int layer = 0) {
		if (nodes.Count < layer + 1)
			nodes.Add(new List<GameObject>());

		GameObject newItem = Instantiate(Resources.Load<GameObject>(data.path));
		newItem.AddComponent<PrefabItem>();
		PrefabItem pi = newItem.GetComponent<PrefabItem>();
		pi.path = data.path;
		pi.myTime = data.time;

		GameObject displayer = Instantiate(ec.ValueDisplayer, newItem.transform.position, Quaternion.identity, newItem.transform);
		newItem.transform.Find("ValueDisplayer(Clone)/InputField/Text").gameObject.GetComponent<Text>().text = data.time.ToString();

		foreach (NextData n in data.Nexts.Nexts) {
			GameObject nextItem = CreateGameObjectsFromPrefabData(n.next, layer+1);
			GameObject myLine = Instantiate(ec.LinePrefab);
			LineRenderer myLineLR = myLine.GetComponent<LineRenderer>();
			myLineLR.SetPositions(new Vector3[] { newItem.transform.position, nextItem.transform.position });
			myLine.GetComponent<LineController>().DisplayCanvas();

			pi.AddToStartingLine(myLineLR);
			nextItem.GetComponent<PrefabItem>().AddToEndingLine(myLineLR);
		}

		nodes[layer].Add(newItem);

		return newItem;
	}

	private void OrganizeTree() {
		Debug.Log(nodes);
    }
}
