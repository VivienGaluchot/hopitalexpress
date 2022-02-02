using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapController : MonoBehaviour {

    [SerializeField] private List<string> Scenes;
    [SerializeField] private int currentScene;

    private bool isLoading = false;

    private void Update() {
        if(!isLoading) {
            if (Input.GetKeyDown("1") && currentScene != 0) {
                isLoading = true;
                SceneManager.LoadScene(Scenes[0]);
            } else if (Input.GetKeyDown("2") && currentScene != 1) {
                isLoading = true;
                SceneManager.LoadScene(Scenes[1]);
            } else if (Input.GetKeyDown("3") && currentScene != 2) {
                isLoading = true;
                SceneManager.LoadScene(Scenes[2]);
            }
        }
    }
}
