using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
	public static GameController instance;

	public bool displayFirstNeed;

	[SerializeField] private float levelTime;
	[SerializeField] private Image levelTimeImage;
	private float currentLevelTime;

	[SerializeField] private GameObject Player;
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

	private bool isLoaded, isPlaying;

	private Dictionary<int, GameObject> Players;

	private Vector3 playerSpawn;

	public Infos[] DiseasesAvailable;

	public bool isPaused { get; private set; }
	[SerializeField] private GameObject PauseText;

	private void Awake() {
		instance = this;
		isPaused = false;
		PauseText.SetActive(isPaused);
		patientsList = new List<PatientController>();
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = targetFrameRate;
		isLoaded = false;
		isPlaying = false;
		Players = new Dictionary<int, GameObject>();
		score = 0;
		multiplicator = 1;
		PatientQueue = new GameObject[patientQueueSize];
		currentLevelTime = levelTime;
		if (scoreText)
			scoreText.text = "0";
		if (coin != null)
			coinAnimator = coin.GetComponent<Animator>();
	}

	private const float clockStartHue = 100f / 255f;
	private const float clockEndHue = 0f;

	private void Update() {
		if (Input.GetKeyDown("escape"))
			isPaused = !isPaused;
			PauseText.SetActive(isPaused);
		if (!isPaused) {
			if (isLoaded) {
				CheckNewPlayers();

				if (isPlaying) {
					currentLevelTime -= Time.deltaTime;
					UpdateClock(currentLevelTime);

					if (elapsedTime > currentSpawnRate) {
						if (TrySpawnNewPatient())
							elapsedTime -= currentSpawnRate;
					} else
						elapsedTime += Time.deltaTime;
				}
			}
		}
	}

	public void AddWelcomeSeat(GameObject newWelcomeSeat) {
		// If no seat controller or if already inn the list, then abort mission
		if (!newWelcomeSeat.GetComponent<SeatController>() || WelcomeSeats.Contains(newWelcomeSeat))
			return;

		WelcomeSeats.Add(newWelcomeSeat);
    }

	private void UpdateClock(float currentTime) {
		float ratio = Mathf.Max(currentTime / levelTime, 0);
		float newH = clockEndHue + ratio * (clockStartHue - clockEndHue);
		float H, S, V;
		Color.RGBToHSV(levelTimeImage.color, out H, out S, out V);
		H = clockEndHue + ratio * (clockStartHue - clockEndHue);
		levelTimeImage.color = Color.HSVToRGB(newH, S, V);
		levelTimeImage.fillAmount = ratio;
	}

	public void StartGame() {
		isLoaded = true;
		isPaused = false;
		playerSpawn = Vector3.zero;
		patientQueueDirection = "UP";
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
		newPlayer.GetComponent<PlayerController>().Initialize(id, playerSpeed);
		Players.Add(id, newPlayer);

		if (!isPlaying) {
			// The queue will try to advance each half second, starting now
			InvokeRepeating("AdvancePatientQueue", 0f, .5f);
			isPlaying = true;
		}

		currentSpawnRate = spawnRate / ((Players.Count + 1)/2f);
		elapsedTime = currentSpawnRate;
	}

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
}
