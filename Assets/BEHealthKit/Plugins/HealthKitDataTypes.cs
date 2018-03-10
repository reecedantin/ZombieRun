using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BeliefEngine.HealthKit
{

/*!
	@defgroup Enumerations Enumerations
	Public enumeration types
*/

/*! @enum HKAuthorizationStatus
	@ingroup Enumerations
	@brief Identifiers for the health data sharing authorization.
 */
public enum HKAuthorizationStatus {
	NotDetermined,
    SharingDenied,
    SharingAuthorized
}

/*! @enum HKDataType
	@ingroup Enumerations
	@brief Identifiers for the datatypes that HealthKit supports.
 */
public enum HKDataType {
	/*--------------------------------*/
	/*   HKQuantityType Identifiers   */
	/*--------------------------------*/

	// Body Measurements
	HKQuantityTypeIdentifierBodyMassIndex,              // Scalar(Count),               Discrete
	HKQuantityTypeIdentifierBodyFatPercentage,          // Scalar(Percent, 0.0 - 1.0),  Discrete
	HKQuantityTypeIdentifierHeight,                     // Length,                      Discrete
	HKQuantityTypeIdentifierBodyMass,                   // Mass,                        Discrete
	HKQuantityTypeIdentifierLeanBodyMass,               // Mass,                        Discrete

	// Fitness
	HKQuantityTypeIdentifierStepCount,                  // Scalar(Count),               Cumulative
	HKQuantityTypeIdentifierDistanceWalkingRunning,     // Length,                      Cumulative
	HKQuantityTypeIdentifierDistanceCycling,            // Length,                      Cumulative
	HKQuantityTypeIdentifierDistanceWheelchair,         // Length,                      Cumulative  - iOS 10.0
	HKQuantityTypeIdentifierBasalEnergyBurned,          // Energy,                      Cumulative
	HKQuantityTypeIdentifierActiveEnergyBurned,         // Energy,                      Cumulative
	HKQuantityTypeIdentifierFlightsClimbed,             // Scalar(Count),               Cumulative
	HKQuantityTypeIdentifierNikeFuel,                   // Scalar(Count),               Cumulative
	HKQuantityTypeIdentifierAppleExerciseTime,			// Time							Cumulative	- iOS 9.3
	HKQuantityTypeIdentifierPushCount,                  // Scalar(Count),               Cumulative  - iOS 10.0
	HKQuantityTypeIdentifierDistanceSwimming,           // Length,                      Cumulative  - iOS 10.0
	HKQuantityTypeIdentifierSwimmingStrokeCount,        // Scalar(Count),               Cumulative  - iOS 10.0
	// Vitals
	HKQuantityTypeIdentifierHeartRate,                  // Scalar(Count)/Time,          Discrete
	HKQuantityTypeIdentifierBodyTemperature,            // Temperature,                 Discrete
	HKQuantityTypeIdentifierBasalBodyTemperature,		// Basal Body Temperature,		Discrete	- iOS 9.0
	HKQuantityTypeIdentifierBloodPressureSystolic,      // Pressure,                    Discrete
	HKQuantityTypeIdentifierBloodPressureDiastolic,     // Pressure,                    Discrete
	HKQuantityTypeIdentifierRespiratoryRate,            // Scalar(Count)/Time,          Discrete

	// Results
	HKQuantityTypeIdentifierOxygenSaturation,           // Scalar (Percent, 0.0 - 1.0,  Discrete
	HKQuantityTypeIdentifierPeripheralPerfusionIndex,   // Scalar(Percent, 0.0 - 1.0),  Discrete
	HKQuantityTypeIdentifierBloodGlucose,               // Mass/Volume,                 Discrete
	HKQuantityTypeIdentifierNumberOfTimesFallen,        // Scalar(Count),               Cumulative
	HKQuantityTypeIdentifierElectrodermalActivity,      // Conductance,                 Discrete
	HKQuantityTypeIdentifierInhalerUsage,               // Scalar(Count),               Cumulative
	HKQuantityTypeIdentifierBloodAlcoholContent,        // Scalar(Percent, 0.0 - 1.0),  Discrete
	HKQuantityTypeIdentifierForcedVitalCapacity,        // Volume,                      Discrete
	HKQuantityTypeIdentifierForcedExpiratoryVolume1,    // Volume,                      Discrete
	HKQuantityTypeIdentifierPeakExpiratoryFlowRate,     // Volume/Time,                 Discrete

	// Nutrition
	HKQuantityTypeIdentifierDietaryFatTotal,            // Mass,   						Cumulative
	HKQuantityTypeIdentifierDietaryFatPolyunsaturated,  // Mass,   						Cumulative
	HKQuantityTypeIdentifierDietaryFatMonounsaturated,  // Mass,   						Cumulative
	HKQuantityTypeIdentifierDietaryFatSaturated,        // Mass,   						Cumulative
	HKQuantityTypeIdentifierDietaryCholesterol,         // Mass,   						Cumulative
	HKQuantityTypeIdentifierDietarySodium,              // Mass,   						Cumulative
	HKQuantityTypeIdentifierDietaryCarbohydrates,       // Mass,   						Cumulative
	HKQuantityTypeIdentifierDietaryFiber,               // Mass,   						Cumulative
	HKQuantityTypeIdentifierDietarySugar,               // Mass,   						Cumulative
	HKQuantityTypeIdentifierDietaryEnergyConsumed,      // Energy, 						Cumulative
	HKQuantityTypeIdentifierDietaryProtein,             // Mass,   						Cumulative

	HKQuantityTypeIdentifierDietaryVitaminA,            // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryVitaminB6,           // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryVitaminB12,          // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryVitaminC,            // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryVitaminD,            // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryVitaminE,            // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryVitaminK,            // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryCalcium,             // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryIron,                // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryThiamin,             // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryRiboflavin,          // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryNiacin,              // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryFolate,              // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryBiotin,              // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryPantothenicAcid,     // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryPhosphorus,          // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryIodine,              // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryMagnesium,           // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryZinc,                // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietarySelenium,            // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryCopper,              // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryManganese,           // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryChromium,            // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryMolybdenum,          // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryChloride,            // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryPotassium,           // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryCaffeine,            // Mass, 						Cumulative
	HKQuantityTypeIdentifierDietaryWater,				// Volume, 						Cumulative	- iOS 9.0

	HKQuantityTypeIdentifierUVExposure,					// Scalar(Count),				Discrete	- iOS 9.0

	/*--------------------------------*/
	/*   HKCategoryType Identifiers   */
	/*--------------------------------*/

	HKCategoryTypeIdentifierSleepAnalysis,  			// HKCategoryValueSleepAnalysis
	HKCategoryTypeIdentifierAppleStandHour, 			// HKCategoryValueAppleStandHour			- iOS 9.0
	HKCategoryTypeIdentifierCervicalMucusQuality,		// HKCategoryValueCervicalMucusQuality		- iOS 9.0
	HKCategoryTypeIdentifierOvulationTestResult,		// HKCategoryValueOvulationTestResult		- iOS 9.0
	HKCategoryTypeIdentifierMenstrualFlow,				// HKCategoryValueMenstrualFlow				- iOS 9.0
	HKCategoryTypeIdentifierIntermenstrualBleeding,		// (Spotting) HKCategoryValue				- iOS 9.0
	HKCategoryTypeIdentifierSexualActivity,				// HKCategoryValue							- iOS 9.0
	HKCategoryTypeIdentifierMindfulSession,             // HKCategoryValue                          - iOS 10.0

	/*--------------------------------------*/
	/*   HKCharacteristicType Identifiers   */
	/*--------------------------------------*/

	HKCharacteristicTypeIdentifierBiologicalSex,		// HKCharacteristicBiologicalSex
	HKCharacteristicTypeIdentifierBloodType,			// HKCharacteristicBloodType
	HKCharacteristicTypeIdentifierDateOfBirth,			// NSDate
	HKCharacteristicTypeIdentifierFitzpatrickSkinType,	// FitzpatrickSkinType					- iOS 9.0
	HKCharacteristicTypeIdentifierWheelchairUse,        // WheelchairUseObject                    - iOS 10.0

	/*-----------------------------------*/
	/*   HKCorrelationType Identifiers   */
	/*-----------------------------------*/

	HKCorrelationTypeIdentifierBloodPressure,
	HKCorrelationTypeIdentifierFood,

	/*--------------------------------*/
	/*   HKDocumentType Identifiers   */
	/*--------------------------------*/

	HKDocumentTypeIdentifierCDA,                        //  - iOS 10.0
	
	/*------------------------------*/
	/*   HKWorkoutType Identifier   */
	/*------------------------------*/

	HKWorkoutTypeIdentifier,

	NUM_TYPES
};

/*! @enum WorkoutActivityType
	@ingroup Enumerations
	@brief Identifiers for the types of workouts that HealthKit supports.
 */
public enum WorkoutActivityType {
	AmericanFootball = 1,
	Archery,
	AustralianFootball,
	Badminton,
	Baseball,
	Basketball,
	Bowling,
	Boxing, // Kickboxing, Boxing, etc.
	Climbing,
	Cricket,
	CrossTraining, // Any mix of cardio and/or strength and/or flexibility
	Curling,
	Cycling,
	Dance,
	DanceInspiredTraining, // Pilates, Barre, Feldenkrais, etc.
	Elliptical,
	EquestrianSports, // Polo, Horse Racing, Horse Riding, etc.
	Fencing,
	Fishing,
	FunctionalStrengthTraining, // Primarily free weights and/or body weight and/or accessories
	Golf,
	Gymnastics,
	Handball,
	Hiking,
	Hockey, // Ice Hockey, Field Hockey, etc.
	Hunting,
	Lacrosse,
	MartialArts,
	MindAndBody, // Tai chi, meditation, etc.
	MixedMetabolicCardioTraining, // Any mix of cardio-focused exercises
	PaddleSports, // Canoeing, Kayaking, Outrigger, Stand Up Paddle Board, etc.
	Play, // Dodge Ball, Hopscotch, Tetherball, Jungle Gym, etc.
	PreparationAndRecovery, // Foam rolling, stretching, etc.
	Racquetball,
	Rowing,
	Rugby,
	Running,
	Sailing,
	SkatingSports, // Ice Skating, Speed Skating, Inline Skating, Skateboarding, etc.
	SnowSports, // Skiing, Snowboarding, Cross-Country Skiing, etc.
	Soccer,
	Softball,
	Squash,
	StairClimbing,
	SurfingSports, // Traditional Surfing, Kite Surfing, Wind Surfing, etc.
	Swimming,
	TableTennis,
	Tennis,
	TrackAndField, // Shot Put, Javelin, Pole Vaulting, etc.
	TraditionalStrengthTraining, // Primarily machines and/or free weights
	Volleyball,
	Walking,
	WaterFitness,
	WaterPolo,
	WaterSports, // Water Skiing, Wake Boarding, etc.
	Wrestling,
	Yoga,

	Other = 3000,
}

/*! @enum CategoryTypeIdentifier
	@ingroup Enumerations
	@brief Identifiers for the category-type data that HealthKit supports.
 */
public enum CategoryTypeIdentifier {
	SleepAnalysis,
	AppleStandHour,
	CervicalMucusQuality,
	OvulationTestResult,
	MenstrualFlow,
	IntermenstrualBleeding,
	SexualActivity
}

/*! @enum SleepAnalysis
	@ingroup Enumerations
	@brief Identifiers for sleep analysis data. Possible values are "InBed" (0) and "Asleep" (1).
 */
public enum SleepAnalysis {
   InBed = 0,
   Asleep
}

/*! @enum AppleStandHour
	@ingroup Enumerations
	@brief Identifiers for whether the user has stood for at least one minute during the sample.
 */
public enum AppleStandHour {
   Stood = 0,
   Idle
}

/*! @enum CervicalMucusQuality
	@ingroup Enumerations
	@brief Identifiers for representing the quality of the user’s cervical mucus.
 */
public enum CervicalMucusQuality {
   Dry = 1,
   Sticky,
   Creamy,
   Watery,
   EggWhite
}

/*! @enum OvulationTestResult
	@ingroup Enumerations
	@brief Identifiers for recording the result of an ovulation home test.
 */
public enum OvulationTestResult {
   Negative = 1,
   Positive,
   Indeterminate
}

/*! @enum MenstrualFlow
	@ingroup Enumerations
	@brief Identifiers for representing menstrual cycles.
 */
public enum MenstrualFlow {
   Unspecified = 0,
   Light,
   Medium,
   Heavy
}

// --------------------------------------------

/*! @enum BiologicalSex
	@ingroup Enumerations
	@brief possible values for Biological Sex
 */
public enum BiologicalSex {
	NotSet = 0,
	Female,
	Male,
	Other
}

/*! @enum BloodType
	@ingroup Enumerations
	@brief possible values for BloodType
 */
public enum BloodType {
	NotSet = 0,
	APositive,
	ANegative,
	BPositive,
	BNegative,
	ABPositive,
	ABNegative,
	OPositive,
	ONegative
}

/*! @enum FitzpatrickSkinType
	@ingroup Enumerations
	@brief possible values for Fitzpatrick Skin Type
 */
public enum FitzpatrickSkinType {
	FitzpatrickSkinTypeNotSet = 0,
	FitzpatrickSkinTypeI,
	FitzpatrickSkinTypeII,
	FitzpatrickSkinTypeIII,
	FitzpatrickSkinTypeIV,
	FitzpatrickSkinTypeV,
	FitzpatrickSkinTypeVI
}

/*! @enum WheelchairUse
	@ingroup Enumerations
	@brief possible values for Wheelchair Use
 */
public enum WheelchairUse {
	WheelchairUseNotSet = 0,
    WheelchairUseNo,
    WheelchairUseYes
}


// --------------------------------------------


/*!	@brief Storage class for HealthKit data types. Used to create the editor UI & authorization.
 */
[System.Serializable]
public class HKNameValuePair : System.Object {
	public string name;   /*!< human-readable name of the HKDataType */
	public bool read;     /*!< read permission? */
	public bool write;    /*!< write permission? */
	public bool writable; /*!< is writing allowed? */

	/*!	@brief default constructor
	 */
	public HKNameValuePair(string n, bool w) {
		this.name = n;
		this.read = false;
		this.write = false;
		this.writable = w;
	}
}

// --------------------------------------------


/*!	@brief HealthKit data types to authorize. Used to create the editor UI.
 */
[ExecuteInEditMode]
public class HealthKitDataTypes : MonoBehaviour
{
	/*! @brief serializable representation of the data types to read/write.  */
	public string saveData = null;

	/*! @brief Text to present to the user when iOS requests access to read health data.  */
	public string healthShareUsageDescription = "We require access to health data for testing.";
	
	/*! @brief Text to present to the user when iOS requests access to write health data. */
	public string healthUpdateUsageDescription = "We update health data for testing.";
	
	/*! @brief dictionary of identifier/read+write values */
	public Dictionary<string, HKNameValuePair> data; 

	void Awake() {
		// this.data.Load();
	}

	void OnEnable() {
		Load();
	}

	void OnDisable() {
		// Save();
	}

	private void InitializeEntry(HKDataType type, string typeName, bool writable = true) {
		string key = GetIdentifier(type);
		if (!data.ContainsKey(key)) {
			data[key] = new HKNameValuePair(typeName, writable);
		}
	}

	/*! @brief Convenience method to get a HKDataType as a string value
	 */
	public static string GetIdentifier(HKDataType type) {
		return Enum.GetName(typeof(HKDataType), type);
	}

	private void Initialize() {
		Debug.Log("[initializing]");
		// Body Measurements
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierBodyMassIndex, "Body Mass Index");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierBodyFatPercentage, "Body Fat Percentage");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierHeight, "Height");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierBodyMass, "Body Mass");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierLeanBodyMass, "Lean Body Mass");

		// Fitness
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierStepCount, "Step Count");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDistanceWalkingRunning, "Walking/Running Distance");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDistanceCycling, "Cycling Distance");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDistanceWheelchair, "Wheelchair Distance");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierBasalEnergyBurned, "Basal Energy Burned");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierActiveEnergyBurned, "Active Energy Burned");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierFlightsClimbed, "Flights Climbed");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierNikeFuel, "Nike Fuel", false);
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierAppleExerciseTime, "Apple Exercise Time", false);
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierPushCount, "Wheelchair Push Count");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDistanceSwimming, "Swimming Distance");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierSwimmingStrokeCount, "Swimming Stroke Count");

		// Vitals
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierHeartRate, "Heart Rate");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierBodyTemperature, "Body Temperature");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierBasalBodyTemperature, "Basal Body Temperature");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierBloodPressureSystolic, "Systolic Blood Pressure");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierBloodPressureDiastolic, "Diastolic Blood Pressure");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierRespiratoryRate, "Respiratory Rate");

		// Results
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierOxygenSaturation, "Oxygen Saturation");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierPeripheralPerfusionIndex, "Peripheral Perfusion Index");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierBloodGlucose, "Blood Glucose");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierNumberOfTimesFallen, "Number of Times Fallen");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierElectrodermalActivity, "Electrodermal Activity");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierInhalerUsage, "Inhaler Usage");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierBloodAlcoholContent, "Blood Alcohol Content");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierForcedVitalCapacity, "Forced Vital Capacity");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierForcedExpiratoryVolume1, "Forced Expiratory Volume");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierPeakExpiratoryFlowRate, "Peak Expiratory Flow Rate");

		// Nutrition
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryFatTotal, "Dietary Fat Total");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryFatPolyunsaturated, "Dietary Fat (polyunsaturated)");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryFatMonounsaturated, "Dietary Fat (monounsaturated)");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryFatSaturated, "Dietary Fat (saturated)");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryCholesterol, "Dietary Cholesterol");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietarySodium, "Dietary Sodium");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryCarbohydrates, "Dietary Carbohydrates");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryFiber, "Dietary Fiber");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietarySugar, "Dietary Sugar");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryEnergyConsumed, "Dietary Energy Consumed");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryProtein, "Dietary Protein");

		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryVitaminA, "Vitamin A");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryVitaminB6, "Vitamin B6");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryVitaminB12, "Vitamin B12");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryVitaminC, "Vitamin C");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryVitaminD, "Vitamin D");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryVitaminE, "Vitamin E");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryVitaminK, "Vitamin K");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryCalcium, "Dietary Calcium");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryIron, "Dietary Iron");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryThiamin, "Dietary Thiamin");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryRiboflavin, "Dietary Riboflavin");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryNiacin, "Dietary Niacin");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryFolate, "Dietary Folate");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryBiotin, "Dietary Biotin");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryPantothenicAcid, "Dietary Pantothenic Acid");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryPhosphorus, "Dietary Phosphorus");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryIodine, "Dietary Iodine");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryMagnesium, "Dietary Magnesium");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryZinc, "Dietary Zinc");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietarySelenium, "Dietary Selenium");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryCopper, "Dietary Copper");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryManganese, "Dietary Manganese");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryChromium, "Dietary Chromium");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryMolybdenum, "Dietary Molybdenum");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryChloride, "Dietary Chloride");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryPotassium, "Dietary Potassium");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryCaffeine, "Caffeine");
		InitializeEntry(HKDataType.HKQuantityTypeIdentifierDietaryWater, "Dietary Water");

		InitializeEntry(HKDataType.HKQuantityTypeIdentifierUVExposure, "UV Exposure");

		/*--------------------------------*/
		/*   HKCategoryType Identifiers   */
		/*--------------------------------*/

		InitializeEntry(HKDataType.HKCategoryTypeIdentifierSleepAnalysis, "Sleep Analysis");
		InitializeEntry(HKDataType.HKCategoryTypeIdentifierAppleStandHour, "Hours Standing", false);
		InitializeEntry(HKDataType.HKCategoryTypeIdentifierCervicalMucusQuality, "Cervical Mucus Quality");
		InitializeEntry(HKDataType.HKCategoryTypeIdentifierOvulationTestResult, "Ovulation Test Result");
		InitializeEntry(HKDataType.HKCategoryTypeIdentifierMenstrualFlow, "Menstrual Flow");
		InitializeEntry(HKDataType.HKCategoryTypeIdentifierIntermenstrualBleeding, "Intermenstrual Bleeding");
		InitializeEntry(HKDataType.HKCategoryTypeIdentifierSexualActivity, "Sexual Activity");
		InitializeEntry(HKDataType.HKCategoryTypeIdentifierMindfulSession, "Mindful Session");

		/*--------------------------------------*/
		/*   HKCharacteristicType Identifiers   */
		/*--------------------------------------*/

		InitializeEntry(HKDataType.HKCharacteristicTypeIdentifierBiologicalSex, "Biological Sex", false);
		InitializeEntry(HKDataType.HKCharacteristicTypeIdentifierBloodType, "Blood Type", false);
		InitializeEntry(HKDataType.HKCharacteristicTypeIdentifierDateOfBirth, "Date of Birth", false);
		InitializeEntry(HKDataType.HKCharacteristicTypeIdentifierFitzpatrickSkinType, "Fitzpatrick Skin Type", false);
		InitializeEntry(HKDataType.HKCharacteristicTypeIdentifierWheelchairUse, "Wheelchair use", false);

		/*-----------------------------------*/
		/*   HKCorrelationType Identifiers   */
		/*-----------------------------------*/

		InitializeEntry(HKDataType.HKCorrelationTypeIdentifierBloodPressure, "Blood Pressure");
		InitializeEntry(HKDataType.HKCorrelationTypeIdentifierFood, "Food");

		/*--------------------------------*/
		/*   HKDocumentType Identifiers   */
		/*--------------------------------*/

		InitializeEntry(HKDataType.HKDocumentTypeIdentifierCDA, "CDA Document");

		/*------------------------------*/
		/*   HKWorkoutType Identifier   */
		/*------------------------------*/

		InitializeEntry(HKDataType.HKWorkoutTypeIdentifier, "Workout Type");
	}

	// -----------------------------

	/*! @brief Save the authorization preferences.
	 */
	public string Save() {
		string newSaveData = this.saveData;

		#if UNITY_EDITOR
		Debug.Log("[EDITOR] save");
		if (this.data != null) {
			Debug.Log("-- have data to save");
			using (MemoryStream stream = new MemoryStream()) {
				BinaryFormatter bin = new BinaryFormatter();
				Debug.LogFormat("bin.Serialize({0}, {1})", stream, this.data);
				bin.Serialize(stream, this.data);
				string text = Convert.ToBase64String(stream.ToArray());
				newSaveData = text;
			}
		} else {
			Debug.Log("--- NO data to save");
		}
		#endif

		return newSaveData;
	}

	/*! @brief Load the authorization preferences from a supplied file.
		@param saveAsset the save data asset
	 */
	public void Load(TextAsset saveAsset) {
		byte[] bytes = Convert.FromBase64String(saveAsset.text);
		using (MemoryStream stream = new MemoryStream(bytes)) {
			BinaryFormatter bin = new BinaryFormatter();
			this.data = (Dictionary<string, HKNameValuePair>)bin.Deserialize(stream);
		}
	}

	/*! @brief Load the authorization preferences.
	 */
	public void Load() {
		if (this.saveData != null) {
			byte[] bytes = Convert.FromBase64String(this.saveData);
			using (MemoryStream stream = new MemoryStream(bytes)) {
				BinaryFormatter bin = new BinaryFormatter();
				this.data = (Dictionary<string, HKNameValuePair>)bin.Deserialize(stream);
				return;
			}
		}

		// something went wrong
		this.data = new Dictionary<string, HKNameValuePair>();
		Initialize();
	}

	// -----------------------------

	/*! @brief Create a comma-separated list of HealthKit datatypes to authorize */
	public string Transmit() {
		List<string> readList = new List<string>();
		foreach (KeyValuePair<string, HKNameValuePair> pair in this.data) {
			if (pair.Value.read) readList.Add(pair.Key);
		}

		List<string> writeList = new List<string>();
		foreach (KeyValuePair<string, HKNameValuePair> pair in this.data) {
			if (pair.Value.write) writeList.Add(pair.Key);
		}

		return String.Join(",", readList.ToArray()) + "|" + String.Join(",", writeList.ToArray());
	}

	/*! @brief Returns true if there are data types we want to read, and need to request permission to read health data. */
	public bool AskForSharePermission() {
		foreach (KeyValuePair<string, HKNameValuePair> pair in this.data) {
			if (pair.Value.read) return true;
		}
		return false;
	}

	/*! @brief Returns true if there are data types we want to write, and need to request permission to write health data. */
	public bool AskForUpdatePermission() {
		foreach (KeyValuePair<string, HKNameValuePair> pair in this.data) {
			if (pair.Value.write) return true;
		}
		return false;
	}
}

}