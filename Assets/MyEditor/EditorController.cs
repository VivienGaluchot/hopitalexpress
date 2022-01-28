using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EditorController : MonoBehaviour {

	[SerializeField] private GameObject DisplayItem;
	[SerializeField] private Transform canvas;

	[SerializeField] private GameObject LinePrefab;

	private GameObject followerGO;
	private GameObject clicked;
	private LineRenderer myLine;
	private bool isDrawingLine;


	[SerializeField] private GraphicRaycaster m_Raycaster;
	[SerializeField] private PointerEventData m_PointerEventData;
	[SerializeField] private EventSystem m_EventSystem;

	private void Update() {
		if (Input.GetKeyDown("escape")) {
			isDrawingLine = false;
			Destroy(myLine);
			Destroy(followerGO);
		}

		if (followerGO != null) {
			Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			followerGO.transform.position = worldPos + new Vector3(.5f, -.5f, 10f);
			if (Input.GetMouseButtonDown(0) && !DoesHitUI()) {
				GameObject newGO = Instantiate(DisplayItem, Input.mousePosition, Quaternion.identity, canvas);
				newGO.GetComponent<DisplayItemController>().DisplayInformations(this, followerGO.GetComponent<SpriteRenderer>().sprite);
			}
		} else if(isDrawingLine) {
			myLine.SetPosition(1, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

	}

	private bool DoesHitUI() {
		m_PointerEventData = new PointerEventData(m_EventSystem);
		m_PointerEventData.position = Input.mousePosition;

		List<RaycastResult> results = new List<RaycastResult>();
		m_Raycaster.Raycast(m_PointerEventData, results);

		return results.Count != 0;
	}

	public void NewFollower(GameObject follower) {
		followerGO = follower;
	}

    public void ClickedItem(DisplayItemController item) {
		if(clicked == null) {
			clicked = item.gameObject;
			isDrawingLine = true;
			myLine = Instantiate(LinePrefab).GetComponent<LineRenderer>();
			myLine.SetPosition(0, item.GetComponent<RectTransform>().transform.position);
		}
    }
}
