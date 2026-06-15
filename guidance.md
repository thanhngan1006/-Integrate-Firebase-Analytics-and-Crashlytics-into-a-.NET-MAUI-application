How to integrate Firebase Crashlytics into .NET MAUI iOS — beginner guide
1. Create a Firebase project
Go to console.firebase.google.com → New project
Add an iOS app, enter your bundle ID (e.g. com.yourname.appname)
Download GoogleService-Info.plist

2. Choose a unique Bundle ID
In MauiApp2.csproj, set a unique bundle ID (free Apple accounts require it to be globally unique):

```
<ApplicationId>com.yourname.appname</ApplicationId>
```

3. Add NuGet packages

```
<PackageReference Include="Plugin.Firebase" Version="4.0.0" />
<PackageReference Include="Plugin.Firebase.Analytics" Version="4.0.0" />
<PackageReference Include="Plugin.Firebase.Crashlytics" Version="4.0.0" />
```

4. Bundle the plist into the app
Place GoogleService-Info.plist in iOS and add to MauiApp2.csproj:

```
<ItemGroup Condition="'$(TargetFramework)' == 'net10.0-ios'">
  <BundleResource Include="Platforms\iOS\GoogleService-Info.plist">
    <Link>GoogleService-Info.plist</Link>
  </BundleResource>
</ItemGroup>
```
The <Link> tag is critical — it places the file at the app bundle root where Firebase expects it.

5. Fix the iOS linker so Crashlytics can find the crash entry point
This is the key fix that makes Crashlytics actually capture crashes. Add to MauiApp2.csproj:

```
<PropertyGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">
  <_ExportSymbolsExplicitly>false</_ExportSymbolsExplicitly>
</PropertyGroup>

<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">
  <_ReferencesLinkerFlags Include="-u __mh_execute_header" />
</ItemGroup>
```

Without this, Crashlytics initializes but silently fails to hook its crash signal handlers.

6. Initialize Firebase in MauiProgram.cs

```
using ObjCRuntime;
using Plugin.Firebase.Bundled.Platforms.iOS;
using Plugin.Firebase.Crashlytics;

// Inside CreateMauiApp(), in the .AddiOS() lifecycle block:
events.AddiOS(iOS => iOS.WillFinishLaunching((app, options) =>
{
    CrossFirebase.Initialize(new CrossFirebaseSettings(
        isAnalyticsEnabled: true,
        isCrashlyticsEnabled: true));

    CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(true);
    CrossFirebaseCrashlytics.Current.SendUnsentReports();

    // Routes managed .NET exceptions through native stack so Crashlytics intercepts them
    Runtime.MarshalManagedException += (_, args) =>
        args.ExceptionMode = MarshalManagedExceptionMode.UnwindNativeCode;

    // Record unhandled exceptions before the crash
    AppDomain.CurrentDomain.UnhandledException += (_, args) =>
    {
        if (args.ExceptionObject is Exception ex)
            CrossFirebaseCrashlytics.Current.RecordException(ex);
    };

    return true; // must return true or app won't launch
}));
```

7. Get an Apple signing certificate
Open Xcode → create any temporary iOS app with your Apple ID → run it on your device once
This auto-creates an Apple Development certificate and provisions your device
Back in your MAUI project, build with: -p:CodesignProvision=Automatic -p:CodesignKey="Apple Development"
8. About the dSYM warning you see now
The dashboard says "1 unprocessed crash" — the crash was captured, but Firebase needs a dSYM file to show a human-readable stack trace. To upload it:

Find the file: MauiApp2.app.dSYM
In Firebase Console → Crashlytics → dSYMs tab → click Manage → upload that .dSYM folder (zip it first)
After uploading, the crash will show a full symbolicated stack trace.