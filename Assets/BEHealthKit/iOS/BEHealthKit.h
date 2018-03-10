//
//  BEHealthKit.h
//  Unity-iPhone
//
//  Created by greay on 3/25/15.
//
//

#import <Foundation/Foundation.h>
#import <HealthKit/HealthKit.h>
#import "BEPedometer.h"


/*! @brief Handles HealthKit requests from Unity.
 */
@interface BEHealthKit : NSObject

@property HKHealthStore *healthStore; /*!< @brief The HKHealthStore object */
@property NSString *controllerName;	/*!< @brief name of the GameObject to send messages to */
@property (nonatomic, strong) BEPedometer *pedometer; /*!< @brief The BEPedometer object */

/*! @brief returns the shared BEHealthKit object */
+ (instancetype)sharedHealthKit;


/*! @brief                   returns the authorization status for the given data type.
	@param dataTypeString    HealthKit datatype identifiers to query.
 */
- (int)authorizationStatusForType:(NSString *)dataTypeString;

/*! @brief                   brings up the system health data authorization panel.
	@details                 Wrapper around the HealthKit -requestAuthorizationToShareTypes:readTypes:completion method.
	@param readIdentifiers   array of HealthKit datatype identifiers to read.
	@param writeIdentifiers  array of HealthKit datatype identifiers to write.
	@param completion        called after the user responds to the request. If success is false, error contain information about what went wrong, otherwise it will be set to nil.
 */
- (void)authorizeHealthKitToRead:(NSArray *)readIdentifiers write:(NSArray *)writeIdentifiers completion:(void (^)(bool success, NSError *error))completion;

/*! @brief					 read quantity, category or correlation samples.
	@details				 Executes a query with -initWithSampleType:predicate:limit:sortDescriptors:resultsHandler:. Limit will be set to no limit, and they will be sorted by startDate, in ascending order.
	@param sampleType		 the type of sample to read.
	@param startDate		 the starting limit for the query.
	@param endDate			 the end date.
	@param resultsHandler	 Called when the query finishes executing. If unsuccessful, error contains information about what went wrong, otherwise it will be set to nil.
 */
- (void)readSamples:(HKSampleType *)sampleType fromDate:(NSDate *)startDate toDate:(NSDate *)endDate resultsHandler:(void (^)(NSArray *results, NSError *error))resultsHandler;


/*! @brief					read a characteristic.
	@details				Characteristics are things that don't change over time, like blood type or birth date.
	@param characteristic	The characteristic to read.
	@param resultsHandler	Called when the query finishes executing. If unsuccessful, error contains information about what went wrong, otherwise it will be set to nil.
 */
- (void)readCharacteristic:(HKCharacteristicType *)characteristic resultsHandler:(void (^)(id result, NSError *error))resultsHandler;


/*! @brief					read workout samples
	@details				...
	@param activity		The activity type to read. See [HKWorkoutActivityType documentation](https://developer.apple.com/library/prerelease/ios/documentation/HealthKit/Reference/HealthKit_Constants/index.html#//apple_ref/c/tdef/HKWorkoutActivityType)
	@param startDate		the starting limit for the query.
	@param endDate			the end date.
	@param resultsHandler	Called when the query finishes executing. If unsuccessful, error contains information about what went wrong, otherwise it will be set to nil.
 */
- (void)readSamplesForWorkoutActivity:(HKWorkoutActivityType)activity fromDate:(NSDate *)startDate toDate:(NSDate *)endDate resultsHandler:(void (^)(NSArray *results, NSError *error))resultsHandler;


/*! @brief			Sends an error back to Unity.
	@details		Converts the error to XML (See NSError+XML.h), and calls ErrorOccurred() on the HealthStore GameObject.
	@param error	the error.
 */
- (void)errorOccurred:(NSError *)error;


/*! @brief				Returns the default unit for a sample type.
	@details			I try to be somewhat intelligent about this (temperature, for example, will be returned in the device's locale, so you don't need to worry about converting it before displaying to the user.
						That said, if you want to change some of these defaults this is the place to do it. Will return nil if there is no default unit, but this shouldn't happen.
	@param sampleType	The sample type in question. Can be any sample type supported by HealthKit.
 */
- (HKUnit *)defaultUnitForSampleType:(HKSampleType *)sampleType;


// ----------------------------------



@end
