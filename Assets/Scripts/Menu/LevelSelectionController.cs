using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionController : MonoBehaviour {

	public GameObject playerPrefab;

	public GameObject playerSpawn;


	void Start() {
		Player.SpawnPlayers(Player.GetPlayers(), playerPrefab, playerSpawn);
	}

	void Update() {
		
	}

}
