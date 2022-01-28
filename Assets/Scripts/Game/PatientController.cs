using UnityEngine;
using UnityEngine.UI;

public class PatientController : MonoBehaviour {

	private GameController gc;

	[SerializeField] private GameObject needDisplayer;
	[SerializeField] private SpriteRenderer needSr;
	[SerializeField] private Sprite happySprite;
	[SerializeField] private Sprite deadSprite;
	[SerializeField] private Image TimeBarImage;

	private Disease myDisease;
	private SpriteRenderer sr;

	public enum States { 
		sick,
		diagnosticed,
		cured,
		dead
	}

	public States state { get; private set; }
	private float lifetime;

	public int patientValue { get; private set; }

	private void Start() {
		state = States.diagnosticed;

		sr = GetComponent<SpriteRenderer>();
		needDisplayer = transform.GetChild(0).gameObject;
		needSr = needDisplayer.transform.GetChild(0).GetComponent<SpriteRenderer>();
		//needDisplayer.SetActive(false);

		myDisease = new Disease(this);
		lifetime = myDisease.myInfos._lifespan;
		patientValue = myDisease.myInfos._points;

		sr.sprite = myDisease.sickFace;
		DisplayNextNeed();
	}

    void Update() {
        if (state == States.sick || state == States.diagnosticed) {
            lifetime -= Time.deltaTime;
			TimeBarImage.fillAmount = lifetime / myDisease.myInfos._lifespan;
            if (lifetime < 0f)
                DiseaseLifetimeElapsed();
        }
    }

    public void Initialize(GameController parent) {
		gc = parent;
    }

    private void Diagnostic() {
		state = States.diagnosticed;
		needDisplayer.SetActive(true);
		DisplayNextNeed();

	}

	public void DisplayNextNeed() {
		needSr.sprite = myDisease.GetNeedSprite();
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
		needDisplayer.SetActive(false);
		sr.sprite = deadSprite;
		state = States.dead;
		gc.PatientDead(this);
	}

	public void DiseaseCured() {
		TimeBarImage.transform.parent.gameObject.SetActive(false);
		needDisplayer.SetActive(false);
		state = States.cured;
		sr.sprite = happySprite;
	}

}
