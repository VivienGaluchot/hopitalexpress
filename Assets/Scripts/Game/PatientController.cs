using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatientController : MonoBehaviour {

	[SerializeField] private GameObject face;
	[SerializeField] private GameObject clothes;
	//[SerializeField] private GameObject body;
	[SerializeField] private GameObject needBubble;
	[SerializeField] private Image TimeBarImage;

	private List<GameObject> PlayersNearby;

	public float noDiseaseDuration;
	public float diseaseDuration;

	private Disease myDisease;
	private bool needDisplayed, doingAnim;
	private (bool isSet, int value) initialClothesSkinIndex = (false, 0);

	public enum States { 
		sick,
		cured,
		dead
	}

	public States state { get; private set; }
	public float lifetime { get; private set; }

	public int patientValue { get; private set; }

	private float periodWithoutDiseaseAnimation = 0;
	private GameObject needIcon = null;

	public float timeEffect;

	private void Awake() {
		state = States.sick;
		needBubble.SetActive(false);
		timeEffect = 1f;
		PlayersNearby = new List<GameObject>();
	}
    private void Start() {
		myDisease = new Disease(this);
		lifetime = myDisease.myInfos._lifespan;
		patientValue = myDisease.myInfos._points;
		face.GetComponent<SkinManager>().skinSelected = myDisease.GetFaceSkinIndex();

		if(GameController.instance.displayFirstNeed) DisplayNextNeed();
	}

	void Update() {
		if (state == States.sick && !GameController.instance.isPaused) {
			lifetime -= Time.deltaTime * timeEffect;
			TimeBarImage.fillAmount = lifetime / myDisease.myInfos._lifespan;
			if (lifetime < 0f)
				DiseaseLifetimeElapsed();

			if (noDiseaseDuration > 0 && diseaseDuration > 0) {
				periodWithoutDiseaseAnimation += Time.deltaTime;
				if (periodWithoutDiseaseAnimation > (noDiseaseDuration + diseaseDuration)) {
					periodWithoutDiseaseAnimation = 0;
					face.GetComponent<SkinManager>().frameSelected = 0;
					needBubble.SetActive(needDisplayed);
					doingAnim = false;
				} else if (periodWithoutDiseaseAnimation > noDiseaseDuration) {
					face.GetComponent<SkinManager>().frameSelected = 1;
					needBubble.SetActive(false);
					doingAnim = true;
				}
			}
		}
	}

	public void DisplayNextNeed() {
		if (!needBubble.activeSelf) needBubble.SetActive(true);
		if (needIcon) {
			Destroy(needIcon);
			needIcon = null;
		}
		GameObject icon = myDisease.GetNeedIcon();
		if (icon != null)
			needIcon = Instantiate(icon, needBubble.transform);

		needDisplayed = true;
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
			// Stop displayed need because we're in good machine
			needDisplayed = false;
			needBubble.SetActive(false);

			return (true, step.time);
		}

		return (false, 0f);
	}

	public void LeavingMachine() {
		if(state == States.sick) {
			needDisplayed = true;
			needBubble.SetActive(!doingAnim);
		}
	}

	// We used the machine during time, tell the disease about it!
	public void MachineDone(string machineName, float time) {
		// select the blouse skin
		if (machineName == "Diagnostable") {
			initialClothesSkinIndex.isSet = true;
			initialClothesSkinIndex.value = clothes.GetComponent<SkinManager>().skinSelected;
			clothes.GetComponent<SkinManager>().skinSelected = 0;
		}
		myDisease.UsedMachine(machineName, time);
	}

	public void DiseaseLifetimeElapsed() {
		TimeBarImage.transform.parent.gameObject.SetActive(false);
		state = States.dead;
		face.GetComponent<SkinManager>().skinSelected = 3;
		needBubble.SetActive(false);
		GameController.instance.PatientDead(patientValue);
	}

	public void DiseaseCured() {
		TimeBarImage.transform.parent.gameObject.SetActive(false);
		face.GetComponent<SkinManager>().skinSelected = 4;
		needBubble.SetActive(false);
		needDisplayed = false;
		state = States.cured;
		if (initialClothesSkinIndex.isSet) {
			clothes.GetComponent<SkinManager>().skinSelected = initialClothesSkinIndex.value;
			initialClothesSkinIndex.isSet = false;
		}
	}

	public void Exited() {
		if (state == States.cured)
			GameController.instance.PatientCured(patientValue);
		else if(state == States.sick)
			GameController.instance.PatientDead(patientValue);
		// if state == States.dead then we already lost points
	}

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.tag == "Player") {
			PlayersNearby.Add(collision.gameObject);
			needBubble.SetActive(needDisplayed);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
		if (PlayersNearby.Contains(collision.gameObject)) {
			PlayersNearby.Remove(collision.gameObject);
			if (PlayersNearby.Count == 0)
				needBubble.SetActive(false);
        }
    }
}
