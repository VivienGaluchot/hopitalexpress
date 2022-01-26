using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	[SerializeField] private GameObject Player;
	[SerializeField] private float playerSpeed;
	
	// new patient can only spawn on welcome seats
	[SerializeField] private GameObject[] WelcomeSeats;
	[SerializeField] private GameObject Patient;
	[SerializeField] private float spawnRate;

	private float elapsedTime;

	[SerializeField] private Text scoreText;
	private int score;
	
	private int counter;

	private bool isPlaying;

	private Dictionary<int, GameObject> Players;

	private void Start() {
		isPlaying = false;
		Players = new Dictionary<int, GameObject>();
		score = 0;
		scoreText.text = "0";
	}

	private void Update() {

		if (Input.GetKeyDown(KeyCode.Return) && !Players.ContainsKey(0)) {
			GameObject newPlayer = Instantiate(Player);
			newPlayer.GetComponent<PlayerController>().Initialize(0, this, playerSpeed);
			Players.Add(0, newPlayer);
			isPlaying = true;
		}

		if (Input.GetKeyDown(KeyCode.Joystick1Button7) && !Players.ContainsKey(1)) {
			GameObject newPlayer = Instantiate(Player);
			newPlayer.GetComponent<PlayerController>().Initialize(1, this, playerSpeed);
			Players.Add(1, newPlayer);
			isPlaying = true;
		}

		if (Input.GetKeyDown(KeyCode.Joystick2Button7) && !Players.ContainsKey(2)) {
			GameObject newPlayer = Instantiate(Player);
			newPlayer.GetComponent<PlayerController>().Initialize(2, this, playerSpeed);
			Players.Add(2, newPlayer);
			isPlaying = true;
		}

		if (isPlaying) {
			if (elapsedTime > spawnRate) {
				GameObject emptySeat = null;
				// Looking for an empty welcome seat
				for (int i = 0; i < WelcomeSeats.Length; i++) {
					if (!WelcomeSeats[i].GetComponent<SeatController>().isHolding) {
						emptySeat = WelcomeSeats[i];
						break;
					}
				}

				// An empty welcome seat was found
				if (emptySeat != null) {
					GameObject newPatient = Instantiate(Patient, emptySeat.transform.position, emptySeat.transform.rotation);
					newPatient.name = "patient " + counter++;
					newPatient.GetComponent<PatientController>().Initialize(this);
					if (!emptySeat.GetComponent<SeatController>().ReceiveHold(newPatient)) {
						// If a problem occurs, delete the patient
						Destroy(newPatient);
					}
					elapsedTime -= spawnRate;
				}

			} else
				elapsedTime += Time.deltaTime;
		}
	}

	public void PatientCured(PatientController patient) {
		score += 100;
		scoreText.text = score.ToString();
	}

	public void PatientDead(PatientController patient) {
		score -= 50;
		scoreText.text = score.ToString();
    }
}
