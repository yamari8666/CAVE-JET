using UnityEngine;
using System.Collections;

public class Destroyer : MonoBehaviour {
	public float dtimer;
	// Use this for initialization
	void Start () {
		StartCoroutine ("SelfDestroyer",dtimer);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public IEnumerator SelfDestroyer(float dt)
	{
		yield return new WaitForSeconds(dt);
		Destroy(this.gameObject);
	}
}
