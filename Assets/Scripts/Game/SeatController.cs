using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatController : MonoBehaviour {

	[SerializeField] private Transform PlacerHolder;
	public GameObject goHeld { get; protected set; }

	public bool isHolding { get; protected set; }

	protected static Vector3 epsilonY = new Vector3(0f, .0001f, 0f);


	protected virtual void Start() {
		isHolding = false;
	}

    public virtual bool ReceiveHold(GameObject target) {
		if(!isHolding) {
			isHolding = true;
			goHeld = target;

			goHeld.transform.parent = PlacerHolder;
			goHeld.transform.position = PlacerHolder.position;
			goHeld.transform.rotation = PlacerHolder.rotation;
			goHeld.GetComponent<Rigidbody2D>().simulated = false;

			return true;
		}

		return false;
	}

	public virtual GameObject GiveHold() {
		if(isHolding) {
			GameObject returnGO = goHeld;
			goHeld = null;
			isHolding = false;

			return returnGO;
		}

		return null;
	}
}
