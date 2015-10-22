using UnityEngine;
using System.Collections;

public class StarScript : MonoBehaviour {

	private Collider2D col;

	void Start () 
	{
		col = gameObject.GetComponent<Collider2D> ();
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == Tags.character)
			Debug.Log ("Level Complete!");
	}

}
