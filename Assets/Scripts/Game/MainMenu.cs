using UnityEngine;
using System.Collections;
using System.IO;

public class MainMenu : MonoBehaviour {

	public GameObject MainMenuUI;

	public int levelNumber;
	public int android;

	private GameObject activeWindow;
	private InterfaceController interController;

	void Start()
	{
		interController = gameObject.GetComponent<InterfaceController> ();
		activeWindow = MainMenuUI;
		MainMenuUI.SetActive (true);
		PlayerPrefs.SetInt ("AndroidMod", android);
		if (!PlayerPrefs.HasKey ("LastLevel")) {
			PlayerPrefs.SetInt ("LastLevel", 1);
			levelNumber = 1;
			PlayerPrefs.SetString("LastLayerName","01");
		} 
		else
			levelNumber=PlayerPrefs.GetInt("LastLevel");


	}
	
	void Update	()
	{
		if (android==1)
		{
			if (Input.touchCount == 1) 
			{
				if (!string.Equals (interController.CheckButtons (), "Nothing")) {
					string s = interController.CheckButtons ();
					if (string.Equals(s,"StartGame"))
						StartGame();
					else if (string.Equals(s,"LevelSelect"))
						LevelSelect();
					else if (string.Equals(s,"Settings"))
						Settings();
				}
			}
		}
	}
	
	public void StartGame()
	{
		PlayerPrefs.DeleteKey("AnchNumber");
		if (levelNumber < 10) 
		{
			File.Delete(Application.dataPath + "/Saves/SavedDataLevel0"+levelNumber+".xml");
			Application.LoadLevel ("Level0" + levelNumber);
		}
		else
		{
			File.Delete(Application.dataPath + "/Saves/SavedDataLevel"+levelNumber+".xml");
			Application.LoadLevel ("Level" + levelNumber);
		}
	}
	
	public void LevelSelect()
	{
		File.Delete(Application.dataPath + "/Saves/SavedDataLevel01.xml");
		PlayerPrefs.DeleteKey("AnchNumber");
		Application.LoadLevel("Level01");
	}

	
	public void Settings()
	{
	}
}
