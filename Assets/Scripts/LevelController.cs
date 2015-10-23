using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization; 
using System;
using System.IO; 

public class LevelController : MonoBehaviour {

	private float eps = 1f, checkEps=5f, timeEps=0.000001f;//TimeEps отвечает за то, насколько точно дубли будут следовать своей хронолгии
	private bool begin;

	public float refreshTime=1f;
	public float revisionTime=1f;

	public float timer;
	public GameObject[] anchors;
	public GameObject defaultAnchor;
	public GameObject character;

	public TimeChronology chronology;	// Какие действия совершали дубли?
	private string datapath;	// путь к файлу сохранения для этой локации
	private TimeSequence currentSequence;
	private List<TimeEvent> appearances= new List<TimeEvent>();//коллекция времён появления дублей
	private List<bool> whoHasAppeared=new List<bool>();//Кто уже появился из дублей?

	//Следующие переменные использовались при проверке правильности работы, если их удалить, прога смотжет нормально работать
	public int count;
	public Vector2 actualVelocity;
	public Vector2 chronologicalVelocity;
	//Хронологические списки
	public int doubleNumber;//Номер, для которого мы строим список
	public List<float> times=new List<float>();
	public List<Vector2> velocities=new List<Vector2>();
	public List<string> actions=new List<string>();

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
		count = chronology.chronology [0].sequence.Count;
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
			StartCoroutine(Restart ());
		}

		if ((begin)&&(Input.GetButtonDown("Jump")))//Здесь мы придём в прошлое
		{
			CreateDouble(chronology.chronology.Count);
		}

		if (Input.GetButtonDown("Vertical"))
		{
			MakeChronologyLists(doubleNumber);
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



	public bool CompareVelocityPrecisely(int number, int actNumber, Vector2 velocity)//Функция проверки, насколько скорость дубля отличается от его хронологичной
	{
		return (Vector2.Distance(chronology.chronology [number].sequence [actNumber].velocity, velocity) < eps);
	}

	public bool CompareVelocity(int number, int actNumber, Vector2 velocity)//Такая же функция, только менее точная
	{
		//if (Mathf.Abs (chronology.chronology [number].sequence [actNumber].velocity.x - velocity.x) < checkEps)
		//{
		//	Time.timeScale = 0f;
		//}
		return ((Mathf.Abs(chronology.chronology [number].sequence [actNumber].velocity.x- velocity.x) < checkEps)||
			(!String.Equals(chronology.chronology[number].sequence[actNumber].action,"ChangeSpeed")));
			
	}

	//Следующие четыре функции позволяют проверить, следует ли дубль своему каконическому поведению
	public Vector2 WhatChronologicalVelocity(int number, int actNumber)
	{
		return chronology.chronology [number].sequence [actNumber].velocity;
	}

	public float WhatChronologicalTime (int number, int actNumber)
	{
		return chronology.chronology [number].sequence [actNumber].time;
	}

	public string WhatChronologicalAction (int number, int actNumber)
	{
		return chronology.chronology [number].sequence [actNumber].action;
	}

 	public void MakeChronologyLists(int number)
	{
		times.Clear();
		velocities.Clear();
		actions.Clear ();
		for (int i=0;i<chronology.chronology [number].sequence.Count;i++)
		{
			times.Add (WhatChronologicalTime (number,i));
			velocities.Add (WhatChronologicalVelocity (number,i));
			actions.Add (WhatChronologicalAction (number,i));
		}
	}

	public bool CompareTimer(int number, int actNumber)//Функция проверки, не настало ли время для перехода к новому записанному событию
	{
		if (actNumber >= chronology.chronology [number].sequence.Count)
			return false;
		else 
			return ((Mathf.Abs(timer-chronology.chronology[number].sequence[actNumber].time)<timeEps)||
				(timer>chronology.chronology[number].sequence[actNumber].time));
	}

	public string ChronoAction(int number, int actNumber)//Так дубль узнаёт, какое действие ему совершить
	{
		return chronology.chronology [number].sequence [actNumber].action;
	}

	void CreateDouble(int number)//Создание дубля
	{
		GameObject doubler= Instantiate(character,defaultAnchor.transform.position, character.transform.rotation) as GameObject;
		CharacterController charControl = doubler.GetComponent<CharacterController> ();
		charControl.SetNumber(number);
		charControl.SetActNumber(0);
		if (number == chronology.chronology.Count) 
		{
			charControl.underControl = true;
			currentSequence = new TimeSequence ();
			currentSequence.AddEvent (new TimeEvent (timer, new Vector2 (0f, 0f), "Appear"));
			chronology.AddSequence (currentSequence);
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
		begin = false;
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
