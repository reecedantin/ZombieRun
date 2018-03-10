//
//  NSDate+bridge.m
//  Unity-iPhone
//
//  Created by greay on 3/28/15.
//
//

#import "NSDate+bridge.h"

@implementation NSDate (conversion)

+ (NSNumberFormatter *)bridgeFormatter
{
	static NSNumberFormatter *format = nil;
	static dispatch_once_t onceToken;
	dispatch_once(&onceToken, ^{
		format = [[NSNumberFormatter alloc] init];
		format.numberStyle = NSNumberFormatterDecimalStyle;
	});
	return format;
}


+ (instancetype)dateFromToken:(NSNumber *)n
{
	NSDate *date = [NSDate dateWithTimeIntervalSince1970:(NSTimeInterval)[n doubleValue]];
	return date;
}

+ (instancetype)dateFromBridgeString:(char *)stamp
{
	NSNumber *n = [[self bridgeFormatter] numberFromString:[NSString stringWithCString:stamp encoding:NSUTF8StringEncoding]];
	return [self dateFromToken:n];
}

- (NSNumber *)bridgeToken
{
	NSTimeInterval token = [self timeIntervalSince1970];
	return @(token);
}

@end
