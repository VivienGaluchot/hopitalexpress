using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MachineTypes {
	Diagnostable,
	Scanner
}

public class MachineController : SeatController {

	public string machineName;
	public Image TimeBar;

	private float elapsedTime, neededTime;
	private bool isWorking;

	protected override void Start() {
		base.Start();
		isWorking = false;
		TimeBar.transform.parent.gameObject.SetActive(false);
	}

	private void Update() {
		if(isWorking) {
			elapsedTime += Time.deltaTime;
			TimeBar.fillAmount = elapsedTime / neededTime;
			if(elapsedTime > neededTime) {
				// Our job here is done, tell the patient
				isWorking = false;
				TimeBar.transform.parent.gameObject.SetActive(false);
				holder.held.GetComponent<PatientController>().MachineDone(machineName, elapsedTime);
			}
		}
	}

	protected override void OnReceive(GameObject target) {
		base.OnReceive(target);
		// Ask patient if machine is needed
		// If yes, for how long?
		var patientAnswer = target.GetComponent<PatientController>().UseMachine(machineName);
		if(patientAnswer.isNeeded) {
			// Machine is needed, activate it and set up time (and timebar)
			if(patientAnswer.time == 0) {
				target.GetComponent<PatientController>().MachineDone(machineName, patientAnswer.time);
			} else {
				TimeBar.transform.parent.gameObject.SetActive(true);
				elapsedTime = 0f;
				isWorking = true;
				neededTime = patientAnswer.time;
			}
		}
	}

	protected override void OnGive(GameObject target) {
		base.OnGive(target);
		TimeBar.transform.parent.gameObject.SetActive(false);
		isWorking = false;
		target.GetComponent<PatientController>().LeavingMachine();
	}

}
