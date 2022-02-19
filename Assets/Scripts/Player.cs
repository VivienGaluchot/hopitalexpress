using System.Collections.Generic;
using UnityEngine;


public class Player {
	
	public static readonly List<Player> All = new List<Player>();


	// Spawn

	public static List<Player> SpawnPlayers(GameObject playerPrefab, GameObject playerSpawn, bool enableTestPlayersOnEmpty) {
		List<Player> players = Player.All;
		if (Player.All.Count == 0 && enableTestPlayersOnEmpty) {
			// fake players for testing
			players = new List<Player>() {
				{ new Player(PlayerInput.All[0], new Player.SkinData()) },
				{ new Player(PlayerInput.All[1], new Player.SkinData()) }
			};
		}
		float spreadX = (players.Count - 1) * 1f;
		int count = 0;
		foreach (Player player in players) {
			Vector3 offset;
			if (players.Count > 1) {
				float r = (float)count / (float)(players.Count - 1);
				offset = Vector2.Lerp(new Vector2(-spreadX / 2, 0), new Vector2(spreadX / 2, 0), r);
			} else {
				offset = new Vector3();
			}
			GameObject obj = GameObject.Instantiate(playerPrefab, playerSpawn.transform.position + offset, Quaternion.identity, playerSpawn.transform);
			obj.GetComponent<PlayerWalkController>().SetupForPlayer(player);
			count++;
		}
		return players;
	}

	public static List<Player> SpawnPlayers(GameObject playerPrefab, Vector3 playerSpawn, bool enableTestPlayersOnEmpty) {
		List<Player> players = Player.All;
		if (Player.All.Count == 0 && enableTestPlayersOnEmpty) {
			// fake players for testing
			players = new List<Player>() {
				{ new Player(PlayerInput.All[0], new Player.SkinData()) },
				{ new Player(PlayerInput.All[1], new Player.SkinData()) }
			};
		}
		float spreadX = players.Count * .7f;
		int count = 0;
		foreach (Player player in players) {
			Vector3 offset;
			if (players.Count > 1) {
				float r = (float)count / (float)(players.Count - 1);
				offset = Vector2.Lerp(new Vector2(-spreadX / 2, 0), new Vector2(spreadX / 2, 0), r);
			} else {
				offset = new Vector3();
			}
			GameObject obj = GameObject.Instantiate(playerPrefab, playerSpawn + offset, Quaternion.identity);
			obj.GetComponent<PlayerWalkController>().SetupForPlayer(player);
			count++;
		}
		return players;
	}


	// Player object

	public struct SkinData {
		public int headId;
		public int skinId;
		public int clothesId;
	}


	public readonly PlayerInput input;

	public readonly SkinData skin;

	public Player(PlayerInput input, SkinData skin) {
		this.input = input;
		this.skin = skin;
	}

}
