using UnityEngine;

public class PatientController : MonoBehaviour {

	[SerializeField] private Sprite happySprite;
	[SerializeField] private Sprite deadSprite;
	[SerializeField] private GameObject[] Diseases;
	private Disease myDisease;
	private SpriteRenderer sr;

	public bool isCured { get; private set; }

	private void Start() {
		sr = GetComponent<SpriteRenderer>();
		GameObject newDisease = Instantiate(Diseases[Random.Range(0, Diseases.Length)], transform);
		myDisease = newDisease.GetComponent<Disease>();
		myDisease.Initialize(this);
		sr.sprite = myDisease.sickFace;
		isCured = false;
	}

	public void TakeItem(GameObject item) {
		myDisease.TakeItem(item.GetComponent<ItemController>().itemName);
		Destroy(item);
    }

	public void TryMachine() {
		// ?
    }

	public void DiseaseLifetimeElapsed() {
		sr.sprite = deadSprite;
	}

	public void DiseaseCured() {
		isCured = true;
		sr.sprite = happySprite;
	}

}
