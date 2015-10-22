using UnityEngine;
using System.Collections;

public class ButtonScript : MonoBehaviour {


	private Collider2D col;

	public GameObject door;

	void Awake () 
	{
		col = gameObject.GetComponent<Collider2D> ();
	
	}

	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == Tags.character)
		{
			door.GetComponent<Collider2D>().enabled = false;
		}		

	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.tag == Tags.character)
		{
			door.GetComponent<Collider2D>().enabled = true;
		}
	}

}
