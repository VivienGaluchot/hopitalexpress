using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeMenuController : MonoBehaviour {

    public GameObject continueButton;

    public GameObject newGameButton;

    public GameObject editorButton;

    public GameObject optionButton;

    public GameObject quitButton;


    void Start() {
        continueButton.GetComponent<Button>().onClick.AddListener(onContinue);
        newGameButton.GetComponent<Button>().onClick.AddListener(onNewGame);
        editorButton.GetComponent<Button>().onClick.AddListener(onEditor);
        optionButton.GetComponent<Button>().onClick.AddListener(onOption);
        quitButton.GetComponent<Button>().onClick.AddListener(onQuit);
    }

    private void onContinue() {

    }

    private void onNewGame() {
        StartCoroutine(LoadAsync("PlayerSelectionScene"));
    }

    private void onEditor() {
        StartCoroutine(LoadAsync("EditorScene"));
    }

    private void onOption() {

    }

    private void onQuit() {
        Application.Quit();
    }

    private IEnumerator LoadAsync(string sceneName) {
        continueButton.SetActive(false);
        newGameButton.SetActive(false);
        editorButton.SetActive(false);
        optionButton.SetActive(false);
        quitButton.SetActive(false);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone) {
            yield return null;
        }
    }
}
