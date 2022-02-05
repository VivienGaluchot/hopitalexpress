using UnityEngine;
using UnityEngine.UI;

public class LevelMenuController : MonoBehaviour {

	private RectTransform rt;
	[SerializeField] private Text arrowText;
	private bool isVisible;

	private Image[] Buttons;
	private GameObject[] Content;
	private int activeContent;

	[SerializeField] private Color activeColor;
	[SerializeField] private Color passiveColor;

    private void Awake() {
		rt = GetComponent<RectTransform>();
		isVisible = true; 
		
		Transform tabs = transform.Find("Tabs");
		Buttons = new Image[tabs.childCount];
		for (int i = 0; i < Buttons.Length; i++) {
			Buttons[i] = tabs.GetChild(i).GetComponent<Image>();
		}
		activeContent = 1;
	}

    private void Start() {
		Transform content = transform.Find("Content");
		Content = new GameObject[content.childCount];
		for (int i = 0; i < Content.Length; i++) {
			Content[i] = content.GetChild(i).gameObject;
			Content[i].SetActive(false);
		}
		ChangeActiveContent(0);
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

	public void ChangeActiveContent(int index) {
		if (index != activeContent) {
			Content[activeContent].SetActive(false);
			Buttons[activeContent].color = passiveColor;
			Content[index].SetActive(true);
			Buttons[index].color = activeColor;
			activeContent = index;
		}
		DrawMenuController.instance.Unclick();
		LevelEditorController.instance.drawType = DrawType.none;
	}
}
