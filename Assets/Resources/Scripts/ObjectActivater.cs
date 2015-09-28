using UnityEngine;
using System.Collections;

public class ObjectActivater : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void activater()
	{
		this.gameObject.SetActive (false);
	}
}
