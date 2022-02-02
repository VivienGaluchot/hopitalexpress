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
	[SerializeField] private Transform PatientQueueParent;
	[SerializeField] private int patientQueueSize;
	[SerializeField] public int targetFrameRate = 120;

	private string patientQueueDirection;

	private GameObject[] PatientQueue;

	private float elapsedTime, currentSpawnRate;

	[SerializeField] private Text scoreText;
	private int score, multiplicator;
	
	private int counter;

	private bool isLoaded, isPlaying;

	private Dictionary<int, GameObject> Players;

	private Vector3 playerSpawn;

	public Infos[] DiseasesAvailable;

	private void Start() {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = targetFrameRate;
		isLoaded = false;
		isPlaying = false;
		Players = new Dictionary<int, GameObject>();
		score = 0;
		multiplicator = 1;
		if(scoreText)
			scoreText.text = "0";
		PatientQueue = new GameObject[patientQueueSize];
	}

	private void Update() {
		if(isLoaded) {
			CheckNewPlayers();

			if (isPlaying) {
				if (elapsedTime > currentSpawnRate) {
					if (TrySpawnNewPatient())
						elapsedTime -= currentSpawnRate;
				} else
					elapsedTime += Time.deltaTime;
			}
		}
	}

	public void StartGame(Vector3 playerSpawn, Vector3 patientSpawn, string direction) {
		isLoaded = true;
		this.playerSpawn = playerSpawn;
		PatientQueueParent.position = patientSpawn;
		patientQueueDirection = direction;

	}

	// One at a time
	private void AdvancePatientQueue() {
		GameObject emptySeat = null;
		// Looking for an empty welcome seat
		for (int i = 0; i < WelcomeSeats.Length; i++) {
			if (!WelcomeSeats[i].GetComponent<SeatController>().isHolding) {
				emptySeat = WelcomeSeats[i];
				break;
			}
		}

		for (int i = 0; i < PatientQueue.Length; i++) {
			if (PatientQueue[i] != null) {
				if(emptySeat != null) {
					PatientQueue[i].transform.parent = null;
					PatientQueue[i].transform.position = emptySeat.transform.position;
					PatientQueue[i].transform.rotation = emptySeat.transform.rotation;
					emptySeat.GetComponent<SeatController>().ReceiveHold(PatientQueue[i]);
					emptySeat = null;
					PatientQueue[i] = null;
				}
			} else {
				// Empty place in queue, can someone advance?
				for(int j = i+1; j < PatientQueue.Length; j++) {
					if(PatientQueue[j] != null) {
						PatientQueue[i] = PatientQueue[j];
						PatientQueue[j] = null;
						PatientQueue[i].transform.localPosition = new Vector3(0, i, 0);
						break;
					}
                }
            }
		}
	}

	private bool TrySpawnNewPatient() {
		GameObject emptySeat = null;
		// Looking for an empty welcome seat
		for (int i = 0; i < WelcomeSeats.Length; i++) {
			if (!WelcomeSeats[i].GetComponent<SeatController>().isHolding) {
				emptySeat = WelcomeSeats[i];
				break;
			}
		}

		GameObject newPatient = null;
		// An empty welcome seat was found
		if (emptySeat != null) {
			newPatient = Instantiate(Patient, emptySeat.transform.position, emptySeat.transform.rotation);
			newPatient.name = "patient " + counter++;
			newPatient.GetComponent<PatientController>().Initialize(this);
			emptySeat.GetComponent<SeatController>().ReceiveHold(newPatient);

			return true;
		} else {
			// No welcome seat, try to add to queue
			for (int i = 0; i < PatientQueue.Length; i++) {
				if (PatientQueue[i] == null) {
					// Nice slot you got there mate
					PatientQueue[i] = Instantiate(Patient, PatientQueueParent);
					Vector3 nextPos = patientQueueDirection == "UP" ? new Vector3(0, i, 0)
						: patientQueueDirection == "RIGHT" ? new Vector3(i, 0, 0)
						: patientQueueDirection == "DOWN" ? new Vector3(0, -i, 0) : new Vector3(-i, 0, 0);
					PatientQueue[i].transform.localPosition = nextPos;
					PatientQueue[i].name = "patient " + counter++;
					PatientQueue[i].GetComponent<PatientController>().Initialize(this);

					return true;
				}
			}
		}

		return false;
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
		GameObject newPlayer = Instantiate(Player, playerSpawn, Quaternion.identity);
		newPlayer.GetComponent<PlayerController>().Initialize(id, this, playerSpeed);
		Players.Add(id, newPlayer);

        if (!isPlaying) {
            // The queue will try to advance each half second, starting now
            InvokeRepeating("AdvancePatientQueue", 0f, .5f);
            isPlaying = true;
        }

        currentSpawnRate = spawnRate / ((Players.Count + 1)/2f);
		elapsedTime = currentSpawnRate;
	}

	public void PatientCured(PatientController patient) {
		score += patient.patientValue * multiplicator++;
		if (scoreText)
			scoreText.text = score.ToString();
	}

	public void PatientDead(PatientController patient) {
		score -= patient.patientValue;
		multiplicator = 1;
		if (scoreText)
			scoreText.text = score.ToString();
    }
}
