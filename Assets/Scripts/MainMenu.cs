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
			PlayerPrefs.SetString("LastLayerName","01");
		} 
		else
			levelNumber=PlayerPrefs.GetInt("LastLevel");


	}
	
	void Update	()
	{
	}
	
	public void StartGame()
	{
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
		Application.LoadLevel("Level01");
	}

	
	public void Settings()
	{
	}
}
