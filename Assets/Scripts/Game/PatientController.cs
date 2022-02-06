using UnityEngine;
using UnityEngine.UI;

public class PatientController : MonoBehaviour {

	public GameController gc { get; private set; }

	[SerializeField] private GameObject face;
	[SerializeField] private GameObject body;
	[SerializeField] private GameObject needBubble;
	[SerializeField] private Image TimeBarImage;

	public float noDiseaseDuration;
	public float diseaseDuration;

	private Disease myDisease;

	public enum States { 
		sick,
		cured,
		dead
	}

	public States state { get; private set; }
	private float lifetime;

	public int patientValue { get; private set; }

	private float periodWithoutDiseaseAnimation = 0;
	private GameObject needIcon = null;

	private void Start() {
		state = States.sick;
		myDisease = new Disease(this);
		lifetime = myDisease.myInfos._lifespan;
		patientValue = myDisease.myInfos._points;
		face.GetComponent<SkinManager>().skinSelected = myDisease.GetFaceSkinIndex();
		DisplayNextNeed();
	}

	void Update() {
		if (state == States.sick) {
			lifetime -= Time.deltaTime;
			TimeBarImage.fillAmount = lifetime / myDisease.myInfos._lifespan;
			if (lifetime < 0f)
				DiseaseLifetimeElapsed();

			if (noDiseaseDuration > 0 && diseaseDuration > 0) {
				periodWithoutDiseaseAnimation += Time.deltaTime;
				if (periodWithoutDiseaseAnimation > (noDiseaseDuration + diseaseDuration)) {
					periodWithoutDiseaseAnimation = 0;
					face.GetComponent<SkinManager>().frameSelected = 0;
					needBubble.SetActive(true);
				} else if (periodWithoutDiseaseAnimation > noDiseaseDuration) {
					face.GetComponent<SkinManager>().frameSelected = 1;
					needBubble.SetActive(false);
				}
			}
		}
	}

	public void Initialize(GameController parent) {
		gc = parent;
	}

	public void DisplayNextNeed() {
		if (needIcon) {
			Destroy(needIcon);
			needIcon = null;
		}
		GameObject icon = myDisease.GetNeedIcon();
		if (icon != null) {
			icon.transform.SetParent(needBubble.transform);
			needIcon = icon;
		}
	}

	public void TakeItem(GameObject item) {
		if(state == States.sick)
			myDisease.TakeItem(item.GetComponent<ItemController>().itemType.ToString());

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
		state = States.dead;
		face.GetComponent<SkinManager>().skinSelected = 3;
		needBubble.SetActive(false);
		gc?.PatientDead(this);
	}

	public void DiseaseCured() {
		TimeBarImage.transform.parent.gameObject.SetActive(false);
		face.GetComponent<SkinManager>().skinSelected = 4;
		needBubble.SetActive(false);
		state = States.cured;
	}

}
