using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class TreatmentItemController : TreatmentObjectController {

	public string path;
	public List<LineController> startingLines, endingLines;
	public InputField valueField;

	private bool isClicked, isMoving;
	private Vector3 clickedPos, offset;

	private void Start() {
		if(startingLines == null) startingLines = new List<LineController>();
		if(endingLines == null) endingLines = new List<LineController>();
	}

	private void Update() {
		if(isClicked) {
			Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);

			if (!isMoving)
				isMoving = (clickedPos - worldPos).sqrMagnitude > .1f;

			if (isMoving) {
				TreatmentEditorController.instance.StopDrawLine();
				transform.position = worldPos + offset;
				for (int i = 0; i < startingLines.Count; i++) {
					if (startingLines[i])
						startingLines[i].lr.SetPosition(0, transform.position);
					else
						startingLines.RemoveAt(i);
				}
				for (int i = 0; i < endingLines.Count; i++) {
					if (endingLines[i])
						endingLines[i].lr.SetPosition(1, transform.position);
					else
						endingLines.RemoveAt(i);
				}
			}
		}   
	}

	public override void Delete() {
		foreach (LineController lc in endingLines) {
			lc.starter.startingLines.Remove(lc);
			Destroy(lc.gameObject);
		}
		foreach (LineController lc in startingLines) {
			lc.ender.endingLines.Remove(lc);
			Destroy(lc.gameObject);
		}
		TreatmentEditorController.instance.overTICs.RemoveAll(e => e == this);
		TreatmentEditorController.instance.TreatmentItems.RemoveAll(e => e == this);

		Destroy(gameObject);
	}

	public float TimeDisplayedValue() {
		string value = valueField.text;
		return value != "" ? float.Parse(value) : 0f;
	}

	// We try to add the item as next
	public bool TryAddNext(LineController line, TreatmentItemController item) {
		// Check if it's already our next or if cycle
		if(!ConfirmedNoLoop(item, this))
			return false;

		// Check if we're not already its next
		foreach (LineController lc in endingLines)
			if (lc.starter == item)
				return false;

		// All good, add lines
		item.endingLines.Add(line);
		startingLines.Add(line);
		line.starter = this;
		line.ender = item;
		return true;
	}

	private bool ConfirmedNoLoop(TreatmentItemController current, TreatmentItemController target) {
		foreach (LineController lc in current.startingLines) {
			if (lc.ender == target || !ConfirmedNoLoop(lc.ender, target))
				return false;
		}

		return true;
    }

	private void OnMouseDown() {
		isClicked = true;
		isMoving = false;
		clickedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0f, 0f, 10f);
		offset = transform.position - clickedPos;
	}

	private void OnMouseUp() {
		isClicked = false;
		if(!isMoving) {
			TreatmentEditorController.instance.clickedObject = gameObject;
			TreatmentEditorController.instance.DrawLine(this);
		} else {
			foreach (LineController lc in startingLines)
				lc.UpdateMesh();
			foreach (LineController lc in endingLines)
				lc.UpdateMesh();
		}
	}

	private void OnMouseEnter() {
		if (!TreatmentEditorController.instance.overTICs.Contains(this))
			TreatmentEditorController.instance.overTICs.Add(this);
	}

    private void OnMouseExit() {
		TreatmentEditorController.instance.overTICs.RemoveAll(e => e == this);
	}
}
