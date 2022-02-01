using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellController : MonoBehaviour {

    private LevelEditorController lec;
    private int row, column;

    private bool isClicked;

    public void Setup(LevelEditorController parent, int i, int j) {
        lec = parent;
        row = i;
        column = j;
    }

    public void AdjustClickedBool(int value) {
        isClicked = value > 0;
    }

    private void OnMouseEnter() {
        if(!isClicked && Input.GetMouseButton(0) && !lec.DoesHitUI()) {
            isClicked =  lec.ClickedCell(row, column, true);
        } else if(isClicked && Input.GetMouseButton(1) && !lec.DoesHitUI()) {
            isClicked =  lec.ClickedCell(row, column, true, true);
        }
    }

    private void OnMouseDown() {
        if(!isClicked && !lec.DoesHitUI()) {
            isClicked = lec.ClickedCell(row, column, true);
        }
    }

    private void OnMouseOver() {
        if(Input.GetMouseButtonDown(1) && isClicked && !lec.DoesHitUI()) {
            isClicked = lec.ClickedCell(row, column, true, true);
        }
    }
}
