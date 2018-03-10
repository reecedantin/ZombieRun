using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityEngine.XR.iOS
{
	public class ArrowScript : MonoBehaviour {

		// Use this for initialization
		void Start () {
//			ARPoint point = new ARPoint {
//				x = transform.position.x,
//				y = transform.position.z
//			};
//
//			List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest (point, ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);
//			if (hitResults.Count > 0) {
//				foreach (var hitResult in hitResults) {
//					//Debug.Log ("Got hit!");
//					transform.position = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
//					//transform.position = new Vector3(transform.position.x + 0, transform.position.y, transform.position.z + 0);
//					//transform.rotation = UnityARMatrixOps.GetRotation (hitResult.worldTransform);
//
//					//Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", transform.position.x, transform.position.y, transform.position.z));
//				}
//			}
//			transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, Camera.main.transform.eulerAngles.z);
		}

		// Update is called once per frame
		void Update () {
			transform.Translate (Vector3.forward * Time.deltaTime);
		}
	}
}
