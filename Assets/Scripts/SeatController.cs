using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatController : MonoBehaviour {

	public GameObject goHeld { get; private set; }

	public bool isHolding { get; private set; }

	public bool ReceiveHold(GameObject target) {
		if(!isHolding) {
			goHeld = target;
			isHolding = true;

			return true;
		}

		return false;
	}

	public GameObject GiveHold() {
		if(isHolding) {
			GameObject returnGO = goHeld;
			goHeld = null;
			isHolding = false;

			return returnGO;
		}

		return null;
	}
}
