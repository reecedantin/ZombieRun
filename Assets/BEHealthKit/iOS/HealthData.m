//
//  HealthData.m
//  Unity-iPhone
//
//  Created by greay on 3/28/15.
//
//

#import "HealthData.h"
#import <HealthKit/HealthKit.h>
#import "BEHealthKit.h"
#import "XMLDictionary/XMLDictionary.h"
#import "NSDate+bridge.h"

// ---------------------
// MARK: serialization
// ---------------------

@implementation HKObjectType (serialization)

- (id)be_serializable {
	NSMutableDictionary *dict = [NSMutableDictionary dictionary];
	dict[@"identifier"] = self.identifier;
	return dict;
}

@end


@implementation HKSampleType (serialization)

- (id)be_serializable {
	NSMutableDictionary *dict = [super be_serializable];
	// nothing?
	return dict;
}

@end


@implementation HKQuantityType (serialization)

- (id)be_serializable {
	NSMutableDictionary *dict = [super be_serializable];
	dict[@"aggregationStyle"] = (self.aggregationStyle == HKQuantityAggregationStyleCumulative) ? @"cumulative" : @"discrete";
	return dict;
}

@end


@implementation HKCorrelationType (serialization)

- (id)be_serializable {
	NSMutableDictionary *dict = [super be_serializable];
	// nothing?
	return dict;
}

@end

@implementation HKQuantity (serialization)

- (id)be_serializableWithUnit:(HKUnit *)unit {
//	NSArray *units = @[[HKUnit countUnit], [HKUnit percentUnit], [HKUnit gramUnit], [HKUnit kilocalorieUnit],
//					   [[HKUnit countUnit] unitDividedByUnit:[HKUnit minuteUnit]],
//					   [HKUnit degreeCelsiusUnit], [HKUnit millimeterOfMercuryUnit],
//					   [[HKUnit gramUnit] unitDividedByUnit:[HKUnit literUnit]],
//					   [HKUnit siemenUnit],
//					   [[HKUnit literUnit] unitDividedByUnit:[HKUnit hourUnit]]
//					   ];
	if ([self isCompatibleWithUnit:unit]) {
		NSMutableDictionary *dict = [NSMutableDictionary dictionary];
		dict[@"unit"] = [unit unitString];
		dict[@"value"] = @([self doubleValueForUnit:unit]);
		
		return dict;
	}
	NSLog(@"error; don't know which unit to use!");
	return nil;
}

@end


@implementation HKObject (serialization)

- (id)be_serializable {
	NSMutableDictionary *dict = [NSMutableDictionary dictionary];
	if (self.metadata) {
		NSMutableDictionary *metadata = [@{} mutableCopy];
		for (NSString *key in self.metadata) {
			if ([self.metadata[key] isKindOfClass:[NSString class]] || [self.metadata[key] isKindOfClass:[NSNumber class]]) {
				metadata[key] = self.metadata[key];
			}
			else if ([self.metadata[key] isKindOfClass:[NSDate class]]) {
				metadata[key] = [(NSDate *)self.metadata[key] bridgeToken];
			}
			else {
				NSLog(@"error:don't know how to handle metadata[%@] = %@ (%@)", key, self.metadata[key], [self.metadata[key] class]);
			}
		}
		dict[@"metadata"] = metadata;
		
	}
	return dict;
}

@end



@implementation HKSample (serialization)

- (id)be_serializable {
	NSMutableDictionary *dict = [super be_serializable];
	dict[@"startDate"] = [self.startDate bridgeToken];
	dict[@"endDate"] = [self.endDate bridgeToken];
	dict[@"sampleType"] = [self.sampleType be_serializable];
	return dict;
}

@end


@implementation HKQuantitySample (serialization)

- (id)be_serializable {
	NSMutableDictionary *dict = [super be_serializable];
	dict[@"quantityType"] = [self.quantityType be_serializable];
	
	HKUnit *unit = [[BEHealthKit sharedHealthKit] defaultUnitForSampleType:self.quantityType];
	dict[@"quantity"] = [self.quantity be_serializableWithUnit:unit];
	
	return dict;
}

@end


@implementation HKCorrelation (serialization)

- (id)be_serializable {
	NSMutableDictionary *dict = [super be_serializable];
#warning this doesnt seem to do much of anything!
	dict[@"correlationType"] = [self.correlationType be_serializable];
	
	NSMutableArray *objects = [NSMutableArray array];
	for (HKSample *object in self.objects) {
		#warning what class are these? 
		[objects addObject:[object be_serializable]];
	}
	dict[@"objects"] = objects;
	
	return dict;
}

@end


@implementation HKWorkoutEvent (serialization)

- (id)be_serializable {
	NSMutableDictionary *dict = [NSMutableDictionary dictionary];
	dict[@"date"] = [self.date bridgeToken];
	dict[@"eventType"] = @(self.type);
	
	return dict;
}

@end


@implementation HKWorkout (serialization)

- (id)be_serializable {
	NSMutableDictionary *dict = [super be_serializable];
	dict[@"duration"] = @(self.duration);
	dict[@"totalDistance"] = [self.totalDistance be_serializableWithUnit:[HKUnit mileUnit]];
	dict[@"energyBurned"] = [self.totalEnergyBurned be_serializableWithUnit:[HKUnit kilocalorieUnit]];
	dict[@"activityType"] = @(self.workoutActivityType);
	
	NSMutableArray *events = [NSMutableArray array];
	for (HKWorkoutEvent *event in self.workoutEvents) {
		[events addObject:[event be_serializable]];
	}
	dict[@"events"] = events;
	
	return dict;
}

@end


@implementation HKCategoryType (serialization)

- (id)be_serializable {
	NSMutableDictionary *dict = [super be_serializable];
	return dict;
}

@end


@implementation HKCategorySample (serialization)

- (id)be_serializable {
	NSMutableDictionary *dict = [super be_serializable];
	dict[@"categoryType"] = [self.categoryType be_serializable];
	dict[@"value"] = @(self.value);
	return dict;
}

@end


@implementation CMPedometerData (serialization)

- (id)be_serializable {
	NSMutableDictionary *dict = [NSMutableDictionary dictionary];
	dict[@"startDate"] = [self.startDate bridgeToken];
	dict[@"endDate"] = [self.endDate bridgeToken];
	dict[@"numberOfSteps"] = self.numberOfSteps;
	return dict;
}

@end

// ---------------------
// MARK: XML
// ---------------------


@implementation HealthData

+ (NSString *)XMLFromQuantitySamples:(NSArray *)quantitySamples datatype:(NSString *)datatype
{
	NSMutableDictionary *dict = [NSMutableDictionary dictionary];
	dict[XMLDictionaryNodeNameKey] = @"quantity";
	dict[@"datatype"] = datatype;
	
	NSMutableArray *samples = [NSMutableArray array];
	for (HKQuantitySample *sample in quantitySamples) {
		[samples addObject:[sample be_serializable]];
	}
	dict[@"quantitySample"] = samples;
	
	return [dict XMLString];
//	return [self plistXML:dict];
}

+ (NSString *)XMLFromCombinedTotal:(double)total
{
	NSDictionary *dict = @{@"total":@(total), XMLDictionaryNodeNameKey:@"total"};
	return [dict XMLString];
}

+ (NSString *)XMLFromCategorySamples:(NSArray *)categorySamples datatype:(NSString *)datatype
{
	NSMutableDictionary *dict = [NSMutableDictionary dictionary];
	dict[XMLDictionaryNodeNameKey] = @"category";
	dict[@"datatype"] = datatype;
	
	NSMutableArray *samples = [NSMutableArray array];
	for (HKCategorySample *sample in categorySamples) {
		[samples addObject:[sample be_serializable]];
	}
	dict[@"categorySample"] = samples;
	
	return [dict XMLString];
}

+ (NSString *)XMLFromCorrelationSamples:(NSArray *)correlationSamples datatype:(NSString *)datatype
{
	NSMutableDictionary *dict = [NSMutableDictionary dictionary];
	dict[XMLDictionaryNodeNameKey] = @"correlation";
	dict[@"datatype"] = datatype;
	
	NSMutableArray *samples = [NSMutableArray array];
	for (HKCorrelation *sample in correlationSamples) {
		[samples addObject:[sample be_serializable]];
	}
	dict[@"correlationSample"] = samples;
	
	return [dict XMLString];
}

+ (NSString *)XMLFromWorkoutSamples:(NSArray *)workoutSamples workoutType:(int)workoutType
{
	NSMutableDictionary *dict = [NSMutableDictionary dictionary];
	dict[XMLDictionaryNodeNameKey] = @"workout";
	dict[@"datatype"] = HKWorkoutTypeIdentifier;
	dict[@"workoutType"] = @(workoutType);
	
	NSMutableArray *samples = [NSMutableArray array];
	for (HKWorkout *sample in workoutSamples) {
		[samples addObject:[sample be_serializable]];
	}
	dict[@"workoutSample"] = samples;
	
	return [dict XMLString];
}


+ (NSString *)XMLFromCharacteristic:(id)characteristic datatype:(NSString *)datatype
{
	NSMutableDictionary *dict = [NSMutableDictionary dictionary];
	dict[XMLDictionaryNodeNameKey] = @"characteristic";
	dict[@"datatype"] = datatype;
	
	if ([characteristic isKindOfClass:[HKBiologicalSexObject class]]) {
		dict[@"sex"] = @([(HKBiologicalSexObject *)characteristic biologicalSex]);
	} else if ([characteristic isKindOfClass:[HKBloodTypeObject class]]) {
		dict[@"bloodType"] = @([(HKBloodTypeObject *)characteristic bloodType]);
	} else if ([characteristic isKindOfClass:[HKFitzpatrickSkinTypeObject class]]) {
		dict[@"skinType"] = @([(HKFitzpatrickSkinTypeObject *)characteristic skinType]);
	} else if ([characteristic isKindOfClass:[HKWheelchairUseObject class]]) {
		dict[@"wheelchairUse"] = @([(HKWheelchairUseObject *)characteristic wheelchairUse]);
	} else if ([characteristic isKindOfClass:[NSDate class]]) {
		dict[@"DOB"] = [(NSDate *)characteristic bridgeToken];
	} else {
		NSLog(@"Error; unrecognized characteristic:%@", [characteristic class]);
		return nil;
	}
	
	return [dict XMLString];
}

+ (NSString *)XMLFromPedometerData:(CMPedometerData *)data
{
	NSMutableDictionary *dict = [NSMutableDictionary dictionary];
	dict[@"pedometerData"] = [data be_serializable];
	dict[XMLDictionaryNodeNameKey] = @"pedometer";

	return [dict XMLString];
}


+ (NSString *)plistXML:(NSDictionary *)plist
{
	NSError *error;
	NSData *data = [NSPropertyListSerialization dataWithPropertyList:plist format:NSPropertyListXMLFormat_v1_0 options:0 error: &error];
	if (data == nil) {
		NSLog (@"error serializing to xml: %@", error);
		return nil;
	}
	return [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
}

@end

