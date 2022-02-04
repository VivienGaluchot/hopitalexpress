using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Windows;


public class TreatmentEditorController : MonoBehaviour {

	public static TreatmentEditorController instance;
	public List<TreatmentItemController> overTICs;
	public GameObject LinePrefab;
	public string[] treatmentPaths;

	public List<TreatmentItemController> TreatmentItems { get; private set; }
	public GameObject TreatmentItemPrefab;
	public GameObject myLine;

	public GameObject Follower, clickedObject;

	private TreatmentItemController lineStart;
	private SpriteRenderer followerSR;
	private LineRenderer myLineLR;
	private bool isDrawingLine;
	private string followerPath;

	[SerializeField] private GraphicRaycaster m_Raycaster;
	[SerializeField] private PointerEventData m_PointerEventData;
	[SerializeField] private EventSystem m_EventSystem;

	private void Start() {
		instance = this;
		overTICs = new List<TreatmentItemController>();
		TreatmentItems = new List<TreatmentItemController>();
		followerSR = Follower.GetComponent<SpriteRenderer>();
		Follower.SetActive(false);
	}

	private void Update() {
		if (Input.GetKeyDown("escape")) {
			StopDrawLine();
			Follower.SetActive(false);
		}

		if (Input.GetKeyDown("delete") && clickedObject != null) {
			TreatmentObjectController item = clickedObject.GetComponent<TreatmentObjectController>();
			item.Delete();
			StopDrawLine();
		}

		bool GMBD0 = Input.GetMouseButtonDown(0);

		if (Follower.activeSelf || GMBD0 || isDrawingLine) {
			Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);

			if (Follower.activeSelf)
				Follower.transform.position = worldPos + new Vector3(.5f, -.5f, 10f);

			if (GMBD0) {
				if(overTICs.Count == 0) {
					if (isDrawingLine) {
						StopDrawLine();
					} else if (!DoesHitUI() && Follower.activeSelf) {
						GameObject newGo = Instantiate(TreatmentItemPrefab, worldPos, Quaternion.identity, transform);
						newGo.GetComponent<TreatmentItemController>().path = followerPath;
						newGo.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = followerSR.sprite;
						TreatmentItems.Add(newGo.GetComponent<TreatmentItemController>());
					}
				}
			} else if (isDrawingLine) {
				myLineLR.SetPosition(1, worldPos);
			}
		}
	}

	public void ClearScreen() {
		for (int i = 1; i < transform.childCount; i++)
			Destroy(transform.GetChild(i).gameObject);

		TreatmentItems.Clear();
		Follower.SetActive(false);
	}

	private bool DoesHitUI() {
		m_PointerEventData = new PointerEventData(m_EventSystem);
		m_PointerEventData.position = Input.mousePosition;

		List<RaycastResult> results = new List<RaycastResult>();
		m_Raycaster.Raycast(m_PointerEventData, results);

		return results.Count != 0;
	}

	// The follower is a gameobject which follow to mouse, used to display what we'll create if we click
	public void NewFollower(Sprite sprite, string path) {
		Follower.SetActive(true);
		followerSR.sprite = sprite;
		followerPath = path;
	}

	public void DrawLine(TreatmentItemController item) {
		Follower.SetActive(false);
		if (lineStart == null) {
			// Start drawing a line
			lineStart = item;
			isDrawingLine = true;
			myLine = Instantiate(LinePrefab, transform);
			myLineLR = myLine.GetComponent<LineRenderer>();
			myLineLR.SetPosition(0, lineStart.transform.position);
		} else if (lineStart != item && !(item.endingLines.Count > 0) && lineStart.TryAddNext(myLine.GetComponent<LineController>(), item)) {
			// Clicked another object, try to assign it to the end of the road (rope? line!)
			myLineLR.SetPosition(1, item.transform.position);
			myLine.GetComponent<LineController>().UpdateMesh();

			lineStart = null;
			myLine = null;
			DrawLine(item);
		} else {
			StopDrawLine();
		}
	}

	public void StopDrawLine() {
		isDrawingLine = false;
		lineStart = null;
		Destroy(myLine);
		myLine = null;
	}
}
