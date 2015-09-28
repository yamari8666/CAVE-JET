using UnityEngine;
using System.Collections;

public class RockGenerator : MonoBehaviour {
	GameObject[] rock = new GameObject[4];
	GameObject itmBoost;
	GameObject itmBarrier;
	Vector3 genePos;
	float geneAreaX;
	float geneAreaY;
	int cur = 0;
	int count;
	int cutItmBo = 0;
	int cutItmBa = 0;
	int itemBoostCount;
	int itemBarrierCount;
	GameObject gm;
	GameManager gmgm;

	// Use this for initialization
	void Start () {
		gm = GameObject.Find ("gamemanager");
		gmgm = gm.GetComponent<GameManager> ();
		count = Random.Range(20,60);
		itemBoostCount = Random.Range(500,1500);
		itemBarrierCount = Random.Range(100,1000);
		for (int rc = 0; rc <= 3; rc++) 
		{
			rock[rc] = Instantiate(Resources.Load("Models/rock/rock" + (rc + 1) + "/rock"),new Vector3(45 + (rc*2),0,0),Quaternion.identity)as GameObject;
		}
		itmBoost = Instantiate (Resources.Load ("Models/item/boost/item"), new Vector3 (50f, 0f, 5f), Quaternion.identity) as GameObject;
		itmBarrier = Instantiate (Resources.Load ("Models/item/barrier/item"), new Vector3 (40f, 0f, 5f), Quaternion.identity) as GameObject;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (gmgm.pSwitch == true) 
		{
			RockGenerate();
			itemBoostGenerate ();
			itemBarrierGenerate();
		}
	}

	void RockGenerate()
	{
		if (cur < count) {
			cur ++;
		} else
			if (cur >= count) 
		{
			RockBirth();
			count = Random.Range(20,80);
			cur = 0;
		}
	}
	void itemBoostGenerate()
	{
		if (cutItmBo < itemBoostCount) {
			cutItmBo ++;
		} else
			if (cutItmBo >= itemBoostCount) 
		{
			ItemBoostBirth();
			itemBoostCount = Random.Range(500,1500);
			cutItmBo = 0;
		}
	}
	void itemBarrierGenerate()
	{
		if (cutItmBa < itemBarrierCount) {
			cutItmBa ++;
		} else
			if (cutItmBa >= itemBarrierCount) 
		{
			ItemBarrierBirth();
			itemBarrierCount = Random.Range(100,1000);
			cutItmBa = 0;
		}
	}

	void RockBirth()
	{
		geneAreaX = Random.Range (-1, 1);
		geneAreaY = Random.Range (-12, 12);
		genePos = new Vector3 (geneAreaX, geneAreaY, this.transform.position.z);
		int rockCount = Random.Range (0, 4);
		GameObject rClone = Instantiate(rock[rockCount],genePos,Quaternion.identity) as GameObject;
		float scaleNum = Random.Range (0.8f, 2.5f);
		rClone.transform.localScale = new Vector3 (scaleNum, scaleNum, scaleNum);
		rClone.AddComponent<RockController> ();
	}
	void ItemBoostBirth()
	{
		geneAreaX = Random.Range (-1, 1);
		geneAreaY = Random.Range (-12, 12);
		genePos = new Vector3 (geneAreaX, geneAreaY, this.transform.position.z);
		GameObject iClone = Instantiate (itmBoost, genePos, Quaternion.identity) as GameObject;
		iClone.AddComponent<ItemController> ();
	}
	void ItemBarrierBirth()
	{
		geneAreaX = Random.Range (-1, 1);
		geneAreaY = Random.Range (-12, 12);
		genePos = new Vector3 (geneAreaX, geneAreaY, this.transform.position.z);
		GameObject iClone = Instantiate (itmBarrier, genePos, Quaternion.identity) as GameObject;
		iClone.AddComponent<ItemController> ();
	}
}
