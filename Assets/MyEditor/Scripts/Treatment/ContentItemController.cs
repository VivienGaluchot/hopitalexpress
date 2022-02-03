using UnityEngine;
using UnityEngine.UI;

public class ContentItemController : MonoBehaviour {

    private TreatmentEditorController ec;
    private string path;
    private GameObject myObject;

    public void SetInformations(TreatmentEditorController edCo, GameObject go, string _path) {
        ec = edCo;
        myObject = go;
        path = _path;
    }

    public void SendInformations() {
        ec.NewFollower(myObject, path);
    }
}
