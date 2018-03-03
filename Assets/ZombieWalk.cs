using System;
using System.Collections.Generic;

namespace UnityEngine.XR.iOS
{
	public class ZombieWalk : MonoBehaviour
	{
		public float maxRayDistance = 30.0f;
		private LayerMask collisionLayer = 1 << 10;  //ARKitPlane layer

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
            transform.eulerAngles = new Vector3(0, 180, 0);

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

			if(dX > 0) {
				angle = (180 + angle) % 360;
			}

			Debug.Log ("Angle of stuff " + angle + " " + dX + " " + dZ);
			//Vector3 destination = new Vector3(90, Mathf.Rad2Deg * angle, 0);
			transform.eulerAngles = new Vector3(0, angle, 0);

			transform.Translate(Vector3.forward/2 * Time.deltaTime);

            

            //transform.eulerAngles = Vector3.Lerp(transform.rotation.eulerAngles, destination, Time.deltaTime);
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
