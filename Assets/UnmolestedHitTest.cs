using System;
using System.Collections.Generic;

namespace UnityEngine.XR.iOS
{
	public class UnmolestedHitTest : MonoBehaviour
	{
		public Transform m_HitTransform;
		public float maxRayDistance = 30.0f;
		public LayerMask collisionLayer = 1 << 10;  //ARKitPlane layer

		bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes)
		{
			List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, resultTypes);
			if (hitResults.Count > 0) {
				foreach (var hitResult in hitResults) {
					Debug.Log ("Got hit!");
					m_HitTransform.position = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
                    m_HitTransform.position = new Vector3(m_HitTransform.position.x + 0, m_HitTransform.position.y + 1, m_HitTransform.position.z + 0);
					m_HitTransform.rotation = UnityARMatrixOps.GetRotation (hitResult.worldTransform);

					 Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));
					return true;
				}
			}
			return false;
		}

		void Start() {
            Debug.Log("Place zombie");
			ARPoint point = new ARPoint {
				x = transform.position.x,
				y = transform.position.z
			};

			List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest (point, ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane);
			if (hitResults.Count > 0) {
				foreach (var hitResult in hitResults) {
					Debug.Log ("Got hit!");
					m_HitTransform.position = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
					m_HitTransform.position = new Vector3(m_HitTransform.position.x + 0, m_HitTransform.position.y + 1, m_HitTransform.position.z + 0);
					m_HitTransform.rotation = UnityARMatrixOps.GetRotation (hitResult.worldTransform);

					Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));
				}
			}
            transform.eulerAngles = new Vector3(0, 0, 0);
		}

		// Update is called once per frame
		void Update () {
			float dX = Camera.main.transform.position.x - transform.position.x;
            float dZ = Camera.main.transform.position.z - transform.position.z;
			float angle = Mathf.Atan(dX / dZ);
            // if((dX/dZ) < 0) {
            //     angle = angle * -1;
            // }
			Debug.Log ("Angle of stuff " + angle + " " + dX + " " + dZ);
			//Vector3 destination = new Vector3(90, Mathf.Rad2Deg * angle, 0);
            transform.eulerAngles = new Vector3(0, Mathf.Rad2Deg * angle, 0);

            if(dZ < 0) {
                transform.Translate(Vector3.back/2 * Time.deltaTime);
            } else {
                transform.Translate(Vector3.forward/2 * Time.deltaTime);
            }

            //transform.eulerAngles = Vector3.Lerp(transform.rotation.eulerAngles, destination, Time.deltaTime);
		}
	}
}
