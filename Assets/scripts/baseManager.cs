using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class baseManager : MonoBehaviour {

	public GameObject[] basePreFabs;

	private Transform playertransform;
	private float spwnZ = -10.0f;
	private float safezone = 20.0f;
	private float baselength = 26.0f;
	private int amtbase = 7;
	private int lastprefab = 0;
	private List<GameObject> active = new List<GameObject>();

	// Use this for initialization
	void Start () {
		playertransform = GameObject.FindGameObjectWithTag ("Player").transform;

		for (int i = 0; i < amtbase; i++) {
			if (i < 2)
				spwnbase (0);
			else
				spwnbase ();
		}

	}

	// Update is called once per frame
	void Update () {
		if (playertransform.position.z - safezone > (spwnZ - amtbase * baselength)) {
			spwnbase ();
			deletebase ();
		}
	}

	private void spwnbase(int prefabindex = -1)
	{
		GameObject go;
		if ( prefabindex == -1)
			go = Instantiate (basePreFabs [randomprefab()]) as GameObject;
		else 
			go = Instantiate (basePreFabs [prefabindex]) as GameObject;

		go.transform.SetParent (transform);
		go.transform.position = Vector3.forward * spwnZ;
		spwnZ += baselength;
		active.Add (go);
	}

	private void deletebase(){
		Destroy (active [0]);
		active.RemoveAt (0);
	}

	private int randomprefab()
	{
		if (basePreFabs.Length <= 1)
			return 0;

		int randomindex = lastprefab;
		while (randomindex == lastprefab) {
			randomindex = Random.Range (0, basePreFabs.Length);
		}

		lastprefab = randomindex;
		return randomindex;
	}
}
