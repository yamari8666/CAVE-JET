using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	Rigidbody pRg;
	float powUp = 50.0f;
	[SerializeField]
	GameObject parOver;
	[SerializeField]
	GameObject parForward;
	[SerializeField]
	GameObject parBoost;
	[SerializeField]
	GameObject parBarrier;

	GameObject gm;
	GameManager gmgm;
	StageManager gmsm;
	GameObject efCrashOb;
	public bool brSwitch = false;

	// Use this for initialization
	void Start () 
	{
		pRg = this.GetComponent<Rigidbody> ();
		parOver.SetActive (false);
		parForward.SetActive (true);
		gm = GameObject.Find ("gamemanager");
		gmgm = gm.GetComponent<GameManager> ();
		gmsm = gm.GetComponent<StageManager> ();
		efCrashOb = Instantiate(Resources.Load("Prefabs/ef_crashObstacle"),new Vector3(45,0,5),Quaternion.identity) as GameObject;


	}
	
	// Update is called once per frame
	void Update () 
	{
		if (gmgm.pSwitch == false) 
		{
			pRg.isKinematic = true;
		}else
		if (gmgm.pSwitch == true) {
			pRg.isKinematic = false;
			if (Input.GetMouseButton (0)) {
				pRg.AddForce (0, powUp * Time.deltaTime, 0, ForceMode.Impulse);
				parOver.SetActive (true);
			}
			if (Input.GetMouseButtonUp (0)) {
				parOver.SetActive (false);
			}
		}
	}
	void OnCollisionEnter(Collision col)
	{
		if (gmsm.invSwitch == false) {
			gmgm.pSwitch = false;
			Vector3 myPos = this.transform.position;
			Instantiate (Resources.Load ("Prefabs/ef_explo"), myPos, Quaternion.identity);
			this.gameObject.SetActive (false);
			gmsm.stgSpd = gmsm.stgSpd /2f;
			gmgm.GameOver();
		} else if(gmsm.invSwitch == true && col.gameObject.tag == "obstacle")
		{
			GameObject c = Instantiate(efCrashOb,col.transform.position,Quaternion.identity) as GameObject;
			c.AddComponent<Destroyer>();
			c.GetComponent<Destroyer>().dtimer = 2f;
			Destroy (col.gameObject);
		}
	}
	public IEnumerator Barrier()
	{
		brSwitch = true;
		gmsm.invSwitch = true;
		parBarrier.SetActive (true);
		gmsm.invtime = 4f;
		yield return new WaitForSeconds (gmsm.invtime);
		parBarrier.SetActive (false);
		brSwitch = false;
		if (gmsm.boostSwitch == false) 
		{
			gmsm.invSwitch = false;
		}
	}

	void OnTriggerEnter(Collider cor)
	{
		if (cor.gameObject.tag == "Item_boost") {
			IEnumerator spu = gmsm.SpeedUp (2.0f, 4.0f, 2);
			gmsm.StartCoroutine (spu);
		} else
		if (cor.gameObject.tag == "Item_barrier") 
		{
			StartCoroutine("Barrier");
		}
		Destroy (cor.gameObject);
	}

	public void BoostEffectSwitch(bool bl,bool bf)
	{
		parBoost.SetActive (bl);
		parForward.SetActive (bf);
	}
}
