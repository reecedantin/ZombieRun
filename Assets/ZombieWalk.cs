using System;
using System.Collections.Generic;

namespace UnityEngine.XR.iOS
{
	public class ZombieWalk : MonoBehaviour
	{
		protected Animator myAnimation;
		protected float speed = 1.0F;
		protected float rotationSpeed = 100.0F;
		public float maxRayDistance = 30.0f;

		bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes)
		{
			List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, resultTypes);
			if (hitResults.Count > 0) {
				foreach (var hitResult in hitResults) {
					Debug.Log ("Got hit!");
					transform.position = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
					transform.position = new Vector3(transform.position.x + 0, transform.position.y + 1, transform.position.z + 0);
					transform.rotation = UnityARMatrixOps.GetRotation (hitResult.worldTransform);

					Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", transform.position.x, transform.position.y, transform.position.z));
					return true;
				}
			}
			return false;
		}

		void Start() {
			myAnimation = GetComponent<Animator>();
			myAnimation.SetBool ("isWalking", true);
            //Debug.Log("Place zombie");
			ARPoint point = new ARPoint {
				x = transform.position.x,
				y = transform.position.z
			};

			List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest (point, ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);
			if (hitResults.Count > 0) {
				foreach (var hitResult in hitResults) {
					//Debug.Log ("Got hit!");
					transform.position = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
					transform.position = new Vector3(transform.position.x + 0, transform.position.y - 3, transform.position.z + 0);
					transform.rotation = UnityARMatrixOps.GetRotation (hitResult.worldTransform);

					//Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", transform.position.x, transform.position.y, transform.position.z));
				}
			}

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

			if(angle < 0) {
				angle += 360;
			}

			transform.eulerAngles = new Vector3(0, angle, 0);


			//UnityARSessionNativeInterface.ARAnchorUpdatedEvent += UpdatePositionIfARScrewUp;
		}

		// Update is called once per frame
		void Update () {
			float dX = Camera.main.transform.position.x - transform.position.x;
            float dZ = Camera.main.transform.position.z - transform.position.z;
			float angle = Mathf.Atan(dX / dZ);
			if ((dX / dZ) > 0) {
				angle = (180 + Mathf.Rad2Deg * angle) % 360;
			} else {
				angle = Mathf.Rad2Deg * angle;
			}

			//Debug.Log ("Angle of stuff " + angle + " " + dX + " " + dZ);


			if ((Mathf.Abs(dX) < 2 && Mathf.Abs(dZ) < 2) && (Mathf.Abs(dX) > 0.5 && Mathf.Abs(dZ) > 0.5)) {
				myAnimation.SetBool ("isWalking", false);
				myAnimation.SetBool ("isAttacking", true);
			}  else if (Mathf.Abs(dX) > 20 && Mathf.Abs(dZ) > 20) {
				Debug.Log ("Zombie killed " + dZ + " " + dX);
				Destroy(this);
			} else if (Mathf.Abs(dX) < 0.5 && Mathf.Abs(dZ) < 0.5) {
				myAnimation.SetBool ("isWalking", false);
				myAnimation.SetBool ("isAttacking", true);
				Debug.Log ("Player killed " + dZ + " " + dX);
				//Camera.main.GetComponent<GameScript>().isDead = true;
			} else {
				myAnimation.SetBool ("isWalking", true);
				myAnimation.SetBool ("isAttacking", false);
			}

			if(dX > 0) {
				angle = (180 + angle) % 360;
			}

			if(angle < 0) {
				angle += 360;
			}

			//Debug.Log ("Angle of stuff " + angle + " " + dX + " " + dZ);

			//transform.eulerAngles = new Vector3(0, angle, 0);
			float intermediateangle = 0;
			if(angle < transform.eulerAngles.y) {
				intermediateangle = transform.eulerAngles.y - 1;
			} else if(angle > transform.eulerAngles.y) {
				intermediateangle = transform.eulerAngles.y + 1;
			} else {
				intermediateangle = angle;
			}

			intermediateangle = intermediateangle % 360;

			transform.eulerAngles = new Vector3(transform.eulerAngles.x, intermediateangle, transform.eulerAngles.z);
			Debug.Log (transform.eulerAngles + " " + angle);
            //transform.eulerAngles = Vector3.Lerp(transform.rotation.eulerAngles, new Vector3(0, angle, 0), Time.deltaTime * .5F);

			transform.Translate(Vector3.forward * Time.deltaTime);
			//Camera.main.transform.Translate(Vector3.forward * Time.deltaTime * .2f);

		}

		void UpdatePositionIfARScrewUp(ARPlaneAnchor arPlaneAnchor) {
			ARPoint point = new ARPoint {
				x = transform.position.x,
				y = transform.position.z
			};

			List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest (point, ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane);
			if (hitResults.Count > 0) {
				foreach (var hitResult in hitResults) {
					//Debug.Log ("Got hit!");
					transform.position = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
					//transform.rotation = UnityARMatrixOps.GetRotation (hitResult.worldTransform);

					//Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", transform.position.x, transform.position.y, transform.position.z));
				}
			}
		}

	}
}
