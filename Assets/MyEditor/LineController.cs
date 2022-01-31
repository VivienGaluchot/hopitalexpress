using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour {

	private Transform myCanvas;
	private LineRenderer lr;

	private void Start() {
		myCanvas = transform.GetChild(0);
		myCanvas.gameObject.SetActive(false);

		lr = GetComponent<LineRenderer>();
	}

	private void Update() {
		int lineCounter = lr.positionCount;
		Vector3[] linePos = new Vector3[lineCounter];
		if(lineCounter == 2) {
			lr.GetPositions(linePos);
			myCanvas.position = (linePos[0] + linePos[1])/ 2f;
		}
	}

	public void DisplayCanvas() {
		myCanvas.gameObject.SetActive(true);
	}
}
