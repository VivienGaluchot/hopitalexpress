using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellController : MonoBehaviour {

    private LevelEditorController lec;
    private int row, column;

    public void Setup(LevelEditorController parent, int i, int j) {
        lec = parent;
        row = i;
        column = j;
    }

    private void OnMouseEnter() {
        if (Input.GetMouseButton(0) && !lec.DoesHitUI())
            lec.ClickedCell(row, column, true);
        else if (Input.GetMouseButton(1) && !lec.DoesHitUI())
            lec.ClickedCell(row, column, true, true);
    }

    private void OnMouseDown() {
        if (!lec.DoesHitUI())
            lec.ClickedCell(row, column, true);
    }

    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(1) && !lec.DoesHitUI()) {
            lec.ClickedCell(row, column, true, true);
        }
    }
}
