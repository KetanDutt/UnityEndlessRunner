using UnityEngine;
using System.Collections;

public class cameraMotor : MonoBehaviour {

	private Transform lookout;
	private Vector3 offset;
	private Vector3 moveVector;

	private float transition = 0.0f;
	private float animationduration = 2.0f;
	private Vector3 animationoffset = new Vector3 (0,5, 5);

	void Start () {
		lookout = GameObject.FindGameObjectWithTag ("Player").transform;
		offset = transform.position - lookout.position;
	}
	
	 void Update () {
		moveVector = lookout.position + offset;
		moveVector.x = 0;
		moveVector.y = Mathf.Clamp(0,7,10);


		if (transition > 1.0f) {	transform.position = moveVector;
		} 
		else {	
			transform.position = Vector3.Lerp (offset + animationoffset, moveVector, transition);
			transition += Time.deltaTime * 1 / animationduration;
			transform.LookAt (lookout.position + Vector3.up);
		}
	}
}
