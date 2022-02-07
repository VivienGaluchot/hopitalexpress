using UnityEngine;

public class CellController : MonoBehaviour {
	public int row { get; private set; }
	public int column { get; private set; }

	public void Setup(int i, int j) {
		row = i;
		column = j;
	}

	//private void OnMouseEnter() {
	//    if(!GlobalFunctions.DoesHitUI()) {
	//        if (Input.GetMouseButton(0))
	//            LevelEditorController.instance.ClickedCell(row, column, true);
	//        else if (Input.GetMouseButton(1))
	//            LevelEditorController.instance.ClickedCell(row, column, true, true);
	//    }
	//}

	//private void OnMouseDown() {
	//    if (!GlobalFunctions.DoesHitUI())
	//        LevelEditorController.instance.ClickedCell(row, column, true);

	//    // ça serait bien, mais mouseButton(1) ne trigger pas cette fonction -.-
	//    //if (!GlobalFunctions.DoesHitUI()) {
	//    //    if (Input.GetMouseButton(0))
	//    //        LevelEditorController.instance.ClickedCell(row, column, true);
	//    //    else if (Input.GetMouseButton(1))
	//    //        LevelEditorController.instance.ClickedCell(row, column, true, true);
	//    //}
	//}

	//private void OnMouseOver() {
	//    // a défaut du truc du dessus, on fait ce check en permanence tant qu'on over une cellule
	//    if (Input.GetMouseButtonDown(1) && !GlobalFunctions.DoesHitUI()) {
	//        LevelEditorController.instance.ClickedCell(row, column, true, true);
	//    }
	//}
}
