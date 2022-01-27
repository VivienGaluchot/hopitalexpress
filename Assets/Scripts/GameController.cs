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

	private float elapsedTime, currentSpawnRate;

	[SerializeField] private Text scoreText;
	private int score, multiplicator;
	
	private int counter;

	private bool isPlaying;

	private Dictionary<int, GameObject> Players;

	private void Start() {
		isPlaying = false;
		Players = new Dictionary<int, GameObject>();
		score = 0;
		multiplicator = 1;
		scoreText.text = "0";
	}

	private void Update() {
		CheckNewPlayers();

		if (isPlaying) {
			if (elapsedTime > currentSpawnRate) {
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
					elapsedTime -= currentSpawnRate;
				}

			} else
				elapsedTime += Time.deltaTime;
		}
	}

	private void CheckNewPlayers() {
		if (Input.GetKeyDown(KeyCode.Return) && !Players.ContainsKey(0))
			NewPlayer(0);

		if (Input.GetKeyDown(KeyCode.Joystick1Button7) && !Players.ContainsKey(1))
			NewPlayer(1);

		if (Input.GetKeyDown(KeyCode.Joystick2Button7) && !Players.ContainsKey(2))
			NewPlayer(2);
	}

	private void NewPlayer(int id) {
		GameObject newPlayer = Instantiate(Player);
		newPlayer.GetComponent<PlayerController>().Initialize(id, this, playerSpeed);
		Players.Add(id, newPlayer);
		isPlaying = true;

		currentSpawnRate = spawnRate / Players.Count;
		elapsedTime = currentSpawnRate;
	}

	public void PatientCured(PatientController patient) {
		score += patient.patientValue * multiplicator++;
		scoreText.text = score.ToString();
	}

	public void PatientDead(PatientController patient) {
		score -= patient.patientValue;
		multiplicator = 1;
		scoreText.text = score.ToString();
    }
}
