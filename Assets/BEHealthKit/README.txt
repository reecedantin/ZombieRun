Introduction
============

BEHealthKit is a simple Unity plugin for Apple's HealthKit framework. It allows you to read health data on iPhones or iPod Touches into a Unity app.

HealthKit was introduced in iOS 8.0. For an overview of its capabilities, [Apple's HealthKit page](https://developer.apple.com/healthkit/) is a good place to start.

This (and further) documentation is also available online here: <http://beliefengine.com/BEHealthKit/documentation/>.


Setting Up
==========

As of BEHealthKit 1.5, I've included a build post-processor script to automatically modify the Xcode project, so in most cases it's safe to skip this section & jump straight to Basic Usage.
But if for some reason you don't want to use that, or something goes wrong, these are the steps you need to take to get the Xcode project ready to build:

First, *and this is important*, when you build your Xcode project, make sure you add HealthKit to your target capabilities. If you don't know where this is:

  1. Select your project in the project navigator (this will most likely be the very first thing, called "Unity-iPhone" in the left-hand navigation)
  2. In the left-hand side of the main view, you'll see one project and two targets. Select the main target (also probably called "Unity-iPhone")
  3. Before the next step, you will probably need to change the Base SDK to "Latest iOS" – Unity sets this to something Xcode doesn't quite understand, and several of the tabs (like "Capabilities") will be missing or empty. Then close & reopen the project (or restart Xcode).
  4. You should see a few "tabs" – General, Capabilities, Info, Build Settings, et cetera. Select "Capabilities"
  5. Scroll down until you see HealthKit (probably near the bottom) and turn it to "ON". This'll automatically do a few required things, like add the HealthKit framework to your Xcode project.

And finally, **NSHealthShareUsageDescription** and **NSHealthUpdateUsageDescription**. This requirement was introduced in iOS 10 – you need to add a short description of what your app intends to use the health data for.
**NSHealthShareUsageDescription** describes the reasons for reading the user's health data. The corresponding **NSHealthUpdateUsageDescription** describes why your app wants to write health data. For more information, see
[the documentation](https://developer.apple.com/library/content/documentation/General/Reference/InfoPlistKeyReference/Articles/CocoaKeys.html#//apple_ref/doc/uid/TP40009251-SW48).

 1. In the project navigator, select Info.plist
 2. Hover over the header of the first column (*Key*), revealing a "+", and click it to add a new entry
 3. For the key, enter "NSHealthShareUsageDescription"
 4. For the type, select *String* if it isn't already
 5. Enter the required text in the *Value* column.
 6. If your app intends to write health data as well, repeat 1-5 for "NSHealthUpdateUsageDescription".
 7. All done! You should be good to go.


Basic Usage
===========

First things first, since HealthKit was introduced in iOS 8 you'll need to set your target iOS version in Unity to 8.0. Also, I recommend setting the scripting backend to IL2CPP.

There are two key behavior scripts: HealthStore and HealthKitDataTypes. Attach HealthKitDataTypes to any object in your scene, and it provides inspector UI (in the editor) to check all the data types you want your app to be able to read. Attach HealthStore to any object, and call Authorize(), supplying the HealthKitDataTypes object (e.g. like this):

	this.healthStore = this.GetComponent<HealthStore>();
	this.healthStore.Authorize(this.dataTypes);

This'll pop up the native iOS UI where your user can choose to authorize your app to read the supplied data types. **NOTE:** *they can choose not to authorize some or all of the types, so be sure to handle this in your app!*

From there, it's fairly simple to read data. For the most part, each function takes a data type, a start time, an end time, and a delegate (to handle the response).

So for example, let’s say we want to read the user’s steps over the last 24 hours, which are stored as HKDataType.HKQuantityTypeIdentifierStepCount. Here's one way:

	DateTimeOffset now = DateTimeOffset.UtcNow;
	DateTimeOffset start = now.AddDays(-1);
	healthStore.ReadQuantitySamples(HKDataType.HKQuantityTypeIdentifierStepCount, start, now, delegate(List<QuantitySample> samples) {
		foreach (QuantitySample sample in samples) {
			Debug.Log(String.Format(“ - {0} from {1} to {2}”, sample.quantity.doubleValue, sample.startDate, sample.endDate);
		}
	});

Alternatively, if you'd rather keep your parsing contained in its own method, you could do the following:

	public void ProcessData(List<QuantitySample> samples) {
		foreach (QuantitySample sample in samples) {
			Debug.Log(String.Format(“ - {0} from {1} to {2}”, sample.quantity.doubleValue, sample.startDate, sample.endDate);
		}
	}

And then somewhere else, do basically the same as before (but plug in that method instead of the inline function):

	DateTimeOffset now = DateTimeOffset.UtcNow;
	DateTimeOffset start = now.AddDays(-1);
	healthStore.ReadQuantitySamples(HKDataType.HKQuantityTypeIdentifierStepCount, start, now, new ReceivedQuantitySamples(ProcessData));


Health Share & Update Usage
---------------------------

In order to submit to the iTunes store, it's required that any app that reads health data supply a "Health Share Usage Description". This text is presented to the user and describes what your app intends to do with the information, along with what data types you are requesting to read. Likewise, the "Health Update Usage Description" describes your intent to write health data, if you do. For more information on these two keys, see 
[the documentation](https://developer.apple.com/library/content/documentation/General/Reference/InfoPlistKeyReference/Articles/CocoaKeys.html#//apple_ref/doc/uid/TP40009251-SW48).

The build postprocessor script included with BEHealthKit automatically determines which keys need to be included in the Xcode project, based on the data types you've indicated as wanting read or write permission, so it's only necessary to supply text for the ones you need. Otherwise, the placeholder "for testing" text is fine. 


Understanding the data
======================

All of the delegates (except for a few convenience methods) return their data in classes pretty closely modeled after their HealthKit counterparts. It's probably worthwhile to check out Apple's [official documentation](https://developer.apple.com/library/prerelease/ios/documentation/HealthKit/Reference/HealthKit_Framework/index.html#//apple_ref/doc/uid/TP40014707), but by no means necessary. The C# classes are all contained in HealthData.cs.

Probably the most useful classes are QuantitySample / CategorySample, and Quantity (these mirror HKQuantitySample, HKCategorySample, and HKQuantity). QuantitySample is used for body measurements, fitness, vitals, test results & nutrition. CategorySamples are used for things like sleep or ovulation tracking.

Quantity Samples
----------------

These all have a startDate and an endDate (as DateTimeOffset). They also have a quantityType, which is either QuantityType.cumulative or QuantityType.discrete.
Cumulative is for values that can be summed over time, like steps or nutritional information. Discrete is for things like body mass or heart rate. Finally, the actual
quantity is stored in a Quantity object. This wraps a unit (as a string) and, for simplicity's sake, all values as a doubleValue. Unlike the HealthKit library, I don't
currently support requesting values in arbitrary compatible units, so some conversion will probably be necessary. For example, the default unit for volume is liters, so if you want something else you'll have to do the conversion. Likewise, the default unit for mass is either pounds or kilograms, depending on the user's locale. These are probably fine for body mass, but won't be as useful for nutritional data.

I do plan on adding support for arbitrary units in a future update (soon).

Category Samples
----------------

Like all Sample types, this also has a startDate and an endDate. CategorySamples, however, only have one other property: value. This is an returned as an int, although it should be converted to the appropriate enum in HealthKitDataTypes.

For sleep data, it's probably worth reading the [HKCategoryValueSleepAnalysis](https://developer.apple.com/library/ios/documentation/HealthKit/Reference/HealthKit_Constants/#//apple_ref/c/tdef/HKCategoryValueSleepAnalysis) documentation to understand what you're looking at. Basically a value of 0 means "in bed", and a 1 means "asleep".  These *will* overlap, assuming a good HealthKit citizen is writing the data.

Likewise, for menstrual flow, the [HKCategoryValueMenstrualFlow](https://developer.apple.com/library/ios/documentation/HealthKit/Reference/HealthKit_Constants/#//apple_ref/c/tdef/HKCategoryValueMenstrualFlow) documentation explains how the values are represented. For these, the same period may be represented by multiple samples.

The others are generally self-explanatory, except for IntermenstrualBleeding & SexualActivity – these will always be 0 (HKCategoryValueNotApplicable). Sexual activity samples, in particular, may include metadata indicating whether or not protection was used – SexualActivityProtectionUsed.

Characteristics
---------------

Characteristics are immutable, so reading them doesn't take date ranges. As of iOS 9.0, there are 4 possible characteristics: Biological Sex, Blood Type, Date of Birth, and Fitzpatrick Skin Type. Any of these can have a value of "NotSet" (or null, for date of birth).

Correlation Samples
-------------------

Correlations are a little trickier. Like other Samples, they have a startDate and an endDate. But Correlations are used to examine multiple pieces of information. For example, blood pressure is stored as a correlation type: it contains 2 discrete samples; one for systolic and and one for diastolic values. Nutrition correlation samples can contain a range of dietary information, such as fat, protein, carbohydrates etc.

Workout Samples
---------------

Workout samples, in addition to a start & end date, have a duration (which, in nearly all cases, will probably be the difference between the start & end date. But it's possible it could be different, for example, if an app decided to subtract a rest period between the start & end).  It also has a workout type (e.g. running, cardio, etc.), and totalDistance & totalEnergyBurned properties. Not all workouts will record distance & energy burned, however.

Finally, some workouts will include a list of WorkoutEvents. These include a date & a type, which will be either Pause or Resume.


Writing data
============

Writing health data is similar to reading it. To write a sample, you'll need a start date and an end date. It it's a quantity sample (e.g. walking/running distance), you'll need to create a Quantity object. This takes a quantity (as a double), and a unit (as a string). A description of the valid strings can be found in [Apple's documentation](https://developer.apple.com/reference/healthkit/hkunit/1615733-unitfromstring). Then simply call WriteQuantitySample on the HealthStore object, supplying the data type, quantity, and start & end date of the sample.

		DateTimeOffset now = DateTimeOffset.UtcNow;
		DateTimeOffset start = now.AddMinutes(-10);
		Quantity quantity = new Quantity("mi", 0.5);
		
		this.healthStore.WriteQuantitySample(HKDataType.HKQuantityTypeIdentifierDistanceWalkingRunning, quantity, start, now);




HealthStore Methods
===================

First is the IsHealthDataAvailable() method. This simply returns a boolean; it will return true if HealthKit is supported by the device, false otherwise.

Then there's Authorize() which takes a single parameter, the HealthKitDataTypes object where you select which data types to request authorization for.

Finally, there's the methods to actually read the data:

 - ReadQuantitySamples: read quantity samples for the given datatype, between a start & end date.
 - ReadCombinedQuantitySamples: same as above, but combine them into a single value. Useful for things like steps in a given period, or Calories / day.
 - ReadCategorySamples: read category data between a start & end date.


Scripts
=======

Editor
------

 - HealthKitBuildProcessor :	Automatically adds the proper settings to the Xcode project.
 - HealthKitDataTypesEditor :	Custom editor for HealthKitDataTypes. Attach that (HealthKitDataTypes) to any object in your scene & check the data types you want your app to read. In v1, this was a Property Drawer.

Example
-------

 - HealthKitTest : 				Contains the logic for the example scene. It demonstrates authorization & if you tap on the "Read Data" button, will read step samples for the
 								last 24 hours.
 - HealthKitFullTest :			Contains the logic for a more detailed example scene. This includes a couple different example methods, as well as a drop-down to read any of the
 								data types that HealthKit supports.

iOS
---

This is all the native code. Of use if you want a better understanding of how the plugin works, or want to extend / change it.

 - BEHealthKit : 				Handles HealthKit requests from Unity. This is the heart of the plugin.
 - BEHealthKit+dummyData : 		Category for generating dummy data (mainly for use in the iOS Simulator)
 - BEHealthKit+Unity :			Point-of-contact for the Unity plugin.
 - BEPedometer :				Handles reading from the pedometer, for real-time step data.
 - HealthData : 				Helper class to generate XML from HealthKit data, for sending to Unity.
 - NSDate+bridge :	 			Category for bridging dates between C# and Objective-C
 - NSError+XML : 				Helper category to generate XML from an NSError, for sending to Unity.
 - XMLDictionary : 				Third-party helper class to ease XML generation from an NSDictionary.

Plugins
-------

 - DateTimeBridge :				A small helper class to bridge dates between C# and Objective-C. C# counterpart to NSDate+bridge.
 - Error :						Wraps information about an error from HealthKit. C# counterpart to NSError+XML.
 - HealthData :					Used to send HealthKit data back to Unity. In addition to the primary HealthData class which parses the XML sent by the plugin, contains a
 								collection of C# classes to mirror HealthKit's data model.
 - HealthKitDataTypes :			Contains information about all the HealthKit data types, and is a wrapper for the data types to authorize. Used to create the editor UI.
 								You'll need it for the authorization step.
 - HealthStore :				This is the primary interface for HealthKit. Allows you to request authorization to read data (supplying a HealthKitDataTypes object), and
 								read the various types of health data.


TODO
====

There's a lot more to be done with this. My immediate goal is to extend support for all basic functionality. Currently I'm only *officially* supporting Quantity and Category types, so in rough order of priority:

 - let you supply units if you don't want whatever the default is
 - improve the documentation
 - other general improvements

And further down the road, I'd like to add support for Google's and Microsoft's health libraries, but HealhKit is the focus for now.


Support
=======

For questions, bug reports, suggestions, or if you just want to chat, email <support@beliefengine.com>.
