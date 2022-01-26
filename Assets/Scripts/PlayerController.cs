using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	[SerializeField] private Text pointsText;
	private int score;

	[SerializeField] private Transform PlaceHolder;
	[SerializeField] private float speed;
	private Rigidbody2D rb2D;

	private List<GameObject> seatTargets, itemTargets, patientTargets, containerTargets;
	private GameObject trashTarget, exitTarget;
	private GameObject HeldGO;

	enum HeldTypes {
		none,
		patient,
		item
	}

	private HeldTypes heldType;

	void Start() {
		rb2D = GetComponent<Rigidbody2D>();
		seatTargets = new List<GameObject>();
		itemTargets = new List<GameObject>();
		patientTargets = new List<GameObject>();
		containerTargets = new List<GameObject>();
		heldType = HeldTypes.none;

		score = 0;
		pointsText.text = score.ToString();
	}

	private void Update() {
		if(Input.GetKeyDown("space")) {
			// Are we already holding something?
			switch(heldType) {
				case HeldTypes.patient:
					// We are holding a patient, we try to put him somewhere
					if(!TryPutPatientToExit())
						if(!TryPutPatientOnSeat())
							TryPutPatientInTrash();
					break;
				case HeldTypes.item:
					// We try to give the item, if we can't, then drop it
					if(!TryGiveItemToPatient())
						TryDropItem();
					break;
				case HeldTypes.none:
					// Holding nothing, can we grab something?
					if (!TryTakePatientFromSeat())
						if (!TryTakeItem())
							TryTakeFromContainer();
					break;
			}
		}
	}

	void FixedUpdate() {
		float horiz = Input.GetAxis("Horizontal"), vert = Input.GetAxis("Vertical");
		Vector3 m_Input = new Vector3(horiz, vert, 0);
		
		// y'a p'têt un moyen plus joli de faire ça XD
		if(vert == 0) {
			if (horiz > 0)
				transform.rotation = Quaternion.Euler(0, 0, 90);
			else if (horiz < 0)
				transform.rotation = Quaternion.Euler(0, 0, -90);
		} else if (horiz == 0) {
			if(vert > 0)
				transform.rotation = Quaternion.Euler(0, 0, 180);
			else if (vert < 0)
				transform.rotation = Quaternion.Euler(0, 0, 0);
		} else {
			// limit diagonal speed
			horiz /= 1.414f;
			vert /= 1.414f;
			if (horiz > 0 && vert > 0)
				transform.rotation = Quaternion.Euler(0, 0, 135);
			else if (horiz > 0 && vert < 0)
				transform.rotation = Quaternion.Euler(0, 0, 45);
			else if (horiz < 0 && vert < 0)
				transform.rotation = Quaternion.Euler(0, 0, -45);
			else if (horiz < 0 && vert > 0)
				transform.rotation = Quaternion.Euler(0, 0, -135);
		}

		rb2D.MovePosition(transform.position + m_Input * Time.deltaTime * speed);
	}

	// Sort list items by distance from the player (closer first)
	private void SortListByDistance(List<GameObject> theList) {
		theList.Sort(delegate (GameObject x, GameObject y) {
			if (Vector3.Distance(transform.position, x.transform.position) > Vector3.Distance(transform.position, y.transform.position))
				return -1;
			else
				return 1;
		});
	}

	private bool TryTakeFromContainer() {
		if(containerTargets.Count > 0) {
			SortListByDistance(containerTargets);
			HoldMyBeer(containerTargets[0].GetComponent<Container>().GiveItem());
			heldType = HeldTypes.item;
			return true;
		}

		return false;
	}

	private bool TryTakeItem() {
		// Look for item nearby
		if(itemTargets.Count > 0) {
			// Sort by distance
			SortListByDistance(itemTargets);
			HoldMyBeer(itemTargets[0]);
			heldType = HeldTypes.item;
			return true;
		}

		return false;
	}

	private bool TryDropItem() {
		HeldGO.transform.parent = null;
		HeldGO.transform.rotation = Quaternion.identity;
		HeldGO.GetComponent<Rigidbody2D>().simulated = true;
		HeldGO = null;
		heldType = HeldTypes.none;

		return true;
	}

	private bool TryGiveItemToPatient() {
		// Look for closer patient to give item
		if(patientTargets.Count > 0) {
			SortListByDistance(patientTargets);
			patientTargets[0].GetComponent<PatientController>().TakeItem(HeldGO);

			HeldGO = null;
			heldType = HeldTypes.none;

			return true;
		}
		return false;
	}

	private bool TryTakePatientFromSeat() {
		// Look for a not empty seat in seatTargets
		if (seatTargets.Count > 0) {
			// Step 1 : sort by distance
			SortListByDistance(seatTargets);

			// Step 2 : look for occupied seat
			GameObject occupiedSeat = null;
			for (int i = 0; i < seatTargets.Count; i++) {
				if (seatTargets[i].GetComponent<SeatController>().isHolding) {
					occupiedSeat = seatTargets[i];
					break;
				}
			}

			// Step 3 : Take patient from seat if found
			if (occupiedSeat != null) {
				HoldMyBeer(occupiedSeat.GetComponent<SeatController>().GiveHold());
				if (HeldGO != null) {
					heldType = HeldTypes.patient;
					return true;
				}

				return false;
			}
		}

		return false;
	}

	private bool TryPutPatientInTrash() {
		if(trashTarget != null) {
			Destroy(HeldGO);
			HeldGO = null;
			heldType = HeldTypes.none;

			return true;
		}

		return false;
	}

	private bool TryPutPatientOnSeat() {
		// Look for an empty seat in seatTargets
		if (seatTargets.Count > 0) {
			// Step 1 : sort by distance
			SortListByDistance(seatTargets);

			// Step 2 : look for empty seat
			GameObject emptySeat = null;
			for (int i = 0; i < seatTargets.Count; i++) {
				if (!seatTargets[i].GetComponent<SeatController>().isHolding) {
					emptySeat = seatTargets[i];
					break;
				}
			}

			// Step 3 : Allocate patient to seat if found
			if (emptySeat != null && emptySeat.GetComponent<SeatController>().ReceiveHold(HeldGO)) {
				HeldGO.transform.parent = null;
				HeldGO.transform.position = emptySeat.transform.position;
				HeldGO.transform.rotation = emptySeat.transform.rotation;
				HeldGO.GetComponent<Rigidbody2D>().simulated = true;
				HeldGO = null;
				heldType = HeldTypes.none;

				return true;
			}
		}

		return false;
	}

	private bool TryPutPatientToExit() {
		if(exitTarget != null && HeldGO.GetComponent<PatientController>().isCured) {
			score += 100;
			pointsText.text = score.ToString();
			Destroy(HeldGO);
			HeldGO = null;
			heldType = HeldTypes.none;

			return true;
		}

		return false;
    }

	private void HoldMyBeer(GameObject Beer) {
		HeldGO = Beer;
		HeldGO.GetComponent<Rigidbody2D>().simulated = false;
		HeldGO.transform.parent = PlaceHolder;
		HeldGO.transform.localPosition = Vector3.zero;
	}
	
	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.gameObject.tag == "Seat") {
			if (!seatTargets.Contains(collision.gameObject))
				seatTargets.Add(collision.gameObject);
		} else if (collision.gameObject.tag == "Item") {
			if (!itemTargets.Contains(collision.gameObject))
				itemTargets.Add(collision.gameObject);
		} else if (collision.gameObject.tag == "Patient") {
			if (!patientTargets.Contains(collision.gameObject))
				patientTargets.Add(collision.gameObject);
		} else if (collision.gameObject.tag == "Container") {
			if (!containerTargets.Contains(collision.gameObject))
				containerTargets.Add(collision.gameObject);
		} else if (collision.gameObject.tag == "Trash") {
			trashTarget = collision.gameObject;
		} else if (collision.gameObject.tag == "Exit") {
			exitTarget = collision.gameObject;
		}
	}

	private void OnTriggerExit2D(Collider2D collision) {
		if (seatTargets.Contains(collision.gameObject))
			seatTargets.Remove(collision.gameObject);
		else if (itemTargets.Contains(collision.gameObject))
			itemTargets.Remove(collision.gameObject);
		else if (patientTargets.Contains(collision.gameObject))
			patientTargets.Remove(collision.gameObject);
		else if (containerTargets.Contains(collision.gameObject))
			containerTargets.Remove(collision.gameObject);
		else if (trashTarget = collision.gameObject)
			trashTarget = null;
		else if (exitTarget = collision.gameObject)
			exitTarget = null;
	}
}
