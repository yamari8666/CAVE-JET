using UnityEngine;
using System.Collections;

public class ItemController : MonoBehaviour {
	float itmSpd;
	GameObject sm;

	// Use this for initialization
	void Start () {
		sm = GameObject.Find("gamemanager");
	}
	
	// Update is called once per frame
	void Update () {
		itmSpd = sm.GetComponent<StageManager> ().stgSpd;
		this.transform.Translate (new Vector3 (0, 0, itmSpd * Time.deltaTime * -1), Space.World);
		if (this.transform.position.z <= -70)
		{
			Destroy(this.gameObject);
		}
	}
}
