using UnityEngine;
using System.Collections;
using System.IO;

public class MainMenu : MonoBehaviour {

	public GameObject MainMenuUI, LevelSelectUI;

	private int levelNumber;
	public int android;

	private GameObject activeWindow;

	void Start()
	{
		activeWindow = MainMenuUI;
		MainMenuUI.SetActive (true);
		LevelSelectUI.SetActive (false);
		PlayerPrefs.SetInt ("AndroidMod", android);
		if (!PlayerPrefs.HasKey ("LastLevel")) {
			PlayerPrefs.SetInt ("LastLevel", 1);
			levelNumber = 1;
			PlayerPrefs.SetString("LastLevelName","01");
		} 
		else
			levelNumber=PlayerPrefs.GetInt("LastLevel");


	}
	
	void Update	()
	{
	}
	
	public void StartGame()
	{
		PlayerPrefs.DeleteKey("AnchNumber");
		if (levelNumber < 10) 
		{
			if (File.Exists((Application.platform == RuntimePlatform.Android? Application.dataPath: Application.persistentDataPath) + "SavedDataLevel0"+levelNumber+".xml"))
				File.Delete((Application.platform == RuntimePlatform.Android? Application.dataPath: Application.persistentDataPath) + "SavedDataLevel0"+levelNumber+".xml");
			DeletePrefs();
			Application.LoadLevel ("Level0" + levelNumber);
		}
		else
		{
			if (File.Exists((Application.platform == RuntimePlatform.Android? Application.dataPath: Application.persistentDataPath) + "SavedDataLevel"+levelNumber+".xml"))
				File.Delete((Application.platform == RuntimePlatform.Android? Application.dataPath: Application.persistentDataPath) + "SavedDataLevel"+levelNumber+".xml");
			DeletePrefs();	
			Application.LoadLevel ("Level" + levelNumber);
		}
	}
	
	public void LevelSelect()
	{
		MainMenuUI.SetActive (false);
		LevelSelectUI.SetActive (true);
		activeWindow = LevelSelectUI;
		gameObject.GetComponent<SpriteRenderer> ().enabled = false;
	}

	
	public void Settings()
	{
	}

	public void Return()
	{
		activeWindow.SetActive (false);
		MainMenuUI.SetActive (true);
		activeWindow = MainMenuUI;
		gameObject.GetComponent<SpriteRenderer> ().enabled = true;
	}

	public void ChooseLevel (string lvlName)
	{
		if (File.Exists((Application.platform == RuntimePlatform.Android? Application.dataPath: Application.persistentDataPath) + "SavedData"+lvlName+".xml"))
			File.Delete((Application.platform == RuntimePlatform.Android? Application.dataPath: Application.persistentDataPath) + "SavedData"+lvlName+".xml");
		DeletePrefs();	
		Application.LoadLevel (lvlName);
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
