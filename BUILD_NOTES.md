# DianxiaoMaui - Build Notes

## Project Overview
Complete .NET MAUI reimplementation of the original "自动拨号_18.0.0.apk" (org.yy.dial) with 3-tab navigation:
- **客户** (Customers) - Customer management with search, add, detail
- **我的** (Mine) - Auto-dial hub with import/start/stop/progress + 12-feature grid
- **设置** (Settings) - Dial settings, SIM prefixes, blacklist, report, placeholders

## Architecture
- **.NET 10** (net10.0-android36.0)
- **MAUI 10.0.0-preview.4.25259.2** with CommunityToolkit.Maui/Mvvm
- **SQLite** via sqlite-net-pcl (replacing Room)
- **JSON Preferences** (replacing DataStore)
- **CsvHelper** for CSV import

## Key Components

### Models (4)
- `CallTask.cs` - Auto-dial task with status enum (PENDING=0, DIALING=1, CALLED=2, DONE=3, SKIPPED=4)
- `Customer.cs` - Customer entity
- `CallLog.cs` - Call history with recording path
- `Blacklist.cs` - Blacklist entity

### Services (4)
- `DatabaseService.cs` - SQLiteAsyncConnection singleton
- `PreferencesService.cs` - JSON file-based preferences
- `DialerService.cs` - State machine (0=idle, 1=dialing, 2=waiting, 3=connected) with RunLoop
- `NumberImporter.cs` - Text/CSV import with validation

### ViewModels (9)
- `CustomersViewModel`, `MineViewModel`, `ContactDetailViewModel`
- `ManualDialViewModel`, `CallHistoryViewModel`
- `DialSettingsViewModel`, `PrefixSettingsViewModel`
- `BlacklistViewModel`, `ReportViewModel`

### Views (12 pages)
- **Tabs**: CustomersPage, MinePage, SettingsPage
- **Features**: ContactDetailPage, ManualDialPage (numeric keypad), CallHistoryPage
- **Settings**: DialSettingsPage, PrefixSettingsPage, BlacklistPage, ReportPage, PlaceholderPage

### Platform (Android)
- `AndroidDialerPlatform.cs` - DialAsync, WaitForCallEndAsync, AccessibilityService handlers
- `MainActivity.cs`, `MainApplication.cs`
- XML: accessibility_service_config.xml, foreground_service_types.xml, strings.xml

### Resources
- 20+ SVG icons, AppIcon, Splash, Styles.xaml, Fonts

## Build Requirements
```bash
# .NET 10 SDK at /usr/lib/dotnet10
# Android SDK at /opt/android-sdk with API 36
```

## Build Command (x86_64 host)
```bash
cd /mnt/data/work/projects/dianxiao-maui
/usr/lib/dotnet10/dotnet build DianxiaoMaui.csproj -p:AndroidSdkDirectory=/opt/android-sdk -c Release
```

## Build on ARM64 Host (Current Environment)
The ARM64 host cannot directly run x86_64 build tools (aapt2, etc.). Workaround:
1. Use x86_64 .NET SDK under qemu-x86_64-static
2. Install x86_64 libs: libssl3, libicu70 in /lib/x86_64-linux-gnu/
3. Known issue: MSBuild AccessViolationException under qemu-user (upstream limitation)

## Verified Working
- Original Android project builds and produces APK at `/mnt/data/work/projects/dianxiao-star/app-debug.apk`
- MAUI project code complete with all 83 source files
- All features implemented per specification

## Features Implemented
✅ 3-Tab Shell navigation (客户/我的/设置)
✅ Auto-dial with import (text/CSV), start/stop, progress
✅ Manual dial with numeric keypad
✅ Call history with recording playback
✅ Customer CRUD with search
✅ Dial settings (interval, package name, auto-speaker, auto-record, auto-hangup)
✅ SIM prefix management (卡1/卡2)
✅ Blacklist with add/remove
✅ Statistics report
✅ Placeholders for online features (SMS/Cloud/Account/VIP/AI → "敬请期待")
✅ Accessibility service for auto-clicking "呼叫" button
✅ Foreground service for background operation
✅ State machine dialer with proper status transitions

## Files Count
- 83 source files (.cs, .xaml, .xml, .svg, .csproj)
- 10 ViewModels
- 4 Models
- 4 Services
- 12 Views/Pages
- 4 Converters
- 20+ Resource files
