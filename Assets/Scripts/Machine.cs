using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machine : SeatController {

    public string machineName;

	public override bool ReceiveHold(GameObject target) {
		if(base.ReceiveHold(target)) {

			goHeld.GetComponent<PatientController>().TryMachine(machineName);

			return true;
        }

		return false;
	}

	public override GameObject GiveHold() {
		return base.GiveHold();
	}

}
