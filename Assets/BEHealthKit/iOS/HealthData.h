//
//  HealthData.h
//  Unity-iPhone
//
//  Created by greay on 3/28/15.
//
//

#import <Foundation/Foundation.h>
#import <CoreMotion/CoreMotion.h>

/*! @brief Helper class to generate XML from HealthKit data, for sending to Unity
 */
@interface HealthData : NSObject

+ (NSString *)XMLFromQuantitySamples:(NSArray *)quantitySamples datatype:(NSString *)datatype;          /*!< @brief generate XML from an array of QuantitySamples */
+ (NSString *)XMLFromCategorySamples:(NSArray *)categorySamples datatype:(NSString *)datatype;          /*!< @brief generate XML from an array of CategorySamples */
+ (NSString *)XMLFromCorrelationSamples:(NSArray *)correlationSamples datatype:(NSString *)datatype;    /*!< @brief generate XML from an array of CorrelationSamples */
+ (NSString *)XMLFromCharacteristic:(id)characteristic datatype:(NSString *)datatype;                   /*!< @brief generate XML for a Characteristic */
+ (NSString *)XMLFromWorkoutSamples:(NSArray *)workoutSamples workoutType:(int)workoutType;             /*!< @brief generate XML from an array of WorkoutSamples */


+ (NSString *)XMLFromCombinedTotal:(double)total; /*!< @brief generate XML from an a total of combined QuantitySamples */

+ (NSString *)XMLFromPedometerData:(CMPedometerData *)data; /*!< @brief generate XML from CMPedometerData */

@end
