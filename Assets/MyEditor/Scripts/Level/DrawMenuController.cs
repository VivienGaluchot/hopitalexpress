using UnityEngine;
using UnityEngine.UI;
using System;

public class DrawMenuController : MonoBehaviour {
	public static DrawMenuController instance;

	[SerializeField] private Dropdown LayersDropdown;
	[SerializeField] private Dropdown ColorsDropdown;

	private int layer;
	private int button;
	private int color;

	[SerializeField] private Image[] Buttons;

	private Color oldColor;

    private void Awake() {
		instance = this;
		ColorsDropdown.gameObject.SetActive(false);
	}

    private void UpdateDrawType() { 
		LevelEditorController.instance.SetDrawType(button > 0 ? (DrawType)(layer*3 + button) : DrawType.none, color);
	}

	public void Unclick() {
		if (button > 0) Buttons[button - 1].color = oldColor;
		button = 0;
		UpdateDrawType();
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
		if(layer == 1)
			ColorsDropdown.gameObject.SetActive(true);
		else
			ColorsDropdown.gameObject.SetActive(false);
		UpdateDrawType();
	}

	public void ColorChanged() {
		color = ColorsDropdown.value;
		UpdateDrawType();
	}

	public void Clear() {
		if(LayersDropdown.value == 0)
			LevelEditorController.instance.ClearGrid(LevelEditorController.instance.floorGrid);
		else if (LayersDropdown.value == 1)
			LevelEditorController.instance.ClearGrid(LevelEditorController.instance.wallGrid, true);
	}
}
