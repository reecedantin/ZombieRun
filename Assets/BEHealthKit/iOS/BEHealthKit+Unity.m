//
//  BEHealthKit+Unity.m
//  Unity-iPhone
//
//  Created by greay on 3/25/15.
//
//

#import "BEHealthKit+Unity.h"
#import "BEHealthKit.h"
#import "HealthData.h"

#import "BEPedometer.h"
#import "XMLDictionary/XMLDictionary.h"

#import "BEHealthKit+dummyData.h"
#import "NSDate+bridge.h"

NSArray *parseTransmission(char *dataTypesString);
void saveSample(NSString *datatype, HKObject *sample);

// -------------------------------------
#pragma mark - External interface
// -------------------------------------

void _InitializeNative(char *controllerName)
{
//	[[BEHealthKit sharedHealthKit] start];
	[BEHealthKit sharedHealthKit].controllerName = [NSString stringWithCString:controllerName encoding:NSUTF8StringEncoding];
}

void _Authorize(char *dataTypesString)
{
	BEHealthKit *kit = [[BEHealthKit alloc] init];
	NSArray *types = parseTransmission(dataTypesString);
	[kit authorizeHealthKitToRead:types[0] write:types[1] completion:^(bool success, NSError *error) {
		if (!success) {
			NSLog(@"Error authorizing healthkit:%@", error);
			[kit errorOccurred:error];
		}
		
		NSString *response = (success) ? @"true" : @"false";
		UnitySendMessage([[BEHealthKit sharedHealthKit].controllerName cStringUsingEncoding:NSUTF8StringEncoding], "AuthorizeComplete", [response cStringUsingEncoding:NSUTF8StringEncoding]);
	}];
}

int _AuthorizationStatusForType(char *dataTypeString)
{
	BEHealthKit *kit = [[BEHealthKit alloc] init];
	return [kit authorizationStatusForType:[NSString stringWithCString:dataTypeString encoding:NSUTF8StringEncoding]];
}

BOOL _IsHealthDataAvailable()
{
	return [HKHealthStore isHealthDataAvailable];
}

void _ReadQuantity(char *identifier, char *startDateString, char *endDateString, bool combineSamples)
{
	BEHealthKit *kit = [[BEHealthKit alloc] init];

	NSString *identifierString = [NSString stringWithCString:identifier encoding:NSUTF8StringEncoding];
	HKQuantityType *sampleType = [HKSampleType quantityTypeForIdentifier:identifierString];
	if (!sampleType) {
		NSLog(@"Error; unknown quantity-type identifier '%@'", identifierString);
		NSError *err = [NSError errorWithDomain:@"beliefengine" code:2001 userInfo:@{NSLocalizedDescriptionKey:[NSString stringWithFormat:@"Unknown quantity-type identifier %@.", identifierString]}];
		[kit errorOccurred:err];
		return;
	}

	NSDate *startDate = [NSDate dateFromBridgeString:startDateString];
	NSDate *endDate = [NSDate dateFromBridgeString:endDateString];
	NSLog(@"[reading quantity from %@ to %@]", startDate, endDate);

	[kit readSamples:sampleType fromDate:startDate toDate:endDate resultsHandler:^(NSArray *results, NSError *error) {
		if(!error && results)
		{
			NSString *xml = nil;
			if (!combineSamples) {
				xml = [HealthData XMLFromQuantitySamples:results datatype:identifierString];
			} else {
				HKUnit *unit = [kit defaultUnitForSampleType:sampleType];
				double total = 0;
				for (HKQuantitySample *sample in results) {
					total += [sample.quantity doubleValueForUnit:unit];
				}
				if (sampleType.aggregationStyle == HKQuantityAggregationStyleCumulative) {
					xml = [HealthData XMLFromCombinedTotal:total];
				} else {
					double average = total / (double)results.count;
					xml = [HealthData XMLFromCombinedTotal:average];
				}
			}
			UnitySendMessage([[BEHealthKit sharedHealthKit].controllerName cStringUsingEncoding:NSUTF8StringEncoding], "ParseHealthXML", [xml cStringUsingEncoding:NSUTF8StringEncoding]);
		} else {
			NSLog(@"Error: unable to read q:%@", error);
			[kit errorOccurred:error];
		}
	}];
}

void _WriteQuantity(char *identifier, char *unitString, double doubleValue, char *startDateString, char *endDateString)
{
	NSString *identifierString = [NSString stringWithCString:identifier encoding:NSUTF8StringEncoding];
	HKQuantityType *quantityType = [HKQuantityType quantityTypeForIdentifier:identifierString];
	HKUnit *unit = [HKUnit unitFromString:[NSString stringWithCString:unitString encoding:NSUTF8StringEncoding]];
	HKQuantity *quantity = [HKQuantity quantityWithUnit:unit doubleValue:doubleValue];
	NSDate *startDate = [NSDate dateFromBridgeString:startDateString];
	NSDate *endDate = [NSDate dateFromBridgeString:endDateString];
	
	HKQuantitySample *sample = [HKQuantitySample quantitySampleWithType:quantityType quantity:quantity startDate:startDate endDate:endDate];
	
	saveSample(identifierString, sample);
}

void _ReadCategory(char *identifier, char *startDateString, char *endDateString)
{
	BEHealthKit *kit = [[BEHealthKit alloc] init];

	NSString *identifierString = [NSString stringWithCString:identifier encoding:NSUTF8StringEncoding];
	HKCategoryType *sampleType = [HKSampleType categoryTypeForIdentifier:identifierString];
	if (!sampleType) {
		NSLog(@"Error; unknown category-type identifier '%@'", identifierString);
		NSError *err = [NSError errorWithDomain:@"beliefengine" code:2002 userInfo:@{NSLocalizedDescriptionKey:[NSString stringWithFormat:@"Unknown category-type identifier %@.", identifierString]}];
		[kit errorOccurred:err];
		return;
	}

	NSDate *startDate = [NSDate dateFromBridgeString:startDateString];
	NSDate *endDate = [NSDate dateFromBridgeString:endDateString];
	NSLog(@"[reading category data from %@ to %@]", startDate, endDate);

	[kit readSamples:sampleType fromDate:startDate toDate:endDate resultsHandler:^(NSArray *results, NSError *error) {
		if(!error && results)
		{
			NSString *xml = nil;
			xml = [HealthData XMLFromCategorySamples:results datatype:identifierString];
			UnitySendMessage([[BEHealthKit sharedHealthKit].controllerName cStringUsingEncoding:NSUTF8StringEncoding], "ParseHealthXML", [xml cStringUsingEncoding:NSUTF8StringEncoding]);
		} else {
			NSLog(@"Error: unable to read cat:%@", error);
			[kit errorOccurred:error];
		}
	}];
}

void _WriteCategory(char *identifier, int intValue, char *startDateString, char *endDateString)
{
	NSString *identifierString = [NSString stringWithCString:identifier encoding:NSUTF8StringEncoding];
	HKCategoryType *categoryType = [HKCategoryType categoryTypeForIdentifier:identifierString];
	NSDate *startDate = [NSDate dateFromBridgeString:startDateString];
	NSDate *endDate = [NSDate dateFromBridgeString:endDateString];
	
	HKCategorySample *sample = [HKCategorySample categorySampleWithType:categoryType value:intValue startDate:startDate endDate:endDate];

	saveSample(identifierString, sample);
}

void _ReadCharacteristic(char *identifier)
{
	BEHealthKit *kit = [[BEHealthKit alloc] init];

	NSString *identifierString = [NSString stringWithCString:identifier encoding:NSUTF8StringEncoding];
	HKCharacteristicType *characteristic = [HKCharacteristicType characteristicTypeForIdentifier:identifierString];
	if (!characteristic) {
		NSLog(@"Error; unknown characteristic-type identifier '%@'", identifierString);
		NSError *err = [NSError errorWithDomain:@"beliefengine" code:2001 userInfo:@{NSLocalizedDescriptionKey:[NSString stringWithFormat:@"Unknown characteristic-type identifier %@.", identifierString]}];
		[kit errorOccurred:err];
		return;
	}

	[kit readCharacteristic:characteristic resultsHandler:^(id result, NSError *error) {
		if(!error && result)
		{
			NSString *xml = nil;
			xml = [HealthData XMLFromCharacteristic:result datatype:identifierString];
			UnitySendMessage([[BEHealthKit sharedHealthKit].controllerName cStringUsingEncoding:NSUTF8StringEncoding], "ParseHealthXML", [xml cStringUsingEncoding:NSUTF8StringEncoding]);
		} else {
			NSLog(@"Error: unable to read char:%@", error);
			[kit errorOccurred:error];
		}
	}];
}

void _ReadCorrelation(char *identifier, char *startDateString, char *endDateString, bool combineSamples)
{
	BEHealthKit *kit = [[BEHealthKit alloc] init];

	NSString *identifierString = [NSString stringWithCString:identifier encoding:NSUTF8StringEncoding];
	HKCorrelationType *sampleType = [HKCorrelationType correlationTypeForIdentifier:identifierString];
	if (!sampleType) {
		NSLog(@"Error; unknown correlation-type identifier '%@'", identifierString);
		[kit errorOccurred:nil];
		return;
	}

	NSDate *startDate = [NSDate dateFromBridgeString:startDateString];
	NSDate *endDate = [NSDate dateFromBridgeString:endDateString];
//	NSLog(@"[reading correlation from %@ to %@]", startDate, endDate);

	[kit readSamples:sampleType fromDate:startDate toDate:endDate resultsHandler:^(NSArray *results, NSError *error) {
		if(!error && results)
		{
			NSString *xml = nil;
			xml = [HealthData XMLFromCorrelationSamples:results datatype:identifierString];
			UnitySendMessage([[BEHealthKit sharedHealthKit].controllerName cStringUsingEncoding:NSUTF8StringEncoding], "ParseHealthXML", [xml cStringUsingEncoding:NSUTF8StringEncoding]);
		} else {
			NSLog(@"Error: unable to read correlation:%@", error);
			[kit errorOccurred:error];
		}
	}];
}

void _ReadWorkout(int activityID, char *startDateString, char *endDateString, bool combineSamples)
{
	HKWorkoutActivityType activityType = (HKWorkoutActivityType)activityID;

	BEHealthKit *kit = [[BEHealthKit alloc] init];
	NSDate *startDate = [NSDate dateFromBridgeString:startDateString];
	NSDate *endDate = [NSDate dateFromBridgeString:endDateString];
//	NSLog(@"[reading workout from %@ to %@]", startDate, endDate);

	[kit readSamplesForWorkoutActivity:activityType fromDate:startDate toDate:endDate resultsHandler:^(NSArray *results, NSError *error) {
		if(!error && results)
		{
			NSString *xml = nil;
			xml = [HealthData XMLFromWorkoutSamples:results workoutType:activityType];
			UnitySendMessage([[BEHealthKit sharedHealthKit].controllerName cStringUsingEncoding:NSUTF8StringEncoding], "ParseHealthXML", [xml cStringUsingEncoding:NSUTF8StringEncoding]);
		} else {
			NSLog(@"Error: unable to read correlation:%@", error);
			[kit errorOccurred:error];
		}
	}];
}

void _WriteWorkoutSimple(int activityID, char *startDateString, char *endDateString)
{
	HKWorkoutActivityType activityType = (HKWorkoutActivityType)activityID;
	NSDate *startDate = [NSDate dateFromBridgeString:startDateString];
	NSDate *endDate = [NSDate dateFromBridgeString:endDateString];
	
	HKWorkout *sample = [HKWorkout workoutWithActivityType:activityType startDate:startDate endDate:endDate];
	
	saveSample(HKWorkoutTypeIdentifier, sample);
}

void _WriteWorkout(int activityID, char *startDateString, char *endDateString, double kilocaloriesBurned, double distance)
{
	/*
	 // Provide summary information when creating the workout.
	 HKWorkout *run = [HKWorkout workoutWithActivityType:HKWorkoutActivityTypeRunning
	 startDate:start
	 endDate:end
	 duration:0
	 totalEnergyBurned:energyBurned
	 totalDistance:distance
	 metadata:nil];
	 */

	HKWorkoutActivityType activityType = (HKWorkoutActivityType)activityID;

	HKQuantity *cal =	(kilocaloriesBurned == 0) ?	[HKQuantity quantityWithUnit:[HKUnit kilocalorieUnit] doubleValue:kilocaloriesBurned] : nil;
	HKQuantity *d =		(distance == 0) ?			[HKQuantity quantityWithUnit:[HKUnit mileUnit] doubleValue:distance] : nil;
	
	NSDate *startDate = [NSDate dateFromBridgeString:startDateString];
	NSDate *endDate = [NSDate dateFromBridgeString:endDateString];
	
	HKWorkout *sample = [HKWorkout workoutWithActivityType:activityType startDate:startDate endDate:endDate duration:0 totalEnergyBurned:cal totalDistance:d metadata:nil];
	
	saveSample(HKWorkoutTypeIdentifier, sample);
}


// -------------------------------------------------
#pragma mark -
// -------------------------------------------------

void _GenerateDummyData(char *dataTypesString)
{
	BEHealthKit *kit = [[BEHealthKit alloc] init];
	NSArray *types = parseTransmission(dataTypesString);
	NSArray *combined = [[[NSSet setWithArray:types[0]] setByAddingObjectsFromSet:[NSSet setWithArray:types[1]]] allObjects];
	[kit authorizeHealthKitToRead:types[0] write:combined completion:^(bool success, NSError *error) {
		if (!success) {
			NSLog(@"Error authorizing healthkit:%@", error);
			[kit errorOccurred:error];
			return;
		} else {
			[kit generateDummyData];
		}
	}];
}


// -------------------------------------------------
#pragma mark -
// -------------------------------------------------


void _ReadPedometer(char *startDateString, char *endDateString)
{
	NSDate *startDate = [NSDate dateFromBridgeString:startDateString];
	NSDate *endDate =   [NSDate dateFromBridgeString:endDateString];
	
	if (![BEHealthKit sharedHealthKit].pedometer) {
		[BEHealthKit sharedHealthKit].pedometer = [[BEPedometer alloc] init];
	}
	[[BEHealthKit sharedHealthKit].pedometer queryPedometerDataFromDate:startDate toDate:endDate];
}

void _StartReadingPedometerFromDate(char *startDateString)
{
	NSDate *startDate = [NSDate dateFromBridgeString:startDateString];

	if (![BEHealthKit sharedHealthKit].pedometer) {
		[BEHealthKit sharedHealthKit].pedometer = [[BEPedometer alloc] init];
	}
	[[BEHealthKit sharedHealthKit].pedometer startPedometerUpdatesFromDate:startDate];
}

void _StopReadingPedometer()
{
	[[BEHealthKit sharedHealthKit].pedometer stopPedometerUpdates];
}


// -------------------------------------------------
#pragma mark - Internal
// -------------------------------------------------

void saveSample(NSString *datatype, HKObject *sample) {
	BEHealthKit *kit = [[BEHealthKit alloc] init];
	[kit.healthStore saveObject:sample withCompletion:^(BOOL success, NSError *error) {
		if (!success) NSLog(@"error: %@", error);
		
		NSMutableDictionary *dict = [@{@"success":@(success), XMLDictionaryNodeNameKey:@"write"} mutableCopy];
		dict[@"datatype"] = datatype;
		if ([sample isKindOfClass:[HKWorkout class]]) {
			int workoutType = [(HKWorkout *)sample workoutActivityType];
			dict[@"workoutType"] = @(workoutType);
		}
		if (error) {
			dict[@"error"] = error;
		}
		NSString *xml = [dict XMLString];
		UnitySendMessage([[BEHealthKit sharedHealthKit].controllerName cStringUsingEncoding:NSUTF8StringEncoding], "ParseHealthXML", [xml cStringUsingEncoding:NSUTF8StringEncoding]);
		
	}];
}

NSArray *parseTransmission(char *dataTypesString) {
	NSArray *strings = [[NSString stringWithCString:dataTypesString encoding:NSUTF8StringEncoding] componentsSeparatedByString:@"|"];
	NSMutableArray *read = [[strings[0] componentsSeparatedByString:@","] mutableCopy];
	NSMutableArray *write = [[strings[1] componentsSeparatedByString:@","] mutableCopy];
	[read removeObject:@""];
	[write removeObject:@""];
	return @[read, write];
}
