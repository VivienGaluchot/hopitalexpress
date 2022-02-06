using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelMenuCategoryController : MonoBehaviour {

    [SerializeField] private RectTransform Content;
    [SerializeField] private RectTransform Arrow;

    private Button button;
    private bool isDisplayed, goingUp, goingDown, justFinished;
    private const float scaleSpeed = 10f;

    private void Awake() {
        isDisplayed = false;
        goingDown = false;
        goingUp = true;
        button = GetComponent<Button>();
        button.onClick.AddListener(SwitchVisibility);
    }

    private void Update() {
        if (justFinished) {
            justFinished = false;
            LayoutRebuilder.MarkLayoutForRebuild(Content);
        }
        if (goingDown) {
            float newScale = Content.localScale.y + scaleSpeed * Time.deltaTime;
            if (newScale >= 1f) {
                goingDown = false;
                newScale = 1f;
                justFinished = true;
            }
            Content.localScale = new Vector3(Content.localScale.x, newScale, Content.localScale.z);
            LayoutRebuilder.MarkLayoutForRebuild(Content);
        } else if (goingUp) {
            float newScale = Content.localScale.y - scaleSpeed * Time.deltaTime;
            if(newScale <= 0f) {
                goingUp = false;
                newScale = 0f;
                justFinished = true;
            }
            Content.localScale = new Vector3(Content.localScale.x, newScale, Content.localScale.z);
            LayoutRebuilder.MarkLayoutForRebuild(Content);
        }
    }

    private void SwitchVisibility() {
        if(isDisplayed) {
            goingDown = false;
            goingUp = true;
            Arrow.rotation = Quaternion.identity;
        } else {
            goingUp = false;
            goingDown = true;
            Arrow.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
        isDisplayed = !isDisplayed;
    }

}
