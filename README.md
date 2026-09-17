# TRPOLARIS

TRPOLARIS is a Windows virtual-display system with a Flutter Android viewer.

The repository contains:

- **PolarisDisplay.Server** — Windows server application.
- **PolarisDisplay.Client** — Windows client application.
- **Driver** — Windows Indirect Display Driver (IddCx) and runtime files.
- **Installer** — Inno Setup scripts for Server and Viewer.
- **TPViewer** — Flutter Android viewer.

## Architecture

```text
Windows PC
┌──────────────────────────────────────────────┐
│ PolarisDisplay.Server                        │
│   ├─ IddSampleDriver                         │
│   ├─ IddSampleApp                            │
│   └─ Screen streaming / discovery / PIN      │
└───────────────────┬──────────────────────────┘
                    │ LAN
          UDP 50504  │  discovery
          TCP 50505  │  display stream
                    │
                    ▼
              Android TPViewer
```

### Network ports

| Purpose | Protocol | Port |
|---|---|---:|
| Server discovery | UDP | **50504** |
| Display stream | TCP | **50505** |

The Android viewer uses LAN discovery and can also connect to a saved server profile by IP/port.

## Features

### Android viewer

- Automatic LAN server discovery.
- Saved server profiles.
- Server name, IP, port and PIN.
- Automatic reconnect after short network interruptions.
- Protection against stale socket callbacks closing a newer connection.
- Real-time FPS and Mbps statistics.
- Ping measurement.
- Resolution and stream-status information.
- Full-screen viewing.
- Portrait/landscape handling based on the remote display.
- Animated launch/splash assets.
- Keep-screen-on support.
- Saved last-used server/session information.

The supplied project notes describe the viewer's discovery/display protocol as UDP 50504 and TCP 50505 and identify the release-discovery fix as part of the final Android work.

### Windows server/client

- Windows virtual display through IddCx.
- Automatic driver bootstrap from the server.
- Embedded driver runtime resources.
- Driver certificate installation during setup.
- Server/client WinForms UI.
- Server broadcasting/discovery.
- PIN authentication.
- Display configuration and resolution handling.
- Statistics and connection management.
- Inno Setup installers.

The server embeds the following runtime files:

```text
Driver/Runtime/
├── IddSampleDriver.inf
├── IddSampleDriver.cat
├── IddSampleDriver.dll
├── IddSampleDriver.cer
└── IddSampleApp.exe
```

## Requirements

### Windows development

The supplied driver/project configuration targets:

- Windows x64
- Visual Studio **2022**
- MSVC **v143**
- Windows SDK **10.0.26100.0**
- WDK **26100** (the supplied project notes specify WDK 26100.6584)
- .NET SDK **8.x**
- Inno Setup **7.x** for building installers

The driver solution contains:

```text
Driver/IddSampleDriver.sln
```

and the main Windows solution contains:

```text
PolarisDisplay.sln
```

### Android development

Install:

- Flutter SDK with Dart SDK **3.9+**
- Android Studio
- Android SDK
- Android SDK Platform/Build Tools required by your Flutter SDK
- Android NDK **28.2.13676358**
- A physical Android device or Android emulator

The Flutter project declares:

```yaml
environment:
  sdk: ">=3.9.0 <4.0.0"
```

and the Android module explicitly requests:

```text
NDK 28.2.13676358
```

## Windows driver setup

### 1. Install Visual Studio 2022

Install the C++ desktop workload and the components needed for Windows driver development.

Make sure the following are available:

- MSVC v143
- Windows 10/11 SDK
- Windows Driver Kit (WDK)
- C++ build tools

The supplied project is configured around Windows SDK/WDK 26100 and MSVC v143.

### 2. Build the driver

Open:

```text
Driver/IddSampleDriver.sln
```

Build:

```text
Release | x64
```

This solution contains:

- `IddSampleDriver`
- `IddSampleApp`

### 3. Stage the runtime

The server expects these files in:

```text
Driver/Runtime/
```

```text
IddSampleDriver.inf
IddSampleDriver.cat
IddSampleDriver.dll
IddSampleDriver.cer
IddSampleApp.exe
```

The repository includes `Build-Driver-And-Stage.ps1` to automate driver build/staging when the local Visual Studio/WDK environment is configured correctly.

Run PowerShell from the repository root:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\Build-Driver-And-Stage.ps1
```

For an explicit x64 build:

```powershell
.\Build-Driver-And-Stage.ps1 -Platform x64
```

If your copy of the script exposes different parameters, use:

```powershell
Get-Help .\Build-Driver-And-Stage.ps1 -Detailed
```

### 4. Build the Windows applications

From the repository root:

```powershell
dotnet restore .\PolarisDisplay.Server\PolarisDisplay.Server.csproj
dotnet restore .\PolarisDisplay.Client\PolarisDisplay.Client.csproj
```

Build Release:

```powershell
dotnet build .\PolarisDisplay.Server\PolarisDisplay.Server.csproj -c Release
dotnet build .\PolarisDisplay.Client\PolarisDisplay.Client.csproj -c Release
```

Or open:

```text
PolarisDisplay.sln
```

in Visual Studio 2022 and build both projects as Release.

### 5. Publish the applications

Server:

```powershell
dotnet publish .\PolarisDisplay.Server\PolarisDisplay.Server.csproj `
  -c Release -r win-x64 --self-contained true
```

Client:

```powershell
dotnet publish .\PolarisDisplay.Client\PolarisDisplay.Client.csproj `
  -c Release -r win-x64 --self-contained true
```

The project files are configured for single-file, self-contained publishing.

## Building the Server installer

Install Inno Setup 7.

Open:

```text
Installer/Server/Server.iss
```

Before compiling, make sure the Server publish output expected by the script is present:

```text
Installer/Server/Publish/PolarisDisplay.Server.exe
```

Then compile `Server.iss` with Inno Setup.

The installer:

1. Installs the Server.
2. Installs the supplied development/test driver certificate into the Windows certificate stores.
3. Starts the Server with administrator privileges.
4. Lets the Server bootstrap its embedded driver runtime.

### Important: test certificate

The included `IddSampleDriver.cer` is a **development/test certificate**, not a production code-signing certificate.

For public production distribution, replace the development/test signing workflow with an appropriate production driver-signing process. Do not treat the included certificate as a production trust model.

## Building the Viewer installer

Build/publish the Windows Viewer first:

```powershell
dotnet publish .\PolarisDisplay.Client\PolarisDisplay.Client.csproj `
  -c Release -r win-x64 --self-contained true
```

Then place the resulting executable where expected by:

```text
Installer/Viewer/Viewer.iss
```

Compile:

```text
Installer/Viewer/Viewer.iss
```

with Inno Setup.

## Android viewer setup

Go to the Android project:

```powershell
cd .\TPViewer
```

Check Flutter:

```powershell
flutter --version
flutter doctor
```

Install dependencies:

```powershell
flutter clean
flutter pub get
```

Run on a connected device:

```powershell
flutter devices
flutter run
```

### Android release APK

Build:

```powershell
flutter clean
flutter pub get
flutter build apk --release
```

The APK will be generated under Flutter's normal build output directory:

```text
TPViewer/build/app/outputs/flutter-apk/
```

### Android release test

For a clean release test, install the generated APK directly on the Android device instead of relying on an Android Studio debug run.

Verify:

- Splash/launch screen.
- Automatic server discovery.
- Server connection.
- PIN authentication.
- FPS.
- Mbps.
- Ping.
- Resolution.
- Stream status.
- Full-screen mode.
- Information panel timeout/tap behavior.
- Windows landscape → portrait.
- Windows portrait → landscape.
- Phone rotation.
- Reconnect after network interruption.

## Android project configuration

The main manifest contains the required network permission:

```xml
<uses-permission android:name="android.permission.INTERNET"/>
```

The Android application uses Flutter's Android embedding v2.

The project explicitly configures:

```text
Java 17
Kotlin JVM target 17
NDK 28.2.13676358
```

The Gradle wrapper bundled with the supplied project is configured for Gradle 9.3.1.

## Firewall / LAN

The Windows machine and Android device must be able to communicate over the local network.

Allow the TRPOLARIS Server through Windows Firewall when Windows asks for network access.

Required traffic:

```text
UDP 50504  -> discovery
TCP 50505  -> display stream
```

If automatic discovery does not work:

1. Confirm both devices are on the same LAN.
2. Check Windows Firewall.
3. Confirm UDP 50504 is not blocked.
4. Confirm TCP 50505 is not blocked.
5. Try connecting with the server's IP address and TCP port manually.

## Repository layout

```text
TRPOLARIS/
├── Driver/
│   ├── IddSampleApp/
│   ├── IddSampleDriver/
│   ├── Runtime/
│   └── IddSampleDriver.sln
│
├── Installer/
│   ├── Server/
│   │   └── Server.iss
│   └── Viewer/
│       └── Viewer.iss
│
├── PolarisDisplay.Server/
├── PolarisDisplay.Client/
├── PolarisDisplay.sln
├── Build-Driver-And-Stage.ps1
├── TPViewer/
│   ├── android/
│   ├── assets/
│   ├── lib/
│   ├── pubspec.yaml
│   └── pubspec.lock
├── .gitignore
└── README.md
```

## What is intentionally excluded

This repository has been cleaned for GitHub/source distribution. The following categories are intentionally excluded:

- Visual Studio `.vs` data.
- JetBrains/Android Studio `.idea` data.
- `.NET` `bin/` and `obj/` output.
- Gradle caches.
- Flutter `.dart_tool/` and build output.
- Android `local.properties`.
- Per-user Visual Studio/project files.
- NuGet/MSBuild generated metadata.
- Debug logs and compiler analysis artifacts.
- Inno Setup generated `Output/` and `Publish/` directories.
- Previous internal development notes and temporary README text.

The driver runtime under `Driver/Runtime/` is intentionally retained because the Windows Server project embeds those files as resources.

## Development workflow

For driver changes:

```text
IddSampleDriver / IddSampleApp
        ↓
Release x64 build
        ↓
Driver/Runtime
        ↓
PolarisDisplay.Server build
        ↓
Server publish
        ↓
Inno Setup
        ↓
Server installer
```

For Android changes:

```text
TPViewer
   ↓
flutter clean
   ↓
flutter pub get
   ↓
flutter run
   ↓
device testing
   ↓
flutter build apk --release
```

Keep the Windows UI Designer layout stable unless a UI change is intentional. Runtime behavior should preferably remain in the existing runtime/partial code structure.

## Troubleshooting

### Driver headers are missing

Errors such as:

```text
wudfwdm.h not found
wdf.h not found
iddcx.h not found
```

usually indicate that the correct WDK/SDK installation is missing or Visual Studio is not using the expected toolchain.

Verify:

```text
Visual Studio 2022
MSVC v143
Windows SDK 26100
WDK 26100
```

### Driver installation reports certificate trust errors

A development/test certificate may not be trusted on a clean Windows installation.

The Server installer is designed to add the included certificate to:

```text
Root
TrustedPublisher
```

For production distribution, use a production driver-signing and trust model rather than the included development certificate.

### Android discovery works in debug but not release

Verify that the main manifest contains:

```xml
<uses-permission android:name="android.permission.INTERNET"/>
```

The supplied project already places this permission in the main manifest.

Also verify Windows Firewall and LAN access to UDP 50504/TCP 50505.

### Android build complains about NDK

Use:

```text
NDK 28.2.13676358
```

The application module explicitly requests this version.

### Flutter project cannot find Android configuration

From `TPViewer`:

```powershell
flutter clean
flutter pub get
flutter create --platforms=android .
```

Use the last command only if the Android platform files have been removed or corrupted; review any regenerated files against the repository before committing.

## Security notes

- Never commit real production passwords, private keys, API keys, or signing keys.
- The included driver certificate is for development/test use.
- Replace development signing with a production signing workflow before distributing a production Windows driver.
- Review firewall rules before deploying outside a trusted LAN.
- PINs are application credentials; do not publish real server PINs in source control.

## License

No license was supplied with the provided project files. Add an appropriate `LICENSE` file before publishing the repository if you want to grant reuse rights.

## Status

This repository is a cleaned source package based on the supplied TRPOLARIS Windows and Android project files. The supplied project notes identify the Android viewer's final release-discovery fix as the last critical test item; therefore, verify the release APK on a clean physical Android device before treating a release as production-ready.
