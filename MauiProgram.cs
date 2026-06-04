//using Microsoft.Extensions.Logging;
//using Microsoft.Maui.LifecycleEvents;
//using Plugin.Firebase.Analytics;

//#if IOS
//using Plugin.Firebase.Bundled.Platforms.iOS;
//#elif ANDROID
//using Plugin.Firebase.Bundled.Platforms.Android;
//using Plugin.Firebase.Bundled.Shared;
//#endif

//namespace MauiApp2;

//public static class MauiProgram
//{
//    public static MauiApp CreateMauiApp()
//    {
//        var builder = MauiApp.CreateBuilder();

//        builder
//            .UseMauiApp<App>()
//            .RegisterFirebaseServices()
//            .ConfigureFonts(fonts =>
//            {
//                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
//                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
//            });

//#if DEBUG
//        builder.Logging.AddDebug();
//#endif

//        return builder.Build();
//    }

//    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
//    {
//        builder.ConfigureLifecycleEvents(events =>
//        {
//#if IOS
//            events.AddiOS(iOS => iOS.WillFinishLaunching((_, _) =>
//            {
//                CrossFirebase.Initialize(CreateCrossFirebaseSettings());
//                return false;
//            }));
//#elif ANDROID
//            events.AddAndroid(android => android.OnCreate((activity, _) =>
//            {
//                CrossFirebase.Initialize(
//                    activity,
//                    () => Microsoft.Maui.ApplicationModel.Platform.CurrentActivity,
//                    CreateCrossFirebaseSettings()
//                );

//                // Initialize Analytics
//                FirebaseAnalyticsImplementation.Initialize(activity);
//            }));
//#endif
//        });

//        builder.Services.AddSingleton(_ => CrossFirebaseAnalytics.Current);

//        return builder;
//    }


//#if IOS || ANDROID
//    private static Plugin.Firebase.Shared.CrossFirebaseSettings CreateCrossFirebaseSettings()
//    {
//        return new Plugin.Firebase.Shared.CrossFirebaseSettings(
//            isAnalyticsEnabled: true,
//            isAuthEnabled: false,
//            isCloudMessagingEnabled: false,
//            isCrashlyticsEnabled: false,
//            isDynamicLinksEnabled: false,
//            isFirestoreEnabled: false,
//            isFunctionsEnabled: false,
//            isRemoteConfigEnabled: false,
//            isStorageEnabled: false
//        );
//    }
//#endif
//}

using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

#if IOS || ANDROID
using Plugin.Firebase.Analytics;
using Plugin.Firebase.Crashlytics;
#endif

#if IOS
using Plugin.Firebase.Core.Platforms.iOS;
#elif ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#endif

namespace MauiApp2;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .RegisterFirebaseServices()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
    {
#if IOS || ANDROID
        builder.ConfigureLifecycleEvents(events =>
        {
#if IOS
            events.AddiOS(iOS => iOS.WillFinishLaunching((_, _) =>
            {
                CrossFirebase.Initialize();
                return false;
            }));
#elif ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) =>
            {
                CrossFirebase.Initialize(activity, () => Microsoft.Maui.ApplicationModel.Platform.CurrentActivity);
                FirebaseAnalyticsImplementation.Initialize(activity);
            }));
#endif
        });

        builder.Services.AddSingleton(_ => CrossFirebaseAnalytics.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseCrashlytics.Current);
#endif

        return builder;
    }
}