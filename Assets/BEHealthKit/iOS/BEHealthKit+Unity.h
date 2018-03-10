//
//  BEHealthKit+Unity.h
//  Unity-iPhone
//
//  Created by greay on 3/25/15.
//
//

#import <Foundation/Foundation.h>
#import <HealthKit/HealthKit.h>


/*! @brief Point-of-contact for the Unity plugin.
 */

// hooks for external interface
void _InitializeNative(char *controllerName);
void _Authorize(char *dataTypesString);
int _AuthorizationStatusForType(char *dataTypeString);
BOOL _IsHealthDataAvailable();

void _ReadQuantity(char *identifier, char *startDateString, char *endDateString, bool combineSamples);
void _ReadCategory(char *identifier, char *startDateString, char *endDateString);
void _ReadCharacteristic(char *identifier);
void _ReadCorrelation(char *identifier, char *startDateString, char *endDateString, bool combineSamples);
void _ReadWorkout(int activityID, char *startDateString, char *endDateString, bool combineSamples);

void _GenerateDummyData();

// -----------------------------

void _ReadPedometer(char *startDateString, char *endDateString);
void _StartReadingPedometerFromDate(char *startDateString);
void _StopReadingPedometer();
