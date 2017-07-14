using UnityEngine;
using System.Collections;

public class DeathPlane : MonoBehaviour {
	private Transform lookout;
	private Vector3 offset;
	private Vector3 moveVector;

	// Use this for initialization
	void Start () {
		lookout = GameObject.FindGameObjectWithTag ("Player").transform;
		offset = transform.position - lookout.position;
	}
	
	// Update is called once per frame
	void Update () {
		moveVector = lookout.position + offset;
		moveVector.x = 0;
		moveVector.y = -3;

		transform.position = moveVector;
	}
}
