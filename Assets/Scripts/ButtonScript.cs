using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonScript : MonoBehaviour {


	private Collider2D col;
	private List<GameObject> whoPushesMe=new List<GameObject>();//Список объектов, стоящих на кнопке

	public GameObject door;

	void Awake () 
	{
		col = gameObject.GetComponent<Collider2D> ();
		door.GetComponent<Collider2D>().enabled = true;
	
	}

	void Update () 
	{
		for (int i=0; i<whoPushesMe.Count; i++)
			if (whoPushesMe [i] == null)
				whoPushesMe.RemoveAt (i);
		if (whoPushesMe.Count>0)
			door.GetComponent<Collider2D>().enabled = false;
		else
			door.GetComponent<Collider2D>().enabled = true;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		bool unique=true;
		if (other.gameObject.tag == Tags.character)
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
		if (other.gameObject.tag == Tags.character)
		{
			whoPushesMe.Remove(other.gameObject);
		}
	}

}
