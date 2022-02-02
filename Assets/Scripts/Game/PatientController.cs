using UnityEngine;
using UnityEngine.UI;

public class PatientController : MonoBehaviour {

	public GameController gc { get; private set; }

	[SerializeField] private GameObject face;
	[SerializeField] private GameObject body;
	[SerializeField] private SpriteRenderer need;
	[SerializeField] private Image TimeBarImage;

	public float noDiseaseDuration;
	public float diseaseDuration;

	private Disease myDisease;

	public enum States { 
		sick,
		diagnosticed,
		cured,
		dead
	}

	public States state { get; private set; }
	private float lifetime;

	public int patientValue { get; private set; }

	private float periodWithoutDiseaseAnimation = 0;

	private void Start() {
		state = States.diagnosticed;

		myDisease = new Disease(this);
		lifetime = myDisease.myInfos._lifespan;
		patientValue = myDisease.myInfos._points;

		face.GetComponent<SkinManager>().skinSelected = myDisease.GetFaceSkinIndex();

		DisplayNextNeed();
	}

	void Update() {
		if (state == States.sick || state == States.diagnosticed) {
			lifetime -= Time.deltaTime;
			TimeBarImage.fillAmount = lifetime / myDisease.myInfos._lifespan;
			if (lifetime < 0f)
				DiseaseLifetimeElapsed();
		}

		if (noDiseaseDuration > 0 && diseaseDuration > 0) {
			periodWithoutDiseaseAnimation += Time.deltaTime;
			if (periodWithoutDiseaseAnimation > (noDiseaseDuration + diseaseDuration)) {
				periodWithoutDiseaseAnimation = 0;
				face.GetComponent<SkinManager>().frameSelected = 0;
			} else if (periodWithoutDiseaseAnimation > noDiseaseDuration) {
				face.GetComponent<SkinManager>().frameSelected = 1;
			}
		}
	}

	public void Initialize(GameController parent) {
		gc = parent;
	}

	private void Diagnostic() {
		state = States.diagnosticed;
		need.gameObject.SetActive(true);
		DisplayNextNeed();
	}

	public void DisplayNextNeed() {
		need.GetComponent<SpriteRenderer>().sprite = myDisease.GetNeedSprite();
	}

	public void TakeItem(GameObject item) {
		if(state == States.diagnosticed)
			myDisease.TakeItem(item.GetComponent<ItemController>().itemName);

		Destroy(item);
	}

	public (bool isNeeded, float time) UseMachine(string machineName) {
		// Check is the machine is needed in the current step
		// If yes, then return true and time needed
		var step = myDisease.GetCurrentStep();
		if (machineName == step.name) {
			return (true, step.time);
		}

		return (false, 0f);
	}

	// We used the machine during time, tell the disease about it!
	public void MachineDone(string machineName, float time) {
		myDisease.UsedMachine(machineName, time);
	}

	public void DiseaseLifetimeElapsed() {
		TimeBarImage.transform.parent.gameObject.SetActive(false);
		need.gameObject.SetActive(false);
		state = States.dead;
		gc?.PatientDead(this);
	}

	public void DiseaseCured() {
		TimeBarImage.transform.parent.gameObject.SetActive(false);
		need.gameObject.SetActive(false);
		state = States.cured;
	}

}
