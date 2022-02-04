using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellController : MonoBehaviour {
    private int row, column;

    public void Setup(int i, int j) {
        row = i;
        column = j;
    }

    private void OnMouseEnter() {
        if (Input.GetMouseButton(0) && !LevelEditorController.instance.DoesHitUI())
            LevelEditorController.instance.ClickedCell(row, column, true);
        else if (Input.GetMouseButton(1) && !LevelEditorController.instance.DoesHitUI())
            LevelEditorController.instance.ClickedCell(row, column, true, true);
    }

    private void OnMouseDown() {
        if (!LevelEditorController.instance.DoesHitUI())
            LevelEditorController.instance.ClickedCell(row, column, true);
    }

    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(1) && !LevelEditorController.instance.DoesHitUI()) {
            LevelEditorController.instance.ClickedCell(row, column, true, true);
        }
    }
}
