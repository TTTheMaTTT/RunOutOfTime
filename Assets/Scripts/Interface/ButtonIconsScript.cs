using UnityEngine;
using System.Collections;

public class ButtonIconsScript : MonoBehaviour {

	public GameObject button;
	public int pause;

	void Start () {
	
	}

	void Update () 
	{
		transform.position =new Vector3(button.transform.position.x,
		                                button.transform.position.y,
		                                transform.position.z);
		//Некоторые кнопки доступны только во время паузы. Если pause=-1, то всегда активно, если 0, то только при паузе
		//Если 1, то при активной игре.
		if (pause >= 0)
			gameObject.GetComponent<SpriteRenderer> ().enabled = ((Time.timeScale == 0f) == (pause == 0));
	}
}
