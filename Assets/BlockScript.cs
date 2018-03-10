using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		float dX = Camera.main.transform.position.x - transform.position.x;
		float dZ = Camera.main.transform.position.z - transform.position.z;
		float angle = Mathf.Atan(dX / dZ);
		if ((dX / dZ) > 0) {
			angle = (180 + Mathf.Rad2Deg * angle) % 360;
		} else {
			angle = Mathf.Rad2Deg * angle;
		}

		if(dX > 0) {
			angle = (180 + angle) % 360;
		}

		Debug.Log ("Angle of stuff " + angle + " " + dX + " " + dZ);
		transform.eulerAngles = new Vector3(0, angle, 0);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
