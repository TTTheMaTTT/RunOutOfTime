using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DirectionChanger : MonoBehaviour {

	private Collider2D col;
	private CharacterController hero;


	
	void Awake () 
	{
		col = gameObject.GetComponent<Collider2D> ();
		
	}
	
	void Update () 
	{
	}
	
	void OnTriggerStay2D(Collider2D other)
	{
		if ((other.gameObject.tag == Tags.character)&&(hero==null))
		{
			if (other.gameObject.GetComponent<CharacterController>().underControl)
			{
				hero=other.gameObject.GetComponent<CharacterController>();
				hero.ChangeDirection();
				hero.WriteChronology("ChangeDirection");
			}
		}		
		
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if ((other.gameObject.tag == Tags.character)&&(hero!=null))
		{
			hero=null;
		}
	}

}
