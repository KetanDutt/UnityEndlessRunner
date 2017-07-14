using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class mainmenu : MonoBehaviour {
	public Text highscoretext;
	// Use this for initialization
	void Start () {
	 
		highscoretext.text = "Highscore : " + ((int)PlayerPrefs.GetFloat ("Highscore")).ToString();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void togame()
	{
		SceneManager.LoadScene ("GameScene");
	}
}
