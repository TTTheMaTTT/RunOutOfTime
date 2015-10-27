using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextController : MonoBehaviour {

	public Text text;

	private int number;
	// Use this for initialization
	void Start () {
		number = 0;
		text.text=number.ToString();
	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetButtonDown ("Jump"))
			number++;
		text.text=number.ToString();
	}
}
