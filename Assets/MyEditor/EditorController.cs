using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System;

public class EditorController : MonoBehaviour {

	[SerializeField] private Transform canvas;

	[SerializeField] private GameObject LinePrefab;
	[SerializeField] private GameObject ValueDisplayer;

	// Store prefabs to serialize them later
	private List<PrefabItem> MyPrefabs;

	private GameObject followerGO;
	private GameObject clickedDown, lineStart;
	private GameObject myLine;
	private LineRenderer myLineLR;
	private bool isDrawingLine;

	private Vector3 clickedDownPos;

	[SerializeField] private GraphicRaycaster m_Raycaster;
	[SerializeField] private PointerEventData m_PointerEventData;
	[SerializeField] private EventSystem m_EventSystem;


    private void Start() {
		Debug.Log(Application.dataPath);
		MyPrefabs = new List<PrefabItem>();
    }
    private void Update() {
		if (Input.GetKeyDown("escape")) {
			StopDrawLine();
			Destroy(followerGO);
		}

		Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);
		if (followerGO != null)
			followerGO.transform.position = worldPos + new Vector3(.5f, -.5f, 10f);

		if (isDrawingLine)
			myLineLR.SetPosition(1, Camera.main.ScreenToWorldPoint(Input.mousePosition));

		if(Input.GetMouseButtonUp(0)) {
			// We didn't move, we have something to create, we're not over UI, we didn't clickeddownn something
			if(clickedDownPos == worldPos && !DoesHitUI()) {
				if (!clickedDown) {
					if (followerGO != null)
						MyPrefabs.Add(Instantiate(followerGO, worldPos, Quaternion.identity).GetComponent<PrefabItem>());
					else
						StopDrawLine();
				} else {
					DrawLine();
                }
			}
		} else if(Input.GetMouseButtonDown(0)) {
			clickedDownPos = worldPos;
			TryRayCastClicked();
		} else if(Input.GetMouseButton(0)) {
			// Do we drag something?
			if(!isDrawingLine && clickedDown && clickedDownPos != worldPos) {
				// WE MOVED! MOVE THE CLIKED ITEM
				clickedDown.transform.position = worldPos;
			}
        }
	}

	private void TryRayCastClicked() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);

		clickedDown = null;
		if (hit2D.collider)
			clickedDown = hit2D.collider.gameObject;
	}

	private bool DoesHitUI() {
		m_PointerEventData = new PointerEventData(m_EventSystem);
		m_PointerEventData.position = Input.mousePosition;

		List<RaycastResult> results = new List<RaycastResult>();
		m_Raycaster.Raycast(m_PointerEventData, results);

		return results.Count != 0;
	}

	public void NewFollower(string path) {
		followerGO = Resources.Load<GameObject>(path);
		if (!followerGO)
			Debug.Log("No sprite at path : " + path);
		else {
			followerGO = Instantiate(followerGO);
			followerGO.AddComponent<PrefabItem>();
			followerGO.GetComponent<PrefabItem>().SetPath(path);
			Instantiate(ValueDisplayer, followerGO.transform.position, Quaternion.identity, followerGO.transform);
		}
	}

	private void StopDrawLine() {
		isDrawingLine = false;
		lineStart = null;
		Destroy(myLine);
	}

	public void DrawLine() {
		if (lineStart == null) {
			// Start drawing a line from the clickedDown object!
			lineStart = clickedDown;
			isDrawingLine = true;
			myLine = Instantiate(LinePrefab);
			myLineLR = myLine.GetComponent<LineRenderer>();
			myLineLR.SetPosition(0, lineStart.transform.position);
		} else if (lineStart != clickedDown && !clickedDown.GetComponent<PrefabItem>().isNexted
			&& lineStart.GetComponent<PrefabItem>().TryAddNext(myLineLR, clickedDown.GetComponent<PrefabItem>())) {
			// Clicked another object, try to assign it to the end of the road (rope? line!)
			myLineLR.SetPosition(1, clickedDown.transform.position);
			myLine.GetComponent<LineController>().DisplayCanvas();

			isDrawingLine = false;
			lineStart = null;
			myLine = null;
		} else {
			// Clicked the starting object again
			// or target is already nexted
			// or we couldn't add target to clicked's next
			// then delete the line, abort mission
			StopDrawLine();
		}	
	}


	// ------------------------------------------------------------------
	// SERIALIZATION AREA -- WE SHOULD MOVE IT ELSEWHERE
	// ------------------------------------------------------------------



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
		public NextsData(List<NextData> Nexts) { this.Nexts = Nexts;  }
		public List<NextData> Nexts;
	}

	[Serializable]
	public class NextData {
		public NextData(float proba, PrefabData next) { this.proba = proba; this.next = next; }
		public float proba;
		public PrefabData next;
    }

	public PrefabData converterPrefabItemToPrefabData(PrefabItem item) {

		List<NextData> nextDataList = new List<NextData>();
		NextsData nsData = new NextsData(nextDataList);

		foreach (PrefabItem.Next next in item.Nexts) {
			nsData.Nexts.Add(new NextData(next.proba, converterPrefabItemToPrefabData(next.item)));
		}

		return new PrefabData(item.path, item.myTime, nsData);
    }

	public void SaveData() {

		PrefabItem starter = null;
		foreach(PrefabItem prefab in MyPrefabs) {
			if(!prefab.isNexted) {
				starter = prefab;
				break;
			}
		}

		WriteToFile(JsonUtility.ToJson(converterPrefabItemToPrefabData(starter)));
	}

	private void WriteToFile(string content) {
		StreamWriter sw = new StreamWriter(Application.dataPath + "/DataTest.txt");
		sw.WriteLine(content);
		sw.Close();
	}

	public void LoadData() {

    }

	private string ReadFromFile(string path) {
		return "";
    }
}
