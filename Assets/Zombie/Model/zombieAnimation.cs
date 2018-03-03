using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zombieAnimation : MonoBehaviour {
	protected Animator myAnimation;
	protected float speed = 1.0F;
	protected float rotationSpeed = 100.0F;

	// Use this for initialization
	void Start () {
		myAnimation = GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update () {
//		float translation = Input.GetAxis ("Vertical") * speed;
//		float rotation = Input.GetAxis ("Horizontal") * rotationSpeed;
//		translation *= Time.deltaTime;
//		rotation *= Time.deltaTime;
//		transform.Translate (0, 0, translation);
//		transform.Rotate (0, rotation, 0);

//		if (Input.GetButtonDown ("Jump")) {
//			myAnimation.SetTrigger ("isAttacking");
//		}
		myAnimation.SetBool ("isWalking", true);
//		if (translation != 0 || rotation != 0) {
//			myAnimation.SetBool ("isWalking", true);
//		} else {
//			myAnimation.SetBool ("isWalking", false);
//		}
	}
}
