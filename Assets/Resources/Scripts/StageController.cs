using UnityEngine;
using System.Collections;

public class StageController : MonoBehaviour {
	float sSpd;
	StageManager sm;


	// Use this for initialization
	void Start () 
	{
		sm = GameObject.Find ("gamemanager").GetComponent<StageManager>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		sSpd = sm.stgSpd * Time.deltaTime;
		this.transform.Translate (new Vector3(0,0,sSpd * -1));
		if(this.transform.position.z <= sm.bunPos)
		{
			sm.StageReturn(this.gameObject);
		}
	}
}
