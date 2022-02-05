using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public abstract class DataController : MonoBehaviour {

	[SerializeField] protected Dropdown FilesDropdown;
	[SerializeField] protected InputField FileNameInputField;
	[SerializeField] protected string path;
	protected bool clickedDelete;

	protected void Start() {
		path = Path.Combine(Application.dataPath, path);
		FetchFilesNamesToLoad();
	}

	public abstract void LoadData();
    public abstract void SaveData();

	protected virtual void FetchFilesNamesToLoad() {
		string[] paths = System.IO.Directory.GetFiles(path);
		List<string> pathsList = new List<string>();
		foreach (string s in paths) {
			if (!s.EndsWith(".meta"))
				pathsList.Add(Path.GetFileNameWithoutExtension(s));
		}
		FilesDropdown.ClearOptions();
		FilesDropdown.AddOptions(pathsList);
	}

	protected void WriteToFile(string content) {
		StreamWriter sw = new StreamWriter(Path.Combine(path, FileNameInputField.text + ".json"));
		sw.WriteLine(content);
		sw.Close();
	}

	protected string ReadFromFile(string fileName) {
		StreamReader sr = new StreamReader(Path.Combine(path, fileName));
		return sr.ReadToEnd();
	}
	public virtual void DeleteFile(Text text) {
		if (!clickedDelete) {
			if(FilesDropdown.options.Count > 0) {
				clickedDelete = true;
				text.text = "Sure?";
			} else {
				Debug.LogWarning("No file to delete");
            }
		} else {
			text.text = "Delete";
			clickedDelete = false;
			string filePath = Path.Combine(path, FilesDropdown.options[FilesDropdown.value].text + ".json");
			if (File.Exists(filePath))
				File.Delete(filePath);
			FetchFilesNamesToLoad();
		}
	}
}
