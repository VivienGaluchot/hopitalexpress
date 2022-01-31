using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EditorController : MonoBehaviour {

	// IDEA : use monobehaviour.OnMouse...
	[SerializeField] private Material defaultMat, outlineMat;
	[SerializeField] private Transform canvas;
	public GameObject LinePrefab;
	public GameObject ValueDisplayer;

	// Store prefabs to serialize them later
	public List<PrefabItem> MyPrefabs { get; private set; }
	public List<GameObject> everyObjects { get; private set; }

	private GameObject followerGO, clickedDown, lineStart, myLine;
	private LineRenderer myLineLR;
	private bool isDrawingLine;

	private Vector3 clickedDownPos;

	[SerializeField] private GraphicRaycaster m_Raycaster;
	[SerializeField] private PointerEventData m_PointerEventData;
	[SerializeField] private EventSystem m_EventSystem;

    private void Start() {
		MyPrefabs = new List<PrefabItem>();
		everyObjects = new List<GameObject>();
	}
    private void Update() {
		if (Input.GetKeyDown("escape")) {
			StopDrawLine();
			Destroy(followerGO);
		}

		if (Input.GetKeyDown("delete") && clickedDown != null) {
			clickedDown.GetComponent<PrefabItem>().Delete();
			StopDrawLine();
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
					if (followerGO != null) {
						GameObject newGo = Instantiate(followerGO, worldPos, Quaternion.identity);
						MyPrefabs.Add(newGo.GetComponent<PrefabItem>());
						everyObjects.Add(newGo);
					} else
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

	public void ClearScreen() {
		foreach(GameObject go in everyObjects) {
			Destroy(go);
		}
		everyObjects.Clear();
		MyPrefabs.Clear();
	}

	private void TryRayCastClicked() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);

		if(clickedDown != null) {
			clickedDown.GetComponentInChildren<SpriteRenderer>().material = defaultMat;
			clickedDown = null;
		}
		if (hit2D.collider) {
			clickedDown = hit2D.collider.gameObject;
			clickedDown.GetComponentInChildren<SpriteRenderer>().material = outlineMat;
		}
	}

	private bool DoesHitUI() {
		m_PointerEventData = new PointerEventData(m_EventSystem);
		m_PointerEventData.position = Input.mousePosition;

		List<RaycastResult> results = new List<RaycastResult>();
		m_Raycaster.Raycast(m_PointerEventData, results);

		return results.Count != 0;
	}

	// The follower is a gameobject which follow to mouse, used to display what we'll create if we click
	public void NewFollower(string path) {
		followerGO = Resources.Load<GameObject>(path);
		if (!followerGO)
			Debug.Log("No sprite at path : " + path);
		else {
			followerGO = Instantiate(followerGO);
			followerGO.AddComponent<PrefabItem>();
			followerGO.GetComponent<PrefabItem>().path = path;
			Instantiate(ValueDisplayer, followerGO.transform.position, Quaternion.identity, followerGO.transform);
		}
	}

	private void StopDrawLine() {
		isDrawingLine = false;
		lineStart = null;
		Destroy(myLine);
	}

	private void DrawLine() {
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
			everyObjects.Add(myLine);
			myLine = null;
		} else {
			// Clicked the starting object again
			// or target is already nexted
			// or we couldn't add target to clicked's next
			// then delete the line, abort mission
			StopDrawLine();
		}	
	}
}
