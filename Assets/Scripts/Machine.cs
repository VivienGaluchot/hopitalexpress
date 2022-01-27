using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Machine : SeatController {

    public string machineName;
	public Image TimeBar;

	private float elapsedtime, neededTime;
	private bool isWorking;

    protected override void Start() {
		base.Start();
		isWorking = false;
		TimeBar.transform.parent.gameObject.SetActive(false);
	}

    private void Update() {
        if(isWorking) {
			elapsedtime += Time.deltaTime;
			TimeBar.fillAmount = elapsedtime / neededTime;
			if(elapsedtime > neededTime) {
				// Our job here is done, tell the patient
				isWorking = false;
				TimeBar.transform.parent.gameObject.SetActive(false);
				goHeld.GetComponent<PatientController>().MachineDone(machineName, elapsedtime);
			}
        }
    }

    public override bool ReceiveHold(GameObject target) {
		if(base.ReceiveHold(target)) {
			// Ask patient if machine is needed
			// If yes, for how long?
			var patientAnswer = goHeld.GetComponent<PatientController>().UseMachine(machineName);
			
			if(patientAnswer.isNeeded) {
				// Machine is needed, activate it and set up time (and timebar)
				TimeBar.transform.parent.gameObject.SetActive(true);
				elapsedtime = 0f;
				isWorking = true;
				neededTime = patientAnswer.time;
            }

			return true;
        }

		return false;
	}

	public override GameObject GiveHold() {
		TimeBar.transform.parent.gameObject.SetActive(false);
		isWorking = false;
		return base.GiveHold();
	}

}
