﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityEngine.XR.iOS
{
	public class PlaceBlock : MonoBehaviour
	{
		public float maxRayDistance = 30.0f;
		public GameObject blockPrefab;

		bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes, GameObject prefab)
		{
			List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, resultTypes);
			if (hitResults.Count > 0) {
				foreach (var hitResult in hitResults) {
					Debug.Log ("Got hit!");
					Vector3 pos = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
					float posy = 0.5f + pos.y;
					pos = new Vector3 (pos.x, posy, pos.z);
					Quaternion rot = UnityARMatrixOps.GetRotation (hitResult.worldTransform);

					Instantiate (prefab, pos, rot);

					// Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));
					return true;
				}
			}
			return false;
		}

		// Update is called once per frame
		void Update () {
			//#else
			if (Input.touchCount > 0) {
				var touch = Input.GetTouch (0);
				if (touch.phase == TouchPhase.Began) {
					var screenPosition = Camera.main.ScreenToViewportPoint (touch.position);
					ARPoint point = new ARPoint {
						x = screenPosition.x,
						y = screenPosition.y
					};

					// prioritize reults types
					ARHitTestResultType[] resultTypes = {
						//ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingGeometry,
						ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent,
						// if you want to use infinite planes use this:
						//ARHitTestResultType.ARHitTestResultTypeExistingPlane,
						//ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane,
						//ARHitTestResultType.ARHitTestResultTypeEstimatedVerticalPlane,
						//ARHitTestResultType.ARHitTestResultTypeFeaturePoint
					};

					foreach (ARHitTestResultType resultType in resultTypes) {
						if (HitTestWithResultType (point, resultType, blockPrefab)) {
							return;
						}
					}
				}
			}

		}
	}
}
