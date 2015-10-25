using UnityEngine;
using System.Collections;

public class AnchorScript : MonoBehaviour {

	private Collider2D col;
	private CharacterController hero;	

	public int number;

	void Awake () 
	{
		col = gameObject.GetComponent<Collider2D> ();
		
	}
	
	void Update () 
	{
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == Tags.character)
		{
			hero=other.gameObject.GetComponent<CharacterController>();
			PlayerPrefs.SetInt ("AnchNumber",number);
		}		
		
	}

	public void SetNumber(int _number)
	{
		number = _number;
	}
}
