using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization; 
using System;
using System.IO; 

public class LevelController : MonoBehaviour {
	
	private float eps = 1f, checkEps=2f;
	private float timeEps=0.0001f;//TimeEps отвечает за то, насколько точно дубли будут следовать своей хронолгии
	private float timeRecoil=3f;//Эта переменная отвечает за то, насколько раньше самого раннего времени сможет появиться новый дубль	
	private bool begin, pause;

	public float refreshTime=1f;
	public float revisionTime=2f;
		
	public float timer;
	public GameObject[] anchors;
	public Text timeText;
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
	public int anchNumb;

	public Text numberText;//Вспомогательные элементы, помогающие определиться
	private Text ttext;//Как правильно работать с текстом
	private int doubleToChange;//дубль, начиная с которого переписывается история прохождения уровня
	private GameObject interfacer, settings;//пользовательский интерфейс во время игры и  настройки
	private GameObject activeWindow;

	//Хронологические списки
	/*public int doubleNumber;//Номер, для которого мы строим список
	public List<float> times=new List<float>();
	public List<Vector2> velocities=new List<Vector2>();
	public List<string> actions=new List<string>();*/

	public List<Vector2> places=new List<Vector2>();

	void Start () 
	{
		//инициализируем элементы интерфейса
		GameObject[] interfaces = GameObject.FindGameObjectsWithTag ("Interface");
		for (int i=0;i<interfaces.Length;i++)
		{
			if (string.Equals(interfaces[i].name,"Interface"))
				interfacer=interfaces[i];
			else if (string.Equals(interfaces[i].name,"Settings"))
				settings=interfaces[i];
		}
		settings.SetActive (false);
		interfacer.SetActive (true);
		activeWindow = interfacer;
		doubleToChange = 0;//Если игрок откроет Settings, то ему по дефолту предложится начать уровень заново с нулевого уровня

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
		datapath = (Application.platform == RuntimePlatform.Android? Application.dataPath: Application.persistentDataPath) + "SavedData" + Application.loadedLevelName + ".xml";

		if (PlayerPrefs.GetInt("NewLevel")==1)//Чтоб наверняка начать новый уровень сызнова-заново!!!
		{
			PlayerPrefs.SetInt("NewLevel",0);
			File.Delete(datapath);
			DeletePrefs();
			Application.LoadLevel (Application.loadedLevel);
			
		}

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
		timeText.text = (Mathf.Round (timer*100f)/100f).ToString();
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
		{
			Time.timeScale = 0f;
			PauseSript();
		}
		else
		{
			Time.timeScale = 1f;
			GameObject[] texts = GameObject.FindGameObjectsWithTag ("NumberText");
			for (int i=0;i<texts.Length;i++) Destroy(texts[i]);
		}
		if (begin)//Здесь мы придём в прошлое
		{
			bool appear=false;
			if ((andr))
			{
				if (Input.touchCount==1)
				{
					Touch touch=Input.GetTouch(0);
					if (touch.phase==TouchPhase.Began)
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
			DeletePrefs();
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
		{
			if (doubles [i].GetComponent<CharacterController> ().GetNumber () > number) 
			{
				Destroy (doubles [i]);
				//doubles [i].GetComponent<CharacterController> ().SetReturning();
			}
		}
		for (int i=chronology.chronology.Count-1; i>number; i--)
		{
			if (i<appearances.Count)
				appearances.RemoveAt(i);
			chronology.chronology.RemoveAt(i);

		}
		PlayerPrefs.SetFloat ("beginTime", 0f);
		for (int i=0;i<appearances.Count;i++)
		{
			if (appearances[i].time<PlayerPrefs.GetFloat("beginTime"))
				PlayerPrefs.SetFloat("beginTime", appearances[i].time);
		}
		for (int i=chronology.chronology[number].sequence.Count-1; i>paradox.GetActNumber(); i--)
			chronology.chronology [number].sequence.RemoveAt (i);
		paradox.underControl = true;
		mainCharacter = paradox.transform;
		begin = false;
	}

	//Функции, навешиваемые на кнопки
	public void Return()//функция, которая будет вызываться кнопкой на экране
	{
		if ((!begin) && (!pause))
			StartCoroutine(Restart (new Vector2(mainCharacter.position.x,mainCharacter.position.y)));

		if (pause) 
		{
			activeWindow.SetActive(false);
			activeWindow=settings;
			activeWindow.SetActive(true);
		}
	}

	//Выбираем, с какого дубля мы хотим переиграть уровень
	public void ChangeRestartNumber()
	{
		doubleToChange++;
		if (doubleToChange >= chronology.chronology.Count)
			doubleToChange = 0;
		Text text = settings.transform.FindChild ("ChangeClone").transform.FindChild ("Text").GetComponent<Text> ();
		text.text = doubleToChange.ToString ();
	}

	public void ChangeHistory ()//Начинаем уровень заново с дубля под номером doubleToChange
	{
		for (int i=chronology.chronology.Count-1; i>=doubleToChange; i--)
		{
			if (i<appearances.Count)
				appearances.RemoveAt(i);
			chronology.chronology.RemoveAt(i);	
		}
		PlayerPrefs.SetFloat ("beginTime", 0f);
		for (int i=0;i<appearances.Count;i++)
		{
			if (appearances[i].time<PlayerPrefs.GetFloat("beginTime"))
				PlayerPrefs.SetFloat("beginTime", appearances[i].time);
		}
		PlayerPrefs.SetFloat("beginTime",PlayerPrefs.GetFloat("beginTime")+timeRecoil);
		begin = false;
		Serializator.SaveXml(chronology, datapath); 
		Application.LoadLevel (Application.loadedLevel);
	}

	//Перемещаемся между окнами интерфейса
	public void BackToMenu(string name)
	{
		activeWindow.SetActive (false);
		if (string.Equals (name, "Interface"))
			activeWindow = interfacer;
		activeWindow.SetActive (true);
	}
	
	public void Pause()//Тоже вызывается кнопкой паузы
	{
		pause = !pause;
	}

	//Если нажимаете на экран, то активный персонаж либо появляется, либо прыгает
	public void MainAction()
	{
		if (begin)
		{
			if (timer<PlayerPrefs.GetFloat("beginTime"))
				PlayerPrefs.SetFloat("beginTime",timer);
			CreateDouble(chronology.chronology.Count);
		}
		else 
		{
			mainCharacter.gameObject.GetComponent<CharacterController>().SetJumping();
		}

	}

	//Функция, которая поставит данный объект на то место, где он должен находиться в данное время, следуя своей хронологии
	public Vector3 Locate(Vector3 prevPosition, int number, int actNumber)
	{
		return new Vector3 (chronology.chronology [number].sequence [actNumber].location.x,
		                   chronology.chronology [number].sequence [actNumber].location.y,
		                   prevPosition.z);
	}

	//Так игрок отправляется в прошлое 
	IEnumerator Restart(Vector2 location)//Отправиться в прошлое
	{
		TimeEvent tEvent = new TimeEvent(timer, location,"Return");
		SetChronology (chronology.chronology.Count-1, tEvent);
		Serializator.SaveXml(chronology, datapath);
		mainCharacter.gameObject.GetComponent<CharacterController> ().SetReturning ();
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

	void DeletePrefs()//при переходе на следующий уровень некоторые данные должны быть удалены
	{
		PlayerPrefs.SetInt("AnchNumber",0);
		PlayerPrefs.DeleteKey("CameraSize");
		PlayerPrefs.SetFloat("nu",0f);
		PlayerPrefs.DeleteKey ("beginTime");
		PlayerPrefs.DeleteKey("Anch0Active");
		PlayerPrefs.DeleteKey("Anch1Active");
	}
	
	void PauseSript()//Скрипт, который определяет, что происходит при паузе
	{
		GameObject[] doubles = GameObject.FindGameObjectsWithTag (Tags.character);
		for (int i=0; i<doubles.Length; i++)
		{
			ttext=Instantiate(numberText,doubles[i].transform.position,doubles[i].transform.rotation) as Text;
			ttext.transform.localScale=interfacer.transform.localScale;
			ttext.transform.SetParent(interfacer.transform);
			int numb=doubles[i].GetComponent<CharacterController>().GetNumber();
			ttext.text=numb.ToString();
			ttext.name="Number "+numb.ToString();
			ttext.color=Color.white;
			ttext.transform.position=new Vector3(ttext.transform.position.x+2f,
			                                     ttext.transform.position.y+2f,
			                                     ttext.transform.position.z);
		}
	}

	//Когда скрипт достигнет 500 строк его следует разбить на мелкие кусочки кода
}
