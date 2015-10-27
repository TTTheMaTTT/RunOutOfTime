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
			PlayerPrefs.SetInt("LastLevel",PlayerPrefs.GetInt("LastLevel")+1);
			if (PlayerPrefs.GetInt("LastLevel")<10)
				PlayerPrefs.SetString("LastLevelName","Level0"+PlayerPrefs.GetInt("LastLevel"));
			else
				PlayerPrefs.SetString("LastLevelName","Level"+PlayerPrefs.GetInt("LastLevel"));
		}
		File.Delete(Application.dataPath + "SavedData"+nextLevel+".xml");
		DeletePrefs ();
		PlayerPrefs.SetInt ("NewLevel", 1);
		Application.LoadLevel (nextLevel);
	}

	void DeletePrefs()//при переходе на следующий уровень некоторые данные должны быть удалены
	{
		PlayerPrefs.DeleteKey("AnchNumber");
		PlayerPrefs.DeleteKey("CameraSize");
		PlayerPrefs.DeleteKey("nu");
		PlayerPrefs.DeleteKey("beginTime");
	}
}
