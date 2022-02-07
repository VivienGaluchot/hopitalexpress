using UnityEngine;
using UnityEngine.UI;
using System;

public class DrawMenuController : MonoBehaviour {

	[SerializeField] private Dropdown LayersDropdown;

	//public enum DrawType {
	//	none,
	//	floor,
	//	eraseFloor,
	//	fillFloor,
	//	walls,
	//	eraseWalls,
	//	fillWalls,
	//	playerSpawn,
	//	patientSpawn,
	//	levelObject
	//}
	private int layer;
	private int button;

	[SerializeField] private Image[] Buttons;

	private Color oldColor;

	private void UpdateDrawType() { 
		LevelEditorController.instance.drawType = button > 0 ? (DrawType)(layer*3 + button) : DrawType.none;
	}

	public void Clicked(int index) {
		if (button != index) {
			if(button > 0) Buttons[button-1].color = oldColor;
			oldColor = Buttons[index - 1].color;
			Buttons[index-1].color = Color.green;
			button = index;
		} else {
			Buttons[index-1].color = oldColor;
			button = 0;
		}
		UpdateDrawType();
	}

	public void LayerChanged() {
		layer = LayersDropdown.value;
		UpdateDrawType();
	}

	public void Clear() {
		LevelEditorController.instance.ClearGrid(LayersDropdown.value);
	}
}
