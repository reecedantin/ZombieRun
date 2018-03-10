using UnityEngine;
using System;
using System.Xml;
using System.Collections.Generic;

using System.Runtime.InteropServices;

namespace BeliefEngine.HealthKit
{

/*! @brief callback delegate for the ReadCombinedQuantitySamples method */
public delegate void ReceivedQuantity(double num);

/*! @brief callback delegate for the ReadQuantitySamples method */
public delegate void ReceivedQuantitySamples(List<QuantitySample> samples);

/*! @brief callback delegate for the ReadCategorySamples method */
public delegate void ReceivedCategorySamples(List<CategorySample> samples);

/*! @brief callback delegate for the ReadCharacteristic method */
public delegate void ReceivedCharacteristic(Characteristic characteristic);

/*! @brief callback delegate for the ReadCorrelationSamples method */
public delegate void ReceivedCorrelationSamples(List<CorrelationSample> samples);

/*! @brief callback delegate for the ReadWorkoutSamples method */
public delegate void ReceivedWorkoutSamples(List<WorkoutSample> samples);

/*! @brief callback delegate for the BeginReadingPedometerData method */
public delegate void ReceivedPedometerData(List<PedometerData> data);

/*! @brief callback delegate for the various Write Sample methods */
public delegate void WroteSample(bool success, Error error);

/*! @brief callback delegate for the Authorize method */
public delegate void AuthorizationHandler(bool success);


/*! @brief Primary interface for HealthKit.
 */
public class HealthStore : MonoBehaviour {

	/*! @brief Does basic setup for the plugin */
	public void Awake() {
		#if UNITY_IOS && !UNITY_EDITOR
		_InitializeNative(this.gameObject.name);
		#endif
		receivedQuantityHandlers = new Dictionary<HKDataType, ReceivedQuantity>();
		receivedQuantitySamplesHandlers = new Dictionary<HKDataType, ReceivedQuantitySamples>();
		receivedCategorySamplesHandlers = new Dictionary<HKDataType, ReceivedCategorySamples>();
		receivedCharacteristicHandlers = new Dictionary<HKDataType, ReceivedCharacteristic>();
		receivedCorrelationSamplesHandlers = new Dictionary<HKDataType, ReceivedCorrelationSamples>();
		receivedWorkoutSamplesHandlers = new Dictionary<WorkoutActivityType, ReceivedWorkoutSamples>();
		wroteSampleHandlers = new Dictionary<HKDataType, WroteSample>();
	}

	/*! @brief returns true if HealthKit is available on this device. */
	public bool IsHealthDataAvailable() {
		#if UNITY_IOS && !UNITY_EDITOR
		return _IsHealthDataAvailable();
		#else
		return false;
		#endif
	}

	/*! @brief   returns authorization status for a given datatype.
		@details See [HKAuthorizationStatus](https://developer.apple.com/documentation/healthkit/hkauthorizationstatus) in the Apple documentation.
				 More useful for write permission; will not tell you if the user denies permission to read the data; it will merely appear as if there is no data.
		@param   dataType the HealthKit datatype to query	
	*/
	public HKAuthorizationStatus AuthorizationStatusForType(HKDataType dataType) {
		HKAuthorizationStatus status = HKAuthorizationStatus.NotDetermined;
		
		#if UNITY_IOS && !UNITY_EDITOR
		string identifier = HealthKitDataTypes.GetIdentifier(dataType);
		try {
			status = (HKAuthorizationStatus)_AuthorizationStatusForType(identifier);
		}
		catch (System.Exception) {
			Debug.LogErrorFormat("error parsing authorization status: '{0}'", identifier);
		}
		#endif
		
		return status;
	}

	/*! @brief requests authorization to read the supplied data types, with a completion handler. */
	public void Authorize(HealthKitDataTypes types, AuthorizationHandler handler) {
		#if UNITY_IOS && !UNITY_EDITOR
		if (handler != null) this.authorizationHandler += handler;
		Debug.Log("--- authorizing ---");
		_Authorize(types.Transmit());
		#endif
	}

	/*! @brief requests authorization to read the supplied data types. */
	public void Authorize(HealthKitDataTypes types) {
		this.Authorize(types, null);
	}

	/*! @brief Generates dummy data for the supplied data types. Mostly for testing in the Simulator. */
	public void GenerateDummyData(HealthKitDataTypes types) {
		#if UNITY_IOS && !UNITY_EDITOR
		Debug.Log("--- generating debug data ---");
		_GenerateDummyData(types.Transmit());
		#else
		Debug.LogError("Dummy data is not currently available in the editor.");
		#endif
	}

	// ------------------------------------------------------------------------------
	// Delegate Interface
	// ------------------------------------------------------------------------------

	// Quantity types

	/*! @brief 				Read quantity samples & return the sum.
		@details
		@param dataType		The datatype to read.
		@param startDate	The date to start reading samples from.
		@param endDate		The end date to limit samples to.
		@param handler		Called when the function finishes executing.
	 */
	public void ReadCombinedQuantitySamples(HKDataType dataType, DateTimeOffset startDate, DateTimeOffset endDate, ReceivedQuantity handler) {
		this.receivedQuantityHandlers[dataType] = handler;
		this.ReadQuantity(dataType, startDate, endDate, true);
	}

	/*! @brief 				Read quantity samples & return a list of QuantitySamples.
		@details
		@param dataType		The datatype to read.
		@param startDate	The date to start reading samples from.
		@param endDate		The end date to limit samples to.
		@param handler		Called when the function finishes executing.
	 */
	public void ReadQuantitySamples(HKDataType dataType, DateTimeOffset startDate, DateTimeOffset endDate, ReceivedQuantitySamples handler) {
		this.receivedQuantitySamplesHandlers[dataType] = handler;
		this.ReadQuantity(dataType, startDate, endDate, false);
	}

	/*! @brief 				Write a quantity sample.
		@details
		@param dataType		The datatype to write.
		@param quantity		the quantity to use to create a sample.
		@param startDate	The starting date of the sample to write.
		@param endDate		The ending date of the sample to write.
		@param handler		Called when the function finishes executing.
	 */
	public void WriteQuantitySample(HKDataType dataType, Quantity quantity, DateTimeOffset startDate, DateTimeOffset endDate, WroteSample handler) {
		this.wroteSampleHandlers[dataType] = handler;
		if (this.IsHealthDataAvailable()) {
			string identifier = HealthKitDataTypes.GetIdentifier(dataType);
			_WriteQuantity(identifier, quantity.unit, quantity.doubleValue, DateTimeBridge.DateToString(startDate), DateTimeBridge.DateToString(endDate));
		} else {
			Debug.LogError("Error: no health data is available. Are you running on an iOS device that supports HealthKit?");
		}
	}

	// Category types

	/*! @brief 				Read category samples & return a list of CategorySamples.
		@details
		@param dataType		The datatype to read.
		@param startDate	The date to start reading samples from.
		@param endDate		The end date to limit samples to.
		@param handler		Called when the function finishes executing.
	 */
	public void ReadCategorySamples(HKDataType dataType, DateTimeOffset startDate, DateTimeOffset endDate, ReceivedCategorySamples handler) {
		this.receivedCategorySamplesHandlers[dataType] = handler;
		this.ReadCategory(dataType, startDate, endDate);
	}

	/*! @brief 				Write a category sample.
		@details
		@param dataType		The datatype to write.
		@param value		the (integer) value to use to create a sample.
		@param startDate	The starting date of the sample to write.
		@param endDate		The ending date of the sample to write.
		@param handler		Called when the function finishes executing.
	 */
	public void WriteCategorySample(HKDataType dataType, int value, DateTimeOffset startDate, DateTimeOffset endDate, WroteSample handler) {
		this.wroteSampleHandlers[dataType] = handler;
		if (this.IsHealthDataAvailable()) {
			string identifier = HealthKitDataTypes.GetIdentifier(dataType);
			_WriteCategory(identifier, value, DateTimeBridge.DateToString(startDate), DateTimeBridge.DateToString(endDate));
		} else {
			Debug.LogError("Error: no health data is available. Are you running on an iOS device that supports HealthKit?");
		}
	}

	// Characteristic types

	/*! @brief 				Read a characteristic.
		@details
		@param dataType		The datatype to read.
		@param handler		Called when the function finishes executing.
	 */
	public void ReadCharacteristic(HKDataType dataType, ReceivedCharacteristic handler) {
		this.receivedCharacteristicHandlers[dataType] = handler;
		string identifier = HealthKitDataTypes.GetIdentifier(dataType);
		_ReadCharacteristic(identifier);

	}

	// Correlation types

	/*! @brief 				Read correlation samples & return a list of CorrelationSamples.
		@details
		@param dataType		The datatype to read.
		@param startDate	The date to start reading samples from.
		@param endDate		The end date to limit samples to.
		@param handler		Called when the function finishes executing.
	 */
	public void ReadCorrelationSamples(HKDataType dataType, DateTimeOffset startDate, DateTimeOffset endDate, ReceivedCorrelationSamples handler) {
		this.receivedCorrelationSamplesHandlers[dataType] = handler;
		ReadCorrelation(dataType, startDate, endDate, false);
	}

	// Workout types

	/*! @brief 				Read workout samples & return a list of WorkoutSamples.
		@details
		@param activityType	The activity type to read.
		@param startDate	The date to start reading samples from.
		@param endDate		The end date to limit samples to.
		@param handler		Called when the function finishes executing.
	 */
	public void ReadWorkoutSamples(WorkoutActivityType activityType, DateTimeOffset startDate, DateTimeOffset endDate, ReceivedWorkoutSamples handler) {
		this.receivedWorkoutSamplesHandlers[activityType] = handler;
		ReadWorkout(activityType, startDate, endDate, false);

	}

	/*! @brief 				Write a workout sample.
		@details
		@param activityType	The workout activity type to write.
		@param startDate	The starting date of the sample to write.
		@param endDate		The ending date of the sample to write.
		@param handler		Called when the function finishes executing.
	 */
	public void WriteWorkoutSample(WorkoutActivityType activityType, DateTimeOffset startDate, DateTimeOffset endDate, WroteSample handler) {
		this.wroteSampleHandlers[HKDataType.HKWorkoutTypeIdentifier] = handler;
		if (this.IsHealthDataAvailable()) {
			int identifier = (int)activityType;
			_WriteWorkoutSimple(identifier, DateTimeBridge.DateToString(startDate), DateTimeBridge.DateToString(endDate));
		} else {
			Debug.LogError("Error: no health data is available. Are you running on an iOS device that supports HealthKit?");
		}
	}

	/*! @brief 				Write a workout sample.
		@details
		@param activityType	The workout activity type to write.
		@param startDate	The starting date of the sample to write.
		@param endDate		The ending date of the sample to write.
		@param calories		The kilocalories burned during the activity
		@param distance		The distance traveled during the activity (for e.g. running)
		@param handler		Called when the function finishes executing.
	 */
	public void WriteWorkoutSample(WorkoutActivityType activityType, DateTimeOffset startDate, DateTimeOffset endDate, double calories, double distance, WroteSample handler) {
		this.wroteSampleHandlers[HKDataType.HKWorkoutTypeIdentifier] = handler;
		if (this.IsHealthDataAvailable()) {
			int identifier = (int)activityType;
			_WriteWorkout(identifier, DateTimeBridge.DateToString(startDate), DateTimeBridge.DateToString(endDate), calories, distance);
		} else {
			Debug.LogError("Error: no health data is available. Are you running on an iOS device that supports HealthKit?");
		}
	}


	/*! @brief 				start reading from the pedometer.
		@details			Start reading from the pedometer & register a delegate to parse the samples.
		@param startDate	The date to start reading samples from.
		@param handler		Called when a sample is received.
	 */
	public void BeginReadingPedometerData(DateTimeOffset startDate, ReceivedPedometerData handler) {
		this.receivedPedometerDataHandler += handler;
		this.BeginReadingPedometer(startDate);
	}


	/*! @brief 				stop reading from the pedometer.
		@details
	 */
	public void StopReadingPedometerData() {
		this.StopReadingPedometer();
		this.receivedPedometerDataHandler = null;
	}

	// Convenience

	/*! @brief a convenience method to read HKQuantityTypeIdentifierStepCount quantity samples */
	public void ReadSteps(DateTimeOffset startDate, DateTimeOffset endDate, ReceivedQuantity handler) {
		this.receivedQuantityHandlers[HKDataType.HKQuantityTypeIdentifierStepCount] = handler;
		this.ReadQuantity(HKDataType.HKQuantityTypeIdentifierStepCount, startDate, endDate, true);
	}


	// ------------------------------------------------------------------------------
	// Lambda Functions
	// ------------------------------------------------------------------------------



	// ------------------------------------------------------------------------------
	// Internal
	// ------------------------------------------------------------------------------
	
	private event AuthorizationHandler authorizationHandler;
	private Dictionary<HKDataType, ReceivedQuantity> receivedQuantityHandlers;
	private Dictionary<HKDataType, ReceivedQuantitySamples> receivedQuantitySamplesHandlers;
	private Dictionary<HKDataType, ReceivedCategorySamples> receivedCategorySamplesHandlers;
	private Dictionary<HKDataType, ReceivedCharacteristic> receivedCharacteristicHandlers;
	private Dictionary<HKDataType, ReceivedCorrelationSamples> receivedCorrelationSamplesHandlers;
	private Dictionary<WorkoutActivityType, ReceivedWorkoutSamples> receivedWorkoutSamplesHandlers;
	private Dictionary<HKDataType, WroteSample> wroteSampleHandlers;
	private event ReceivedPedometerData receivedPedometerDataHandler;

	private void ReadQuantity(HKDataType dataType, DateTimeOffset startDate, DateTimeOffset endDate, bool combineSamples) {
		if (this.IsHealthDataAvailable()) {
			string identifier = HealthKitDataTypes.GetIdentifier(dataType);
			string startStamp = DateTimeBridge.DateToString(startDate);
			string endStamp = DateTimeBridge.DateToString(endDate);
			Debug.LogFormat("reading quantity from:\n-{0} ({1})\nto:\n-{2} ({3})", startDate, startStamp, endDate, endStamp);
			_ReadQuantity(identifier, startStamp, endStamp, combineSamples);
		} else {
			Debug.LogError("Error: no health data is available. Are you running on an iOS device that supports HealthKit?");
		}
	}

	private void ReadCategory(HKDataType dataType, DateTimeOffset startDate, DateTimeOffset endDate) {
		if (this.IsHealthDataAvailable()) {
			string identifier = HealthKitDataTypes.GetIdentifier(dataType);
			_ReadCategory(identifier, DateTimeBridge.DateToString(startDate), DateTimeBridge.DateToString(endDate));
		} else {
			Debug.LogError("Error: no health data is available. Are you running on an iOS device that supports HealthKit?");
		}
	}

	private void ReadCorrelation(HKDataType dataType, DateTimeOffset startDate, DateTimeOffset endDate, bool combineSamples) {
		if (this.IsHealthDataAvailable()) {
			string identifier = HealthKitDataTypes.GetIdentifier(dataType);
			_ReadCorrelation(identifier, DateTimeBridge.DateToString(startDate), DateTimeBridge.DateToString(endDate), combineSamples);
		} else {
			Debug.LogError("Error: no health data is available. Are you running on an iOS device that supports HealthKit?");
		}
	}

	private void ReadWorkout(WorkoutActivityType activityType, DateTimeOffset startDate, DateTimeOffset endDate, bool combineSamples) {
		if (this.IsHealthDataAvailable()) {
			int identifier = (int)activityType;
			_ReadWorkout(identifier, DateTimeBridge.DateToString(startDate), DateTimeBridge.DateToString(endDate), combineSamples);
		} else {
			Debug.LogError("Error: no health data is available. Are you running on an iOS device that supports HealthKit?");
		}
	}

	private void BeginReadingPedometer(DateTimeOffset startDate) {
		if (this.IsHealthDataAvailable()) {
			_StartReadingPedometerFromDate(DateTimeBridge.DateToString(startDate));
		} else {
			Debug.LogError("Error: no health data is available. Are you running on an iOS device that supports HealthKit?");
		}
	}

	private void StopReadingPedometer() {
		if (this.IsHealthDataAvailable()) {
			_StopReadingPedometer();
		} else {
			Debug.LogError("Error: no health data is available. Are you running on an iOS device that supports HealthKit?");
		}
	}

	private void AuthorizeComplete(string response) {
		bool success = (response == "true");
		if (this.authorizationHandler != null) {
			this.authorizationHandler(success);
			this.authorizationHandler = null;
		}
	}

	private void ParseHealthXML(string xmlString) {
		HealthData xml = new HealthData(xmlString);
		string rootName = xml.RootName();
		switch (rootName) {
			case "quantity":
				List<QuantitySample> qSamples = xml.ParseQuantitySamples();
				if (this.receivedQuantitySamplesHandlers.ContainsKey(xml.datatype)) {
					this.receivedQuantitySamplesHandlers[xml.datatype](qSamples);
					this.receivedQuantitySamplesHandlers[xml.datatype] = null;
				}
				break;
			case "category":
				List<CategorySample> catSamples = xml.ParseCategorySamples();
				if (this.receivedCategorySamplesHandlers.ContainsKey(xml.datatype)) {
					this.receivedCategorySamplesHandlers[xml.datatype](catSamples);
					this.receivedCategorySamplesHandlers[xml.datatype] = null;
				}
				break;
			case "characteristic":
				Characteristic c = xml.ParseCharacteristic();
				if (this.receivedCharacteristicHandlers.ContainsKey(xml.datatype)) {
					this.receivedCharacteristicHandlers[xml.datatype](c);
					this.receivedCharacteristicHandlers[xml.datatype] = null;
				}
				break;
			case "correlation":
				List<CorrelationSample> corSamples = xml.ParseCorrelationSamples();
				if (this.receivedCorrelationSamplesHandlers.ContainsKey(xml.datatype)) {
					this.receivedCorrelationSamplesHandlers[xml.datatype](corSamples);
					this.receivedCorrelationSamplesHandlers[xml.datatype] = null;
				}
				break;
			case "workout":
				List<WorkoutSample> wSamples = xml.ParseWorkoutSamples();
				if (this.receivedWorkoutSamplesHandlers.ContainsKey(xml.workoutType)) {
					this.receivedWorkoutSamplesHandlers[xml.workoutType](wSamples);
					this.receivedWorkoutSamplesHandlers[xml.workoutType] = null;
				}
				break;
			case "count":
				double count = xml.ParseTotal();
				if (this.receivedQuantityHandlers.ContainsKey(xml.datatype)) {
					this.receivedQuantityHandlers[xml.datatype](count);
					this.receivedQuantityHandlers[xml.datatype] = null;
				}
				break;
			case "pedometer":
				List<PedometerData> qData = xml.ParsePedometerData();
				if (this.receivedPedometerDataHandler != null) {
					this.receivedPedometerDataHandler(qData);
					// this.receivedPedometerDataHandler = null;
				}
				break;
			case "write":
				if (this.wroteSampleHandlers.ContainsKey(xml.datatype)) {
					this.wroteSampleHandlers[xml.datatype](true, null);
					this.wroteSampleHandlers[xml.datatype] = null;
				}
				break;
			default:
				Debug.LogError("error; unrecognized root node:" + rootName);
				break;
		}
	}
	
	private void HealthKitErrorOccurred(string xmlString) {
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(xmlString);
			Error error = new Error(xml.SelectSingleNode("error"));
			Debug.LogError("error from HealthKit plugin: ERROR domain:" + error.domain + " code:" + error.code + " \"" + error.localizedDescription + "\"");
			BroadcastMessage("ErrorOccurred", error);
	}


	// ------------------------------------------------------------------------------
	// Interface to native implementation
	// ------------------------------------------------------------------------------


	[DllImport ("__Internal")]
	private static extern void _InitializeNative(string controllerName);

	[DllImport ("__Internal")]
	private static extern void _Authorize(string dataTypes);

	[DllImport ("__Internal")]
	private static extern int _AuthorizationStatusForType(string dataType);

	[DllImport ("__Internal")]
	private static extern bool _IsHealthDataAvailable();

	[DllImport ("__Internal")]
	private static extern void _ReadQuantity(string identifier, string startDate, string endDate, bool combineSamples);

	[DllImport ("__Internal")]
	private static extern void _WriteQuantity(string identifier, string unitString, double doubleValue, string startDateString, string endDateString);

	[DllImport ("__Internal")]
	private static extern void _ReadCategory(string identifier, string startDate, string endDate);

	[DllImport ("__Internal")]
	private static extern void _WriteCategory(string identifier, int value, string startDateString, string endDateString);

	[DllImport ("__Internal")]
	private static extern void _ReadCharacteristic(string identifier);

	[DllImport ("__Internal")]
	private static extern void _ReadCorrelation(string identifier, string startDateString, string endDateString, bool combineSamples);

	[DllImport ("__Internal")]
	private static extern void _ReadWorkout(int activityID, string startDateString, string endDateString, bool combineSamples);

	[DllImport ("__Internal")]
	private static extern void _WriteWorkoutSimple(int activityID, string startDateString, string endDateString);

	[DllImport ("__Internal")]
	private static extern void _WriteWorkout(int activityID, string startDateString, string endDateString, double kilocaloriesBurned, double distance);

	[DllImport ("__Internal")]
	private static extern void _GenerateDummyData(string dataTypesString);

	// ------------------------

	[DllImport ("__Internal")]
	private static extern void _ReadPedometer(string startDateString, string endDateString);

	[DllImport ("__Internal")]
	private static extern void _StartReadingPedometerFromDate(string startDateString);

	[DllImport ("__Internal")]
	private static extern void _StopReadingPedometer();
}

}
