using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawMenuController : MonoBehaviour {
	public static DrawMenuController instance;

	public Image[] DrawButtons;
	private int currentButtonIndex;
	private bool anyClicked;

	private void Awake() {
		instance = this;
		currentButtonIndex = 1;
		anyClicked = false;
	}

    private void Update() {
		if (Input.GetKeyDown("escape"))
			Unclick();
	}

	public void Unclick() {
		if (anyClicked) {
			DrawButtons[currentButtonIndex].color = Color.white;
			anyClicked = false;
		}
	}

    // Check enum DrawType dans LevelEditorController.cs
    public void ClickedButton(int index) {
		if(anyClicked) DrawButtons[currentButtonIndex].color = Color.white;

		if(!anyClicked || index != currentButtonIndex) {
			anyClicked = true;
			currentButtonIndex = index;
			DrawButtons[currentButtonIndex].color = Color.green;
		} else {
			anyClicked = false;
		}
		LevelEditorController.instance.drawType = (DrawType)currentButtonIndex;
	}
}
