using UnityEngine;

public class PatientController : MonoBehaviour {

	private GameController gc;
	private GameObject needDisplayer;
	private SpriteRenderer needSr;

	[SerializeField] private Sprite happySprite;
	[SerializeField] private Sprite deadSprite;
	[SerializeField] private GameObject[] Diseases;
	private Disease myDisease;
	private SpriteRenderer sr;

	private bool isDiagnosticed;
	public bool isCured { get; private set; }

	private void Start() {
		sr = GetComponent<SpriteRenderer>();
		GameObject newDisease = Instantiate(Diseases[Random.Range(0, Diseases.Length)], transform);
		myDisease = newDisease.GetComponent<Disease>();
		myDisease.Initialize(this);
		sr.sprite = myDisease.sickFace;
		isCured = false;

		needDisplayer = transform.GetChild(0).gameObject;
		needSr = needDisplayer.transform.GetChild(0).GetComponent<SpriteRenderer>();
		needDisplayer.SetActive(false);
	}

    public void Initialize(GameController parent) {
		gc = parent;
    }

    private void Diagnostic() {
		isDiagnosticed = true;
		needDisplayer.SetActive(true);
		DisplayNextNeed();

	}

	public void DisplayNextNeed() {
		needSr.sprite = myDisease.GetNeedSprite();
	}

	public void TakeItem(GameObject item) {
		if(isDiagnosticed) {
			myDisease.TakeItem(item.GetComponent<ItemController>().itemName);
		}
		Destroy(item);
	}

	public void TryMachine(string machineName) {
		if (machineName == "Bed")
			Diagnostic();
    }

	public void DiseaseLifetimeElapsed() {
		needDisplayer.SetActive(false);
		sr.sprite = deadSprite;
		gc.PatientDead(this);
	}

	public void DiseaseCured() {
		needDisplayer.SetActive(false);
		isCured = true;
		sr.sprite = happySprite;
	}

}
