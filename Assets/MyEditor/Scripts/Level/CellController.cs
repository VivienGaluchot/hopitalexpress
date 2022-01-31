using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellController : MonoBehaviour {

    private LevelEditorController lec;
    private int row, column;

    private Color baseColor;

    private bool isClicked;

    public void Setup(LevelEditorController parent, int i, int j) {
        lec = parent;
        row = i;
        column = j;
    }

    private void OnMouseEnter() {
        if(!isClicked && Input.GetMouseButton(0)) {
            isClicked = true;
            lec.ClickedCell(row, column, true);
        } else if(isClicked && Input.GetMouseButton(1)) {
            isClicked = false;
            lec.ClickedCell(row, column, true, true);
        }
    }

    private void OnMouseDown() {
        if(!isClicked) {
            isClicked = true;
            lec.ClickedCell(row, column, true);
        }
    }

    private void OnMouseOver() {
        if(Input.GetMouseButtonDown(1) && isClicked) {
            isClicked = false;
            lec.ClickedCell(row, column, true, true);
        }
    }
}
