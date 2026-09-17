<p align="center">
  <img src="trpolaris-tpscreen-cover.png" alt="TRPOLARIS TP-SCREEN" width="100%">
</p>

<h1 align="center">TRPOLARIS TP-SCREEN</h1>

<p align="center">
  Windows → Android & Windows Real-Time Screen Streaming
</p>

---



# 🚀 TRPOLARIS

<p align="center">
  <strong>Windows Virtual Display & Android Viewer</strong>
</p>

<p align="center">
  TRPOLARIS is a display streaming system that creates a virtual display on Windows and streams the display to Android devices in real time over a local network.
</p>

<p align="center">

![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Android-blue)
![Driver](https://img.shields.io/badge/Driver-IddCx-purple)
![Server](https://img.shields.io/badge/Server-.NET-512BD4)
![Viewer](https://img.shields.io/badge/Viewer-Flutter-02569B)
![Network](https://img.shields.io/badge/Network-LAN-green)

</p>

---

## ✨ Features

### 🖥️ Windows Server

* 🖥️ Creates a virtual display on Windows
* ⚙️ Windows Indirect Display Driver (IddCx) support
* 🔌 Automatic driver installation
* 🔐 Driver certificate installation
* 📡 LAN server discovery
* 🔑 PIN-based connection
* 📊 Connection and streaming statistics
* 🖥️ Resolution management
* 🔄 Client connection management
* 📦 Embedded driver runtime resources

### 📱 Android Viewer

* 🔎 Automatic LAN server discovery
* 🔗 Quick server connection
* 💾 Saved server profiles
* 🔐 PIN support
* 🔄 Automatic reconnect
* 📈 Real-time FPS
* 🌐 Mbps information
* 📶 Ping measurement
* 🖥️ Resolution information
* 🎬 Stream status
* ⛶ Fullscreen viewing
* 🔄 Portrait / landscape support
* 🖥️ Detects Windows display resolution changes
* 👆 Touch-based information panel
* 🎞️ Animated splash screen

---

# 🧩 Project Structure

```text
TRPOLARIS/
│
├── Driver/
│   ├── IddSampleApp/
│   ├── IddSampleDriver/
│   └── Runtime/
│       ├── IddSampleDriver.inf
│       ├── IddSampleDriver.cat
│       ├── IddSampleDriver.dll
│       ├── IddSampleDriver.cer
│       └── IddSampleApp.exe
│
├── PolarisDisplay.Server/
│
├── PolarisDisplay.Client/
│
├── Installer/
│   ├── Server/
│   │   └── Server.iss
│   └── Viewer/
│       └── Viewer.iss
│
├── TPViewer/
│   ├── android/
│   ├── assets/
│   ├── lib/
│   ├── pubspec.yaml
│   └── pubspec.lock
│
├── PolarisDisplay.sln
├── Build-Driver-And-Stage.ps1
├── .gitignore
├── .gitattributes
└── README.md
```

---

# 🌐 Network

TRPOLARIS uses two main network connections over the local network:

| Purpose             | Protocol |    Port |
| ------------------- | -------- | ------: |
| 🔎 Server Discovery | UDP      | `50504` |
| 🎥 Display Stream   | TCP      | `50505` |

### Connection Flow

```text
Android Viewer
      │
      │ UDP 50504
      ▼
Server Discovery
      │
      │ TCP 50505
      ▼
TRPOLARIS Server
      │
      ▼
Virtual Display
```

---

# 🖥️ Driver

The TRPOLARIS Display Driver is a **Windows Indirect Display Driver (IddCx)** based driver responsible for creating and managing virtual displays on Windows.

### Driver Runtime

The Server uses the following runtime files:

```text
IddSampleDriver.inf
IddSampleDriver.cat
IddSampleDriver.dll
IddSampleDriver.cer
IddSampleApp.exe
```

When the Server starts, it uses these runtime files to initialize the virtual display infrastructure.

### Driver Build

Recommended development environment:

```text
Visual Studio 2022
MSVC v143
Windows SDK 26100
WDK 26100
Platform: x64
```

Driver solution:

```text
Driver/IddSampleDriver.sln
```

The driver should be built as **Release / x64**.

---

# ⚙️ Driver Installation Flow

The TRPOLARIS Server includes an automatic driver bootstrap system:

```text
Server.exe
   │
   ▼
DriverBootstrap.EnsureInstalled()
   │
   ▼
Embedded Driver Files
   │
   ▼
Certificate Verification
   │
   ▼
Windows Certificate Store
   │
   ▼
pnputil /add-driver
   │
   ▼
pnputil /scan-devices
   │
   ▼
IddSampleApp.exe
   │
   ▼
Virtual Display
```

This system is designed so that end users do not have to manually manage driver folders or installation commands.

---

# 🔐 Certificate

The included `IddSampleDriver.cer` certificate is intended for **development/testing purposes**.

During installation, the certificate is added to:

```text
Trusted Root
        +
Trusted Publisher
```

> ⚠️ **Important:** The included development/test certificate should not be used as a production driver-signing solution. A proper production driver-signing process should be used for public distribution.

---

# 🛠️ Requirements

## Windows

Before building the Windows components, install:

```text
Visual Studio 2022
Windows SDK 26100
WDK 26100
MSVC v143
.NET SDK
Inno Setup 7.x
```

---

# 🔨 Windows Build

Clone the repository:

```powershell
git clone https://github.com/trpolaris/TP-SCREEN.git
cd TP-SCREEN
```

## Driver

Build and stage the driver:

```powershell
.\Build-Driver-And-Stage.ps1
```

For an explicit x64 build:

```powershell
.\Build-Driver-And-Stage.ps1 -Platform x64
```

---

## Server

Restore dependencies:

```powershell
dotnet restore .\PolarisDisplay.Server\PolarisDisplay.Server.csproj
```

Build Release:

```powershell
dotnet build .\PolarisDisplay.Server\PolarisDisplay.Server.csproj -c Release
```

Publish:

```powershell
dotnet publish .\PolarisDisplay.Server\PolarisDisplay.Server.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true
```

---

## Client

Restore dependencies:

```powershell
dotnet restore .\PolarisDisplay.Client\PolarisDisplay.Client.csproj
```

Build Release:

```powershell
dotnet build .\PolarisDisplay.Client\PolarisDisplay.Client.csproj -c Release
```

Publish:

```powershell
dotnet publish .\PolarisDisplay.Client\PolarisDisplay.Client.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true
```

---

# 📦 Installer

The installer uses **Inno Setup 7.x**.

Server installer:

```text
Installer/Server/Server.iss
```

Viewer installer:

```text
Installer/Viewer/Viewer.iss
```

The main Server installer workflow is:

```text
Installer
   │
   ├── Server installation
   ├── Certificate installation
   ├── Driver preparation
   └── Server startup
```

Administrator privileges are required during installation.

---

# 📱 Android Viewer

The Android Viewer is located in:

```text
TPViewer/
```

Required tools:

```text
Flutter SDK
Dart SDK
Android Studio
Android SDK
Android SDK Build Tools
Android NDK
```

Check the Flutter environment:

```powershell
flutter doctor
```

---

# 🚀 Android Setup

Go to the Android project:

```powershell
cd TPViewer
```

Clean the project:

```powershell
flutter clean
```

Install dependencies:

```powershell
flutter pub get
```

Check connected devices:

```powershell
flutter devices
```

Run the application:

```powershell
flutter run
```

---

# 📦 Release APK

Build a release APK:

```powershell
flutter clean
```

```powershell
flutter pub get
```

```powershell
flutter build apk --release
```

The APK is generated under:

```text
TPViewer/build/app/outputs/flutter-apk/
```

For release testing, it is recommended to install the APK directly on a physical Android device instead of relying on an Android Studio debug run.

---

# 🔄 Reconnect System

The TRPOLARIS Viewer can automatically reconnect when the network connection is temporarily interrupted.

Basic flow:

```text
Connected
   │
   ▼
Connection Lost
   │
   ▼
Wait
   │
   ▼
Reconnect
   │
   ▼
Connected
```

The connection system also uses socket identity/generation checks to prevent callbacks from an old socket from accidentally closing a newer connection.

---

# 📊 Stream Statistics

The Viewer can display:

```text
FPS
Mbps
Ping
Resolution
Stream Status
```

When the first frame is received, the stream becomes active.

---

# 🖥️ Fullscreen & Orientation

Fullscreen mode supports:

* Hiding system bars.
* Keeping the screen awake.
* Displaying FPS / Mbps / Ping information.
* Automatically hiding the information panel after a timeout.
* Showing the panel again when the screen is touched.
* Tracking Windows display orientation changes.

Example:

```text
1920 × 1080
      ↓
Landscape

1080 × 1920
      ↓
Portrait
```

---

# 🔥 Release Test Checklist

Before releasing a new version, verify:

```text
☐ Splash screen
☐ Server discovery
☐ Server connection
☐ PIN authentication
☐ FPS
☐ Mbps
☐ Ping
☐ Resolution
☐ Stream status
☐ Fullscreen
☐ Information panel
☐ Panel timeout
☐ Touch to show panel
☐ Landscape
☐ Portrait
☐ Windows rotation
☐ Phone rotation
☐ Reconnect
```

---

# 🧪 Development Workflow

For driver changes:

```text
Driver Build
     ↓
Driver/Runtime
     ↓
Server Build
     ↓
Server Publish
     ↓
Installer
     ↓
Test
```

For Android changes:

```text
Flutter Source
     ↓
flutter clean
     ↓
flutter pub get
     ↓
flutter run
     ↓
Device Test
     ↓
flutter build apk --release
     ↓
Release Test
```

---

# 🧹 GitHub Repository Cleanup

The repository uses `.gitignore` to prevent unnecessary build and user-specific files from being committed.

Examples:

```text
.vs/
.idea/
bin/
obj/
build/
.dart_tool/
.gradle/
local.properties
*.pdb
*.log
```

Required driver runtime files are intentionally kept in the repository.

---

# 🔒 Security

Never commit the following to GitHub:

```text
❌ Private Keys
❌ API Keys
❌ Real PINs
❌ Production Certificates
❌ Passwords
❌ User-specific configuration files
```

Development/test certificates should not be used for production distribution.

---

# 📁 Main Components

| Component                   | Description            |
| --------------------------- | ---------------------- |
| 🖥️ `PolarisDisplay.Server` | Windows Server         |
| 🖼️ `PolarisDisplay.Client` | Windows Client         |
| ⚙️ `Driver`                 | Virtual Display Driver |
| 📦 `Installer`              | Windows Installers     |
| 📱 `TPViewer`               | Android Viewer         |

---

# 📌 Project Status

TRPOLARIS is an actively developed Windows + Android virtual display solution.

The current project includes:

```text
✅ Virtual Display
✅ Automatic Discovery
✅ PIN Connection
✅ Reconnect
✅ FPS
✅ Mbps
✅ Ping
✅ Fullscreen
✅ Portrait / Landscape
✅ Resolution Handling
✅ Driver Bootstrap
✅ Installer
✅ Android Viewer
```

---

# 🧑‍💻 Development

When implementing new features, the existing working architecture should be preserved whenever possible.

In particular, avoid unnecessary changes to:

```text
UI Designer
Driver Runtime
Connection Architecture
```

Recommended development approach:

```text
Existing Code
     ↓
Minimal Changes
     ↓
Build
     ↓
Test
     ↓
Release
```

---

# 📄 License

TRPOLARIS application code is licensed under the **MIT License**.

The Windows Indirect Display Driver components under the `Driver/` directory
contain code derived from Microsoft's Windows Driver Samples and are subject
to the **Microsoft Public License (MS-PL)** and the applicable copyright and
attribution notices.

See [`LICENSE`](./LICENSE) and [`NOTICE.md`](./NOTICE.md) for details.
---

<p align="center">

### 🚀 TRPOLARIS

**Virtual Display • Windows • Android • LAN**

</p>
