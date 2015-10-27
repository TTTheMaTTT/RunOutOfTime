using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization; 
using System;
using System.IO; 

public class LevelController : MonoBehaviour {
	
	private float eps = 1f, checkEps=2f;
	private float timeEps=0.0001f;//TimeEps отвечает за то, насколько точно дубли будут следовать своей хронолгии
	private float timeRecoil=2f;//Эта переменная отвечает за то, насколько раньше самого раннего времени сможет появиться новый дубль	
	private bool begin, pause;

	public float refreshTime=1f;
	public float revisionTime=2f;

	public float timer;
	public GameObject[] anchors;
	private int defaultAnchNumber;
	public GameObject character;

	public TimeChronology chronology;	// Какие действия совершали дубли?
	private string datapath;	// путь к файлу сохранения для этой локации
	private TimeSequence currentSequence;
	private List<TimeEvent> appearances= new List<TimeEvent>();//коллекция времён появления дублей
	private List<bool> whoHasAppeared=new List<bool>();//Кто уже появился из дублей?
	private Transform mainCharacter;//Положение активного персонажа
	[HideInInspector]
	public bool andr;
	public InterfaceController interController;
	public int anchNumb;

	//Хронологические списки
	/*public int doubleNumber;//Номер, для которого мы строим список
	public List<float> times=new List<float>();
	public List<Vector2> velocities=new List<Vector2>();
	public List<string> actions=new List<string>();*/

	public List<Vector2> places=new List<Vector2>();

	void Start () 
	{
		interController = gameObject.GetComponent<InterfaceController> ();
		if (PlayerPrefs.HasKey ("AnchNumber"))//где создавать главный дубль?
			defaultAnchNumber=PlayerPrefs.GetInt("AnchNumber");
		else 
		{
			PlayerPrefs.SetInt("AnchNumber",0);
			defaultAnchNumber=PlayerPrefs.GetInt("AnchNumber");
		}
		andr = (PlayerPrefs.GetInt ("AndroidMod") == 1);//включить режим Андроид-приложения?

		begin = true; pause = false;

		for (int i=0; i<anchors.Length; i++)//инициализация якорей
			anchors [i].GetComponent<AnchorScript> ().SetNumber (i);

		if (!PlayerPrefs.HasKey ("beginTime")) 
		{
			PlayerPrefs.SetFloat ("beginTime", 0f);
			timer=PlayerPrefs.GetFloat("beginTime");
		}
		else
			timer=PlayerPrefs.GetFloat("beginTime")-timeRecoil;
		datapath = Application.dataPath + "/Saves/SavedData" + Application.loadedLevelName + ".xml";	
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
	}

	void Update () {
		timer += Time.deltaTime;//отсчёт времени
		anchNumb=PlayerPrefs.GetInt("AnchNumber");
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

		if ((Input.GetButtonDown("Cancel"))&&(!begin))//Здесь мы отправляемся в прошлое
		{
			StartCoroutine(Restart (new Vector2(mainCharacter.position.x,mainCharacter.position.y)));
		}

		if (Input.GetButtonDown("Pause"))//Функция паузы
		{
			pause=!pause;
		}

		if (pause) //Сама пауза
			Time.timeScale = 0f;
		else
			Time.timeScale = 1f;
		if (andr)
		{
			if (Input.touchCount == 1) 
			{
				if (!string.Equals (interController.CheckButtons (), "Nothing")) {
					string s = interController.CheckButtons ();
					if (string.Equals(s,"TimeTravel"))
						Return();
					else if (string.Equals(s,"Pause"))
						Pause();
				}
			}
		}
		if (begin)//Здесь мы придём в прошлое
		{
			bool appear=false;
			if ((andr))
			{
				if (Input.touchCount==1)
				{
					Touch touch=Input.GetTouch(0);
					if ((touch.phase==TouchPhase.Began)&&
					    (string.Equals(interController.CheckButtons(),"Nothing")))
						appear=true;
				}
			}
			else if (Input.GetButton("Jump"))
				appear=true;
			if(appear)
			{
				if (timer<PlayerPrefs.GetFloat("beginTime"))
					PlayerPrefs.SetFloat("beginTime",timer);
				CreateDouble(chronology.chronology.Count);
			}
		}

		if (Input.GetButtonDown("Vertical"))
		{
			AppearancesPlaces();
		}

		if (Input.GetButtonDown("Fire2"))//удаление файла сохранения. Вскоре этот код будет убран в более подходящее место
		{
			File.Delete(datapath);
			chronology.chronology.Clear();
			PlayerPrefs.DeleteKey("beginTime");
			PlayerPrefs.DeleteKey("AnchNumber");
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



	public bool CompareLocationPrecisely(int number, int actNumber, Vector2 location)//Функция проверки, насколько скорость дубля отличается от его хронологичной
	{
		return (Vector2.Distance(chronology.chronology [number].sequence [actNumber].location, location) < eps);
	}

	public bool CompareLocation(int number, int actNumber, Vector2 location)//Такая же функция, только менее точная
	{
		//if (Mathf.Abs (chronology.chronology [number].sequence [actNumber].velocity.x - velocity.x) < checkEps)
		//{
		//	Time.timeScale = 0f;
		//}
		return ((Vector2.Distance(chronology.chronology [number].sequence [actNumber].location, location) < checkEps)||
			(!String.Equals(chronology.chronology[number].sequence[actNumber].action,"ChangeLocation")));
			
	}

	//Следующие четыре функции позволяют проверить, следует ли дубль своему каконическому поведению
	public Vector2 WhatChronologicalLocation(int number, int actNumber)
	{
		return chronology.chronology [number].sequence [actNumber].location;
	}

	public float WhatChronologicalTime (int number, int actNumber)
	{
		return chronology.chronology [number].sequence [actNumber].time;
	}

	public string WhatChronologicalAction (int number, int actNumber)
	{
		return chronology.chronology [number].sequence [actNumber].action;
	}

 	/*public void MakeChronologyLists(int number)
	{
		times.Clear();
		velocities.Clear();
		actions.Clear ();
		for (int i=0;i<chronology.chronology [number].sequence.Count;i++)
		{
			times.Add (WhatChronologicalTime (number,i));
			velocities.Add (WhatChronologicalLocation (number,i));
			actions.Add (WhatChronologicalAction (number,i));
		}
	}*/

	public bool CompareTimer(int number, int actNumber)//Функция проверки, не настало ли время для перехода к новому записанному событию
	{

		if (actNumber >= chronology.chronology [number].sequence.Count)
			return false;
		else 
			return (/*(Mathf.Abs(timer-chronology.chronology[number].sequence[actNumber].time)<timeEps)||*/
				(timer>chronology.chronology[number].sequence[actNumber].time));
	}

	public string ChronoAction(int number, int actNumber)//Так дубль узнаёт, какое действие ему совершить
	{
		return chronology.chronology [number].sequence [actNumber].action;
	}

	void CreateDouble(int number)//Создание дубля
	{
		GameObject doubler;
		if (number!=chronology.chronology.Count)
		{
			doubler= Instantiate(character,
			                     new Vector3(appearances[number].location.x,
			            					 appearances[number].location.y,
			            					 character.transform.position.z), 
			                     character.transform.rotation) as GameObject;
		}
		else
		{
			doubler= Instantiate(character,
			                     anchors[defaultAnchNumber].transform.position, 
			                     character.transform.rotation) as GameObject;

		}
		CharacterController charControl = doubler.GetComponent<CharacterController> ();
		charControl.SetNumber(number);
		charControl.SetActNumber(0);
		if (number == chronology.chronology.Count) 
		{
			charControl.underControl = true;
			currentSequence = new TimeSequence ();
			currentSequence.AddEvent (new TimeEvent (timer, new Vector2 (anchors[defaultAnchNumber].transform.position.x,
			                                                             anchors[defaultAnchNumber].transform.position.y),
			                                         "Appear"+PlayerPrefs.GetInt("AnchNumber")));
			chronology.AddSequence (currentSequence);
			mainCharacter=doubler.transform;
			begin = false;
		}
		else 
		{
			charControl.underControl = false;
		}
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
		for (int i=chronology.chronology[number].sequence.Count-1; i>paradox.GetActNumber(); i--)
			chronology.chronology [number].sequence.RemoveAt (i);
		paradox.underControl = true;
		mainCharacter = paradox.transform;
		begin = false;
	}

	public void Return()//функция, которая будет вызываться кнопкой на экране
	{
		if (!begin)
			StartCoroutine(Restart (new Vector2(mainCharacter.position.x,mainCharacter.position.y)));
	}

	public void Pause()//Тоже вызывается кнопкой паузы
	{
		pause = !pause;

	}

	public Vector3 Locate(Vector3 prevPosition, int number, int actNumber)
	{
		return new Vector3 (chronology.chronology [number].sequence [actNumber].location.x,
		                   chronology.chronology [number].sequence [actNumber].location.y,
		                   prevPosition.z);
	}

	IEnumerator Restart(Vector2 location)//Отправиться в прошлое
	{
		TimeEvent tEvent = new TimeEvent(timer, location,"Return");
		SetChronology (chronology.chronology.Count-1, tEvent);
		Serializator.SaveXml(chronology, datapath); 
		yield return new WaitForSeconds (0.5f);
		Application.LoadLevel (Application.loadedLevel);
	}

	//Позволяет проверить, появляются ли дубли в нужном времени и нужном месте
	void AppearancesPlaces()
	{
		places.Clear();
		for (int i=0;i<appearances.Count;i++)
		{
			places.Add(appearances[i].location);
		}
	}
}
