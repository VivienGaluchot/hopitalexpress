using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
	public static GameController instance;

	public bool waitForLoad, displayFirstNeed;

	[SerializeField] private float levelTime;
	[SerializeField] private Image levelTimeImage;
	private float currentLevelTime;

	[SerializeField] private GameObject playerPrefab;
	[SerializeField] private float playerSpeed;
	
	// new patient can only spawn on welcome seats
	[SerializeField] private List<GameObject> WelcomeSeats;
	[SerializeField] private GameObject Patient;
	[SerializeField] private float spawnRate;
	[SerializeField] private Transform PatientQueueParent;
	[SerializeField] private int patientQueueSize;
	[SerializeField] public int targetFrameRate = 120;

	private string patientQueueDirection;

	private GameObject[] PatientQueue;

	public List<PatientController> patientsList { get; private set; }

	private float elapsedTime, currentSpawnRate;

	[SerializeField] private Text scoreText;
	private int score, multiplicator;
	[SerializeField] private  GameObject coin;
	private Animator coinAnimator;
	
	private int counter;

	private Vector3 playerSpawn;

	public Infos[] DiseasesAvailable;
	
	[SerializeField] private GameObject PauseCanvas;
	[SerializeField] private GameObject EndCanvas;
	[SerializeField] private GameObject EndCanvasScoreText;


	// State management

	public enum State {
		NotLoaded,
		Playing,
		Paused,
		Timeout
	}

	private State state = State.NotLoaded;

	public State GetState() {
		return state;
	}

	public bool IsPlaying() {
		return state == State.Playing;
	}


	// Unity callbacks

	private void Awake() {
		instance = this;
		patientsList = new List<PatientController>();
		Application.targetFrameRate = targetFrameRate;
		score = 0;
		multiplicator = 1;
		if (scoreText)
			scoreText.text = "0";
		if (coin != null)
			coinAnimator = coin.GetComponent<Animator>();

		if (!waitForLoad) {
			PatientQueue = new GameObject[patientQueueSize];
			currentLevelTime = levelTime;
		} else {
			WelcomeSeats = new List<GameObject>();
		}
	}

	private const float clockStartHue = 100f / 255f;
	private const float clockEndHue = 0f;

	private void Update() {
		if (Input.GetKeyDown("escape")) {
			if (state == State.Playing) {
				state = State.Paused;
				PauseCanvas.SetActive(true);
			} else if (state == State.Paused) {
				state = State.Playing;
				PauseCanvas.SetActive(false);
			} else if (state == State.Timeout) {
				ExitGame();
			}
		}

		if (state == State.Playing) {
			currentLevelTime -= Time.deltaTime;
			if (currentLevelTime > 0) {
				UpdateClock(currentLevelTime);

				if (elapsedTime > currentSpawnRate) {
					if (TrySpawnNewPatient())
						elapsedTime -= currentSpawnRate;
				} else
					elapsedTime += Time.deltaTime;
			} else {
				state = State.Timeout;
				EndCanvasScoreText.GetComponent<Text>().text += score.ToString();
				EndCanvas.SetActive(true);
			}
		}
	}

	// Loading

	public void StartGame() {
		playerSpawn = Vector3.zero;
		patientQueueDirection = "UP";
		SpawnPlayersAndStart();
	}

	public void StartGame(Vector3 playerSpawn, Vector3 patientSpawn, string direction, int queueSize, float levelTime, List<GameObject> welcomeSeats) {
		this.playerSpawn = playerSpawn;
		PatientQueueParent.position = patientSpawn;
		patientQueueDirection = direction;
		PatientQueue = new GameObject[queueSize];
		currentLevelTime = levelTime;
		foreach(GameObject seat in welcomeSeats)
			if (seat.GetComponent<SeatController>() && !WelcomeSeats.Contains(seat))
				WelcomeSeats.Add(seat);
		SpawnPlayersAndStart();
	}

	public void ExitGame() {
		// Go back to level selection
		Scenes.LoadAsync(this, Scenes.GameScenes.LevelSelectionScene, () => {
			instance = null;
		});
	}

	// Scoring

	public void PatientCured(int patientValue) {
		score += patientValue * multiplicator++;
		if (scoreText)
			scoreText.text = score.ToString();
		if (coin) {
			coinAnimator.SetTrigger("addCoin");
		}
	}

	public void PatientDead(int patientValue) {
		score -= patientValue;
		multiplicator = 1;
		if (scoreText)
			scoreText.text = score.ToString();
		if (coin) {
			coinAnimator.SetTrigger("removeCoin");
		}
	}

	// Internal

	private void UpdateClock(float currentTime) {
		float ratio = Mathf.Max(currentTime / levelTime, 0);
		float newH = clockEndHue + ratio * (clockStartHue - clockEndHue);
		float H, S, V;
		Color.RGBToHSV(levelTimeImage.color, out H, out S, out V);
		H = clockEndHue + ratio * (clockStartHue - clockEndHue);
		levelTimeImage.color = Color.HSVToRGB(newH, S, V);
		levelTimeImage.fillAmount = ratio;
	}

	private void SpawnPlayersAndStart() {
		// spawn players
		var players = Player.GetPlayers();
		Player.SpawnPlayers(players, playerPrefab, playerSpawn);

		// The queue will try to advance each half second, starting now
		InvokeRepeating("AdvancePatientQueue", 0f, .5f);
		currentSpawnRate = spawnRate / ((players.Count + 1)/2f);
		elapsedTime = currentSpawnRate;

		state = State.Playing;
	}

	// One at a time
	private void AdvancePatientQueue() {
		GameObject emptySeat = null;
		// Looking for an empty welcome seat
		for (int i = 0; i < WelcomeSeats.Count; i++) {
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
		for (int i = 0; i < WelcomeSeats.Count; i++) {
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
			emptySeat.GetComponent<SeatController>().ReceiveHold(newPatient);

			patientsList.Add(newPatient.GetComponent<PatientController>());

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

					patientsList.Add(PatientQueue[i].GetComponent<PatientController>());

					return true;
				}
			}
		}

		return false;
	}
}
