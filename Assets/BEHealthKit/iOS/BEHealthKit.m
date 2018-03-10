//
//  BEHealthKit.m
//  Unity-iPhone
//
//  Created by greay on 3/25/15.
//
//

#import "BEHealthKit.h"
#import <HealthKit/HealthKit.h>
#import "BEHealthKit+Unity.h"
#import "NSError+XML.h"

// @cond INTERNAL
@interface BEHealthKit ()

@property (nonatomic, assign) NSUInteger anchor;

@end
// @endcond

// -----------------------------------

@implementation BEHealthKit

+ (instancetype)sharedHealthKit
{
	static dispatch_once_t onceToken;
	static BEHealthKit *healthKit = nil;
	dispatch_once(&onceToken, ^{
		healthKit = [[self alloc] init];
	});
	return healthKit;
}


- (instancetype)init {
	if (self = [super init]) {
		self.healthStore = [[HKHealthStore alloc] init];
		self.anchor = 0;
	}
	return self;
}

- (void)authorizeHealthKitToRead:(NSArray *)readIdentifiers write:(NSArray *)writeIdentifiers completion:(void (^)(bool success, NSError *error))completion {
	if ([HKHealthStore isHealthDataAvailable]) {
		NSSet *healthKitTypesToRead = [self dataTypes:readIdentifiers writePermission:false];
		NSSet *healthKitTypesToWrite = [self dataTypes:writeIdentifiers writePermission:true];
		[self.healthStore requestAuthorizationToShareTypes:healthKitTypesToWrite readTypes:healthKitTypesToRead completion:^(BOOL success, NSError *error) {
			if( completion != nil ) {
				completion(success, error);
			}
		}];
	} else {
		NSLog(@"error; Health Store is unavailable");
		NSError *err = [NSError errorWithDomain:@"beliefengine" code:1001 userInfo:@{NSLocalizedDescriptionKey:@"HealthKit is not available on this device."}];
		[self errorOccurred:err];
	}
}


- (int)authorizationStatusForType:(NSString *)dataTypeString {
	HKObjectType *datatype = [[self dataTypes:@[dataTypeString] writePermission:false] anyObject];
	HKAuthorizationStatus status = [self.healthStore authorizationStatusForType:datatype];
	return (int)status;
}


- (void)readSamples:(HKSampleType *)sampleType fromDate:(NSDate *)startDate toDate:(NSDate *)endDate resultsHandler:(void (^)(NSArray *results, NSError *error))resultsHandler
{
	NSDate *methodStart = [NSDate date];

	NSPredicate *predicate = [HKQuery predicateForSamplesWithStartDate:startDate endDate:endDate options:HKQueryOptionStrictStartDate];
	NSSortDescriptor *sortDescriptor = [NSSortDescriptor sortDescriptorWithKey:HKSampleSortIdentifierStartDate ascending:YES];

	HKSampleQuery *sampleQuery = [[HKSampleQuery alloc] initWithSampleType:sampleType
																 predicate:predicate
																	 limit:HKObjectQueryNoLimit
														   sortDescriptors:@[sortDescriptor]
															resultsHandler:^(HKSampleQuery *query, NSArray *results, NSError *error) {
																NSDate *methodFinish = [NSDate date];
																NSTimeInterval executionTime = [methodFinish timeIntervalSinceDate:methodStart];
																NSLog(@"--- querying HealthKit took %f seconds ---", executionTime);
																resultsHandler(results, error);
															}];

	[self.healthStore executeQuery:sampleQuery];

}

- (void)readSamplesForWorkoutActivity:(HKWorkoutActivityType)activity fromDate:(NSDate *)startDate toDate:(NSDate *)endDate resultsHandler:(void (^)(NSArray *results, NSError *error))resultsHandler
{
	NSDate *methodStart = [NSDate date];

	NSPredicate *activityPredicate = [HKQuery predicateForWorkoutsWithWorkoutActivityType:activity];
	NSPredicate *rangePredicate = [HKQuery predicateForSamplesWithStartDate:startDate endDate:endDate options:HKQueryOptionStrictStartDate];
	NSPredicate *predicate = [NSCompoundPredicate andPredicateWithSubpredicates:@[activityPredicate, rangePredicate]];

	NSSortDescriptor *sortDescriptor = [NSSortDescriptor sortDescriptorWithKey:HKSampleSortIdentifierStartDate ascending:YES];

	HKSampleQuery *sampleQuery = [[HKSampleQuery alloc] initWithSampleType:[HKWorkoutType workoutType]
																 predicate:predicate
																	 limit:HKObjectQueryNoLimit
														   sortDescriptors:@[sortDescriptor]
															resultsHandler:^(HKSampleQuery *query, NSArray *results, NSError *error) {
																NSDate *methodFinish = [NSDate date];
																NSTimeInterval executionTime = [methodFinish timeIntervalSinceDate:methodStart];
																NSLog(@"--- querying HealthKit took %f seconds ---", executionTime);
																resultsHandler(results, error);
															}];

	[self.healthStore executeQuery:sampleQuery];

}
- (void)readCharacteristic:(HKCharacteristicType *)characteristic resultsHandler:(void (^)(id result, NSError *error))resultsHandler
{
	NSDate *methodStart = [NSDate date];

	id result = nil;
	NSError *error = nil;

	if ([characteristic.identifier isEqualToString:HKCharacteristicTypeIdentifierBiologicalSex]) {
		result = [self.healthStore biologicalSexWithError:&error];
	} else if ([characteristic.identifier isEqualToString:HKCharacteristicTypeIdentifierBloodType]) {
		result = [self.healthStore bloodTypeWithError:&error];
	} else if ([characteristic.identifier isEqualToString:HKCharacteristicTypeIdentifierDateOfBirth]) {
		result = [self.healthStore dateOfBirthWithError:&error];
	} else if (&HKCharacteristicTypeIdentifierFitzpatrickSkinType != NULL && [characteristic.identifier isEqualToString:HKCharacteristicTypeIdentifierFitzpatrickSkinType]) {
		result = [self.healthStore fitzpatrickSkinTypeWithError:&error];
	} else if (&HKCharacteristicTypeIdentifierWheelchairUse != NULL && [characteristic.identifier isEqualToString:HKCharacteristicTypeIdentifierWheelchairUse]) {
		result = [self.healthStore wheelchairUseWithError:&error];
	} else {
		NSLog(@"error: unknown characteristic %@", characteristic);
	}

	NSDate *methodFinish = [NSDate date];
	NSTimeInterval executionTime = [methodFinish timeIntervalSinceDate:methodStart];
	NSLog(@"--- querying HealthKit took %f seconds ---", executionTime);
	resultsHandler(result, error);
}


// ----------------------------
#pragma mark -
// ----------------------------


- (void)errorOccurred:(NSError *)error
{
	UnitySendMessage([[BEHealthKit sharedHealthKit].controllerName cStringUsingEncoding:NSUTF8StringEncoding], "HealthKitErrorOccurred", [[error XMLString] cStringUsingEncoding:NSUTF8StringEncoding]);
}


// ----------------------------
#pragma mark -
// ----------------------------


- (NSSet *)dataTypes:(NSArray *)staticIdentifiers writePermission:(BOOL)write
{
	// HKCharacteristicTypes, HKCategoryTypeIdentifierAppleStandHour, HKQuantityTypeIdentifierAppleExerciseTime disallowed
	
	NSMutableArray *identifiers = [staticIdentifiers mutableCopy];
	NSMutableSet *dataTypes = [NSMutableSet set];

	/*--------------------------------*/
	/*   HKQuantityType Identifiers   */
	/*--------------------------------*/

	void (^checkQuantityType)(NSString *, NSMutableArray *, NSMutableSet *) = ^void (NSString *identifier, NSMutableArray *allIdentifiers, NSMutableSet *dataTypes)
	{
		if ([allIdentifiers containsObject:identifier]) {
			NSLog(@"adding %@", identifier);
			[dataTypes addObject:[HKQuantityType quantityTypeForIdentifier:identifier]];
			[allIdentifiers removeObject:identifier];
		}

	};

	// Body Measurements
	checkQuantityType(HKQuantityTypeIdentifierBodyMassIndex, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierBodyFatPercentage, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierHeight, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierBodyMass, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierLeanBodyMass, identifiers, dataTypes);
	
	// Fitness
	checkQuantityType(HKQuantityTypeIdentifierStepCount, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDistanceWalkingRunning, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDistanceCycling, identifiers, dataTypes);
	if (&HKQuantityTypeIdentifierDistanceWheelchair != NULL) checkQuantityType(HKQuantityTypeIdentifierDistanceWheelchair, identifiers, dataTypes);     // iOS 10+
	checkQuantityType(HKQuantityTypeIdentifierBasalEnergyBurned, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierActiveEnergyBurned, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierFlightsClimbed, identifiers, dataTypes);
	if (!write) {
		checkQuantityType(HKQuantityTypeIdentifierNikeFuel, identifiers, dataTypes);
		if (&HKQuantityTypeIdentifierAppleExerciseTime != NULL) checkQuantityType(HKQuantityTypeIdentifierAppleExerciseTime, identifiers, dataTypes);	// iOS 9.3+
	} else {
		[identifiers removeObject:HKQuantityTypeIdentifierNikeFuel];
		if (&HKQuantityTypeIdentifierAppleExerciseTime != NULL) [identifiers removeObject:HKQuantityTypeIdentifierAppleExerciseTime];					// iOS 9.3+
	}
	if (&HKQuantityTypeIdentifierPushCount != NULL) checkQuantityType(HKQuantityTypeIdentifierPushCount, identifiers, dataTypes);                       // iOS 10+
	if (&HKQuantityTypeIdentifierDistanceSwimming != NULL) checkQuantityType(HKQuantityTypeIdentifierDistanceSwimming, identifiers, dataTypes);         // iOS 10+
	if (&HKQuantityTypeIdentifierSwimmingStrokeCount != NULL) checkQuantityType(HKQuantityTypeIdentifierSwimmingStrokeCount, identifiers, dataTypes);   // iOS 10+
	

	// Vitals
	checkQuantityType(HKQuantityTypeIdentifierHeartRate, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierBodyTemperature, identifiers, dataTypes);
	if (&HKQuantityTypeIdentifierBasalBodyTemperature != NULL) checkQuantityType(HKQuantityTypeIdentifierBasalBodyTemperature, identifiers, dataTypes);	// iOS 9+
	checkQuantityType(HKQuantityTypeIdentifierBloodPressureSystolic, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierBloodPressureDiastolic, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierRespiratoryRate, identifiers, dataTypes);

	// Results
	checkQuantityType(HKQuantityTypeIdentifierOxygenSaturation, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierPeripheralPerfusionIndex, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierBloodGlucose, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierNumberOfTimesFallen, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierElectrodermalActivity, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierInhalerUsage, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierBloodAlcoholContent, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierForcedVitalCapacity, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierForcedExpiratoryVolume1, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierPeakExpiratoryFlowRate, identifiers, dataTypes);

	// Nutrition
	checkQuantityType(HKQuantityTypeIdentifierDietaryFatTotal, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryFatPolyunsaturated, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryFatMonounsaturated, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryFatSaturated, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryCholesterol, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietarySodium, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryCarbohydrates, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryFiber, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietarySugar, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryEnergyConsumed, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryProtein, identifiers, dataTypes);

	checkQuantityType(HKQuantityTypeIdentifierDietaryVitaminA, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryVitaminB6, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryVitaminB12, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryVitaminC, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryVitaminD, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryVitaminE, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryVitaminK, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryCalcium, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryIron, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryThiamin, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryRiboflavin, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryNiacin, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryFolate, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryBiotin, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryPantothenicAcid, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryPhosphorus, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryIodine, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryMagnesium, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryZinc, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietarySelenium, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryCopper, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryManganese, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryChromium, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryMolybdenum, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryChloride, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryPotassium, identifiers, dataTypes);
	checkQuantityType(HKQuantityTypeIdentifierDietaryCaffeine, identifiers, dataTypes);
	if (&HKQuantityTypeIdentifierDietaryWater != NULL) checkQuantityType(HKQuantityTypeIdentifierDietaryWater, identifiers, dataTypes);				// iOS 9+
	
	if (&HKQuantityTypeIdentifierUVExposure != NULL) checkQuantityType(HKQuantityTypeIdentifierUVExposure, identifiers, dataTypes);					// iOS 9+

	/*--------------------------------*/
	/*   HKCategoryType Identifiers   */
	/*--------------------------------*/

	void (^checkCategoryType)(NSString *, NSMutableArray *, NSMutableSet *) = ^void (NSString *identifier, NSMutableArray *allIdentifiers, NSMutableSet *dataTypes)
	{
		if ([allIdentifiers containsObject:identifier]) {
			[dataTypes addObject:[HKCategoryType categoryTypeForIdentifier:identifier]];
			[allIdentifiers removeObject:identifier];
		}

	};


	checkCategoryType(HKCategoryTypeIdentifierSleepAnalysis, identifiers, dataTypes);
	if (&HKCategoryTypeIdentifierAppleStandHour != NULL) { // iOS 9+
		if (!write) {
			checkCategoryType(HKCategoryTypeIdentifierAppleStandHour, identifiers, dataTypes);
		} else {
			[identifiers removeObject:HKCategoryTypeIdentifierAppleStandHour];
		}
	}
	if (&HKCategoryTypeIdentifierCervicalMucusQuality != NULL) checkCategoryType(HKCategoryTypeIdentifierCervicalMucusQuality, identifiers, dataTypes);     // iOS 9+
	if (&HKCategoryTypeIdentifierOvulationTestResult != NULL) checkCategoryType(HKCategoryTypeIdentifierOvulationTestResult, identifiers, dataTypes);       // iOS 9+
	if (&HKCategoryTypeIdentifierMenstrualFlow != NULL) checkCategoryType(HKCategoryTypeIdentifierMenstrualFlow, identifiers, dataTypes);                   // iOS 9+
	if (&HKCategoryTypeIdentifierIntermenstrualBleeding != NULL) checkCategoryType(HKCategoryTypeIdentifierIntermenstrualBleeding, identifiers, dataTypes); // iOS 9+
	if (&HKCategoryTypeIdentifierSexualActivity != NULL) checkCategoryType(HKCategoryTypeIdentifierSexualActivity, identifiers, dataTypes);                 // iOS 9+
	if (&HKCategoryTypeIdentifierMindfulSession != NULL) checkCategoryType(HKCategoryTypeIdentifierMindfulSession, identifiers, dataTypes);                 // iOS 10+

	/*--------------------------------------*/
	/*   HKCharacteristicType Identifiers   */
	/*--------------------------------------*/
	
	
	if (!write) {
		void (^checkCharacteristicType)(NSString *, NSMutableArray *, NSMutableSet *) = ^void (NSString *identifier, NSMutableArray *allIdentifiers, NSMutableSet *dataTypes)
		{
			if ([allIdentifiers containsObject:identifier]) {
				[dataTypes addObject:[HKCharacteristicType characteristicTypeForIdentifier:identifier]];
				[allIdentifiers removeObject:identifier];
			}

		};

		checkCharacteristicType(HKCharacteristicTypeIdentifierBiologicalSex, identifiers, dataTypes);
		checkCharacteristicType(HKCharacteristicTypeIdentifierBloodType, identifiers, dataTypes);
		checkCharacteristicType(HKCharacteristicTypeIdentifierDateOfBirth, identifiers, dataTypes);
		if (&HKCharacteristicTypeIdentifierFitzpatrickSkinType != NULL) checkCharacteristicType(HKCharacteristicTypeIdentifierFitzpatrickSkinType, identifiers, dataTypes);
		if (&HKCharacteristicTypeIdentifierWheelchairUse != NULL) checkCharacteristicType(HKCharacteristicTypeIdentifierWheelchairUse, identifiers, dataTypes);
	} else {
		[identifiers removeObjectsInArray:@[HKCharacteristicTypeIdentifierBiologicalSex, HKCharacteristicTypeIdentifierBloodType, HKCharacteristicTypeIdentifierDateOfBirth]];
		if (&HKCharacteristicTypeIdentifierFitzpatrickSkinType != NULL) [identifiers removeObject:HKCharacteristicTypeIdentifierFitzpatrickSkinType];  // iOS 9+
		if (&HKCharacteristicTypeIdentifierWheelchairUse != NULL) [identifiers removeObject:HKCharacteristicTypeIdentifierWheelchairUse];              // iOS 10+
	}

	/*-----------------------------------*/
	/*   HKCorrelationType Identifiers   */
	/*-----------------------------------*/

	void (^checkCorrelationType)(NSString *, NSMutableArray *, NSMutableSet *) = ^void (NSString *identifier, NSMutableArray *allIdentifiers, NSMutableSet *dataTypes)
	{
		void (^addQuantityIdentifier)(NSString *, NSMutableArray *, NSMutableSet *) = ^void (NSString *identifier, NSMutableArray *allIdentifiers, NSMutableSet *dataTypes)
		{
			[dataTypes addObject:[HKQuantityType quantityTypeForIdentifier:identifier]];
			[allIdentifiers removeObject:identifier];
		};

		if ([allIdentifiers containsObject:identifier]) {
			if ([identifier isEqualToString:HKCorrelationTypeIdentifierBloodPressure]) {
				addQuantityIdentifier(HKQuantityTypeIdentifierBloodPressureSystolic, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierBloodPressureDiastolic, allIdentifiers, dataTypes);
			}
			else if ([identifier isEqualToString:HKCorrelationTypeIdentifierFood]) {
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryEnergyConsumed, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryFatTotal, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryFatPolyunsaturated, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryFatMonounsaturated, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryFatSaturated, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryCholesterol, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietarySodium, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryCarbohydrates, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryFiber, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietarySugar, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryEnergyConsumed, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryProtein, allIdentifiers, dataTypes);

				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryVitaminA, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryVitaminB6, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryVitaminB12, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryVitaminC, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryVitaminD, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryVitaminE, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryVitaminK, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryCalcium, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryIron, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryThiamin, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryRiboflavin, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryNiacin, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryFolate, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryBiotin, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryPantothenicAcid, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryPhosphorus, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryIodine, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryMagnesium, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryZinc, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietarySelenium, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryCopper, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryManganese, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryChromium, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryMolybdenum, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryChloride, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryPotassium, allIdentifiers, dataTypes);
				addQuantityIdentifier(HKQuantityTypeIdentifierDietaryCaffeine, allIdentifiers, dataTypes);
			}
			[allIdentifiers removeObject:identifier];
		}

	};

	checkCorrelationType(HKCorrelationTypeIdentifierBloodPressure, identifiers, dataTypes);
	checkCorrelationType(HKCorrelationTypeIdentifierFood, identifiers, dataTypes);

	/*------------------------------*/
	/*   HKWorkoutType Identifier   */
	/*------------------------------*/

	void (^checkWorkoutType)(NSString *, NSMutableArray *, NSMutableSet *) = ^void (NSString *identifier, NSMutableArray *allIdentifiers, NSMutableSet *dataTypes)
	{
		if ([allIdentifiers containsObject:identifier]) {
			[dataTypes addObject:[HKWorkoutType workoutType]];
			[allIdentifiers removeObject:identifier];
		}

	};


	checkWorkoutType(HKWorkoutTypeIdentifier, identifiers, dataTypes);

	// ---------

	if (identifiers.count > 0) {
		NSLog(@"Error: failed to find data types for %@", identifiers);
		NSError *err = [NSError errorWithDomain:@"beliefengine" code:1002 userInfo:@{NSLocalizedDescriptionKey:[NSString stringWithFormat:@"Unable to find data types for identifiers:(%@)", identifiers]}];
		[self errorOccurred:err];
	}

	return dataTypes;
}

- (HKUnit *)defaultUnitForSampleType:(HKSampleType *)sampleType
{
	NSLocale *currentLocale = [NSLocale currentLocale];

	if ([sampleType isKindOfClass:[HKQuantityType class]]) {
		HKQuantityType *quantityType = (HKQuantityType *)sampleType;
		
		NSArray *countTypes = @[HKQuantityTypeIdentifierBodyMassIndex, HKQuantityTypeIdentifierStepCount, HKQuantityTypeIdentifierFlightsClimbed, HKQuantityTypeIdentifierNikeFuel, HKQuantityTypeIdentifierNumberOfTimesFallen, HKQuantityTypeIdentifierInhalerUsage];
		if (&HKQuantityTypeIdentifierUVExposure != NULL) countTypes = [countTypes arrayByAddingObject:HKQuantityTypeIdentifierUVExposure];
		if ([countTypes containsObject:quantityType.identifier]) {
			return [HKUnit countUnit];
		}
		
		NSArray *percentTypes = @[HKQuantityTypeIdentifierBodyFatPercentage, HKQuantityTypeIdentifierOxygenSaturation, HKQuantityTypeIdentifierPeripheralPerfusionIndex, HKQuantityTypeIdentifierBloodAlcoholContent];
		if ([percentTypes containsObject:quantityType.identifier]) {
			return [HKUnit percentUnit];
		}
		
		if ([sampleType.identifier isEqualToString:HKQuantityTypeIdentifierHeight]) {
			if ([[currentLocale objectForKey:NSLocaleUsesMetricSystem] boolValue]) {
				return [HKUnit meterUnit];
			} else {
				return [HKUnit footUnit];
			}
		}
		NSArray *lengthTypes = @[HKQuantityTypeIdentifierDistanceWalkingRunning, HKQuantityTypeIdentifierDistanceCycling];
		if ([lengthTypes containsObject:quantityType.identifier]) {
			if ([[currentLocale objectForKey:NSLocaleUsesMetricSystem] boolValue]) {
				return [HKUnit meterUnitWithMetricPrefix:HKMetricPrefixKilo];
			} else {
				return [HKUnit mileUnit];
			}
		}
		
		NSArray *weightTypes = @[HKQuantityTypeIdentifierBodyMass, HKQuantityTypeIdentifierLeanBodyMass];
		if ([weightTypes containsObject:quantityType.identifier]) {
			if ([[currentLocale objectForKey:NSLocaleUsesMetricSystem] boolValue]) {
				return [HKUnit gramUnitWithMetricPrefix:HKMetricPrefixKilo];
			} else {
				return [HKUnit poundUnit];
			}
		}
		
		NSArray *nutritionTypes = @[HKQuantityTypeIdentifierDietaryFatTotal, HKQuantityTypeIdentifierDietaryFatTotal, HKQuantityTypeIdentifierDietaryFatPolyunsaturated, HKQuantityTypeIdentifierDietaryFatMonounsaturated, HKQuantityTypeIdentifierDietaryFatSaturated,
									HKQuantityTypeIdentifierDietaryCholesterol, HKQuantityTypeIdentifierDietarySodium, HKQuantityTypeIdentifierDietaryCarbohydrates, HKQuantityTypeIdentifierDietaryFiber, HKQuantityTypeIdentifierDietarySugar, HKQuantityTypeIdentifierDietaryProtein,
									HKQuantityTypeIdentifierDietaryVitaminA, HKQuantityTypeIdentifierDietaryVitaminB6, HKQuantityTypeIdentifierDietaryVitaminB12, HKQuantityTypeIdentifierDietaryVitaminC, HKQuantityTypeIdentifierDietaryVitaminD, HKQuantityTypeIdentifierDietaryVitaminE,
									HKQuantityTypeIdentifierDietaryVitaminK, HKQuantityTypeIdentifierDietaryCalcium, HKQuantityTypeIdentifierDietaryIron, HKQuantityTypeIdentifierDietaryThiamin, HKQuantityTypeIdentifierDietaryRiboflavin, HKQuantityTypeIdentifierDietaryNiacin,
									HKQuantityTypeIdentifierDietaryFolate, HKQuantityTypeIdentifierDietaryBiotin, HKQuantityTypeIdentifierDietaryPantothenicAcid, HKQuantityTypeIdentifierDietaryPhosphorus, HKQuantityTypeIdentifierDietaryIodine, HKQuantityTypeIdentifierDietaryMagnesium,
									HKQuantityTypeIdentifierDietaryZinc, HKQuantityTypeIdentifierDietarySelenium, HKQuantityTypeIdentifierDietaryCopper, HKQuantityTypeIdentifierDietaryManganese, HKQuantityTypeIdentifierDietaryChromium, HKQuantityTypeIdentifierDietaryMolybdenum,
									HKQuantityTypeIdentifierDietaryChloride, HKQuantityTypeIdentifierDietaryPotassium, HKQuantityTypeIdentifierDietaryCaffeine];
		if ([nutritionTypes containsObject:quantityType.identifier]) {
			return [HKUnit gramUnit];
		}
		
		NSArray *calorieTypes = @[HKQuantityTypeIdentifierBasalEnergyBurned, HKQuantityTypeIdentifierActiveEnergyBurned, HKQuantityTypeIdentifierDietaryEnergyConsumed];
		if ([calorieTypes containsObject:quantityType.identifier]) {
			return [HKUnit kilocalorieUnit];
		}
		
		NSArray *tempTypes = @[HKQuantityTypeIdentifierBodyTemperature];
		if (&HKQuantityTypeIdentifierBasalBodyTemperature != NULL) tempTypes = [tempTypes arrayByAddingObject:HKQuantityTypeIdentifierBasalBodyTemperature]; // iOS 9+
		if ([tempTypes containsObject:quantityType.identifier]) {
			if ([[currentLocale objectForKey:NSLocaleUsesMetricSystem] boolValue]) {
				return [HKUnit degreeCelsiusUnit];
			} else {
				return [HKUnit degreeFahrenheitUnit];
			}
		}
		
		NSArray *volumeUnits = @[HKQuantityTypeIdentifierForcedVitalCapacity, HKQuantityTypeIdentifierForcedExpiratoryVolume1];
		if (&HKQuantityTypeIdentifierDietaryWater != NULL) volumeUnits = [volumeUnits arrayByAddingObject:HKQuantityTypeIdentifierDietaryWater]; // iOS 9+
		if ([volumeUnits containsObject:quantityType.identifier]) {
			return [HKUnit literUnit];
		}

		if ([quantityType.identifier isEqual:HKQuantityTypeIdentifierHeartRate]) return [[HKUnit countUnit] unitDividedByUnit:[HKUnit minuteUnit]];
		if ([quantityType.identifier isEqual:HKQuantityTypeIdentifierBloodPressureSystolic]) return [HKUnit millimeterOfMercuryUnit];
		if ([quantityType.identifier isEqual:HKQuantityTypeIdentifierBloodPressureDiastolic]) return [HKUnit millimeterOfMercuryUnit];
		if ([quantityType.identifier isEqual:HKQuantityTypeIdentifierRespiratoryRate]) return [[HKUnit countUnit] unitDividedByUnit:[HKUnit minuteUnit]];
		
		// Results
		if ([quantityType.identifier isEqual:HKQuantityTypeIdentifierBloodGlucose]) return [[HKUnit gramUnitWithMetricPrefix:HKMetricPrefixMilli] unitDividedByUnit:[HKUnit literUnitWithMetricPrefix:HKMetricPrefixDeci]];
		if ([quantityType.identifier isEqual:HKQuantityTypeIdentifierElectrodermalActivity]) return [HKUnit siemenUnitWithMetricPrefix:HKMetricPrefixMicro];

		if ([quantityType.identifier isEqual:HKQuantityTypeIdentifierPeakExpiratoryFlowRate]) return  [[HKUnit literUnit] unitDividedByUnit:[HKUnit minuteUnit]];
	
	}
	

	NSLog(@"Error; not sure what unit to use for %@", sampleType);
	NSError *err = [NSError errorWithDomain:@"beliefengine" code:1003 userInfo:@{NSLocalizedDescriptionKey:[NSString stringWithFormat:@"No default unit for sample type %@.", sampleType]}];
	[self errorOccurred:err];
	return nil;
}

- (HKUnit *)genericUnitForSampleType:(HKSampleType *)sampleType
{
	NSLocale *currentLocale = [NSLocale currentLocale];

	if ([sampleType isKindOfClass:[HKQuantityType class]]) {
		HKQuantityType *quantityType = (HKQuantityType *)sampleType;

		HKUnit *unit = nil;

		// Scalar(Count)
		if ([quantityType isCompatibleWithUnit:[HKUnit countUnit]]) return [HKUnit countUnit];
		// Scalar(Percent, 0.0 - 1.0)
		if ([quantityType isCompatibleWithUnit:[HKUnit percentUnit]]) return [HKUnit percentUnit];
		// Energy
		if ([quantityType isCompatibleWithUnit:[HKUnit calorieUnit]]) return [HKUnit calorieUnit];
		// Length
		if ([quantityType isCompatibleWithUnit:[HKUnit meterUnit]]) {
			if ([[currentLocale objectForKey:NSLocaleUsesMetricSystem] boolValue]) {
				return [HKUnit meterUnit];
			} else {
				return [HKUnit footUnit];
			}
		}
		// Mass
		if ([quantityType isCompatibleWithUnit:[HKUnit poundUnit]]) {
			if ([[currentLocale objectForKey:NSLocaleUsesMetricSystem] boolValue]) {
				return [HKUnit gramUnitWithMetricPrefix:HKMetricPrefixKilo];
			} else {
				return [HKUnit poundUnit];
			}
		}
		// Temperature
		if ([quantityType isCompatibleWithUnit:[HKUnit degreeCelsiusUnit]]) {
			if ([[currentLocale objectForKey:NSLocaleUsesMetricSystem] boolValue]) {
				return [HKUnit degreeCelsiusUnit];
			} else {
				return [HKUnit degreeFahrenheitUnit];
			}
		}
		// Scalar(Count)/Time
		unit = [[HKUnit countUnit] unitDividedByUnit:[HKUnit minuteUnit]];
		if ([quantityType isCompatibleWithUnit:unit]) {
			return unit;
		}
		// Pressure
		if ([quantityType isCompatibleWithUnit:[HKUnit millimeterOfMercuryUnit]]) {
			return [HKUnit millimeterOfMercuryUnit];
		}
		// Mass/Volume
		unit = [[HKUnit gramUnitWithMetricPrefix:HKMetricPrefixMilli] unitDividedByUnit:[HKUnit literUnitWithMetricPrefix:HKMetricPrefixDeci]];
		if ([quantityType isCompatibleWithUnit:unit]) {
			return unit;
		}
		// Conductance
		if ([quantityType isCompatibleWithUnit:[HKUnit siemenUnit]]) {
			return [HKUnit siemenUnitWithMetricPrefix:HKMetricPrefixMicro];
		}
		// Volume
		if ([quantityType isCompatibleWithUnit:[HKUnit literUnit]]) {
			return [HKUnit literUnit];
		}
		// Volume/Time
		unit = [[HKUnit literUnit] unitDividedByUnit:[HKUnit hourUnit]];
		if ([quantityType isCompatibleWithUnit:unit]) {
			return unit;
		}
	}

	NSLog(@"Error; not sure what unit to use for %@", sampleType);
	NSError *err = [NSError errorWithDomain:@"beliefengine" code:1003 userInfo:@{NSLocalizedDescriptionKey:[NSString stringWithFormat:@"No default unit for sample type %@.", sampleType]}];
	[self errorOccurred:err];
	return nil;
}

@end
