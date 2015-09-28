using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour {
	Canvas canvas;
	// Use this for initialization
	void Start () {
		canvas = this.GetComponent<Canvas> ();
	
	}
	public void UIactivater(string name,bool b)
	{
		foreach (Transform child in canvas.transform) 
		{
			if(child.name == name)
			{
				child.gameObject.SetActive(b);
				return;
			}
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
