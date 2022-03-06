using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneSwitchDetector : MonoBehaviour {

	public GameObject fillImg;

	public float loadTime = 3;

	public float unloadTime = .5f;

	public string sceneToLoad = "";


	private HashSet<PlayerWalkController> collidingPlayers = new HashSet<PlayerWalkController>();

	private List<Player> players;
	
	private float rate = 0;

	private bool isLoading = false;


	void Start() {
		players = Player.GetPlayers();
		fillImg.GetComponent<Image>().fillAmount = 0;
	}

	void Update() {
		if (sceneToLoad != "" && !isLoading) {
			bool allInside = collidingPlayers.Count >= players.Count;
			if (allInside) {
				rate += Time.deltaTime / loadTime;
			} else {
				rate -= Time.deltaTime / unloadTime;
			}
			rate = Mathf.Clamp(rate, 0, 1);
			fillImg.GetComponent<Image>().fillAmount = rate;

			if (rate == 1) {
				StartCoroutine(LoadAsync(sceneToLoad));
				isLoading = true;
			}
		}
	}
	
	private void OnTriggerEnter2D(Collider2D collision) {
		var ctr = collision.gameObject.GetComponent<PlayerWalkController>();
		if (ctr != null) {
			collidingPlayers.Add(ctr);
		}
	}

	private void OnTriggerExit2D(Collider2D collision) {
		var ctr = collision.gameObject.GetComponent<PlayerWalkController>();
		if (ctr != null) {
			collidingPlayers.Remove(ctr);
		}
	}

	private IEnumerator LoadAsync(string sceneName) {
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		while (!asyncLoad.isDone) {
			yield return null;
		}
	}
}
