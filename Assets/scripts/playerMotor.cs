using UnityEngine;
using System.Collections;

public class playerMotor : MonoBehaviour {

	private CharacterController controller;

	private bool isDead = false;

	private float verticalvelocity = 0.0f;
	private float speed = 10f;
	private float gravity = 80f;
	private float angle = 0f;
	private float animationduration = 2.0f;
	private float starttime;
	//private float frictionbase = -5f;

	private Vector3 moveVector;

	void Start () {
		controller = GetComponent<CharacterController> ();
		starttime = Time.time;
	}


	void Update () {

		if (isDead)
			return;

		if (Time.time - starttime < animationduration) {
			controller.Move (Vector3.forward * speed * Time.deltaTime);
			return;
		}

		moveVector = Vector3.zero;

		if (controller.isGrounded) {
			//verticalvelocity = frictionbase;
			verticalvelocity += Input.GetAxisRaw ("Jump") * speed * 3;

		}
		else{	verticalvelocity -= gravity * Time.deltaTime;
		}
		//For x axis
		moveVector.x -= angle ;

		//for y axis
		moveVector.y = verticalvelocity;

		//For z axis
		moveVector.z = speed;

		controller.Move ( moveVector * Time.deltaTime );
	}


	public void Setspeed(float modifier)
	{	
		speed = 10f + modifier*3;
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.gameObject.tag == "Enemy")
			Death ();
	}

	private void Death()
	{
		isDead = true;
		GetComponent<Score> ().OnDeath ();
	}
}
