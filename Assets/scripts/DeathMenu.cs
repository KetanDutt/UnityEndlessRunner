using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour {

	public Text Scoretext;
	public Image bgimage;

	private bool isShown = false;

	private float transition = 0.0f;

	// Use this for initialization
	void Start () {
		gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
	
		if (!isShown)
			return;

		transition += Time.deltaTime;
		bgimage.color = Color.Lerp (new Color (0, 0, 0, 0), Color.black, transition);

	}

	public void toggleEndMenu (float scorevalue)
	{
		gameObject.SetActive (true);
		Scoretext.text = ((int)scorevalue).ToString ();
		isShown = true;
	}

	public void Restart()
	{
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	public void menu()
	{
		SceneManager.LoadScene ("Menu");
	}
}
