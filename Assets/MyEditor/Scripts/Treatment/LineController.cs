using UnityEngine;
using UnityEngine.UI;

public class LineController : TreatmentObjectController {


	[SerializeField] private Transform myCanvas;
    public InputField inputField;
    public LineRenderer lr { get; private set; }
	private MeshCollider mc;

	public TreatmentItemController starter;
	public TreatmentItemController ender;

	private void Start() {
		Init();
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

	public void Init() {
		if (lr == null) lr = GetComponent<LineRenderer>();
		if (mc == null) mc = GetComponent<MeshCollider>();
	}

	public void UpdateMeshAndPosition() {
		lr.SetPositions(new Vector3[] { starter.transform.position, ender.transform.position });
		Invoke("UpdateMesh", .1f);
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
