using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml;

namespace BeliefEngine.HealthKit
{

/*! @enum QuantityType
	@ingroup Enumerations
	@brief denotes whether a Quantity is cumulative, or discrete
 */
public enum QuantityType {
	cumulative,	discrete
}

/*! @brief Wrapper around HKQuantity.
 */
public class Quantity : System.Object {
	public string unit; /*!< the unit of this quantity, as a string */
	public double doubleValue; /*!< the value of this quantity, as a double */

	/*! @brief		The constructor used internally when reading health data.
		@param node	the XmlNode to create this object from.
	 */
	public Quantity(XmlNode node) {
		this.unit = node["unit"].InnerText;
		this.doubleValue = Convert.ToDouble(node["value"].InnerText);
	}

	/*! @brief				The default constructor.
		@param unitString	the string representation of the unit. For example, count, kg, or m/s^2.
							see: https://developer.apple.com/reference/healthkit/hkunit/1615733-unitfromstring
		@param value		the value of the quantity, as a double
	 */
	public Quantity(double value, string unitString) {
		this.unit = unitString;
		this.doubleValue = value;
	}

	/*! @brief convert to a reasonable string representation
	 */
	override public string ToString() {
		return string.Format("{0} {1}", this.doubleValue, this.unit);
	}
}

/*! @brief Wrapper around HKSample.
 */
public class Sample : System.Object {
	public DateTimeOffset startDate; /*!< the starting date of this sample */
	public DateTimeOffset endDate; /*!< the ending date of this sample */
	// sample type

	/*! @brief		The default constructor.
		@param node	the XmlNode to create this object from.
	 */
	public Sample(XmlNode node) {
		this.startDate = DateTimeBridge.DateFromString(node["startDate"].InnerText);
		this.endDate = DateTimeBridge.DateFromString(node["endDate"].InnerText);
	}
}


/*! @brief Wrapper around HKQuantitySample.
 */
public class QuantitySample : Sample {
	public QuantityType quantityType; /*!< the aggregation style of this sample, either cumulative or discrete */
	public Quantity quantity; /*!< the quantity */

	/*! @brief		The default constructor.
		@param node	the XmlNode to create this object from.
	 */
	public QuantitySample(XmlNode node) : base(node) {
		this.quantity = new Quantity(node["quantity"]);
		string aggregationStyle = node.SelectSingleNode("quantityType/aggregationStyle").InnerText;
		this.quantityType = (aggregationStyle == "cumulative") ? QuantityType.cumulative : QuantityType.discrete;
	}

	/*! @brief convert to a reasonable string representation
	 */
	override public string ToString() {
		return string.Format("[{0}-{1} : {2}]", this.startDate, this.endDate, this.quantity);
	}
}

/*! @brief Wrapper around HKCategorySample.
 */
public class CategorySample : Sample {
	/*! @brief 		the value of this sample
		@details	This is an int, and it's probably worth reading the [HKCategoryValueSleepAnalysis](https://developer.apple.com/library/ios/documentation/HealthKit/Reference/HealthKit_Constants/#//apple_ref/c/tdef/HKCategoryValueSleepAnalysis)
					documentation to understand what you're looking at. Basically a value of 0 means "in bed", and a 1 means "asleep".  These *will* overlap, assuming a good HealthKit citizen is writing the data.
	 */
	public int value;

	/*! @brief		The default constructor.
		@param node	the XmlNode to create this object from.
	 */
	public CategorySample(XmlNode node) : base(node) {
		this.value = Int32.Parse(node["value"].InnerText);
	}

	/*! @brief convert to a reasonable string representation
	 */
	override public string ToString() {
		return string.Format("[{0}-{1} : {2}]", this.startDate, this.endDate, this.value);
	}
}

/*! @brief Wrapper around HKCorrelationSample.
 */
public class CorrelationSample : Sample {
	public string correlationType; /*!< TODO the correlation type. */
	public List<Sample> objects; /*!< the list of samples */

	/*! @brief		The default constructor.
		@param node	the XmlNode to create this object from.
	 */
	public CorrelationSample(XmlNode node) : base(node) {
		this.correlationType = node["correlationType"].InnerText;
		this.objects = new List<Sample>();
		XmlNodeList sampleNodes = node.SelectNodes("objects");
		foreach (XmlNode sample in sampleNodes) {
			// can these be something other than Quantity Samples?
			this.objects.Add(new QuantitySample(sample));
		}
	}

	/*! @brief convert to a reasonable string representation
	 */
	override public string ToString() {
		string s = string.Format("[{0}:\n", correlationType);
		foreach (Sample sample in this.objects) {
			s = s + sample + "\n";
		}
		s = s + "]";
		return s;
	}
}

/*! @enum WorkoutEventType
	@ingroup Enumerations
	@brief denotes the type of Workout Event (Pause or Resume)
 */
public enum WorkoutEventType {
	Pause = 1,
	Resume
}

/*! @brief Wrapper around HKWorkoutEvent.
 */
public class WorkoutEvent : System.Object {
	public DateTimeOffset date; /*!< @brief time of the event */
	public WorkoutEventType eventType;	/*!< either Pause or Resume  */

	/*! @brief		The default constructor.
		@param node	the XmlNode to create this object from.
	 */
	public WorkoutEvent(XmlNode node) {
		this.date = DateTimeBridge.DateFromString(node["date"].InnerText);
		this.eventType = (WorkoutEventType)Int32.Parse(node["eventType"].InnerText);
	}
}

/*! @brief Wrapper around HKWorkoutSample.
 */
public class WorkoutSample : Sample {
	public double duration;						/*!< @brief duration of the sample, in seconds */
	public Quantity totalDistance;				/*!< @brief total distance walked/run/etc. during the workout */
	public Quantity totalEnergyBurned;			/*!< @brief total energy burned during the workout */
	public WorkoutActivityType activityType;	/*!< @brief type of the workout */
	public List<WorkoutEvent> workoutEvents; 	/*!< @brief workout events contained in this sample */

	/*! @brief		The default constructor.
		@param node	the XmlNode to create this object from.
	 */
	public WorkoutSample(XmlNode node) : base(node) {
		this.duration = Int32.Parse(node["duration"].InnerText);
		this.totalDistance = new Quantity(node["totalDistance"]);
		this.totalEnergyBurned = new Quantity(node["energyBurned"]);
		this.activityType = (WorkoutActivityType)Int32.Parse(node["activityType"].InnerText);

		this.workoutEvents = new List<WorkoutEvent>();
		XmlNodeList eventNodes = node.SelectNodes("events");
		foreach (XmlNode sample in eventNodes) {
			this.workoutEvents.Add(new WorkoutEvent(sample));
		}
	}

	// override public string ToString() {
	// 	return string.Format("[WORKOUT SAMPLE]");
	// }
}

/*! @brief Wrapper around HKCharacteristic.
 */
public class Characteristic : System.Object
{

}

/*! @brief Wrapper around HKBiologicalSexCharacteristic.
 */
public class BiologicalSexCharacteristic : Characteristic
{
	/*! @brief		the biological sex
		@details	This can either be NotSet, Male, Female, or Other.
	 */
	public BiologicalSex value;

	/*! @brief		The default constructor.
		@param node	the XmlNode to create this object from.
	 */
	public BiologicalSexCharacteristic(XmlNode node) {
		this.value = (BiologicalSex)Int32.Parse(node.InnerText);
	}

	/*! @brief string representation. */
	override public string ToString() {
		return string.Format("[biological sex:{0}]", this.value);
	}
}

/*! @brief Wrapper around HKBloodTypeCharacteristic.
 */
public class BloodTypeCharacteristic : Characteristic
{
	/*! the blood type. NotSet is also a valid value. */
	public BloodType value;

	/*! @brief		The default constructor.
		@param node	the XmlNode to create this object from.
	 */
	public BloodTypeCharacteristic(XmlNode node) {
		this.value = (BloodType)Int32.Parse(node.InnerText);
	}

	/*! @brief string representation. */
	override public string ToString() {
		return string.Format("[blood type:{0}]", this.value);
	}
}

/*! @brief Wrapper around HKDateOfBirthCharacteristic.
 */
public class DateOfBirthCharacteristic : Characteristic
{
	/*! the user's birthday. */
	public DateTimeOffset value;

	/*! @brief		The default constructor.
		@param node	the XmlNode to create this object from.
	 */
	public DateOfBirthCharacteristic(XmlNode node) {
		this.value = DateTimeBridge.DateFromString(node.InnerText);
	}

	/*! @brief string representation. */
	override public string ToString() {
		return string.Format("[date of birth:{0}]", this.value);
	}
}

/*! @brief Wrapper around HKFitzpatrickSkinTypeObject.
 */
public class FitzpatrickSkinTypeCharacteristic : Characteristic
{
	/*! the skin type. NotSet is also a valid value. */
	public FitzpatrickSkinType value;

	/*! @brief		The default constructor.
		@param node	the XmlNode to create this object from.
	 */
	public FitzpatrickSkinTypeCharacteristic(XmlNode node) {
		this.value = (FitzpatrickSkinType)Int32.Parse(node.InnerText);
	}

	/*! @brief string representation. */
	override public string ToString() {
		return string.Format("[skin type:{0}]", this.value);
	}
}

/*! @brief Wrapper around HKWheelchairUseObject.
 */
public class WheelchairUseCharacteristic : Characteristic
{
	/*! the skin type. NotSet is also a valid value. */
	public WheelchairUse value;

	/*! @brief		The default constructor.
		@param node	the XmlNode to create this object from.
	 */
	public WheelchairUseCharacteristic(XmlNode node) {
		this.value = (WheelchairUse)Int32.Parse(node.InnerText);
	}

	/*! @brief string representation. */
	override public string ToString() {
		return string.Format("[wheelchair use:{0}]", this.value);
	}
}

/*! @brief Used to send HealthKit data back to Unity.
 */
public class HealthData : System.Object {
	
	public HKDataType datatype;  /*!< @brief the type of health data */
	public WorkoutActivityType workoutType;  /*!< @brief the workout type, if applicable */
	
	private XmlDocument xml;

	/*! @brief				The default constructor.
		@param xmlString	the XML string to create this object from.
	 */
	public HealthData(string xmlString) {
		xml = new XmlDocument();
		xml.LoadXml(xmlString);
		
		XmlNode node = xml.FirstChild["datatype"];
		if (node != null && node.InnerText != null) {
			this.datatype = (HKDataType)System.Enum.Parse(typeof(HKDataType), (string)node.InnerText);
		} else {
			Debug.LogError("datatype node is missing or invalid");
		}
		
		if (this.datatype == HKDataType.HKWorkoutTypeIdentifier) {
			node = xml.FirstChild["workoutType"];
			if (node != null && node.InnerText != null) this.workoutType = (WorkoutActivityType)Enum.ToObject(typeof(WorkoutActivityType), node.InnerText);
			else Debug.LogError("workoutType node is missing or invalid");
		}
	}

	/*! @brief		The name of the root node of the XML document.
		@details	This is used to determine what kind of HealthKit data to process as.
	 */
	public string RootName() {
		return xml.DocumentElement.Name;
	}

	/*! @brief parse XML containing QuantitySamples, and return a list. */
	public List<QuantitySample> ParseQuantitySamples() {
		XmlNodeList sampleNodes = xml.SelectNodes("/quantity/quantitySample");
		List<QuantitySample> samples = new List<QuantitySample>();
		foreach (XmlNode node in sampleNodes) {
			samples.Add(new QuantitySample(node));
		}

		return samples;
	}

	/*! @brief parse XML containing CategorySamples, and return a list. */
	public List<CategorySample> ParseCategorySamples() {
		XmlNodeList sampleNodes = xml.SelectNodes("/category/categorySample");
		List<CategorySample> samples = new List<CategorySample>();
		foreach (XmlNode node in sampleNodes) {
			samples.Add(new CategorySample(node));
		}

		return samples;
	}

	/*! @brief parse XML containing a combined total of QuantitySamples, and return a double. */
	public double ParseTotal() {
		XmlNode node = xml.SelectSingleNode("total/total");
		return Double.Parse(node.InnerText);
	}
	
	/*! @brief parse XML & determine if writing was a success. */
	public bool ParseSuccess() {
		XmlNode node = xml.SelectSingleNode("write/success");
		return bool.Parse(node.InnerText);
	}

	/*! @brief if there was an error, parse & return it; otherwise null. */
	public Error ParseError() {
		XmlNode node = xml.SelectSingleNode("write/error");
		if (node != null) return new Error(node);
		else return null;
	}

	/*! @brief parse XML containing CorrelationSamples, and return a list. */
	public List<CorrelationSample> ParseCorrelationSamples() {
		XmlNodeList sampleNodes = xml.SelectNodes("/correlation/correlationSample");
		List<CorrelationSample> samples = new List<CorrelationSample>();
		foreach (XmlNode node in sampleNodes) {
			samples.Add(new CorrelationSample(node));
		}

		return samples;
	}

	/*! @brief parse XML containing WorkoutSamples, and return a list. */
	public List<WorkoutSample> ParseWorkoutSamples() {
		XmlNodeList sampleNodes = xml.SelectNodes("/workout/workoutSample");
		List<WorkoutSample> samples = new List<WorkoutSample>();
		foreach (XmlNode node in sampleNodes) {
			samples.Add(new WorkoutSample(node));
		}

		return samples;
	}

	/*! @brief parse XML containing a Characteristic, and return it. */
	public Characteristic ParseCharacteristic() {
		foreach (XmlNode node in xml["characteristic"].ChildNodes) {
			switch (node.Name) {
			case "sex":
				return new BiologicalSexCharacteristic(node);
			case "bloodType":
				return new BloodTypeCharacteristic(node);
			case "skinType":
				return new FitzpatrickSkinTypeCharacteristic(node);
			case "wheelchairUse":
				return new WheelchairUseCharacteristic(node);
			case "DOB":
				return new DateOfBirthCharacteristic(node);
			case "datatype":
				continue;
			default:
				Debug.LogErrorFormat("Error: unrecognized characteristic '{0}'", node.Name);
				break;
		}
		}
		
		return null;
	}

	/*! @brief parse XML containing pedometer data, and return it. */
	public List<PedometerData> ParsePedometerData() {
		XmlNodeList sampleNodes = xml.SelectNodes("/pedometer/pedometerData");
		List<PedometerData> samples = new List<PedometerData>();
		foreach (XmlNode node in sampleNodes) {
			samples.Add(new PedometerData(node));
		}

		return samples;
	}
}

/*! @brief Wrapper around CMPedometerData.
 */
public class PedometerData : System.Object {
	public DateTimeOffset startDate; /*!< the starting date of this sample */
	public DateTimeOffset endDate; /*!< the ending date of this sample */
	public int numberOfSteps; /*!< the number of steps taken in this sample */
	// sample type

	/*! @brief		The default constructor.
		@param node	the XmlNode to create this object from.
	 */
	public PedometerData(XmlNode node) {
		this.startDate = DateTimeBridge.DateFromString(node["startDate"].InnerText);
		this.endDate = DateTimeBridge.DateFromString(node["endDate"].InnerText);
		this.numberOfSteps = Convert.ToInt32(node["numberOfSteps"].InnerText);
	}
}

}
