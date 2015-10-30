using UnityEngine;
using System.Collections;
using System.IO;

public class StarScript : MonoBehaviour {

	private Collider2D col;
	private string levelNumber;

	private bool trigger;

	public string nextLevel;


	private bool letsGo;//Переходим на новый уровень?

	void Start () 
	{
		letsGo = false;
		trigger = false;
		string levelName = Application.loadedLevelName;
		levelNumber = levelName.Substring (5);
		col = gameObject.GetComponent<Collider2D> ();
	}



	void Update()
	{
		if (letsGo) 
		{
			Debug.Log ((Application.platform == RuntimePlatform.Android? Application.dataPath: Application.persistentDataPath) + " Complete!");
			if (string.Equals (levelNumber, PlayerPrefs.GetString ("LastLevelName"))) {
				PlayerPrefs.SetInt ("LastLevel", PlayerPrefs.GetInt ("LastLevel") + 1);
				if (PlayerPrefs.GetInt ("LastLevel") < 10)
					PlayerPrefs.SetString ("LastLevelName", "Level0" + PlayerPrefs.GetInt ("LastLevel"));
				else
					PlayerPrefs.SetString ("LastLevelName", "Level" + PlayerPrefs.GetInt ("LastLevel"));
			}
			File.Delete ((Application.platform == RuntimePlatform.Android? Application.dataPath: Application.persistentDataPath) + "SavedData" + nextLevel + ".xml");
			DeletePrefs ();
			PlayerPrefs.SetInt ("NewLevel", 1);
			Application.LoadLevel (nextLevel);
		}
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if (other.gameObject.tag == Tags.character)
		{
			letsGo=true;
		}
	}

	void DeletePrefs()//при переходе на следующий уровень некоторые данные должны быть удалены
	{
		PlayerPrefs.SetInt("AnchNumber",0);
		PlayerPrefs.DeleteKey("CameraSize");
		PlayerPrefs.SetFloat("nu",0f);
		PlayerPrefs.DeleteKey("beginTime");
		PlayerPrefs.DeleteKey("Anch0Active");
		PlayerPrefs.DeleteKey("Anch1Active");
	}
}
