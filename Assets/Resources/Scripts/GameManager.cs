using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
	public bool pSwitch = false;
	public int score;
	public int scoreP = 1;
	public int bestScore = 0;
	public GameObject canvas;
	int[] rankscore = new int[5]{300,200,100,50,10};
	public Text[] rankscoretext = new Text[5];
	[SerializeField]
	Animator cAnim;
	[SerializeField]
	Text scoreText;
	[SerializeField]
	Text bestScoreText;
	[SerializeField]
	Text scoreBotderText;
	[SerializeField]
	StageManager stagemanager;
	[SerializeField]
	GameObject gameresult;
	[SerializeField]
	Text resultScore;
	[SerializeField]
	Text challengeText;
	[SerializeField]
	Text challengeTextAni;
	[SerializeField]
	Text bestScoreTextTop;
	[SerializeField]
	GameObject howtoplay;
	[SerializeField]
	GameObject btnGameStart;
	[SerializeField]
	GameObject btnHowtoplay;

	UIManager uimana;
	int scoreBorder = 1000;
	int challengeCount = 0;
	
	void Start () 
	{
		bestScore = PlayerPrefs.GetInt ("bestscore");
		if (Application.loadedLevelName == "top") 
		{
			bestScoreTextTop.text = bestScore.ToString () + "m";
		}
		if (Application.loadedLevelName == "main") 
		{
			bestScoreText.text = "BEST:" + bestScore.ToString() + "m";
		}
		challengeCount = PlayerPrefs.GetInt ("challengecount");
		challengeCount ++;

		stagemanager = this.GetComponent<StageManager> ();
		stagemanager.stgSpd = 1000f * Time.deltaTime;
		canvas = GameObject.Find ("Canvas");
		uimana = canvas.GetComponent<UIManager> ();
		if (Application.loadedLevelName == "main")
		{
			stagemanager.stgSpd = 50f;
			StartCoroutine ("GameCountDown");
			cAnim.SetBool("gameview",true);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (pSwitch == true) {
			score = score + scoreP;
			scoreText.text = score.ToString() + "m";
			if (score > bestScore)
			{
				bestScore = score;
				bestScoreText.text = "BEST:" + bestScore.ToString() + "m";
			}
		} else
			if (pSwitch == false)
		{
			scoreText.text = score.ToString() + "m";
		}
		if (score == scoreBorder) 
		{
			GameObject sb = canvas.transform.FindChild("scoreborder").gameObject;
			sb.SetActive(true);
			scoreBotderText.text = scoreBorder + "m";
			scoreBorder += 1000;
		}
	}
	public void GameOver()
	{
		cAnim.SetTrigger ("gameover");
		PlayerPrefs.SetInt ("bestscore", bestScore);
		PlayerPrefs.SetInt ("challengecount", challengeCount);
		StartCoroutine ("ResultOn");
	}

	public void GameRestart()
	{
		Application.LoadLevel ("main");
		stagemanager.stgSpd = 39f * Time.deltaTime;
	}

	public IEnumerator GameCountDown()
	{
		pSwitch = false;
		challengeTextAni.text = "CHALLENGE " + challengeCount.ToString ();
		uimana.UIactivater ("countdown", true);
		yield return new WaitForSeconds(2f);
		pSwitch = true;
	}
	public IEnumerator ResultOn()
	{
		uimana.UIactivater ("matt_score", false);
		uimana.UIactivater ("score", false);
		uimana.UIactivater ("bestScore", false);
		yield return new WaitForSeconds (1.5f);
		uimana.UIactivater ("result", true);
		resultScore.text = score.ToString () +"m";
		challengeText.text = "CHALLENGE " + challengeCount.ToString ();
		for (int rks = 0; rks <= rankscore.Length -1; rks++) 
		{
			rankscore[rks] = PlayerPrefs.GetInt("rankscore" + (rks +1));
			rankscoretext[rks].text = rankscore[rks].ToString();
		}
		Ranking ();
		yield return new WaitForSeconds (0.5f);
		uimana.UIactivater ("Button_restart", true);
		uimana.UIactivater ("Button_backhome", true);
	}
	void Ranking()
	{
		int sc = score;
		int sc2 = 0;
		for (int rs = 0; rs <= rankscore.Length -1; rs++) 
		{
			if(rankscore[rs] < sc)
			{
				sc2 = rankscore[rs];
				rankscore[rs] = sc;
				sc = sc2;
			}
			rankscoretext[rs].text = rankscore[rs].ToString() + "m";
			PlayerPrefs.SetInt("rankscore" + (rs +1),rankscore[rs]);
		}
	}
	public void GameStart()
	{
		Application.LoadLevel("main");
	}
	public void BackHome()
	{
		Application.LoadLevel("top");
	}

	public void howtoplayactivator()
	{
		howtoplay.SetActive (true);
		btnGameStart.SetActive (false);
		btnHowtoplay.SetActive (false);
	}
	public void howtoplaycloser()
	{
		btnGameStart.SetActive (true);
		btnHowtoplay.SetActive(true);
		howtoplay.SetActive (false);
	}

	public void plefsreaset()
	{
		PlayerPrefs.DeleteAll ();
	}
}
