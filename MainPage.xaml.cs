using Plugin.Firebase.Analytics;
using Plugin.Firebase.Crashlytics;

namespace MauiApp2
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnLogEventClicked(object sender, EventArgs e)
        {
            CrossFirebaseAnalytics.Current.LogEvent("test_button_clicked",
                new Dictionary<string, object> { { "button", "analytics_test" } });

            DisplayAlert("Analytics", "Event sent to Firebase!", "OK");
        }

        private void OnCrashClicked(object sender, EventArgs e)
        {
            CrossFirebaseCrashlytics.Current.RecordException(new Exception("Test non-fatal exception"));
            throw new Exception("This is a deliberate test crash for Crashlytics");
        }
    }
}
