using UnityEngine;
using System.Collections;
using GAF.Core;

public class AnchorScript : MonoBehaviour {

	private Collider2D col;
	private CharacterController hero;	
	private float activationTime=0.5f;

	public int number;
	private GAFMovieClip platform, mainPart;
	private bool active;

	void Awake () 
	{
		active = false;
		col = gameObject.GetComponent<Collider2D> ();
		platform = transform.FindChild ("Portal").GetComponent<GAFMovieClip> ();
		mainPart = transform.FindChild ("PortalMainPart").GetComponent<GAFMovieClip> ();
	}
	
	void Update () 
	{
		if ((PlayerPrefs.HasKey ("Anch0Active") && (number == 0)) ||
			(PlayerPrefs.HasKey ("Anch1Active") && (number == 1)))
			active = true;
		if (!active)
		{
			platform.setSequence("Unactivated",true);
			mainPart.setSequence("Nothing",true);
		}
		if (active)
		{
			platform.setSequence("Activated",true);
			if (activationTime>0)
				activationTime-=Time.deltaTime;
			if (activationTime>0)
			{
				mainPart.setAnimationWrapMode(GAF.Core.GAFWrapMode.Once);
				if (number==PlayerPrefs.GetInt ("AnchNumber"))
					mainPart.setSequence("ActivateNum1",true);
				else 
					mainPart.setSequence("ActivateNum2",true);
			}
			else
			{
				mainPart.setAnimationWrapMode(GAF.Core.GAFWrapMode.Loop);
				if (number==PlayerPrefs.GetInt ("AnchNumber"))
					mainPart.setSequence("LevitateNum1",true);
				else 
					mainPart.setSequence("LevitateNum2",true);
			}
		}
	}
	
	void OnTriggerStay2D(Collider2D other)
	{
		if (other.gameObject.tag == Tags.character)
		{
			hero=other.gameObject.GetComponent<CharacterController>();
			PlayerPrefs.SetInt ("AnchNumber",number);
		}		

		if (number==0)
			PlayerPrefs.SetInt("Anch0Active",1);
		else
			PlayerPrefs.SetInt("Anch1Active",1);
	}

	public void SetNumber(int _number)
	{
		number = _number;
	}
}
