//
//  UniversalDeepLink.h
//  UniversalDeepLink
//
//  Created by Ana Correia on 27/04/2018.
//  Copyright © 2018 ImaginationOverflow. All rights reserved.
//

#import <Cocoa/Cocoa.h>
#import <Foundation/Foundation.h>
#import <AppKit/AppKit.h>

//! Project version number for UniversalDeepLink.
FOUNDATION_EXPORT double UniversalDeepLink_OSXVersionNumber;

//! Project version string for UniversalDeepLink.
FOUNDATION_EXPORT const unsigned char UniversalDeepLink_OSXVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <UniversalDeepLink/PublicHeader.h>

#import "Swizzler.h"
#import "UniversalDeepLinkAppDelegate.h"


@interface UniversalDeepLink : NSObject
@end

