using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InitGlobalFunctions : MonoBehaviour {
	[SerializeField] private GraphicRaycaster m_Raycaster;
	[SerializeField] private PointerEventData m_PointerEventData;
	[SerializeField] private EventSystem m_EventSystem;

    private void Awake() {
		GlobalFunctions.m_Raycaster = m_Raycaster;
		GlobalFunctions.m_PointerEventData = m_PointerEventData;
		GlobalFunctions.m_EventSystem = m_EventSystem;
	}
}

public static class GlobalFunctions {

	public static GraphicRaycaster m_Raycaster;
	public static PointerEventData m_PointerEventData;
	public static EventSystem m_EventSystem;

	public static bool DoesHitUI() {
		m_PointerEventData = new PointerEventData(m_EventSystem);
		m_PointerEventData.position = Input.mousePosition;

		List<RaycastResult> results = new List<RaycastResult>();
		m_Raycaster.Raycast(m_PointerEventData, results);

		return results.Count != 0;
	}
}
