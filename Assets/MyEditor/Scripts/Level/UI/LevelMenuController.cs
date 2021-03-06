using UnityEngine;
using UnityEngine.UI;

public class LevelMenuController : MonoBehaviour {

	private RectTransform rt;
	[SerializeField] private Text arrowText;
	private bool isVisible;

    private void Awake() {
		rt = GetComponent<RectTransform>();
		isVisible = true; 
	}

    private void Update() {
		if (Input.GetKeyDown("tab"))
			SwitchVisible();
	}

	public void SwitchVisible() {
		if(isVisible) {
			rt.anchoredPosition = new Vector2(200f, rt.anchoredPosition.y);
			arrowText.text = "<";
		} else {
			rt.anchoredPosition = new Vector2(-200f, rt.anchoredPosition.y);
			arrowText.text = ">";
		}
		isVisible = !isVisible;
	}
}
