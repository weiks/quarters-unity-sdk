#include "UnityAppController.h"

@interface CustomUrlSub : UnityAppController 
@end

@implementation CustomUrlSub
- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation
{
  if( url != NULL ) {
    const char * urlString = [[url absoluteString] UTF8String];
    UnitySendMessage( "Quarters", "DeepLink", urlString );
  }
  return [super application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
}
@end

IMPL_APP_CONTROLLER_SUBCLASS( CustomUrlSub );
