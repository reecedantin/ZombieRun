using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace BeliefEngine.HealthKit
{

/*! @brief Property drawer for HealthKitDataTypes */
[CustomEditor (typeof (HealthKitDataTypes))]
public class HealthKitDataTypesEditor : Editor
{
	private HealthKitDataTypes obj;

	private bool bodyMeasurementSection = true;
	private bool fitnessSection = true;
	private bool vitalsSection = true;
	private bool resultsSection = true;
	private bool nutritionSection = true;

	private bool categorySection = true;
	private bool characteristicSection = true;
	private bool correlationSection = true;
	private bool otherSection = true;

	void Awake() {
		obj = (HealthKitDataTypes)target;
	}


	private SerializedProperty saveDataProperty;
	private SerializedProperty usageStringProperty;
	private SerializedProperty updateStringProperty;
	void OnEnable() {
		this.saveDataProperty = serializedObject.FindProperty("saveData");
		this.usageStringProperty = serializedObject.FindProperty("healthShareUsageDescription");
		this.updateStringProperty = serializedObject.FindProperty("healthUpdateUsageDescription");
	}

	/*! @brief draws the GUI */
	public override void OnInspectorGUI() {
		serializedObject.Update();
		
		GUILayout.BeginVertical();

		EditorGUILayout.PropertyField(usageStringProperty, new GUIContent("Health Share Usage"), null);
		EditorGUILayout.PropertyField(updateStringProperty, new GUIContent("Health Update Usage"), null);

		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Datatype", EditorStyles.boldLabel, GUILayout.MaxWidth(240));
		EditorGUILayout.LabelField("read", EditorStyles.boldLabel, GUILayout.MaxWidth(40));
		EditorGUILayout.LabelField("write", EditorStyles.boldLabel, GUILayout.MaxWidth(40));
		GUILayout.EndHorizontal();

		bodyMeasurementSection = EditorGUILayout.Foldout(bodyMeasurementSection, "Body Measurements");
		if (bodyMeasurementSection) {
			DrawDataTypes(HKDataType.HKQuantityTypeIdentifierBodyMassIndex, HKDataType.HKQuantityTypeIdentifierLeanBodyMass);
		}
		fitnessSection = EditorGUILayout.Foldout(fitnessSection, "Fitness");
		if (fitnessSection) {
			DrawDataTypes(HKDataType.HKQuantityTypeIdentifierStepCount, HKDataType.HKQuantityTypeIdentifierSwimmingStrokeCount);
		}
		vitalsSection = EditorGUILayout.Foldout(vitalsSection, "Vitals");
		if (vitalsSection) {
			DrawDataTypes(HKDataType.HKQuantityTypeIdentifierHeartRate, HKDataType.HKQuantityTypeIdentifierRespiratoryRate);
		}
		resultsSection = EditorGUILayout.Foldout(resultsSection, "Results");
		if (resultsSection) {
			DrawDataTypes(HKDataType.HKQuantityTypeIdentifierOxygenSaturation, HKDataType.HKQuantityTypeIdentifierPeakExpiratoryFlowRate);
		}
		nutritionSection = EditorGUILayout.Foldout(nutritionSection, "Nutrition");
		if (nutritionSection) {
			DrawDataTypes(HKDataType.HKQuantityTypeIdentifierDietaryFatTotal, HKDataType.HKQuantityTypeIdentifierDietaryWater);
		}
		categorySection = EditorGUILayout.Foldout(categorySection, "Categories");
		if (categorySection) {
			DrawDataTypes(HKDataType.HKCategoryTypeIdentifierSleepAnalysis, HKDataType.HKCategoryTypeIdentifierMindfulSession);
		}
		characteristicSection = EditorGUILayout.Foldout(characteristicSection, "Characteristics");
		if (characteristicSection) {
			DrawDataTypes(HKDataType.HKCharacteristicTypeIdentifierBiologicalSex, HKDataType.HKCharacteristicTypeIdentifierWheelchairUse);
		}
		correlationSection = EditorGUILayout.Foldout(correlationSection, "Correlations");
		if (correlationSection) {
			DrawDataTypes(HKDataType.HKCorrelationTypeIdentifierBloodPressure, HKDataType.HKCorrelationTypeIdentifierFood);
		}
		otherSection = EditorGUILayout.Foldout(otherSection, "Other");
		if (otherSection) {
			DrawDataType(HKDataType.HKQuantityTypeIdentifierUVExposure);
			DrawDataType(HKDataType.HKWorkoutTypeIdentifier);
		}

		GUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties();
	}

	private void DrawDataTypes(HKDataType startType, HKDataType endType) {
		for (int i = (int)startType; i <= (int)endType; i++) {
			DrawDataType((HKDataType)i);
		}
	}

	private void DrawDataType(HKDataType dataType) {
		GUILayout.BeginHorizontal();
		Dictionary<string, HKNameValuePair> data = obj.data;
		if (data != null) {
			string key = HealthKitDataTypes.GetIdentifier(dataType);
			if (data.ContainsKey(key)) {
				EditorGUILayout.LabelField(data[key].name, GUILayout.MaxWidth(240));

				EditorGUI.BeginChangeCheck();
				bool readValue = EditorGUILayout.Toggle(data[key].read, GUILayout.MaxWidth(40));
				if (EditorGUI.EndChangeCheck()) {
					data[key].read = readValue;
					string saveData = obj.Save();
					this.saveDataProperty.stringValue = saveData;
				}

				if (!data[key].writable) GUI.enabled = false;

				EditorGUI.BeginChangeCheck();
				bool writeValue = EditorGUILayout.Toggle(data[key].write, GUILayout.MaxWidth(40));
				if (EditorGUI.EndChangeCheck()) {
					data[key].write = writeValue;
					// EditorUtility.SetDirty(prop.serializedObject.targetObject);
					string saveData = obj.Save();
					this.saveDataProperty.stringValue = saveData;
				}

				GUI.enabled = true;
			} else {
				EditorGUILayout.LabelField(key, GUILayout.MaxWidth(240));
				EditorGUILayout.LabelField("ERROR", GUILayout.MaxWidth(80));
			}
		}
		GUILayout.EndHorizontal();
	}
}

}