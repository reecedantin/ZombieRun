using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using BeliefEngine.HealthKit;

/*! @brief 		Simple test class.
	@details	This class should help get you started. It demonstrates everything you need to start reading health data.
				To read steps in the last 24 hours, simply tap "Read data". To read sleep, uncomment the ReadSleep() call in Test().
				If you're testing in the simulator, or are okay with junking up your device with dummy data, set generateDummyData to true.
				This will un-hide another button – "[dummy data]" – tap this one to call the GenerateDummyData() method.
				If something goes wrong, it should display an error underneath the box.
 */
public class HealthKitTest : MonoBehaviour {
	
	public Text instructionLabel; 	/*!< @brief instruction text */
	public Text resultsLabel;		/*!< @brief to display the results of ReadData */
	public Text resultsLabel2;		// to display the results of ReadData
	public Text errorLabel;			/*!< @brief error text, for if something goes wrong */
	public Text buttonLabel;		/*!< @brief button label */

	public HealthKitDataTypes types; /*!< Provides editor UI for HealthKit datatypes, and used for authorization. */

	private HealthStore healthStore;
	private bool reading = false; // have we already sent off a request?

	// this is disabled by default, to prevent junking up your device with debug data, in case you use your primary phone as a test device.
	// if that's the case, it's probably better to use this in the simulator.
	private bool generateDummyData = true;

	void Start() {
		Debug.Log("---------- START ----------");
		this.healthStore = this.GetComponent<HealthStore>();

		if (Application.platform != RuntimePlatform.IPhonePlayer) {
			this.instructionLabel.fontSize = 20;
			this.instructionLabel.color = Color.red;
			this.instructionLabel.text = "To use this plugin, you MUST run on an iOS device or in the iOS Simulator. \nIt WILL NOT WORK in the editor.";

			string error = "HealthKit only works on iOS devices! It will not work in the Unity Editor.";
			this.errorLabel.text = error;
			Debug.LogError(error);
		} else {
			this.healthStore.Authorize(this.types, delegate(bool success) {
				Debug.LogFormat("authorization: {0}", success);
			});
		}
	}

	/*! @brief Fire off the appropriate HealthKit query.
 	 */
	public void ReadData() {
		// since this can take a while, especially on older devices, I use this to avoid firing off multiple concurrent requests.
		Debug.Log("read data...");
		if (!reading) {
			reading = true;
			
			DateTimeOffset now = DateTimeOffset.UtcNow;
			// for this example, we'll read everything from the past 24 hours
			DateTimeOffset start = now.AddDays(-1);
			
			double steps = 0;
			healthStore.ReadQuantitySamples(HKDataType.HKQuantityTypeIdentifierStepCount, start, now, delegate(List<QuantitySample> samplesW) {
				if (samplesW.Count > 0) {
		            foreach (QuantitySample sample in samplesW) {
						Debug.Log(String.Format(" - {0} from {1} to {2}", sample.quantity.doubleValue, sample.startDate, sample.endDate));
						steps += sample.quantity.doubleValue;
		            }
					 resultsLabel.text = "steps:" + steps.ToString();
				}
				reading = false;
	        });
		}
	}

	private void GenerateDummyData() {
		this.healthStore.GenerateDummyData(this.types);
	}

	/*! @brief 			do something with the steps
		@param steps	the total (cumulative) number of steps taken during the range supplied above
	 */
	private void GotSteps(int steps) {
		Debug.Log("*** READ STEPS:" + steps);
		reading = false;
	}

	private void ErrorOccurred(Error err) {
		this.errorLabel.text = err.localizedDescription;
	}

	// --- dummy data --------

	void OnGUI() {
		if (generateDummyData) {
			if (GUILayout.Button("[dummy data]")) {
				Debug.Log("Generating debug data...");
				this.GenerateDummyData();
			}
		}
	}
}
