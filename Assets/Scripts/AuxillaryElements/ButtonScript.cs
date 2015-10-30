using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonScript : MonoBehaviour {


	private Collider2D col;
	private Collider2D doorCol;
	private SpriteRenderer doorSprite;
	private GameObject active, unactive; // Две анимации активной и неактивной кнопки
	private List<GameObject> whoPushesMe=new List<GameObject>();//Список объектов, стоящих на кнопке

	public GameObject door;

	void Awake () 
	{
		col = gameObject.GetComponent<Collider2D> ();
		doorCol = door.GetComponent<Collider2D> ();
		doorSprite = door.GetComponent<SpriteRenderer> ();
		doorCol.enabled = true;
		active=transform.FindChild("Button_Red_1").gameObject;
		unactive=transform.FindChild("Button_Green").gameObject;
	}

	void Update () 
	{
		for (int i=0; i<whoPushesMe.Count; i++)
			if (whoPushesMe [i] == null)
				whoPushesMe.RemoveAt (i);
		if (whoPushesMe.Count>0)
		{
			doorCol.enabled = false;
			active.SetActive(false);
			unactive.SetActive(true);
		}
		else
		{
			doorCol.enabled = true;
			active.SetActive(true);
			unactive.SetActive(false);
		}
	}

	void OnTriggerStay2D(Collider2D other)
	{
		bool unique=true;
		if ((other.gameObject.tag == Tags.character)||(other.gameObject.tag == "Box"))
		{
			for (int i=0;i<whoPushesMe.Count;i++)
				if (other.gameObject==whoPushesMe[i])
					unique=false;
			if (unique)
				whoPushesMe.Add (other.gameObject);
		}		

	}

	void OnTriggerExit2D(Collider2D other)
	{
		if ((other.gameObject.tag == Tags.character)||(other.gameObject.tag == "Box"))
		{
			whoPushesMe.Remove(other.gameObject);
		}
	}

}
