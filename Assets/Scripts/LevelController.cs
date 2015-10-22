using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization; 
using System;
using System.IO; 

public class LevelController : MonoBehaviour {

	private float eps = 5f, timeEps=0.007f;//TimeEps отвечает за то, насколько точно дубли будут следовать своей хронолгии
	private bool begin;

	public float refreshTime=1f;
	public float revisionTime=3f;

	public float timer;
	public GameObject[] anchors;
	public GameObject defaultAnchor;
	public GameObject character;

	public TimeChronology chronology;	// Какие действия совершали дубли?
	private string datapath;	// путь к файлу сохранения для этой локации
	private TimeSequence currentSequence;
	private List<TimeEvent> appearances= new List<TimeEvent>();//коллекция времён появления дублей
	private List<bool> whoHasAppeared=new List<bool>();//Кто уже появился из дублей?

	public int count;
	public int k;

	void Start () 
	{
		begin = true;
		timer = 0;
		datapath = Application.dataPath + "/Saves/SavedData" + Application.loadedLevel + ".xml";	
		if (File.Exists (datapath)) {	// если файл сохранения уже существует
			chronology = Serializator.DeXml (datapath);  // считываем state оттуда
			for (int i=0;i<chronology.chronology.Count;i++)
			{
				appearances.Add(chronology.chronology[i].sequence[0]);
				whoHasAppeared.Add (false);
			}
		}
		else 
		{
			SetDefaultChronology();
		}
		if (PlayerPrefs.HasKey ("startTime"))
			timer = PlayerPrefs.GetFloat ("startTime");
	}

	void Update () {
		timer += Time.deltaTime;//отсчёт времени

		for (int i=0;i<appearances.Count;i++)//Здесь создаются временные клоны
		{
			if (!whoHasAppeared[i])
			{
				if (timer>appearances[i].time)
				{
					CreateDouble(i);
					whoHasAppeared[i]=true;
				}
			}
		}
		count = chronology.chronology.Count;
		if ((Input.GetButtonDown("Cancel"))&&(!begin))//Здесь мы отправляемся в прошлое
		{
			StartCoroutine(Restart ());
		}

		if ((begin)&&(Input.GetButtonDown("Jump")))//Здесь мы придём в прошлое
		{
			CreateDouble(chronology.chronology.Count);
		}

		if (Input.GetButtonDown("Fire2"))//удаление файла сохранения. Вскоре этот код будет убран в более подходящее место
		{
			File.Delete(datapath);
			chronology.chronology.Clear();
			Application.LoadLevel (Application.loadedLevel);
		}

	}

	void SetDefaultChronology()//С этого начинается летопись хронология
	{
		chronology= new TimeChronology();
		CreateDouble (0);
	}
	

	public void SetChronology (int number, TimeEvent tEvent)//Так записывается хронология
	{
		chronology.chronology [number].AddEvent (tEvent);
	}



	public bool CompareVelocity(int number, int actNumber, Vector2 velocity)//Функция проверки, насколько скорость дубля отличается от его хронологичной
	{
		return (Vector2.Distance (chronology.chronology [number].sequence [actNumber].velocity, velocity) < eps);
	}

	public bool CompareTimer(int number, int actNumber)//Функция проверки, не настало ли время для перехода к новому записанному событию
	{
		if (actNumber >= chronology.chronology [number].sequence.Count)
			return false;
		else 
			return (Mathf.Abs(timer-chronology.chronology[number].sequence[actNumber].time)<timeEps);
	}

	public string ChronoAction(int number, int actNumber)//Так дубль узнаёт, какое действие ему совершить
	{
		return chronology.chronology [number].sequence [actNumber].action;
	}

	void CreateDouble(int number)//Создание дубля
	{
		GameObject doubler= Instantiate(character,defaultAnchor.transform.position, character.transform.rotation) as GameObject;
		doubler.GetComponent<CharacterController>().SetNumber(number);
		doubler.GetComponent<CharacterController>().SetActNumber(0);
		if (number == chronology.chronology.Count) 
		{
			doubler.GetComponent<CharacterController> ().underControl = true;
			currentSequence = new TimeSequence ();
			currentSequence.AddEvent (new TimeEvent (timer, new Vector2 (0f, 0f), "Appear"));
			chronology.AddSequence (currentSequence);
			begin = false;
		}
		else 
			doubler.GetComponent<CharacterController> ().underControl = false;
	}

	public void DeleteDoubles(int number, CharacterController paradox)//Удаление всех дублей, начиная с номера number, а также переключение управления на парадоксальный дубль
	{
		GameObject[] doubles = GameObject.FindGameObjectsWithTag (Tags.character);
		for (int i=0; i<doubles.Length; i++)
			if (doubles [i].GetComponent<CharacterController> ().GetNumber () > number)
				Destroy (doubles [i]);
		for (int i=chronology.chronology.Count-1; i>number; i--)
		{
			if (i<appearances.Count)
				appearances.RemoveAt(i);
			chronology.chronology.RemoveAt(i);

		}
		k++;
		for (int i=chronology.chronology[number].sequence.Count-1; i>paradox.GetActNumber(); i--)
			chronology.chronology [number].sequence.RemoveAt (i);
		paradox.underControl = true;
		k++;
	}

	IEnumerator Restart()//Отправиться в прошлое
	{
		TimeEvent tEvent = new TimeEvent(timer, new Vector2(0f,0f),"Return");
		SetChronology (chronology.chronology.Count-1, tEvent);
		Serializator.SaveXml(chronology, datapath); 
		yield return new WaitForSeconds (1f);
		Application.LoadLevel (Application.loadedLevel);
	}
}
