using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {

    private RectTransform rt;
    [SerializeField] private Text arrowText;
    private bool isVisible;

    private Image[] Buttons;
    private GameObject[] Content;
    private int activeContent;

    [SerializeField] private Color activeColor;
    [SerializeField] private Color passiveColor;

    private void Start() {
        rt = GetComponent<RectTransform>();
        isVisible = true;

        Transform content = transform.Find("Content");
        Content = new GameObject[content.childCount];
        for (int i = 0; i < Content.Length; i++) {
            Content[i] = content.GetChild(i).gameObject;
            Content[i].SetActive(false);
        }

        Transform tabs = transform.Find("Tabs");
        Buttons = new Image[tabs.childCount];
        for (int i = 0; i < Buttons.Length; i++) {
            Buttons[i] = tabs.GetChild(i).GetComponent<Image>();
        }

        activeContent = 1;
        ChangeActiveContent(0);
    }

    private void Update() {
        if (Input.GetKeyDown("tab"))
            SwitchVisible();
    }

    public void ChangeActiveContent(int index) {
        if(index != activeContent) {
            Content[activeContent].SetActive(false);
            Buttons[activeContent].color = passiveColor;

            Content[index].SetActive(true);
            Buttons[index].color = activeColor;

            activeContent = index;
            if (activeContent == 1 || activeContent == 2) {
                LevelEditorController.instance.canDraw = true;
                LevelEditorController.instance.currentLayer = activeContent - 1;
                LevelObjectsController.instance.StopDisplay();
            } else {
                LevelEditorController.instance.canDraw = false;
                LevelObjectsController.instance.StartDisplay();
            }

            LevelEditorController.instance.UnclickFillers();
            LevelEditorController.instance.StopSelectingSpawns();
            LevelEditorController.instance.ShowWalls(activeContent == 1 || activeContent == 2);
        }
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
