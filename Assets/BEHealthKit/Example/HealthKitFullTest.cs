using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using BeliefEngine.HealthKit;

public class HealthKitFullTest : MonoBehaviour {
	
	//public Text instructionLabel;
	public Text resultsLabel;
	//public Text errorLabel;
	//public Dropdown dropdown;
	//public Text buttonLabel;

	public HealthKitDataTypes types;

	private HealthStore healthStore;
	private bool reading = false;
	private bool generateDummyData = true;

	void Awake() {
		//dropdown.ClearOptions();
		List<string> opts = new List<string>();
		for (int i = 0; i <= (int)HKDataType.HKQuantityTypeIdentifierUVExposure; i++) {
			HKDataType dataType = (HKDataType)i;
			opts.Add(HealthKitDataTypes.GetIdentifier(dataType));
		}

		opts.Add("———");

		for (int i = (int)HKDataType.HKCharacteristicTypeIdentifierBiologicalSex; i <= (int)HKDataType.HKCharacteristicTypeIdentifierWheelchairUse; i++) {
			HKDataType dataType = (HKDataType)i;
			opts.Add(HealthKitDataTypes.GetIdentifier(dataType));
		}

		opts.Add("———");

		for (int i = (int)HKDataType.HKCorrelationTypeIdentifierBloodPressure; i <= (int)HKDataType.HKCorrelationTypeIdentifierFood; i++) {
			HKDataType dataType = (HKDataType)i;
			opts.Add(HealthKitDataTypes.GetIdentifier(dataType));
		}

		opts.Add("———");

		opts.Add(HealthKitDataTypes.GetIdentifier(HKDataType.HKWorkoutTypeIdentifier));

		//dropdown.AddOptions(opts);
	}

	void Start() {
		Debug.Log("---------- START ----------");
		this.healthStore = this.GetComponent<HealthStore>();

		if (Application.platform != RuntimePlatform.IPhonePlayer) {
//			this.instructionLabel.fontSize = 20;
//			this.instructionLabel.color = Color.red;
//			this.instructionLabel.text = "To use this plugin, you MUST run on an iOS device or in the iOS Simulator. \nIt WILL NOT WORK in the editor.";
//
//			string error = "HealthKit only works on iOS devices! It will not work in the Unity Editor.";
//			this.errorLabel.text = error;
//			Debug.LogError(error);
		} else {
			this.healthStore.Authorize(this.types);
			this.ReadData();
		}
	}

	public void ReadData() {
		Debug.Log("read data...");
		if (!reading) {
//			string selectedName = dropdown.options[dropdown.value].text;
			try {
				HKDataType dataType = HKDataType.HKQuantityTypeIdentifierStepCount;
				reading = true;

				DateTimeOffset now = DateTimeOffset.UtcNow;
				// for this example, we'll read everything from the past 24 hours
				// commented prev example, trying to read current pedo value
//				DateTimeOffset start = now.AddDays(-1);

				ReadPedometer(now);

//				if (dataType <= HKDataType.HKQuantityTypeIdentifierUVExposure) {
//					// quantity-type
//					Debug.Log("reading quantity-type...");
//					ReadQuantityData(dataType, start, now);
//				}
//				else if (dataType <= HKDataType.HKCategoryTypeIdentifierMindfulSession) {
//					// category-type
//					Debug.Log("reading category-type...");
//				}
//				else if (dataType <= HKDataType.HKCharacteristicTypeIdentifierWheelchairUse) {
//					// characteristic-type
//					Debug.Log("reading characteristic-type...");
//					ReadCharacteristic(dataType);
//				}
//				else if (dataType <= HKDataType.HKCorrelationTypeIdentifierFood) {
//					// correlation-type
//					Debug.Log("reading correlation-type...");
//					ReadCorrelationData(dataType, start, now);
//				}
//				else if (dataType == HKDataType.HKWorkoutTypeIdentifier) {
//					// finally, workout-type
//					Debug.Log("reading workout-type...");
//					ReadWorkoutData(dataType, start, now);
//				} else {
//					Debug.LogError(string.Format("data type {0} invalid", HealthKitDataTypes.GetIdentifier(dataType)));
//				}
			}
			catch (ArgumentException) {
				// they just selected a divider; nothing to worry about
				Debug.Log("Try for ReadData Failed");
			}


			// Or:
			// ReadSteps(start, now);
			// ReadFlights(start, now);

			// or alternatively
			// ReadSleep(start, now);
			// or...
			// ReadPedometer(now);
			
		}
	}

	public void WriteData() {
		Quantity quantity = new Quantity(0.5, "mi");
		DateTimeOffset now = DateTimeOffset.UtcNow;
		// for this example, we'll say this sample was from the last 10 minutes
		DateTimeOffset start = now.AddMinutes(-10);
		
		// this.healthStore.WriteQuantitySample(HKDataType.HKQuantityTypeIdentifierDistanceWalkingRunning, quantity, start, now, delegate(bool success, Error error) {
		// 	if (!success) {
		// 		Debug.LogErrorFormat("error:{0}", error.localizedDescription);
		// 	} else {
		// 		Debug.Log(@"success");
		// 	}
		// });

		this.healthStore.WriteWorkoutSample(WorkoutActivityType.AmericanFootball, start, now, 1000, 0, delegate(bool success, Error error) {
			if (!success) {
				Debug.LogErrorFormat("error:{0}", error.localizedDescription);
			} else {
				Debug.Log(@"success");
			}
		});
	}

	// this is an example of reading Category data. You can cast the sample value to whatever appropriate enum for the sample type. See HealthKitDataTypes.cs for other types.
	private void ReadSleep(DateTimeOffset start, DateTimeOffset end) {
		Debug.Log("reading sleep from " + start + " to " + end);
		this.healthStore.ReadCategorySamples(HKDataType.HKCategoryTypeIdentifierSleepAnalysis, start, end, delegate(List<CategorySample> samples) {
			string text = "";
			foreach (CategorySample sample in samples) {
				string valueString = ((SleepAnalysis)sample.value == SleepAnalysis.Asleep) ? "Sleeping" : "In Bed";
				string str = string.Format("- {0} from {1} to {2}", valueString, sample.startDate, sample.endDate);
				Debug.Log(str);
				text = text + str + "\n";
			}
			this.resultsLabel.text = text;

			// all done
			reading = false;
		});
	}

	private void ReadFlights(DateTimeOffset start, DateTimeOffset end) {
		this.resultsLabel.text = string.Format("Reading flights climbed from {0} to {1}...\n", start, end);
		int steps = 0;
		this.healthStore.ReadQuantitySamples(HKDataType.HKQuantityTypeIdentifierFlightsClimbed, start, end, delegate(List<QuantitySample> samples) {
			Debug.Log("found " + samples.Count + " flights samples");
			foreach (QuantitySample sample in samples) {
				Debug.Log("   - " + sample.quantity.doubleValue + " from " + sample.startDate + " to " + sample.endDate);
				steps += Convert.ToInt32(sample.quantity.doubleValue);
			}

			if (steps > 0) {
				this.resultsLabel.text += "FLIGHTS CLIMBED:" + steps;
			} else {
				this.resultsLabel.text += "No flights found.";
			}

			// all done
			reading = false;
		});
	}

	// A basic example of reading Quantity data.
	private void ReadQuantityData(HKDataType dataType, DateTimeOffset start, DateTimeOffset end) {
		string typeName = HealthKitDataTypes.GetIdentifier(dataType);
		Debug.LogFormat("reading {0} from {1} to {2}", typeName, start, end);
		double sum = 0;
		this.healthStore.ReadQuantitySamples(dataType, start, end, delegate(List<QuantitySample> samples) {
			if (samples.Count > 0) {
				Debug.Log("found " + samples.Count + " samples");
				bool cumulative = (samples[0].quantityType == QuantityType.cumulative);
				string text = "";
				foreach (QuantitySample sample in samples) {
					Debug.Log("   - " + sample);
					if (cumulative) sum += Convert.ToInt32(sample.quantity.doubleValue);
					else text = text + "- " + sample + "\n";
				}

				if (cumulative) {
					if (sum > 0) this.resultsLabel.text = typeName + ":" + sum;
				} else {
					this.resultsLabel.text = text;
				}
			} else {
				Debug.Log("found no samples");
			}


			// all done
			reading = false;
		});
	}
	
	// reading a Characteristic
	private void ReadCharacteristic(HKDataType dataType) {
		string typeName = HealthKitDataTypes.GetIdentifier(dataType);
		Debug.LogFormat("reading {0}", typeName);
		this.healthStore.ReadCharacteristic(dataType, delegate(Characteristic characteristic) {
			Debug.Log("FINISHED");
			string text = string.Format("{0} = {1}", dataType, characteristic);
			this.resultsLabel.text = text;

			// all done
			reading = false;
		});

	}

	// a generic example of reading correlation data. If you're interested in nutritional correlations, you'd probably tailor your delegate to the specific nutritional information you're looking at.
	private void ReadCorrelationData(HKDataType dataType, DateTimeOffset start, DateTimeOffset end) {
		this.healthStore.ReadCorrelationSamples(dataType, start, end, delegate(List<CorrelationSample> samples) {
			string text = "";
			foreach (CorrelationSample sample in samples) {
				Debug.Log("   - " + sample);
				text = text + "- " + sample + "\n";
			}
			this.resultsLabel.text = text;

			// all done
			reading = false;
		});
	}

	private void ReadWorkoutData(HKDataType dataType, DateTimeOffset start, DateTimeOffset end) {
		this.healthStore.ReadWorkoutSamples(WorkoutActivityType.Fencing, start, end, delegate(List<WorkoutSample> samples) {
			string text = "";
			foreach (WorkoutSample sample in samples) {
				Debug.Log("   - " + sample);
				text = text + "- " + sample + "\n";
			}
			this.resultsLabel.text = text;

			// all done
			reading = false;
		});
	}

	private void ReadPedometer(DateTimeOffset start) {
		Debug.Log("isReading val: " + reading);
		if (reading) {
			int steps = 0;
			this.healthStore.BeginReadingPedometerData(start, delegate(List<PedometerData> data) {
				foreach (PedometerData sample in data) {
					steps += sample.numberOfSteps;
				}
				this.resultsLabel.text = string.Format("{0}", steps);
				Debug.Log("pedometer " + steps);
			});
//			buttonLabel.text = "Stop reading";
			reading = true;
		} else {
			this.healthStore.StopReadingPedometerData();
//			buttonLabel.text = "Start reading";
			reading = false;
		}
	}

	
	private void GenerateDummyData() {
		this.healthStore.GenerateDummyData(this.types);
	}

	private void GotSteps(int steps) {
		Debug.Log("*** READ STEPS:" + steps);
		reading = false;
	}

	private void ErrorOccurred(Error err) {
//		this.errorLabel.text = err.localizedDescription;
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
