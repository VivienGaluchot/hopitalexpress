using UnityEngine;
using UnityEngine.UI;

public class LineController : TreatmentObjectController {


	[SerializeField] private Transform myCanvas;
	//[SerializeField] private InputField inputField;
	public LineRenderer lr { get; private set; }
	private MeshCollider mc;

	public TreatmentItemController starter;
	public TreatmentItemController ender;

	private void Start() {
		mc = GetComponent<MeshCollider>();
		lr = GetComponent<LineRenderer>();
	}

	private void Update() {
		int lineCounter = lr.positionCount;
		Vector3[] linePos = new Vector3[lineCounter];
		if(lineCounter == 2) {
			lr.GetPositions(linePos);
			myCanvas.position = (linePos[0] + linePos[1])/ 2f;
		}
	}

	public override void Delete() {
		starter.startingLines.Remove(this);
		ender.endingLines.Remove(this);
		Destroy(gameObject);
    }

	public void UpdateMesh() {
		Mesh mesh = new Mesh();
		lr.BakeMesh(mesh, true);
		mc.sharedMesh = mesh;
	}

    private void OnMouseUpAsButton() {
		TreatmentEditorController.instance.clickedObject = gameObject;

	}
}
