using UnityEngine;
using System.Collections;
using GAF.Core;

public class DoorScript : MonoBehaviour {

	private Collider2D col;
	private GAFMovieClip mov;

	void Start () 
	{
		col = GetComponent<Collider2D> ();
		mov = GetComponentInChildren<GAFMovieClip> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (col.enabled == false)
			mov.setSequence ("DoorOpen", true);
		if (col.enabled == true)
			mov.setSequence ("DoorClose", true);
	}
}
