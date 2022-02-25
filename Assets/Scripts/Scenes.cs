using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenes {

    public enum GameScenes {
        PlayerSelectionScene,
        EditorScene,
        LevelSelectionScene,
        HomeMenuScene
    };

    static public void LoadAsync(MonoBehaviour target, GameScenes scene, Action callback=null) {
        target.StartCoroutine(LoadAsyncCoroutine(scene, callback));
    }

    static private IEnumerator LoadAsyncCoroutine(GameScenes scene, Action callback) {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene.ToString());
        while (!asyncLoad.isDone) {
            yield return null;
        }
        if (callback != null)
            callback();
    }

}
