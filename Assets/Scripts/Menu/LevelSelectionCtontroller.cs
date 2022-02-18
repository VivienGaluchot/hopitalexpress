using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectionCtontroller : MonoBehaviour {
    public GameObject playerPrefab
    ;
    public GameObject playerSpawn;

    void Start() {
        foreach (Player player in Player.All) {
            GameObject obj = Instantiate(playerPrefab, playerSpawn.transform.position, Quaternion.identity, playerSpawn.transform);
            obj.GetComponent<WalkPlayerController>().SetupForPlayer(player);
            obj.GetComponent<PlayerController>().enabled = false;
        }
    }

    void Update() {
        
    }
}
