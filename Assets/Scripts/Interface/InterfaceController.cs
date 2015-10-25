using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InterfaceController : MonoBehaviour {
	
	private List<GameObject> buttons=new List<GameObject>();
	

	void Start () 
	{
		GameObject[] objects=GameObject.FindGameObjectsWithTag("Buttons");
		for (int i=0; i<objects.Length; i++)
			buttons.Add (objects [i]);	
	}
	

	void Update () {
	
	}

	public string CheckButtons()
	{
		Touch touch = Input.GetTouch (0);
		for (int i=0;i<buttons.Count;i++)
		{
			Vector2 vect=new Vector2(buttons[i].transform.position.x,buttons[i].transform.position.y);
			RectTransform size = buttons[i].GetComponent<RectTransform>();
			if ((touch.position.x<vect.x+size.sizeDelta.x)&&
			    (touch.position.x>vect.x-size.sizeDelta.x)&&
			    (touch.position.y>vect.y-size.sizeDelta.y)&&
			    (touch.position.y<vect.y+size.sizeDelta.y))
				return (buttons[i].name);
		}
		return ("Nothing");
	}
}
