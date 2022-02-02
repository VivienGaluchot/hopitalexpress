using UnityEngine;
using UnityEngine.UI;

public class LineController : MonoBehaviour {

	private Transform myCanvas;
	private LineRenderer lr;

	private void Start() {
		if (myCanvas == null) {
			myCanvas = transform.GetChild(0);
			myCanvas.gameObject.SetActive(false);
		}

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

	public InputField ProbaDisplayedValue() {
		return myCanvas.Find("InputField").GetComponent<InputField>();
	}

	public void DisplayCanvas() {
		if(myCanvas == null)
			myCanvas = transform.GetChild(0);

		myCanvas.gameObject.SetActive(true);
	}
}
