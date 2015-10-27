using UnityEngine;
using System.Collections;
using System.IO;

public class MainMenu : MonoBehaviour {

	public GameObject MainMenuUI;

	public int levelNumber;
	public int android;

	private GameObject activeWindow;

	void Start()
	{
		activeWindow = MainMenuUI;
		MainMenuUI.SetActive (true);
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
			if (File.Exists(Application.dataPath + "SavedDataLevel0"+levelNumber+".xml"))
				File.Delete(Application.dataPath + "SavedDataLevel0"+levelNumber+".xml");
			DeletePrefs();
			Application.LoadLevel ("Level0" + levelNumber);
		}
		else
		{
			if (File.Exists(Application.dataPath + "SavedDataLevel"+levelNumber+".xml"))
				File.Delete(Application.dataPath + "SavedDataLevel"+levelNumber+".xml");
			DeletePrefs();	
			Application.LoadLevel ("Level" + levelNumber);
		}
	}
	
	public void LevelSelect()
	{
		if (File.Exists(Application.dataPath + "SavedDataLevel01.xml"))
			File.Delete(Application.dataPath + "SavedDataLevel01.xml");
		DeletePrefs ();
		Application.LoadLevel("Level01");
	}

	
	public void Settings()
	{
	}

	void DeletePrefs()//при переходе на следующий уровень некоторые данные должны быть удалены
	{
		PlayerPrefs.DeleteKey("AnchNumber");
		PlayerPrefs.DeleteKey("CameraSize");
		PlayerPrefs.DeleteKey("nu");
		PlayerPrefs.DeleteKey("beginTime");
	}
}
