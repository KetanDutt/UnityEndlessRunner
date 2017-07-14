using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Score : MonoBehaviour {

	private float scorevalue = 0.0f;

	private bool isDeaD = false;

	private int DifficultyLevel = 1;
	private int MaxDifficultyLevel = 10;
	private int scorenextLevel = 10;


	public Text scoretext;
	public DeathMenu deathmenu;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if (isDeaD)
			return;

		if (scorevalue >= scorenextLevel)
			levelup ();

		scorevalue += Time.deltaTime * DifficultyLevel;
		scoretext.text = ((int)scorevalue).ToString ();
	}

	void levelup()
	{
		if (DifficultyLevel == MaxDifficultyLevel)
			return;

		scorenextLevel *= 2;
		DifficultyLevel++;

		GetComponent<playerMotor>().Setspeed (DifficultyLevel);

		Debug.Log (DifficultyLevel);
	}

	public void OnDeath()
	{
		isDeaD = true;
		if (PlayerPrefs.GetFloat ("Highscore") < scorevalue)
			PlayerPrefs.SetFloat ("Highscore", scorevalue);
		deathmenu.toggleEndMenu (scorevalue);
	}
}
