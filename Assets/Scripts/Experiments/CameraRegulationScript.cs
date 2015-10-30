using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraRegulationScript : MonoBehaviour {
	
	private int mode;
	private Camera cam;
	public GameObject corner;//левый нижний угол камеры
	private SpriteRenderer sprite;//спрайт рендерер этого улла

	private float strength=0.2f;//как сильно можно менять параметры камеры при одном нажатии
	private float nu;//nu - отношение боковых сторон камеры, которое мы хотим получить 		
	public float offsetX=3f, offsetY=2f;
	private bool buttonsLocated = false;

	public GameObject button1, button2;

	private enum mods{x,y,size,width,height,x1,y1};
	private bool vis;

	void Start () {
		buttonsLocated = false;
		Vector2 vect;
		float k;
		vis = true;
		cam = gameObject.GetComponent<Camera> ();
		sprite=corner.GetComponent<SpriteRenderer>();
		vect=new Vector2(cam.transform.position.x-corner.transform.position.x,
		                 cam.transform.position.y-corner.transform.position.y);
		if (PlayerPrefs.HasKey ("nu")) 
		{
			float k1;
			k1=PlayerPrefs.GetFloat("nu");
			if (k1==0f)
				PlayerPrefs.SetFloat("nu",vect.y / vect.x);
		}
		if (!PlayerPrefs.HasKey ("nu"))
		{
			nu = vect.y / vect.x;
			PlayerPrefs.SetFloat("nu", nu);
			cam.orthographicSize = 1f;
			vis = false;
		}
		else nu=PlayerPrefs.GetFloat("nu");
		if (11f/ 5f * nu > 1f)
		{
			k=5f/22f/nu;
			cam.rect = new Rect (0.5f - k, 0f, 2f*k, 1f);
		}
		else if (11f / 5f * nu < 1f)
		{
			k=11f/10f*nu;
			cam.rect = new Rect (0f, 0.5f-k, 1f, 2f*k);
		}//Нужно поставить объект "Corner" в нужное место и тогда камера будет иметь левый нижний угол на месте "Corner" 
		if (PlayerPrefs.HasKey("CameraSize"))
		{
			cam.orthographicSize=PlayerPrefs.GetFloat("CameraSize");
			LocateButtons ();
			buttonsLocated=false;
		}
	}

	void Update () 
	{
		/*
		float f1=0f, f2=0f;
		f1 = Input.GetAxis ("Horizontal");
		if (Input.GetButtonDown("Vertical"))
		{
			f2=Input.GetAxis ("Vertical");
		}
		if (f2 != 0)
			SwitchMode (f2);
		if (f1 != 0)
			ChangeValue (f1);
			*/
		if (!vis)
		{	
			cam.orthographicSize+=1f;
			vis = (sprite.isVisible);
		}
		if (vis) 
		{
			if (!buttonsLocated)
			{
				LocateButtons ();
				buttonsLocated=true;
			}
			sprite.enabled = false;
			PlayerPrefs.SetFloat("CameraSize",cam.orthographicSize);
			PlayerPrefs.SetFloat("nu", nu);
		}

	}

	void LocateButtons()
	{
		float width = cam.orthographicSize*2.2f*cam.rect.width;
		float height = cam.orthographicSize*cam.rect.height;
		button1.transform.position = new Vector3 (cam.transform.position.x - width+offsetX,
		                                       	  cam.transform.position.y - height+offsetY,
		                                       	  button1.transform.position.z);
		button2.transform.position = new Vector3 (cam.transform.position.x + width-offsetX,
		                                          cam.transform.position.y - height+offsetY,
		                                          button2.transform.position.z);

	}

	void SwitchMode(float f)//Смена режима ввода данных
	{
		if (f > 0) 
		{
			mode++;
			if (mode>6)
				mode=0;
		}
		if (f<0)
		{
			mode--;
			if (mode<0)
				mode=6;
		}
	}

/*	void ChangeValue(float f)//Функции считывания данных камеры
	{
		f *= strength;
		switch (mode)
		{
		case ((int)mods.x):
			cam.transform.position=new Vector3(cam.transform.position.x+f,
			                                   cam.transform.position.y,
			                                   cam.transform.position.z);
			text1.text="PositionX";
			text2.text=cam.transform.position.x.ToString();
			break;
		case ((int)mods.y):
			cam.transform.position=new Vector3(cam.transform.position.x,
			                                   cam.transform.position.y+f,
			                                   cam.transform.position.z);
			text1.text="PositionY";
			text2.text=cam.transform.position.y.ToString();
			break;
		case ((int)mods.size):
			cam.orthographicSize=cam.orthographicSize+f;
			text1.text="Size";
			text2.text=cam.orthographicSize.ToString();
			break;	
		case ((int)mods.width):
			cam.rect=new Rect(cam.rect.x,cam.rect.y,cam.rect.width+f,cam.rect.height);
			text1.text="Width";
			text2.text=cam.rect.width.ToString();
			break;
		case ((int)mods.height):
			cam.rect=new Rect(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height+f);
			text1.text="Height";
			text2.text=cam.rect.height.ToString();
			break;
		case ((int)mods.x1):
			cam.transform.localScale=new Vector3(cam.transform.localScale.x+f,
			                                     cam.transform.localScale.y,
			                                     cam.transform.localScale.z);
			text1.text="InterfaceX";
			text2.text=cam.transform.localScale.x.ToString();
			break;
		case ((int)mods.y1):
			cam.transform.localScale=new Vector3(cam.transform.localScale.x,
			                                     cam.transform.localScale.y+f,
			                                     cam.transform.localScale.z);
			text1.text="InterfaceY";
			text2.text=cam.transform.localScale.y.ToString();
			break;
		default:
			break;
		}
	}*/

}
