using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatController : MonoBehaviour {


	// LAYER 6 = SEATS
	private static int seatLayer = 6;

	[SerializeField] private Transform PlacerHolder;
	public GameObject goHeld { get; protected set; }
	public bool isHolding { get; protected set; }
	private RigidbodyType2D holdRBType;

	protected static Vector3 epsilonY = new Vector3(0f, .0001f, 0f);


	protected virtual void Start() {
		gameObject.layer = seatLayer;
		isHolding = false;
	}

    public virtual bool ReceiveHold(GameObject target) {
		if(!isHolding) {
			isHolding = true;
			goHeld = target;

			goHeld.transform.parent = PlacerHolder;
			goHeld.transform.position = PlacerHolder.position;
			goHeld.transform.rotation = PlacerHolder.rotation;
			holdRBType = goHeld.GetComponent<Rigidbody2D>().bodyType;
			goHeld.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
			goHeld.GetComponent<Rigidbody2D>().simulated = true;

			return true;
		}

		return false;
	}

	public virtual GameObject GiveHold() {
		if(isHolding) {
			goHeld.GetComponent<Rigidbody2D>().bodyType = holdRBType;
			GameObject returnGO = goHeld;
			goHeld = null;
			isHolding = false;

			return returnGO;
		}

		return null;
	}
}
