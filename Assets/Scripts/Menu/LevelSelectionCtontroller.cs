using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionCtontroller : MonoBehaviour {

	public GameObject playerPrefab;

	public GameObject playerSpawn;


	void Start() {
		Player.SpawnPlayers(Player.GetPlayers(), playerPrefab, playerSpawn);
	}

	void Update() {
		
	}

}
