using Plugin.Firebase.Analytics;
using Plugin.Firebase.Crashlytics;
#if IOS
using Foundation;
using System.Runtime.InteropServices;
#endif

namespace MauiApp2
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

#if IOS
        // SIGABRT via abort() — the standard C signal Crashlytics hooks on iOS.
        // This is the most reliable way to produce a fatal crash that Crashlytics captures.
        [DllImport("libSystem.dylib")]
        private static extern void abort();
#endif

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnLogEventClicked(object sender, EventArgs e)
        {
            CrossFirebaseAnalytics.Current.LogEvent("test_button_clicked",
                new Dictionary<string, object> { { "button", "analytics_test" } });

            DisplayAlert("Analytics", "Event sent to Firebase!!!", "OK");
        }

        private void OnCrashClicked(object sender, EventArgs e)
        {
            // Show Firebase init error if it failed
            if (MauiProgram.FirebaseInitError != null)
            {
                DisplayAlert("Firebase Init FAILED", MauiProgram.FirebaseInitError, "OK");
                return;
            }

            if (CrossFirebaseCrashlytics.Current == null)
            {
                DisplayAlert("Crashlytics ERROR", "CrossFirebaseCrashlytics.Current is null — Firebase did not initialize.", "OK");
                return;
            }

            CrossFirebaseCrashlytics.Current.Log("About to trigger fatal test crash via abort()");

#if IOS
            // abort() raises SIGABRT — Crashlytics installs a handler for this signal
            // during initialization and will intercept it and write a crash report.
            abort();
#else
            throw new Exception("[Test] Fatal crash triggered by user");
#endif
        }
    }
}
