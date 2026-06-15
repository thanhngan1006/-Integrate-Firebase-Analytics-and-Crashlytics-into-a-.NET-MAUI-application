
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

#if IOS || ANDROID
using Plugin.Firebase.Analytics;
using Plugin.Firebase.Crashlytics;
using Plugin.Firebase.Bundled.Shared;
#endif

#if IOS
using ObjCRuntime;
using Plugin.Firebase.Bundled.Platforms.iOS;
#elif ANDROID
using Plugin.Firebase.Bundled.Platforms.Android;
#endif

namespace MauiApp2;

public static class MauiProgram
{
    public static string? FirebaseInitError { get; private set; }
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
                try
                {
                    CrossFirebase.Initialize(CreateCrossFirebaseSettings());
                    CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(true);
                    CrossFirebaseCrashlytics.Current.SendUnsentReports();

                    // Required: route managed .NET exceptions through native unwind path
                    // so Crashlytics (a native crash reporter) can intercept them.
                    // Without this, managed exceptions are invisible to Crashlytics on iOS.
                    Runtime.MarshalManagedException += (_, args) =>
                    {
                        args.ExceptionMode = MarshalManagedExceptionMode.UnwindNativeCode;
                    };

                    // Record unhandled managed exceptions to Crashlytics before the process dies
                    AppDomain.CurrentDomain.UnhandledException += (_, args) =>
                    {
                        if (args.ExceptionObject is Exception ex)
                        {
                            CrossFirebaseCrashlytics.Current.Log($"Fatal: {ex.GetType().Name}: {ex.Message}");
                            CrossFirebaseCrashlytics.Current.RecordException(ex);
                        }
                    };
                }
                catch (Exception ex)
                {
                    FirebaseInitError = $"{ex.GetType().Name}: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"Firebase init error: {ex}");
                }
                return true;
            }));
#elif ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) =>
            {
                CrossFirebase.Initialize(
                    activity,
                    () => Microsoft.Maui.ApplicationModel.Platform.CurrentActivity,
                    CreateCrossFirebaseSettings()
                );
                FirebaseAnalyticsImplementation.Initialize(activity);
            }));
#endif
        });

        builder.Services.AddSingleton(_ => CrossFirebaseAnalytics.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseCrashlytics.Current);
#endif

        return builder;
    }

#if IOS || ANDROID
    private static CrossFirebaseSettings CreateCrossFirebaseSettings()
    {
        return new CrossFirebaseSettings(
            isAnalyticsEnabled: true,
            isCrashlyticsEnabled: true
        );
    }
#endif
}
