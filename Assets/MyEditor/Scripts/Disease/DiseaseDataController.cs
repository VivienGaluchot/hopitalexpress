using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Globalization;

public class DiseaseDataController : MonoBehaviour {

	[SerializeField] private Dropdown dd;
	[SerializeField] private InputField FileNameInputField;
	[SerializeField] private string path;
	private DiseaseEditorController dec;

	private void Start() {
		dec = GetComponent<DiseaseEditorController>();
		path = Path.Combine(Application.dataPath, path);
		FetchDDOptions();
	}

	private void FetchDDOptions() {
		string[] paths = System.IO.Directory.GetFiles(path);
		List<string> pathsList = new List<string>();
		foreach (string s in paths) {
			if (!s.EndsWith(".meta"))
				pathsList.Add(Path.GetFileName(s));
		}
		dd.ClearOptions();
		dd.AddOptions(pathsList);
	}

	public class DiseaseData {
		public DiseaseData(string name, float lifespan, float points, string sprite, string treatment) {
			this.name = name; this.lifespan = lifespan; this.points = points; this.sprite = sprite; this.treatment = treatment;
        }

		public string name;
		public float lifespan;
		public float points;
		public string sprite;
		public string treatment;
    }

	public void SaveData() {
		WriteToFile(JsonUtility.ToJson(FetchDataToDiseaseData()));
		FetchDDOptions();
	}

	public DiseaseData FetchDataToDiseaseData() {
		string name = dec.Name.text;
		float lifespan = ParseFromString(dec.Lifespan.text);
		float points = ParseFromString(dec.Points.text);
		string sprite = dec.SickFaceImage.sprite.name;
		string treatment = dec.Treatment.options[dec.Treatment.value].text;

		return new DiseaseData(name, lifespan, points, sprite, treatment);
	}

	private float ParseFromString(string s) {
		string value = s.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
		value = value.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

		return float.Parse(value);
    }

	private void WriteToFile(string content) {
		StreamWriter sw = new StreamWriter(Path.Combine(path, FileNameInputField.text + ".txt"));
		sw.WriteLine(content);
		sw.Close();
	}

	public void LoadData() {
		string filename = dd.options[dd.value].text;
		DiseaseData Data = JsonUtility.FromJson<DiseaseData>(ReadFromFile(filename));
		FileNameInputField.text = Path.GetFileNameWithoutExtension(filename);
		DisplayLoadedData(Data);
	}

	private void DisplayLoadedData(DiseaseData Data) {
		dec.Name.text = Data.name;
		dec.Lifespan.text = Data.lifespan.ToString();
		dec.Points.text = Data.points.ToString();

		Sprite[] sickFaces = Resources.LoadAll<Sprite>("Illustrations/Perso/Faces");
		for(int i = 0; i < sickFaces.Length; i++) {
			if (sickFaces[i].name == Data.sprite) {
				dec.SickFaceImage.sprite = sickFaces[i];
				break;
			}
		}

		for(int i = 0; i < dec.Treatment.options.Count; i++) {
			if(dec.Treatment.options[i].text == Data.treatment) {
				dec.Treatment.value = i;
				break;
            }
        }
	}

	private string ReadFromFile(string fileName) {
		StreamReader sr = new StreamReader(Path.Combine(path, fileName));
		return sr.ReadToEnd();
	}
}