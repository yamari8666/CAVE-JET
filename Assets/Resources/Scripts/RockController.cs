using UnityEngine;
using System.Collections;

public class RockController : MonoBehaviour {
	float rockSpd;
	Vector3 myRot;
	GameObject sm;

	// Use this for initialization
	void Start () {
		sm = GameObject.Find("gamemanager");
		myRot = new Vector3 (Random.Range (-2f, 2f), Random.Range (-2f, 2f), Random.Range (-2f, 2f));
	}
	
	// Update is called once per frame
	void Update () {
		rockSpd = sm.GetComponent<StageManager> ().stgSpd;
		this.transform.Translate(new Vector3(0,0,rockSpd * Time.deltaTime * -1),Space.World);
		if (this.transform.position.z <= -70)
		{
			Destroy(this.gameObject);
		}
		this.transform.Rotate(myRot);
	}
}
