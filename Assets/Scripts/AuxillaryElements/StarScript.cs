using UnityEngine;
using System.Collections;
using System.IO;

public class StarScript : MonoBehaviour {

	private Collider2D col;
	private string levelNumber;

	public string nextLevel;

	void Start () 
	{
		string levelName = Application.loadedLevelName;
		levelNumber = levelName.Substring (5);
		col = gameObject.GetComponent<Collider2D> ();
	}

	

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == Tags.character)
			Debug.Log (Application.loadedLevelName+" Complete!");
		if (string.Equals(levelNumber,PlayerPrefs.GetString("LastLevelName")))
		{
			PlayerPrefs.SetInt("LastLayer",PlayerPrefs.GetInt("LastLayer")+1);
			if (PlayerPrefs.GetInt("LastLayer")<10)
				PlayerPrefs.SetString("LastLayerName","Level0"+PlayerPrefs.GetInt("LastLayer"));
			else
				PlayerPrefs.SetString("LastLayerName","Level"+PlayerPrefs.GetInt("LastLayer"));
		}
		File.Delete(Application.dataPath + "/Saves/SavedData"+nextLevel+".xml");
		Application.LoadLevel (nextLevel);
	}

}
