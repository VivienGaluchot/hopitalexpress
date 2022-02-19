using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionCtontroller : MonoBehaviour {

	public GameObject playerPrefab;

	public GameObject playerSpawn;

	public GameObject testButton;


	void Start() {
		Player.SpawnPlayers(playerPrefab, playerSpawn, true);
		testButton.GetComponent<Button>().onClick.AddListener(() => {
			StartCoroutine(LoadAsync("MainScene"));
		});
	}

	void Update() {
		
	}

	private IEnumerator LoadAsync(string sceneName) {
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		while (!asyncLoad.isDone) {
			yield return null;
		}
	}

}
