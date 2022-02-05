using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Globalization;

public class DiseaseData {
	public DiseaseData(string name, float lifespan, float points, string sprite, int faceID, string treatment) {
		this.name = name; this.lifespan = lifespan; this.points = points; this.sprite = sprite; this.faceID = faceID; this.treatment = treatment;
	}

	public string name;
	public float lifespan;
	public float points;
	public string sprite;
	public int faceID;
	public string treatment;
}

public class DiseaseDataController : DataController {

	public override void SaveData() {
		WriteToFile(JsonUtility.ToJson(FetchDataToDiseaseData()));
		FetchFilesNamesToLoad();
	}

	public DiseaseData FetchDataToDiseaseData() {
		string name = DiseaseEditorController.instance.Name.text;
		float lifespan = ParseFromString(DiseaseEditorController.instance.Lifespan.text);
		float points = ParseFromString(DiseaseEditorController.instance.Points.text);
		string sprite = DiseaseEditorController.instance.SickFaceImage.sprite.name;
		int faceID = DiseaseEditorController.instance.sickFaceID;
		string treatment = DiseaseEditorController.instance.Treatment.options[DiseaseEditorController.instance.Treatment.value].text;

		return new DiseaseData(name, lifespan, points, sprite, faceID, treatment);
	}

	private float ParseFromString(string s) {
		string value = s.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
		value = value.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

		return float.Parse(value);
    }

	public override void LoadData() {
		string filename = FilesDropdown.options[FilesDropdown.value].text + ".json";
		DiseaseData Data = JsonUtility.FromJson<DiseaseData>(ReadFromFile(filename));
		FileNameInputField.text = Path.GetFileNameWithoutExtension(filename);
		DisplayLoadedData(Data);
	}

	private void DisplayLoadedData(DiseaseData Data) {
		DiseaseEditorController.instance.Name.text = Data.name;
		DiseaseEditorController.instance.Lifespan.text = Data.lifespan.ToString();
		DiseaseEditorController.instance.Points.text = Data.points.ToString();

		Sprite[] sickFaces = Resources.LoadAll<Sprite>("Illustrations/Perso/Faces");
		for(int i = 0; i < sickFaces.Length; i++) {
			if (sickFaces[i].name == Data.sprite) {
				DiseaseEditorController.instance.SickFaceImage.sprite = sickFaces[i];
				DiseaseEditorController.instance.sickFaceID = Data.faceID;
				break;
			}
		}

		for(int i = 0; i < DiseaseEditorController.instance.Treatment.options.Count; i++) {
			if(DiseaseEditorController.instance.Treatment.options[i].text == Data.treatment) {
				DiseaseEditorController.instance.Treatment.value = i;
				break;
            }
        }
	}
}