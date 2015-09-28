using UnityEngine;
using System.Collections;

public class StageManager : MonoBehaviour {
	public float stgSpd;
	public float bunPos;
	public float resPos;

	GameObject[] stg = new GameObject[7];
	GameObject stage;
	float stageDepth;

	[SerializeField]
	GameObject efSonicWind;

	public bool boostSwitch = false;
	public bool invSwitch = false;
	public float invtime = 0;

	void Start () {
		Vector3 stgPos = new Vector3(0,0,-30);
		for (int i = 0; i <= stg.Length-1; i++)
		{
			stg[i] = Instantiate (Resources.Load("Models/stage/stage"), stgPos, Quaternion.identity) as GameObject;
			if(i == 0)
			{
				stageDepth = stg[i].GetComponent<Renderer>().bounds.size.z;
				bunPos = stg[i].transform.position.z + (stageDepth * -1);
				resPos = stg[i].transform.position.z + (stageDepth * 6);
			}
			stgPos.z = stg[i].transform.position.z + stageDepth;
		}
	}
	
	// Update is called once per frame
	void Update () {

	}
	public IEnumerator SpeedUp(float spdMagni,float upTime,int scMagni)
	{
		boostSwitch = true;
		invSwitch = true;
		float baseSpd = stgSpd;
		stgSpd = stgSpd * spdMagni;
		int scP = this.GetComponent<GameManager> ().scoreP;
		scP = scP * scMagni;
		PlayerController pl = GameObject.Find ("player").GetComponent<PlayerController>();
		pl.BoostEffectSwitch(true,false);
		efSonicWind.SetActive (true);
		invtime = upTime;
		yield return new WaitForSeconds (invtime);
		stgSpd = baseSpd;
		this.GetComponent<GameManager> ().scoreP = 1;
		pl.BoostEffectSwitch (false,true);
		efSonicWind.SetActive (false);
		boostSwitch = false;
		if (pl.brSwitch == false) 
		{
			invSwitch = false;
		}
	}
	public void StageReturn(GameObject s)
	{
		resPos = s.transform.position.z + (stageDepth * 7);
		s.transform.position = new Vector3 (0f, 0f, resPos);
	}
}
