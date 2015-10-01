//
//  NendPlugin.mm
//  Unity-iPhone
//

#import "NendPlugin.h"

#import <objc/runtime.h>

static const char* INTERSTITIAL_GAME_OBJECT = "NendAdInterstitial";

extern UIView* UnityGetGLView();
extern UIViewController* UnityGetGLViewController();

static NSMutableDictionary* _bannerAdDict = [NSMutableDictionary new];

enum NendGravity {
    LEFT = 1,
    TOP = 2,
    RIGHT = 4,
    BOTTOM = 8,
    CENTER_VERTICAL = 16,
    CENTER_HORIZONTAL = 32,
};

enum NendBannerSize {
    SIZE_320X50 = 0,
    SIZE_320X100 = 1,
    SIZE_300X100 = 2,
    SIZE_300X250 = 3,
    SIZE_728X90 = 4,
};

static NSString* CreateNSString(const char* string)
{
    if (string)
        return @(string);
    else
        return @"";
}

static CGSize BannerSize(NendBannerSize size)
{
    switch (size) {
        case SIZE_320X50:
            return CGSizeMake(320, 50);
        case SIZE_320X100:
            return CGSizeMake(320, 100);
        case SIZE_300X100:
            return CGSizeMake(300, 100);
        case SIZE_300X250:
            return CGSizeMake(300, 250);
        case SIZE_728X90:
            return CGSizeMake(728, 90);
        default:
            return CGSizeZero;
    }
}

static CGPoint CalculatePoint(NSInteger gravity, CGSize viewSize, NSInteger left, NSInteger top, NSInteger right, NSInteger bottom)
{
    CGPoint point = CGPointZero;
    CGSize screenSize = UnityGetGLView().bounds.size;

    if (0 != (gravity & CENTER_HORIZONTAL))
        point.x = (screenSize.width - viewSize.width) / 2;
    if (0 != (gravity & RIGHT))
        point.x = screenSize.width - viewSize.width;
    if (0 != (gravity & LEFT))
        point.x = 0.0f;

    if (0 != (gravity & CENTER_VERTICAL))
        point.y = (screenSize.height - viewSize.height) / 2;
    if (0 != (gravity & BOTTOM))
        point.y = screenSize.height - viewSize.height;
    if (0 != (gravity & TOP))
        point.y = 0.0f;

    point.x += left;
    point.y += top;
    point.x -= right;
    point.y -= bottom;

    return point;
}

static BOOL ShouldAutorotateIMP(id self, SEL _cmd)
{
    if ([UnityGetGLViewController() respondsToSelector:@selector(shouldAutorotate)])
        return [UnityGetGLViewController() shouldAutorotate];
    else
        return YES;
}

static NSUInteger SupportedInterfaceOrientationsIMP(id self, SEL _cmd)
{
    if ([UnityGetGLViewController() respondsToSelector:@selector(supportedInterfaceOrientations)])
        return [UnityGetGLViewController() supportedInterfaceOrientations];
    else
        return UIInterfaceOrientationMaskAll;
}

static void AddRotateMethodToInterstitialViewController()
{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        Class cls = NSClassFromString(@"NADInterstitialViewController");
        class_replaceMethod(cls, @selector(shouldAutorotate), (IMP)&ShouldAutorotateIMP, "c8@0:4");
        class_replaceMethod(cls, @selector(supportedInterfaceOrientations), (IMP)&SupportedInterfaceOrientationsIMP, "I8@0:4");
    });
}

static NSInteger InterstitialStatusCodeToInteger(NADInterstitialStatusCode status)
{
    switch (status) {
        case SUCCESS:
            return 0;
        case INVALID_RESPONSE_TYPE:
            return 1;
        case FAILED_AD_REQUEST:
            return 2;
        case FAILED_AD_DOWNLOAD:
            return 3;
        default:
            return -1;
    }
}

static NSInteger InterstitialClickTypeToInteger(NADInterstitialClickType type)
{
    switch (type) {
        case DOWNLOAD:
            return 0;
        case CLOSE:
            return 1;
        default:
            return -1;
    }
}

///-----------------------------------------------
/// @name Implementations
///-----------------------------------------------

@implementation NADViewEventDispatcher

- (instancetype)initWithGameObject:(NSString*)gameObject
{
    self = [super init];
    if (self) {
        _gameObject = [gameObject copy];
        _loadCompletionHandler = NULL;
    }
    return self;
}

- (void)dealloc
{
    [_gameObject release];
    if (_loadCompletionHandler) {
        Block_release(_loadCompletionHandler);
        _loadCompletionHandler = NULL;
    }
    [super dealloc];
}

- (void)nadViewDidFinishLoad:(NADView*)adView
{
    if (self.loadCompletionHandler) {
        self.loadCompletionHandler();
        self.loadCompletionHandler = NULL;
    }
    UnitySendMessage([self.gameObject UTF8String], "NendAdView_OnFinishLoad", "");
}

- (void)nadViewDidFailToReceiveAd:(NADView*)adView
{
    NSString* message = [NSString stringWithFormat:@"%ld:%@", (long)adView.error.code, adView.error.domain];
    UnitySendMessage([self.gameObject UTF8String], "NendAdView_OnFailToReceiveAd", (char *)[message UTF8String]);
}

- (void)nadViewDidReceiveAd:(NADView*)adView
{
    UnitySendMessage([self.gameObject UTF8String], "NendAdView_OnReceiveAd", "");
}

- (void)nadViewDidClickAd:(NADView*)adView
{
    UnitySendMessage([self.gameObject UTF8String], "NendAdView_OnClickAd", "");
}

@end

//==============================================================================

@implementation NADInterstitialEventDispatcher

+ (instancetype)sharedDispatcher
{
    static NADInterstitialEventDispatcher* instance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        instance = [[NADInterstitialEventDispatcher alloc] init];
    });
    return instance;
}

- (void)dispatchShowResult:(NADInterstitialShowResult)result spotId:(NSString*)spotId
{
    NSInteger value = -1;
    switch (result) {
        case AD_SHOW_SUCCESS:
            value = 0;
            break;
        case AD_LOAD_INCOMPLETE:
            value = 1;
            break;
        case AD_REQUEST_INCOMPLETE:
            value = 2;
            break;
        case AD_DOWNLOAD_INCOMPLETE:
            value = 3;
            break;
        case AD_FREQUENCY_NOT_REACHABLE:
            value = 4;
            break;
        case AD_SHOW_ALREADY:
            value = 5;
            break;
    }
    NSString* message = [NSString stringWithFormat:@"%ld:%@", (long)value, spotId];
    UnitySendMessage(INTERSTITIAL_GAME_OBJECT, "NendAdInterstitial_OnShowAd", (char *)[message UTF8String]);
}

- (void)didFinishLoadInterstitialAdWithStatus:(NADInterstitialStatusCode)status
{
    NSInteger value = InterstitialStatusCodeToInteger(status);
    NSString* message = [[NSNumber numberWithInteger:value] stringValue];
    UnitySendMessage(INTERSTITIAL_GAME_OBJECT, "NendAdInterstitial_OnFinishLoad", (char *)[message UTF8String]);
}

- (void)didFinishLoadInterstitialAdWithStatus:(NADInterstitialStatusCode)status spotId:(NSString*)spotId
{
    NSInteger value = InterstitialStatusCodeToInteger(status);
    NSString* message = [NSString stringWithFormat:@"%ld:%@", (long)value, spotId];
    UnitySendMessage(INTERSTITIAL_GAME_OBJECT, "NendAdInterstitial_OnFinishLoad", (char *)[message UTF8String]);
}

- (void)didClickWithType:(NADInterstitialClickType)type
{
    NSInteger value = InterstitialClickTypeToInteger(type);
    NSString* message = [[NSNumber numberWithInteger:value] stringValue];
    UnitySendMessage(INTERSTITIAL_GAME_OBJECT, "NendAdInterstitial_OnClickAd", (char *)[message UTF8String]);
}

- (void)didClickWithType:(NADInterstitialClickType)type spotId:(NSString*)spotId
{
    NSInteger value = InterstitialClickTypeToInteger(type);
    NSString* message = [NSString stringWithFormat:@"%ld:%@", (long)value, spotId];
    UnitySendMessage(INTERSTITIAL_GAME_OBJECT, "NendAdInterstitial_OnClickAd", (char *)[message UTF8String]);
}

@end

//==============================================================================

@implementation BannerParams

+ (instancetype)paramWithString:(NSString*)paramString
{
    return [[[BannerParams alloc] initWithParamString:paramString] autorelease];
}

- (instancetype)initWithParamString:(NSString*)paramString
{
    self = [super init];
    if (self) {
        NSArray* paramArray = [paramString componentsSeparatedByString:@":"];
        int index = 0;
        _gameObject = [(NSString*)paramArray[index++] copy];
        _apiKey = [(NSString*)paramArray[index++] copy];
        _spotID = [(NSString*)paramArray[index++] copy];
        _outputLog = [@"true" isEqualToString:(NSString*)paramArray[index++]];
        _adjustSize = [@"true" isEqualToString:(NSString*)paramArray[index++]];
        _size = [paramArray[index++] integerValue];
        _gravity = [paramArray[index++] integerValue];
        _leftMargin = [paramArray[index++] integerValue];
        _topMargin = [paramArray[index++] integerValue];
        _rightMargin = [paramArray[index++] integerValue];
        _bottomMargin = [paramArray[index++] integerValue];
    }
    return self;
}

- (void)updateLayoutWithString:(NSString*)paramString
{
    NSArray* paramArray = [paramString componentsSeparatedByString:@":"];
    int index = 0;
    self.gravity = [paramArray[index++] integerValue];
    self.leftMargin = [paramArray[index++] integerValue];
    self.topMargin = [paramArray[index++] integerValue];
    self.rightMargin = [paramArray[index++] integerValue];
    self.bottomMargin = [paramArray[index++] integerValue];
}

- (void)dealloc
{
    [_gameObject release];
    [_apiKey release];
    [_spotID release];
    [super dealloc];
}

@end

//==============================================================================

@implementation NendAdBanner

+ (instancetype)bannerAdWithParams:(BannerParams*)params
{
    return [[[NendAdBanner alloc] initWithParams:params] autorelease];
}

- (instancetype)initWithParams:(BannerParams*)params;
{
    self = [super init];
    if (self) {
        _params = [params retain];
        _rotationHandler = [NendRotationHandler new];

        _bannerView = [[NADView alloc] initWithIsAdjustAdSize:params.adjustSize];
        _bannerView.hidden = YES;
        
        NADViewEventDispatcher* dispatcher = [[NADViewEventDispatcher alloc] initWithGameObject:params.gameObject];
        __block NendAdBanner* weakSelf = self;
        dispatcher.loadCompletionHandler = ^{
            [weakSelf layout];
        };
        
        _bannerView.delegate = dispatcher;
        _bannerView.isOutputLog = params.outputLog;
        [_bannerView setNendID:params.apiKey spotID:params.spotID];
    }

    return self;
}

- (void)dealloc
{
    [_rotationHandler release];

    [_bannerView removeFromSuperview];
    [_bannerView.delegate release];
    _bannerView.delegate = nil;

    [_bannerView release];
    [_params release];

    _bannerView = nil;
    _params = nil;

    [super dealloc];
}

- (void)load
{
    if (self.bannerView)
        [self.bannerView load];
}

- (void)show
{
    if (self.bannerView && self.bannerView.hidden) {
        __block NendAdBanner* weakSelf = self;
        [self.rotationHandler startHandlingUsingBlock:^{
            [weakSelf didRotate];
        }];
        [self layout];
        self.bannerView.hidden = NO;
    }
}

- (void)hide
{
    if (self.bannerView && !self.bannerView.hidden) {
        [self.rotationHandler stopHandling];
        self.bannerView.hidden = YES;
    }
}

- (void)resume
{
    if (self.bannerView)
        [self.bannerView resume];
}

- (void)pause
{
    if (self.bannerView)
        [self.bannerView pause];
}

- (void)layout
{
    if (!self.bannerView)
        return;
    
    CGSize bannerSize;
    if (self.params.adjustSize) {
        bannerSize = self.bannerView.frame.size;
        if (0.f == bannerSize.width || 0.f == bannerSize.height) {
            return;
        }
    } else {
        bannerSize = BannerSize((NendBannerSize)self.params.size);
    }
    
    CGPoint point = CalculatePoint(self.params.gravity, bannerSize, self.params.leftMargin, self.params.topMargin, self.params.rightMargin, self.params.bottomMargin);
    self.bannerView.frame = CGRectMake(point.x, point.y, bannerSize.width, bannerSize.height);
}

- (void)updateLayoutWithString:(NSString*)paramString
{
    [self.params updateLayoutWithString:paramString];
    [self layout];
}

- (void)didRotate
{
    if (self.bannerView && !self.bannerView.hidden) {
        [self layout];
    }
}

@end

//==============================================================================

@implementation NendRotationHandler

- (instancetype)init
{
    self = [super init];
    if (self) {
        [[NSNotificationCenter defaultCenter] addObserver:self
                                                 selector:@selector(didRotate:)
                                                     name:UIDeviceOrientationDidChangeNotification
                                                   object:nil];

    }
    return self;
}

- (void)startHandlingUsingBlock:(dispatch_block_t)block
{
    self.block = block;
}

- (void) stopHandling
{
    self.block = NULL;
}

- (void)dealloc
{
    [[NSNotificationCenter defaultCenter] removeObserver:self name:UIDeviceOrientationDidChangeNotification object:nil];
    if (_block) {
        Block_release(_block);
        _block = NULL;
    }
    [super dealloc];
}

- (void)didRotate:(NSNotification *)note
{
    if (self.block) {
        dispatch_async(dispatch_get_main_queue(), ^{
            self.block();
        });
    }
}

@end

//==============================================================================

void _TryCreateBanner(const char* paramString)
{
    BOOL didLoaded = NO;
    BannerParams* params = [BannerParams paramWithString:CreateNSString(paramString)];
    NendAdBanner* ad = _bannerAdDict[params.gameObject];

    if (!ad) {
        ad = [NendAdBanner bannerAdWithParams:params];
        _bannerAdDict[params.gameObject] = ad;
    } else
        didLoaded = YES;

    if (!ad.bannerView.superview)
        [UnityGetGLView() addSubview:ad.bannerView];

    if (!didLoaded)
        [ad.bannerView load];
}

void _ShowBanner(const char* gameObject)
{
    NendAdBanner* ad = _bannerAdDict[CreateNSString(gameObject)];
    if (ad)
        [ad show];
}

void _HideBanner(const char* gameObject)
{
    NendAdBanner* ad = _bannerAdDict[CreateNSString(gameObject)];
    if (ad)
        [ad hide];
}

void _ResumeBanner(const char* gameObject)
{
    NendAdBanner* ad = _bannerAdDict[CreateNSString(gameObject)];
    if (ad)
        [ad resume];
}

void _PauseBanner(const char* gameObject)
{
    NendAdBanner* ad = _bannerAdDict[CreateNSString(gameObject)];
    if (ad)
        [ad pause];
}

void _DestroyBanner(const char* gameObject)
{
    [_bannerAdDict removeObjectForKey:CreateNSString(gameObject)];
}

void _LayoutBanner(const char* gameObject, const char* paramString)
{
    NendAdBanner* ad = _bannerAdDict[CreateNSString(gameObject)];
    if (ad)
        [ad updateLayoutWithString:CreateNSString(paramString)];
}

void _LoadInterstitialAd(const char* apiKey, const char* spotId, BOOL isOutputLog)
{
    AddRotateMethodToInterstitialViewController();

    [NADInterstitial sharedInstance].isOutputLog = isOutputLog;
    [NADInterstitial sharedInstance].delegate = [NADInterstitialEventDispatcher sharedDispatcher];

    [[NADInterstitial sharedInstance] loadAdWithApiKey:CreateNSString(apiKey) spotId:CreateNSString(spotId)];
}

void _ShowInterstitialAd(const char* spotId)
{
    NSString* spot = CreateNSString(spotId);
    NADInterstitialShowResult result;
    if (spot && 0 < spot.length)
        result = [[NADInterstitial sharedInstance] showAdWithSpotId:spot];
    else
        result = [[NADInterstitial sharedInstance] showAd];

    [[NADInterstitialEventDispatcher sharedDispatcher] dispatchShowResult:result spotId:spot];
}

void _DismissInterstitialAd()
{
    [[NADInterstitial sharedInstance] dismissAd];
}