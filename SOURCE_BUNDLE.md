# 全部源代码与构建文件

## Directory.Build.props

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <Deterministic>true</Deterministic>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
    <CETCompat>false</CETCompat>
  </PropertyGroup>
</Project>

```

## docs/verification/desktop-final.json

```json
{
  "width": 1440,
  "height": 900,
  "deviceCount": 2,
  "nodes": 5,
  "selectedNode": "com.matrix.demo:id/tap",
  "apps": 1,
  "colors": 1,
  "shell": 1,
  "script": "Completed",
  "tabs": 11,
  "languageSwitch": "English navigation/settings bindings and Chinese restoration passed",
  "unavailableControlsDisabled": true,
  "error": ""
}
```

## docs/verification/desktop-nodes.json

```json
{
  "className": "android.widget.FrameLayout",
  "packageName": "com.matrix.demo",
  "index": 0,
  "instance": 0,
  "clickable": false,
  "enabled": true,
  "focusable": false,
  "focused": false,
  "scrollable": false,
  "longClickable": false,
  "checkable": false,
  "checked": false,
  "selected": false,
  "visibleToUser": true,
  "bounds": {
    "left": 0,
    "top": 0,
    "right": 360,
    "bottom": 640,
    "width": 360,
    "height": 640,
    "centerX": 180,
    "centerY": 320
  },
  "childNodeList": [
    {
      "text": "SIMULATED DEVICE DEMO-001",
      "resourceId": "com.matrix.demo:id/title",
      "className": "android.widget.TextView",
      "packageName": "com.matrix.demo",
      "index": 0,
      "instance": 0,
      "clickable": false,
      "enabled": true,
      "focusable": false,
      "focused": false,
      "scrollable": false,
      "longClickable": false,
      "checkable": false,
      "checked": false,
      "selected": false,
      "visibleToUser": true,
      "bounds": {
        "left": 22,
        "top": 10,
        "right": 338,
        "bottom": 98,
        "width": 316,
        "height": 88,
        "centerX": 180,
        "centerY": 54
      },
      "childNodeList": []
    },
    {
      "text": "Tap 1",
      "resourceId": "com.matrix.demo:id/tap",
      "className": "android.widget.Button",
      "packageName": "com.matrix.demo",
      "index": 1,
      "instance": 0,
      "clickable": true,
      "enabled": true,
      "focusable": false,
      "focused": false,
      "scrollable": false,
      "longClickable": false,
      "checkable": false,
      "checked": false,
      "selected": false,
      "visibleToUser": true,
      "bounds": {
        "left": 22,
        "top": 190,
        "right": 338,
        "bottom": 282,
        "width": 316,
        "height": 92,
        "centerX": 180,
        "centerY": 236
      },
      "childNodeList": []
    },
    {
      "text": "DEMO INPUT",
      "resourceId": "com.matrix.demo:id/input",
      "className": "android.widget.EditText",
      "packageName": "com.matrix.demo",
      "index": 2,
      "instance": 0,
      "clickable": true,
      "enabled": true,
      "focusable": true,
      "focused": true,
      "scrollable": false,
      "longClickable": false,
      "checkable": false,
      "checked": false,
      "selected": false,
      "visibleToUser": true,
      "bounds": {
        "left": 22,
        "top": 307,
        "right": 338,
        "bottom": 372,
        "width": 316,
        "height": 65,
        "centerX": 180,
        "centerY": 339
      },
      "childNodeList": []
    },
    {
      "text": "Settings",
      "resourceId": "com.matrix.demo:id/settings",
      "className": "android.widget.Button",
      "packageName": "com.matrix.demo",
      "index": 3,
      "instance": 1,
      "clickable": true,
      "enabled": true,
      "focusable": false,
      "focused": false,
      "scrollable": false,
      "longClickable": false,
      "checkable": false,
      "checked": false,
      "selected": false,
      "visibleToUser": true,
      "bounds": {
        "left": 22,
        "top": 402,
        "right": 338,
        "bottom": 450,
        "width": 316,
        "height": 48,
        "centerX": 180,
        "centerY": 426
      },
      "childNodeList": []
    }
  ]
}
```

## docs/verification/desktop-pixels.json

```json
[
  {
    "File": "desktop-english-settings.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 50,
    "Transparent": 0
  },
  {
    "File": "desktop-english.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 69,
    "Transparent": 0
  },
  {
    "File": "desktop-small-english-settings.png",
    "Width": 984,
    "Height": 661,
    "Colors": 51,
    "Transparent": 0
  },
  {
    "File": "desktop-small-english.png",
    "Width": 984,
    "Height": 661,
    "Colors": 70,
    "Transparent": 0
  },
  {
    "File": "desktop-small-tab0.png",
    "Width": 984,
    "Height": 661,
    "Colors": 71,
    "Transparent": 0
  },
  {
    "File": "desktop-small-tab1.png",
    "Width": 984,
    "Height": 661,
    "Colors": 60,
    "Transparent": 0
  },
  {
    "File": "desktop-small-tab10.png",
    "Width": 984,
    "Height": 661,
    "Colors": 53,
    "Transparent": 0
  },
  {
    "File": "desktop-small-tab2.png",
    "Width": 984,
    "Height": 661,
    "Colors": 52,
    "Transparent": 0
  },
  {
    "File": "desktop-small-tab3.png",
    "Width": 984,
    "Height": 661,
    "Colors": 43,
    "Transparent": 0
  },
  {
    "File": "desktop-small-tab4.png",
    "Width": 984,
    "Height": 661,
    "Colors": 50,
    "Transparent": 0
  },
  {
    "File": "desktop-small-tab5.png",
    "Width": 984,
    "Height": 661,
    "Colors": 49,
    "Transparent": 0
  },
  {
    "File": "desktop-small-tab6.png",
    "Width": 984,
    "Height": 661,
    "Colors": 40,
    "Transparent": 0
  },
  {
    "File": "desktop-small-tab7.png",
    "Width": 984,
    "Height": 661,
    "Colors": 51,
    "Transparent": 0
  },
  {
    "File": "desktop-small-tab8.png",
    "Width": 984,
    "Height": 661,
    "Colors": 47,
    "Transparent": 0
  },
  {
    "File": "desktop-small-tab9.png",
    "Width": 984,
    "Height": 661,
    "Colors": 70,
    "Transparent": 0
  },
  {
    "File": "desktop-small.png",
    "Width": 984,
    "Height": 661,
    "Colors": 71,
    "Transparent": 0
  },
  {
    "File": "desktop-tab0.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 75,
    "Transparent": 0
  },
  {
    "File": "desktop-tab1.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 87,
    "Transparent": 0
  },
  {
    "File": "desktop-tab10.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 63,
    "Transparent": 0
  },
  {
    "File": "desktop-tab2.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 60,
    "Transparent": 0
  },
  {
    "File": "desktop-tab3.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 45,
    "Transparent": 0
  },
  {
    "File": "desktop-tab4.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 51,
    "Transparent": 0
  },
  {
    "File": "desktop-tab5.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 49,
    "Transparent": 0
  },
  {
    "File": "desktop-tab6.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 42,
    "Transparent": 0
  },
  {
    "File": "desktop-tab7.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 52,
    "Transparent": 0
  },
  {
    "File": "desktop-tab8.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 49,
    "Transparent": 0
  },
  {
    "File": "desktop-tab9.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 70,
    "Transparent": 0
  },
  {
    "File": "desktop.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 75,
    "Transparent": 0
  }
]

```

## docs/verification/desktop-small.json

```json
{
  "width": 1000,
  "height": 700,
  "deviceCount": 2,
  "nodes": 5,
  "selectedNode": "com.matrix.demo:id/tap",
  "apps": 1,
  "colors": 1,
  "shell": 1,
  "script": "Completed",
  "tabs": 11,
  "languageSwitch": "English navigation/settings bindings and Chinese restoration passed",
  "unavailableControlsDisabled": true,
  "error": ""
}
```

## docs/verification/desktop.json

```json
{
  "width": 1440,
  "height": 900,
  "deviceCount": 2,
  "nodes": 5,
  "selectedNode": "com.matrix.demo:id/tap",
  "apps": 1,
  "colors": 1,
  "shell": 1,
  "script": "Completed",
  "tabs": 11,
  "languageSwitch": "English navigation/settings bindings and Chinese restoration passed",
  "unavailableControlsDisabled": true,
  "error": ""
}
```

## docs/verification/ldplayer-desktop-pixels.json

```json
[
  {
    "File": "ldplayer-desktop-small-tab0.png",
    "Width": 984,
    "Height": 661,
    "Colors": 55,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-small-tab1.png",
    "Width": 984,
    "Height": 661,
    "Colors": 41,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-small-tab10.png",
    "Width": 984,
    "Height": 661,
    "Colors": 32,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-small-tab11.png",
    "Width": 984,
    "Height": 661,
    "Colors": 34,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-small-tab2.png",
    "Width": 984,
    "Height": 661,
    "Colors": 49,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-small-tab3.png",
    "Width": 984,
    "Height": 661,
    "Colors": 42,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-small-tab4.png",
    "Width": 984,
    "Height": 661,
    "Colors": 27,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-small-tab5.png",
    "Width": 984,
    "Height": 661,
    "Colors": 32,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-small-tab6.png",
    "Width": 984,
    "Height": 661,
    "Colors": 36,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-small-tab7.png",
    "Width": 984,
    "Height": 661,
    "Colors": 29,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-small-tab8.png",
    "Width": 984,
    "Height": 661,
    "Colors": 43,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-small-tab9.png",
    "Width": 984,
    "Height": 661,
    "Colors": 38,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-small.png",
    "Width": 984,
    "Height": 661,
    "Colors": 55,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-tab0.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 51,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-tab1.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 36,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-tab10.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 38,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-tab11.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 42,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-tab2.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 68,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-tab3.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 38,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-tab4.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 26,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-tab5.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 26,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-tab6.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 26,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-tab7.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 27,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-tab8.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 34,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop-tab9.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 33,
    "TransparentSamples": 0
  },
  {
    "File": "ldplayer-desktop.png",
    "Width": 1424,
    "Height": 861,
    "Colors": 51,
    "TransparentSamples": 0
  }
]

```

## docs/verification/ldplayer-desktop-small.json

```json
{
  "width": 1000,
  "height": 700,
  "deviceCount": 2,
  "ldPlayerInstances": 2,
  "ldStateButtonsVerified": true,
  "ldBusyVerified": true,
  "ldConfirmationCancellationVerified": true,
  "nodes": 5,
  "selectedNode": "com.matrix.demo:id/tap",
  "apps": 1,
  "colors": 1,
  "shell": 1,
  "script": "\u5DF2\u5B8C\u6210",
  "tabs": 12,
  "language": "zh-CN",
  "chineseInterfaceVerified": true,
  "unavailableControlsDisabled": true,
  "error": ""
}
```

## docs/verification/ldplayer-desktop.json

```json
{
  "width": 1440,
  "height": 900,
  "deviceCount": 2,
  "ldPlayerInstances": 2,
  "ldStateButtonsVerified": true,
  "ldBusyVerified": true,
  "ldConfirmationCancellationVerified": true,
  "nodes": 5,
  "selectedNode": "com.matrix.demo:id/tap",
  "apps": 1,
  "colors": 1,
  "shell": 1,
  "script": "\u5DF2\u5B8C\u6210",
  "tabs": 12,
  "language": "zh-CN",
  "chineseInterfaceVerified": true,
  "unavailableControlsDisabled": true,
  "error": ""
}
```

## docs/verification/ui-redesign/desktop-nodes.json

```json
{
  "className": "android.widget.FrameLayout",
  "packageName": "com.matrix.demo",
  "index": 0,
  "instance": 0,
  "clickable": false,
  "enabled": true,
  "focusable": false,
  "focused": false,
  "scrollable": false,
  "longClickable": false,
  "checkable": false,
  "checked": false,
  "selected": false,
  "visibleToUser": true,
  "bounds": {
    "left": 0,
    "top": 0,
    "right": 360,
    "bottom": 640,
    "width": 360,
    "height": 640,
    "centerX": 180,
    "centerY": 320
  },
  "childNodeList": [
    {
      "text": "SIMULATED DEVICE DEMO-001",
      "resourceId": "com.matrix.demo:id/title",
      "className": "android.widget.TextView",
      "packageName": "com.matrix.demo",
      "index": 0,
      "instance": 0,
      "clickable": false,
      "enabled": true,
      "focusable": false,
      "focused": false,
      "scrollable": false,
      "longClickable": false,
      "checkable": false,
      "checked": false,
      "selected": false,
      "visibleToUser": true,
      "bounds": {
        "left": 22,
        "top": 10,
        "right": 338,
        "bottom": 98,
        "width": 316,
        "height": 88,
        "centerX": 180,
        "centerY": 54
      },
      "childNodeList": []
    },
    {
      "text": "Tap 1",
      "resourceId": "com.matrix.demo:id/tap",
      "className": "android.widget.Button",
      "packageName": "com.matrix.demo",
      "index": 1,
      "instance": 0,
      "clickable": true,
      "enabled": true,
      "focusable": false,
      "focused": false,
      "scrollable": false,
      "longClickable": false,
      "checkable": false,
      "checked": false,
      "selected": false,
      "visibleToUser": true,
      "bounds": {
        "left": 22,
        "top": 190,
        "right": 338,
        "bottom": 282,
        "width": 316,
        "height": 92,
        "centerX": 180,
        "centerY": 236
      },
      "childNodeList": []
    },
    {
      "text": "DEMO INPUT",
      "resourceId": "com.matrix.demo:id/input",
      "className": "android.widget.EditText",
      "packageName": "com.matrix.demo",
      "index": 2,
      "instance": 0,
      "clickable": true,
      "enabled": true,
      "focusable": true,
      "focused": true,
      "scrollable": false,
      "longClickable": false,
      "checkable": false,
      "checked": false,
      "selected": false,
      "visibleToUser": true,
      "bounds": {
        "left": 22,
        "top": 307,
        "right": 338,
        "bottom": 372,
        "width": 316,
        "height": 65,
        "centerX": 180,
        "centerY": 339
      },
      "childNodeList": []
    },
    {
      "text": "Settings",
      "resourceId": "com.matrix.demo:id/settings",
      "className": "android.widget.Button",
      "packageName": "com.matrix.demo",
      "index": 3,
      "instance": 1,
      "clickable": true,
      "enabled": true,
      "focusable": false,
      "focused": false,
      "scrollable": false,
      "longClickable": false,
      "checkable": false,
      "checked": false,
      "selected": false,
      "visibleToUser": true,
      "bounds": {
        "left": 22,
        "top": 402,
        "right": 338,
        "bottom": 450,
        "width": 316,
        "height": 48,
        "centerX": 180,
        "centerY": 426
      },
      "childNodeList": []
    }
  ]
}
```

## docs/verification/ui-redesign/desktop-small.json

```json
{
  "width": 1000,
  "height": 700,
  "deviceCount": 2,
  "ldPlayerInstances": 2,
  "ldStateButtonsVerified": true,
  "ldBusyVerified": true,
  "ldConfirmationCancellationVerified": true,
  "navigationVerified": true,
  "navigationStatePreserved": true,
  "sidebarCollapseVerified": true,
  "wallModesVerified": true,
  "filteredContextPreserved": true,
  "clearHiddenSelectionVerified": true,
  "emptyStateVerified": true,
  "groupDropdownVerified": true,
  "directoryFilterContextPreserved": true,
  "nodes": 5,
  "selectedNode": "com.matrix.demo:id/tap",
  "apps": 1,
  "colors": 1,
  "shell": 1,
  "script": "\u5DF2\u5B8C\u6210",
  "tabs": 12,
  "language": "zh-CN",
  "chineseInterfaceVerified": true,
  "unavailableControlsDisabled": true,
  "error": ""
}
```

## docs/verification/ui-redesign/desktop.json

```json
{
  "width": 1440,
  "height": 900,
  "deviceCount": 2,
  "ldPlayerInstances": 2,
  "ldStateButtonsVerified": true,
  "ldBusyVerified": true,
  "ldConfirmationCancellationVerified": true,
  "navigationVerified": true,
  "navigationStatePreserved": true,
  "sidebarCollapseVerified": true,
  "wallModesVerified": true,
  "filteredContextPreserved": true,
  "clearHiddenSelectionVerified": true,
  "emptyStateVerified": true,
  "groupDropdownVerified": true,
  "directoryFilterContextPreserved": true,
  "nodes": 5,
  "selectedNode": "com.matrix.demo:id/tap",
  "apps": 1,
  "colors": 1,
  "shell": 1,
  "script": "\u5DF2\u5B8C\u6210",
  "tabs": 12,
  "language": "zh-CN",
  "chineseInterfaceVerified": true,
  "unavailableControlsDisabled": true,
  "error": ""
}
```

## examples/basic/manifest.json

```json
{
  "Id": "matrix.basic-example",
  "Name": "基础动作验证",
  "Version": "1.0.0",
  "Description": "仅在专用测试设备上运行。",
  "Entry": "script.json"
}

```

## examples/basic/script.json

```json
{
  "Steps": [
    { "Action": "wait", "DurationMs": 500 },
    { "Action": "click", "Value": "50,50" },
    { "Action": "swipe", "Value": "50,200,50,100,300" }
  ],
  "Parameters": []
}

```

## global.json

```json
{"sdk":{"version":"10.0.402","rollForward":"latestPatch"}}

```

## MatrixDevice.slnx

```xml
<Solution>
  <Project Path="src/MatrixDevice.Core/MatrixDevice.Core.csproj" />
  <Project Path="src/MatrixDevice.Features/MatrixDevice.Features.csproj" />
  <Project Path="src/MatrixDevice.Host/MatrixDevice.Host.csproj" />
  <Project Path="src/MatrixDevice.Desktop/MatrixDevice.Desktop.csproj" />
  <Project Path="src/MatrixDevice.Cli/MatrixDevice.Cli.csproj" />
  <Project Path="tests/MatrixDevice.Tests/MatrixDevice.Tests.csproj" />
</Solution>

```

## NuGet.Config

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration><packageSources><clear /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources></configuration>

```

## src/MatrixDevice.Cli/MatrixDevice.Cli.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType></PropertyGroup>
  <ItemGroup><ProjectReference Include="../MatrixDevice.Host/MatrixDevice.Host.csproj" /></ItemGroup>
</Project>

```

## src/MatrixDevice.Cli/packages.lock.json

```json
{
  "version": 1,
  "dependencies": {
    "net10.0": {
      "AdvancedSharpAdbClient": {
        "type": "Transitive",
        "resolved": "3.6.16",
        "contentHash": "w5p3s+9Rkx9tmn5S5KU0XSTap5YWGHOvQOw/40M0d7eGOHf7idhX+kizWPmnP1ahbkic/yiX03SMYNR1mJSAVw=="
      },
      "Microsoft.Data.Sqlite": {
        "type": "Transitive",
        "resolved": "10.0.12",
        "contentHash": "xUq9xUc653ue7ac2rMfCaf8UQN+U3zS8poWYlugycpvJllplMM0Mk7v/+MM0CTlu9zGvbGML4BPH8YWhrUOfGw==",
        "dependencies": {
          "Microsoft.Data.Sqlite.Core": "10.0.12",
          "SQLitePCLRaw.bundle_e_sqlite3": "2.1.12",
          "SQLitePCLRaw.core": "2.1.12"
        }
      },
      "Microsoft.Data.Sqlite.Core": {
        "type": "Transitive",
        "resolved": "10.0.12",
        "contentHash": "2aL9eL5HQr0V4b8V909SD+jQAx6qnTTl7sT/v3D8dIvmr2UGdpZIQxqm1pj2Js3e4EYTvpw0oMH7DjapJJuPPw==",
        "dependencies": {
          "SQLitePCLRaw.core": "2.1.12"
        }
      },
      "SkiaSharp": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "TghYLOUaP3q6rTjQw7U9uQaaXh8lEs9rfA1U7/VtmyAptVDtj/SmjtcWzFWmcC/VMWxe3QgbV9uyvqDL7ySFtQ==",
        "dependencies": {
          "SkiaSharp.NativeAssets.Win32": "4.153.1",
          "SkiaSharp.NativeAssets.macOS": "4.153.1"
        }
      },
      "SkiaSharp.NativeAssets.macOS": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "YTojWeWIhnpF7uC8vpMxB5wRONKkDB2t3r/nNPA3DL2pgj9fqMXRevA/rIanYazWnco4jo1ufgp3HkXuh1Tz8A=="
      },
      "SkiaSharp.NativeAssets.Win32": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "Yqg6rChTVh0BUE8QQxtOA3uUBnihfgID4yex/AvDa0+os2bVcBjmI3Qj9tnOB+R1hd/Jry1Gwl8qw5JI9HZG8Q=="
      },
      "SQLitePCLRaw.bundle_e_sqlite3": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "mAgscpQMLw5/nfA1Q5oJVAT29yROUo1ifZGbbTpx/lwZpSxMUGoYbKfmvdm8oXER+RzxqBmmQzeBEVKfeHv2nw==",
        "dependencies": {
          "SQLitePCLRaw.lib.e_sqlite3": "2.1.12",
          "SQLitePCLRaw.provider.e_sqlite3": "2.1.12"
        }
      },
      "SQLitePCLRaw.core": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "ETpNw9DY3ckWLgRRAeCHj+GKOuPi61aeczkXhgHexUvqoZBAYg8RYESE2J7O1M7+o6QbdSEZwrw9bfqztUVWXg=="
      },
      "SQLitePCLRaw.lib.e_sqlite3": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "fWi8Dbknuhgg72fWinIdjXVaqO1hHL4YBBwVLnr7e1c9TAZwJ0QE38j9syW1hwx6HaqEVTwI+O07WPdZn8Rp0w=="
      },
      "SQLitePCLRaw.provider.e_sqlite3": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "W3oH4XIfCzFrgUSDKHhN6N+dgzA5YHOR2VxX8GB6Qy7CyrJJgxPEG8NirgYWlPQC5P2jz2knSsexWu4tDUL33g==",
        "dependencies": {
          "SQLitePCLRaw.core": "2.1.12"
        }
      },
      "matrixdevice.core": {
        "type": "Project",
        "dependencies": {
          "AdvancedSharpAdbClient": "[3.6.16, )",
          "SkiaSharp": "[4.153.1, )"
        }
      },
      "matrixdevice.features": {
        "type": "Project",
        "dependencies": {
          "MatrixDevice.Core": "[1.0.0, )"
        }
      },
      "matrixdevice.host": {
        "type": "Project",
        "dependencies": {
          "MatrixDevice.Features": "[1.0.0, )",
          "Microsoft.Data.Sqlite": "[10.0.12, )"
        }
      }
    }
  }
}
```

## src/MatrixDevice.Cli/Program.cs

```csharp
using System.Text.Json;
using MatrixDevice.Core;
using MatrixDevice.Host;

var allowed = new HashSet<string> { "--demo", "--serve", "--data", "--adb", "--serial", "--ldconsole", "--ldplayer-list", "--basic-port", "--automator-port", "--help" };
var values = new Dictionary<string, string?>();
for (var i = 0; i < args.Length; i++)
{
    if (!allowed.Contains(args[i])) throw new ArgumentException("未知参数：" + args[i]);
    var key = args[i];
    values[key] = key is "--data" or "--adb" or "--serial" or "--ldconsole" or "--basic-port" or "--automator-port"
        ? ++i < args.Length ? args[i] : throw new ArgumentException("参数缺少值：" + key) : null;
}
if (values.ContainsKey("--help"))
{
    Console.WriteLine("MatrixDevice.Cli [--demo] [--serve] [--data 路径] [--serial 授权串号] [--ldconsole 绝对路径] [--ldplayer-list] [--basic-port 端口] [--automator-port 端口]");
    return;
}
var directory = Path.GetFullPath(values.GetValueOrDefault("--data") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MatrixDevice.Reconstructed"));
var store = new SettingsStore(Path.Combine(directory, "AppConfig.json"));
var settings = store.Load();
if (values.GetValueOrDefault("--adb") is { } adb) settings = settings with { AdbPath = adb };
if (values.GetValueOrDefault("--serial") is { } serial) settings = settings with { AdbAllowedSerials = [serial] };
if (values.GetValueOrDefault("--ldconsole") is { } ldConsole) settings = settings with { LdConsolePath = ldConsole };
if (values.GetValueOrDefault("--basic-port") is { } basic) settings = settings with { BasicApiPort = int.Parse(basic) };
if (values.GetValueOrDefault("--automator-port") is { } automator) settings = settings with { AutomatorApiPort = int.Parse(automator) };
using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancellation.Cancel(); };
if (values.ContainsKey("--ldplayer-list"))
{
    // Read-only mode must never construct the ADB backend or invoke an ADB client.
    await using var service = new LdPlayerService(values.ContainsKey("--demo")
        ? new DemoLdConsoleRunner() : new LdConsoleRunner(settings.LdConsolePath, settings.CommandTimeoutSeconds), values.ContainsKey("--demo"));
    await service.RefreshAsync(cancellation.Token);
    Console.WriteLine(JsonSerializer.Serialize(service.Instances, JsonDefaults.Options));
    return;
}
await using var runtime = await MatrixRuntime.CreateAsync(settings with { AutoStartApi = false }, directory, values.ContainsKey("--demo"));
Console.WriteLine(JsonSerializer.Serialize(new { mode = runtime.IsDemo ? "demo" : runtime.LdPlayer is not null && settings.AdbAllowedSerials.Length == 0 ? "ldplayer" : "adb-existing-server", discoveryError = runtime.DiscoveryError, devices = runtime.GetDevices(), ldPlayerError = runtime.LdPlayerError, ldPlayerInstances = runtime.LdPlayer?.Instances }, JsonDefaults.Options));
if (values.ContainsKey("--serve"))
{
    await runtime.StartApisAsync(cancellation.Token);
    foreach (var device in runtime.Devices.Snapshot.Where(x => x.Online)) await runtime.Api.StartDeviceAsync(device.Serial, cancellation.Token);
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        basicGateway = $"http://localhost:{settings.BasicApiPort}/",
        automatorGateway = $"http://127.0.0.1:{settings.AutomatorApiPort}/",
        devicePorts = runtime.Api.DevicePorts
    }, JsonDefaults.Options));
    try
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        while (await timer.WaitForNextTickAsync(cancellation.Token))
        {
            try
            {
                if (runtime.IsDemo || settings.AdbAllowedSerials.Length > 0) await runtime.RefreshAsync(cancellation.Token);
                if (runtime.LdPlayer is not null) await runtime.RefreshLdPlayerAsync(cancellation.Token);
            }
            catch (Exception e) when (e is not OperationCanceledException) { runtime.Log.Write("discovery-error", e.Message); Console.Error.WriteLine(e.Message); }
        }
    }
    catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
}

```

## src/MatrixDevice.Core/Devices/AdbDeviceBackend.cs

```csharp
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace MatrixDevice.Core;

public sealed class AdbDeviceBackend : IDeviceBackend
{
    private readonly AppSettings _settings;
    private readonly ExistingAdbTransport _transport;
    private readonly HashSet<string> _allowedSerials;
    private readonly SerialCommandQueue _queue = new();
    private readonly SemaphoreSlim _discoveryGate = new(1, 1);
    private readonly ConcurrentDictionary<string, PhoneAgentClient> _agents = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Func<string, CancellationToken, Task>> _unicodeInputs = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Func<CancellationToken, Task>> _identityGuards = new(StringComparer.Ordinal);
    public const int MaximumStdoutBytes = 32 * 1024 * 1024;
    public const int MaximumStderrBytes = 1024 * 1024;
    private int _disposed;
    public AdbDeviceBackend(AppSettings settings, IPEndPoint? serverEndpoint = null)
    {
        _settings = settings;
        _allowedSerials = settings.AdbAllowedSerials.ToHashSet(StringComparer.Ordinal);
        foreach (var serial in _allowedSerials) ValidateSerial(serial);
        var endpoint = serverEndpoint ?? new IPEndPoint(IPAddress.Loopback, 5037);
        if (!IPAddress.IsLoopback(endpoint.Address)) throw new ArgumentException("ADB server must be on loopback.", nameof(serverEndpoint));
        _transport = new ExistingAdbTransport(settings, endpoint);
    }

    public Task<string> ConnectNetworkAsync(string ip, int port, CancellationToken ct = default)
    {
        if (!IPAddress.TryParse(ip, out var address) || address.AddressFamily != AddressFamily.InterNetwork || port is < 1 or > 65535)
            throw new DeviceException($"Invalid ADB network endpoint: {ip}:{port}.", 400, 1007);
        var endpoint = address + ":" + port.ToString(CultureInfo.InvariantCulture);
        return _queue.RunAsync("$adb-network-control", async token =>
        {
            var output = Decode(await ExecuteAsync(null, ["connect", endpoint], token).ConfigureAwait(false));
            var lines = output.Split('\n').Select(line => line.Trim()).ToArray();
            if (!lines.Any(line => line.Equals("connected to " + endpoint, StringComparison.OrdinalIgnoreCase) || line.Equals("already connected to " + endpoint, StringComparison.OrdinalIgnoreCase)))
                throw new DeviceException($"ADB did not confirm connection to {endpoint}: {output}", 503, 2000);
            return output;
        }, ct);
    }

    public void RegisterAgent(string serial, PhoneAgentClient client)
    {
        AuthorizeSerial(serial); ArgumentNullException.ThrowIfNull(client);
        if (!_agents.TryAdd(serial, client)) throw new DeviceException($"An agent is already registered for serial={serial}.");
    }
    public void RegisterUnicodeInput(string serial, Func<string, CancellationToken, Task> send)
    {
        AuthorizeSerial(serial); ArgumentNullException.ThrowIfNull(send);
        if (!_unicodeInputs.TryAdd(serial, send)) throw new DeviceException($"Unicode input is already bound: serial={serial}.");
    }
    public void RegisterIdentityGuard(string serial, Func<CancellationToken, Task> verify)
    {
        AuthorizeSerial(serial); ArgumentNullException.ThrowIfNull(verify);
        if (!_identityGuards.TryAdd(serial, verify)) throw new DeviceException($"An identity guard is already bound: serial={serial}.", 409, 1003);
    }
    public void InheritServerEpoch(AdbDeviceBackend source) => _transport.InheritServerEpoch(source._transport);
    public Task VerifyIdentityAsync(string serial, CancellationToken ct = default)
    {
        AuthorizeSerial(serial);
        if (!_identityGuards.ContainsKey(serial)) throw new DeviceException("设备身份尚未绑定。", 403, 1003);
        return RunDeviceAsync(serial, async token => { await ExecuteAsync(null, ["devices", "-l"], token).ConfigureAwait(false); }, ct);
    }

    public async Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default)
    {
        await _discoveryGate.WaitAsync(ct).ConfigureAwait(false);
        try { return await DiscoverCoreAsync(ct).ConfigureAwait(false); }
        finally { _discoveryGate.Release(); }
    }
    public async Task<IReadOnlyList<AdbRegisteredDevice>> ListRegisteredDevicesAsync(CancellationToken ct = default)
    {
        var output = Decode(await ExecuteAsync(null, ["devices", "-l"], ct).ConfigureAwait(false));
        var devices = new List<AdbRegisteredDevice>();
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var value = line.Trim();
            if (value.StartsWith("List of devices", StringComparison.Ordinal) || value.StartsWith('*') || value.Length == 0) continue;
            var fields = Regex.Split(value, @"\s+");
            if (fields.Length < 2) throw new DeviceException("Malformed ADB host directory row.", 502, 2000);
            ValidateSerial(fields[0]);
            var identity = fields.FirstOrDefault(field => field.StartsWith("transport_id:", StringComparison.Ordinal));
            uint? transport = uint.TryParse(identity?[13..], NumberStyles.None, CultureInfo.InvariantCulture, out var number) && number > 0 ? number : null;
            devices.Add(new(fields[0], fields[1], transport));
        }
        return devices.AsReadOnly();
    }
    private async Task<IReadOnlyList<DeviceInfo>> DiscoverCoreAsync(CancellationToken ct)
    {
        try
        {
        var result = await ExecuteAsync(null, ["devices", "-l"], ct).ConfigureAwait(false);
        var discovered = new List<DeviceInfo>();
        foreach (var line in Decode(result).Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var value = line.Trim();
            if (value.StartsWith("List of devices", StringComparison.Ordinal) || value.StartsWith('*') || value.Length == 0) continue;
            var fields = Regex.Split(value, @"\s+");
            if (fields.Length < 2) throw new DeviceException($"Malformed ADB discovery row: {value}", 503, 2000);
            var serial = fields[0]; ValidateSerial(serial);
            if (!_allowedSerials.Contains(serial)) continue;
            var state = fields[1] switch { "device" => "Ready", "offline" => "Offline", "unauthorized" => "Unauthorized", _ => "Failed" };
            var identity = fields.FirstOrDefault(field => field.StartsWith("transport_id:", StringComparison.Ordinal));
            uint? transport = uint.TryParse(identity?[13..], NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id > 0 ? id : null;
            if (state == "Ready" && transport is null) state = "Failed";
            var model = fields.FirstOrDefault(field => field.StartsWith("model:", StringComparison.Ordinal))?[6..].Replace('_', ' ') ?? serial;
            var device = new DeviceInfo { Serial = serial, Name = model, Model = model, State = state, TransportId = transport, ConnectionType = serial.StartsWith("emulator-", StringComparison.Ordinal) ? "Emulator" : serial.Contains(':') ? "TCP" : "USB", Error = state == "Ready" ? null : $"ADB state: {fields[1]}; transport_id={identity ?? "missing"}" };
            discovered.Add(device);
        }
        _transport.ReplaceTransports(discovered.Where(device => device.Online).ToDictionary(device => device.Serial, device => device.TransportId!.Value, StringComparer.Ordinal));
        var devices = new List<DeviceInfo>();
        foreach (var observed in discovered)
        {
            var device = observed; var serial = observed.Serial;
            if (device.Online)
            {
                try
                {
                    device = await RunDeviceAsync(serial, async token =>
                    {
                        var props = Decode(await ExecuteAsync(serial, ["shell", "getprop"], token)).Split('\n');
                        string Get(string key) => props.FirstOrDefault(item => item.StartsWith("[" + key + "]: [", StringComparison.Ordinal))?.Trim().Split("]: [", 2)[1].TrimEnd(']') ?? "";
                        var size = Decode(await ExecuteAsync(serial, ["shell", "wm", "size"], token));
                        var input = ""; string? rotationError = null;
                        try { input = Decode(await ExecuteAsync(serial, ["shell", "dumpsys input"], token)); }
                        catch (DeviceException ex) when (ex.Status is not (409 or 413)) { rotationError = "Rotation unavailable: " + ex.Message; }
                        var display = ParseDisplayInfo(size, input);
                        return device with { Brand = Get("ro.product.brand"), Model = Get("ro.product.model"), AndroidVersion = Get("ro.build.version.release"), Width = display.Width, Height = display.Height, Rotation = display.Rotation, Error = rotationError };
                    }, ct).ConfigureAwait(false);
                }
                catch (DeviceException ex) { _transport.InvalidateTransport(serial); device = device with { State = "Failed", Error = ex.Message }; }
            }
            devices.Add(device);
        }
        return devices.AsReadOnly();
        }
        catch { _transport.ReplaceTransports(new(StringComparer.Ordinal)); throw; }
    }

    public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        return RunDeviceAsync(serial, async token => Decode(await ExecuteAsync(serial, useRoot ? ["shell", "su", "-c", Quote(command)] : ["shell", command], token)), ct);
    }
    public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => RunDeviceAsync(serial, async token =>
    {
        var png = await ExecuteAsync(serial, ["exec-out", "screencap", "-p"], token).ConfigureAwait(false);
        try { PngImage.ReadDimensions(png); }
        catch (Exception ex) when (ex is InvalidDataException or EndOfStreamException) { throw new DeviceException($"Invalid Android screenshot: serial={serial}: {ex.Message}", 502, 2000); }
        return png;
    }, ct);

    public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => RunDeviceAsync(serial, async token =>
    {
        var path = "/data/local/tmp/matrix-ui-" + Guid.NewGuid().ToString("N") + ".xml";
        try
        {
            var output = Decode(await ExecuteAsync(serial, ["shell", "uiautomator dump " + Quote(path) + " 2>&1"], token));
            if (output.Contains("ERROR", StringComparison.OrdinalIgnoreCase) || !output.Contains(path, StringComparison.Ordinal))
                throw new DeviceException($"UI dump did not create a confirmed node snapshot: serial={serial}. Pause moving content or open a static page and retry. {output}", 409, 1003);
            var xml = Decode(await ExecuteAsync(serial, ["shell", "cat " + Quote(path)], token));
            try { return ParseUiXml(xml); }
            catch (Exception ex) when (ex is XmlException or InvalidDataException or FormatException or OverflowException)
            { throw new DeviceException($"Invalid UI node snapshot: serial={serial}: {ex.Message}", 502, 2000); }
        }
        finally
        {
            if (!token.IsCancellationRequested)
                await ExecuteAsync(serial, ["shell", "rm -f " + Quote(path)], token).ConfigureAwait(false);
        }
    }, ct);

    public static AndroidNode ParseUiXml(string xml)
    {
        var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 16 * 1024 * 1024 };
        using (var inspection = XmlReader.Create(new StringReader(xml), settings))
        {
            var elements = 0;
            while (inspection.Read())
            {
                if (inspection.Depth > 256) throw new InvalidDataException("UI dump exceeds the maximum XML depth of 256.");
                if (inspection.NodeType == XmlNodeType.Element && ++elements > 100000) throw new InvalidDataException("UI dump exceeds 100000 elements.");
            }
        }
        using var reader = XmlReader.Create(new StringReader(xml), settings);
        var root = XDocument.Load(reader).Root ?? throw new InvalidDataException("UI dump has no root.");
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        AndroidNode ConvertNode(XElement element)
        {
            string? Text(string name) => (string?)element.Attribute(name);
            bool Flag(string name) => bool.TryParse(Text(name), out var flag) && flag;
            var boundsText = Text("bounds") ?? "[0,0][0,0]";
            var match = Regex.Match(boundsText, @"^\[(-?\d+),(-?\d+)\]\[(-?\d+),(-?\d+)\]$");
            if (!match.Success) throw new InvalidDataException($"Invalid UI node bounds: {boundsText}");
            var numbers = match.Groups.Cast<Group>().Skip(1).Select(group => int.Parse(group.Value, CultureInfo.InvariantCulture)).ToArray();
            var nodeClass = Text("class") ?? "";
            counts.TryGetValue(nodeClass, out var instance); counts[nodeClass] = instance + 1;
            return new AndroidNode
            {
                Text = Text("text"), ResourceId = Text("resource-id"), ClassName = nodeClass, PackageName = Text("package"), ContentDesc = Text("content-desc"),
                Index = int.TryParse(Text("index"), out var index) ? index : 0, Instance = instance,
                Clickable = Flag("clickable"), Enabled = Flag("enabled"), Focusable = Flag("focusable"), Focused = Flag("focused"), Scrollable = Flag("scrollable"),
                LongClickable = Flag("long-clickable"), Checkable = Flag("checkable"), Checked = Flag("checked"), Selected = Flag("selected"),
                VisibleToUser = Text("visible-to-user") is null || Flag("visible-to-user"), Bounds = new(numbers[0], numbers[1], numbers[2], numbers[3]),
                ChildNodeList = element.Elements("node").Select(ConvertNode).ToList()
            };
        }
        if (root.Name.LocalName == "node") return ConvertNode(root);
        return new AndroidNode { ClassName = "hierarchy", Enabled = true, ChildNodeList = root.Elements("node").Select(ConvertNode).ToList() };
    }

    public static (int Width, int Height, int? Rotation) ParseDisplayInfo(string wmSize, string inputDump)
    {
        var sizes = Regex.Matches(wmSize, @"(\d+)x(\d+)");
        var width = sizes.Count == 0 ? 0 : int.Parse(sizes[^1].Groups[1].Value, CultureInfo.InvariantCulture);
        var height = sizes.Count == 0 ? 0 : int.Parse(sizes[^1].Groups[2].Value, CultureInfo.InvariantCulture);
        var viewport = inputDump.Split('\n').FirstOrDefault(line => Regex.IsMatch(line, @"\bdisplayId\s*[=:]\s*0\b") && Regex.IsMatch(line, @"\borientation\s*[=:]\s*[0-3]\b")) ?? "";
        var orientation = Regex.Match(inputDump, @"\bSurfaceOrientation\s*:\s*([0-3])\b");
        if (!orientation.Success) orientation = Regex.Match(viewport, @"\borientation\s*[=:]\s*([0-3])\b");
        int? rotation = orientation.Success ? int.Parse(orientation.Groups[1].Value, CultureInfo.InvariantCulture) * 90 : null;
        var surfaceWidth = Regex.Match(inputDump, @"\bSurfaceWidth\s*:\s*(\d+)\b");
        var surfaceHeight = Regex.Match(inputDump, @"\bSurfaceHeight\s*:\s*(\d+)\b");
        if (surfaceWidth.Success && surfaceHeight.Success)
            return (int.Parse(surfaceWidth.Groups[1].Value, CultureInfo.InvariantCulture), int.Parse(surfaceHeight.Groups[1].Value, CultureInfo.InvariantCulture), rotation);
        var frame = Regex.Match(viewport, @"logicalFrame\s*=\s*\[(-?\d+),\s*(-?\d+)\]\[(-?\d+),\s*(-?\d+)\]");
        if (!frame.Success) frame = Regex.Match(viewport, @"logicalFrame\s*=\s*\[(-?\d+),\s*(-?\d+),\s*(-?\d+),\s*(-?\d+)\]");
        if (frame.Success)
        {
            var x = int.Parse(frame.Groups[1].Value, CultureInfo.InvariantCulture); var y = int.Parse(frame.Groups[2].Value, CultureInfo.InvariantCulture);
            var right = int.Parse(frame.Groups[3].Value, CultureInfo.InvariantCulture); var bottom = int.Parse(frame.Groups[4].Value, CultureInfo.InvariantCulture);
            if (right > x && bottom > y) return (right - x, bottom - y, rotation);
        }
        // Logical viewport dimensions take priority; never double-swap an already-landscape override.
        if (rotation is 90 or 270 && width < height) (width, height) = (height, width);
        return (width, height, rotation);
    }

    public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) { ValidatePoint(x, y); return CommandAsync(serial, $"input tap {x} {y}", ct); }
    public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default)
    { ValidatePoint(x1, y1); ValidatePoint(x2, y2); if (durationMs < 1 || durationMs > 60000) throw new DeviceException($"Invalid swipe duration: {durationMs}.", 400, 1007); return CommandAsync(serial, $"input swipe {x1} {y1} {x2} {y2} {durationMs}", ct); }
    public Task KeyEventAsync(string serial, int code, CancellationToken ct = default)
    { if (code < 0 || code > 400) throw new DeviceException($"Invalid Android key code: {code}.", 400, 1007); return CommandAsync(serial, $"input keyevent {code}", ct); }
    public Task TextAsync(string serial, string text, CancellationToken ct = default)
    {
        AuthorizeSerial(serial);
        ArgumentNullException.ThrowIfNull(text);
        if (text.Any(value => value < 32 || value > 126) || text.Contains("%s", StringComparison.Ordinal))
        {
            if (!_unicodeInputs.TryGetValue(serial, out var send))
                throw new DeviceException("Global Unicode input requires an explicitly bound LDPlayer instance; node set-text still requires a phone agent.", 501, 2001);
            if (text.Length > 4096 || text.Any(value => value < 32) || text.TrimStart().StartsWith("--", StringComparison.Ordinal))
                throw new DeviceException("LDPlayer text must have at most 4096 characters, no control characters or option prefix.", 400, 1007);
            return RunDeviceAsync(serial, async token =>
            {
                await ExecuteAsync(serial, ["shell", ":"], token).ConfigureAwait(false);
                await send(text, token).ConfigureAwait(false);
            }, ct);
        }
        return CommandAsync(serial, "input text " + Quote(text.Replace(" ", "%s", StringComparison.Ordinal)), ct);
    }
    public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default)
    {
        ValidatePackage(packageName);
        if (activity is not null && !Regex.IsMatch(activity, @"^[A-Za-z0-9_.$]+$")) throw new DeviceException($"Invalid activity: {activity}.", 400, 1007);
        return RunDeviceAsync(serial, async token =>
        {
            var component = packageName + "/" + activity;
            if (activity is null)
            {
                // Launcher aliases need not have CATEGORY_DEFAULT, so resolve before an explicit start.
                var resolution = Decode(await ExecuteAsync(serial, ["shell", "cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER -p " + packageName], token).ConfigureAwait(false));
                var candidates = resolution.Split('\n').Select(line => line.Trim()).Where(line => line.StartsWith(packageName + "/", StringComparison.Ordinal) && Regex.IsMatch(line[(packageName.Length + 1)..], @"^[A-Za-z0-9_.$]+$")).ToArray();
                if (candidates.Length != 1) throw new DeviceException($"No unique enabled launcher activity: serial={serial}, package={packageName}: {resolution}", 409, 1003);
                component = candidates[0];
            }
            var command = "am start -W -a android.intent.action.MAIN -c android.intent.category.LAUNCHER -n " + component;
            var output = Decode(await ExecuteAsync(serial, ["shell", command], token).ConfigureAwait(false));
            if (output.Split('\n').Any(line => line.TrimStart().StartsWith("Error", StringComparison.OrdinalIgnoreCase)) ||
                Regex.IsMatch(output, @"(?im)^\s*Status:\s*(?!ok\s*$)\S+"))
                throw new DeviceException($"Android activity launch failed: serial={serial}, package={packageName}: {output}", 409, 1003);
            if (!output.Contains("Status: ok", StringComparison.OrdinalIgnoreCase))
                throw new DeviceException($"Android activity launch was not confirmed: serial={serial}, package={packageName}: {output}", 502, 2000);
            return output;
        }, ct);
    }
    public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) { ValidatePackage(packageName); return ShellAsync(serial, "am force-stop " + packageName, ct: ct); }
    public async Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) { ValidatePackage(packageName); return (await ShellAsync(serial, "pm clear " + packageName, ct: ct)).Trim() == "Success"; }
    public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => RunDeviceAsync<IReadOnlyList<AppInfo>>(serial, async token =>
    {
        var output = Decode(await ExecuteAsync(serial, showSystem ? ["shell", "pm list packages"] : ["shell", "pm list packages -3"], token));
        var system = showSystem ? Decode(await ExecuteAsync(serial, ["shell", "pm list packages -s"], token)).Split('\n').Select(line => line.Trim().Replace("package:", "", StringComparison.Ordinal)).ToHashSet(StringComparer.Ordinal) : [];
        var apps = new List<AppInfo>();
        foreach (var line in output.Split('\n'))
        {
            if (!line.Trim().StartsWith("package:", StringComparison.Ordinal)) continue;
            var package = line.Trim()[8..]; ValidatePackage(package);
            string? version = null;
            if (needVersion)
            {
                var detail = Decode(await ExecuteAsync(serial, ["shell", "dumpsys package " + package], token));
                version = Regex.Match(detail, @"versionName=([^\r\n]+)").Groups[1].Value.Trim();
            }
            apps.Add(new(package, package, version, system.Contains(package)));
        }
        return apps.AsReadOnly();
    }, ct);
    public Task InstallAsync(string serial, string localPath, CancellationToken ct = default)
    {
        ValidateLocalFile(localPath);
        return RunDeviceAsync(serial, async token => { var output = Decode(await ExecuteAsync(serial, ["install", "-r", Path.GetFullPath(localPath)], token)); if (output.Trim() != "Success") throw new DeviceException($"APK install failed: serial={serial}: {output}", 409, 1003); }, ct);
    }
    public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default)
    { ValidatePackage(packageName); return RunDeviceAsync(serial, async token => { var output = Decode(await ExecuteAsync(serial, ["uninstall", packageName], token)); if (output.Trim() != "Success") throw new DeviceException($"Uninstall failed: serial={serial}: {output}", 409, 1003); }, ct); }
    public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default)
    { ValidateLocalFile(localPath); ValidateRemotePath(remotePath); return RunDeviceAsync(serial, async token => { progress?.Report(0); await ExecuteAsync(serial, ["push", Path.GetFullPath(localPath), remotePath], token); progress?.Report(1); }, ct); }
    public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        ValidateRemotePath(remotePath); ArgumentException.ThrowIfNullOrWhiteSpace(localPath);
        return RunDeviceAsync(serial, async token => { progress?.Report(0); await ExecuteAsync(serial, ["pull", remotePath, Path.GetFullPath(localPath)], token); if (!File.Exists(localPath)) throw new DeviceException($"Pull returned without the requested file: {localPath}", 502, 2000); progress?.Report(1); }, ct);
    }
    public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default)
    {
        AuthorizeSerial(serial);
        if (!_agents.TryGetValue(serial, out var agent)) throw new DeviceException($"Node set-text requires an explicitly registered phone agent: serial={serial}.", 501, 2001);
        return RunDeviceAsync(serial, token => agent.SetNodeTextAsync(selector, text, token), ct);
    }

    private Task CommandAsync(string serial, string command, CancellationToken ct) => RunDeviceAsync(serial, async token => { await ExecuteAsync(serial, ["shell", command], token); }, ct);
    private Task<T> RunDeviceAsync<T>(string serial, Func<CancellationToken, Task<T>> action, CancellationToken ct)
    {
        AuthorizeSerial(serial);
        var expected = _transport.CurrentTransport(serial) ?? throw new DeviceException($"ADB transport has not been discovered: {serial}; refresh devices first.", 503, 2000);
        return _queue.RunAsync(serial, token => _transport.RunPinnedAsync(serial, expected, async pinned =>
        {
            if (_identityGuards.TryGetValue(serial, out var verify)) await verify(pinned).ConfigureAwait(false);
            return await action(pinned).ConfigureAwait(false);
        }, token), ct);
    }
    internal uint? ObservedTransportId(string serial) => _transport.CurrentTransport(serial);
    private Task RunDeviceAsync(string serial, Func<CancellationToken, Task> action, CancellationToken ct) => RunDeviceAsync(serial, async token => { await action(token).ConfigureAwait(false); return true; }, ct);
    private Task<byte[]> ExecuteAsync(string? serial, string[] arguments, CancellationToken ct)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        if (serial is not null) AuthorizeSerial(serial);
        return _transport.ExecuteAsync(serial, arguments, ct);
    }
    public static string Quote(string value) => "'" + value.Replace("'", "'\"'\"'", StringComparison.Ordinal) + "'";
    private static string Decode(byte[] bytes) => Encoding.UTF8.GetString(bytes).TrimEnd('\r', '\n');
    private void AuthorizeSerial(string serial)
    {
        ValidateSerial(serial);
        if (!_allowedSerials.Contains(serial)) throw new DeviceException($"ADB serial is not authorized: {serial}.", 403, 1007);
    }
    private static void ValidateSerial(string serial) { if (string.IsNullOrWhiteSpace(serial) || serial.StartsWith('-') || serial.Any(char.IsWhiteSpace) || serial.Contains('\0')) throw new DeviceException($"Invalid device serial: {serial}.", 400, 1007); }
    private static void ValidatePoint(int x, int y) { if (x < 0 || y < 0) throw new DeviceException($"Negative device point: ({x},{y}).", 400, 1007); }
    private static void ValidatePackage(string value) { if (value != "android" && !Regex.IsMatch(value ?? "", @"^[A-Za-z][A-Za-z0-9_]*(\.[A-Za-z][A-Za-z0-9_]*)+$")) throw new DeviceException($"Invalid package name: {value}.", 400, 1007); }
    private static void ValidateLocalFile(string path) { if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) throw new DeviceException($"Local file does not exist: {path}.", 400, 1007); }
    private static void ValidateRemotePath(string path) { if (string.IsNullOrWhiteSpace(path) || !path.StartsWith('/') || path.Contains('\0') || path.Contains('\n')) throw new DeviceException($"Invalid absolute Android path: {path}.", 400, 1007); }
    public async ValueTask DisposeAsync() { if (Interlocked.Exchange(ref _disposed, 1) != 0) return; await _queue.DisposeAsync(); foreach (var agent in _agents.Values) await agent.DisposeAsync(); }
}

public sealed record AdbRegisteredDevice(string Serial, string State, uint? TransportId);

```

## src/MatrixDevice.Core/Devices/AdbServerIdentity.cs

```csharp
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace MatrixDevice.Core;

internal readonly record struct AdbServerIdentity(int ProcessId, long StartedUtcTicks)
{
    public static AdbServerIdentity ForConnection(Socket socket)
    {
        if (!OperatingSystem.IsWindows()) throw new DeviceException("Existing ADB owner verification requires Windows.", 503, 2000);
        var local = socket.LocalEndPoint as IPEndPoint ?? throw new IOException("Missing ADB local endpoint.");
        var remote = socket.RemoteEndPoint as IPEndPoint ?? throw new IOException("Missing ADB remote endpoint.");
        var size = 0;
        var result = GetExtendedTcpTable(IntPtr.Zero, ref size, false, 2, 4, 0);
        for (var attempt = 0; attempt < 3 && result == 122; attempt++)
        {
            if (size < 4 || size > 8 * 1024 * 1024) throw new IOException($"Invalid TCP table size: {size}.");
            var buffer = Marshal.AllocHGlobal(size);
            try
            {
                var capacity = size;
                result = GetExtendedTcpTable(buffer, ref size, false, 2, 4, 0);
                if (result == 122) continue;
                if (result != 0) throw new IOException($"TCP owner query failed: {result}.");
                var count = Marshal.ReadInt32(buffer);
                var stride = Marshal.SizeOf<TcpRow>();
                if (count < 0 || count > (capacity - 4) / stride) throw new IOException("Malformed TCP owner table.");
                var owners = new HashSet<int>();
                for (var i = 0; i < count; i++)
                {
                    var row = Marshal.PtrToStructure<TcpRow>(buffer + 4 + i * stride);
                    // Identify the server end of this exact established connection, not only its listener.
                    if (row.State == 5 && Address(row.LocalAddress).Equals(remote.Address) && Port(row.LocalPort) == remote.Port &&
                        Address(row.RemoteAddress).Equals(local.Address) && Port(row.RemotePort) == local.Port)
                        owners.Add(checked((int)row.ProcessId));
                }
                if (owners.Count != 1) throw new DeviceException($"ADB connection must have one verified server owner; actual={owners.Count}.", 503, 2000);
                using var process = Process.GetProcessById(owners.Single());
                var identity = new AdbServerIdentity(process.Id, process.StartTime.ToUniversalTime().Ticks);
                if (process.HasExited) throw new IOException("ADB server exited during owner verification.");
                return identity;
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }
        throw new IOException($"TCP owner query failed after bounded retries: {result}.");
    }

    private static IPAddress Address(uint value) => new(BitConverter.GetBytes(value));
    private static int Port(uint value) => (int)(((value & 255) << 8) | ((value >> 8) & 255));
    [StructLayout(LayoutKind.Sequential)]
    private struct TcpRow { public uint State, LocalAddress, LocalPort, RemoteAddress, RemotePort, ProcessId; }
    [DllImport("iphlpapi.dll", ExactSpelling = true)]
    private static extern uint GetExtendedTcpTable(IntPtr table, ref int size, [MarshalAs(UnmanagedType.Bool)] bool ordered, uint family, uint tableClass, uint reserved);
}

internal sealed class AdbServerEpoch
{
    private readonly object gate = new();
    private AdbServerIdentity? observed;
    public AdbServerIdentity Snapshot { get { lock (gate) return observed ?? throw new DeviceException("尚未核验ADB服务身份，不能继承连接。", 409, 1003); } }
    public void Require(AdbServerIdentity current)
    {
        lock (gate)
        {
            if (observed is { } previous && previous != current)
                throw new DeviceException($"ADB server owner changed: previous={previous}, current={current}. Restart this client and rediscover before continuing; server left untouched.", 409, 1003);
            observed = current;
        }
    }
}

```

## src/MatrixDevice.Core/Devices/DemoDeviceBackend.cs

```csharp
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace MatrixDevice.Core;

public sealed class DemoDeviceBackend : IDeviceBackend
{
    private sealed class DeviceState
    {
        public required DeviceInfo Info { get; set; }
        public int Count;
        public string Text = "DEMO INPUT";
        public string ActiveApp = "com.matrix.demo";
        public bool Enabled = true;
        public Dictionary<string, byte[]> Files { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, string> Properties { get; } = new(StringComparer.Ordinal);
        public HashSet<string> Apps { get; } = new(StringComparer.Ordinal) { "com.matrix.demo", "com.android.settings" };
    }
    private readonly SerialCommandQueue _queue = new();
    private readonly ConcurrentDictionary<string, DeviceState> _devices = new(StringComparer.Ordinal);
    public bool IsSimulation => true;

    public DemoDeviceBackend()
    {
        foreach (var (serial, width, height, model) in new[] { ("DEMO-001", 360, 640, "演示设备 A"), ("DEMO-002", 412, 720, "演示设备 B") })
            _devices[serial] = new DeviceState { Info = new DeviceInfo { Serial = serial, Name = model + "（模拟）", Brand = "演示", Model = model, AndroidVersion = "模拟", State = "Ready", ConnectionType = "Simulation", Width = width, Height = height, Rotation = 0 } };
    }
    public void SetOnline(string serial, bool online)
    {
        if (!_devices.TryGetValue(serial, out var device)) throw new DeviceException($"Demo device not found: {serial}.", 404, 1002);
        lock (device) device.Info = device.Info with { State = online ? "Ready" : "Offline", Error = online ? null : "Demo disconnect" };
    }
    public void SetDimensions(string serial, int width, int height)
    {
        if (width <= 0 || height <= 0 || (long)width * height > 24_000_000) throw new DeviceException($"Invalid demo dimensions: {width}x{height}.", 400, 1007);
        var device = Get(serial);
        lock (device) device.Info = device.Info with { Width = width, Height = height };
    }
    public void SetRotation(string serial, int degrees)
    {
        if (degrees is not (0 or 90 or 180 or 270)) throw new DeviceException($"Invalid demo rotation: {degrees}.", 400, 1007);
        var device = Get(serial);
        lock (device)
        {
            var wasLandscape = device.Info.Rotation is 90 or 270;
            var isLandscape = degrees is 90 or 270;
            device.Info = device.Info with
            {
                Rotation = degrees,
                Width = wasLandscape == isLandscape ? device.Info.Width : device.Info.Height,
                Height = wasLandscape == isLandscape ? device.Info.Height : device.Info.Width
            };
        }
    }
    public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default)
    { ct.ThrowIfCancellationRequested(); return Task.FromResult<IReadOnlyList<DeviceInfo>>(_devices.Values.Select(device => { lock (device) return device.Info; }).OrderBy(device => device.Serial).ToArray()); }
    private DeviceState Get(string serial)
    {
        if (!_devices.TryGetValue(serial, out var device)) throw new DeviceException($"Demo device not found: serial={serial}.", 404, 1002);
        if (!device.Info.Online) throw new DeviceException($"Demo device offline: serial={serial}.", 410, 1003);
        return device;
    }
    private Task<T> Run<T>(string serial, Func<DeviceState, T> action, CancellationToken ct) => _queue.RunAsync(serial, token => { token.ThrowIfCancellationRequested(); var device = Get(serial); lock (device) return Task.FromResult(action(device)); }, ct);
    private Task Run(string serial, Action<DeviceState> action, CancellationToken ct) => Run(serial, device => { action(device); return true; }, ct);

    public Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default) => Run(serial, device =>
    {
        var input = command.Trim();
        if (input == "wm size") return $"Physical size: {device.Info.Width}x{device.Info.Height}";
        if (input.StartsWith("getprop ", StringComparison.Ordinal)) return device.Properties.GetValueOrDefault(input[8..].Trim().Trim('\'', '"'), "");
        if (input == "getprop") return "[ro.product.model]: [" + device.Info.Model + "]\n[ro.build.version.release]: [Demo]";
        if (input.StartsWith("setprop ", StringComparison.Ordinal))
        { var split = input[8..].Split(' ', 2); if (split.Length != 2) throw new DeviceException("Demo setprop requires key and value.", 400, 1007); device.Properties[split[0].Trim('\'', '"')] = split[1].Trim('\'', '"'); return ""; }
        if (input == "id" || input == "id -u") return useRoot ? "0 (simulation)" : "2000 (simulation)";
        if (input.StartsWith("pm list packages", StringComparison.Ordinal)) return string.Join('\n', device.Apps.Select(package => "package:" + package));
        if (input.StartsWith("am force-stop ", StringComparison.Ordinal)) { device.ActiveApp = "com.matrix.demo"; return ""; }
        if (input.StartsWith("pm clear ", StringComparison.Ordinal)) { device.Count = 0; device.Text = "DEMO INPUT"; return "Success"; }
        if (input.StartsWith("monkey -p ", StringComparison.Ordinal)) { device.ActiveApp = input.Split(' ')[2]; return "Events injected: 1 (simulation)"; }
        if (input.StartsWith("input tap ", StringComparison.Ordinal)) { device.Count++; return ""; }
        if (input.StartsWith("input swipe ", StringComparison.Ordinal)) { device.Count++; return ""; }
        if (input.StartsWith("input keyevent ", StringComparison.Ordinal)) { device.ActiveApp = "com.matrix.demo"; return ""; }
        if (input.StartsWith("input text ", StringComparison.Ordinal)) { device.Text = input[11..].Trim('\'', '"').Replace("%s", " ", StringComparison.Ordinal); return ""; }
        if (input.StartsWith("ls", StringComparison.Ordinal)) return string.Join('\n', device.Files.Keys);
        throw new DeviceException($"Demo shell does not implement: {command}. This is a simulated backend.", 501, 2001);
    }, ct);

    public Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default) => Run(serial, device =>
    {
        var width = device.Info.Width; var height = device.Info.Height;
        var accent = serial == "DEMO-001" ? (R: (byte)20, G: (byte)150, B: (byte)132) : (R: (byte)198, G: (byte)76, B: (byte)107);
        var words = new List<(string Text, int X, int Y, int Scale)>
        {
            ("SIMULATED DEVICE", 22, 22, 2), (serial, 22, 68, 3), ("MATRIX DEMO", 22, 126, 3),
            ("TAP " + device.Count, 48, 214, 4), (device.Text.ToUpperInvariant(), 26, 326, 2),
            ("SETTINGS", 30, 420, 2), ("APPS", 30, 470, 2), ("FILES", 30, 520, 2)
        };
        return PngImage.Create(width, height, (x, y) =>
        {
            (byte R, byte G, byte B) color = (242, 246, 247);
            if (y < 100) color = accent;
            else if (y > 190 && y < 282 && x > 22 && x < width - 22) color = (222, 234, 238);
            else if (y > 307 && y < 372 && x > 22 && x < width - 22) color = (255, 255, 255);
            else if (y > height - 48) color = (26, 36, 39);
            if (words.Any(word => GlyphPixel(word.Text, x - word.X, y - word.Y, word.Scale))) color = y < 100 ? ((byte)255, (byte)255, (byte)255) : ((byte)25, (byte)41, (byte)46);
            if (y > height - 28 && y < height - 24 && x > width / 2 - 42 && x < width / 2 + 42) color = (255, 255, 255);
            return color;
        });
    }, ct);

    public Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default) => Run(serial, device => new AndroidNode
    {
        ClassName = "android.widget.FrameLayout", PackageName = device.ActiveApp, Enabled = true, Bounds = new(0, 0, device.Info.Width, device.Info.Height),
        ChildNodeList =
        [
            new() { Text = "SIMULATED DEVICE " + serial, ResourceId = "com.matrix.demo:id/title", ClassName = "android.widget.TextView", PackageName = device.ActiveApp, Enabled = true, Bounds = new(22, 10, device.Info.Width - 22, 98) },
            new() { Text = "Tap " + device.Count, ResourceId = "com.matrix.demo:id/tap", ClassName = "android.widget.Button", PackageName = device.ActiveApp, Clickable = true, Enabled = true, Index = 1, Bounds = new(22, 190, device.Info.Width - 22, 282) },
            new() { Text = device.Text, ResourceId = "com.matrix.demo:id/input", ClassName = "android.widget.EditText", PackageName = device.ActiveApp, Clickable = true, Enabled = true, Focusable = true, Focused = true, Index = 2, Bounds = new(22, 307, device.Info.Width - 22, 372) },
            new() { Text = "Settings", ResourceId = "com.matrix.demo:id/settings", ClassName = "android.widget.Button", PackageName = device.ActiveApp, Clickable = true, Enabled = true, Instance = 1, Index = 3, Bounds = new(22, 402, device.Info.Width - 22, 450) }
        ]
    }, ct);
    public Task TapAsync(string serial, int x, int y, CancellationToken ct = default) => Run(serial, device => { CheckPoint(device, x, y); device.Count++; }, ct);
    public Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default) => Run(serial, device => { CheckPoint(device, x1, y1); CheckPoint(device, x2, y2); if (durationMs <= 0 || durationMs > 60000) throw new DeviceException("Swipe duration must be in 1..60000ms.", 400, 1007); device.Count++; }, ct);
    public Task KeyEventAsync(string serial, int code, CancellationToken ct = default) => Run(serial, device => { if (code < 0 || code > 400) throw new DeviceException("Invalid demo key code.", 400, 1007); if (code == 3) device.ActiveApp = "com.matrix.demo"; }, ct);
    public Task TextAsync(string serial, string text, CancellationToken ct = default) => Run(serial, device => device.Text = text, ct);
    public Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default) => Run(serial, device => { if (!device.Apps.Contains(packageName)) throw new DeviceException($"演示应用未安装：{packageName}", 404, 1002); device.ActiveApp = packageName; return "演示应用已启动：" + packageName; }, ct);
    public Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default) => Run(serial, device => { device.ActiveApp = "com.matrix.demo"; return "演示应用已停止：" + packageName; }, ct);
    public Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default) => Run(serial, device => { if (!device.Apps.Contains(packageName)) return false; device.Count = 0; device.Text = "DEMO INPUT"; return true; }, ct);
    public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default) => Run<IReadOnlyList<AppInfo>>(serial, device => device.Apps.Where(package => showSystem || package != "com.android.settings").Select(package => new AppInfo(package, package, needVersion ? "1.0-demo" : null, package == "com.android.settings")).ToArray(), ct);
    public Task InstallAsync(string serial, string localPath, CancellationToken ct = default) => Run(serial, device => { if (!File.Exists(localPath)) throw new DeviceException($"APK file does not exist: {localPath}.", 400, 1007); device.Apps.Add("com.matrix.demo.imported"); }, ct);
    public Task UninstallAsync(string serial, string packageName, CancellationToken ct = default) => Run(serial, device => { if (!device.Apps.Remove(packageName)) throw new DeviceException($"Demo package not installed: {packageName}.", 404, 1002); }, ct);
    public Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default) => _queue.RunAsync(serial, async token =>
    { var bytes = await File.ReadAllBytesAsync(localPath, token); var device = Get(serial); lock (device) device.Files[remotePath] = bytes; progress?.Report(1); }, ct);
    public Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default) => _queue.RunAsync(serial, async token =>
    { var device = Get(serial); byte[] bytes; lock (device) bytes = device.Files.TryGetValue(remotePath, out var file) ? file.ToArray() : throw new DeviceException($"Demo device file not found: {remotePath}.", 404, 1002); await File.WriteAllBytesAsync(localPath, bytes, token); progress?.Report(1); }, ct);
    public Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default) => Run(serial, device =>
    {
        if (selector.ResourceId is not null && selector.ResourceId != "com.matrix.demo:id/input") throw new DeviceException("Demo set-text target is not an editable node.", 409, 1003);
        if (selector.ClassName is not null && selector.ClassName != "android.widget.EditText") throw new DeviceException("Demo set-text target is not an editable node.", 409, 1003);
        if (selector.Text is not null && selector.Text != device.Text) throw new DeviceException("Demo editable node does not match selector.", 404, 1002);
        var editable = new AndroidNode { Text = device.Text, ResourceId = "com.matrix.demo:id/input", ClassName = "android.widget.EditText", PackageName = device.ActiveApp, Clickable = true, Enabled = true, Focusable = true, Focused = true, Index = 2 };
        foreach (var property in typeof(NodeSelector).GetProperties())
        {
            var expected = property.GetValue(selector);
            if (expected is null) continue;
            var matches = property.Name == nameof(NodeSelector.TextContains)
                ? editable.Text?.Contains((string)expected, StringComparison.Ordinal) == true
                : Equals(typeof(AndroidNode).GetProperty(property.Name)!.GetValue(editable), expected);
            if (!matches) throw new DeviceException("Demo editable node does not match selector: " + property.Name, 404, 1002);
        }
        device.Text = text;
    }, ct);
    private static void CheckPoint(DeviceState device, int x, int y) { if (x < 0 || y < 0 || x >= device.Info.Width || y >= device.Info.Height) throw new DeviceException($"Demo point ({x},{y}) outside {device.Info.Width}x{device.Info.Height}.", 400, 1007); }
    private static readonly Dictionary<char, string> Glyphs = new()
    {
        ['A']="01110100011000111111100011000110001",['B']="11110100011000111110100011000111110",['C']="01111100001000010000100001000001111",['D']="11110100011000110001100011000111110",['E']="11111100001000011110100001000011111",['F']="11111100001000011110100001000010000",['G']="01111100001000010111100011000101111",['H']="10001100011000111111100011000110001",['I']="11111001000010000100001000010011111",['J']="00111000100001000010100101001001100",['K']="10001100101010011000101001001010001",['L']="10000100001000010000100001000011111",['M']="10001110111010110101100011000110001",['N']="10001110011010110011100011000110001",['O']="01110100011000110001100011000101110",['P']="11110100011000111110100001000010000",['Q']="01110100011000110001101011001001101",['R']="11110100011000111110101001001010001",['S']="01111100001000001110000010000111110",['T']="11111001000010000100001000010000100",['U']="10001100011000110001100011000101110",['V']="10001100011000110001100010101000100",['W']="10001100011000110101101011101110001",['X']="10001100010101000100010101000110001",['Y']="10001100010101000100001000010000100",['Z']="11111000010001000100010001000011111",['0']="01110100011001110101110011000101110",['1']="00100011000010000100001000010001110",['2']="01110100010000100010001000100011111",['3']="11110000010000101110000010000111110",['4']="00010001100101010010111110001000010",['5']="11111100001000011110000010000111110",['6']="01110100001000011110100011000101110",['7']="11111000010001000100010000100001000",['8']="01110100011000101110100011000101110",['9']="01110100011000101111000010000101110",['-']="00000000000000011111000000000000000",[' ']="00000000000000000000000000000000000"
    };
    private static bool GlyphPixel(string text, int x, int y, int scale)
    {
        if (x < 0 || y < 0 || y >= 7 * scale) return false;
        var character = x / (6 * scale); if (character >= text.Length || x % (6 * scale) >= 5 * scale) return false;
        if (!Glyphs.TryGetValue(text[character], out var pattern)) return false;
        return pattern[(y / scale) * 5 + x % (6 * scale) / scale] == '1';
    }
    public ValueTask DisposeAsync() => _queue.DisposeAsync();
}

```

## src/MatrixDevice.Core/Devices/DeviceCatalog.cs

```csharp
namespace MatrixDevice.Core;

public sealed class DeviceCatalog(IDeviceBackend backend)
{
    private readonly SemaphoreSlim _refresh = new(1, 1);
    private readonly object _gate = new();
    private IReadOnlyList<DeviceInfo> _snapshot = Array.Empty<DeviceInfo>();
    public event Action<string>? DeviceDisconnected;
    public IReadOnlyList<DeviceInfo> Snapshot { get { lock (_gate) return _snapshot; } }

    public DeviceInfo GetRequired(string serial)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serial);
        DeviceInfo? device;
        lock (_gate) device = _snapshot.FirstOrDefault(item => item.Serial == serial);
        if (device is null) throw new DeviceException($"Device not found: serial={serial}", 404, 1002);
        if (!device.Online) throw new DeviceException($"Device unavailable: serial={serial}, state={device.State}", 410, 1003);
        return device;
    }

    public async Task<IReadOnlyList<DeviceInfo>> RefreshAsync(CancellationToken ct = default)
    {
        await _refresh.WaitAsync(ct).ConfigureAwait(false);
        var disconnected = new List<string>();
        try
        {
            var discovered = await backend.DiscoverAsync(ct).ConfigureAwait(false);
            if (discovered.GroupBy(item => item.Serial, StringComparer.Ordinal).Any(group => group.Count() > 1))
                throw new DeviceException("Discovery returned duplicate serial numbers.", 503, 2000);
            lock (_gate)
            {
                var old = _snapshot.ToDictionary(item => item.Serial, StringComparer.Ordinal);
                var next = new List<DeviceInfo>();
                foreach (var item in discovered)
                {
                    ArgumentException.ThrowIfNullOrWhiteSpace(item.Serial);
                    var generation = 1L;
                    if (old.Remove(item.Serial, out var previous))
                    {
                        generation = previous.Generation;
                        if (previous.Online != item.Online || previous.Width != item.Width || previous.Height != item.Height || previous.Rotation != item.Rotation || previous.TransportId != item.TransportId) generation++;
                        if (previous.Online && (!item.Online || previous.TransportId != item.TransportId)) disconnected.Add(item.Serial);
                    }
                    next.Add(item with { Generation = generation });
                }
                foreach (var previous in old.Values)
                {
                    if (previous.Online) disconnected.Add(previous.Serial);
                    next.Add(previous with
                    {
                        State = "Offline", Error = "Device absent from the latest ADB discovery.",
                        Generation = previous.Generation + (previous.Online ? 1 : 0)
                    });
                }
                _snapshot = next.AsReadOnly();
            }
        }
        finally { _refresh.Release(); }
        foreach (var serial in disconnected) DeviceDisconnected?.Invoke(serial);
        return Snapshot;
    }
}

```

## src/MatrixDevice.Core/Devices/ExistingAdbSocket.cs

```csharp
using System.Net;
using System.Net.Sockets;
using System.Buffers.Binary;
using System.Text;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;

namespace MatrixDevice.Core;

// The library's single-send methods permit short writes. Complete each owned write before returning.
internal sealed class ExistingAdbSocket(TcpSocket tcp) : AdbSocket(tcp), IAdbSocket
{
    internal System.Net.Sockets.Socket ConnectedSocket => tcp.Socket;
    public static async Task<ExistingAdbSocket> OpenAsync(IPEndPoint endpoint, CancellationToken ct)
    {
        var tcp = new TcpSocket();
        try
        {
            await tcp.ConnectAsync(endpoint, ct).ConfigureAwait(false);
            return new ExistingAdbSocket(tcp);
        }
        catch { tcp.Dispose(); throw; }
    }

    public override void Send(ReadOnlySpan<byte> data)
    {
        while (!data.IsEmpty)
        {
            var written = tcp.Socket.Send(data, SocketFlags.None);
            if (written <= 0) throw new IOException("ADB socket closed during send.");
            data = data[written..];
        }
    }
    public override void Send(byte[] data) => Send(data.AsSpan());
    public override void Send(byte[] data, int length) => Send(data, 0, length);
    public override void Send(byte[] data, int offset, int length) => Send(data.AsSpan(offset, length == -1 ? data.Length - offset : length));
    public override async ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        while (!data.IsEmpty)
        {
            var written = await tcp.Socket.SendAsync(data, SocketFlags.None, ct).ConfigureAwait(false);
            if (written <= 0) throw new IOException("ADB socket closed during send.");
            data = data[written..];
        }
    }
    public override Task SendAsync(byte[] data, CancellationToken ct = default) => SendAsync(data.AsMemory(), ct).AsTask();
    public override Task SendAsync(byte[] data, int length, CancellationToken ct = default) => SendAsync(data, 0, length, ct);
    public override Task SendAsync(byte[] data, int offset, int length, CancellationToken ct = default) => SendAsync(data.AsMemory(offset, length == -1 ? data.Length - offset : length), ct).AsTask();

    public override int Read(Span<byte> data)
    {
        var length = data.Length;
        while (!data.IsEmpty)
        {
            var count = tcp.Socket.Receive(data, SocketFlags.None);
            if (count == 0) throw new EndOfStreamException("Truncated ADB response.");
            data = data[count..];
        }
        return length;
    }
    public override int Read(byte[] data) => Read(data.AsSpan());
    public override int Read(byte[] data, int length) => Read(data, 0, length);
    public override int Read(byte[] data, int offset, int length) => Read(data.AsSpan(offset, length == -1 ? data.Length - offset : length));
    public override async ValueTask<int> ReadAsync(Memory<byte> data, CancellationToken ct = default)
    {
        var length = data.Length;
        while (!data.IsEmpty)
        {
            var count = await tcp.Socket.ReceiveAsync(data, SocketFlags.None, ct).ConfigureAwait(false);
            if (count == 0) throw new EndOfStreamException("Truncated ADB response.");
            data = data[count..];
        }
        return length;
    }
    public override Task<int> ReadAsync(byte[] data, CancellationToken ct = default) => ReadAsync(data.AsMemory(), ct).AsTask();
    public override Task<int> ReadAsync(byte[] data, int length, CancellationToken ct = default) => ReadAsync(data, 0, length, ct);
    public override Task<int> ReadAsync(byte[] data, int offset, int length, CancellationToken ct = default) => ReadAsync(data.AsMemory(offset, length == -1 ? data.Length - offset : length), ct).AsTask();
    private static int SyncLength(byte[] bytes)
    {
        var length = BinaryPrimitives.ReadInt32LittleEndian(bytes);
        if (length < 0) throw new IOException($"Invalid ADB sync string length: {length}.");
        if (length > AdbDeviceBackend.MaximumStderrBytes) throw new DeviceException($"ADB sync message too large: {length}.", 413, 1007);
        return length;
    }
    public override string ReadSyncString()
    {
        var prefix = new byte[4]; Read(prefix);
        var bytes = new byte[SyncLength(prefix)]; Read(bytes);
        return Encoding.UTF8.GetString(bytes);
    }
    public override async Task<string> ReadSyncStringAsync(CancellationToken ct = default)
    {
        var prefix = new byte[4]; await ReadAsync(prefix, ct).ConfigureAwait(false);
        var bytes = new byte[SyncLength(prefix)]; await ReadAsync(bytes, ct).ConfigureAwait(false);
        return Encoding.UTF8.GetString(bytes);
    }
    SyncCommand IAdbSocket.ReadSyncResponse()
    {
        var response = base.ReadSyncResponse();
        if (response is SyncCommand.DONE or SyncCommand.OKAY)
        {
            var trailer = new byte[4]; Read(trailer);
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer) != 0) throw new IOException("Malformed ADB sync completion.");
        }
        return response;
    }
    async Task<SyncCommand> IAdbSocket.ReadSyncResponseAsync(CancellationToken ct)
    {
        var response = await base.ReadSyncResponseAsync(ct).ConfigureAwait(false);
        if (response is SyncCommand.DONE or SyncCommand.OKAY)
        {
            var trailer = new byte[4]; await ReadAsync(trailer, ct).ConfigureAwait(false);
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer) != 0) throw new IOException("Malformed ADB sync completion.");
        }
        return response;
    }
}

```

## src/MatrixDevice.Core/Devices/ExistingAdbTransport.cs

```csharp
using System.Buffers.Binary;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Models;

namespace MatrixDevice.Core;

internal sealed class ExistingAdbTransport(AppSettings settings, IPEndPoint endpoint)
{
    private readonly HashSet<string> allowedSerials = settings.AdbAllowedSerials.ToHashSet(StringComparer.Ordinal);
    private readonly object transportGate = new();
    private readonly AdbServerEpoch serverEpoch = new();
    private Dictionary<string, uint> transports = new(StringComparer.Ordinal);
    private readonly AsyncLocal<uint?> pinnedTransport = new();
    public void InheritServerEpoch(ExistingAdbTransport source) => serverEpoch.Require(source.serverEpoch.Snapshot);
    public uint? CurrentTransport(string serial) { lock (transportGate) return transports.TryGetValue(serial, out var id) ? id : null; }
    public void ReplaceTransports(Dictionary<string, uint> current) { lock (transportGate) transports = current; }
    public void InvalidateTransport(string serial) { lock (transportGate) transports.Remove(serial); }
    public void RequireTransport(string serial, uint? expected)
    {
        if (expected != CurrentTransport(serial)) throw new DeviceException($"ADB transport changed while queued: serial={serial}, expected={expected}, actual={CurrentTransport(serial)}.", 409, 1003);
    }
    public async Task<T> RunPinnedAsync<T>(string serial, uint? expected, Func<CancellationToken, Task<T>> action, CancellationToken ct)
    {
        RequireTransport(serial, expected);
        var previous = pinnedTransport.Value;
        pinnedTransport.Value = expected;
        try { return await action(ct).ConfigureAwait(false); }
        finally { pinnedTransport.Value = previous; }
    }
    public async Task<byte[]> ExecuteAsync(string? serial, string[] args, CancellationToken ct)
    {
        if (serial is not null && !allowedSerials.Contains(serial))
            throw new DeviceException($"ADB serial is not authorized: {serial}.", 403, 1007);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.CommandTimeoutSeconds, 1, 600)));
        try
        {
            var token = deadline.Token;
            await CheckVersionAsync(token).ConfigureAwait(false);
            using var socket = await OpenVerifiedAsync(token).ConfigureAwait(false);
            using var cancellation = token.Register(socket.Dispose);
            if (serial is null)
            {
                var request = args is ["devices", "-l"] ? "host:devices-l" :
                    args is ["connect", var address] && allowedSerials.Contains(address) ? "host:connect:" + address :
                    throw new DeviceException($"Unsupported or unauthorized ADB host operation: {string.Join(' ', args)}.", 403, 1007);
                await socket.SendAdbRequestAsync(request, token).ConfigureAwait(false);
                await socket.ReadAdbResponseAsync(token).ConfigureAwait(false);
                var text = await socket.ReadStringAsync(token).ConfigureAwait(false);
                return Encoding.UTF8.GetBytes(args[0] == "devices" ? "List of devices attached\n" + text : text);
            }
            var transport = pinnedTransport.Value ?? CurrentTransport(serial) ?? throw new DeviceException($"ADB transport has not been discovered: {serial}.", 503, 2000);
            RequireTransport(serial, transport);
            var device = new DeviceData { Serial = serial, TransportId = transport.ToString(CultureInfo.InvariantCulture) };
            if (args[0] is "push" or "pull")
            {
                // SyncService opens synchronously; cancellation closes only this connection, never the server.
                return await Task.Run(async () =>
                {
                    using var sync = new SyncService(socket, device);
                    if (args[0] == "push")
                    {
                        using var file = File.OpenRead(args[1]);
                        await sync.PushAsync(file, args[2], UnixFileStatus.DefaultFileMode, DateTimeOffset.UtcNow, null, false, token).ConfigureAwait(false);
                    }
                    else
                    {
                        var part = args[2] + ".matrix-part-" + Guid.NewGuid().ToString("N");
                        Exception? failure = null;
                        try
                        {
                            var expected = await sync.StatAsync(args[1], token).ConfigureAwait(false);
                            using (var file = File.Create(part))
                            {
                                await sync.PullAsync(args[1], file, null, false, token).ConfigureAwait(false);
                                if (file.Length != expected.Size) throw new DeviceException($"ADB pull byte count mismatch: expected={expected.Size}, actual={file.Length}, remote={args[1]}.", 502, 2000);
                            }
                            var after = await sync.StatAsync(args[1], token).ConfigureAwait(false);
                            if (after.Size != expected.Size || after.Time != expected.Time) throw new DeviceException($"Remote file changed during download: {args[1]}; local file left untouched.", 409, 1003);
                            token.ThrowIfCancellationRequested();
                            File.Move(part, args[2], overwrite: true);
                        }
                        catch (Exception ex) { failure = ex; throw; }
                        finally
                        {
                            try { if (File.Exists(part)) File.Delete(part); }
                            catch (Exception ex) when (failure is not null && ex is IOException or UnauthorizedAccessException) { failure.Data["PartialFileCleanupError"] = $"{part}: {ex.Message}"; }
                        }
                    }
                    return Array.Empty<byte>();
                }, token).ConfigureAwait(false);
            }
            await socket.SetDeviceAsync(device, token).ConfigureAwait(false);
            if (args[0] == "install")
            {
                using var apk = File.OpenRead(args[^1]);
                await socket.SendAdbRequestAsync("exec:cmd package install -r -S " + apk.Length.ToString(CultureInfo.InvariantCulture), token).ConfigureAwait(false);
                await socket.ReadAdbResponseAsync(token).ConfigureAwait(false);
                var block = new byte[64 * 1024];
                int count;
                while ((count = await apk.ReadAsync(block, token).ConfigureAwait(false)) > 0)
                    await socket.SendAsync(block, count, token).ConfigureAwait(false);
                using var stream = socket.Socket.GetStream();
                return await BoundedStreamRead.ReadAsync(stream, AdbDeviceBackend.MaximumStderrBytes, token).ConfigureAwait(false);
            }
            if (args[0] == "exec-out")
            {
                await socket.SendAdbRequestAsync("exec:" + string.Join(' ', args.Skip(1).Select(AdbDeviceBackend.Quote)), token).ConfigureAwait(false);
                await socket.ReadAdbResponseAsync(token).ConfigureAwait(false);
                // GetShellStream normalizes CRLF and would corrupt binary PNG data.
                using var stream = socket.Socket.GetStream();
                return await BoundedStreamRead.ReadAsync(stream, AdbDeviceBackend.MaximumStdoutBytes, token).ConfigureAwait(false);
            }
            var command = args[0] switch
            {
                "shell" => string.Join(' ', args.Skip(1)),
                "uninstall" => "pm uninstall " + AdbDeviceBackend.Quote(args[1]),
                _ => throw new DeviceException($"Unsupported ADB operation: {args[0]}.", 400, 1007)
            };
            await socket.SendAdbRequestAsync("shell,v2,raw:" + command, token).ConfigureAwait(false);
            await socket.ReadAdbResponseAsync(token).ConfigureAwait(false);
            await socket.SendAsync(new byte[] { 4, 0, 0, 0, 0 }, token).ConfigureAwait(false);
            using var outputStream = socket.Socket.GetStream();
            return await ReadShellAsync(outputStream, command, token).ConfigureAwait(false);
        }
        catch (Exception) when (deadline.IsCancellationRequested)
        {
            ct.ThrowIfCancellationRequested();
            throw new DeviceException($"ADB command timed out: serial={serial ?? "host"}, operation={args[0]}.", 504, 2000);
        }
        catch (AggregateException ex) when (ex.GetBaseException() is DeviceException)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.GetBaseException()).Throw();
            throw;
        }
        catch (Exception ex) when (ex.GetBaseException() is SocketException or IOException or AdbException or ObjectDisposedException)
        {
            throw new DeviceException($"Existing ADB server request failed: serial={serial ?? "host"}, operation={args[0]}: {ex.Message}", 503, 2000);
        }
    }

    private async Task CheckVersionAsync(CancellationToken ct)
    {
        using var socket = await OpenVerifiedAsync(ct).ConfigureAwait(false);
        using var cancellation = ct.Register(socket.Dispose);
        await socket.SendAdbRequestAsync("host:version", ct).ConfigureAwait(false);
        await socket.ReadAdbResponseAsync(ct).ConfigureAwait(false);
        var version = await socket.ReadStringAsync(ct).ConfigureAwait(false);
        if (!int.TryParse(version, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var number) || number != 41)
            throw new DeviceException($"Unsupported existing ADB server protocol: {version}; expected 0029. Server left untouched.", 503, 2000);
    }

    private async Task<ExistingAdbSocket> OpenVerifiedAsync(CancellationToken ct)
    {
        var socket = await ExistingAdbSocket.OpenAsync(endpoint, ct).ConfigureAwait(false);
        try
        {
            ct.ThrowIfCancellationRequested();
            serverEpoch.Require(AdbServerIdentity.ForConnection(socket.ConnectedSocket));
            return socket;
        }
        catch { socket.Dispose(); throw; }
    }

    private static async Task<byte[]> ReadShellAsync(Stream stream, string command, CancellationToken ct)
    {
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();
        var header = new byte[5];
        while (true)
        {
            await stream.ReadExactlyAsync(header, ct).ConfigureAwait(false);
            var length = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(1));
            if (header[0] is not (1 or 2 or 3) || (header[0] == 3 && length != 1)) throw new IOException($"Malformed ADB shell packet: channel={header[0]}, length={length}.");
            var limit = header[0] == 2 ? AdbDeviceBackend.MaximumStderrBytes : AdbDeviceBackend.MaximumStdoutBytes;
            if (length > limit) throw new DeviceException($"ADB shell output too large: {command}.", 413, 1007);
            var payload = new byte[(int)length];
            await stream.ReadExactlyAsync(payload, ct).ConfigureAwait(false);
            if (header[0] == 3)
            {
                if (payload.Length != 1) throw new IOException("Malformed ADB shell exit packet.");
                if (payload[0] != 0) throw new DeviceException($"ADB shell failed: exit={payload[0]}, command={command}, stderr={Encoding.UTF8.GetString(stderr.ToArray())}", 503, 2000);
                return stdout.ToArray();
            }
            var target = header[0] switch { 1 => stdout, 2 => stderr, _ => throw new IOException($"Unexpected ADB shell channel: {header[0]}.") };
            if (target.Length + payload.Length > limit) throw new DeviceException($"ADB shell output too large: {command}.", 413, 1007);
            await target.WriteAsync(payload, ct).ConfigureAwait(false);
        }
    }
}

```

## src/MatrixDevice.Core/Devices/NetworkDeviceScanner.cs

```csharp
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace MatrixDevice.Core;

public sealed record NetworkScanProgress(int Completed, int Total, int Found, int Connected, string CurrentAddress, string? Error = null);
public sealed record NetworkScanEndpoint(string Address, int Port, bool Found, bool Connected, string? Error = null);
public sealed record NetworkScanResult(int Total, int Found, int Connected, IReadOnlyList<NetworkScanEndpoint> Endpoints);

public sealed class NetworkDeviceScanner(AdbDeviceBackend backend)
{
    public async Task<NetworkScanResult> ScanAsync(string startIp, string endIp, int port, int concurrency = 16, int probeTimeoutMs = 1500, IProgress<NetworkScanProgress>? progress = null, CancellationToken ct = default)
    {
        var start = ParseAddress(startIp); var end = ParseAddress(endIp);
        var count = (long)end - start + 1;
        if (count is < 1 or > 1024) throw new DeviceException($"IPv4 scan must contain 1..1024 endpoints: start={startIp}, end={endIp}, count={count}.", 400, 1007);
        if (port is < 1 or > 65535 || concurrency is < 1 or > 16 || probeTimeoutMs is < 100 or > 30000)
            throw new DeviceException($"Invalid scan options: port={port}, concurrency={concurrency}, timeoutMs={probeTimeoutMs}.", 400, 1007);
        var total = (int)count; var completed = 0; var foundCount = 0; var connectedCount = 0;
        var endpoints = new ConcurrentDictionary<uint, NetworkScanEndpoint>();
        await Parallel.ForEachAsync(Enumerable.Range(0, total), new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = concurrency }, async (index, token) =>
        {
            var numeric = start + (uint)index;
            var bytes = new byte[4]; BinaryPrimitives.WriteUInt32BigEndian(bytes, numeric);
            var address = new IPAddress(bytes).ToString();
            var found = false; var connected = false; string? error = null;
            try
            {
                using (var probe = new TcpClient(AddressFamily.InterNetwork))
                using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(token))
                {
                    timeout.CancelAfter(probeTimeoutMs);
                    await probe.ConnectAsync(IPAddress.Parse(address), port, timeout.Token).ConfigureAwait(false);
                    found = true; Interlocked.Increment(ref foundCount);
                }
                await backend.ConnectNetworkAsync(address, port, token).ConfigureAwait(false);
                connected = true; Interlocked.Increment(ref connectedCount);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
            catch (OperationCanceledException) { error = $"TCP probe timed out after {probeTimeoutMs} ms."; }
            catch (Exception ex) when (ex is SocketException or DeviceException or IOException) { error = ex.Message; }
            finally
            {
                endpoints[numeric] = new(address, port, found, connected, error);
                var done = Interlocked.Increment(ref completed);
                progress?.Report(new(done, total, Volatile.Read(ref foundCount), Volatile.Read(ref connectedCount), address, error));
            }
        }).ConfigureAwait(false);
        return new(total, foundCount, connectedCount, endpoints.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToArray());
    }
    private static uint ParseAddress(string value)
    {
        if (!IPAddress.TryParse(value, out var address) || address.AddressFamily != AddressFamily.InterNetwork)
            throw new DeviceException($"Invalid IPv4 address: {value}.", 400, 1007);
        return BinaryPrimitives.ReadUInt32BigEndian(address.GetAddressBytes());
    }
}

```

## src/MatrixDevice.Core/Devices/PngImage.cs

```csharp
using SkiaSharp;

namespace MatrixDevice.Core;

public static class PngImage
{
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];
    public static (int Width, int Height) ReadDimensions(ReadOnlySpan<byte> png)
    {
        using var image = Decode(png);
        return (image.Width, image.Height);
    }
    public static byte[] Create(int width, int height, Func<int, int, (byte R, byte G, byte B)> pixel)
    {
        ValidateSize(width, height);
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        for (var y = 0; y < height; y++) for (var x = 0; x < width; x++)
        { var color = pixel(x, y); bitmap.SetPixel(x, y, new SKColor(color.R, color.G, color.B)); }
        return Encode(bitmap);
    }
    public static byte[] Crop(ReadOnlySpan<byte> png, int x, int y, int width, int height)
    {
        using var source = Decode(png);
        if (x < 0 || y < 0 || width <= 0 || height <= 0 || (long)x + width > source.Width || (long)y + height > source.Height)
            throw new InvalidDataException($"Crop ({x},{y},{width},{height}) is outside PNG {source.Width}x{source.Height}.");
        ValidateSize(width, height);
        using var cropped = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var sourceImage = SKImage.FromBitmap(source);
        using (var canvas = new SKCanvas(cropped)) canvas.DrawImage(sourceImage, new SKRect(x, y, x + width, y + height), new SKRect(0, 0, width, height), new SKSamplingOptions(SKFilterMode.Nearest));
        return Encode(cropped);
    }
    public static (byte R, byte G, byte B) ColorAt(ReadOnlySpan<byte> png, int x, int y)
    {
        using var image = Decode(png);
        if (x < 0 || x >= image.Width || y < 0 || y >= image.Height) throw new ArgumentOutOfRangeException(nameof(x));
        var color = image.GetPixel(x, y); return (color.Red, color.Green, color.Blue);
    }
    private static SKBitmap Decode(ReadOnlySpan<byte> png)
    {
        if (png.Length < 33 || !png[..8].SequenceEqual(Signature)) throw new InvalidDataException("Screenshot is not a PNG image.");
        using var data = SKData.CreateCopy(png.ToArray());
        using var codec = SKCodec.Create(data) ?? throw new InvalidDataException("PNG decoder could not open screenshot.");
        ValidateSize(codec.Info.Width, codec.Info.Height);
        var bitmap = new SKBitmap(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels());
        if (result != SKCodecResult.Success) { bitmap.Dispose(); throw new InvalidDataException($"PNG decoding failed: {result}."); }
        return bitmap;
    }
    private static byte[] Encode(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var bytes = image.Encode(SKEncodedImageFormat.Png, 100) ?? throw new InvalidDataException("PNG encoding failed.");
        return bytes.ToArray();
    }
    private static void ValidateSize(int width, int height)
    { if (width <= 0 || height <= 0 || (long)width * height > 24_000_000) throw new InvalidDataException($"Unsupported PNG size: {width}x{height}."); }
}

```

## src/MatrixDevice.Core/Devices/SerialCommandQueue.cs

```csharp
using System.Collections.Concurrent;

namespace MatrixDevice.Core;

public sealed class SerialCommandQueue : IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, Lane> _lanes = new(StringComparer.Ordinal);
    private readonly CancellationTokenSource _lifetime = new();
    private int _disposed;

    private sealed class Lane
    {
        public object Gate { get; } = new();
        public Task Tail { get; set; } = Task.CompletedTask;
    }

    public async Task<T> RunAsync<T>(string serial, Func<CancellationToken, Task<T>> action, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serial);
        ArgumentNullException.ThrowIfNull(action);
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        var lane = _lanes.GetOrAdd(serial, _ => new Lane());
        Task previous;
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (lane.Gate)
        {
            previous = lane.Tail;
            lane.Tail = completed.Task;
        }
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _lifetime.Token);
        try
        {
            // Wait for predecessors even when cancelled, so successors cannot overtake them.
            await previous.ConfigureAwait(false);
            linked.Token.ThrowIfCancellationRequested();
            return await action(linked.Token).ConfigureAwait(false);
        }
        finally { completed.TrySetResult(); }
    }

    public Task RunAsync(string serial, Func<CancellationToken, Task> action, CancellationToken ct = default) =>
        RunAsync(serial, async token => { await action(token).ConfigureAwait(false); return true; }, ct);

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        _lifetime.Cancel();
        await Task.WhenAll(_lanes.Values.Select(lane => { lock (lane.Gate) return lane.Tail; })).ConfigureAwait(false);
        _lifetime.Dispose();
    }
}

```

## src/MatrixDevice.Core/Input/InputCoordinator.cs

```csharp
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MatrixDevice.Core;

public enum InputKind { Down, Move, Up, Cancel, Tap, Swipe, Key, Text }
public sealed record InputSnapshot
{
    public required string SourceSerial { get; init; }
    public InputKind Kind { get; init; }
    public IReadOnlyList<string> TargetSerials { get; init; } = Array.Empty<string>();
    public long Generation { get; init; }
    public long SourceGeneration { get; init; }
    public long GestureId { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int X2 { get; init; }
    public int Y2 { get; init; }
    public int SourceWidth { get; init; }
    public int SourceHeight { get; init; }
    public int DurationMs { get; init; } = 300;
    public int KeyCode { get; init; }
    public string Text { get; init; } = "";
    public bool IncludeSource { get; init; } = true;
}

public sealed class InputCoordinator : IAsyncDisposable
{
    private sealed record Target(string Serial, long Generation, int Width, int Height, int? Rotation);
    private sealed class Work(InputSnapshot snapshot, Target[] targets, CancellationToken cancellation)
    {
        public InputSnapshot Snapshot { get; } = snapshot;
        public Target[] Targets { get; } = targets;
        public CancellationToken Cancellation { get; } = cancellation;
        public List<TaskCompletionSource> Waiters { get; } = [];
    }
    private sealed record Gesture(string Source, long SourceGeneration, long Generation, long Id, long DeviceGeneration, int X, int Y, int LastX, int LastY, long Started);
    private readonly DeviceCatalog _catalog;
    private readonly IDeviceBackend _backend;
    private readonly object _gate = new();
    private readonly LinkedList<Work> _pending = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly CancellationTokenSource _lifetime = new();
    private readonly SerialCommandQueue _targets = new();
    private readonly ConcurrentDictionary<string, Gesture> _gestures = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<long, Task> _inflight = new();
    private readonly Task _worker;
    private long _generation = 1;
    private long _dispatchId;
    private string? _source;
    private bool _disposed;
    public long CurrentGeneration { get { lock (_gate) return _generation; } }
    public string InputMode => "ADB completed gestures; not real-time touch";
    public event Action<string, Exception>? TargetFailed;

    public InputCoordinator(DeviceCatalog catalog, IDeviceBackend backend)
    {
        _catalog = catalog; _backend = backend;
        _catalog.DeviceDisconnected += OnDisconnected;
        _worker = Task.Run(WorkerAsync);
    }

    public Task SendAsync(InputSnapshot snapshot, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ct.ThrowIfCancellationRequested();
        var source = _catalog.GetRequired(snapshot.SourceSerial);
        if (snapshot.SourceGeneration != 0 && snapshot.SourceGeneration != source.Generation)
            throw new DeviceException($"Source generation changed: {snapshot.SourceSerial}.");
        var serials = snapshot.TargetSerials.Where(serial => !string.Equals(serial, source.Serial, StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.Ordinal).ToList();
        if (snapshot.IncludeSource) serials.Insert(0, source.Serial);
        var targets = serials.Select(serial => _catalog.GetRequired(serial)).Select(info => new Target(info.Serial, info.Generation, info.Width, info.Height, info.Rotation)).ToArray();
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var resolved = snapshot with { Generation = snapshot.Generation == 0 ? _generation : snapshot.Generation, SourceGeneration = source.Generation, SourceWidth = snapshot.SourceWidth > 0 ? snapshot.SourceWidth : source.Width, SourceHeight = snapshot.SourceHeight > 0 ? snapshot.SourceHeight : source.Height, TargetSerials = serials.ToArray() };
            if (_source is not null && resolved.SourceSerial != _source && resolved.Kind is not (InputKind.Up or InputKind.Cancel))
                throw new DeviceException($"Input source changed: requested={resolved.SourceSerial}, current={_source}.");
            if (resolved.Generation != _generation && resolved.Kind is not (InputKind.Up or InputKind.Cancel))
                throw new DeviceException($"Input generation {resolved.Generation} is stale; current={_generation}.");
            var work = new Work(resolved, targets, ct); work.Waiters.Add(completed);
            if (_pending.Last is { } tail && CanReplaceMove(tail.Value, work))
            {
                work.Waiters.AddRange(tail.Value.Waiters);
                tail.Value = work;
            }
            else { _pending.AddLast(work); _signal.Release(); }
        }
        return completed.Task;
    }

    private static bool CanReplaceMove(Work previous, Work next) => previous.Snapshot.Kind == InputKind.Move && next.Snapshot.Kind == InputKind.Move
        && previous.Snapshot.Generation == next.Snapshot.Generation && previous.Snapshot.GestureId == next.Snapshot.GestureId
        && previous.Snapshot.SourceGeneration == next.Snapshot.SourceGeneration && previous.Snapshot.SourceSerial == next.Snapshot.SourceSerial
        && previous.Targets.SequenceEqual(next.Targets) && previous.Cancellation == next.Cancellation;

    private async Task WorkerAsync()
    {
        try
        {
            while (true)
            {
                await _signal.WaitAsync(_lifetime.Token).ConfigureAwait(false);
                Work work;
                lock (_gate) { work = _pending.First!.Value; _pending.RemoveFirst(); }
                var id = Interlocked.Increment(ref _dispatchId);
                var completion = DispatchAsync(work);
                _inflight[id] = completion;
                _ = RemoveCompletedAsync(id, completion);
            }
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        finally
        {
            lock (_gate) { foreach (var work in _pending) foreach (var waiter in work.Waiters) waiter.TrySetCanceled(); _pending.Clear(); }
        }
    }
    private async Task RemoveCompletedAsync(long id, Task task) { await task.ConfigureAwait(false); _inflight.TryRemove(id, out _); }
    private async Task DispatchAsync(Work work)
    {
        try
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(work.Cancellation, _lifetime.Token);
            linked.Token.ThrowIfCancellationRequested();
            var requests = work.Targets.Select(target => _targets.RunAsync(target.Serial, async token =>
            {
                try { await SendTargetAsync(work.Snapshot, target, token).ConfigureAwait(false); }
                catch (Exception error) { TargetFailed?.Invoke(target.Serial, error); throw; }
            }, linked.Token));
            await Task.WhenAll(requests).ConfigureAwait(false);
            foreach (var waiter in work.Waiters) waiter.TrySetResult();
        }
        catch (OperationCanceledException) { foreach (var waiter in work.Waiters) waiter.TrySetCanceled(); }
        catch (Exception error) { foreach (var waiter in work.Waiters) waiter.TrySetException(error); }
    }

    private async Task SendTargetAsync(InputSnapshot snapshot, Target target, CancellationToken ct)
    {
        var current = _catalog.GetRequired(target.Serial);
        if (current.Generation != target.Generation || current.Width != target.Width || current.Height != target.Height || current.Rotation != target.Rotation)
        { _gestures.TryRemove(target.Serial, out _); throw new DeviceException($"Target generation or dimensions changed: serial={target.Serial}."); }
        var stale = snapshot.Generation != CurrentGeneration;
        if (stale)
        {
            // Completed-gesture ADB mode has no outstanding injected DOWN to release.
            if (_gestures.TryGetValue(target.Serial, out var old) && Matches(old, snapshot, target)) _gestures.TryRemove(target.Serial, out _);
            return;
        }
        if (snapshot.Kind != InputKind.Cancel)
        {
            var source = _catalog.GetRequired(snapshot.SourceSerial);
            if (source.Generation != snapshot.SourceGeneration) { _gestures.TryRemove(target.Serial, out _); throw new DeviceException($"Source reconnected: serial={source.Serial}."); }
        }
        (int X, int Y) Point(int x, int y) => ScalePoint(x, y, snapshot.SourceWidth, snapshot.SourceHeight, target.Width, target.Height);
        switch (snapshot.Kind)
        {
            case InputKind.Down:
                var down = Point(snapshot.X, snapshot.Y);
                _gestures[target.Serial] = new(snapshot.SourceSerial, snapshot.SourceGeneration, snapshot.Generation, snapshot.GestureId, target.Generation, down.X, down.Y, down.X, down.Y, Stopwatch.GetTimestamp());
                break;
            case InputKind.Move:
                if (_gestures.TryGetValue(target.Serial, out var move) && Matches(move, snapshot, target))
                { var next = Point(snapshot.X, snapshot.Y); _gestures[target.Serial] = move with { LastX = next.X, LastY = next.Y }; }
                break;
            case InputKind.Up:
                if (_gestures.TryGetValue(target.Serial, out var gesture) && Matches(gesture, snapshot, target))
                {
                    _gestures.TryRemove(target.Serial, out _);
                    var end = Point(snapshot.X, snapshot.Y);
                    var duration = Math.Clamp((int)Stopwatch.GetElapsedTime(gesture.Started).TotalMilliseconds, 30, 60000);
                    if (Math.Abs(gesture.X - end.X) <= 2 && Math.Abs(gesture.Y - end.Y) <= 2 && duration < 500)
                        await _backend.TapAsync(target.Serial, end.X, end.Y, ct).ConfigureAwait(false);
                    else await _backend.SwipeAsync(target.Serial, gesture.X, gesture.Y, end.X, end.Y, duration, ct).ConfigureAwait(false);
                }
                break;
            case InputKind.Cancel:
                if (_gestures.TryGetValue(target.Serial, out var cancel) && Matches(cancel, snapshot, target)) _gestures.TryRemove(target.Serial, out _);
                break;
            case InputKind.Tap:
                var tap = Point(snapshot.X, snapshot.Y); await _backend.TapAsync(target.Serial, tap.X, tap.Y, ct).ConfigureAwait(false); break;
            case InputKind.Swipe:
                var start = Point(snapshot.X, snapshot.Y); var stop = Point(snapshot.X2, snapshot.Y2);
                await _backend.SwipeAsync(target.Serial, start.X, start.Y, stop.X, stop.Y, snapshot.DurationMs, ct).ConfigureAwait(false); break;
            case InputKind.Key: await _backend.KeyEventAsync(target.Serial, snapshot.KeyCode, ct).ConfigureAwait(false); break;
            case InputKind.Text: await _backend.TextAsync(target.Serial, snapshot.Text, ct).ConfigureAwait(false); break;
            default: throw new DeviceException($"Unknown input action: {snapshot.Kind}.", 400, 1007);
        }
    }
    private static bool Matches(Gesture gesture, InputSnapshot snapshot, Target target) => gesture.Source == snapshot.SourceSerial && gesture.SourceGeneration == snapshot.SourceGeneration && gesture.Generation == snapshot.Generation && gesture.Id == snapshot.GestureId && gesture.DeviceGeneration == target.Generation;
    public static (int X, int Y) ScalePoint(int x, int y, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
    {
        if (sourceWidth <= 0 || sourceHeight <= 0 || targetWidth <= 0 || targetHeight <= 0) throw new DeviceException($"Input dimensions are unavailable: source={sourceWidth}x{sourceHeight}, target={targetWidth}x{targetHeight}.", 409, 1003);
        if (x < 0 || y < 0 || x >= sourceWidth || y >= sourceHeight) throw new DeviceException($"Source point ({x},{y}) is outside {sourceWidth}x{sourceHeight}.", 400, 1007);
        return (Math.Clamp((int)((long)x * targetWidth / sourceWidth), 0, targetWidth - 1), Math.Clamp((int)((long)y * targetHeight / sourceHeight), 0, targetHeight - 1));
    }

    public async Task<long> HandoffAsync(string? sourceSerial, CancellationToken ct = default)
    {
        if (sourceSerial is not null) _catalog.GetRequired(sourceSerial);
        long generation;
        lock (_gate) { ObjectDisposedException.ThrowIf(_disposed, this); generation = ++_generation; _source = sourceSerial; }
        await ReleaseGesturesAsync(generation, ct).ConfigureAwait(false);
        return generation;
    }
    public async Task ReleaseAsync(CancellationToken ct = default)
    {
        long generation;
        lock (_gate) { ObjectDisposedException.ThrowIf(_disposed, this); generation = ++_generation; }
        await ReleaseGesturesAsync(generation, ct).ConfigureAwait(false);
    }
    private async Task ReleaseGesturesAsync(long generation, CancellationToken ct)
    {
        var devices = _catalog.Snapshot.Select(device => device.Serial).Concat(_gestures.Keys).Distinct(StringComparer.Ordinal).ToArray();
        await Task.WhenAll(devices.Select(serial => _targets.RunAsync(serial, token =>
        {
            if (_gestures.TryGetValue(serial, out var gesture) && gesture.Generation < generation)
                _gestures.TryRemove(new KeyValuePair<string, Gesture>(serial, gesture));
            return Task.CompletedTask;
        }, ct))).ConfigureAwait(false);
    }
    private void OnDisconnected(string serial)
    {
        foreach (var pair in _gestures.Where(pair => pair.Key == serial || pair.Value.Source == serial))
            _gestures.TryRemove(new KeyValuePair<string, Gesture>(pair.Key, pair.Value));
    }
    public async ValueTask DisposeAsync()
    {
        lock (_gate) { if (_disposed) return; _disposed = true; ++_generation; }
        _catalog.DeviceDisconnected -= OnDisconnected;
        _lifetime.Cancel();
        await _worker.ConfigureAwait(false);
        await Task.WhenAll(_inflight.Values).ConfigureAwait(false);
        await _targets.DisposeAsync().ConfigureAwait(false);
        _gestures.Clear(); _signal.Dispose(); _lifetime.Dispose();
    }
}

```

## src/MatrixDevice.Core/IO/BoundedStreamRead.cs

```csharp
using System.Buffers;

namespace MatrixDevice.Core;

public static class BoundedStreamRead
{
    public static async Task<byte[]> ReadAsync(Stream stream, int maximumBytes, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (maximumBytes < 0) throw new ArgumentOutOfRangeException(nameof(maximumBytes));
        var buffer = ArrayPool<byte>.Shared.Rent(65536);
        try
        {
            using var output = new MemoryStream(Math.Min(maximumBytes, buffer.Length));
            while (true)
            {
                var count = await stream.ReadAsync(buffer.AsMemory(), ct).ConfigureAwait(false);
                if (count == 0) return output.ToArray();
                if (output.Length + count > maximumBytes) throw new DeviceException($"Response exceeds {maximumBytes} bytes: received at least {output.Length + count}.", 413, 1007);
                output.Write(buffer, 0, count);
            }
        }
        finally { ArrayPool<byte>.Shared.Return(buffer); }
    }
}

```

## src/MatrixDevice.Core/LdPlayer/DemoLdConsoleRunner.cs

```csharp
using System.Globalization;

namespace MatrixDevice.Core;

public sealed class DemoLdConsoleRunner : ILdConsoleRunner
{
    private readonly object _gate = new();
    private readonly Dictionary<int, LdPlayerInstance> _instances = new()
    {
        [0] = new(0, "雷电模拟器（模拟）", 10001, 10002, true, 20001, 20002, 1600, 900, 240),
        [1] = new(1, "雷电模拟器-1（模拟）", 0, 0, false, -1, -1, 1280, 720, 240)
    };
    public IReadOnlyList<string>? LastArguments { get; private set; }
    public void RemoveInstance(int index) { lock (_gate) _instances.Remove(index); }
    public void AddInstance(LdPlayerInstance instance)
    {
        if (instance.Index < 0 || instance.Index == 99999 || instance.Name.IndexOfAny(['\0', '\r', '\n']) >= 0) throw new ArgumentException("模拟实例索引或名称不合法。");
        lock (_gate) _instances.Add(instance.Index, instance);
    }
    public Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (arguments.Count == 0) throw new DeviceException("模拟 CLI 参数为空。", 400, 1007);
        lock (_gate)
        {
            LastArguments = arguments.ToArray();
            if (arguments[0] == "list2") return Task.FromResult(string.Join('\n', _instances.Values.OrderBy(instance => instance.Index).Select(instance =>
            {
                var fields = new List<object?> { instance.Index, instance.Name, instance.TopWindow, instance.BindWindow, instance.AndroidStarted ? 1 : 0, instance.ProcessId, instance.VBoxProcessId };
                if (instance.Width is not null || instance.Height is not null || instance.Dpi is not null) fields.AddRange([instance.Width, instance.Height, instance.Dpi]);
                return string.Join(',', fields);
            })));
            if (arguments.Count < 3 || arguments[1] != "--index" || !int.TryParse(arguments[2], NumberStyles.None, CultureInfo.InvariantCulture, out var index) || !_instances.TryGetValue(index, out var instance))
                throw new DeviceException("模拟雷电实例不存在或参数不合法。", 404, 1002);
            switch (arguments[0])
            {
                case "launch": case "reboot": _instances[index] = instance with { AndroidStarted = true, TopWindow = 10001 + index * 10, BindWindow = 10002 + index * 10, ProcessId = 20001 + index * 10, VBoxProcessId = 20002 + index * 10 }; break;
                case "quit": _instances[index] = instance with { AndroidStarted = false, TopWindow = 0, BindWindow = 0, ProcessId = -1, VBoxProcessId = -1 }; break;
                case "runapp": case "killapp": case "installapp": case "action": break;
                default: throw new DeviceException("模拟 CLI 未开放命令：" + arguments[0], 501, 2001);
            }
            return Task.FromResult("模拟命令已返回；未连接真实模拟器。");
        }
    }
}

```

## src/MatrixDevice.Core/LdPlayer/LdConsoleRunner.cs

```csharp
using System.Diagnostics;
using System.Text;

namespace MatrixDevice.Core;

public sealed class LdConsoleRunner : ILdConsoleRunner
{
    public const int MaximumStdoutBytes = 32 * 1024 * 1024;
    public const int MaximumStderrBytes = 1024 * 1024;
    private readonly string _executable;
    private readonly TimeSpan _timeout;
    private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);
    private static readonly Encoding Gbk = CreateGbk();

    public LdConsoleRunner(string executable, int timeoutSeconds = 30)
    {
        if (string.IsNullOrWhiteSpace(executable) || !Path.IsPathFullyQualified(executable) || !File.Exists(executable))
            throw new DeviceException($"雷电 CLI 路径必须为已存在的完整 Windows 路径：{executable}", 400, 1007);
        if (timeoutSeconds is < 1 or > 600) throw new DeviceException($"雷电 CLI 超时必须为 1 至 600 秒：{timeoutSeconds}", 400, 1007);
        _executable = Path.GetFullPath(executable);
        _timeout = TimeSpan.FromSeconds(timeoutSeconds);
    }

    public async Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        if (arguments.Count == 0 || arguments.Any(argument => argument is null || argument.Contains('\0')))
            throw new DeviceException("雷电 CLI 参数不能为空，也不能包含 NUL。", 400, 1007);
        ct.ThrowIfCancellationRequested();
        var info = new ProcessStartInfo(_executable)
        {
            WorkingDirectory = Path.GetDirectoryName(_executable)!, UseShellExecute = false,
            CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        foreach (var argument in arguments) info.ArgumentList.Add(argument);
        using var process = new Process { StartInfo = info };
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(_timeout);
        try
        {
            if (!process.Start()) throw new DeviceException("无法启动雷电 CLI 子进程。", 503, 2000);
        }
        catch (System.ComponentModel.Win32Exception error)
        { throw new DeviceException($"无法启动雷电 CLI：{error.Message}", 503, 2000); }

        async Task<byte[]> ReadAsync(Stream stream, int maximumBytes)
        {
            try { return await BoundedStreamRead.ReadAsync(stream, maximumBytes, deadline.Token).ConfigureAwait(false); }
            catch { deadline.Cancel(); throw; }
        }
        var stdout = ReadAsync(process.StandardOutput.BaseStream, MaximumStdoutBytes);
        var stderr = ReadAsync(process.StandardError.BaseStream, MaximumStderrBytes);
        try
        {
            await Task.WhenAll(stdout, stderr, process.WaitForExitAsync(deadline.Token)).ConfigureAwait(false);
            var output = Decode(await stdout.ConfigureAwait(false));
            var error = Decode(await stderr.ConfigureAwait(false));
            if (process.ExitCode != 0)
                throw new DeviceException($"雷电 CLI 执行失败：命令={arguments[0]}，退出码={process.ExitCode}，信息={error.Trim()} {output.Trim()}", 503, 2000);
            return output;
        }
        catch (Exception original)
        {
            deadline.Cancel();
            try
            {
                // Launch/reboot descendants may be the emulator, not disposable CLI workers.
                if (!process.HasExited) process.Kill(entireProcessTree: false);
                await process.WaitForExitAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }
            catch (Exception cleanup) when (cleanup is InvalidOperationException or System.ComponentModel.Win32Exception or TimeoutException) { }
            // Observe both readers before releasing their process streams.
            try { await Task.WhenAll(stdout, stderr).ConfigureAwait(false); } catch { }
            var limit = new[] { stdout.Exception, stderr.Exception }.OfType<AggregateException>()
                .SelectMany(error => error.Flatten().InnerExceptions).OfType<DeviceException>().FirstOrDefault(error => error.Status == 413);
            if (limit is not null) throw new DeviceException($"雷电 CLI 输出过大：命令={arguments[0]}；标准输出上限 32 MiB，错误输出上限 1 MiB。", 413, 1007);
            if (original is OperationCanceledException)
            {
                if (ct.IsCancellationRequested) throw;
                throw new DeviceException($"雷电 CLI 超时：命令={arguments[0]}，限制={_timeout.TotalSeconds} 秒。结果未知，不自动重试；仅尝试终止本次 CLI 进程，不关闭模拟器子进程。", 504, 2000);
            }
            throw;
        }
    }

    public static string Decode(ReadOnlySpan<byte> bytes)
    {
        try { return StrictUtf8.GetString(bytes).TrimStart('\ufeff'); }
        catch (DecoderFallbackException)
        {
            try { return Gbk.GetString(bytes); }
            catch (DecoderFallbackException error) { throw new DeviceException($"雷电 CLI 输出无法按 UTF-8 或 GBK 解码：{error.Message}", 502, 2000); }
        }
    }
    private static Encoding CreateGbk()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(936, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
    }
}

```

## src/MatrixDevice.Core/LdPlayer/LdInstanceEditor.cs

```csharp
using System.Globalization;

namespace MatrixDevice.Core;

public sealed record LdInstanceConfiguration(int Cpu, int MemoryMb, int Width, int Height, int Dpi);
public sealed record LdInstanceCreationResult(LdPlayerInstance Instance, bool CommandReportedFailure, string Message);

public sealed class LdInstanceEditor(ILdConsoleRunner runner, LdPlayerService service)
{
    public const int MaximumNameLength = 80;
    public const int MinimumScreenDimension = 240;
    public const int MaximumScreenDimension = 4096;
    public const int MinimumDpi = 72;
    public const int MaximumDpi = 640;
    private readonly ILdConsoleRunner runner = runner ?? throw new ArgumentNullException(nameof(runner));
    private readonly LdPlayerService service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly SemaphoreSlim edits = new(1, 1);

    public async Task<LdInstanceCreationResult> CreateAsync(string name, CancellationToken ct = default)
    {
        var hasControlCharacters = name?.Any(char.IsControl) == true;
        name = name?.Trim() ?? "";
        if (name.Length is 0 or > MaximumNameLength || name.StartsWith("--", StringComparison.Ordinal) || hasControlCharacters || name.Contains(','))
            throw new DeviceException($"实例名称必须为 1 至 {MaximumNameLength} 字符，不能含控制字符、逗号或选项前缀。", 400, 1007);
        await edits.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var before = await service.RefreshAsync(ct).ConfigureAwait(false);
            if (before.Any(instance => instance.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new DeviceException($"实例名称已经存在：{name}；未创建或复制任何实例。", 409, 1003);
            DeviceException? commandFailure = null;
            try { await runner.RunAsync(["add", "--name", name], ct).ConfigureAwait(false); }
            catch (DeviceException error) when (error.Status == 503) { commandFailure = error; }
            ct.ThrowIfCancellationRequested();
            IReadOnlyList<LdPlayerInstance> after;
            try { after = await service.RefreshAsync(ct).ConfigureAwait(false); }
            catch (DeviceException error)
            {
                throw new DeviceException($"创建命令之后无法核对目录：{error.Message}；结果未知，未再次创建，请手动刷新。" +
                    (commandFailure is null ? "" : $" 原命令错误：{commandFailure.Message}"), error.Status, error.Code);
            }
            var oldIndices = before.Select(instance => instance.Index).ToHashSet();
            var added = after.Where(instance => !oldIndices.Contains(instance.Index)).ToArray();
            if (added.Length != 1 || added[0].Index <= 0 || added[0].Index == 99999 || added[0].Name != name || before.Any(instance => !after.Any(current => current.Index == instance.Index)))
            {
                var observed = string.Join(", ", added.Select(instance => $"{instance.Index}:{instance.Name}"));
                throw new DeviceException($"无法唯一确认新建实例：名称={name}，新增目录=[{observed}]；未重试、复制或删除实例，请刷新目录检查。" +
                    (commandFailure is null ? "" : $" 原命令错误：{commandFailure.Message}"), 409, 1003);
            }
            var message = commandFailure is null
                ? "目录差集已确认唯一新实例；未自动启动、安装应用或核验 ADB。"
                : $"命令报告失败，但目录差集确认了唯一同名新实例；未重发 add，也未自动启动。原命令错误：{commandFailure.Message}";
            return new(added[0], commandFailure is not null, message);
        }
        finally { edits.Release(); }
    }

    public async Task<LdPlayerCommandResult> ConfigureAsync(int index, long expectedGeneration,
        LdInstanceConfiguration configuration, CancellationToken ct = default)
    {
        if (index <= 0 || index == 99999) throw new DeviceException($"资源配置不允许实例编号 {index}；原实例 0 与桌面编号受保护。", 400, 1007);
        if (expectedGeneration <= 0) throw new DeviceException("expectedGeneration 必须为正整数。", 400, 1007);
        Validate(configuration);
        await edits.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await service.RefreshAsync(ct).ConfigureAwait(false);
            var before = service.Instances.FirstOrDefault(instance => instance.Index == index)
                ?? throw new DeviceException($"雷电实例不存在：索引={index}", 404, 1002);
            if (before.Generation != expectedGeneration)
                throw new DeviceException($"实例代次已变化：索引={index}，请求={expectedGeneration}，当前={before.Generation}；未修改资源。", 409, 1003);
            if (before.IsStarting || before.AndroidStarted || before.ProcessId > 0 || before.VBoxProcessId > 0 || before.TopWindow != 0 || before.BindWindow != 0)
                throw new DeviceException($"实例 {index} 尚未完全停止；请确认播放器、虚拟机与窗口全部退出后配置。", 409, 1003);
            await runner.RunAsync(["modify", "--index", Number(index), "--cpu", Number(configuration.Cpu),
                "--memory", Number(configuration.MemoryMb), "--resolution",
                string.Join(',', Number(configuration.Width), Number(configuration.Height), Number(configuration.Dpi))], ct).ConfigureAwait(false);
            try { await service.RefreshAsync(ct).ConfigureAwait(false); }
            catch (DeviceException error)
            {
                return new(index, "modify", true, false, $"资源配置命令已返回，但目录复查失败：{error.Message}；结果待核验，不自动重试。", null);
            }
            var after = service.Instances.FirstOrDefault(instance => instance.Index == index);
            return new(index, "modify", true, false,
                "资源配置命令已返回；尚未核验实际 CPU、内存或 Android 分辨率，需要启动后另行验证。未请求修改型号、设备身份或 root。", after);
        }
        finally { edits.Release(); }
    }

    public static void Validate(LdInstanceConfiguration configuration)
    {
        if (configuration is null) throw new DeviceException("资源配置不能为空。", 400, 1007);
        if (configuration.Cpu is not (1 or 2 or 3 or 4)) throw new DeviceException($"CPU 核数必须为 1、2、3 或 4：{configuration.Cpu}", 400, 1007);
        if (configuration.MemoryMb is not (256 or 512 or 768 or 1024 or 1536 or 2048 or 4096 or 8192))
            throw new DeviceException($"内存值不在雷电官方枚举中：{configuration.MemoryMb} MiB", 400, 1007);
        if (configuration.Width is < MinimumScreenDimension or > MaximumScreenDimension || configuration.Height is < MinimumScreenDimension or > MaximumScreenDimension)
            throw new DeviceException($"宽高必须分别为 {MinimumScreenDimension} 至 {MaximumScreenDimension} 的正整数：{configuration.Width}x{configuration.Height}", 400, 1007);
        if (configuration.Dpi is < MinimumDpi or > MaximumDpi)
            throw new DeviceException($"DPI 必须为 {MinimumDpi} 至 {MaximumDpi} 的正整数：{configuration.Dpi}", 400, 1007);
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}

```

## src/MatrixDevice.Core/LdPlayer/LdLaunchCoordinator.cs

```csharp
namespace MatrixDevice.Core;

public sealed record LdLaunchProgress(int Index, string State, string Message, LdPlayerInstance? Instance = null);

public sealed class LdLaunchCoordinator
{
    private readonly LdPlayerService service;
    private readonly TimeSpan instanceTimeout;
    private readonly TimeSpan pollInterval;
    private readonly SemaphoreSlim batch = new(1, 1);

    public LdLaunchCoordinator(LdPlayerService service, TimeSpan? instanceTimeout = null, TimeSpan? pollInterval = null)
    {
        this.service = service ?? throw new ArgumentNullException(nameof(service));
        this.instanceTimeout = instanceTimeout ?? TimeSpan.FromMinutes(3);
        this.pollInterval = pollInterval ?? TimeSpan.FromSeconds(2);
        if (this.instanceTimeout <= TimeSpan.Zero || this.instanceTimeout > TimeSpan.FromMinutes(10))
            throw new ArgumentOutOfRangeException(nameof(instanceTimeout), "实例启动等待必须大于零且不超过十分钟。");
        if (this.pollInterval <= TimeSpan.Zero || this.pollInterval > this.instanceTimeout)
            throw new ArgumentOutOfRangeException(nameof(pollInterval), "轮询间隔必须大于零且不超过实例等待期限。");
    }

    public async Task<IReadOnlyList<LdLaunchProgress>> StartAsync(int[] indices,
        Func<CancellationToken, Task> ensureResourceReserve, IProgress<LdLaunchProgress>? progress = null,
        CancellationToken ct = default, IReadOnlyList<LdPlayerInstance>? expectedInstances = null)
    {
        if (indices is null || indices.Length == 0 || indices.Any(index => index <= 0 || index == 99999) || indices.Distinct().Count() != indices.Length)
            throw new DeviceException("启动批次必须包含不同的正整数实例编号，不允许原实例 0 或桌面编号 99999。", 400, 1007);
        ArgumentNullException.ThrowIfNull(ensureResourceReserve);
        var requested = indices.ToArray();
        var expected = expectedInstances?.ToArray();
        if (expected is not null && (expected.Length != requested.Length || expected.Any(instance => instance is null) ||
            !expected.Select(instance => instance.Index).ToHashSet().SetEquals(requested)))
            throw new DeviceException("确认快照必须完整且唯一地对应本次启动编号，不能缺少、重复或包含额外实例。", 400, 1007);
        var confirmed = expected?.ToDictionary(instance => instance.Index);
        await batch.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var results = new List<LdLaunchProgress>();
            foreach (var index in requested)
            {
                ct.ThrowIfCancellationRequested();
                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
                deadline.CancelAfter(instanceTimeout);
                var token = deadline.Token;
                try
                {
                    await service.RefreshAsync(token).ConfigureAwait(false);
                    var original = Require(index);
                    if (confirmed is not null && (original.Generation != confirmed[index].Generation || original.Name != confirmed[index].Name))
                        throw new DeviceException($"确认后实例身份或代次已变化：索引={index}；未提交启动命令，请刷新后重新确认。", 409, 1003);
                    if (IsReady(original))
                    {
                        var skipped = new LdLaunchProgress(index, "skipped", "实例已运行，未重复启动；尚未核验 ADB、设备授权或应用页面。", original);
                        progress?.Report(skipped); results.Add(skipped); continue;
                    }
                    if (IsStopped(original))
                    {
                        progress?.Report(new(index, "checking-resources", "正在检查本次启动的资源预留，不推测未知实例内存。", original));
                        await ensureResourceReserve(token).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        await service.RefreshAsync(token).ConfigureAwait(false);
                        var before = RequireSameIdentity(original);
                        if (before.Generation != original.Generation)
                            throw new DeviceException($"资源检查期间实例代次已变化：索引={index}，原代次={original.Generation}，当前={before.Generation}；未提交启动命令，请刷新后重新确认。", 409, 1003);
                        if (IsStopped(before))
                        {
                            progress?.Report(new(index, "starting", "正在提交一次启动命令；取消或超时不会关闭已启动实例。", before));
                            await service.ExecuteAsync(index, "launch", new System.Text.Json.Nodes.JsonObject
                            { ["expectedGeneration"] = before.Generation }, token).ConfigureAwait(false);
                        }
                    }
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        await service.RefreshAsync(token).ConfigureAwait(false);
                        var observed = RequireSameIdentity(original);
                        if (IsReady(observed))
                        {
                            var ready = new LdLaunchProgress(index, "ready", "雷电目录已确认安卓启动标志及播放器、虚拟机进程；尚未核验 ADB、设备授权或应用页面。", observed);
                            progress?.Report(ready); results.Add(ready); break;
                        }
                        progress?.Report(new(index, "waiting", "等待现有启动过程，不重复提交 launch；ADB 尚未核验。", observed));
                        await Task.Delay(pollInterval, token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    progress?.Report(new(index, "cancelled", "已取消本程序等待及后续启动；未关闭任何已启动实例。"));
                    throw;
                }
                catch (OperationCanceledException) when (deadline.IsCancellationRequested)
                {
                    var message = $"实例 {index} 在 {instanceTimeout.TotalSeconds:0.###} 秒内未确认启动完成；结果未知，不重发 launch，不启动后续实例，也不关闭已启动实例。";
                    progress?.Report(new(index, "failed", message));
                    throw new DeviceException(message, 504, 2000);
                }
                catch (Exception error)
                {
                    progress?.Report(new(index, "failed", $"实例启动批次已停止：{error.Message}；未关闭已启动实例。"));
                    throw;
                }
            }
            return results.AsReadOnly();
        }
        finally { batch.Release(); }
    }

    private LdPlayerInstance Require(int index) => service.Instances.FirstOrDefault(instance => instance.Index == index)
        ?? throw new DeviceException($"雷电实例不存在：索引={index}", 404, 1002);

    private LdPlayerInstance RequireSameIdentity(LdPlayerInstance original)
    {
        var current = Require(original.Index);
        if (current.Name != original.Name || current.Width != original.Width || current.Height != original.Height || current.Dpi != original.Dpi)
            throw new DeviceException($"等待期间实例身份或配置已变化：索引={original.Index}；停止当前批次，请刷新目录后重新确认。", 409, 1003);
        return current;
    }

    private static bool IsReady(LdPlayerInstance instance) => !instance.IsStarting && instance.AndroidStarted && instance.ProcessId > 0 && instance.VBoxProcessId > 0;
    private static bool IsStopped(LdPlayerInstance instance) => !instance.IsStarting && !instance.AndroidStarted && instance.ProcessId <= 0 && instance.VBoxProcessId <= 0 && instance.TopWindow == 0 && instance.BindWindow == 0;
}

```

## src/MatrixDevice.Core/LdPlayer/LdPlayerAdbBinding.cs

```csharp
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace MatrixDevice.Core;

public static class LdPlayerAdbBinding
{
    public static async Task VerifyAsync(string serial, int expectedVBoxPid, long expectedStartedUtcTicks, CancellationToken ct = default)
    {
        IPEndPoint endpoint;
        if (serial.StartsWith("emulator-", StringComparison.Ordinal) &&
            int.TryParse(serial[9..], NumberStyles.None, CultureInfo.InvariantCulture, out var consolePort) && consolePort is > 0 and < 65535)
            endpoint = new(IPAddress.Loopback, consolePort + 1);
        else if (IPEndPoint.TryParse(serial, out var parsed) && parsed.AddressFamily == AddressFamily.InterNetwork && parsed.Port > 0)
            endpoint = parsed;
        else throw new DeviceException($"Local LDPlayer endpoint cannot be verified for serial={serial}; Unicode input disabled.", 501, 2001);
        if (expectedVBoxPid <= 0 || expectedStartedUtcTicks <= 0) throw new DeviceException("Invalid expected LDPlayer process identity.", 409, 1003);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct); deadline.CancelAfter(TimeSpan.FromSeconds(3));
        try
        {
            // The serial supplies only a candidate port. Its actual owner must match the frozen instance process.
            // No ADB request, guest command, or host:connect is sent over this owned probe.
            using var connection = await ExistingAdbSocket.OpenAsync(endpoint, deadline.Token).ConfigureAwait(false);
            var actual = AdbServerIdentity.ForConnection(connection.ConnectedSocket);
            if (actual.ProcessId != expectedVBoxPid || actual.StartedUtcTicks != expectedStartedUtcTicks)
                throw new DeviceException($"LDPlayer serial/index binding mismatch: serial={serial}, expectedPid={expectedVBoxPid}, actualPid={actual.ProcessId}. No text was sent.", 409, 1003);
        }
        catch (Exception ex) when (ex is SocketException or IOException or ObjectDisposedException || ex is OperationCanceledException && !ct.IsCancellationRequested)
        { throw new DeviceException($"Cannot verify the local LDPlayer binding: serial={serial}: {ex.Message}. No text was sent.", 503, 2000); }
    }
}

```

## src/MatrixDevice.Core/LdPlayer/LdPlayerModels.cs

```csharp
namespace MatrixDevice.Core;

public sealed record LdPlayerInstance(
    int Index, string Name, long TopWindow, long BindWindow, bool AndroidStarted,
    int ProcessId, int VBoxProcessId, int? Width = null, int? Height = null, int? Dpi = null,
    long Generation = 1, bool IsStarting = false);

public sealed record LdPlayerCommandResult(
    int Index, string Operation, bool Accepted, bool Verified, string Message,
    LdPlayerInstance? Instance);

public sealed record LdPlayerCapability(
    string Operation, bool Supported, bool RequiresConfirmation, string Description,
    IReadOnlyList<string> Limitations);

public interface ILdConsoleRunner
{
    Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct = default);
}

```

## src/MatrixDevice.Core/LdPlayer/LdPlayerService.cs

```csharp
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace MatrixDevice.Core;

public sealed class LdPlayerService : IAsyncDisposable
{
    private sealed record History(long Generation, bool Present);
    private readonly ILdConsoleRunner _runner;
    private readonly object _gate = new();
    private readonly SemaphoreSlim _refresh = new(1, 1);
    private readonly SerialCommandQueue _commands = new();
    private readonly CancellationTokenSource _lifetime = new();
    private readonly Dictionary<int, History> _history = [];
    private IReadOnlyList<LdPlayerInstance> _instances = Array.Empty<LdPlayerInstance>();
    private static long _lastEpoch;
    private const long MaximumSafeGeneration = 9_007_199_254_740_991;
    private readonly long _generationEpoch = NewGenerationEpoch();
    private bool _disposed;
    public bool IsDemo { get; }
    public IReadOnlyList<LdPlayerInstance> Instances { get { lock (_gate) return _instances; } }
    public const string OfficialDocumentation = "https://help.ldmnq.com/docs/LDPlayer-ming-ling-hang-kai-fa-zhe-shi-yong-shuo-ming/n";

    public static IReadOnlyList<LdPlayerCapability> Capabilities { get; } = Array.AsReadOnly(new[]
    {
        new LdPlayerCapability("list2", true, false, "查询实例目录", ["兼容 7 列与 10 列；运行标志不能证明 ADB 或应用就绪。"]),
        new LdPlayerCapability("launch", true, false, "启动指定实例", ["重查 list2 的运行标志与进程字段；未测 Android 首帧。"]),
        new LdPlayerCapability("quit", true, true, "关闭指定实例", ["必须 confirmed:true；可能中断该实例的应用和任务。"]),
        new LdPlayerCapability("reboot", true, true, "重启指定实例", ["必须 confirmed:true；不以退出码证明重启已完成。"]),
        new LdPlayerCapability("runapp", true, false, "启动指定应用", ["需实例运行；仅提交命令，不验证应用业务页面。"]),
        new LdPlayerCapability("killapp", true, false, "停止指定应用", ["需实例运行；不验证目标应用进程已经结束。"]),
        new LdPlayerCapability("installapp", true, false, "安装指定 APK", ["仅接受存在的完整 Windows .apk 路径；未验证安装结果。"]),
        new LdPlayerCapability("input", true, false, "输入文本", ["使用 call.input；允许中文，但未注入测试；拒绝换行、NUL 和选项前缀。"]),
        new LdPlayerCapability("keyevent", true, false, "发送系统按键", ["仅 back、home、volumeup、volumedown；不验证界面效果。"]),
        new LdPlayerCapability("quitall", false, false, "全局退出", ["本适配器不开放全局操作。"]),
        new LdPlayerCapability("remove/restore/reset", false, false, "删除、恢复、重置", ["涉及实例数据，此处不开放。"]),
        new LdPlayerCapability("modify/setprop/locate/network", false, false, "身份、权限、网络或环境修改", ["此处不开放 root、设备身份和权限边界变更。"]),
        new LdPlayerCapability("adb", false, false, "共享 ADB 服务", ["不启动、连接或重启 ADB；索引不自动推导已验证串号。"]),
        new LdPlayerCapability("backupapp", false, false, "应用数据备份", ["官方当前文档标为暂不可用。"]),
        new LdPlayerCapability("restoreapp", false, false, "应用数据恢复", ["官方当前文档标为暂不可用。"]),
        new LdPlayerCapability("globalsetting", false, false, "全局设置", ["官方当前文档标为暂不可用；也不开放全局变更。"]),
        new LdPlayerCapability("keyevent.menu", false, false, "菜单按键", ["官方当前文档明确说明 menu 映射暂不可用。"]),
        new LdPlayerCapability("push/pull/operate/downcpu/scan/zoom", false, false, "暂不可用命令", ["官方当前文档标为暂不可用；CLI 帮助中出现不等于可用。"])
    });

    public LdPlayerService(ILdConsoleRunner runner, bool isDemo = false)
    { _runner = runner ?? throw new ArgumentNullException(nameof(runner)); IsDemo = isDemo; }

    public async Task<IReadOnlyList<LdPlayerInstance>> RefreshAsync(CancellationToken ct = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _lifetime.Token);
        await _refresh.WaitAsync(linked.Token).ConfigureAwait(false);
        try
        {
            lock (_gate) ObjectDisposedException.ThrowIf(_disposed, this);
            var parsed = ParseList2(await _runner.RunAsync(["list2"], linked.Token).ConfigureAwait(false));
            lock (_gate)
            {
                var next = new List<LdPlayerInstance>();
                var present = parsed.Select(instance => instance.Index).ToHashSet();
                var previous = _instances.ToDictionary(instance => instance.Index);
                foreach (var instance in parsed)
                {
                    var generation = _history.TryGetValue(instance.Index, out var history) ? (history.Present ? history.Generation : NextGeneration(history.Generation)) : _generationEpoch;
                    if (history?.Present == true && previous.TryGetValue(instance.Index, out var old) && (old with { Generation = 1 }) != (instance with { Generation = 1 })) generation = NextGeneration(generation);
                    _history[instance.Index] = new(generation, true);
                    next.Add(instance with { Generation = generation });
                }
                foreach (var pair in _history.Where(pair => pair.Value.Present && !present.Contains(pair.Key)).ToArray())
                    _history[pair.Key] = new(NextGeneration(pair.Value.Generation), false);
                _instances = next.AsReadOnly();
                return _instances;
            }
        }
        finally { _refresh.Release(); }
    }

    public async Task<LdPlayerCommandResult> ExecuteAsync(int index, string operation, JsonObject body, CancellationToken ct = default)
    {
        ValidateIndex(index);
        ArgumentNullException.ThrowIfNull(body);
        operation = operation?.Trim().ToLowerInvariant() ?? "";
        var frozen = body.DeepClone().AsObject();
        var expected = ExpectedGeneration(frozen);
        BuildArguments(index, operation, frozen);
        await RefreshAsync(ct).ConfigureAwait(false);
        RequireGeneration(index, expected);
        return await _commands.RunAsync<LdPlayerCommandResult>(index.ToString(CultureInfo.InvariantCulture), async token =>
        {
            await RefreshAsync(token).ConfigureAwait(false);
            var before = RequireGeneration(index, expected);
            if (operation is not ("launch" or "quit" or "reboot") && !before.AndroidStarted)
                throw new DeviceException($"实例 {index} 尚未运行，不能执行 {operation}。", 409, 1003);
            await _runner.RunAsync(BuildArguments(index, operation, frozen), token).ConfigureAwait(false);
            LdPlayerInstance? after;
            try { await RefreshAsync(token).ConfigureAwait(false); after = Instances.FirstOrDefault(instance => instance.Index == index); }
            catch (DeviceException error) { return new(index, operation, true, false, $"命令已返回，但无法复查实例状态：{error.Message}", null); }
            if (after is null)
                return new(index, operation, true, false, "命令已返回，但实例消失或代次变化，结果无法确认。", after);
            var identityChanged = after.Name != before.Name || after.Width != before.Width || after.Height != before.Height || after.Dpi != before.Dpi;
            if (after.Generation != before.Generation && (operation is not ("launch" or "quit") || identityChanged))
                return new(index, operation, true, false, "命令已返回，但实例观察指纹变化，结果无法确认。", after);
            var verified = operation switch
            {
                "launch" => after.AndroidStarted && after.ProcessId > 0,
                "quit" => !after.AndroidStarted && after.ProcessId <= 0,
                _ => false
            };
            var label = operation switch { "launch" => "启动", "quit" => "关闭", "reboot" => "重启", "runapp" => "应用启动", "killapp" => "应用停止", "installapp" => "APK 安装", "input" => "文本输入", "keyevent" => "系统按键", _ => operation };
            var message = verified ? $"{label}命令已返回，list2 已确认目标运行状态；未验证 ADB、首帧或业务页面。" : $"{label}命令已返回；CLI 退出码仅表示受理，实际设备效果尚未验证。";
            if (after.Generation != before.Generation) message += $" 实例代次已更新为 {after.Generation}。";
            if (IsDemo) message = "模拟模式：" + message;
            return new(index, operation, true, verified, message, after);
        }, ct).ConfigureAwait(false);
    }

    private LdPlayerInstance RequireGeneration(int index, long expected)
    {
        var instance = Instances.FirstOrDefault(item => item.Index == index) ?? throw new DeviceException($"雷电实例不存在：索引={index}", 404, 1002);
        if (instance.Generation != expected) throw new DeviceException($"雷电实例代次已变化：索引={index}，请求={expected}，当前={instance.Generation}", 409, 1003);
        return instance;
    }
    private static IReadOnlyList<string> BuildArguments(int index, string operation, JsonObject body)
    {
        var allowed = operation switch
        {
            "launch" => new[] { "expectedGeneration" }, "quit" or "reboot" => ["expectedGeneration", "confirmed"],
            "runapp" or "killapp" => ["expectedGeneration", "packageName"], "installapp" => ["expectedGeneration", "file"],
            "input" => ["expectedGeneration", "text"], "keyevent" => ["expectedGeneration", "key"],
            _ => throw new DeviceException($"雷电操作未开放：{operation}", 501, 2001)
        };
        foreach (var pair in body)
            if (!allowed.Contains(pair.Key, StringComparer.OrdinalIgnoreCase)) throw new DeviceException($"不允许的雷电参数：{pair.Key}", 400, 1007);
        List<string> args = [operation is "input" or "keyevent" ? "action" : operation, "--index", index.ToString(CultureInfo.InvariantCulture)];
        if (operation is "quit" or "reboot")
        {
            if (Property(body, "confirmed") is not JsonValue flag || !flag.TryGetValue<bool>(out var value) || !value)
                throw new DeviceException("关闭或重启前必须提供 confirmed:true。", 403, 1003);
        }
        if (operation is "runapp" or "killapp")
        {
            var package = String(body, "packageName");
            if (!Regex.IsMatch(package, @"^[A-Za-z][A-Za-z0-9_]*(\.[A-Za-z][A-Za-z0-9_]*)+$")) throw new DeviceException($"应用包名不合法：{package}", 400, 1007);
            args.AddRange(["--packagename", package]);
        }
        else if (operation == "installapp")
        {
            var file = String(body, "file");
            if (!Path.IsPathFullyQualified(file) || !file.EndsWith(".apk", StringComparison.OrdinalIgnoreCase) || !File.Exists(file))
                throw new DeviceException($"APK 必须为已存在的完整 Windows .apk 路径：{file}", 400, 1007);
            args.AddRange(["--filename", Path.GetFullPath(file)]);
        }
        else if (operation == "input")
        {
            var text = String(body, "text");
            if (text.Length > 4096 || text.IndexOfAny(['\0', '\r', '\n']) >= 0 || text.TrimStart().StartsWith("--", StringComparison.Ordinal))
                throw new DeviceException("输入文本不可超过 4096 字符，不得包含 NUL、换行或选项前缀。", 400, 1007);
            args.AddRange(["--key", "call.input", "--value", text]);
        }
        else if (operation == "keyevent")
        {
            var key = String(body, "key").ToLowerInvariant();
            if (key is not ("back" or "home" or "volumeup" or "volumedown")) throw new DeviceException($"系统按键未开放：{key}", 400, 1007);
            args.AddRange(["--key", "call.keyboard", "--value", key]);
        }
        return args;
    }
    private static long ExpectedGeneration(JsonObject body)
    {
        if (Property(body, "expectedGeneration") is JsonValue number)
        {
            if (number.TryGetValue<long>(out var value) && value > 0) return value;
            if (number.TryGetValue<int>(out var small) && small > 0) return small;
        }
        throw new DeviceException("expectedGeneration 必须为正整数。", 400, 1007);
    }
    private static JsonNode? Property(JsonObject body, string name)
    {
        var matches = body.Where(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (matches.Length > 1) throw new DeviceException($"参数重复：{name}", 400, 1007);
        return matches.Length == 0 ? null : matches[0].Value;
    }
    private static string String(JsonObject body, string name)
    {
        if (Property(body, name) is JsonValue value && value.TryGetValue<string>(out var text) && !string.IsNullOrWhiteSpace(text)) return text;
        throw new DeviceException($"参数 {name} 必须为非空字符串。", 400, 1007);
    }
    private static void ValidateIndex(int index)
    { if (index < 0 || index == 99999) throw new DeviceException($"雷电实例索引不合法：{index}", 400, 1007); }

    private static long NewGenerationEpoch()
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        RandomNumberGenerator.Fill(bytes);
        var candidate = checked((long)(BitConverter.ToUInt64(bytes) & ((1UL << 40) - 1)) * 1024);
        while (true)
        {
            var previous = Volatile.Read(ref _lastEpoch);
            var next = Math.Max(candidate, checked(previous + 65_536));
            if (next > MaximumSafeGeneration) throw new DeviceException("雷电实例代次空间已耗尽，请重新启动服务。", 503, 2000);
            if (Interlocked.CompareExchange(ref _lastEpoch, next, previous) == previous) return next;
        }
    }

    private static long NextGeneration(long generation)
    {
        if (generation >= MaximumSafeGeneration) throw new DeviceException("雷电实例代次空间已耗尽，请重新启动服务。", 503, 2000);
        return checked(generation + 1);
    }

    public static IReadOnlyList<LdPlayerInstance> ParseList2(string output)
    {
        ArgumentNullException.ThrowIfNull(output);
        var instances = new List<LdPlayerInstance>();
        var indices = new HashSet<int>();
        var lineNumber = 0;
        foreach (var raw in output.TrimStart('\ufeff').Split('\n'))
        {
            lineNumber++;
            var line = raw.TrimEnd('\r'); if (string.IsNullOrWhiteSpace(line)) continue;
            var fields = line.Split(',');
            if (fields.Length < 2 || !int.TryParse(fields[0].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var index))
                throw new DeviceException($"list2 第 {lineNumber} 行索引不合法。", 400, 1007);
            if (index == 99999) continue;
            ValidateIndex(index);
            bool Candidate(int suffix, out LdPlayerInstance? instance)
            {
                instance = null;
                if (fields.Length < suffix + 2) return false;
                var start = fields.Length - suffix;
                var name = string.Join(',', fields.Skip(1).Take(start - 1));
                if (string.IsNullOrWhiteSpace(name)) return false;
                var numbers = new long[suffix];
                for (var position = 0; position < suffix; position++)
                    if (!long.TryParse(fields[start + position].Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out numbers[position])) return false;
                var knownStartupTransition = suffix == 8 && numbers[2] == 2;
                if (numbers[0] < 0 || numbers[1] < 0 || numbers[2] is not (0 or 1) && !knownStartupTransition || numbers[3] is < -1 or > int.MaxValue || numbers[4] is < -1 or > int.MaxValue) return false;
                if (suffix == 8 && numbers.Skip(5).Any(value => value is <= 0 or > int.MaxValue)) return false;
                instance = new(index, name, numbers[0], numbers[1], numbers[2] == 1, (int)numbers[3], (int)numbers[4], suffix == 8 ? (int)numbers[5] : null, suffix == 8 ? (int)numbers[6] : null, suffix == 8 ? (int)numbers[7] : null, IsStarting: knownStartupTransition);
                return true;
            }
            var oldValid = Candidate(5, out var old);
            var modernValid = Candidate(8, out var modern);
            if (oldValid && modernValid) throw new DeviceException($"list2 第 {lineNumber} 行存在 7 列/10 列歧义；请确认名称中的数字及逗号。", 502, 2000);
            if (oldValid && !modernValid && fields.Length >= 10 && fields.TakeLast(8).All(field => long.TryParse(field.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _)))
                throw new DeviceException($"list2 第 {lineNumber} 行疑似损坏的 10 列格式，或名称中的数字及逗号产生歧义；拒绝将其解析为旧版窗口和进程字段。", 502, 2000);
            if (!oldValid && !modernValid) throw new DeviceException($"list2 第 {lineNumber} 行字段不合法，要求旧版 7 列或新版 10 列数值后缀。", 400, 1007);
            if (!indices.Add(index)) throw new DeviceException($"list2 包含重复索引：{index}", 502, 2000);
            instances.Add((modern ?? old)!);
        }
        return instances.AsReadOnly();
    }

    public async ValueTask DisposeAsync()
    {
        lock (_gate) { if (_disposed) return; _disposed = true; }
        _lifetime.Cancel();
        await _commands.DisposeAsync().ConfigureAwait(false);
        await _refresh.WaitAsync(CancellationToken.None).ConfigureAwait(false);
        _refresh.Release(); _refresh.Dispose(); _lifetime.Dispose();
        if (_runner is IAsyncDisposable disposable) await disposable.DisposeAsync().ConfigureAwait(false);
    }
}

```

## src/MatrixDevice.Core/MatrixDevice.Core.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="AdvancedSharpAdbClient" Version="3.6.16" />
    <PackageReference Include="SkiaSharp" Version="4.153.1" />
  </ItemGroup>
</Project>

```

## src/MatrixDevice.Core/Models/Contracts.cs

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MatrixDevice.Core;

public static class JsonDefaults
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };
}

public sealed class DeviceException(string message, int status = 409, int code = 1003) : Exception(message)
{
    public int Status { get; } = status;
    public int Code { get; } = code;
}

public sealed record DeviceInfo
{
    public required string Serial { get; init; }
    public string Name { get; init; } = "";
    public string Brand { get; init; } = "";
    public string Model { get; init; } = "";
    public string AndroidVersion { get; init; } = "";
    public string State { get; init; } = "Discovered";
    public string ConnectionType { get; init; } = "USB";
    public string GroupId { get; init; } = "";
    public int Width { get; init; }
    public int Height { get; init; }
    public int? Rotation { get; init; }
    public long Generation { get; init; } = 1;
    public uint? TransportId { get; init; }
    public string? Error { get; init; }
    public bool Online => State == "Ready";
}

public sealed record NodeBounds(int Left, int Top, int Right, int Bottom)
{
    public int Width => Right - Left;
    public int Height => Bottom - Top;
    public int CenterX => Left + Width / 2;
    public int CenterY => Top + Height / 2;
}

public sealed record AndroidNode
{
    public string? Text { get; init; }
    public string? ResourceId { get; init; }
    public string? ClassName { get; init; }
    public string? PackageName { get; init; }
    public string? ContentDesc { get; init; }
    public int Index { get; init; }
    public int Instance { get; init; }
    public bool Clickable { get; init; }
    public bool Enabled { get; init; }
    public bool Focusable { get; init; }
    public bool Focused { get; init; }
    public bool Scrollable { get; init; }
    public bool LongClickable { get; init; }
    public bool Checkable { get; init; }
    public bool Checked { get; init; }
    public bool Selected { get; init; }
    public bool VisibleToUser { get; init; } = true;
    public NodeBounds Bounds { get; init; } = new(0, 0, 0, 0);
    public List<AndroidNode> ChildNodeList { get; init; } = [];
}

public sealed record NodeSelector
{
    public string? Text { get; init; }
    public string? TextContains { get; init; }
    public string? ResourceId { get; init; }
    public string? ClassName { get; init; }
    public string? PackageName { get; init; }
    public int? Instance { get; init; }
    public int? Index { get; init; }
    public bool? Clickable { get; init; }
    public bool? Enabled { get; init; }
    public bool? Focusable { get; init; }
    public bool? Focused { get; init; }
    public bool? Scrollable { get; init; }
    public bool? LongClickable { get; init; }
    public bool? Checkable { get; init; }
    public bool? Checked { get; init; }
    public bool? Selected { get; init; }
    public bool? VisibleToUser { get; init; }
}

public sealed record AppInfo(string PackageName, string? Name = null, string? VersionName = null, bool IsSystem = false);

public interface IDeviceBackend : IAsyncDisposable
{
    Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default);
    Task<string> ShellAsync(string serial, string command, bool useRoot = false, CancellationToken ct = default);
    Task<byte[]> ScreenshotAsync(string serial, CancellationToken ct = default);
    Task<AndroidNode> DumpAsync(string serial, CancellationToken ct = default);
    Task TapAsync(string serial, int x, int y, CancellationToken ct = default);
    Task SwipeAsync(string serial, int x1, int y1, int x2, int y2, int durationMs, CancellationToken ct = default);
    Task KeyEventAsync(string serial, int code, CancellationToken ct = default);
    Task TextAsync(string serial, string text, CancellationToken ct = default);
    Task<string> StartAppAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default);
    Task<string> StopAppAsync(string serial, string packageName, CancellationToken ct = default);
    Task<bool> ClearAppAsync(string serial, string packageName, CancellationToken ct = default);
    Task<IReadOnlyList<AppInfo>> ListAppsAsync(string serial, bool showSystem = false, bool needVersion = true, CancellationToken ct = default);
    Task InstallAsync(string serial, string localPath, CancellationToken ct = default);
    Task UninstallAsync(string serial, string packageName, CancellationToken ct = default);
    Task PushAsync(string serial, string localPath, string remotePath, IProgress<double>? progress = null, CancellationToken ct = default);
    Task PullAsync(string serial, string remotePath, string localPath, IProgress<double>? progress = null, CancellationToken ct = default);
    Task SetNodeTextAsync(string serial, NodeSelector selector, string text, CancellationToken ct = default);
}

public sealed record AppSettings
{
    public string LdConsolePath { get; init; } = "";
    public Dictionary<int, string> LdSerialBindings { get; init; } = [];
    public string AdbPath { get; init; } = "";
    public string[] AdbAllowedSerials { get; init; } = [];
    public string ScrcpyPath { get; init; } = "";
    public string? AgentBaseUrl { get; init; }
    public string? AgentSerial { get; init; }
    public int BasicApiPort { get; init; } = 8080;
    public int AutomatorApiPort { get; init; } = 17171;
    public bool AutoStartApi { get; init; }
    public bool AutoConnect { get; init; }
    public bool SyncMouse { get; init; }
    public bool SyncKeyboard { get; init; }
    public int WallColumns { get; init; } = 5;
    public int MainMaxSize { get; init; } = 1280;
    public int MiniMaxSize { get; init; } = 240;
    public int MainMaxFps { get; init; } = 45;
    public int MiniMaxFps { get; init; } = 25;
    public int MainBitRate { get; init; } = 6000000;
    public int MiniBitRate { get; init; } = 100000;
    public bool ForceHardwareDecode { get; init; }
    public bool ShowFps { get; init; }
    public string Language { get; init; } = "zh-CN";
    public double WindowWidth { get; init; } = 1440;
    public double WindowHeight { get; init; } = 900;
    public string FileRoot { get; init; } = "/sdcard/MatrixExchange";
    public int CommandTimeoutSeconds { get; init; } = 30;
}

```

## src/MatrixDevice.Core/packages.lock.json

```json
{
  "version": 1,
  "dependencies": {
    "net10.0": {
      "AdvancedSharpAdbClient": {
        "type": "Direct",
        "requested": "[3.6.16, )",
        "resolved": "3.6.16",
        "contentHash": "w5p3s+9Rkx9tmn5S5KU0XSTap5YWGHOvQOw/40M0d7eGOHf7idhX+kizWPmnP1ahbkic/yiX03SMYNR1mJSAVw=="
      },
      "SkiaSharp": {
        "type": "Direct",
        "requested": "[4.153.1, )",
        "resolved": "4.153.1",
        "contentHash": "TghYLOUaP3q6rTjQw7U9uQaaXh8lEs9rfA1U7/VtmyAptVDtj/SmjtcWzFWmcC/VMWxe3QgbV9uyvqDL7ySFtQ==",
        "dependencies": {
          "SkiaSharp.NativeAssets.Win32": "4.153.1",
          "SkiaSharp.NativeAssets.macOS": "4.153.1"
        }
      },
      "SkiaSharp.NativeAssets.macOS": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "YTojWeWIhnpF7uC8vpMxB5wRONKkDB2t3r/nNPA3DL2pgj9fqMXRevA/rIanYazWnco4jo1ufgp3HkXuh1Tz8A=="
      },
      "SkiaSharp.NativeAssets.Win32": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "Yqg6rChTVh0BUE8QQxtOA3uUBnihfgID4yex/AvDa0+os2bVcBjmI3Qj9tnOB+R1hd/Jry1Gwl8qw5JI9HZG8Q=="
      }
    }
  }
}
```

## src/MatrixDevice.Core/Properties/AssemblyInfo.cs

```csharp
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MatrixDevice.Tests")]

```

## src/MatrixDevice.Core/Protocols/PhoneAgentClient.cs

```csharp
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace MatrixDevice.Core;

public sealed class PhoneAgentClient : IAsyncDisposable
{
    private readonly HttpClient _http;
    public const int MaximumResponseBytes = 32 * 1024 * 1024;
    public Uri BaseUrl { get; }
    public PhoneAgentClient(string baseUrl, HttpMessageHandler? handler = null)
    {
        if (!Uri.TryCreate(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/", UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            throw new ArgumentException($"Invalid agent BaseURL: {baseUrl}", nameof(baseUrl));
        BaseUrl = uri;
        _http = handler is null ? new HttpClient() : new HttpClient(handler);
        _http.BaseAddress = uri;
        _http.Timeout = TimeSpan.FromSeconds(15);
        _http.DefaultRequestHeaders.ConnectionClose = true;
    }

    public async Task<JsonElement> SendAsync(HttpMethod method, string path, object? body = null, IReadOnlyDictionary<string, string?>? query = null, CancellationToken ct = default)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        try { return await SendCoreAsync(method, path, body, query, deadline.Token).ConfigureAwait(false); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw new DeviceException($"Agent HTTP request timed out: {method} {path}.", 504, 2000); }
    }

    private async Task<JsonElement> SendCoreAsync(HttpMethod method, string path, object? body, IReadOnlyDictionary<string, string?>? query, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (Uri.TryCreate(path, UriKind.Absolute, out _)) throw new ArgumentException("Agent paths must be relative.", nameof(path));
        var target = path.TrimStart('/');
        if (query is not null)
        {
            var values = query.Where(item => item.Value is not null).Select(item => Uri.EscapeDataString(item.Key) + "=" + Uri.EscapeDataString(item.Value!));
            var encoded = string.Join("&", values);
            if (encoded.Length > 0) target += "?" + encoded;
        }
        using var request = new HttpRequestMessage(method, target);
        if (body is not null) request.Content = JsonContent.Create(body, options: JsonDefaults.Options);
        HttpResponseMessage response;
        try { response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false); }
        catch (HttpRequestException ex) { throw new DeviceException($"Agent HTTP request failed: {method} {path}: {ex.Message}", 503, 2000); }
        using (response)
        {
            if (response.Content.Headers.ContentLength > MaximumResponseBytes) throw new DeviceException($"Agent response exceeds {MaximumResponseBytes} bytes: {path}.", 413, 1007);
            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            var bytes = await BoundedStreamRead.ReadAsync(stream, MaximumResponseBytes, ct).ConfigureAwait(false);
            var text = System.Text.Encoding.UTF8.GetString(bytes);
            if (!response.IsSuccessStatusCode) throw new DeviceException($"Agent returned HTTP {(int)response.StatusCode}: {path}: {text}", 502, 2000);
            try
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("code", out var code) || code.ValueKind != JsonValueKind.Number || !code.TryGetInt32(out var value))
                    throw new JsonException("Envelope must be an object with an integer code.");
                if (value != 0)
                {
                    var hasMessage = root.TryGetProperty("message", out var detail) && detail.ValueKind == JsonValueKind.String;
                    var message = hasMessage ? detail.GetString() : "message omitted or invalid";
                    throw new DeviceException($"Agent API failed: {path}, code={value}, message={message}", hasMessage ? 409 : 502, value);
                }
                return root.TryGetProperty("data", out var data) ? data.Clone() : JsonSerializer.SerializeToElement<object?>(null);
            }
            catch (JsonException ex) { throw new DeviceException($"Agent returned invalid JSON: {path}: {ex.Message}", 502, 2000); }
        }
    }

    public async Task<AndroidNode> DumpAsync(bool compressed = true, CancellationToken ct = default)
    {
        var data = await SendAsync(HttpMethod.Get, "device/dump", query: new Dictionary<string, string?> { ["compressed"] = compressed ? "true" : "false" }, ct: ct);
        if (data.ValueKind != JsonValueKind.Object) throw new DeviceException($"Agent dump must be an AndroidNode object; received {data.ValueKind}.", 502, 2000);
        try { return data.Deserialize<AndroidNode>(JsonDefaults.Options) ?? throw new DeviceException("Agent dump was null.", 502, 2000); }
        catch (JsonException ex) { throw new DeviceException("Invalid agent AndroidNode data: " + ex.Message, 502, 2000); }
    }

    public Task<JsonElement> ShellAsync(string command, CancellationToken ct = default) => SendAsync(HttpMethod.Post, "device/cmd", new { cmd = command }, ct: ct);
    public Task<JsonElement> ListAppsAsync(CancellationToken ct = default) => SendAsync(HttpMethod.Get, "app/list", ct: ct);
    public Task<JsonElement> StartAppAsync(string packageName, CancellationToken ct = default) => SendAsync(HttpMethod.Post, "app/start", new { package = packageName }, ct: ct);
    public Task<JsonElement> StopAppAsync(string packageName, CancellationToken ct = default) => SendAsync(HttpMethod.Post, "app/stop", new { package = packageName }, ct: ct);
    public Task<JsonElement> TapAsync(int x, int y, CancellationToken ct = default) => SendAsync(HttpMethod.Post, "input/tap", new { x, y }, ct: ct);
    public Task<JsonElement> SwipeAsync(int x1, int y1, int x2, int y2, int steps = 55, int stepMs = 5, CancellationToken ct = default) =>
        SendAsync(HttpMethod.Post, "input/swipe", new { x1, y1, x2, y2, steps, stepMs }, ct: ct);
    public Task<JsonElement> KeyEventAsync(int code, CancellationToken ct = default) => SendAsync(HttpMethod.Post, "input/keyevent", new { code }, ct: ct);
    public Task<JsonElement> ServiceInfoAsync(CancellationToken ct = default) => SendAsync(HttpMethod.Get, "service/info", ct: ct);
    public Task<JsonElement> StopServiceAsync(CancellationToken ct = default) => SendAsync(HttpMethod.Get, "service/stop", ct: ct);
    public Task<JsonElement> InfoAsync(CancellationToken ct = default) => SendAsync(HttpMethod.Get, "info/detail", ct: ct);

    public Task<JsonElement> NodeInfoAsync(NodeSelector selector, CancellationToken ct = default) => SendAsync(HttpMethod.Get, "node/info", query: SelectorQuery(selector), ct: ct);
    public Task<JsonElement> NodeExistsAsync(NodeSelector selector, CancellationToken ct = default) => SendAsync(HttpMethod.Get, "node/existsVisible", query: SelectorQuery(selector), ct: ct);
    public Task<JsonElement> ClickNodeAsync(NodeSelector selector, bool longClick = false, CancellationToken ct = default) => SendAsync(HttpMethod.Post, longClick ? "node/long-click" : "node/click", selector, ct: ct);
    public Task<JsonElement> SetNodeTextAsync(NodeSelector selector, string text, CancellationToken ct = default)
    {
        var fields = SelectorQuery(selector).ToDictionary(item => item.Key, item => (object?)item.Value);
        foreach (var property in typeof(NodeSelector).GetProperties())
        {
            var value = property.GetValue(selector);
            if (value is not null) fields[JsonNamingPolicy.CamelCase.ConvertName(property.Name)] = value;
        }
        fields["set_text"] = text;
        return SendAsync(HttpMethod.Post, "node/set-text", fields, ct: ct);
    }

    public async Task<byte[]> ScreenshotAsync(int? x = null, int? y = null, int? width = null, int? height = null, int? quality = null, CancellationToken ct = default)
    {
        var values = new Dictionary<string, string?> { ["x"] = x?.ToString(CultureInfo.InvariantCulture), ["y"] = y?.ToString(CultureInfo.InvariantCulture), ["width"] = width?.ToString(CultureInfo.InvariantCulture), ["height"] = height?.ToString(CultureInfo.InvariantCulture), ["quality"] = quality?.ToString(CultureInfo.InvariantCulture) };
        var data = await SendAsync(HttpMethod.Get, "device/screenshot", query: values, ct: ct);
        string? encoded = null;
        if (data.ValueKind == JsonValueKind.String) encoded = data.GetString();
        else if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("base64", out var base64) && base64.ValueKind == JsonValueKind.String) encoded = base64.GetString();
        if (string.IsNullOrEmpty(encoded)) throw new DeviceException("Agent screenshot did not contain base64 PNG bytes.", 502, 2000);
        if (encoded.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var comma = encoded.IndexOf(',');
            if (comma < 0) throw new DeviceException("Invalid screenshot data URI: comma separator is missing.", 502, 2000);
            encoded = encoded[(comma + 1)..];
        }
        try { var png = Convert.FromBase64String(encoded); PngImage.ReadDimensions(png); return png; }
        catch (Exception ex) when (ex is FormatException or InvalidDataException) { throw new DeviceException($"Invalid agent PNG screenshot: {ex.Message}", 502, 2000); }
    }

    private static IReadOnlyDictionary<string, string?> SelectorQuery(NodeSelector selector) =>
        typeof(NodeSelector).GetProperties().Where(property => property.GetValue(selector) is not null).ToDictionary(property => JsonNamingPolicy.CamelCase.ConvertName(property.Name), property =>
        {
            var value = property.GetValue(selector);
            return value is bool flag ? flag.ToString().ToLowerInvariant() : Convert.ToString(value, CultureInfo.InvariantCulture);
        });
    public ValueTask DisposeAsync() { _http.Dispose(); return ValueTask.CompletedTask; }
}

```

## src/MatrixDevice.Core/Protocols/ScrcpyProtocol.cs

```csharp
using System.Buffers.Binary;

namespace MatrixDevice.Core;

public sealed record VideoPacketHeader(ulong Flags, bool IsCodecConfig, bool IsKeyFrame, int PayloadLength)
{
    public ulong TimestampBits => Flags & 0x3fffffffffffffff;
}

public static class ScrcpyProtocol
{
    public const int VideoHeaderLength = 12;
    public const int TouchPacketLength = 32;
    public const int DefaultMaximumPayload = 16 * 1024 * 1024;

    public static VideoPacketHeader ParseVideoHeader(ReadOnlySpan<byte> bytes, int maximumPayload = DefaultMaximumPayload)
    {
        if (bytes.Length != VideoHeaderLength) throw new InvalidDataException($"Video header must be 12 bytes, received {bytes.Length}.");
        if (maximumPayload <= 0) throw new ArgumentOutOfRangeException(nameof(maximumPayload));
        var flags = BinaryPrimitives.ReadUInt64BigEndian(bytes);
        var length = BinaryPrimitives.ReadInt32BigEndian(bytes[8..]);
        if (length > maximumPayload) throw new InvalidDataException($"Video payload {length} exceeds the configured {maximumPayload} bytes.");
        return new(flags, (flags & (1UL << 63)) != 0, (flags & (1UL << 62)) != 0, length);
    }

    public static async Task<(VideoPacketHeader Header, byte[] Payload)?> ReadPacketAsync(Stream stream, CancellationToken ct = default)
    {
        var headerBytes = new byte[VideoHeaderLength];
        while (true)
        {
            var first = await stream.ReadAsync(headerBytes.AsMemory(0, 1), ct).ConfigureAwait(false);
            if (first == 0) return null;
            await stream.ReadExactlyAsync(headerBytes.AsMemory(1), ct).ConfigureAwait(false);
            var header = ParseVideoHeader(headerBytes);
            if (header.PayloadLength <= 0) continue;
            var payload = new byte[header.PayloadLength];
            await stream.ReadExactlyAsync(payload, ct).ConfigureAwait(false);
            return (header, payload);
        }
    }

    public static byte[] EncodeTouch(byte action, ulong pointerId, int x, int y, ushort width, ushort height, uint buttons = 1)
    {
        if (width == 0 || height == 0) throw new InvalidDataException($"Touch screen size must be positive: {width}x{height}.");
        if (x < 0 || x >= width || y < 0 || y >= height) throw new InvalidDataException($"Touch point ({x},{y}) is outside {width}x{height}.");
        if (action > 6) throw new InvalidDataException($"Unknown Android touch action: {action}.");
        var packet = new byte[TouchPacketLength];
        var released = action is 1 or 3;
        packet[0] = 2;
        packet[1] = action;
        BinaryPrimitives.WriteUInt64BigEndian(packet.AsSpan(2), pointerId);
        BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(10), x);
        BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(14), y);
        BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(18), width);
        BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(20), height);
        BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(22), released ? (ushort)0 : ushort.MaxValue);
        BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(24), 1);
        BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(28), released ? 0 : buttons);
        return packet;
    }
}

```

## src/MatrixDevice.Desktop/app.manifest

```xml
<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <assemblyIdentity version="1.0.0.0" name="MatrixDevice.Desktop" />
  <trustInfo xmlns="urn:schemas-microsoft-com:asm.v3"><security><requestedPrivileges><requestedExecutionLevel level="asInvoker" uiAccess="false" /></requestedPrivileges></security></trustInfo>
  <application xmlns="urn:schemas-microsoft-com:asm.v3"><windowsSettings><dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true/pm</dpiAware><dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness></windowsSettings></application>
</assembly>

```

## src/MatrixDevice.Desktop/App.xaml

```xml
<Application x:Class="MatrixDevice.Desktop.App" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="Themes/ModernTheme.xaml" />
      </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
  </Application.Resources>
</Application>

```

## src/MatrixDevice.Desktop/App.xaml.cs

```csharp
using System.IO;
using System.Windows;
using MatrixDevice.Core;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class App : Application
{
    private MatrixRuntime? runtime;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var smoke = e.Args.Contains("--smoke", StringComparer.OrdinalIgnoreCase);
            var liveVerification = e.Args.Contains("--verify-live", StringComparer.OrdinalIgnoreCase);
            var unifiedVerification = e.Args.Contains("--verify-unified-access", StringComparer.OrdinalIgnoreCase);
            var oneClickVerification = e.Args.Contains("--verify-one-click-start", StringComparer.OrdinalIgnoreCase);
            if (new[] { smoke, liveVerification, unifiedVerification, oneClickVerification }.Count(enabled => enabled) > 1)
                throw new ArgumentException("一次只能选择一种自动验收模式。");
            string? unifiedCandidate = null;
            var demo = smoke || e.Args.Contains("--demo", StringComparer.OrdinalIgnoreCase);
            var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MatrixDevice.Reconstructed", demo ? "demo" : "data");
            var directoryIndex = Array.FindIndex(e.Args, arg => arg.Equals("--data-dir", StringComparison.OrdinalIgnoreCase));
            if (directoryIndex >= 0)
            {
                if (directoryIndex + 1 >= e.Args.Length || !Path.IsPathFullyQualified(e.Args[directoryIndex + 1]))
                    throw new ArgumentException("--data-dir 后必须提供独立配置目录的绝对路径");
                dataDirectory = Path.GetFullPath(e.Args[directoryIndex + 1]);
            }
            Directory.CreateDirectory(dataDirectory);
            var store = new SettingsStore(Path.Combine(dataDirectory, "AppConfig.json"));
            var settings = store.Load() with { Language = "zh-CN" };
            var ldConsoleIndex = Array.FindIndex(e.Args, arg => arg.Equals("--ldconsole", StringComparison.OrdinalIgnoreCase));
            if (ldConsoleIndex >= 0)
            {
                if (ldConsoleIndex + 1 >= e.Args.Length || e.Args[ldConsoleIndex + 1].StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException("--ldconsole 后必须提供雷电命令行程序的完整路径");
                settings = settings with { LdConsolePath = Path.GetFullPath(e.Args[ldConsoleIndex + 1]) };
            }
            if (smoke)
            {
                settings = settings with { WindowWidth = 1440, WindowHeight = 900, AutoStartApi = false, AutoConnect = false };
                var widthIndex = Array.FindIndex(e.Args, arg => arg == "--width"); var heightIndex = Array.FindIndex(e.Args, arg => arg == "--height");
                if (widthIndex >= 0 && widthIndex + 1 < e.Args.Length && double.TryParse(e.Args[widthIndex + 1], out var width)) settings = settings with { WindowWidth = Math.Clamp(width, 1000, 2400) };
                if (heightIndex >= 0 && heightIndex + 1 < e.Args.Length && double.TryParse(e.Args[heightIndex + 1], out var height)) settings = settings with { WindowHeight = Math.Clamp(height, 700, 1800) };
            }
            if (unifiedVerification)
            {
                if (directoryIndex < 0 || !dataDirectory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("接入验收必须使用artifacts下的独立配置。");
                if (!settings.LdSerialBindings.TryGetValue(2, out var approved) || !settings.AdbAllowedSerials.Contains(approved, StringComparer.Ordinal)) throw new ArgumentException("独立实例2未在复制配置中授权。");
                if (!settings.LdSerialBindings.TryGetValue(1, out var candidate) || !settings.AdbAllowedSerials.Contains(candidate, StringComparer.Ordinal) || candidate == approved) throw new ArgumentException("接入验收仅允许复制配置中已经明确授权的独立实例1/2。");
                unifiedCandidate = candidate;
                settings = settings with { AutoStartApi = false, AutoConnect = false, AdbAllowedSerials = [approved], LdSerialBindings = new() { [2] = approved } };
            }
            if (oneClickVerification)
            {
                if (demo || unifiedVerification || directoryIndex < 0 || !dataDirectory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !settings.LdSerialBindings.TryGetValue(1, out var candidate) || !settings.AdbAllowedSerials.Contains(candidate, StringComparer.Ordinal)) throw new ArgumentException("一键冷启动验收必须使用包含原批准实例1的独立artifacts配置，不能混用其它验收模式。");
                unifiedCandidate = candidate;
                settings = settings with { AutoStartApi = false, AutoConnect = false, AdbAllowedSerials = [], LdSerialBindings = new(), AgentBaseUrl = null, AgentSerial = null };
            }
            runtime = await MatrixRuntime.CreateAsync(settings, dataDirectory, demo);
            var window = new MainWindow(new WorkspaceViewModel(runtime, dataDirectory, demo));
            MainWindow = window;
            window.Show();
            await window.ViewModel.RefreshDevicesAsync();
            await window.RecoverBackgroundWindowsAsync();
            window.UpdateUnifiedDirectory();
            if (runtime.Settings.AutoStartApi) await window.ViewModel.StartApisAsync();
            if (smoke)
            {
                var screenshotIndex = Array.FindIndex(e.Args, arg => arg.Equals("--screenshot", StringComparison.OrdinalIgnoreCase));
                var screenshotPath = screenshotIndex >= 0 && screenshotIndex + 1 < e.Args.Length ? e.Args[screenshotIndex + 1] : null;
                await window.RunSmokeAsync(screenshotPath);
                window.Close();
            }
            if (liveVerification)
            {
                var outputIndex = Array.FindIndex(e.Args, arg => arg.Equals("--evidence-dir", StringComparison.OrdinalIgnoreCase));
                if (directoryIndex < 0 || outputIndex < 0 || outputIndex + 1 >= e.Args.Length) throw new ArgumentException("实机界面验收必须提供独立 --data-dir 和绝对 --evidence-dir");
                await window.RunLiveUiAcceptanceAsync(e.Args[outputIndex + 1]); window.Close();
            }
            if (unifiedVerification)
            {
                await window.RunUnifiedAccessAcceptanceAsync(Path.Combine(dataDirectory, "evidence"), unifiedCandidate!, e.Args.Contains("--verify-background-window", StringComparer.OrdinalIgnoreCase)); window.Close();
            }
            if (oneClickVerification)
            {
                await window.RunOneClickStartAcceptanceAsync(Path.Combine(dataDirectory, "evidence"), unifiedCandidate!); window.Close();
            }
        }
        catch (Exception ex)
        {
            if (e.Args.Contains("--smoke", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("--verify-live", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("--verify-unified-access", StringComparer.OrdinalIgnoreCase) || e.Args.Contains("--verify-one-click-start", StringComparer.OrdinalIgnoreCase)) Console.Error.WriteLine(ex);
            else MessageBox.Show(UiErrorPresentation.Describe(ex), "矩阵设备启动失败", MessageBoxButton.OK, MessageBoxImage.Error);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            try
            {
                if (MainWindow is MatrixDevice.Desktop.MainWindow failedWindow) failedWindow.AbortStartup();
                if (runtime is not null) await runtime.DisposeAsync();
            }
            catch (Exception cleanupError) { Console.Error.WriteLine("启动失败后的资源清理异常：" + cleanupError); }
            finally { Shutdown(1); }
        }
    }
}

```

## src/MatrixDevice.Desktop/LdWindowVisibility.cs

```csharp
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public sealed class LdWindowVisibility
{
    private const int SwHide = 0;
    private const int SwShowMinNoActive = 7;
    private const int SwShowNoActivate = 8;
    private const string MarkerPrefix = "MatrixDevice.LdVisibility.";
    private static readonly TimeSpan VisibilityTimeout = TimeSpan.FromSeconds(3);
    private readonly string journalDirectory;
    private readonly string installDirectory;
    private readonly SemaphoreSlim operations = new(1, 1);
    private readonly object gate = new();
    private readonly Dictionary<int, RecoveryRecord> owned = [];
    private readonly int controllerPid;
    private readonly long controllerBirth;

    public LdWindowVisibility(string journalDirectory, string expectedInstallDirectory)
    {
        if (!Path.IsPathFullyQualified(journalDirectory) || !Path.IsPathFullyQualified(expectedInstallDirectory) || !Directory.Exists(expectedInstallDirectory))
            throw new ArgumentException("窗口恢复目录与雷电安装目录必须为绝对路径，且安装目录必须存在。");
        installDirectory = ResolveExistingPath(expectedInstallDirectory);
        this.journalDirectory = Path.GetFullPath(Path.Combine(journalDirectory, "ld-window-visibility"));
        Directory.CreateDirectory(this.journalDirectory);
        this.journalDirectory = ResolveExistingPath(this.journalDirectory);
        using var process = Process.GetCurrentProcess();
        controllerPid = process.Id;
        controllerBirth = process.StartTime.ToUniversalTime().Ticks;
    }

    public async Task<bool> HideAsync(LdPlayerInstance instance, long expectedPlayerBirth, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ValidateIndex(instance.Index);
        if (instance.ProcessId <= 0 || instance.TopWindow <= 0 || expectedPlayerBirth <= 0)
            throw new DeviceException("隐藏窗口需要真实顶层窗口、播放器 PID 与已冻结的进程出生时刻。", 400, 1007);
        await operations.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var window = new IntPtr(instance.TopWindow);
            var executable = ValidateTarget(window, instance.ProcessId, expectedPlayerBirth, null);
            RecoveryRecord? existing;
            lock (gate) owned.TryGetValue(instance.Index, out existing);
            if (existing is not null)
            {
                if (existing.Window != instance.TopWindow || existing.ProcessId != instance.ProcessId || existing.ProcessStartUtcTicks != expectedPlayerBirth)
                    throw new DeviceException($"实例 {instance.Index} 的窗口或进程已变化，不能复用旧隐藏记录。", 409, 1003);
                ValidateRecordOwner(existing);
                await ChangeVisibilityAsync(existing, false, ct).ConfigureAwait(false);
                return true;
            }
            if (!IsWindowVisible(window)) return false;
            if (Directory.EnumerateFiles(journalDirectory, $"ld-window-{instance.Index}-*.json").Any())
                throw new DeviceException($"实例 {instance.Index} 存在未处理的恢复记录，请先恢复或检查记录，未隐藏新窗口。", 409, 1003);
            var id = Guid.NewGuid().ToString("N");
            var record = new RecoveryRecord(1, id, instance.Index, instance.TopWindow, instance.ProcessId,
                expectedPlayerBirth, executable, true, IsIconic(window), MarkerPrefix + id, controllerPid, controllerBirth);
            var markerInstalled = false;
            try
            {
                // The marker disappears with the HWND, including reuse inside the same process.
                if (!SetProp(window, record.Marker, new IntPtr(1))) throw new Win32Exception(Marshal.GetLastWin32Error(), "无法标记自有隐藏窗口，未执行隐藏");
                markerInstalled = true;
                ValidateRecordOwner(record);
                await WriteRecordAsync(record, ct).ConfigureAwait(false);
            }
            catch
            {
                if (markerInstalled && IsWindow(window) && GetProp(window, record.Marker) == new IntPtr(1)) RemoveProp(window, record.Marker);
                throw;
            }
            lock (gate) owned.Add(instance.Index, record);
            await ChangeVisibilityAsync(record, false, ct).ConfigureAwait(false);
            return true;
        }
        finally { operations.Release(); }
    }

    public bool IsHidden(int index)
    {
        RecoveryRecord? record;
        lock (gate) owned.TryGetValue(index, out record);
        if (record is null) return false;
        var window = new IntPtr(record.Window);
        return IsWindow(window) && GetProp(window, record.Marker) == new IntPtr(1) &&
            GetWindowThreadProcessId(window, out var pid) != 0 && pid == record.ProcessId && !IsWindowVisible(window);
    }

    public async Task ShowAsync(int index, CancellationToken ct = default)
    {
        ValidateIndex(index);
        await operations.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            RecoveryRecord? record;
            lock (gate) owned.TryGetValue(index, out record);
            if (record is not null) await RestoreAsync(record, RecordPath(record), ct).ConfigureAwait(false);
        }
        finally { operations.Release(); }
    }

    public async Task RestoreAllAsync(CancellationToken ct = default)
    {
        await operations.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            RecoveryRecord[] records;
            lock (gate) records = owned.Values.ToArray();
            var failures = new List<Exception>();
            foreach (var record in records)
            {
                ct.ThrowIfCancellationRequested();
                try { await RestoreAsync(record, RecordPath(record), ct).ConfigureAwait(false); }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception error) { failures.Add(error); }
            }
            if (failures.Count > 0) throw new AggregateException("部分窗口未能安全恢复，已保留对应恢复记录。", failures);
        }
        finally { operations.Release(); }
    }

    public async Task<IReadOnlyList<string>> RecoverPendingAsync(CancellationToken ct = default)
    {
        await operations.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var errors = new List<string>();
            foreach (var path in Directory.EnumerateFiles(journalDirectory, "ld-window-*.json").Order(StringComparer.Ordinal))
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0 || new FileInfo(path).Length > 64 * 1024)
                        throw new InvalidDataException("恢复记录不能是链接或超过 64 KiB。");
                    var record = JsonSerializer.Deserialize<RecoveryRecord>(await File.ReadAllTextAsync(path, ct).ConfigureAwait(false), JsonDefaults.Options)
                        ?? throw new InvalidDataException("恢复记录为空。");
                    ValidateRecord(record, path);
                    if (record.ControllerPid != controllerPid && IsSameProcessAlive(record.ControllerPid, record.ControllerStartUtcTicks))
                        throw new InvalidOperationException("原控制程序仍在运行，未接管其隐藏窗口。");
                    await RestoreAsync(record, path, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception error) { errors.Add($"{Path.GetFileName(path)}：{error.Message}"); }
            }
            return errors.AsReadOnly();
        }
        finally { operations.Release(); }
    }

    private async Task RestoreAsync(RecoveryRecord record, string path, CancellationToken ct)
    {
        if (!File.Exists(path)) { Forget(record); return; }
        ValidateRecord(record, path);
        if (HasExited(record.ProcessId) || !IsWindow(new IntPtr(record.Window)))
        {
            DeleteRecord(path); Forget(record); return;
        }
        ValidateRecordOwner(record);
        await ChangeVisibilityAsync(record, record.WasVisible, ct).ConfigureAwait(false);
        DeleteRecord(path);
        if (IsWindow(new IntPtr(record.Window)) && GetProp(new IntPtr(record.Window), record.Marker) == new IntPtr(1)) RemoveProp(new IntPtr(record.Window), record.Marker);
        Forget(record);
    }

    private async Task ChangeVisibilityAsync(RecoveryRecord record, bool visible, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ValidateRecordOwner(record);
        var window = new IntPtr(record.Window);
        // ShowWindowAsync reports queueing, not the resulting visibility.
        ShowWindowAsync(window, visible ? record.WasMinimized ? SwShowMinNoActive : SwShowNoActivate : SwHide);
        var deadline = Stopwatch.StartNew();
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            ValidateRecordOwner(record);
            if (IsWindowVisible(window) == visible) return;
            if (deadline.Elapsed >= VisibilityTimeout) throw new TimeoutException($"窗口可见状态未在期限内变为 {visible}，恢复记录已保留：实例 {record.Index}");
            await Task.Delay(25, ct).ConfigureAwait(false);
        }
    }

    private string ValidateTarget(IntPtr window, int processId, long birth, string? expectedExecutable)
    {
        if (!IsWindow(window)) throw new DeviceException("目标窗口已经失效，未修改窗口。", 409, 1003);
        if (GetWindowThreadProcessId(window, out var actualPid) == 0 || actualPid != processId)
            throw new DeviceException("窗口所属 PID 不匹配，拒绝操作复用窗口。", 409, 1003);
        try
        {
            using var process = Process.GetProcessById(processId);
            if (process.HasExited || process.StartTime.ToUniversalTime().Ticks != birth)
                throw new DeviceException("播放器进程出生时刻不匹配，拒绝操作复用 PID。", 409, 1003);
            var executable = ResolveExistingPath(process.MainModule?.FileName ?? throw new InvalidOperationException("无法读取目标进程路径。"));
            var relative = Path.GetRelativePath(installDirectory, executable);
            if (Path.IsPathFullyQualified(relative) || relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                throw new DeviceException("目标进程不属于冻结的模拟器安装目录，未修改窗口。", 403, 1007);
            if (expectedExecutable is not null && !executable.Equals(expectedExecutable, StringComparison.OrdinalIgnoreCase))
                throw new DeviceException("目标可执行文件路径已变化，拒绝修改窗口。", 409, 1003);
            return executable;
        }
        catch (ArgumentException) { throw new DeviceException("目标进程已退出，未修改窗口。", 409, 1003); }
    }

    private void ValidateRecordOwner(RecoveryRecord record)
    {
        ValidateTarget(new IntPtr(record.Window), record.ProcessId, record.ProcessStartUtcTicks, record.ExecutablePath);
        if (GetProp(new IntPtr(record.Window), record.Marker) != new IntPtr(1))
            throw new DeviceException("窗口标记不匹配，可能已销毁或复用，拒绝恢复其它窗口。", 409, 1003);
    }

    private void ValidateRecord(RecoveryRecord record, string path)
    {
        ValidateIndex(record.Index);
        if (record.Version != 1 || !Guid.TryParseExact(record.Id, "N", out _) || record.Window <= 0 || record.ProcessId <= 0 ||
            record.ProcessStartUtcTicks <= 0 || !record.WasVisible || record.Marker != MarkerPrefix + record.Id ||
            record.ControllerPid <= 0 || record.ControllerStartUtcTicks <= 0 || !Path.IsPathFullyQualified(record.ExecutablePath) ||
            !Path.GetFullPath(path).Equals(RecordPath(record), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("恢复记录字段或文件名不合法，未修改窗口。");
    }

    private async Task WriteRecordAsync(RecoveryRecord record, CancellationToken ct)
    {
        var path = RecordPath(record);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            var bytes = new UTF8Encoding(false).GetBytes(JsonSerializer.Serialize(record, JsonDefaults.Options));
            await using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true))
            {
                await file.WriteAsync(bytes, ct).ConfigureAwait(false);
                await file.FlushAsync(ct).ConfigureAwait(false);
                file.Flush(true);
            }
            ct.ThrowIfCancellationRequested();
            File.Move(temporary, path, false);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private string RecordPath(RecoveryRecord record) => Path.Combine(journalDirectory, $"ld-window-{record.Index}-{record.Id}.json");
    private void DeleteRecord(string path)
    {
        if (!Path.GetDirectoryName(Path.GetFullPath(path))!.Equals(journalDirectory, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("恢复记录不在私有目录内，拒绝清理。");
        File.Delete(path);
    }
    private void Forget(RecoveryRecord record)
    {
        lock (gate)
            if (owned.TryGetValue(record.Index, out var current) && current.Id == record.Id) owned.Remove(record.Index);
    }
    private static void ValidateIndex(int index)
    {
        if (index <= 0 || index == 99999) throw new DeviceException($"窗口隐藏只允许非零独立实例：{index}", 400, 1007);
    }
    private static bool HasExited(int pid)
    {
        try { using var process = Process.GetProcessById(pid); return process.HasExited; }
        catch (ArgumentException) { return true; }
    }
    private static bool IsSameProcessAlive(int pid, long birth)
    {
        try { using var process = Process.GetProcessById(pid); return !process.HasExited && process.StartTime.ToUniversalTime().Ticks == birth; }
        catch (ArgumentException) { return false; }
    }
    private static string ResolveExistingPath(string path)
    {
        var absolute = Path.GetFullPath(path);
        var current = Path.GetPathRoot(absolute)!;
        foreach (var part in absolute[current.Length..].Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            var next = Path.Combine(current, part);
            var attributes = File.GetAttributes(next);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                FileSystemInfo info = (attributes & FileAttributes.Directory) != 0 ? new DirectoryInfo(next) : new FileInfo(next);
                current = info.ResolveLinkTarget(true)?.FullName ?? throw new IOException("无法解析路径中的链接，拒绝修改窗口。");
            }
            else current = next;
        }
        return Path.TrimEndingDirectorySeparator(current);
    }

    private sealed record RecoveryRecord(int Version, string Id, int Index, long Window, int ProcessId,
        long ProcessStartUtcTicks, string ExecutablePath, bool WasVisible, bool WasMinimized, string Marker,
        int ControllerPid, long ControllerStartUtcTicks);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ShowWindowAsync(IntPtr window, int command);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindow(IntPtr window);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsIconic(IntPtr window);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out int pid);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetPropW", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetProp(IntPtr window, string name, IntPtr value);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetPropW")] private static extern IntPtr GetProp(IntPtr window, string name);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "RemovePropW")] private static extern IntPtr RemoveProp(IntPtr window, string name);
}

```

## src/MatrixDevice.Desktop/LocalizationService.cs

```csharp
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MatrixDevice.Desktop;

public sealed class LocalizationService : INotifyPropertyChanged
{
    public static LocalizationService Current { get; } = new();
    public string Language => "zh-CN";
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? LanguageChanged;
    public string this[string key] => Get(key);

    private static readonly IReadOnlyDictionary<string, string> Chinese = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Shell"] = "命令终端", ["API"] = "接口服务", ["Home"] = "主页", ["serial"] = "设备序列号",
        ["Ready"] = "就绪", ["Offline"] = "离线", ["Unauthorized"] = "等待授权", ["Discovered"] = "已发现", ["Error"] = "异常",
        ["Idle"] = "未运行", ["Running"] = "运行中", ["Cancelling"] = "正在停止", ["Completed"] = "已完成", ["Cancelled"] = "已停止",
        ["Failed"] = "失败", ["TimedOut"] = "超时", ["Lost"] = "结果待核对", ["HEX"] = "十六进制色值", ["RGB"] = "三色值",
        ["普通 API"] = "普通接口", ["Automator API"] = "自动化接口", ["当前设备 API"] = "当前设备接口", ["普通 API 地址"] = "普通接口地址",
        ["普通 API 端口"] = "普通接口端口", ["Automator 端口"] = "自动化接口端口", ["授权 Agent 地址"] = "自动化服务地址",
        ["Agent 设备 serial"] = "自动化服务设备序列号", ["语言 / 区域"] = "界面语言", ["%"] = "进度（%）",
        ["English"] = "简体中文", ["Demo Devices"] = "演示设备"
    };

    public string Get(string key) => Chinese.TryGetValue(key, out var translated) ? translated : key;
    public void SetLanguage(string value)
    {
        var culture = CultureInfo.GetCultureInfo("zh-CN");
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public static void Bind(DependencyObject target, DependencyProperty property, string key) =>
        BindingOperations.SetBinding(target, property, new Binding($"[{key}]") { Source = Current, Mode = BindingMode.OneWay });
}

public sealed class CaptionChoiceConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var key = values.Length == 0 ? "" : values[0]?.ToString() ?? "";
        return LocalizationService.Current.Get(key);
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class SubsystemCaptionConverter(string label) : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
        LocalizationService.Current.Get(label) + "：" + LocalizationService.Current.Get(values.Length > 0 && values[0] is string value ? value : "未选择");
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class StateCaptionConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => LocalizationService.Current.Get(values.Length > 0 ? values[0]?.ToString() ?? "" : "");
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

```

## src/MatrixDevice.Desktop/MainWindow.xaml

```xml
<Window x:Class="MatrixDevice.Desktop.MainWindow" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" Title="矩阵设备" Width="1440" Height="900" MinWidth="1000" MinHeight="700" WindowStartupLocation="CenterScreen">
  <Grid x:Name="Shell" />
</Window>

```

## src/MatrixDevice.Desktop/MainWindow.xaml.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Process> screenProcesses = [];
    private readonly List<NativeWindowHost> nativeHosts = [];
    private readonly Dictionary<NodeRow, TreeViewItem> nodeTreeItems = [];
    private readonly Dictionary<NodeRow, Rectangle> nodeRectangles = [];
    private readonly DispatcherTimerAdapter captureTimer;
    private readonly DispatcherTimerAdapter directoryTimer;
    private TabControl tabs = null!;
    private TreeView nodeTree = null!;
    private Canvas nodeCanvas = null!;
    private ScrollViewer screenshotScroll = null!;
    private Image screenshotImage = null!;
    private Canvas screenshotCanvas = null!;
    private Rectangle cropRectangle = null!;
    private ComboBox screenshotMode = null!;
    private ScaleTransform screenshotScale = new(1, 1);
    private Int32Rect? crop;
    private Point? cropStart;
    private Point? gestureStart;
    private DeviceRow? gestureDevice;
    private long gestureGeneration;
    private long gestureDeviceGeneration;
    private IReadOnlyList<string> gestureTargets = [];
    private readonly Dictionary<string, long> gestureTargetGenerations = [];
    private int shellHistoryCursor = -1;
    private bool closing;
    private bool closeApproved;
    private bool captureInProgress;
    private Task? captureOperation;
    private int captureCursor;
    private bool autoCapture = true;
    public WorkspaceViewModel ViewModel { get; }

    public MainWindow(WorkspaceViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
        LocalizationService.Current.SetLanguage(viewModel.Runtime.Settings.Language);
        Shell.Background = new SolidColorBrush(Color.FromRgb(247, 249, 252));
        Language = System.Windows.Markup.XmlLanguage.GetLanguage(viewModel.Runtime.Settings.Language);
        Width = viewModel.Runtime.Settings.WindowWidth;
        Height = viewModel.Runtime.Settings.WindowHeight;
        Title = viewModel.Demo ? "矩阵设备 · 演示模式" : "矩阵设备";
        viewModel.DeviceChanged = RefreshScreens;
        viewModel.NodesChanged = RebuildNodes;
        viewModel.NodeSelectionChanged = HighlightNode;
        viewModel.WallChanged = RefreshWall;
        BuildShell();
        LocalizationService.Current.LanguageChanged += OnLanguageChanged;
        captureTimer = new DispatcherTimerAdapter(TimeSpan.FromSeconds(5), CaptureTickAsync);
        directoryTimer = new DispatcherTimerAdapter(TimeSpan.FromSeconds(5), UpdateDeviceDirectoryAsync);
        if (!viewModel.Demo) { directoryTimer.Start(); captureTimer.Start(); }
        Closing += WindowClosing;
        Deactivated += async (_, _) =>
        {
            gestureStart = null; gestureDevice = null;
            if (closing || closeApproved) return;
            try { await ViewModel.Runtime.Input.ReleaseAsync(); }
            catch (Exception ex) { await ViewModel.ExecuteAsync("释放输入", () => Task.FromException(ex)); }
        };
        viewModel.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(WorkspaceViewModel.DebugImage)) RefreshScreens(); };
    }

    private void BuildShell()
    {
        BuildWorkspaceLayout();
        tabs = new TabControl { Background = Brushes.White, BorderThickness = new Thickness(0), Template = WorkspaceContentTemplate() };
        InitializeUnifiedDevices();
        AddTab("设备墙 / 主控", BuildWallPage());
        AddTab("雷电实例", BuildLdPlayerPage());
        AddTab("节点", BuildNodesPage());
        AddTab("截图 / 取色", BuildScreenshotPage());
        AddTab("Shell", BuildShellPage());
        AddTab("应用", BuildAppsPage());
        AddTab("文件", BuildFilesPage());
        AddTab("文本", BuildTextPage());
        AddTab("脚本", BuildScriptPage());
        AddTab("API", BuildApiPage());
        AddTab("扩展", BuildExtensionsPage());
        AddTab("设置", BuildSettingsPage());
        tabs.SelectionChanged += (_, args) => { if (ReferenceEquals(args.Source, tabs)) { UpdateNavigation(); RebuildNodes(); } };
        AttachWorkspacePages();
        NavigateTo("设备墙 / 主控");
    }












    private void AddTab(string title, FrameworkElement content)
    {
        System.Windows.Documents.TextElement.SetForeground(content, WorkspaceBrush("#182239"));
        System.Windows.Documents.TextElement.SetFontWeight(content, FontWeights.Normal);
        var tab = new TabItem { Content = content, Tag = title };
        LocalizationService.Bind(tab, HeaderedContentControl.HeaderProperty, title);
        tabs.Items.Add(tab);
    }
    private static DockPanel Page() => new() { Margin = new Thickness(14), LastChildFill = true };
    private static WrapPanel Toolbar() => new() { Orientation = Orientation.Horizontal };
    private static TextBlock Label(string text)
    {
        var label = new TextBlock { FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 6, 0, 7) };
        LocalizationService.Bind(label, TextBlock.TextProperty, text);
        return label;
    }
    private static TextBlock BoundText(string binding, bool wrap = false) { var text = new TextBlock { VerticalAlignment = VerticalAlignment.Center, TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap }; text.SetBinding(TextBlock.TextProperty, new Binding(binding)); return text; }
    private static Binding TwoWay(string path) => new(path) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, ValidatesOnExceptions = true, ValidatesOnDataErrors = true };
    private static TextBox BoundInput(string binding, bool readOnly = false, bool multiline = false) { var input = new TextBox { IsReadOnly = readOnly, AcceptsReturn = multiline, TextWrapping = multiline ? TextWrapping.Wrap : TextWrapping.NoWrap, VerticalScrollBarVisibility = multiline ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden, VerticalContentAlignment = multiline ? VerticalAlignment.Top : VerticalAlignment.Center }; input.SetBinding(TextBox.TextProperty, readOnly ? new Binding(binding) { Mode = BindingMode.OneWay } : TwoWay(binding)); return input; }
    private static CheckBox Check(string text, string binding) { var checkbox = new CheckBox(); LocalizationService.Bind(checkbox, ContentControl.ContentProperty, text); checkbox.SetBinding(ToggleButton.IsCheckedProperty, TwoWay(binding)); return checkbox; }
    private static FrameworkElement Field(string label, FrameworkElement control, double labelWidth = 155)
    {
        var row = new Grid { Margin = new Thickness(0, 0, 0, 9) }; row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(labelWidth) }); row.ColumnDefinitions.Add(new ColumnDefinition()); var text = new TextBlock { VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap }; LocalizationService.Bind(text, TextBlock.TextProperty, label); row.Children.Add(text); Grid.SetColumn(control, 1); row.Children.Add(control); return row;
    }
    private static Button Button(string glyph, string title, ICommand command)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal }; panel.Children.Add(new TextBlock { Text = glyph, FontFamily = new FontFamily("Segoe MDL2 Assets"), Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center }); var caption = new TextBlock { VerticalAlignment = VerticalAlignment.Center }; LocalizationService.Bind(caption, TextBlock.TextProperty, title); panel.Children.Add(caption); var button = new Button { Content = panel, Command = command }; LocalizationService.Bind(button, ToolTipProperty, title); return button;
    }
    private Button Button(string glyph, string title, Action action) => Button(glyph, title, new ActionCommand(action));
    private Button Button(string glyph, string title, Func<Task> action, bool write = false, Func<bool>? canExecute = null)
    {
        var command = new AsyncCommand(async () => { try { await action(); } catch (Exception ex) { MessageBox.Show(this, ViewModel.PresentError(ex), title, MessageBoxButton.OK, MessageBoxImage.Error); } }, canExecute ?? (write ? ViewModel.CanWriteDevice : () => !ViewModel.Busy));
        ViewModel.PropertyChanged += (_, _) => command.Refresh(); return Button(glyph, title, command);
    }
    private static void TextColumn(DataGrid grid, string header, string path, double width, bool readOnly = true)
    {
        BindingBase binding = readOnly ? new Binding(path) : TwoWay(path);
        if (path is nameof(DeviceRow.ApiAddress) or nameof(DeviceRow.ScriptState) or nameof(DeviceRow.State))
        {
            var stateBinding = new MultiBinding { Converter = new StateCaptionConverter() };
            stateBinding.Bindings.Add(new Binding(path)); stateBinding.Bindings.Add(new Binding(nameof(LocalizationService.Language)) { Source = LocalizationService.Current });
            binding = stateBinding;
        }
        var column = new DataGridTextColumn { Binding = binding, Width = width, IsReadOnly = readOnly };
        LocalizationService.Bind(column, DataGridColumn.HeaderProperty, header);
        grid.Columns.Add(column);
    }
    private static DataTemplate CaptionChoices()
    {
        var caption = new FrameworkElementFactory(typeof(TextBlock));
        var binding = new MultiBinding { Converter = new CaptionChoiceConverter() };
        binding.Bindings.Add(new Binding("."));
        binding.Bindings.Add(new Binding(nameof(LocalizationService.Language)) { Source = LocalizationService.Current });
        caption.SetBinding(TextBlock.TextProperty, binding);
        return new DataTemplate { VisualTree = caption };
    }
    private void OnLanguageChanged(object? sender, EventArgs args)
    {
        Language = System.Windows.Markup.XmlLanguage.GetLanguage(LocalizationService.Current.Language);
        ViewModel.Settings.Language = LocalizationService.Current.Language;
        ViewModel.Settings.Changed(nameof(SettingsEditor.Language));
        Title = LocalizationService.Current.Get("矩阵设备");
        foreach (var row in ViewModel.Devices) row.Changed(nameof(DeviceRow.FrameLabel));
    }

    private void RefreshWall() { UpdateUnifiedDirectory(); }
    private void RefreshScreens()
    {
        if (screenshotImage is null) return;
        screenshotImage.Source = ViewModel.DebugImage;
        if (ViewModel.DebugImage is { } image)
        {
            screenshotImage.Width = screenshotCanvas.Width = image.PixelWidth;
            screenshotImage.Height = screenshotCanvas.Height = image.PixelHeight;
        }
        else { screenshotImage.Width = screenshotCanvas.Width = 0; screenshotImage.Height = screenshotCanvas.Height = 0; }
        crop = null; cropStart = null; cropRectangle.Visibility = Visibility.Collapsed;
        RebuildNodes();
    }





    private async Task CaptureTickAsync()
    {
        if (!autoCapture || captureInProgress || closing || ldBusy || ViewModel.HasActiveOperations || directoryOperation is { IsCompleted: false } || tabs.SelectedIndex != 0 || !IsActive) return;
        var rows = ViewModel.DeviceView.Cast<DeviceRow>().Where(row => row.Info.Online).ToArray();
        if (rows.Length == 0) return;
        var targets = rows.Skip(captureCursor % rows.Length).Concat(rows.Take(captureCursor % rows.Length)).Take(4).Select(row => row.Serial).ToArray();
        captureCursor += targets.Length;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(directoryCancellation.Token); deadline.CancelAfter(TimeSpan.FromSeconds(20));
        captureInProgress = true;
        try { captureOperation = ViewModel.RefreshAutomaticScreensAsync(targets, deadline.Token); await captureOperation; }
        catch (OperationCanceledException) { }
        finally { captureInProgress = false; }
    }

    private async void WindowClosing(object? sender, CancelEventArgs e)
    {
        if (closeApproved) return;
        e.Cancel = true;
        if (closing) return;
        if (ldBusy || ViewModel.Busy) { MessageBox.Show(this, "当前操作尚未结束，请等待操作完成后退出。", "操作进行中", MessageBoxButton.OK, MessageBoxImage.Information); return; }
        if (ViewModel.HasActiveOperations && !ViewModel.Confirm("退出矩阵设备", "仍有脚本或文件传输正在运行。停止这些任务并退出？\n设备与模拟器将保持运行。")) return;
        closing = true;
        directoryCancellation.Cancel();
        directoryTimer.Stop(); captureTimer.Stop();
        if (directoryOperation is not null) { try { await directoryOperation; } catch (OperationCanceledException) { } }
        if (captureOperation is not null) { try { await captureOperation; } catch (OperationCanceledException) { } }
        try
        {
            scanCancellation?.Cancel();
            if (scanOperation is not null) { try { await scanOperation; } catch (OperationCanceledException) { } }
            foreach (var host in nativeHosts.ToList()) host.Restore();
            if (ldWindowVisibility is not null) await ldWindowVisibility.RestoreAllAsync();
            var saved = ViewModel.Runtime.Settings with { WindowWidth = WindowState == WindowState.Normal ? ActualWidth : RestoreBounds.Width, WindowHeight = WindowState == WindowState.Normal ? ActualHeight : RestoreBounds.Height, WallColumns = ViewModel.WallColumns, SyncMouse = ViewModel.SyncMouse, SyncKeyboard = ViewModel.SyncKeyboard };
            ViewModel.Runtime.Settings = ViewModel.Runtime.SettingsStore.Save(saved);
            await ViewModel.ShutdownAsync();
            LocalizationService.Current.LanguageChanged -= OnLanguageChanged;
            foreach (var process in screenProcesses.Values) { if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); } process.Dispose(); }
            closeApproved = true; Close();
        }
        catch (Exception ex) { closing = false; MessageBox.Show(this, "退出未完成：" + ViewModel.PresentError(ex) + "\n投屏恢复记录已保留。", "退出失败", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    internal void AbortStartup()
    {
        closing = closeApproved = true;
        captureTimer?.Stop();
        directoryTimer?.Stop(); directoryCancellation.Cancel();
        scanCancellation?.Cancel();
        Close();
    }

    private void ShowText(string title, string text)
    {
        var window = new Window { Owner = this, Title = title, Width = 700, Height = 500, Content = new TextBox { Text = text, IsReadOnly = true, AcceptsReturn = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new FontFamily("Consolas"), Padding = new Thickness(12) }, WindowStartupLocation = WindowStartupLocation.CenterOwner }; window.Show();
    }

    public async Task RunSmokeAsync(string? screenshotPath = null)
    {
        if (!ViewModel.Demo) throw new InvalidOperationException("启动验收仅允许显式演示模式");
        if (tabs.SelectedIndex != 0 || wallViewMode != "list") throw new InvalidOperationException("初始页面必须是统一设备列表。");
        if (screenshotPath is not null)
        {
            if (!System.IO.Path.IsPathFullyQualified(screenshotPath)) throw new InvalidOperationException("截图输出必须为绝对路径");
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(screenshotPath)!);
        }
        await ViewModel.RefreshWallAsync();
        if (ViewModel.CurrentDevice is { } row) await ViewModel.SetMainAsync(row);
        await ViewModel.LoadNodesAsync();
        ViewModel.SelectedNode = ViewModel.FlattenNodes().FirstOrDefault(node => node.Source.Clickable);
        await ViewModel.CaptureAsync();
        if (ViewModel.Nodes.Count != 0 || ViewModel.SelectedNode is not null)
            throw new InvalidOperationException("单独刷新截图后仍保留旧节点覆盖框与选中节点");
        if (nodeTree.Items.Count != 0 || nodeRectangles.Count != 0) throw new InvalidOperationException("旧节点视觉覆盖未清空");
        await ViewModel.LoadNodesAsync();
        ViewModel.SelectedNode = ViewModel.FlattenNodes().FirstOrDefault(node => node.Source.Clickable);
        await ViewModel.RefreshWallAsync();
        if (ViewModel.Nodes.Count != 0 || ViewModel.SelectedNode is not null)
            throw new InvalidOperationException("设备墙刷新截图后仍保留旧节点");
        await ViewModel.LoadNodesAsync();
        ViewModel.SelectedNode = ViewModel.FlattenNodes().FirstOrDefault(node => node.Source.Clickable);
        await ViewModel.LoadAppsAsync();
        await VerifyOperationGatesAsync();
        await VerifyOneClickProjectionAsync();
        await LdWindowVisibilitySmoke.VerifyAsync(System.IO.Path.Combine(ViewModel.DataDirectory, "window-visibility-smoke"));
        ViewModel.SamplePixel(10, 10);
        ViewModel.ShellInput = "wm size"; await ViewModel.ExecuteShellAsync();
        var acceptanceDirectory = ViewModel.DataDirectory;
        var scriptPath = System.IO.Path.Combine(acceptanceDirectory, "desktop-smoke.jxscript");
        MatrixDevice.Features.ScriptPackageService.Save(scriptPath, new MatrixDevice.Features.ScriptPackage
        {
            Manifest = new MatrixDevice.Features.ScriptManifest { Id = "desktop-smoke", Name = "桌面演示验收", Version = "1.0" },
            Steps = [new MatrixDevice.Features.ScriptStep { Action = "wait", DurationMs = 20 }, new MatrixDevice.Features.ScriptStep { Action = "click", Value = "80,240" }],
            Parameters = [new MatrixDevice.Features.ScriptParameter { Key = "limit", Type = "integer", Min = 1, Max = 100, DefaultValue = JsonSerializer.SerializeToElement(2) }]
        });
        await ViewModel.ImportScriptAsync(scriptPath);
        ViewModel.Parameters.Single(parameter => parameter.Key == "limit").Value = "3";
        await ViewModel.StartScriptAsync();
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (ViewModel.Runtime.Scripts.GetStatus(ViewModel.CurrentDevice!.Serial).State is MatrixDevice.Features.ScriptRunState.Running or MatrixDevice.Features.ScriptRunState.Cancelling)
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("演示脚本验收未在期限内结束");
            await Task.Delay(20);
        }
        if (ViewModel.Runtime.Scripts.GetStatus(ViewModel.CurrentDevice!.Serial).State != MatrixDevice.Features.ScriptRunState.Completed) throw new InvalidOperationException("演示脚本未成功完成");
        var source = ViewModel.CurrentDevice;
        ViewModel.CurrentDevice = ViewModel.Devices.First(device => !ReferenceEquals(device, source));
        ViewModel.CurrentDevice = source;
        if (ViewModel.Parameters.Single(parameter => parameter.Key == "limit").Value != "3") throw new InvalidOperationException("逐设备脚本参数未恢复");
        await ViewModel.LoadNodesAsync();
        ViewModel.SelectedNode = ViewModel.FlattenNodes().FirstOrDefault(node => node.Source.Clickable);
        await ViewModel.LoadAppsAsync();
        LocalizationService.Current.SetLanguage("zh-CN");
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        var settingsTab = tabs.Items.Cast<TabItem>().Single(tab => Equals(tab.Tag, "设置"));
        var ldTab = tabs.Items.Cast<TabItem>().Single(tab => Equals(tab.Tag, "雷电实例"));
        if ((tabs.Items[0] as TabItem)?.Header?.ToString() != "设备墙 / 主控" || settingsTab.Header?.ToString() != "设置" || ldTab.Header?.ToString() != "雷电实例") throw new InvalidOperationException("简体中文导航未生效");
        tabs.SelectedItem = settingsTab;
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        var labels = VisualDescendants<TextBlock>(Shell).Select(text => text.Text).ToHashSet(StringComparer.Ordinal);
        foreach (var expected in new[] { "普通接口端口", "界面语言", "连接与接口", "雷电命令行路径", "简体中文" })
            if (!labels.Contains(expected)) throw new InvalidOperationException("中文设置字段缺失：" + expected);
        var unsupportedFields = VisualDescendants<TextBox>(Shell).Where(input => BindingOperations.GetBinding(input, TextBox.TextProperty)?.Path.Path is "Settings.MiniMaxSize" or "Settings.MiniMaxFps" or "Settings.MiniBitRate").ToArray();
        if (unsupportedFields.Length != 3 || unsupportedFields.Any(input => input.IsEnabled)) throw new InvalidOperationException("未实现的小屏视频参数必须禁用");
        var hardwareField = VisualDescendants<CheckBox>(Shell).Single(input => BindingOperations.GetBinding(input, ToggleButton.IsCheckedProperty)?.Path.Path == "Settings.ForceHardwareDecode");
        if (hardwareField.IsEnabled) throw new InvalidOperationException("未实现的硬件解码控件必须禁用");
        await RefreshLdPlayerUiAsync();
        if (ldRows.Count != 2) throw new InvalidOperationException("演示雷电实例目录不完整");
        if (tabs.Items.Count != 12) throw new InvalidOperationException("桌面页面数量不完整");
        if (navigation.Count != 11 || navigation.ContainsKey("雷电实例") || deviceModes.Count != 4) throw new InvalidOperationException("设备管理统一入口不完整");
        NavigateTo("设备墙 / 主控");
        var removedLabels = new[] { "刷新设备", "扫描设备", "刷新截图" };
        if (VisualDescendants<TextBlock>(Shell).Any(text => removedLabels.Contains(text.Text))) throw new InvalidOperationException("旧重复刷新入口仍可见");
        ldPackage.Text = "com.example.demo"; ldText.Text = "中文演示";
        ldGrid.SelectedItem = ldRows.Single(row => !row.Instance.AndroidStarted);
        var runningOnly = new[] { "启动应用", "停止应用", "安装应用", "发送文字", "主页", "返回", "增大音量", "减小音量" };
        if (!ldCommandsByLabel["启动并连接"].CanExecute(null) || runningOnly.Any(label => ldCommandsByLabel[label].CanExecute(null))) throw new InvalidOperationException("未启动实例的操作状态不正确");
        ldGrid.SelectedItem = ldRows.Single(row => row.Instance.AndroidStarted);
        if (!ldCommandsByLabel["启动并连接"].CanExecute(null) || runningOnly.Any(label => !ldCommandsByLabel[label].CanExecute(null))) throw new InvalidOperationException("已启动实例的连接入口或操作状态不正确");
        ldBusy = true; ldFeedback.Text = "正在执行所选实例命令…"; RefreshLdButtons();
        if (ldCommands.Any(command => command.CanExecute(null)) || !ldFeedback.Text.Contains("正在执行", StringComparison.Ordinal)) throw new InvalidOperationException("雷电忙状态未锁定操作");
        ldBusy = false; RefreshLdButtons();
        var originalConfirm = ViewModel.Confirm;
        var confirmationCount = 0; var beforeConfirmation = ldSelection!.Generation;
        ViewModel.Confirm = (title, message) =>
        {
            if (!title.Contains("雷电实例", StringComparison.Ordinal) || !message.Contains("会话代次", StringComparison.Ordinal)) throw new InvalidOperationException("雷电确认缺少实例作用范围");
            confirmationCount++; return false;
        };
        try { await ExecuteLdPlayerAsync("quit"); await ExecuteLdPlayerAsync("reboot"); }
        finally { ViewModel.Confirm = originalConfirm; }
        if (confirmationCount != 2 || ldSelection.Generation != beforeConfirmation || !ldSelection.Instance.AndroidStarted || ldFeedback.Text != "操作已取消，未发送命令。") throw new InvalidOperationException("雷电关闭或重启的取消确认未生效");
        UpdateLdPlayerRows();
        if (!ldContext.Text.Contains("当前实例", StringComparison.Ordinal)) throw new InvalidOperationException("雷电实例选择上下文缺失");
        var currentBeforeNavigation = ViewModel.CurrentDevice;
        var mainBeforeNavigation = ViewModel.MainDevice;
        ViewModel.ShellInput = "echo navigation-state"; ViewModel.TextInput = "跨页面保留输入";
        var selectedBeforeNavigation = ViewModel.Targets().Select(device => device.Serial).ToArray();
        crop = new Int32Rect(10, 10, 40, 40); cropRectangle.Visibility = Visibility.Visible;
        cropRectangle.Width = cropRectangle.Height = 40; Canvas.SetLeft(cropRectangle, 10); Canvas.SetTop(cropRectangle, 10);
        foreach (var route in WorkspacePages)
        {
            if (route.Tag == "雷电实例") deviceModes.First(mode => mode.Tag == route.Tag).Button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            else navigation[route.Tag].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            var sidebarTag = route.Tag == "雷电实例" ? "设备墙 / 主控" : route.Tag;
            if (!Equals((tabs.SelectedItem as TabItem)?.Tag, route.Tag) || navigation.Count(item => item.Value.IsChecked == true) != 1 || navigation[sidebarTag].IsChecked != true)
                throw new InvalidOperationException("侧栏导航或选中态失效：" + route.Label);
        }
        if (!ReferenceEquals(currentBeforeNavigation, ViewModel.CurrentDevice) || !ReferenceEquals(mainBeforeNavigation, ViewModel.MainDevice) ||
            !selectedBeforeNavigation.SequenceEqual(ViewModel.Targets().Select(device => device.Serial)) || ViewModel.ShellInput != "echo navigation-state" || ViewModel.TextInput != "跨页面保留输入" ||
            crop != new Int32Rect(10, 10, 40, 40) || ViewModel.Parameters.Single(parameter => parameter.Key == "limit").Value != "3")
            throw new InvalidOperationException("跨页面导航丢失设备、目标、输入、裁剪或脚本参数");
        SetSidebarCollapsed(true);
        NavigateTo("设备墙 / 主控");
        if (sidebarColumn.Width.Value != 72 || sidebarCaptions.Any(caption => caption.Visibility != Visibility.Collapsed)) throw new InvalidOperationException("侧栏折叠未生效");
        SetSidebarCollapsed(false);
        foreach (var rowToSelect in ViewModel.Devices) rowToSelect.IsSelected = true;
        ViewModel.UpdateSelection();
        foreach (var mode in new[] { "cards", "screens", "list" })
        {
            ViewModel.Search = "";
            SetWallViewMode(mode);
            await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
            foreach (var search in new[] { ViewModel.Devices[1].Serial, "不存在的演示设备", "" })
            {
                ViewModel.Search = search;
                await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
                if (!ReferenceEquals(ViewModel.CurrentDevice, currentBeforeNavigation) || !ReferenceEquals(ViewModel.MainDevice, mainBeforeNavigation) || ViewModel.Targets().Count != 2)
                    throw new InvalidOperationException($"激活视图筛选改变了上下文：视图={mode}，筛选={search}，当前={ViewModel.CurrentDevice?.Serial}，主控={ViewModel.MainDevice?.Serial}，目标={ViewModel.Targets().Count}");
            }
        }
        SetWallViewMode("cards");
        ViewModel.Search = ViewModel.Devices[1].Serial;
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (ViewModel.DeviceView.Cast<DeviceRow>().Count() != 1 || ViewModel.Targets().Count != 2 || !ReferenceEquals(ViewModel.CurrentDevice, currentBeforeNavigation) || !ReferenceEquals(ViewModel.MainDevice, mainBeforeNavigation))
            throw new InvalidOperationException($"筛选改变了上下文：可见={ViewModel.DeviceView.Cast<DeviceRow>().Count()}，目标={ViewModel.Targets().Count}，当前={ViewModel.CurrentDevice?.Serial ?? "未选择"}，主控={ViewModel.MainDevice?.Serial ?? "未指定"}");
        ClearWallSelection();
        if (ViewModel.Devices.Any(device => device.IsSelected)) throw new InvalidOperationException("清空没有取消隐藏设备的选择");
        ViewModel.Search = "不存在的演示设备";
        SetWallViewMode("cards");
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (ViewModel.DeviceView.Cast<DeviceRow>().Any()) throw new InvalidOperationException("空目录筛选失效");
        if (screenshotPath is not null) SaveRender(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(screenshotPath)!, System.IO.Path.GetFileNameWithoutExtension(screenshotPath) + "-empty.png"));
        ViewModel.Search = "";
        if (ViewModel.DebugImage is not { PixelWidth: > 0 }) throw new InvalidOperationException("视图筛选丢失了当前截图");
        foreach (var mode in new[] { "cards", "screens", "list" })
        {
            wallViewChoices[mode].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
            if (WallViewMode != mode || wallViewChoices.Count(choice => choice.Value.IsChecked == true) != 1 || !ReferenceEquals(ViewModel.CurrentDevice, currentBeforeNavigation) || !ReferenceEquals(ViewModel.MainDevice, mainBeforeNavigation))
                throw new InvalidOperationException("设备视图切换丢失上下文：" + mode);
            if (screenshotPath is not null) SaveRender(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(screenshotPath)!, System.IO.Path.GetFileNameWithoutExtension(screenshotPath) + "-" + mode + ".png"));
        }
        SetWallViewMode("screens");
        var expandedControls = VisualDescendants<Expander>(wallMainPanel).Single(); expandedControls.IsExpanded = true;
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (wallMainPanel is ScrollViewer mainScroll) mainScroll.ScrollToEnd();
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (screenshotPath is not null) SaveRender(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(screenshotPath)!, System.IO.Path.GetFileNameWithoutExtension(screenshotPath) + "-expanded.png"));
        expandedControls.IsExpanded = false;
        SetWallViewMode("cards");
        var groupChoice = VisualDescendants<ComboBox>(Shell).First(input => BindingOperations.GetBinding(input, Selector.SelectedItemProperty)?.Path.Path == nameof(WorkspaceViewModel.GroupFilter));
        groupChoice.IsDropDownOpen = true;
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (groupChoice.Template.FindName("PART_Popup", groupChoice) is not Popup { IsOpen: true }) throw new InvalidOperationException("分组下拉模板未打开");
        groupChoice.SelectedItem = "未分组";
        if (ViewModel.GroupFilter != "未分组") throw new InvalidOperationException("分组下拉绑定失效");
        groupChoice.IsDropDownOpen = false; groupChoice.SelectedItem = "全部分组";
        ShowDeviceDirectory();
        ViewModel.Search = ViewModel.Devices[1].Serial;
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (!ReferenceEquals(ViewModel.CurrentDevice, currentBeforeNavigation) || !ReferenceEquals(ViewModel.MainDevice, mainBeforeNavigation)) throw new InvalidOperationException("设备目录筛选改变了当前设备或主控");
        ViewModel.Search = ""; directoryWindow!.Hide();
        ShowScanner();
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        if (screenshotPath is not null) SaveRender(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(screenshotPath)!, System.IO.Path.GetFileNameWithoutExtension(screenshotPath) + "-scanner.png"), (FrameworkElement)scannerWindow!.Content);
        scannerWindow!.Hide();
        if (ViewModel.DebugImage is { } frame && (frame.PixelWidth < 1 || frame.PixelHeight < 1)) throw new InvalidOperationException("演示截图为空");
        if (screenshotPath is not null)
        {
            var directory = System.IO.Path.GetDirectoryName(screenshotPath)!; Directory.CreateDirectory(directory);
            await ViewModel.SaveScreenshotAsync(System.IO.Path.Combine(directory, "desktop-crop.png"), new Int32Rect(10, 10, 40, 40));
            await ViewModel.ExportNodesAsync(System.IO.Path.Combine(directory, "desktop-nodes.json"));
        }
        var controlStates = new List<object>();
        foreach (TabItem tab in tabs.Items)
        {
            NavigateTo(tab.Tag.ToString()!);
            await Dispatcher.InvokeAsync(() => { UpdateLayout(); }, System.Windows.Threading.DispatcherPriority.Render);
            var pageName = tab.Tag.ToString()!;
            controlStates.AddRange(CaptureControlStates(pageName, "idle", Shell));
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var busyOperation = ViewModel.ExecuteAsync("控件忙状态检查", () => release.Task);
            try
            {
                await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
                controlStates.AddRange(CaptureControlStates(pageName, "busy", Shell));
            }
            finally { release.SetResult(); await busyOperation; }
            var current = ViewModel.CurrentDevice!; var originalInfo = current.Info;
            try
            {
                current.Info = originalInfo with { State = "Offline" }; ViewModel.UpdateSelection();
                await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
                controlStates.AddRange(CaptureControlStates(pageName, "offline", Shell));
            }
            finally { current.Info = originalInfo; ViewModel.UpdateSelection(); }
            if (Equals(tab.Tag, "扩展"))
            {
                var settingsScroll = VisualDescendants<ScrollViewer>(Shell).Single(scroll => scroll.Name == "ExtensionSettingsScroll");
                settingsScroll.ScrollToEnd();
                await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
                var timezoneAction = VisualDescendants<Button>(settingsScroll).Single(button => button.ToolTip as string == "应用并核验时区");
                var point = timezoneAction.TransformToAncestor(settingsScroll).Transform(new Point());
                if (point.Y < 0 || point.Y + timezoneAction.ActualHeight > settingsScroll.ActualHeight + 2) throw new InvalidOperationException("扩展页时区按钮滚动后仍不可达");
            }
            if (screenshotPath is not null)
            {
                if (!System.IO.Path.IsPathFullyQualified(screenshotPath)) throw new InvalidOperationException("截图输出必须为绝对路径");
                var directory = System.IO.Path.GetDirectoryName(screenshotPath)!;
                Directory.CreateDirectory(directory);
                var output = System.IO.Path.Combine(directory, System.IO.Path.GetFileNameWithoutExtension(screenshotPath) + $"-tab{tabs.SelectedIndex}.png");
                SaveRender(output);
            }
        }
        if (screenshotPath is not null)
            await File.WriteAllTextAsync(System.IO.Path.ChangeExtension(screenshotPath, ".controls.json"), JsonSerializer.Serialize(controlStates, JsonDefaults.Options));
        NavigateTo("设备墙 / 主控");
        await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
        await Dispatcher.InvokeAsync(() => { UpdateLayout(); }, System.Windows.Threading.DispatcherPriority.Render);
        var tiles = VisualDescendants<UnifiedDeviceTile>(Shell).Where(tile => tile.IsVisible).ToArray();
        if (screenshotPath is not null) await File.WriteAllTextAsync(System.IO.Path.ChangeExtension(screenshotPath, ".tiles.json"), JsonSerializer.Serialize(tiles.Select(tile => new { tile.Mode, owner = tile.Owner is not null, context = tile.DataContext?.GetType().Name, child = tile.Child?.GetType().Name, tile.ActualWidth, tile.ActualHeight }), JsonDefaults.Options));
        if (wallViewMode != "list" && !unifiedDeviceView.IsEmpty && tiles.Length == 0) throw new InvalidOperationException("非空统一目录没有生成可见卡片。");
        if (tiles.Any(tile => tile.Child is null || tile.ActualHeight < 1)) throw new InvalidOperationException("统一设备视图出现空白条目。");
        if (screenshotPath is not null) SaveRender(screenshotPath);
        var path = System.IO.Path.Combine(ViewModel.DataDirectory, "desktop-smoke.json");
        if (!string.IsNullOrWhiteSpace(ViewModel.Error)) throw new InvalidOperationException("桌面验收产生错误：" + ViewModel.Error);
        var report = JsonSerializer.Serialize(new { width = ActualWidth, height = ActualHeight, deviceCount = ViewModel.Devices.Count, ldPlayerInstances = ldRows.Count, ldStateButtonsVerified = true, ldBusyVerified = true, ldConfirmationCancellationVerified = confirmationCount == 2, navigationVerified = true, navigationStatePreserved = true, sidebarCollapseVerified = true, wallModesVerified = true, filteredContextPreserved = true, clearHiddenSelectionVerified = true, emptyStateVerified = true, groupDropdownVerified = true, directoryFilterContextPreserved = true, nodes = ViewModel.FlattenNodes().Count(), selectedNode = ViewModel.SelectedNode?.Id, apps = ViewModel.Apps.Count, colors = ViewModel.Colors.Count, shell = ViewModel.ShellHistory.Count, script = LocalizationService.Current.Get(ViewModel.Runtime.Scripts.GetStatus(ViewModel.CurrentDevice!.Serial).State.ToString()), tabs = tabs.Items.Count, language = "zh-CN", chineseInterfaceVerified = true, unavailableControlsDisabled = true, error = ViewModel.Error }, JsonDefaults.Options);
        await File.WriteAllTextAsync(path, report);
        if (screenshotPath is not null) await File.WriteAllTextAsync(System.IO.Path.ChangeExtension(screenshotPath, ".json"), report);
    }

    private void SaveRender(string path) => SaveRender(path, Shell);
    private static void SaveRender(string path, FrameworkElement surface)
    {
        var bitmap = new RenderTargetBitmap(Math.Max(1, (int)Math.Ceiling(surface.ActualWidth)), Math.Max(1, (int)Math.Ceiling(surface.ActualHeight)), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(surface);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap)); using var file = File.Create(path); encoder.Save(file);
    }
    private static IEnumerable<T> VisualDescendants<T>(DependencyObject source) where T : DependencyObject
    {
        if (source is T match) yield return match;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(source); index++)
            foreach (var nested in VisualDescendants<T>(VisualTreeHelper.GetChild(source, index))) yield return nested;
    }
}

public sealed class NonEmptyVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotSupportedException();
}

public sealed class DispatcherTimerAdapter
{
    private readonly System.Windows.Threading.DispatcherTimer timer;
    public DispatcherTimerAdapter(TimeSpan interval, Func<Task> tick)
    {
        timer = new System.Windows.Threading.DispatcherTimer { Interval = interval };
        timer.Tick += async (_, _) => await tick();
    }
    public void Start() => timer.Start();
    public void Stop() => timer.Stop();
}

```

## src/MatrixDevice.Desktop/MatrixDevice.Desktop.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <ApplicationManifest>app.manifest</ApplicationManifest>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../MatrixDevice.Host/MatrixDevice.Host.csproj" />
  </ItemGroup>
</Project>

```

## src/MatrixDevice.Desktop/NativeWindowHost.cs

```csharp
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public sealed class NativeWindowHost : HwndHost
{
    private const int GwlStyle = -16;
    private const int GwlExStyle = -20;
    private const long WsChild = 0x40000000;
    private const long WsPopup = 0x80000000;
    private const long WsCaption = 0x00C00000;
    private const long WsThickFrame = 0x00040000;
    private readonly Process ownedProcess;
    private readonly string journalPath;
    private WindowRecoveryRecord? record;
    private IntPtr container;
    public int ProcessId => ownedProcess.Id;
    public bool IsAttached { get; private set; }

    public NativeWindowHost(Process ownedProcess, string journalDirectory)
    {
        this.ownedProcess = ownedProcess;
        Directory.CreateDirectory(journalDirectory);
        journalPath = Path.Combine(journalDirectory, $"{ownedProcess.Id}-{ownedProcess.MainWindowHandle.ToInt64()}.json");
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        ownedProcess.Refresh();
        var window = ownedProcess.MainWindowHandle;
        ValidateOwner(window, ownedProcess.Id, ownedProcess.StartTime.ToUniversalTime().Ticks, ownedProcess.MainModule?.FileName ?? "");
        if (!GetWindowRect(window, out var bounds)) throw new Win32Exception(Marshal.GetLastWin32Error(), "无法读取投屏窗口位置");
        record = new WindowRecoveryRecord(window.ToInt64(), ownedProcess.Id, ownedProcess.StartTime.ToUniversalTime().Ticks, ownedProcess.MainModule?.FileName ?? "", GetParent(window).ToInt64(), GetWindowLongPtr(window, GwlStyle).ToInt64(), GetWindowLongPtr(window, GwlExStyle).ToInt64(), bounds.Left, bounds.Top, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top, IsWindowVisible(window));
        var temporary = journalPath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(record, JsonDefaults.Options));
        File.Move(temporary, journalPath, true);
        container = CreateWindowEx(0, "STATIC", "MatrixDevice.Host", WsChild, 0, 0, 1, 1, hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (container == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error(), "无法创建自有承载窗口");
        try
        {
            SetStyle(window, GwlStyle, (record.Style & ~(WsPopup | WsCaption | WsThickFrame)) | WsChild);
            SetCheckedParent(window, container);
            IsAttached = true;
            ShowWindow(window, 5);
            Layout(window, ActualWidth, ActualHeight);
            return new HandleRef(this, container);
        }
        catch
        {
            Restore();
            if (container != IntPtr.Zero) DestroyWindow(container);
            container = IntPtr.Zero;
            throw;
        }
    }

    protected override void OnWindowPositionChanged(Rect rcBoundingBox)
    {
        base.OnWindowPositionChanged(rcBoundingBox);
        if (IsAttached && record is not null) Layout(new IntPtr(record.Window), rcBoundingBox.Width, rcBoundingBox.Height);
    }

    private void Layout(IntPtr window, double width, double height)
    {
        var dpi = System.Windows.Media.VisualTreeHelper.GetDpi(this);
        if (!SetWindowPos(window, IntPtr.Zero, 0, 0, Math.Max(1, (int)(width * dpi.DpiScaleX)), Math.Max(1, (int)(height * dpi.DpiScaleY)), 0x0004 | 0x0010 | 0x0020)) throw new Win32Exception(Marshal.GetLastWin32Error(), "投屏窗口布局失败");
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        Restore();
        if (hwnd.Handle != IntPtr.Zero && !DestroyWindow(hwnd.Handle)) throw new Win32Exception(Marshal.GetLastWin32Error(), "自有承载窗口关闭失败");
        container = IntPtr.Zero;
    }

    public void Restore()
    {
        if (record is null) return;
        RestoreRecord(record);
        IsAttached = false;
        record = null;
        if (File.Exists(journalPath)) File.Delete(journalPath);
    }

    public static IReadOnlyList<string> RecoverPending(string journalDirectory)
    {
        if (!Directory.Exists(journalDirectory)) return [];
        var errors = new List<string>();
        foreach (var path in Directory.EnumerateFiles(journalDirectory, "*.json"))
        {
            try
            {
                var saved = JsonSerializer.Deserialize<WindowRecoveryRecord>(File.ReadAllText(path), JsonDefaults.Options) ?? throw new InvalidDataException("恢复记录为空");
                RestoreRecord(saved);
                File.Delete(path);
            }
            catch (Exception ex) { errors.Add($"{Path.GetFileName(path)}：{ex.Message}"); }
        }
        return errors;
    }

    private static void RestoreRecord(WindowRecoveryRecord saved)
    {
        var window = new IntPtr(saved.Window);
        ValidateOwner(window, saved.ProcessId, saved.ProcessStartUtcTicks, saved.ExecutablePath);
        var parent = new IntPtr(saved.Parent);
        if (parent != IntPtr.Zero && !IsWindow(parent)) throw new InvalidOperationException($"原父窗口已失效，保留恢复记录：{saved.Window}");
        SetCheckedParent(window, parent);
        SetStyle(window, GwlStyle, saved.Style);
        SetStyle(window, GwlExStyle, saved.ExStyle);
        var point = new WinPoint { X = saved.Left, Y = saved.Top };
        if (parent != IntPtr.Zero && !ScreenToClient(parent, ref point)) throw new Win32Exception(Marshal.GetLastWin32Error(), "恢复坐标换算失败");
        if (!SetWindowPos(window, IntPtr.Zero, point.X, point.Y, saved.Width, saved.Height, 0x0004 | 0x0010 | 0x0020)) throw new Win32Exception(Marshal.GetLastWin32Error(), "投屏窗口恢复位置失败");
        ShowWindow(window, saved.Visible ? 5 : 0);
        if (GetParent(window) != parent || GetWindowLongPtr(window, GwlStyle).ToInt64() != saved.Style) throw new InvalidOperationException("窗口恢复核验不一致");
    }

    private static void ValidateOwner(IntPtr window, int processId, long started, string executable)
    {
        if (!IsWindow(window)) throw new InvalidOperationException($"投屏窗口已失效：{window}");
        GetWindowThreadProcessId(window, out var actualId);
        if (actualId != processId) throw new InvalidOperationException("窗口 PID 已改变，拒绝修改");
        using var process = Process.GetProcessById(processId);
        if (process.StartTime.ToUniversalTime().Ticks != started || !string.Equals(process.MainModule?.FileName, executable, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("进程启动时间或路径不符，拒绝修改重用窗口");
    }

    private static void SetCheckedParent(IntPtr window, IntPtr parent)
    {
        Marshal.SetLastPInvokeError(0);
        var previous = SetParent(window, parent);
        var error = Marshal.GetLastWin32Error();
        if (previous == IntPtr.Zero && error != 0) throw new Win32Exception(error, "窗口父子关系修改失败");
        if (GetParent(window) != parent) throw new InvalidOperationException("窗口父子关系核验失败");
    }
    private static void SetStyle(IntPtr window, int index, long value)
    {
        Marshal.SetLastPInvokeError(0);
        var result = SetWindowLongPtr(window, index, new IntPtr(value));
        if (result == IntPtr.Zero && Marshal.GetLastWin32Error() != 0) throw new Win32Exception(Marshal.GetLastWin32Error(), "窗口样式修改失败");
    }

    public sealed record WindowRecoveryRecord(long Window, int ProcessId, long ProcessStartUtcTicks, string ExecutablePath, long Parent, long Style, long ExStyle, int Left, int Top, int Width, int Height, bool Visible);
    [StructLayout(LayoutKind.Sequential)] private struct WinRect { public int Left; public int Top; public int Right; public int Bottom; }
    [StructLayout(LayoutKind.Sequential)] private struct WinPoint { public int X; public int Y; }
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "CreateWindowExW")] private static extern IntPtr CreateWindowEx(int exStyle, string className, string title, long style, int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr parameter);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool DestroyWindow(IntPtr window);
    [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr SetParent(IntPtr window, IntPtr parent);
    [DllImport("user32.dll")] private static extern IntPtr GetParent(IntPtr window);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowLongPtrW", SetLastError = true)] private static extern IntPtr GetWindowLongPtr(IntPtr window, int index);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetWindowLongPtrW", SetLastError = true)] private static extern IntPtr SetWindowLongPtr(IntPtr window, int index, IntPtr value);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetWindowRect(IntPtr window, out WinRect rect);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetWindowPos(IntPtr window, IntPtr after, int x, int y, int width, int height, uint flags);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ShowWindow(IntPtr window, int command);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindow(IntPtr window);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out int processId);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool ScreenToClient(IntPtr window, ref WinPoint point);
}

```

## src/MatrixDevice.Desktop/packages.lock.json

```json
{
  "version": 1,
  "dependencies": {
    "net10.0-windows7.0": {
      "AdvancedSharpAdbClient": {
        "type": "Transitive",
        "resolved": "3.6.16",
        "contentHash": "w5p3s+9Rkx9tmn5S5KU0XSTap5YWGHOvQOw/40M0d7eGOHf7idhX+kizWPmnP1ahbkic/yiX03SMYNR1mJSAVw=="
      },
      "Microsoft.Data.Sqlite": {
        "type": "Transitive",
        "resolved": "10.0.12",
        "contentHash": "xUq9xUc653ue7ac2rMfCaf8UQN+U3zS8poWYlugycpvJllplMM0Mk7v/+MM0CTlu9zGvbGML4BPH8YWhrUOfGw==",
        "dependencies": {
          "Microsoft.Data.Sqlite.Core": "10.0.12",
          "SQLitePCLRaw.bundle_e_sqlite3": "2.1.12",
          "SQLitePCLRaw.core": "2.1.12"
        }
      },
      "Microsoft.Data.Sqlite.Core": {
        "type": "Transitive",
        "resolved": "10.0.12",
        "contentHash": "2aL9eL5HQr0V4b8V909SD+jQAx6qnTTl7sT/v3D8dIvmr2UGdpZIQxqm1pj2Js3e4EYTvpw0oMH7DjapJJuPPw==",
        "dependencies": {
          "SQLitePCLRaw.core": "2.1.12"
        }
      },
      "SkiaSharp": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "TghYLOUaP3q6rTjQw7U9uQaaXh8lEs9rfA1U7/VtmyAptVDtj/SmjtcWzFWmcC/VMWxe3QgbV9uyvqDL7ySFtQ==",
        "dependencies": {
          "SkiaSharp.NativeAssets.Win32": "4.153.1",
          "SkiaSharp.NativeAssets.macOS": "4.153.1"
        }
      },
      "SkiaSharp.NativeAssets.macOS": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "YTojWeWIhnpF7uC8vpMxB5wRONKkDB2t3r/nNPA3DL2pgj9fqMXRevA/rIanYazWnco4jo1ufgp3HkXuh1Tz8A=="
      },
      "SkiaSharp.NativeAssets.Win32": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "Yqg6rChTVh0BUE8QQxtOA3uUBnihfgID4yex/AvDa0+os2bVcBjmI3Qj9tnOB+R1hd/Jry1Gwl8qw5JI9HZG8Q=="
      },
      "SQLitePCLRaw.bundle_e_sqlite3": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "mAgscpQMLw5/nfA1Q5oJVAT29yROUo1ifZGbbTpx/lwZpSxMUGoYbKfmvdm8oXER+RzxqBmmQzeBEVKfeHv2nw==",
        "dependencies": {
          "SQLitePCLRaw.lib.e_sqlite3": "2.1.12",
          "SQLitePCLRaw.provider.e_sqlite3": "2.1.12"
        }
      },
      "SQLitePCLRaw.core": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "ETpNw9DY3ckWLgRRAeCHj+GKOuPi61aeczkXhgHexUvqoZBAYg8RYESE2J7O1M7+o6QbdSEZwrw9bfqztUVWXg=="
      },
      "SQLitePCLRaw.lib.e_sqlite3": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "fWi8Dbknuhgg72fWinIdjXVaqO1hHL4YBBwVLnr7e1c9TAZwJ0QE38j9syW1hwx6HaqEVTwI+O07WPdZn8Rp0w=="
      },
      "SQLitePCLRaw.provider.e_sqlite3": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "W3oH4XIfCzFrgUSDKHhN6N+dgzA5YHOR2VxX8GB6Qy7CyrJJgxPEG8NirgYWlPQC5P2jz2knSsexWu4tDUL33g==",
        "dependencies": {
          "SQLitePCLRaw.core": "2.1.12"
        }
      },
      "matrixdevice.core": {
        "type": "Project",
        "dependencies": {
          "AdvancedSharpAdbClient": "[3.6.16, )",
          "SkiaSharp": "[4.153.1, )"
        }
      },
      "matrixdevice.features": {
        "type": "Project",
        "dependencies": {
          "MatrixDevice.Core": "[1.0.0, )"
        }
      },
      "matrixdevice.host": {
        "type": "Project",
        "dependencies": {
          "MatrixDevice.Features": "[1.0.0, )",
          "Microsoft.Data.Sqlite": "[10.0.12, )"
        }
      }
    }
  }
}
```

## src/MatrixDevice.Desktop/Presentation.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? property = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        Changed(property);
        return true;
    }
    public void Changed([CallerMemberName] string? property = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
}

public sealed class ActionCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => execute();
    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public sealed class AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
{
    private bool running;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => !running && (canExecute?.Invoke() ?? true);
    public async void Execute(object? parameter)
    { await ExecuteAsync(parameter); }
    public async Task ExecuteAsync(object? parameter = null)
    {
        if (!CanExecute(parameter)) return;
        running = true;
        Refresh();
        try { await execute(); }
        finally { running = false; Refresh(); }
    }
    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public sealed class DeviceRow : ObservableObject
{
    private DeviceInfo info;
    private bool selected;
    private BitmapSource? screenshot;
    private string screenState = "未载入";
    private string automationState = "未连接";
    private string scriptState = "未运行";
    private string apiAddress = "未启动";
    private DateTimeOffset? capturedAt;
    public DeviceRow(DeviceInfo info) => this.info = info;
    public DeviceInfo Info { get => info; set { info = value; Changed(); Changed(nameof(Name)); Changed(nameof(Model)); Changed(nameof(Group)); Changed(nameof(State)); Changed(nameof(Resolution)); } }
    public string Serial => info.Serial;
    public string Name => string.IsNullOrWhiteSpace(info.Name) ? info.Serial : info.Name;
    public string Model => $"{info.Brand} {info.Model}".Trim();
    public string Group => string.IsNullOrWhiteSpace(info.GroupId) ? "未分组" : info.GroupId;
    public string State => LocalizationService.Current.Get(info.State);
    public string Resolution => info.Width > 0 && info.Height > 0 ? $"{info.Width} × {info.Height}" : "未核验";
    public bool IsSelected { get => selected; set => Set(ref selected, value); }
    public BitmapSource? Screenshot { get => screenshot; set => Set(ref screenshot, value); }
    public string ScreenState { get => screenState; set => Set(ref screenState, value); }
    public string AutomationState { get => automationState; set => Set(ref automationState, value); }
    public string ScriptState { get => scriptState; set => Set(ref scriptState, value); }
    public string ApiAddress { get => apiAddress; set => Set(ref apiAddress, value); }
    public DateTimeOffset? CapturedAt { get => capturedAt; set { Set(ref capturedAt, value); Changed(nameof(FrameLabel)); } }
    public string FrameLabel => capturedAt is { } at ? $"{LocalizationService.Current.Get("截图")} {at.ToLocalTime():HH:mm:ss}" : LocalizationService.Current.Get("无截图");
}

public sealed class NodeRow(AndroidNode source, int depth = 0)
{
    public AndroidNode Source { get; } = source;
    public int Depth { get; } = depth;
    public string Label => string.IsNullOrEmpty(Source.Text) ? Source.ClassName?.Split('.').Last() ?? "node" : Source.Text;
    public string Id => Source.ResourceId ?? "";
    public string Bounds => $"[{Source.Bounds.Left},{Source.Bounds.Top}][{Source.Bounds.Right},{Source.Bounds.Bottom}]";
    public ObservableCollection<NodeRow> Children { get; } = new(source.ChildNodeList.Select(node => new NodeRow(node, depth + 1)));
}

public sealed record ShellEntry(int Index, string Time, string Command, string Output, string Serial = "")
{ public string Context => $"{Time} · {Serial}"; }
public sealed record PixelSample(int X, int Y, byte R, byte G, byte B)
{
    public string Hex => $"#{R:X2}{G:X2}{B:X2}";
    public string Rgb => $"RGB({R}, {G}, {B})";
    public string Position => $"{X}, {Y}";
}

public sealed class TransferRow : ObservableObject
{
    private double progress;
    private string state = "待传输";
    public required string Serial { get; init; }
    public required string Direction { get; init; }
    public required string LocalPath { get; init; }
    public required string RemotePath { get; init; }
    public bool AutoInstall { get; init; }
    public string Name => System.IO.Path.GetFileName(LocalPath);
    public string Size => FileSize(LocalPath);
    public double Progress { get => progress; set => Set(ref progress, value); }
    public string State { get => state; set => Set(ref state, value); }
    private static string FileSize(string path)
    {
        try { return System.IO.File.Exists(path) ? $"{new System.IO.FileInfo(path).Length / 1024d:F1} KiB" : "待下载"; }
        catch (Exception) { return "未核验"; }
    }
}

```

## src/MatrixDevice.Desktop/Themes/ModernTheme.xaml

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <SolidColorBrush x:Key="AccentBrush" Color="#3274F6" />
  <SolidColorBrush x:Key="AccentHoverBrush" Color="#2868E4" />
  <SolidColorBrush x:Key="AccentPressedBrush" Color="#215ACE" />
  <SolidColorBrush x:Key="AccentSoftBrush" Color="#EDF3FF" />
  <SolidColorBrush x:Key="CanvasBrush" Color="#F5F7FB" />
  <SolidColorBrush x:Key="SurfaceBrush" Color="#FFFFFF" />
  <SolidColorBrush x:Key="SurfaceHoverBrush" Color="#F6F8FC" />
  <SolidColorBrush x:Key="SurfacePressedBrush" Color="#EEF1F7" />
  <SolidColorBrush x:Key="BorderBrush" Color="#E4E8F0" />
  <SolidColorBrush x:Key="TextBrush" Color="#182239" />
  <SolidColorBrush x:Key="MutedBrush" Color="#63718A" />
  <SolidColorBrush x:Key="DisabledBrush" Color="#A4AEC0" />
  <SolidColorBrush x:Key="DangerBrush" Color="#D94A5C" />
  <SolidColorBrush x:Key="SuccessBrush" Color="#23A57A" />

  <Style x:Key="KeyboardFocusVisual">
    <Setter Property="Control.Template">
      <Setter.Value>
        <ControlTemplate>
          <Border BorderBrush="{StaticResource AccentBrush}" BorderThickness="1"
                  CornerRadius="7" Margin="-2" IsHitTestVisible="False" />
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style TargetType="Window">
    <Setter Property="FontFamily" Value="Microsoft YaHei UI, Segoe UI" />
    <Setter Property="FontSize" Value="13" />
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="Background" Value="{StaticResource CanvasBrush}" />
    <Setter Property="UseLayoutRounding" Value="True" />
    <Setter Property="TextOptions.TextFormattingMode" Value="Display" />
  </Style>
  <Style TargetType="TextBlock">
    <Setter Property="SnapsToDevicePixels" Value="True" />
  </Style>

  <ControlTemplate x:Key="RoundedButtonTemplate" TargetType="Button">
    <Border Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}"
            BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="6"
            SnapsToDevicePixels="True">
      <ContentPresenter Margin="{TemplateBinding Padding}" RecognizesAccessKey="True"
                        HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                        VerticalAlignment="{TemplateBinding VerticalContentAlignment}" />
    </Border>
  </ControlTemplate>
  <Style TargetType="Button">
    <Setter Property="FontSize" Value="12" />
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="Background" Value="{StaticResource SurfaceBrush}" />
    <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="Padding" Value="12,6" />
    <Setter Property="Margin" Value="0,0,6,0" />
    <Setter Property="MinWidth" Value="32" />
    <Setter Property="MinHeight" Value="32" />
    <Setter Property="HorizontalContentAlignment" Value="Center" />
    <Setter Property="VerticalContentAlignment" Value="Center" />
    <Setter Property="Cursor" Value="Hand" />
    <Setter Property="FocusVisualStyle" Value="{StaticResource KeyboardFocusVisual}" />
    <Setter Property="Template" Value="{StaticResource RoundedButtonTemplate}" />
    <Style.Triggers>
      <Trigger Property="IsMouseOver" Value="True">
        <Setter Property="Background" Value="{StaticResource SurfaceHoverBrush}" />
        <Setter Property="BorderBrush" Value="#C7D3E8" />
      </Trigger>
      <Trigger Property="IsPressed" Value="True">
        <Setter Property="Background" Value="{StaticResource SurfacePressedBrush}" />
        <Setter Property="BorderBrush" Value="#B9C9E3" />
      </Trigger>
      <Trigger Property="IsKeyboardFocused" Value="True">
        <Setter Property="BorderBrush" Value="{StaticResource AccentBrush}" />
      </Trigger>
      <Trigger Property="IsDefaulted" Value="True">
        <Setter Property="BorderBrush" Value="{StaticResource AccentBrush}" />
      </Trigger>
      <Trigger Property="IsEnabled" Value="False">
        <Setter Property="Opacity" Value="0.45" />
        <Setter Property="Cursor" Value="Arrow" />
      </Trigger>
    </Style.Triggers>
  </Style>
  <Style x:Key="PrimaryButton" TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">
    <Setter Property="Background" Value="{StaticResource AccentHoverBrush}" />
    <Setter Property="BorderBrush" Value="{StaticResource AccentHoverBrush}" />
    <Setter Property="Foreground" Value="White" />
    <Style.Triggers>
      <Trigger Property="IsMouseOver" Value="True">
        <Setter Property="Background" Value="{StaticResource AccentPressedBrush}" />
        <Setter Property="BorderBrush" Value="{StaticResource AccentPressedBrush}" />
      </Trigger>
      <Trigger Property="IsPressed" Value="True">
        <Setter Property="Background" Value="{StaticResource AccentPressedBrush}" />
        <Setter Property="BorderBrush" Value="{StaticResource AccentPressedBrush}" />
      </Trigger>
    </Style.Triggers>
  </Style>
  <Style x:Key="SubtleButton" TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="BorderBrush" Value="Transparent" />
    <Setter Property="Foreground" Value="{StaticResource MutedBrush}" />
    <Style.Triggers>
      <Trigger Property="IsMouseOver" Value="True">
        <Setter Property="Background" Value="{StaticResource SurfaceHoverBrush}" />
        <Setter Property="BorderBrush" Value="Transparent" />
        <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
      </Trigger>
      <Trigger Property="IsPressed" Value="True">
        <Setter Property="Background" Value="{StaticResource SurfacePressedBrush}" />
        <Setter Property="BorderBrush" Value="Transparent" />
      </Trigger>
    </Style.Triggers>
  </Style>
  <Style x:Key="IconButton" TargetType="Button" BasedOn="{StaticResource SubtleButton}">
    <Setter Property="Width" Value="32" />
    <Setter Property="Height" Value="32" />
    <Setter Property="Padding" Value="0" />
    <Setter Property="Margin" Value="0" />
  </Style>

  <ControlTemplate x:Key="RoundedToggleTemplate" TargetType="ToggleButton">
    <Border Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}"
            BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="6">
      <ContentPresenter Margin="{TemplateBinding Padding}" RecognizesAccessKey="True"
                        HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                        VerticalAlignment="{TemplateBinding VerticalContentAlignment}" />
    </Border>
  </ControlTemplate>
  <Style TargetType="ToggleButton">
    <Setter Property="FontSize" Value="12" />
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="Background" Value="{StaticResource SurfaceBrush}" />
    <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="Padding" Value="12,6" />
    <Setter Property="Margin" Value="0,0,6,0" />
    <Setter Property="MinHeight" Value="32" />
    <Setter Property="HorizontalContentAlignment" Value="Center" />
    <Setter Property="VerticalContentAlignment" Value="Center" />
    <Setter Property="Cursor" Value="Hand" />
    <Setter Property="FocusVisualStyle" Value="{StaticResource KeyboardFocusVisual}" />
    <Setter Property="Template" Value="{StaticResource RoundedToggleTemplate}" />
    <Style.Triggers>
      <Trigger Property="IsMouseOver" Value="True"><Setter Property="Background" Value="{StaticResource SurfaceHoverBrush}" /></Trigger>
      <Trigger Property="IsPressed" Value="True"><Setter Property="Background" Value="{StaticResource SurfacePressedBrush}" /></Trigger>
      <Trigger Property="IsChecked" Value="True">
        <Setter Property="Background" Value="{StaticResource AccentSoftBrush}" />
        <Setter Property="BorderBrush" Value="#C8D9FC" />
        <Setter Property="Foreground" Value="{StaticResource AccentPressedBrush}" />
      </Trigger>
      <Trigger Property="IsKeyboardFocused" Value="True"><Setter Property="BorderBrush" Value="{StaticResource AccentBrush}" /></Trigger>
      <Trigger Property="IsEnabled" Value="False"><Setter Property="Opacity" Value="0.45" /><Setter Property="Cursor" Value="Arrow" /></Trigger>
    </Style.Triggers>
  </Style>
  <Style x:Key="NavigationButton" TargetType="ToggleButton" BasedOn="{StaticResource {x:Type ToggleButton}}">
    <Setter Property="FontSize" Value="13" />
    <Setter Property="MinHeight" Value="40" />
    <Setter Property="Padding" Value="14,9" />
    <Setter Property="Margin" Value="8,2" />
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="BorderThickness" Value="0" />
    <Setter Property="Foreground" Value="{StaticResource MutedBrush}" />
    <Setter Property="HorizontalContentAlignment" Value="Left" />
    <Setter Property="HorizontalAlignment" Value="Stretch" />
    <Style.Triggers>
      <Trigger Property="IsMouseOver" Value="True"><Setter Property="Foreground" Value="{StaticResource TextBrush}" /></Trigger>
      <Trigger Property="IsChecked" Value="True">
        <Setter Property="Foreground" Value="{StaticResource AccentPressedBrush}" />
        <Setter Property="FontWeight" Value="SemiBold" />
      </Trigger>
    </Style.Triggers>
  </Style>
  <Style x:Key="NavigationActionButton" TargetType="Button" BasedOn="{StaticResource SubtleButton}">
    <Setter Property="FontSize" Value="13" />
    <Setter Property="MinHeight" Value="40" />
    <Setter Property="Padding" Value="14,9" />
    <Setter Property="Margin" Value="8,2" />
    <Setter Property="BorderThickness" Value="0" />
    <Setter Property="HorizontalContentAlignment" Value="Left" />
    <Setter Property="HorizontalAlignment" Value="Stretch" />
  </Style>
  <Style x:Key="SegmentedToggle" TargetType="ToggleButton" BasedOn="{StaticResource {x:Type ToggleButton}}">
    <Setter Property="MinWidth" Value="34" />
    <Setter Property="MinHeight" Value="30" />
    <Setter Property="Padding" Value="10,5" />
    <Setter Property="Margin" Value="2" />
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="BorderBrush" Value="Transparent" />
    <Setter Property="Foreground" Value="{StaticResource MutedBrush}" />
    <Style.Triggers>
      <Trigger Property="IsChecked" Value="True">
        <Setter Property="Background" Value="White" />
        <Setter Property="BorderBrush" Value="#D5E0F3" />
        <Setter Property="Foreground" Value="{StaticResource AccentPressedBrush}" />
      </Trigger>
    </Style.Triggers>
  </Style>

  <Style TargetType="TextBox">
    <Setter Property="FontSize" Value="13" />
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="Background" Value="{StaticResource SurfaceBrush}" />
    <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="Padding" Value="10,6" />
    <Setter Property="MinHeight" Value="32" />
    <Setter Property="VerticalContentAlignment" Value="Center" />
    <Setter Property="CaretBrush" Value="{StaticResource AccentBrush}" />
    <Setter Property="SelectionBrush" Value="#BFD4FF" />
    <Setter Property="SelectionOpacity" Value="0.7" />
    <Setter Property="FocusVisualStyle" Value="{x:Null}" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="TextBox">
          <Border Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}"
                  BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="6">
            <ScrollViewer x:Name="PART_ContentHost" Margin="{TemplateBinding Padding}"
                          SnapsToDevicePixels="{TemplateBinding SnapsToDevicePixels}" />
          </Border>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
    <Style.Triggers>
      <Trigger Property="IsMouseOver" Value="True"><Setter Property="BorderBrush" Value="#C7D3E8" /></Trigger>
      <Trigger Property="IsKeyboardFocusWithin" Value="True"><Setter Property="BorderBrush" Value="{StaticResource AccentBrush}" /></Trigger>
      <Trigger Property="IsReadOnly" Value="True"><Setter Property="Background" Value="#FAFBFD" /></Trigger>
      <Trigger Property="IsEnabled" Value="False"><Setter Property="Opacity" Value="0.5" /></Trigger>
      <Trigger Property="Validation.HasError" Value="True"><Setter Property="BorderBrush" Value="{StaticResource DangerBrush}" /></Trigger>
    </Style.Triggers>
  </Style>

  <Style x:Key="ComboDropDownButton" TargetType="ToggleButton">
    <Setter Property="Focusable" Value="False" />
    <Setter Property="IsTabStop" Value="False" />
    <Setter Property="ClickMode" Value="Press" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="ToggleButton">
          <Border Background="Transparent" CornerRadius="6">
            <TextBlock Text="&#xE70D;" FontFamily="Segoe MDL2 Assets" FontSize="10"
                       Foreground="{StaticResource MutedBrush}" Margin="0,0,11,0"
                       HorizontalAlignment="Right" VerticalAlignment="Center" />
          </Border>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>
  <Style x:Key="ComboEditableTextBox" TargetType="TextBox" BasedOn="{StaticResource {x:Type TextBox}}">
    <Setter Property="Padding" Value="0" />
    <Setter Property="MinHeight" Value="0" />
    <Setter Property="BorderThickness" Value="0" />
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="TextBox"><ScrollViewer x:Name="PART_ContentHost" /></ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>
  <Style TargetType="ComboBox">
    <Setter Property="FontSize" Value="13" />
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="Background" Value="{StaticResource SurfaceBrush}" />
    <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="MinHeight" Value="32" />
    <Setter Property="Padding" Value="10,5,30,5" />
    <Setter Property="Margin" Value="0,0,6,0" />
    <Setter Property="VerticalContentAlignment" Value="Center" />
    <Setter Property="HorizontalContentAlignment" Value="Left" />
    <Setter Property="ScrollViewer.HorizontalScrollBarVisibility" Value="Auto" />
    <Setter Property="ScrollViewer.VerticalScrollBarVisibility" Value="Auto" />
    <Setter Property="ScrollViewer.CanContentScroll" Value="True" />
    <Setter Property="FocusVisualStyle" Value="{StaticResource KeyboardFocusVisual}" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="ComboBox">
          <Grid>
            <Border Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}"
                    BorderThickness="{TemplateBinding BorderThickness}" CornerRadius="6" />
            <ToggleButton Style="{StaticResource ComboDropDownButton}"
                          IsChecked="{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}" />
            <ContentPresenter x:Name="SelectionContent" Margin="{TemplateBinding Padding}" IsHitTestVisible="False"
                              Content="{TemplateBinding SelectionBoxItem}" ContentTemplate="{TemplateBinding SelectionBoxItemTemplate}"
                              ContentTemplateSelector="{TemplateBinding ItemTemplateSelector}"
                              ContentStringFormat="{TemplateBinding SelectionBoxItemStringFormat}"
                              HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                              VerticalAlignment="{TemplateBinding VerticalContentAlignment}" />
            <TextBox x:Name="PART_EditableTextBox" Style="{StaticResource ComboEditableTextBox}"
                     Margin="{TemplateBinding Padding}" Visibility="Hidden" IsReadOnly="{TemplateBinding IsReadOnly}"
                     VerticalContentAlignment="Center" />
            <Popup x:Name="PART_Popup" Placement="Bottom" AllowsTransparency="True" Focusable="False"
                   IsOpen="{TemplateBinding IsDropDownOpen}" PopupAnimation="Fade">
              <Border MinWidth="{Binding ActualWidth, RelativeSource={RelativeSource TemplatedParent}}"
                      MaxHeight="{TemplateBinding MaxDropDownHeight}" Background="White"
                      BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" CornerRadius="6" Padding="4" Margin="0,4,0,0">
                <ScrollViewer CanContentScroll="True" HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Auto">
                  <ItemsPresenter KeyboardNavigation.DirectionalNavigation="Contained" />
                </ScrollViewer>
              </Border>
            </Popup>
          </Grid>
          <ControlTemplate.Triggers>
            <Trigger Property="IsEditable" Value="True">
              <Setter Property="IsTabStop" Value="False" />
              <Setter TargetName="SelectionContent" Property="Visibility" Value="Hidden" />
              <Setter TargetName="PART_EditableTextBox" Property="Visibility" Value="Visible" />
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
    <Style.Triggers>
      <Trigger Property="IsMouseOver" Value="True"><Setter Property="BorderBrush" Value="#C7D3E8" /></Trigger>
      <Trigger Property="IsKeyboardFocusWithin" Value="True"><Setter Property="BorderBrush" Value="{StaticResource AccentBrush}" /></Trigger>
      <Trigger Property="IsDropDownOpen" Value="True"><Setter Property="BorderBrush" Value="{StaticResource AccentBrush}" /></Trigger>
      <Trigger Property="IsEnabled" Value="False"><Setter Property="Opacity" Value="0.5" /></Trigger>
      <Trigger Property="Validation.HasError" Value="True"><Setter Property="BorderBrush" Value="{StaticResource DangerBrush}" /></Trigger>
    </Style.Triggers>
  </Style>
  <Style TargetType="ComboBoxItem">
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="Padding" Value="10,6" />
    <Setter Property="MinHeight" Value="30" />
    <Setter Property="HorizontalContentAlignment" Value="Stretch" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="ComboBoxItem">
          <Border Background="{TemplateBinding Background}" CornerRadius="6" Margin="0,1">
            <ContentPresenter Margin="{TemplateBinding Padding}" HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}" />
          </Border>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
    <Style.Triggers>
      <Trigger Property="IsHighlighted" Value="True"><Setter Property="Background" Value="{StaticResource SurfaceHoverBrush}" /></Trigger>
      <Trigger Property="IsSelected" Value="True"><Setter Property="Background" Value="{StaticResource AccentSoftBrush}" /><Setter Property="Foreground" Value="{StaticResource AccentPressedBrush}" /></Trigger>
      <Trigger Property="IsEnabled" Value="False"><Setter Property="Opacity" Value="0.45" /></Trigger>
    </Style.Triggers>
  </Style>

  <Style TargetType="CheckBox">
    <Setter Property="FontSize" Value="12" />
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="VerticalAlignment" Value="Center" />
    <Setter Property="Margin" Value="0,0,14,0" />
    <Setter Property="Padding" Value="0" />
    <Setter Property="MinHeight" Value="24" />
    <Setter Property="Cursor" Value="Hand" />
    <Setter Property="FocusVisualStyle" Value="{StaticResource KeyboardFocusVisual}" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="CheckBox">
          <Grid Background="Transparent">
            <Grid.ColumnDefinitions><ColumnDefinition Width="18" /><ColumnDefinition Width="*" /></Grid.ColumnDefinitions>
            <Border x:Name="CheckBorder" Width="16" Height="16" CornerRadius="4" BorderThickness="1"
                    BorderBrush="#CCD5E4" Background="White" VerticalAlignment="Center">
              <TextBlock x:Name="CheckMark" Text="&#xE73E;" FontFamily="Segoe MDL2 Assets" FontSize="10"
                         Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center" Visibility="Collapsed" />
            </Border>
            <ContentPresenter x:Name="CheckContent" Grid.Column="1" Margin="7,0,0,0" RecognizesAccessKey="True" VerticalAlignment="Center" />
          </Grid>
          <ControlTemplate.Triggers>
            <Trigger Property="HasContent" Value="False"><Setter TargetName="CheckContent" Property="Margin" Value="0" /></Trigger>
            <Trigger Property="IsMouseOver" Value="True"><Setter TargetName="CheckBorder" Property="BorderBrush" Value="{StaticResource AccentBrush}" /></Trigger>
            <Trigger Property="IsChecked" Value="True">
              <Setter TargetName="CheckBorder" Property="Background" Value="{StaticResource AccentBrush}" />
              <Setter TargetName="CheckBorder" Property="BorderBrush" Value="{StaticResource AccentBrush}" />
              <Setter TargetName="CheckMark" Property="Visibility" Value="Visible" />
            </Trigger>
            <Trigger Property="IsChecked" Value="{x:Null}">
              <Setter TargetName="CheckBorder" Property="Background" Value="{StaticResource AccentBrush}" />
              <Setter TargetName="CheckBorder" Property="BorderBrush" Value="{StaticResource AccentBrush}" />
              <Setter TargetName="CheckMark" Property="Text" Value="&#xE738;" />
              <Setter TargetName="CheckMark" Property="Visibility" Value="Visible" />
            </Trigger>
            <Trigger Property="IsPressed" Value="True"><Setter TargetName="CheckBorder" Property="BorderBrush" Value="{StaticResource AccentPressedBrush}" /></Trigger>
            <Trigger Property="IsEnabled" Value="False"><Setter Property="Opacity" Value="0.45" /><Setter Property="Cursor" Value="Arrow" /></Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>
  <Style TargetType="RadioButton">
    <Setter Property="FontSize" Value="12" />
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="Margin" Value="0,0,14,0" />
    <Setter Property="VerticalAlignment" Value="Center" />
    <Setter Property="FocusVisualStyle" Value="{StaticResource KeyboardFocusVisual}" />
  </Style>

  <Style TargetType="DataGrid">
    <Setter Property="FontSize" Value="12" />
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="Background" Value="White" />
    <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="AutoGenerateColumns" Value="False" />
    <Setter Property="CanUserAddRows" Value="False" />
    <Setter Property="CanUserDeleteRows" Value="False" />
    <Setter Property="HeadersVisibility" Value="Column" />
    <Setter Property="GridLinesVisibility" Value="Horizontal" />
    <Setter Property="HorizontalGridLinesBrush" Value="#F0F2F7" />
    <Setter Property="RowHeight" Value="36" />
    <Setter Property="ColumnHeaderHeight" Value="38" />
    <Setter Property="AlternatingRowBackground" Value="#FAFBFE" />
    <Setter Property="RowBackground" Value="White" />
    <Setter Property="EnableRowVirtualization" Value="True" />
    <Setter Property="EnableColumnVirtualization" Value="True" />
    <Setter Property="ScrollViewer.CanContentScroll" Value="True" />
    <Setter Property="ScrollViewer.VerticalScrollBarVisibility" Value="Auto" />
    <Setter Property="ScrollViewer.HorizontalScrollBarVisibility" Value="Auto" />
  </Style>
  <Style TargetType="DataGridColumnHeader">
    <Setter Property="FontSize" Value="12" />
    <Setter Property="FontWeight" Value="SemiBold" />
    <Setter Property="Foreground" Value="{StaticResource MutedBrush}" />
    <Setter Property="Background" Value="#F8FAFD" />
    <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}" />
    <Setter Property="BorderThickness" Value="0,0,0,1" />
    <Setter Property="Padding" Value="10,7" />
    <Setter Property="HorizontalContentAlignment" Value="Left" />
    <Style.Triggers><Trigger Property="IsMouseOver" Value="True"><Setter Property="Background" Value="#F0F4FB" /></Trigger></Style.Triggers>
  </Style>
  <Style TargetType="DataGridRow">
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="FocusVisualStyle" Value="{x:Null}" />
    <Style.Triggers>
      <Trigger Property="IsMouseOver" Value="True"><Setter Property="Background" Value="#F5F8FE" /></Trigger>
      <Trigger Property="IsSelected" Value="True"><Setter Property="Background" Value="{StaticResource AccentSoftBrush}" /><Setter Property="Foreground" Value="{StaticResource TextBrush}" /></Trigger>
    </Style.Triggers>
  </Style>
  <Style TargetType="DataGridCell">
    <Setter Property="Padding" Value="10,0" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="BorderBrush" Value="Transparent" />
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="FocusVisualStyle" Value="{x:Null}" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="DataGridCell">
          <Border Background="{TemplateBinding Background}" BorderBrush="{TemplateBinding BorderBrush}"
                  BorderThickness="{TemplateBinding BorderThickness}" Padding="{TemplateBinding Padding}">
            <ContentPresenter VerticalAlignment="Center" SnapsToDevicePixels="True" />
          </Border>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
    <Style.Triggers>
      <Trigger Property="IsSelected" Value="True"><Setter Property="Background" Value="{StaticResource AccentSoftBrush}" /></Trigger>
      <Trigger Property="IsKeyboardFocusWithin" Value="True"><Setter Property="BorderBrush" Value="{StaticResource AccentBrush}" /></Trigger>
    </Style.Triggers>
  </Style>

  <Style TargetType="TabControl">
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}" />
    <Setter Property="BorderThickness" Value="0" />
    <Setter Property="Padding" Value="0" />
  </Style>
  <Style TargetType="TabItem">
    <Setter Property="FontSize" Value="13" />
    <Setter Property="Foreground" Value="{StaticResource MutedBrush}" />
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="Padding" Value="13,8" />
    <Setter Property="Margin" Value="2,2,2,6" />
    <Setter Property="FocusVisualStyle" Value="{StaticResource KeyboardFocusVisual}" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="TabItem">
          <Border Background="{TemplateBinding Background}" CornerRadius="6">
            <ContentPresenter ContentSource="Header" Margin="{TemplateBinding Padding}" RecognizesAccessKey="True" />
          </Border>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
    <Style.Triggers>
      <Trigger Property="IsMouseOver" Value="True"><Setter Property="Background" Value="{StaticResource SurfaceHoverBrush}" /></Trigger>
      <Trigger Property="IsSelected" Value="True"><Setter Property="Background" Value="{StaticResource AccentSoftBrush}" /><Setter Property="Foreground" Value="{StaticResource AccentPressedBrush}" /><Setter Property="FontWeight" Value="SemiBold" /></Trigger>
      <Trigger Property="IsEnabled" Value="False"><Setter Property="Opacity" Value="0.45" /></Trigger>
    </Style.Triggers>
  </Style>

  <Style TargetType="ListBox">
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}" />
    <Setter Property="BorderThickness" Value="0" />
    <Setter Property="Padding" Value="0" />
    <Setter Property="ScrollViewer.CanContentScroll" Value="True" />
    <Setter Property="VirtualizingPanel.IsVirtualizing" Value="True" />
    <Setter Property="VirtualizingPanel.VirtualizationMode" Value="Recycling" />
  </Style>
  <Style TargetType="ListBoxItem">
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="Padding" Value="10,7" />
    <Setter Property="Margin" Value="0,1" />
    <Setter Property="HorizontalContentAlignment" Value="Stretch" />
    <Setter Property="FocusVisualStyle" Value="{StaticResource KeyboardFocusVisual}" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="ListBoxItem">
          <Border Background="{TemplateBinding Background}" CornerRadius="6">
            <ContentPresenter Margin="{TemplateBinding Padding}" HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}" />
          </Border>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
    <Style.Triggers>
      <Trigger Property="IsMouseOver" Value="True"><Setter Property="Background" Value="{StaticResource SurfaceHoverBrush}" /></Trigger>
      <Trigger Property="IsSelected" Value="True"><Setter Property="Background" Value="{StaticResource AccentSoftBrush}" /><Setter Property="Foreground" Value="{StaticResource AccentPressedBrush}" /></Trigger>
      <Trigger Property="IsEnabled" Value="False"><Setter Property="Opacity" Value="0.45" /></Trigger>
    </Style.Triggers>
  </Style>
  <Style TargetType="ListView" BasedOn="{StaticResource {x:Type ListBox}}" />

  <Style x:Key="TreeExpander" TargetType="ToggleButton">
    <Setter Property="Focusable" Value="False" />
    <Setter Property="Width" Value="18" />
    <Setter Property="Height" Value="28" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="ToggleButton">
          <Border Background="Transparent">
            <TextBlock x:Name="Arrow" Text="&#xE76C;" FontFamily="Segoe MDL2 Assets" FontSize="9"
                       Foreground="{StaticResource MutedBrush}" HorizontalAlignment="Center" VerticalAlignment="Center" />
          </Border>
          <ControlTemplate.Triggers><Trigger Property="IsChecked" Value="True"><Setter TargetName="Arrow" Property="Text" Value="&#xE70D;" /></Trigger></ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>
  <Style TargetType="TreeView">
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="Background" Value="White" />
    <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="Padding" Value="5" />
    <Setter Property="ScrollViewer.HorizontalScrollBarVisibility" Value="Auto" />
    <Setter Property="ScrollViewer.VerticalScrollBarVisibility" Value="Auto" />
  </Style>
  <Style TargetType="TreeViewItem">
    <Setter Property="Foreground" Value="{StaticResource TextBrush}" />
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="Padding" Value="7,5" />
    <Setter Property="HorizontalContentAlignment" Value="Stretch" />
    <Setter Property="FocusVisualStyle" Value="{x:Null}" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="TreeViewItem">
          <Grid>
            <Grid.ColumnDefinitions><ColumnDefinition Width="18" /><ColumnDefinition Width="*" /></Grid.ColumnDefinitions>
            <Grid.RowDefinitions><RowDefinition Height="Auto" /><RowDefinition Height="Auto" /></Grid.RowDefinitions>
            <ToggleButton x:Name="Expander" Style="{StaticResource TreeExpander}"
                          IsChecked="{Binding IsExpanded, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}" />
            <Border x:Name="HeaderBorder" Grid.Column="1" Background="{TemplateBinding Background}"
                    BorderBrush="Transparent" BorderThickness="1" CornerRadius="6" Margin="0,1">
              <ContentPresenter x:Name="PART_Header" ContentSource="Header" Margin="{TemplateBinding Padding}"
                                HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}" />
            </Border>
            <ItemsPresenter x:Name="ItemsHost" Grid.Row="1" Grid.ColumnSpan="2" Margin="18,0,0,0" Visibility="Collapsed" />
          </Grid>
          <ControlTemplate.Triggers>
            <Trigger Property="HasItems" Value="False"><Setter TargetName="Expander" Property="Visibility" Value="Hidden" /></Trigger>
            <Trigger Property="IsExpanded" Value="True"><Setter TargetName="ItemsHost" Property="Visibility" Value="Visible" /></Trigger>
            <Trigger SourceName="HeaderBorder" Property="IsMouseOver" Value="True"><Setter TargetName="HeaderBorder" Property="Background" Value="{StaticResource SurfaceHoverBrush}" /></Trigger>
            <Trigger Property="IsSelected" Value="True"><Setter Property="Foreground" Value="{StaticResource AccentPressedBrush}" /><Setter TargetName="HeaderBorder" Property="Background" Value="{StaticResource AccentSoftBrush}" /></Trigger>
            <Trigger Property="IsKeyboardFocused" Value="True"><Setter TargetName="HeaderBorder" Property="BorderBrush" Value="{StaticResource AccentBrush}" /></Trigger>
            <Trigger Property="IsEnabled" Value="False"><Setter Property="Opacity" Value="0.45" /></Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style x:Key="ScrollPageButton" TargetType="RepeatButton">
    <Setter Property="Focusable" Value="False" />
    <Setter Property="IsTabStop" Value="False" />
    <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="RepeatButton"><Border Background="Transparent" /></ControlTemplate></Setter.Value></Setter>
  </Style>
  <Style x:Key="ScrollThumb" TargetType="Thumb">
    <Setter Property="Background" Value="#CAD3E2" />
    <Setter Property="Template">
      <Setter.Value><ControlTemplate TargetType="Thumb"><Border Background="{TemplateBinding Background}" CornerRadius="4" Margin="2" /></ControlTemplate></Setter.Value>
    </Setter>
    <Style.Triggers>
      <Trigger Property="IsMouseOver" Value="True"><Setter Property="Background" Value="#AEBBD0" /></Trigger>
      <Trigger Property="IsDragging" Value="True"><Setter Property="Background" Value="#8C9FBC" /></Trigger>
    </Style.Triggers>
  </Style>
  <ControlTemplate x:Key="VerticalScrollBarTemplate" TargetType="ScrollBar">
    <Border Background="Transparent">
      <Track x:Name="PART_Track" Orientation="Vertical" IsDirectionReversed="True"
             Minimum="{TemplateBinding Minimum}" Maximum="{TemplateBinding Maximum}"
             Value="{Binding Value, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}" ViewportSize="{TemplateBinding ViewportSize}">
        <Track.DecreaseRepeatButton><RepeatButton Style="{StaticResource ScrollPageButton}" Command="ScrollBar.PageUpCommand" /></Track.DecreaseRepeatButton>
        <Track.Thumb><Thumb Style="{StaticResource ScrollThumb}" MinHeight="28" /></Track.Thumb>
        <Track.IncreaseRepeatButton><RepeatButton Style="{StaticResource ScrollPageButton}" Command="ScrollBar.PageDownCommand" /></Track.IncreaseRepeatButton>
      </Track>
    </Border>
  </ControlTemplate>
  <ControlTemplate x:Key="HorizontalScrollBarTemplate" TargetType="ScrollBar">
    <Border Background="Transparent">
      <Track x:Name="PART_Track" Orientation="Horizontal" Minimum="{TemplateBinding Minimum}" Maximum="{TemplateBinding Maximum}"
             Value="{Binding Value, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}" ViewportSize="{TemplateBinding ViewportSize}">
        <Track.DecreaseRepeatButton><RepeatButton Style="{StaticResource ScrollPageButton}" Command="ScrollBar.PageLeftCommand" /></Track.DecreaseRepeatButton>
        <Track.Thumb><Thumb Style="{StaticResource ScrollThumb}" MinWidth="28" /></Track.Thumb>
        <Track.IncreaseRepeatButton><RepeatButton Style="{StaticResource ScrollPageButton}" Command="ScrollBar.PageRightCommand" /></Track.IncreaseRepeatButton>
      </Track>
    </Border>
  </ControlTemplate>
  <Style TargetType="ScrollBar">
    <Setter Property="Background" Value="Transparent" />
    <Setter Property="Width" Value="10" />
    <Setter Property="MinWidth" Value="10" />
    <Setter Property="Focusable" Value="False" />
    <Setter Property="Template" Value="{StaticResource VerticalScrollBarTemplate}" />
    <Style.Triggers>
      <Trigger Property="Orientation" Value="Horizontal">
        <Setter Property="Width" Value="Auto" />
        <Setter Property="MinWidth" Value="0" />
        <Setter Property="Height" Value="10" />
        <Setter Property="MinHeight" Value="10" />
        <Setter Property="Template" Value="{StaticResource HorizontalScrollBarTemplate}" />
      </Trigger>
      <Trigger Property="IsEnabled" Value="False"><Setter Property="Opacity" Value="0.35" /></Trigger>
    </Style.Triggers>
  </Style>

  <Style TargetType="ToolTip">
    <Style.Resources>
      <Style TargetType="TextBlock"><Setter Property="TextWrapping" Value="Wrap" /><Setter Property="SnapsToDevicePixels" Value="True" /></Style>
    </Style.Resources>
    <Setter Property="FontSize" Value="12" />
    <Setter Property="Foreground" Value="White" />
    <Setter Property="Background" Value="#182239" />
    <Setter Property="BorderThickness" Value="0" />
    <Setter Property="Padding" Value="10,7" />
    <Setter Property="MaxWidth" Value="360" />
    <Setter Property="Template">
      <Setter.Value>
        <ControlTemplate TargetType="ToolTip">
          <Border Background="{TemplateBinding Background}" CornerRadius="6" Padding="{TemplateBinding Padding}">
            <ContentPresenter />
          </Border>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>
  <Style TargetType="Separator">
    <Setter Property="Background" Value="{StaticResource BorderBrush}" />
    <Setter Property="Height" Value="1" />
    <Setter Property="Margin" Value="0,8" />
    <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="Separator"><Border Background="{TemplateBinding Background}" /></ControlTemplate></Setter.Value></Setter>
  </Style>
</ResourceDictionary>

```

## src/MatrixDevice.Desktop/UiErrorPresentation.cs

```csharp
using System.IO;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public static class UiErrorPresentation
{
    public static string Describe(Exception error)
    {
        if (error is TimeoutException || error is TaskCanceledException { InnerException: TimeoutException }) return "操作超时，请核验设备状态后重试";
        if (error is OperationCanceledException) return "操作已停止";
        if (error is FileNotFoundException missing) return "找不到所需文件：" + (missing.FileName ?? "请检查配置路径");
        if (error.Message.TrimStart().FirstOrDefault() is >= '\u4E00' and <= '\u9FFF') return error.Message;
        if (error is DirectoryNotFoundException) return "指定目录不存在，请检查路径";
        if (error is UnauthorizedAccessException) return "没有访问目标文件或设备的权限";
        if (error is System.Text.Json.JsonException) return "数据格式无效，请检查输入的 JSON 内容";
        if (error is ArgumentException) return "参数无效，请检查输入值";
        if (error is DeviceException device)
        {
            if (device.Message.StartsWith("UI dump did not create", StringComparison.Ordinal)) return "节点读取失败：页面可能仍在变化，请暂停动态内容或切换到静止页面后重试";
            if (device.Message.StartsWith("ADB server owner changed", StringComparison.Ordinal)) return "ADB 服务已变化，请重启本程序并重新发现设备；本程序未重启 ADB";
            if (device.Message.StartsWith("Global Unicode input requires", StringComparison.Ordinal)) return "中文输入需要明确绑定雷电实例与授权设备串号；节点指定文本仍需要兼容手机服务";
            if (device.Message.StartsWith("LDPlayer serial/index binding mismatch", StringComparison.Ordinal)) return "雷电索引与设备串号的进程归属不匹配，已拒绝发送文字，请重新核验绑定";
            var message = device.Code switch
            {
                1002 => "找不到目标设备",
                1007 => "请求参数无效",
                2001 => "当前运行环境不支持此能力",
                1003 => "设备当前状态不允许执行此操作",
                _ => "设备命令未能完成"
            };
            return $"{message}（错误码 {device.Code}）";
        }
        if (error is IOException) return "文件读取、写入或本机端口操作失败，请查看诊断日志";
        return "操作未能完成，详细原因已记录到本机诊断日志";
    }
}

```

## src/MatrixDevice.Desktop/UnifiedDeviceRow.cs

```csharp
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public sealed class UnifiedDeviceRow : ObservableObject
{
    private bool selected;
    public string Key { get; }
    public LdPlayerInstance? Instance { get; private set; }
    public DeviceRow? Device { get; private set; }
    public bool Connected { get; private set; }
    private string phase = "";
    public int? Index => Instance?.Index;
    public string Number => Index?.ToString() ?? "-";
    public string Name => Device?.Name ?? Instance?.Name ?? "";
    public string State => Connected ? "已连接" : phase is not ("" or "已连接") ? phase : Instance?.Index == 0 ? "受保护" : Instance?.IsStarting == true ? "启动中" : Instance?.AndroidStarted == true ? "待连接" : Instance is not null ? "已关闭" : Device?.State ?? "离线";
    public string Model => Device?.Info.Model ?? "-";
    public string Brand => Device?.Info.Brand ?? "-";
    public string Android => Device?.Info.AndroidVersion ?? "-";
    public string Serial => Device?.Serial ?? "-";
    public string Resolution => Device?.Resolution ?? (Instance?.Width is { } width && Instance.Height is { } height ? $"{width} × {height}" : "-");
    public string Group => Device?.Group ?? "未分组";
    public bool CanSelect => Index is not (0 or 99999);
    public bool IsSelected
    {
        get => Device?.IsSelected ?? selected;
        set { if (!CanSelect) return; selected = value; if (Device is not null) Device.IsSelected = value; Changed(); }
    }
    public string PrimaryLabel => Connected ? "调试" : Instance?.AndroidStarted == true ? "连接" : "启动";
    public string PrimaryGlyph => Connected ? "\uE946" : "\uE768";
    public AsyncCommand PrimaryCommand { get; }
    public AsyncCommand ShowWindowCommand { get; }
    public AsyncCommand MainCommand { get; }

    public UnifiedDeviceRow(string key, Func<UnifiedDeviceRow, Task> activate, Func<UnifiedDeviceRow, bool> canActivate, Func<UnifiedDeviceRow, Task> showWindow, Func<UnifiedDeviceRow, bool> canShow, Func<UnifiedDeviceRow, Task> setMain)
    {
        Key = key;
        PrimaryCommand = new AsyncCommand(() => activate(this), () => canActivate(this));
        ShowWindowCommand = new AsyncCommand(() => showWindow(this), () => canShow(this));
        MainCommand = new AsyncCommand(() => setMain(this), () => Connected && Device is not null && canActivate(this));
    }

    public void Update(LdPlayerInstance? instance, DeviceRow? device, bool connected, string operationPhase)
    {
        var retained = IsSelected;
        Instance = instance; Device = device; Connected = connected; phase = operationPhase;
        if (CanSelect && retained && device is not null && !device.IsSelected) device.IsSelected = true;
        selected = retained; Changed(""); RefreshCommands();
    }

    public void RefreshCommands() { PrimaryCommand.Refresh(); ShowWindowCommand.Refresh(); MainCommand.Refresh(); }
}

```

## src/MatrixDevice.Desktop/Views/ApiView.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private ComboBox apiProtocol = null!, apiMethod = null!;
    private TextBox apiRoute = null!, apiResponse = null!, apiPayload = null!;
    private AsyncCommand apiSend = null!;
    private bool OwnsApiEndpoint() => apiProtocol.SelectedIndex switch
    {
        1 => ViewModel.Runtime.Api.AutomatorRunning,
        2 => ViewModel.CurrentDevice is { } row && ViewModel.Runtime.Api.DevicePorts.ContainsKey(row.Serial),
        _ => ViewModel.Runtime.Api.BasicRunning
    };
    private Task SendApiTestRequestAsync() => ViewModel.ExecuteAsync("测试 API", async () =>
    {
        apiResponse.Clear();
        if (!OwnsApiEndpoint()) throw new InvalidOperationException("所选本机接口未由当前程序启动");
        var port = apiProtocol.SelectedIndex switch
        {
            1 => ViewModel.Runtime.Api.RunningAutomatorPort ?? throw new InvalidOperationException("自动化接口未启动"),
            2 => ViewModel.Runtime.Api.DevicePorts[ViewModel.CurrentDevice!.Serial],
            _ => ViewModel.Runtime.Api.RunningBasicPort ?? throw new InvalidOperationException("普通接口未启动")
        };
        if (!apiRoute.Text.StartsWith('/') || apiRoute.Text.Contains("://", StringComparison.Ordinal) || apiRoute.Text.StartsWith("//", StringComparison.Ordinal)) throw new InvalidOperationException("路由必须为本机 API 的相对路径");
        using var handler = new HttpClientHandler { AllowAutoRedirect = false };
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
        using var request = new HttpRequestMessage(new HttpMethod(apiMethod.SelectedItem?.ToString() ?? "GET"), $"http://127.0.0.1:{port}{apiRoute.Text}");
        if (apiMethod.SelectedIndex == 1) { using var validation = JsonDocument.Parse(apiPayload.Text); request.Content = new StringContent(apiPayload.Text, System.Text.Encoding.UTF8, "application/json"); }
        try
        {
            using var result = await client.SendAsync(request);
            apiResponse.Text = $"HTTP {(int)result.StatusCode} {result.ReasonPhrase}\n" + await result.Content.ReadAsStringAsync();
        }
        catch (TaskCanceledException ex) { throw new TimeoutException("本机接口请求超时", ex); }
    }, blocksApiWrites: false);
    private FrameworkElement BuildApiPage()
    {
        var page = Page(); var toolbar = Toolbar(); toolbar.Children.Add(Button("\uE768", "启动两个网关", ViewModel.StartApiCommand)); toolbar.Children.Add(Button("\uE71A", "停止网关", ViewModel.StopApiCommand)); toolbar.Children.Add(Button("\uE968", "启用所选设备", ViewModel.StartDeviceApiCommand)); toolbar.Children.Add(Button("\uE711", "停止所选设备", ViewModel.StopDeviceApiCommand)); DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var summary = BoundText(nameof(WorkspaceViewModel.ApiSummary), true); summary.Margin = new Thickness(0, 10, 0, 10); DockPanel.SetDock(summary, Dock.Top); page.Children.Add(summary);
        var requestPanel = new DockPanel { Margin = new Thickness(0, 10, 0, 0), Height = 260 }; DockPanel.SetDock(requestPanel, Dock.Bottom);
        var requestToolbar = Toolbar();
        apiProtocol = new ComboBox { ItemsSource = new[] { "普通 API", "Automator API", "当前设备 API" }, ItemTemplate = CaptionChoices(), SelectedIndex = 0, Width = 165 }; requestToolbar.Children.Add(apiProtocol);
        apiMethod = new ComboBox { ItemsSource = new[] { "GET", "POST" }, SelectedIndex = 0, Width = 85 }; requestToolbar.Children.Add(apiMethod);
        apiRoute = new TextBox { Text = "/health", Width = 190 }; requestToolbar.Children.Add(apiRoute);
        apiResponse = new TextBox { IsReadOnly = true, AcceptsReturn = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new FontFamily("Consolas"), TextWrapping = TextWrapping.Wrap };
        apiPayload = new TextBox { Text = "{}", AcceptsReturn = true, Height = 60, FontFamily = new FontFamily("Consolas"), Margin = new Thickness(0, 7, 0, 7) };
        apiSend = new AsyncCommand(SendApiTestRequestAsync, () => !ViewModel.Busy && !ViewModel.Transferring && OwnsApiEndpoint());
        apiProtocol.SelectionChanged += (_, _) => apiSend.Refresh(); ViewModel.PropertyChanged += (_, _) => apiSend.Refresh();
        requestToolbar.Children.Add(Button("\uE768", "发送请求", apiSend));
        DockPanel.SetDock(requestToolbar, Dock.Top); requestPanel.Children.Add(requestToolbar); DockPanel.SetDock(apiPayload, Dock.Top); requestPanel.Children.Add(apiPayload); requestPanel.Children.Add(apiResponse); page.Children.Add(requestPanel);
        var grid = new DataGrid { ItemsSource = ViewModel.DeviceView, IsSynchronizedWithCurrentItem = false, IsReadOnly = false };
        grid.SetBinding(Selector.SelectedItemProperty, new Binding(nameof(WorkspaceViewModel.CurrentDevice)) { Mode = BindingMode.OneWay });
        grid.SelectionChanged += (_, _) => { if (Equals((tabs.SelectedItem as TabItem)?.Tag, "API") && grid.SelectedItem is DeviceRow selected) ViewModel.CurrentDevice = selected; };
        var selectedColumn = new DataGridCheckBoxColumn { Binding = TwoWay(nameof(DeviceRow.IsSelected)), IsReadOnly = false }; LocalizationService.Bind(selectedColumn, DataGridColumn.HeaderProperty, "选中"); grid.Columns.Add(selectedColumn); TextColumn(grid, "serial", nameof(DeviceRow.Serial), 210); TextColumn(grid, "设备", nameof(DeviceRow.Name), 180); TextColumn(grid, "分组", nameof(DeviceRow.Group), 100); TextColumn(grid, "设备状态", nameof(DeviceRow.State), 120); TextColumn(grid, "普通 API 地址", nameof(DeviceRow.ApiAddress), 280); grid.MouseDoubleClick += (_, _) => { if (grid.SelectedItem is DeviceRow row && row.ApiAddress != "未启动") Clipboard.SetText(row.ApiAddress); }; page.Children.Add(grid); return page;
    }

}

```

## src/MatrixDevice.Desktop/Views/ApplicationsView.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private FrameworkElement BuildAppsPage()
    {
        var page = Page(); var toolbar = Toolbar(); toolbar.Children.Add(Button("\uE72C", "刷新", ViewModel.RefreshAppsCommand)); toolbar.Children.Add(Check("系统应用", nameof(WorkspaceViewModel.ShowSystemApps))); var search = BoundInput(nameof(WorkspaceViewModel.AppSearch)); search.Width = 200; toolbar.Children.Add(search);
        toolbar.Children.Add(Button("\uE898", "安装 APK", async () => { var dialog = new OpenFileDialog { Filter = "Android 应用|*.apk" }; if (dialog.ShowDialog(this) == true) await ViewModel.InstallAppAsync(dialog.FileName); }, canExecute: ViewModel.CanWriteTargets));
        DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var actions = Toolbar(); actions.Margin = new Thickness(0, 8, 0, 8); actions.Children.Add(Button("\uE768", "启动", ViewModel.LaunchAppCommand)); actions.Children.Add(Button("\uE71A", "停止", ViewModel.StopAppCommand)); actions.Children.Add(Button("\uE894", "清数据", ViewModel.ClearAppCommand)); actions.Children.Add(Button("\uE74D", "卸载", ViewModel.UninstallAppCommand)); DockPanel.SetDock(actions, Dock.Top); page.Children.Add(actions);
        var target = BoundText(nameof(WorkspaceViewModel.TargetContext), true); target.Margin = new Thickness(0, 0, 0, 8); DockPanel.SetDock(target, Dock.Top); page.Children.Add(target);
        var grid = new DataGrid { ItemsSource = ViewModel.AppsView, IsReadOnly = true }; grid.SetBinding(Selector.SelectedItemProperty, TwoWay(nameof(WorkspaceViewModel.SelectedApp))); TextColumn(grid, "包名", nameof(AppInfo.PackageName), 350); TextColumn(grid, "名称", nameof(AppInfo.Name), 180); TextColumn(grid, "版本", nameof(AppInfo.VersionName), 130); var systemColumn = new DataGridCheckBoxColumn { Binding = new Binding(nameof(AppInfo.IsSystem)), IsReadOnly = true }; LocalizationService.Bind(systemColumn, DataGridColumn.HeaderProperty, "系统"); grid.Columns.Add(systemColumn); page.Children.Add(grid); return page;
    }

}

```

## src/MatrixDevice.Desktop/Views/DebugView.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private FrameworkElement BuildNodesPage()
    {
        var page = Page();
        var toolbar = Toolbar(); toolbar.Children.Add(Button("\uE72C", "刷新截图和节点", ViewModel.DumpCommand)); toolbar.Children.Add(Button("\uE74E", "导出 JSON", async () => { var dialog = new SaveFileDialog { Filter = "JSON|*.json", FileName = "ui-tree.json" }; if (dialog.ShowDialog(this) == true) await ViewModel.ExportNodesAsync(dialog.FileName); }));
        toolbar.Children.Add(Button("\uE8A3", "展开", () => SetExpanded(true))); toolbar.Children.Add(Button("\uE8A4", "折叠", () => SetExpanded(false)));
        var search = BoundInput(nameof(WorkspaceViewModel.NodeSearch)); search.Width = 170; search.ToolTip = "按文本、资源 ID 或类名过滤"; toolbar.Children.Add(search); DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var grid = new Grid { Margin = new Thickness(0, 10, 0, 0) }; grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) }); grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(270) });
        nodeCanvas = new Canvas { Background = new SolidColorBrush(Color.FromRgb(232, 237, 240)) };
        var viewbox = new Viewbox { Child = nodeCanvas, Stretch = Stretch.Uniform, VerticalAlignment = VerticalAlignment.Center }; grid.Children.Add(viewbox);
        nodeTree = new TreeView { Margin = new Thickness(8, 0, 8, 0), BorderBrush = new SolidColorBrush(Color.FromRgb(212, 221, 225)) }; nodeTree.SelectedItemChanged += (_, _) => { if (nodeTree.SelectedItem is TreeViewItem { Tag: NodeRow row }) ViewModel.SelectedNode = row; }; Grid.SetColumn(nodeTree, 1); grid.Children.Add(nodeTree);
        var properties = new DockPanel(); var label = Label("节点属性"); DockPanel.SetDock(label, Dock.Top); properties.Children.Add(label); var text = BoundInput(nameof(WorkspaceViewModel.NodeProperties), readOnly: true, multiline: true); text.FontFamily = new FontFamily("Consolas"); properties.Children.Add(text); Grid.SetColumn(properties, 2); grid.Children.Add(properties);
        page.Children.Add(grid); return page;
    }

    private void RebuildNodes()
    {
        if (nodeTree is null || nodeCanvas is null) return;
        nodeTree.Items.Clear(); nodeCanvas.Children.Clear(); nodeTreeItems.Clear(); nodeRectangles.Clear();
        if (ViewModel.DebugImage is { } screenshot) { nodeCanvas.Width = screenshot.PixelWidth; nodeCanvas.Height = screenshot.PixelHeight; nodeCanvas.Children.Add(new Image { Source = screenshot, Width = screenshot.PixelWidth, Height = screenshot.PixelHeight }); }
        else { nodeCanvas.Width = 1; nodeCanvas.Height = 1; }
        bool Matches(NodeRow row) => string.IsNullOrWhiteSpace(ViewModel.NodeSearch) || new[] { row.Source.Text, row.Source.ResourceId, row.Source.ClassName, row.Source.ContentDesc }.Any(value => value?.Contains(ViewModel.NodeSearch, StringComparison.OrdinalIgnoreCase) == true);
        TreeViewItem? Build(NodeRow row)
        {
            var children = row.Children.Select(Build).Where(item => item is not null).Cast<TreeViewItem>().ToList();
            if (!Matches(row) && children.Count == 0) return null;
            var item = new TreeViewItem { Header = $"{row.Label}   {row.Id}", Tag = row, IsExpanded = true, ToolTip = row.Bounds }; nodeTreeItems[row] = item; foreach (var child in children) item.Items.Add(child);
            if (Matches(row) && row.Source.Bounds.Width > 0 && row.Source.Bounds.Height > 0)
            {
                var bounds = row.Source.Bounds;
                var rectangle = new Rectangle { Width = bounds.Width, Height = bounds.Height, Stroke = new SolidColorBrush(Color.FromArgb(170, 10, 129, 142)), StrokeThickness = 1.5, Fill = new SolidColorBrush(Color.FromArgb(1, 10, 129, 142)), Tag = row, ToolTip = $"{row.Label}\n{row.Id}\n{row.Bounds}" };
                Canvas.SetLeft(rectangle, bounds.Left); Canvas.SetTop(rectangle, bounds.Top); Panel.SetZIndex(rectangle, row.Depth);
                rectangle.MouseLeftButtonDown += (_, args) => { ViewModel.SelectedNode = row; args.Handled = true; };
                nodeCanvas.Children.Add(rectangle); nodeRectangles[row] = rectangle;
            }
            return item;
        }
        foreach (var node in ViewModel.Nodes) if (Build(node) is { } item) nodeTree.Items.Add(item);
        HighlightNode();
    }

    private void HighlightNode()
    {
        foreach (var pair in nodeRectangles) { var selected = ReferenceEquals(pair.Key, ViewModel.SelectedNode); pair.Value.Stroke = selected ? new SolidColorBrush(Color.FromRgb(215, 55, 92)) : new SolidColorBrush(Color.FromArgb(170, 10, 129, 142)); pair.Value.StrokeThickness = selected ? 4 : 1.5; pair.Value.Fill = new SolidColorBrush(Color.FromArgb(selected ? (byte)35 : (byte)1, 215, 55, 92)); }
        if (ViewModel.SelectedNode is { } node && nodeTreeItems.TryGetValue(node, out var item) && !item.IsSelected) { item.IsSelected = true; item.BringIntoView(); }
    }
    private void SetExpanded(bool expanded) { foreach (var item in nodeTreeItems.Values) item.IsExpanded = expanded; }

}

```

## src/MatrixDevice.Desktop/Views/DirectoryView.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private FrameworkElement BuildDirectory()
    {
        var panel = new DockPanel { Margin = new Thickness(12, 0, 8, 0) };
        var filters = new StackPanel(); DockPanel.SetDock(filters, Dock.Top);
        filters.Children.Add(Label("设备目录"));
        var search = BoundInput(nameof(WorkspaceViewModel.Search)); search.ToolTip = "按名称或 serial 搜索"; search.Margin = new Thickness(0, 0, 0, 7); filters.Children.Add(search);
        var group = new ComboBox { ItemsSource = ViewModel.Groups, ItemTemplate = CaptionChoices() }; group.SetBinding(Selector.SelectedItemProperty, TwoWay(nameof(WorkspaceViewModel.GroupFilter))); group.Margin = new Thickness(0, 0, 0, 7); filters.Children.Add(group);
        var state = new ComboBox { ItemsSource = new[] { "全部状态", "Ready", "Offline", "Unauthorized", "Discovered", "Error" }, ItemTemplate = CaptionChoices() }; state.SetBinding(Selector.SelectedItemProperty, TwoWay(nameof(WorkspaceViewModel.StatusFilter))); state.Margin = new Thickness(0, 0, 0, 7); filters.Children.Add(state);
        var selection = Toolbar(); selection.Children.Add(Button("\uE73A", "全选筛选", () => ViewModel.SelectFiltered(true))); selection.Children.Add(Button("\uE711", "清空", ClearWallSelection)); filters.Children.Add(selection);
        panel.Children.Add(filters);

        var edit = new StackPanel { Margin = new Thickness(0, 10, 0, 8) }; DockPanel.SetDock(edit, Dock.Bottom);
        edit.Children.Add(Label("当前设备")); edit.Children.Add(BoundText(nameof(WorkspaceViewModel.DeviceContext), wrap: true));
        foreach (var (label, property) in new[] { ("画面", nameof(DeviceRow.ScreenState)), ("节点", nameof(DeviceRow.AutomationState)), ("脚本", nameof(DeviceRow.ScriptState)), ("API", nameof(DeviceRow.ApiAddress)) })
        {
            var status = BoundText("CurrentDevice." + property, true);
            var binding = new MultiBinding { Converter = new SubsystemCaptionConverter(label) };
            binding.Bindings.Add(new Binding("CurrentDevice." + property)); binding.Bindings.Add(new Binding(nameof(LocalizationService.Language)) { Source = LocalizationService.Current });
            status.SetBinding(TextBlock.TextProperty, binding);
            status.FontSize = 11; status.Margin = new Thickness(0, 2, 0, 0); edit.Children.Add(status);
        }
        edit.Children.Add(Field("名称", BoundInput(nameof(WorkspaceViewModel.DeviceName)), 50));
        edit.Children.Add(Field("分组", BoundInput(nameof(WorkspaceViewModel.DeviceGroup)), 50));
        edit.Children.Add(Button("\uE74E", "保存信息", ViewModel.SaveMetadataCommand));
        panel.Children.Add(edit);

        var list = new ListBox { ItemsSource = ViewModel.DeviceView, IsSynchronizedWithCurrentItem = false, Background = Brushes.Transparent, BorderThickness = new Thickness(0), HorizontalContentAlignment = HorizontalAlignment.Stretch, ItemTemplate = DeviceTemplate() };
        list.SetBinding(Selector.SelectedItemProperty, new Binding(nameof(WorkspaceViewModel.CurrentDevice)) { Mode = BindingMode.OneWay });
        list.SelectionChanged += (_, _) => { if (list.SelectedItem is DeviceRow selected) ViewModel.CurrentDevice = selected; };
        panel.Children.Add(list);
        return panel;
    }

    private static DataTemplate DeviceTemplate()
    {
        var root = new FrameworkElementFactory(typeof(StackPanel)); root.SetValue(MarginProperty, new Thickness(0, 5, 0, 6));
        var row = new FrameworkElementFactory(typeof(DockPanel));
        var checkbox = new FrameworkElementFactory(typeof(CheckBox)); checkbox.SetBinding(ToggleButton.IsCheckedProperty, TwoWay(nameof(DeviceRow.IsSelected))); checkbox.SetValue(CheckBox.MarginProperty, new Thickness(0, 0, 7, 0)); row.AppendChild(checkbox);
        var name = new FrameworkElementFactory(typeof(TextBlock)); name.SetBinding(TextBlock.TextProperty, new Binding(nameof(DeviceRow.Name))); name.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold); name.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis); row.AppendChild(name); root.AppendChild(row);
        foreach (var property in new[] { nameof(DeviceRow.Serial), nameof(DeviceRow.Resolution), nameof(DeviceRow.State) })
        {
            var text = new FrameworkElementFactory(typeof(TextBlock)); text.SetBinding(TextBlock.TextProperty, new Binding(property)); text.SetValue(TextBlock.FontSizeProperty, 11d); text.SetValue(TextBlock.ForegroundProperty, Brushes.DimGray); text.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis); text.SetValue(MarginProperty, new Thickness(22, 2, 0, 0)); root.AppendChild(text);
        }
        return new DataTemplate { VisualTree = root };
    }

}

```

## src/MatrixDevice.Desktop/Views/ExtensionsView.cs

```csharp
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MatrixDevice.Features;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private FrameworkElement BuildExtensionsPage()
    {
        var page = Page();
        var grid = new Grid(); grid.ColumnDefinitions.Add(new ColumnDefinition()); grid.ColumnDefinitions.Add(new ColumnDefinition());
        var proxyPanel = new StackPanel { Margin = new Thickness(0, 0, 18, 0) };
        proxyPanel.Children.Add(Label("代理配置"));
        var address = new PasswordBox { Padding = new Thickness(7), Height = 34, ToolTip = "代理 URL，凭据输入已遮蔽" };
        proxyPanel.Children.Add(Field("代理 URL", address, 100));
        var output = new TextBox { IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Height = 225, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new FontFamily("Consolas") };
        ProxyProfile? profile = null;
        var proxyActions = Toolbar();
        proxyActions.Children.Add(Button("\uE721", "解析配置", async () => await ViewModel.ExecuteAsync("解析代理配置", async () => { profile = ProxyProfileParser.Parse(address.Password); output.Text = ProxyProfileParser.ExportProfileJson(profile); await Task.CompletedTask; })));
        proxyActions.Children.Add(Button("\uE74E", "导出无凭据 JSON", async () =>
        {
            if (profile is null) throw new InvalidOperationException("尚未解析代理配置");
            var dialog = new SaveFileDialog { Filter = "JSON|*.json", FileName = "proxy-profile.json" };
            if (dialog.ShowDialog(this) == true) await File.WriteAllTextAsync(dialog.FileName, ProxyProfileParser.ExportProfileJson(profile));
        }));
        proxyPanel.Children.Add(proxyActions); proxyPanel.Children.Add(output);
        var proxyBoundary = new TextBlock { Foreground = Brushes.DimGray, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 10) }; LocalizationService.Bind(proxyBoundary, TextBlock.TextProperty, "代理运行器与 ROM 路由规则：未恢复，连接能力不可用"); proxyPanel.Children.Add(proxyBoundary);
        proxyPanel.Children.Add(Button("\uE72E", "核验当前设备 root", async () => await ViewModel.ExecuteAsync("核验 root", async () =>
        {
            var row = ViewModel.CurrentDevice ?? throw new InvalidOperationException("未选择设备");
            if (ViewModel.Demo) throw new InvalidOperationException("演示结果不能作为真实 root 证据");
            await new ProxyService(ViewModel.Runtime.Backend).EnsureRootAsync(row.Serial);
            output.Text = $"设备 {row.Serial} 已返回 root UID=0";
        }), write: true));
        proxyPanel.Children.Add(Label("环境配置"));
        var timezone = new TextBox { Text = "Asia/Shanghai" }; proxyPanel.Children.Add(Field("时区", timezone, 100));
        proxyPanel.Children.Add(Button("\uE74E", "应用并核验时区", async () =>
        {
            var row = ViewModel.CurrentDevice ?? throw new InvalidOperationException("未选择设备");
            if (!ViewModel.Confirm("修改设备时区", $"设备：{row.Name} ({row.Serial})\n时区：{timezone.Text}\n操作需要 root。")) return;
            await ViewModel.ExecuteAsync("修改时区", async () => await new EnvironmentConfigurationService(ViewModel.Runtime.Backend).ApplyAsync(row.Serial, new EnvironmentConfiguration { Timezone = timezone.Text.Trim() }));
        }, write: true));
        grid.Children.Add(new ScrollViewer { Name = "ExtensionSettingsScroll", Content = proxyPanel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });

        var backupPanel = new DockPanel(); Grid.SetColumn(backupPanel, 1);
        var heading = Label("应用资料包"); DockPanel.SetDock(heading, Dock.Top); backupPanel.Children.Add(heading);
        var package = new TextBox { Text = "" }; var packageField = Field("应用包名", package, 100); DockPanel.SetDock(packageField, Dock.Top); backupPanel.Children.Add(packageField);
        var notes = new TextBox(); var notesField = Field("备注", notes, 100); DockPanel.SetDock(notesField, Dock.Top); backupPanel.Children.Add(notesField);
        var storage = System.IO.Path.Combine(ViewModel.DataDirectory, "backups");
        var service = new ApplicationBackupService(ViewModel.Runtime.Backend, storage);
        var records = new ObservableCollection<AppBackupRecord>(service.List());
        var list = new DataGrid { ItemsSource = records, IsReadOnly = true }; TextColumn(list, "包名", nameof(AppBackupRecord.PackageName), 210); TextColumn(list, "创建时间", nameof(AppBackupRecord.CreatedUtc), 190); TextColumn(list, "备注", nameof(AppBackupRecord.Notes), 180);
        var backupActions = Toolbar();
        backupActions.Children.Add(Button("\uE8E5", "验证并导入", async () =>
        {
            var dialog = new OpenFileDialog { Filter = "应用资料包|*.tar.gz" };
            if (dialog.ShowDialog(this) == true)
            {
                var packageName = package.Text.Trim(); var remark = notes.Text; var path = dialog.FileName;
                await ViewModel.ExecuteAsync("导入资料包", async () => { var imported = await Task.Run(() => service.Import(path, packageName, remark)); records.Insert(0, imported); });
            }
        }));
        backupActions.Children.Add(Button("\uE896", "导出所选", async () =>
        {
            if (list.SelectedItem is not AppBackupRecord record) throw new InvalidOperationException("未选择资料包");
            var dialog = new SaveFileDialog { Filter = "资料包|*.tar.gz", FileName = record.PackageName + ".tar.gz" };
            if (dialog.ShowDialog(this) == true) await ViewModel.ExecuteAsync("导出资料包", async () => await Task.Run(() => service.Export(record, dialog.FileName)));
        }));
        backupActions.Children.Add(Button("\uE74D", "删除所选", async () =>
        {
            if (list.SelectedItem is not AppBackupRecord record) throw new InvalidOperationException("未选择资料包");
            if (!ViewModel.Confirm("删除资料包", $"仅删除本地受管备份\n包名：{record.PackageName}\n记录：{record.Id}")) return;
            await ViewModel.ExecuteAsync("删除资料包", async () => { await Task.Run(() => service.Delete(record, true)); records.Remove(record); });
        }));
        DockPanel.SetDock(backupActions, Dock.Top); backupPanel.Children.Add(backupActions);
        var boundary = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray, Margin = new Thickness(0, 9, 0, 9) }; LocalizationService.Bind(boundary, TextBlock.TextProperty, "此页导入和导出本地包，不自动恢复设备数据。"); DockPanel.SetDock(boundary, Dock.Top); backupPanel.Children.Add(boundary); backupPanel.Children.Add(list);
        grid.Children.Add(backupPanel);
        page.Children.Add(grid); return page;
    }
}

```

## src/MatrixDevice.Desktop/Views/FilesView.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private FrameworkElement BuildFilesPage()
    {
        var page = Page(); var path = Toolbar(); var pathLabel = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) }; LocalizationService.Bind(pathLabel, TextBlock.TextProperty, "设备路径"); path.Children.Add(pathLabel); var remote = BoundInput(nameof(WorkspaceViewModel.RemotePath)); remote.Width = 440; path.Children.Add(remote); path.Children.Add(Button("\uE72C", "查看目录", async () =>
        {
            await ViewModel.ExecuteAsync("读取目录", async () =>
            {
                var row = ViewModel.CurrentDevice ?? throw new InvalidOperationException("未选择设备");
                var remotePath = ViewModel.RemotePath;
                if (remotePath.Any(character => !char.IsLetterOrDigit(character) && character is not '/' and not '_' and not '-' and not '.')) throw new InvalidOperationException("目录路径含不支持字符");
                var output = await ViewModel.Runtime.Backend.ShellAsync(row.Serial, $"ls -la '{remotePath}'");
                ShowText("设备目录 · " + row.Serial, output);
            });
        }, canExecute: ViewModel.CanUseDevice)); DockPanel.SetDock(path, Dock.Top); page.Children.Add(path);
        var actions = Toolbar(); actions.Margin = new Thickness(0, 8, 0, 8); var autoInstall = new CheckBox { VerticalAlignment = VerticalAlignment.Center }; LocalizationService.Bind(autoInstall, ContentControl.ContentProperty, "上传 APK 后安装"); actions.Children.Add(autoInstall);
        actions.Children.Add(Button("\uE898", "添加上传", async () => { var dialog = new OpenFileDialog { Multiselect = true }; if (dialog.ShowDialog(this) == true) await ViewModel.ExecuteAsync("添加上传", async () => { foreach (var file in dialog.FileNames) ViewModel.QueuePush(file, autoInstall.IsChecked == true); await Task.CompletedTask; }); }, canExecute: ViewModel.CanWriteTargets));
        actions.Children.Add(Button("\uE896", "添加下载", async () => { var dialog = new SaveFileDialog { FileName = System.IO.Path.GetFileName(ViewModel.RemotePath.TrimEnd('/')) }; if (dialog.ShowDialog(this) == true) await ViewModel.ExecuteAsync("添加下载", async () => { ViewModel.QueuePull(dialog.FileName); await Task.CompletedTask; }); }, canExecute: ViewModel.CanWriteDevice));
        actions.Children.Add(Button("\uE768", "开始", ViewModel.StartTransfersCommand)); actions.Children.Add(Button("\uE71A", "停止", ViewModel.CancelTransfersCommand)); DockPanel.SetDock(actions, Dock.Top); page.Children.Add(actions);
        var progress = new ProgressBar { Minimum = 0, Maximum = 100, Height = 5, Margin = new Thickness(0, 0, 0, 10) }; progress.SetBinding(RangeBase.ValueProperty, new Binding(nameof(WorkspaceViewModel.OverallProgress)) { Mode = BindingMode.OneWay }); DockPanel.SetDock(progress, Dock.Top); page.Children.Add(progress);
        var queue = new DataGrid { ItemsSource = ViewModel.Transfers, IsReadOnly = true }; TextColumn(queue, "设备", nameof(TransferRow.Serial), 160); TextColumn(queue, "方向", nameof(TransferRow.Direction), 65); TextColumn(queue, "文件", nameof(TransferRow.Name), 170); TextColumn(queue, "大小", nameof(TransferRow.Size), 100); TextColumn(queue, "设备路径", nameof(TransferRow.RemotePath), 230); TextColumn(queue, "%", nameof(TransferRow.Progress), 60); TextColumn(queue, "状态", nameof(TransferRow.State), 190); page.Children.Add(queue); return page;
    }

}

```

## src/MatrixDevice.Desktop/Views/LdPlayerView.cs

```csharp
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private readonly ObservableCollection<LdInstanceRow> ldRows = [];
    private readonly List<AsyncCommand> ldCommands = [];
    private readonly Dictionary<string, AsyncCommand> ldCommandsByLabel = [];
    private LdInstanceRow? ldSelection;
    private bool ldBusy;
    private TextBlock ldFeedback = null!;
    private TextBlock ldContext = null!;
    private TextBox ldPackage = null!;
    private TextBox ldText = null!;
    private DataGrid ldGrid = null!;

    private FrameworkElement BuildLdPlayerPage()
    {
        var page = Page();
        var modes = BuildDeviceModes(); DockPanel.SetDock(modes, Dock.Top); page.Children.Add(modes);
        var toolbar = Toolbar();
        toolbar.Children.Add(LdButton("\uE768", "启动并连接", () => ConnectLdInstancesAsync([ldSelection!.Index])));
        toolbar.Children.Add(LdButton("\uE768", "一键启动全部", StartAllLdInstancesAsync, requireSelection: false));
        toolbar.Children.Add(LdButton("\uE8D4", "接入已运行实例", ConnectRunningLdInstancesAsync, requireSelection: false));
        toolbar.Children.Add(LdButton("\uE710", "新建空白实例", () => EditLdInstanceAsync(true), requireSelection: false));
        toolbar.Children.Add(LdButton("\uE713", "资源配置", () => EditLdInstanceAsync(false), requireStopped: true));
        toolbar.Children.Add(LdButton("\uE71A", "关闭实例", () => ExecuteLdPlayerAsync("quit"), requireProcess: true));
        toolbar.Children.Add(LdButton("\uE777", "重启实例", () => ExecuteLdPlayerAsync("reboot"), requireProcess: true));
        var stop = new Button { Content = ShellGlyph("\uE71A"), ToolTip = "停止等待和后续启动，不关闭已运行实例", Width = 34, Height = 34, Margin = new Thickness(0, 0, 0, 6) };
        stop.Click += (_, _) => ldOperationCancellation?.Cancel(); toolbar.Children.Add(stop);
        DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        ldFeedback = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 9, 0, 9), Foreground = Brushes.DimGray };
        DockPanel.SetDock(ldFeedback, Dock.Top); page.Children.Add(ldFeedback);
        var body = new Grid(); body.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); body.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        ldGrid = new DataGrid { ItemsSource = ldRows, IsReadOnly = true, SelectionMode = DataGridSelectionMode.Single };
        TextColumn(ldGrid, "实例编号", nameof(LdInstanceRow.Index), 80);
        TextColumn(ldGrid, "名称", nameof(LdInstanceRow.Name), 175);
        TextColumn(ldGrid, "连接状态", nameof(LdInstanceRow.ConnectionState), 110);
        TextColumn(ldGrid, "安卓状态", nameof(LdInstanceRow.AndroidState), 110);
        TextColumn(ldGrid, "进程状态", nameof(LdInstanceRow.ProcessState), 110);
        TextColumn(ldGrid, "播放器进程", nameof(LdInstanceRow.ProcessId), 110);
        TextColumn(ldGrid, "虚拟机进程", nameof(LdInstanceRow.VBoxProcessId), 110);
        TextColumn(ldGrid, "分辨率", nameof(LdInstanceRow.Size), 120);
        TextColumn(ldGrid, "密度", nameof(LdInstanceRow.Dpi), 85);
        TextColumn(ldGrid, "会话代次", nameof(LdInstanceRow.Generation), 180);
        TextColumn(ldGrid, "显式设备绑定", nameof(LdInstanceRow.Binding), 240);
        ldGrid.SelectionChanged += (_, _) =>
        {
            ldSelection = ldGrid.SelectedItem as LdInstanceRow;
            ldContext.Text = ldSelection is null ? "当前实例：未选择" : $"当前实例：{ldSelection.Name} · 编号 {ldSelection.Index} · 代次 {ldSelection.Generation}";
            RefreshLdButtons();
        };
        body.Children.Add(ldGrid);
        var actions = new StackPanel { Margin = new Thickness(0, 12, 0, 0) }; Grid.SetRow(actions, 1); body.Children.Add(actions);
        ldContext = new TextBlock { Text = "当前实例：未选择", FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8) }; actions.Children.Add(ldContext);
        var packageRow = Toolbar(); packageRow.Children.Add(new TextBlock { Text = "应用包名", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
        ldPackage = new TextBox { Width = 300, Margin = new Thickness(0, 0, 8, 0) }; ldPackage.TextChanged += (_, _) => RefreshLdButtons(); packageRow.Children.Add(ldPackage);
        packageRow.Children.Add(LdButton("\uE768", "启动应用", () => ExecuteLdPlayerAsync("runapp", new JsonObject { ["packageName"] = ldPackage.Text.Trim() }), requirePackage: true, requireStarted: true));
        packageRow.Children.Add(LdButton("\uE71A", "停止应用", () => ExecuteLdPlayerAsync("killapp", new JsonObject { ["packageName"] = ldPackage.Text.Trim() }), requirePackage: true, requireStarted: true));
        packageRow.Children.Add(LdButton("\uE898", "安装应用", async () =>
        {
            var dialog = new OpenFileDialog { Filter = "安卓应用|*.apk" };
            if (dialog.ShowDialog(this) == true) await ExecuteLdPlayerAsync("installapp", new JsonObject { ["file"] = dialog.FileName });
        }, requireStarted: true));
        actions.Children.Add(packageRow);
        var textRow = Toolbar(); textRow.Margin = new Thickness(0, 8, 0, 8); textRow.Children.Add(new TextBlock { Text = "输入文字", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
        ldText = new TextBox { Width = 420, Margin = new Thickness(0, 0, 8, 0) }; ldText.TextChanged += (_, _) => RefreshLdButtons(); textRow.Children.Add(ldText);
        textRow.Children.Add(LdButton("\uE8F4", "发送文字", () => ExecuteLdPlayerAsync("input", new JsonObject { ["text"] = ldText.Text }), requireText: true, requireStarted: true));
        actions.Children.Add(textRow);
        var keys = Toolbar();
        keys.Children.Add(LdButton("\uE80F", "主页", () => ExecuteLdPlayerAsync("keyevent", new JsonObject { ["key"] = "home" }), requireStarted: true));
        keys.Children.Add(LdButton("\uE72B", "返回", () => ExecuteLdPlayerAsync("keyevent", new JsonObject { ["key"] = "back" }), requireStarted: true));
        keys.Children.Add(LdButton("\uE995", "增大音量", () => ExecuteLdPlayerAsync("keyevent", new JsonObject { ["key"] = "volumeup" }), requireStarted: true));
        keys.Children.Add(LdButton("\uE993", "减小音量", () => ExecuteLdPlayerAsync("keyevent", new JsonObject { ["key"] = "volumedown" }), requireStarted: true));
        actions.Children.Add(keys);
        page.Children.Add(body);
        UpdateLdPlayerRows();
        ViewModel.PropertyChanged += (_, _) => RefreshLdButtons();
        return page;
    }

    private Button LdButton(string glyph, string label, Func<Task> action, bool requireSelection = true, bool requirePackage = false, bool requireText = false, bool requireStarted = false, bool requireStopped = false, bool requireProcess = false)
    {
        var command = new AsyncCommand(async () =>
        {
            try { await action(); }
            catch (Exception error) { ldFeedback.Text = label + "失败：" + ViewModel.PresentError(error); }
        }, () => !ldBusy && !captureInProgress && !ViewModel.HasActiveOperations && ViewModel.ConnectionReloadAvailable && ViewModel.Runtime.LdPlayer is not null && (!requireSelection || ldSelection is not null && (ViewModel.Demo || ldSelection.Index > 0)) && (!requirePackage || !string.IsNullOrWhiteSpace(ldPackage?.Text)) && (!requireText || !string.IsNullOrWhiteSpace(ldText?.Text)) && (!requireStarted || ldSelection?.Instance.AndroidStarted == true && (ViewModel.Demo || LdInstanceConnected(ldSelection.Index))) && (!requireStopped || ldSelection?.Instance is { IsStarting: false, AndroidStarted: false, ProcessId: <= 0, VBoxProcessId: <= 0, TopWindow: 0, BindWindow: 0 }) && (!requireProcess || ldSelection?.Instance is { ProcessId: > 0 } || ldSelection?.Instance.AndroidStarted == true));
        ldCommands.Add(command);
        ldCommandsByLabel.Add(label, command);
        return Button(glyph, label, command);
    }

    private void RefreshLdButtons() { foreach (var command in ldCommands) command.Refresh(); RefreshLaunchCommands(); }
    private void UpdateLdPlayerRows()
    {
        var index = ldSelection?.Index;
        ldRows.Clear();
        if (ViewModel.Runtime.LdPlayer is not { } service)
        {
            ldFeedback.Text = "雷电能力未接入：请在设置中选择雷电命令行程序路径，保存并重新启动。本页不会自动配置设备序列号。";
            RefreshLdButtons(); return;
        }
        foreach (var instance in service.Instances)
        {
            var binding = ViewModel.Runtime.Settings.LdSerialBindings.TryGetValue(instance.Index, out var serial) ? serial + "（配置绑定，尚未核验）" : "尚未绑定";
            ldRows.Add(new LdInstanceRow(instance, binding, InstanceConnectionState(instance)));
        }
        ldGrid.SelectedItem = index is { } selected ? ldRows.FirstOrDefault(row => row.Index == selected) : null;
        if (ViewModel.Runtime.LdPlayerError is { } error) ldFeedback.Text = "雷电实例发现失败：" + ViewModel.PresentError(new InvalidOperationException(error));
        else if (string.IsNullOrWhiteSpace(ldFeedback.Text)) ldFeedback.Text = ViewModel.Demo ? "演示模式" : "";
        RefreshLdButtons();
    }

    private async Task RefreshLdPlayerUiAsync()
    {
        if (ViewModel.HasActiveOperations) return;
        ldBusy = true; ViewModel.SetExternalBusy(true); ldFeedback.Text = "正在刷新雷电实例…"; RefreshLdButtons();
        try { await ViewModel.Runtime.RefreshLdPlayerAsync(); UpdateLdPlayerRows(); }
        catch (Exception error) { ldFeedback.Text = "刷新雷电实例失败：" + ViewModel.PresentError(error); }
        finally { ldBusy = false; ViewModel.SetExternalBusy(false); RefreshLdButtons(); }
    }

    private async Task ExecuteLdPlayerAsync(string operation, JsonObject? body = null)
    {
        if (ViewModel.HasActiveOperations) throw new DeviceException("请先结束界面操作、脚本或文件传输，再操作雷电实例");
        var selected = ldSelection ?? throw new InvalidOperationException("未选择雷电实例");
        var service = ViewModel.Runtime.LdPlayer ?? throw new InvalidOperationException("雷电命令行能力尚未接入");
        if (!ViewModel.Demo && selected.Index <= 0) throw new DeviceException("实例0受保护。", 403, 1003);
        if (!ViewModel.Demo && operation is not ("launch" or "quit" or "reboot") && !LdInstanceConnected(selected.Index)) throw new DeviceException("请先接入该实例，再执行应用或输入操作。", 403, 1003);
        body ??= new JsonObject(); body["expectedGeneration"] = selected.Generation;
        if (operation is "quit" or "reboot")
        {
            var label = operation == "quit" ? "关闭雷电实例" : "重启雷电实例";
            if (!ViewModel.Confirm(label, $"实例：{selected.Name}\n编号：{selected.Index}\n会话代次：{selected.Generation}\n此操作会中断该实例中的活动应用。确认继续？")) { ldFeedback.Text = "操作已取消，未发送命令。"; return; }
            body["confirmed"] = true;
        }
        ldBusy = true; ViewModel.SetExternalBusy(true); ldFeedback.Text = "正在执行所选实例命令…"; RefreshLdButtons();
        try
        {
            ViewModel.Runtime.RequireFrozenConnectionSettings();
            if (ViewModel.Runtime.Api.LdCommandIdentityGuard is { } verify) await verify(selected.Index, operation, CancellationToken.None);
            var result = await service.ExecuteAsync(selected.Index, operation, body, CancellationToken.None);
            UpdateLdPlayerRows();
            ViewModel.Runtime.Log.Write("ld-command-result", $"index={result.Index}; accepted={result.Accepted}; verified={result.Verified}; {result.Message}");
            ldFeedback.Text = $"实例 {result.Index}：" + (result.Verified ? "操作已完成" : "命令已提交，等待状态更新");
        }
        catch (Exception error) { UpdateLdPlayerRows(); ldFeedback.Text = $"实例 {selected.Index} 操作失败：{ViewModel.PresentError(error)}"; }
        finally { ldBusy = false; ViewModel.SetExternalBusy(false); RefreshLdButtons(); }
    }
}

public sealed class LdInstanceRow(LdPlayerInstance instance, string binding, string connectionState = "待接入")
{
    public LdPlayerInstance Instance { get; } = instance;
    public int Index => Instance.Index;
    public string Name => Instance.Name;
    public string AndroidState => Instance.AndroidStarted ? "已启动" : "未启动";
    public string ProcessState => Instance.ProcessId > 0 ? "已发现进程" : "未发现进程";
    public int ProcessId => Instance.ProcessId;
    public int VBoxProcessId => Instance.VBoxProcessId;
    public string Size => Instance.Width is { } width && Instance.Height is { } height ? $"{width} × {height}" : "未核验";
    public string Dpi => Instance.Dpi?.ToString() ?? "未核验";
    public long Generation => Instance.Generation;
    public string Binding { get; } = binding;
    public string ConnectionState { get; } = connectionState;
}

```

## src/MatrixDevice.Desktop/Views/LdWindowVisibilitySmoke.cs

```csharp
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public static class LdWindowVisibilitySmoke
{
    public static async Task VerifyAsync(string evidenceDir)
    {
        if (!Path.IsPathFullyQualified(evidenceDir)) throw new ArgumentException("窗口自测证据目录必须为绝对路径。");
        if (Application.Current is null || !Application.Current.Dispatcher.CheckAccess())
            throw new InvalidOperationException("窗口自测必须在本程序 WPF Dispatcher 上运行。");
        var directory = Path.Combine(evidenceDir, "self-window-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        using var process = Process.GetCurrentProcess();
        var birth = process.StartTime.ToUniversalTime().Ticks;
        var executable = process.MainModule?.FileName ?? throw new InvalidOperationException("无法读取自测进程路径。");
        var install = Path.GetDirectoryName(executable)!;
        var visibility = new LdWindowVisibility(directory, install);
        var passed = new List<string>();
        var windows = new List<Window>();
        var window = MakeWindow(); windows.Add(window);
        await RenderAsync();
        var handle = new WindowInteropHelper(window).Handle;
        var instance = new LdPlayerInstance(1, "本程序窗口自测，不是真实模拟器", handle.ToInt64(), 0, false, process.Id, -1);
        var parent = GetParent(handle);
        var style = GetWindowLongPtr(handle, -16).ToInt64();
        var exStyle = GetWindowLongPtr(handle, -20).ToInt64();
        try
        {
            Assert(IsWindowVisible(handle), "自测窗口没有正常显示。");
            await ExpectAsync(400, () => visibility.HideAsync(instance with { Index = 0 }, birth));
            await ExpectAsync(409, () => visibility.HideAsync(instance, birth + 1));
            await ExpectAsync(409, () => visibility.HideAsync(instance with { ProcessId = int.MaxValue }, birth));
            Assert(IsWindowVisible(handle) && JournalCount(directory) == 0, "错误身份触碰了窗口或建立恢复记录。");
            passed.Add("protected-index-and-wrong-owner-refused");

            var outside = Path.Combine(directory, "empty-sdk"); Directory.CreateDirectory(outside);
            var wrongInstall = new LdWindowVisibility(Path.Combine(directory, "wrong-install"), outside);
            await ExpectAsync(403, () => wrongInstall.HideAsync(instance, birth));
            Assert(IsWindowVisible(handle), "安装路径拒绝后窗口被隐藏。");
            passed.Add("executable-outside-frozen-install-refused");

            Assert(await visibility.HideAsync(instance, birth), "自有可见窗口未获得隐藏所有权。");
            Assert(!IsWindowVisible(handle) && visibility.IsHidden(1) && JournalCount(directory) == 1, "隐藏状态或预写恢复记录不正确。");
            Assert((GetWindowLongPtr(handle, -16).ToInt64() & ~0x10000000L) == (style & ~0x10000000L) && GetParent(handle) == parent && GetWindowLongPtr(handle, -20).ToInt64() == exStyle,
                "隐藏改变了可见位以外的样式或父窗口。");
            await visibility.ShowAsync(1);
            Assert(IsWindowVisible(handle) && !visibility.IsHidden(1) && JournalCount(directory) == 0 && GetForegroundWindow() != handle,
                "恢复未显示自有窗口、遗留记录或激活了窗口。");
            Assert(GetWindowLongPtr(handle, -16).ToInt64() == style && GetParent(handle) == parent && GetWindowLongPtr(handle, -20).ToInt64() == exStyle,
                "恢复改变了原窗口样式或父窗口。");
            passed.Add("journal-before-hide-and-nonactivating-show");

            window.Hide(); await RenderAsync();
            Assert(!await visibility.HideAsync(instance, birth) && !visibility.IsHidden(1) && JournalCount(directory) == 0,
                "此前已经隐藏的窗口被本组件占有。");
            await visibility.ShowAsync(1);
            Assert(!IsWindowVisible(handle), "显示操作修改了并非本组件隐藏的窗口。");
            window.Show(); await RenderAsync();
            passed.Add("preexisting-hidden-window-not-owned");

            await visibility.HideAsync(instance, birth);
            var recordPath = Directory.EnumerateFiles(directory, "ld-window-*.json", SearchOption.AllDirectories).Single();
            var recordText = await File.ReadAllTextAsync(recordPath);
            var record = JsonNode.Parse(recordText)!.AsObject();
            var marker = record["marker"]!.GetValue<string>();
            RemoveProp(handle, marker);
            await ExpectAsync(409, () => visibility.ShowAsync(1));
            Assert(!IsWindowVisible(handle) && File.Exists(recordPath), "窗口标记丢失后仍恢复窗口或删除恢复记录。");
            if (!SetProp(handle, marker, new IntPtr(1))) throw new Win32Exception(Marshal.GetLastWin32Error(), "无法恢复自测窗口标记");
            await visibility.ShowAsync(1);
            passed.Add("missing-marker-reuse-guard-refused");

            await visibility.HideAsync(instance, birth);
            recordPath = Directory.EnumerateFiles(directory, "ld-window-*.json", SearchOption.AllDirectories).Single();
            recordText = await File.ReadAllTextAsync(recordPath);
            record = JsonNode.Parse(recordText)!.AsObject();
            record["processStartUtcTicks"] = birth + 1;
            await File.WriteAllTextAsync(recordPath, record.ToJsonString(JsonDefaults.Options));
            var recovery = new LdWindowVisibility(directory, install);
            var refused = await recovery.RecoverPendingAsync();
            Assert(refused.Count == 1 && !IsWindowVisible(handle) && File.Exists(recordPath), "恢复错误出生时刻的记录没有被拒绝。");
            await File.WriteAllTextAsync(recordPath, recordText);
            Assert((await recovery.RecoverPendingAsync()).Count == 0 && IsWindowVisible(handle) && JournalCount(directory) == 0,
                "同进程恢复记录回放失败。");
            await visibility.RestoreAllAsync();
            passed.Add("journal-replay-and-wrong-birth-refusal");

            var second = MakeWindow(); windows.Add(second); await RenderAsync();
            var secondHandle = new WindowInteropHelper(second).Handle;
            var secondInstance = instance with { Index = 2, TopWindow = secondHandle.ToInt64() };
            await visibility.HideAsync(instance, birth);
            await visibility.HideAsync(secondInstance, birth);
            await visibility.RestoreAllAsync();
            Assert(IsWindowVisible(handle) && IsWindowVisible(secondHandle) && JournalCount(directory) == 0,
                "批量恢复没有恢复所有自有窗口。");
            passed.Add("restore-all-owned-windows");

            await visibility.HideAsync(secondInstance, birth);
            second.Close(); windows.Remove(second);
            await visibility.ShowAsync(2);
            Assert(JournalCount(directory) == 0 && !visibility.IsHidden(2), "窗口关闭后未安全清理自有恢复记录。");
            passed.Add("closed-window-record-cleanup");

            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            try { await visibility.HideAsync(instance, birth, cancellation.Token); throw new InvalidOperationException("取消状态被忽略。"); }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            Assert(IsWindowVisible(handle) && JournalCount(directory) == 0, "已取消操作仍隐藏了窗口。");
            passed.Add("pre-cancelled-hide-no-mutation");

            await File.WriteAllTextAsync(Path.Combine(directory, "result.json"), JsonSerializer.Serialize(new
            {
                subject = "own-process-wpf-windows-only", processId = process.Id, processStartUtcTicks = birth,
                executable, externalWindowsTouched = false, realLdPlayerTested = false,
                desktopSha256 = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(typeof(LdWindowVisibility).Assembly.Location))),
                passed, count = passed.Count, pendingRecords = JournalCount(directory)
            }, JsonDefaults.Options));
        }
        finally
        {
            foreach (var ownWindow in windows) ownWindow.Close();
            await visibility.RestoreAllAsync();
        }
    }

    private static Window MakeWindow()
    {
        var window = new Window
        {
            Title = "本程序窗口恢复自测", Width = 280, Height = 120, Left = -10000, Top = -10000,
            WindowStartupLocation = WindowStartupLocation.Manual, ShowActivated = false, ShowInTaskbar = false,
            Content = new TextBlock { Text = "本程序创建的测试窗口，不是真实雷电实例。", TextWrapping = TextWrapping.Wrap }
        };
        window.Show(); return window;
    }
    private static async Task RenderAsync() => await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
    private static int JournalCount(string directory) => Directory.EnumerateFiles(directory, "ld-window-*.json", SearchOption.AllDirectories).Count();
    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static async Task ExpectAsync(int status, Func<Task> action)
    {
        try { await action(); }
        catch (DeviceException error) when (error.Status == status) { return; }
        throw new InvalidOperationException($"预期窗口操作被拒绝，HTTP 类状态码={status}。");
    }
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindowVisible(IntPtr window);
    [DllImport("user32.dll")] private static extern IntPtr GetParent(IntPtr window);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowLongPtrW")] private static extern IntPtr GetWindowLongPtr(IntPtr window, int index);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetPropW", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetProp(IntPtr window, string name, IntPtr value);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "RemovePropW")] private static extern IntPtr RemoveProp(IntPtr window, string name);
}

```

## src/MatrixDevice.Desktop/Views/LiveUiAcceptance.cs

```csharp
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MatrixDevice.Core;
using MatrixDevice.Features;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task RunLiveUiAcceptanceAsync(string output)
    {
        if (ViewModel.Demo || !Path.IsPathFullyQualified(output) || ViewModel.Runtime.Settings.AdbAllowedSerials.Length != 1 ||
            ViewModel.Runtime.Settings.LdSerialBindings.Count != 1 || ViewModel.Runtime.Settings.LdSerialBindings.Single().Key <= 0)
            throw new InvalidOperationException("实机界面验收只允许独立目录内明确授权的一个非零雷电实例");
        Directory.CreateDirectory(output);
        var records = new List<object>();
        async Task Check(string id, Func<Task> action)
        {
            var start = DateTimeOffset.UtcNow;
            try
            {
                await action();
                if (!string.IsNullOrEmpty(ViewModel.Error)) throw new InvalidOperationException(ViewModel.Error);
                records.Add(new { id, start, status = "PASS-LIVE-UI", serial = ViewModel.CurrentDevice?.Serial, generation = ViewModel.CurrentDevice?.Info.Generation });
            }
            catch (Exception ex) { records.Add(new { id, start, status = "FAIL", error = ex.Message }); throw; }
            finally { await File.WriteAllTextAsync(Path.Combine(output, "ui-results.json"), JsonSerializer.Serialize(records, JsonDefaults.Options)); }
        }
        await Check("UI-connect", async () =>
        {
            await ViewModel.RefreshDevicesAsync();
            if (ViewModel.Devices.Count != 1 || !ViewModel.Devices[0].Info.Online || ViewModel.Devices[0].Serial != ViewModel.Runtime.Settings.AdbAllowedSerials.Single())
                throw new InvalidOperationException("独立测试目标未就绪或串号不符");
            await ViewModel.SetMainAsync(ViewModel.Devices[0]);
        });
        await Check("UI-screenshot", async () =>
        {
            await ViewModel.CaptureAsync(); ViewModel.SamplePixel(10, 10);
            await ViewModel.SaveScreenshotAsync(Path.Combine(output, "guest.png"));
            await ViewModel.SaveScreenshotAsync(Path.Combine(output, "crop.png"), new Int32Rect(10, 10, 40, 40));
            if (PngImage.ReadDimensions(await File.ReadAllBytesAsync(Path.Combine(output, "crop.png"))) != (40, 40)) throw new InvalidOperationException("界面裁剪尺寸不符");
        });
        await Check("UI-nodes", async () =>
        {
            await ViewModel.LoadNodesAsync();
            var node = ViewModel.FlattenNodes().FirstOrDefault(row => row.Source.Bounds.Width > 0) ?? throw new InvalidOperationException("界面真实节点为空");
            ViewModel.SelectedNode = node; NavigateTo("节点"); await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
            if (!nodeTreeItems.TryGetValue(node, out var item) || !item.IsSelected || !nodeRectangles.ContainsKey(node)) throw new InvalidOperationException("真实节点树与矩形选择未联动");
            await ViewModel.ExportNodesAsync(Path.Combine(output, "nodes.json"));
        });
        await Check("UI-apps-shell", async () =>
        {
            await ViewModel.LoadAppsAsync();
            if (!ViewModel.Apps.Any(app => app.PackageName == "com.ss.android.ugc.aweme" && app.VersionName == "40.7.0")) throw new InvalidOperationException("界面APP版本列表不符");
            ViewModel.ShellInput = "getprop sys.boot_completed"; await ViewModel.ExecuteShellAsync();
        });
        await Check("UI-file-queue", async () =>
        {
            var row = ViewModel.CurrentDevice!; var local = Path.Combine(output, "ui-upload.txt"); var pulled = Path.Combine(output, "ui-pulled.txt");
            var remote = "/sdcard/matrix-ui-acceptance-" + Guid.NewGuid().ToString("N") + ".txt";
            await File.WriteAllTextAsync(local, "界面传输验收 " + Guid.NewGuid());
            try
            {
                ViewModel.Transfers.Add(new TransferRow { Serial = row.Serial, Direction = "上传", LocalPath = local, RemotePath = remote });
                ViewModel.Transfers.Add(new TransferRow { Serial = row.Serial, Direction = "下载", LocalPath = pulled, RemotePath = remote });
                await ViewModel.StartTransfersAsync();
                if (ViewModel.Transfers.Any(task => task.State != "传输完成") || !File.ReadAllBytes(local).SequenceEqual(File.ReadAllBytes(pulled))) throw new InvalidOperationException("界面文件队列失败或哈希不一致");
            }
            finally { await ViewModel.Runtime.Backend.ShellAsync(row.Serial, "rm -f " + AdbDeviceBackend.Quote(remote)); }
        });
        await Check("UI-text", async () =>
        {
            var serial = ViewModel.CurrentDevice!.Serial;
            await ViewModel.Runtime.Backend.StopAppAsync(serial, "com.android.settings.intelligence"); await ViewModel.Runtime.Backend.StopAppAsync(serial, "com.android.settings");
            await ViewModel.Runtime.Backend.ShellAsync(serial, "am start -W -a android.settings.SETTINGS"); await Task.Delay(800);
            var control = NodeSearch.FindAll(await ViewModel.Runtime.Backend.DumpAsync(serial), new()).First(node => node.PackageName == "com.android.settings" && (node.Text ?? "").Contains("搜索", StringComparison.Ordinal) && node.Bounds.Width > 0);
            await ViewModel.CaptureAsync();
            var row = ViewModel.CurrentDevice!;
            await ViewModel.SendInputAsync(new InputSnapshot { SourceSerial = serial, SourceGeneration = row.Info.Generation, Generation = ViewModel.Runtime.Input.CurrentGeneration,
                Kind = InputKind.Tap, X = control.Bounds.CenterX, Y = control.Bounds.CenterY, SourceWidth = row.Screenshot!.PixelWidth, SourceHeight = row.Screenshot.PixelHeight });
            await Task.Delay(500);
            ViewModel.TextInput = "界面中文 Test 8";
            if (!ViewModel.TextCommand.CanExecute(null)) throw new InvalidOperationException("实机文本按钮不允许操作");
            await ViewModel.SendTextAsync();
            var fields = NodeSearch.FindAll(await ViewModel.Runtime.Backend.DumpAsync(serial), new() { ResourceId = "android:id/search_src_text" });
            if (!fields.Any(node => node.Text == ViewModel.TextInput)) throw new InvalidOperationException("界面中文输入精确回读不符");
            await File.WriteAllTextAsync(Path.Combine(output, "text-readback.json"), JsonSerializer.Serialize(fields, JsonDefaults.Options));
        });
        await Check("UI-clear-stale-nodes", async () =>
        {
            await ViewModel.CaptureAsync();
            if (ViewModel.Nodes.Count != 0 || ViewModel.SelectedNode is not null || nodeTree.Items.Count != 0 || nodeRectangles.Count != 0) throw new InvalidOperationException("实机新截图仍留旧节点");
            await ViewModel.LoadNodesAsync();
        });
        await Check("UI-navigation", async () =>
        {
            var selected = ViewModel.CurrentDevice; var main = ViewModel.MainDevice;
            foreach (var tab in tabs.Items.Cast<TabItem>())
            {
                NavigateTo(tab.Tag!.ToString()!); await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render);
                if (!ReferenceEquals(selected, ViewModel.CurrentDevice) || !ReferenceEquals(main, ViewModel.MainDevice)) throw new InvalidOperationException("实机导航改变目标");
                SaveRender(Path.Combine(output, $"page-{tabs.SelectedIndex:D2}.png"));
            }
            NavigateTo("设备墙 / 主控");
            foreach (var mode in new[] { "cards", "screens", "list" }) { SetWallViewMode(mode); await Dispatcher.InvokeAsync(UpdateLayout, DispatcherPriority.Render); SaveRender(Path.Combine(output, "wall-" + mode + ".png")); }
            SetWallViewMode("cards");
        });
        await File.WriteAllTextAsync(Path.Combine(output, "build-sha256.txt"), Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(MainWindow).Assembly.Location))));
    }
}

```

## src/MatrixDevice.Desktop/Views/NativeView.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private bool CanLaunchScrcpy() => !ViewModel.Demo && ViewModel.CanWriteDevice() && File.Exists(ViewModel.Runtime.Settings.ScrcpyPath) &&
        (!screenProcesses.TryGetValue(ViewModel.CurrentDevice!.Serial, out var process) || process.HasExited);
    private bool CanEmbedScrcpy() => ViewModel.CanWriteDevice() && screenProcesses.TryGetValue(ViewModel.CurrentDevice!.Serial, out var process) && !process.HasExited &&
        !nativeHosts.Any(host => host.ProcessId == process.Id && host.IsAttached);
    private async Task OpenScreenAsync()
    {
        if (!CanLaunchScrcpy()) return;
        if (!ViewModel.Confirm("外部投屏客户端风险", "启动scrcpy会运行其外部ADB客户端，可能影响现有ADB服务。当前程序会在服务身份变化时拒绝继续。确认已核验该工具并接受此风险？")) return;
        await ViewModel.ExecuteAsync("启动 scrcpy", async () =>
        {
            var row = ViewModel.CurrentDevice ?? throw new InvalidOperationException("未选择设备");
            if (ViewModel.Demo) throw new InvalidOperationException("演示模式不启动外部 scrcpy");
            var path = ViewModel.Runtime.Settings.ScrcpyPath;
            if (!File.Exists(path)) throw new FileNotFoundException("请在设置中配置有效 scrcpy 路径", path);
            if (screenProcesses.TryGetValue(row.Serial, out var existing) && !existing.HasExited) throw new InvalidOperationException("该设备已有本程序启动的投屏窗口");
            var start = new ProcessStartInfo(path) { UseShellExecute = false, CreateNoWindow = true };
            foreach (var argument in new[] { "--serial", row.Serial, "--window-title", "MatrixDevice · " + row.Serial, "--max-size", ViewModel.Runtime.Settings.MainMaxSize.ToString(), "--max-fps", ViewModel.Runtime.Settings.MainMaxFps.ToString(), "--video-bit-rate", ViewModel.Runtime.Settings.MainBitRate.ToString(), "--no-audio" }) start.ArgumentList.Add(argument);
            if (ViewModel.Runtime.Settings.ShowFps) start.ArgumentList.Add("--print-fps");
            var process = Process.Start(start) ?? throw new InvalidOperationException("无法创建 scrcpy 进程"); screenProcesses[row.Serial] = process; row.ScreenState = "scrcpy 进程已启动 · 待窗口核验";
            await Task.Delay(400); if (process.HasExited) throw new InvalidOperationException($"scrcpy 已退出，退出码 {process.ExitCode}");
        });
    }

    private async Task EmbedScreenAsync()
    {
        await ViewModel.ExecuteAsync("嵌入投屏窗口", async () =>
        {
            var row = ViewModel.CurrentDevice ?? throw new InvalidOperationException("未选择设备");
            if (!screenProcesses.TryGetValue(row.Serial, out var process) || process.HasExited) throw new InvalidOperationException("请先启动本程序管理的 scrcpy 投屏");
            if (nativeHosts.Any(host => host.ProcessId == process.Id && host.IsAttached)) throw new InvalidOperationException("窗口已嵌入");
            for (var retry = 0; retry < 20 && process.MainWindowHandle == IntPtr.Zero; retry++) { await Task.Delay(100); process.Refresh(); }
            if (process.MainWindowHandle == IntPtr.Zero) throw new InvalidOperationException("投屏窗口尚未出现");
            var host = new NativeWindowHost(process, System.IO.Path.Combine(ViewModel.DataDirectory, "window-recovery"));
            nativeHosts.Add(host);
            var window = new Window { Owner = this, Title = "投屏 · " + row.Serial, Width = 600, Height = 850, Content = host, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            window.Closing += (_, args) => { try { host.Restore(); nativeHosts.Remove(host); } catch (Exception ex) { args.Cancel = true; MessageBox.Show(window, ViewModel.PresentError(ex), "窗口恢复失败", MessageBoxButton.OK, MessageBoxImage.Error); } };
            window.Show(); row.ScreenState = "已嵌入自有 scrcpy 窗口";
        });
    }

    private Task RestoreScreensAsync() => ViewModel.ExecuteAsync("恢复投屏窗口", async () =>
    {
        var failures = new List<string>(); foreach (var host in nativeHosts.ToList()) { try { host.Restore(); } catch (Exception ex) { failures.Add(ViewModel.PresentError(ex)); } }
        if (failures.Count > 0) throw new InvalidOperationException(string.Join("\n", failures)); await Task.CompletedTask;
    });

}

```

## src/MatrixDevice.Desktop/Views/NetworkScannerView.cs

```csharp
using System.Windows;
using System.Windows.Controls;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private CancellationTokenSource? scanCancellation;
    private Task<NetworkScanResult>? scanOperation;

    private FrameworkElement BuildScanner()
    {
        var panel = new StackPanel { Margin = new Thickness(0, 5, 0, 7) };
        var startIp = new TextBox { Text = "127.0.0.1" };
        var endIp = new TextBox { Text = "127.0.0.1" };
        var port = new TextBox { Text = "5555" };
        var timeout = new TextBox { Text = "1500" };
        panel.Children.Add(Field("起始 IP", startIp, 70));
        panel.Children.Add(Field("结束 IP", endIp, 70));
        panel.Children.Add(Field("端口", port, 70));
        panel.Children.Add(Field("超时 ms", timeout, 70));
        var summary = new TextBlock { Text = "未扫描", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) };
        AsyncCommand? scan = null;
        ActionCommand? cancel = null;
        scan = new AsyncCommand(async () =>
        {
            if (ViewModel.Runtime.Backend is not AdbDeviceBackend backend || ViewModel.Demo) return;
            if (!int.TryParse(port.Text, out var remotePort) || !int.TryParse(timeout.Text, out var timeoutMs)) { summary.Text = "端口和超时必须为整数"; return; }
            scanCancellation = new CancellationTokenSource(); cancel?.Refresh(); scan?.Refresh();
            startIp.IsEnabled = endIp.IsEnabled = port.IsEnabled = timeout.IsEnabled = false;
            try
            {
                var progress = new Progress<NetworkScanProgress>(value => summary.Text = $"探测 {value.Completed}/{value.Total}\n发现 {value.Found} 连接 {value.Connected}\n{value.CurrentAddress}");
                scanOperation = new NetworkDeviceScanner(backend).ScanAsync(startIp.Text.Trim(), endIp.Text.Trim(), remotePort, probeTimeoutMs: timeoutMs, progress: progress, ct: scanCancellation.Token);
                var result = await scanOperation;
                if (closing) return;
                summary.Text = $"扫描结束\n目标 {result.Total} 发现 {result.Found} 连接 {result.Connected}";
                await ViewModel.RefreshDevicesAsync();
                if (result.Endpoints.Any(endpoint => endpoint.Found && !endpoint.Connected)) ShowText("网络发现结果", string.Join("\n", result.Endpoints.Select(endpoint => $"{endpoint.Address}:{endpoint.Port} · {(endpoint.Connected ? "已连接" : endpoint.Found ? "连接失败" : "未发现")} · {endpoint.Error}")));
            }
            catch (OperationCanceledException) { summary.Text += "\n扫描已停止"; }
            catch (Exception ex) { summary.Text = "扫描失败：" + ViewModel.PresentError(ex); }
            finally
            {
                scanCancellation.Dispose(); scanCancellation = null; scanOperation = null;
                startIp.IsEnabled = endIp.IsEnabled = port.IsEnabled = timeout.IsEnabled = true;
                cancel?.Refresh(); scan?.Refresh();
            }
        }, () => scanCancellation is null && !ViewModel.Demo && ViewModel.Runtime.Backend is AdbDeviceBackend && !ViewModel.Busy);
        cancel = new ActionCommand(() => scanCancellation?.Cancel(), () => scanCancellation is not null);
        var controls = Toolbar(); controls.Children.Add(Button("\uE721", "扫描", scan)); controls.Children.Add(Button("\uE71A", "停止", cancel)); panel.Children.Add(controls); panel.Children.Add(summary);
        ViewModel.PropertyChanged += (_, _) => scan.Refresh();
        var header = new TextBlock { MaxWidth = 185, TextWrapping = TextWrapping.Wrap };
        LocalizationService.Bind(header, TextBlock.TextProperty, ViewModel.Demo ? "网络发现 · 演示模式禁用" : "网络发现");
        var expander = new Expander { Header = header, Content = panel, Margin = new Thickness(0, 6, 0, 0) };
        return expander;
    }
}

```

## src/MatrixDevice.Desktop/Views/OneClickStartAcceptance.cs

```csharp
using System.IO;
using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task RunOneClickStartAcceptanceAsync(string output, string approvedCandidate)
    {
        var runtime = ViewModel.Runtime;
        if (ViewModel.Demo || !Path.IsPathFullyQualified(output) || !ViewModel.DataDirectory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || runtime.Settings.AdbAllowedSerials.Length != 0)
            throw new InvalidOperationException("冷启动验收必须使用无初始授权的隔离artifacts工作区。");
        var runId = Guid.NewGuid().ToString("N");
        var confirm = ViewModel.Confirm; var confirmations = 0;
        var passed = false; var hidden = false; var colors = 0;
        var othersUnchanged = false; var otherCount = 0; var playerPid = 0; var vboxPid = 0;
        string? failure = null;
        Directory.CreateDirectory(output);
        var resultPath = Path.Combine(output, "result.json");
        await File.WriteAllTextAsync(resultPath, JsonSerializer.Serialize(new { runId, passed = false, state = "running" }, JsonDefaults.Options));
        try
        {
            var target = runtime.LdPlayer!.Instances.Single(item => item.Index == 1);
            if (!target.Name.StartsWith("MatrixAcceptance-", StringComparison.Ordinal) || target.AndroidStarted || target.ProcessId > 0 || target.VBoxProcessId > 0)
                throw new InvalidOperationException("仅允许已关闭的专用测试实例1，不能为验收关闭其它实例。");
            var original = runtime.LdPlayer.Instances.Where(item => item.Index != 1).ToArray();
            otherCount = original.Length;
            ViewModel.Confirm = (_, _) => { confirmations++; throw new InvalidOperationException("一键启动出现确认框。"); };
            await ConnectLdInstancesAsync([1], new Dictionary<int, string> { [1] = approvedCandidate });
            if (confirmations != 0 || !LdInstanceConnected(1) || ViewModel.Runtime.Settings.LdSerialBindings.GetValueOrDefault(1) != approvedCandidate || ViewModel.Devices.Count != 1)
                throw new InvalidOperationException("一键冷启动未完成指定实例连接：" + ldFeedback.Text);
            hidden = ldWindowVisibility?.IsHidden(1) == true;
            if (!hidden) throw new InvalidOperationException("新启动的雷电窗口未保持隐藏。");
            var png = await ViewModel.Runtime.Backend.ScreenshotAsync(approvedCandidate);
            var size = PngImage.ReadDimensions(png);
            colors = Enumerable.Range(0, 16).Select(i => PngImage.ColorAt(png, i * (size.Width - 1) / 15, i * (size.Height - 1) / 15)).Distinct().Count();
            if (colors < 2) throw new InvalidOperationException("后台截图未检出有效画面。");
            await File.WriteAllBytesAsync(Path.Combine(output, "hidden-device.png"), png);
            await ViewModel.Runtime.RefreshLdPlayerAsync();
            var launched = ViewModel.Runtime.LdPlayer!.Instances.Single(item => item.Index == 1);
            playerPid = launched.ProcessId; vboxPid = launched.VBoxProcessId;
            if (!launched.AndroidStarted || playerPid <= 0 || vboxPid <= 0)
                throw new InvalidOperationException("截图后实例未保持运行。");
            foreach (var before in original)
            {
                var after = ViewModel.Runtime.LdPlayer!.Instances.SingleOrDefault(item => item.Index == before.Index);
                if (after is null || after.Name != before.Name || after.ProcessId != before.ProcessId || after.VBoxProcessId != before.VBoxProcessId || after.AndroidStarted != before.AndroidStarted)
                    throw new InvalidOperationException("测试期间其它实例状态发生变化，不能标记互不干扰通过。");
            }
            othersUnchanged = true;
            SetWallViewMode("list"); UpdateUnifiedDirectory();
            await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
            await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
            SaveRender(Path.Combine(output, "connected.png"));
            passed = true;
        }
        catch (Exception ex) { failure = ex.ToString(); throw; }
        finally
        {
            ViewModel.Confirm = confirm;
            Exception? restoreFailure = null;
            try { if (ldWindowVisibility is not null) await ldWindowVisibility.ShowAsync(1); }
            catch (Exception ex) { restoreFailure = ex; passed = false; }
            await File.WriteAllTextAsync(resultPath, JsonSerializer.Serialize(new
            {
                runId, passed, state = "finished", failure, restorationFailure = restoreFailure?.ToString(),
                index = 1, startupConfirmationDialogs = confirmations, hiddenVerified = hidden,
                screenshotSampleColors = colors, restored = ldWindowVisibility?.IsHidden(1) != true,
                emulatorShutdownNotRequested = true, otherInstanceCount = otherCount, othersUnchanged,
                launchAttempted = coldLaunched.Contains(1), observedPlayerPid = playerPid,
                observedVBoxPid = vboxPid, trueHeadless = false
            }, JsonDefaults.Options));
            if (restoreFailure is not null) throw new InvalidOperationException("验收窗口恢复失败，详见本轮result.json。", restoreFailure);
        }
    }
}

```

## src/MatrixDevice.Desktop/Views/ResponsiveColumnsConverter.cs

```csharp
using System.Globalization;
using System.Windows.Data;

namespace MatrixDevice.Desktop;

public sealed class ResponsiveColumnsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var preferred = values.Length > 0 && values[0] is int count ? count : 5;
        var width = values.Length > 1 && values[1] is double available ? available : 700;
        return Math.Max(1, Math.Min(preferred, (int)Math.Floor(width / 135)));
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

```

## src/MatrixDevice.Desktop/Views/ScreenshotView.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private FrameworkElement BuildScreenshotPage()
    {
        var page = Page();
        var toolbar = Toolbar(); toolbar.Children.Add(Button("\uE722", "刷新截图", ViewModel.ScreenshotCommand)); toolbar.Children.Add(Button("\uE74E", "保存 PNG", async () => await SaveImageAsync(false))); toolbar.Children.Add(Button("\uE7A8", "保存裁剪", async () => await SaveImageAsync(true)));
        screenshotMode = new ComboBox { ItemsSource = new[] { "取色", "裁剪" }, ItemTemplate = CaptionChoices(), SelectedIndex = 0, Width = 115 }; toolbar.Children.Add(screenshotMode);
        var zoom = new Slider { Minimum = .1, Maximum = 3, Value = 1, Width = 150, VerticalAlignment = VerticalAlignment.Center, ToolTip = "缩放" }; zoom.ValueChanged += (_, _) => { screenshotScale.ScaleX = zoom.Value; screenshotScale.ScaleY = zoom.Value; }; toolbar.Children.Add(zoom); toolbar.Children.Add(Button("\uE740", "1:1", () => zoom.Value = 1));
        DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var colors = new DataGrid { ItemsSource = ViewModel.Colors, IsReadOnly = true, Width = 300, Margin = new Thickness(10, 10, 0, 0) }; TextColumn(colors, "坐标", nameof(PixelSample.Position), 70); TextColumn(colors, "HEX", nameof(PixelSample.Hex), 80); TextColumn(colors, "RGB", nameof(PixelSample.Rgb), 140); colors.MouseDoubleClick += (_, _) => { if (colors.SelectedItem is PixelSample color) Clipboard.SetText($"{color.Hex} {color.Rgb}"); }; DockPanel.SetDock(colors, Dock.Right); page.Children.Add(colors);
        screenshotCanvas = new Canvas { Background = new SolidColorBrush(Color.FromRgb(232, 237, 240)), LayoutTransform = screenshotScale };
        screenshotImage = new Image { Stretch = Stretch.None }; screenshotCanvas.Children.Add(screenshotImage);
        cropRectangle = new Rectangle { Stroke = new SolidColorBrush(Color.FromRgb(214, 69, 94)), StrokeThickness = 2, Fill = new SolidColorBrush(Color.FromArgb(35, 214, 69, 94)), Visibility = Visibility.Collapsed, IsHitTestVisible = false }; screenshotCanvas.Children.Add(cropRectangle);
        screenshotCanvas.MouseLeftButtonDown += ScreenshotDown; screenshotCanvas.MouseMove += ScreenshotMove; screenshotCanvas.MouseLeftButtonUp += ScreenshotUp;
        screenshotScroll = new ScrollViewer { Content = screenshotCanvas, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Margin = new Thickness(0, 10, 0, 0) }; page.Children.Add(screenshotScroll);
        return page;
    }

    private async Task SaveImageAsync(bool useCrop)
    {
        if (useCrop && crop is null) { MessageBox.Show(this, "尚未选择裁剪区域", "保存裁剪", MessageBoxButton.OK, MessageBoxImage.Information); return; }
        var dialog = new SaveFileDialog { Filter = "PNG|*.png", FileName = useCrop ? "crop.png" : "screenshot.png" };
        if (dialog.ShowDialog(this) == true) await ViewModel.SaveScreenshotAsync(dialog.FileName, useCrop ? crop : null);
    }

    private Point ClampScreenshot(Point point) => ViewModel.DebugImage is { } image ? new Point(Math.Clamp(point.X, 0, image.PixelWidth - 1), Math.Clamp(point.Y, 0, image.PixelHeight - 1)) : default;
    private void ScreenshotDown(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel.DebugImage is null) return;
        var point = ClampScreenshot(e.GetPosition(screenshotCanvas));
        if (screenshotMode.SelectedIndex == 0) ViewModel.SamplePixel((int)point.X, (int)point.Y);
        else { cropStart = point; crop = null; cropRectangle.Visibility = Visibility.Visible; cropRectangle.Width = cropRectangle.Height = 0; screenshotCanvas.CaptureMouse(); }
    }
    private void ScreenshotMove(object sender, MouseEventArgs e)
    {
        if (cropStart is not { } start) return;
        var point = ClampScreenshot(e.GetPosition(screenshotCanvas));
        Canvas.SetLeft(cropRectangle, Math.Min(start.X, point.X)); Canvas.SetTop(cropRectangle, Math.Min(start.Y, point.Y)); cropRectangle.Width = Math.Abs(start.X - point.X); cropRectangle.Height = Math.Abs(start.Y - point.Y);
    }
    private void ScreenshotUp(object sender, MouseButtonEventArgs e)
    {
        if (cropStart is not { } start || ViewModel.DebugImage is not { } image) return;
        var end = ClampScreenshot(e.GetPosition(screenshotCanvas)); var x = (int)Math.Min(start.X, end.X); var y = (int)Math.Min(start.Y, end.Y); var width = (int)Math.Abs(start.X - end.X); var height = (int)Math.Abs(start.Y - end.Y);
        crop = width > 0 && height > 0 ? new Int32Rect(x, y, Math.Min(width, image.PixelWidth - x), Math.Min(height, image.PixelHeight - y)) : null;
        cropStart = null; screenshotCanvas.ReleaseMouseCapture();
    }

    private static Point? MapImage(Image image, Point point)
    {
        if (image.Source is not BitmapSource source || source.PixelWidth <= 0 || source.PixelHeight <= 0 || image.ActualWidth <= 0 || image.ActualHeight <= 0) return null;
        var scale = Math.Min(image.ActualWidth / source.PixelWidth, image.ActualHeight / source.PixelHeight); var left = (image.ActualWidth - source.PixelWidth * scale) / 2; var top = (image.ActualHeight - source.PixelHeight * scale) / 2;
        var x = (point.X - left) / scale; var y = (point.Y - top) / scale;
        return x < 0 || y < 0 || x >= source.PixelWidth || y >= source.PixelHeight ? null : new Point(x, y);
    }
}

```

## src/MatrixDevice.Desktop/Views/ScriptsView.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private FrameworkElement BuildScriptPage()
    {
        var page = Page(); var toolbar = Toolbar(); toolbar.Children.Add(Button("\uE8E5", "导入脚本包", async () => { var dialog = new OpenFileDialog { Filter = "脚本包|*.jxscript;*.zip;*.json|所有文件|*.*" }; if (dialog.ShowDialog(this) == true) await ViewModel.ImportScriptAsync(dialog.FileName); })); toolbar.Children.Add(Button("\uE768", "运行所选", ViewModel.StartScriptCommand)); toolbar.Children.Add(Button("\uE71A", "停止所选", ViewModel.StopScriptCommand)); DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var summary = BoundText(nameof(WorkspaceViewModel.PackageSummary), true); summary.FontWeight = FontWeights.SemiBold; summary.Margin = new Thickness(0, 10, 0, 8); DockPanel.SetDock(summary, Dock.Top); page.Children.Add(summary);
        var targets = BoundText(nameof(WorkspaceViewModel.TargetContext), true); DockPanel.SetDock(targets, Dock.Top); page.Children.Add(targets);
        var grid = new Grid { Margin = new Thickness(0, 10, 0, 0) }; grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(230) }); grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); grid.RowDefinitions.Add(new RowDefinition());
        var parameters = new DataGrid { ItemsSource = ViewModel.Parameters }; TextColumn(parameters, "参数", nameof(ParameterEditor.Key), 170, true); TextColumn(parameters, "名称", nameof(ParameterEditor.Label), 180, true); TextColumn(parameters, "类型", nameof(ParameterEditor.Type), 100, true); TextColumn(parameters, "范围/单位", nameof(ParameterEditor.Range), 150, true); TextColumn(parameters, "值（JSON）", nameof(ParameterEditor.Value), 230, false); grid.Children.Add(parameters);
        var label = Label("当前设备日志 · 最近 500 条"); label.Margin = new Thickness(0, 10, 0, 8); Grid.SetRow(label, 1); grid.Children.Add(label);
        var logs = new ListBox { ItemsSource = ViewModel.ScriptLogs, FontFamily = new FontFamily("Consolas"), FontSize = 12, BorderBrush = new SolidColorBrush(Color.FromRgb(212, 221, 225)) }; Grid.SetRow(logs, 2); grid.Children.Add(logs); page.Children.Add(grid); return page;
    }

}

```

## src/MatrixDevice.Desktop/Views/SettingsView.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private FrameworkElement BuildSettingsPage()
    {
        var page = Page();
        var toolbar = Toolbar();
        toolbar.Children.Add(Button("\uE74E", "保存设置", ViewModel.SaveSettingsCommand));
        var mode = BoundText(nameof(WorkspaceViewModel.ModeLabel)); mode.Foreground = Brushes.DimGray; toolbar.Children.Add(mode);
        DockPanel.SetDock(toolbar, Dock.Top);
        page.Children.Add(toolbar);
        var notice = BoundText(nameof(WorkspaceViewModel.SettingsNotice), true);
        notice.Margin = new Thickness(0, 8, 0, 0);
        DockPanel.SetDock(notice, Dock.Top);
        page.Children.Add(notice);
        var grid = new Grid { Margin = new Thickness(0, 10, 0, 0), MaxWidth = 980 };
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        var left = new StackPanel();
        left.Children.Add(Label("连接与接口"));
        left.Children.Add(PathField("ADB 路径", "Settings.AdbPath"));
        left.Children.Add(Field("授权设备串号 JSON", BoundInput("Settings.AdbSerialsJson", multiline: true)));
        left.Children.Add(PathField("scrcpy 路径", "Settings.ScrcpyPath"));
        left.Children.Add(PathField("雷电命令行路径", "Settings.LdConsolePath"));
        var bindings = BoundInput("Settings.LdBindingsJson", multiline: true); bindings.Height = 85; bindings.ToolTip = "例如：{\"0\":\"实际发现的设备序列号\"}；仅保存显式绑定配置，不表示已核验";
        left.Children.Add(Field("实例绑定（JSON）", bindings));
        left.Children.Add(Field("授权 Agent 地址", BoundInput("Settings.AgentBaseUrl")));
        left.Children.Add(Field("Agent 设备 serial", BoundInput("Settings.AgentSerial")));
        left.Children.Add(Field("普通 API 端口", BoundInput("Settings.BasicApiPort")));
        left.Children.Add(Field("Automator 端口", BoundInput("Settings.AutomatorApiPort")));
        left.Children.Add(Check("启动时连接设备", "Settings.AutoConnect"));
        left.Children.Add(Check("启动时打开 API", "Settings.AutoStartApi"));
        var language = new TextBlock { Text = "简体中文", VerticalAlignment = VerticalAlignment.Center };
        left.Children.Add(Field("语言 / 区域", language));
        left.Children.Add(Field("文件交换目录", BoundInput("Settings.FileRoot")));
        left.Children.Add(Field("命令超时（秒）", BoundInput("Settings.CommandTimeoutSeconds")));
        left.Children.Add(Button("\uE777", "恢复遗留投屏窗口", async () => await ViewModel.ExecuteAsync("恢复遗留窗口", async () =>
        {
            var directory = System.IO.Path.Combine(ViewModel.DataDirectory, "window-recovery");
            var failures = NativeWindowHost.RecoverPending(directory);
            if (failures.Count > 0) throw new InvalidOperationException(string.Join("\n", failures));
            await Task.CompletedTask;
        })));
        grid.Children.Add(left);
        var right = new StackPanel { Margin = new Thickness(25, 0, 0, 0) };
        right.Children.Add(Label("投屏与窗体"));
        right.Children.Add(Field("主屏最大尺寸（px）", BoundInput("Settings.MainMaxSize")));
        right.Children.Add(UnavailableSetting("小屏最大尺寸（px）", BoundInput("Settings.MiniMaxSize"), "当前未接入小屏视频编码参数；值仅保留用于文档对照"));
        right.Children.Add(Field("主屏帧率（FPS）", BoundInput("Settings.MainMaxFps")));
        right.Children.Add(UnavailableSetting("小屏帧率（FPS）", BoundInput("Settings.MiniMaxFps"), "当前未接入小屏视频编码参数；值仅保留用于文档对照"));
        right.Children.Add(Field("主屏码率（bit/s）", BoundInput("Settings.MainBitRate")));
        right.Children.Add(UnavailableSetting("小屏码率（bit/s）", BoundInput("Settings.MiniBitRate"), "当前未接入小屏视频编码参数；值仅保留用于文档对照"));
        var hardware = Check("硬件解码偏好", "Settings.ForceHardwareDecode"); hardware.IsEnabled = false; ToolTipService.SetShowOnDisabled(hardware, true); LocalizationService.Bind(hardware, ToolTipProperty, "当前未接入硬件解码选择；值仅保留用于文档对照"); right.Children.Add(hardware);
        right.Children.Add(Check("显示 FPS", "Settings.ShowFps"));
        right.Children.Add(Field("窗体宽度", BoundInput("Settings.WindowWidth")));
        right.Children.Add(Field("窗体高度", BoundInput("Settings.WindowHeight")));
        Grid.SetColumn(right, 1);
        grid.Children.Add(right);
        page.Children.Add(new ScrollViewer { Content = grid, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        return page;
    }

    private FrameworkElement PathField(string label, string binding)
    {
        var row = new DockPanel(); var browse = Button("\uE8B7", "浏览", () =>
        {
            var dialog = new OpenFileDialog { Filter = "程序|*.exe|所有文件|*.*" };
            if (dialog.ShowDialog(this) != true) return;
            if (binding.EndsWith("AdbPath", StringComparison.Ordinal)) { ViewModel.Settings.AdbPath = dialog.FileName; ViewModel.Settings.Changed(nameof(SettingsEditor.AdbPath)); }
            else if (binding.EndsWith("LdConsolePath", StringComparison.Ordinal)) { ViewModel.Settings.LdConsolePath = dialog.FileName; ViewModel.Settings.Changed(nameof(SettingsEditor.LdConsolePath)); }
            else { ViewModel.Settings.ScrcpyPath = dialog.FileName; ViewModel.Settings.Changed(nameof(SettingsEditor.ScrcpyPath)); }
        }); DockPanel.SetDock(browse, Dock.Right); row.Children.Add(browse); row.Children.Add(BoundInput(binding)); return Field(label, row);
    }

    private static FrameworkElement UnavailableSetting(string label, FrameworkElement control, string reason)
    {
        var row = Field(label, control); row.IsEnabled = false;
        ToolTipService.SetShowOnDisabled(row, true); LocalizationService.Bind(row, ToolTipProperty, reason);
        return row;
    }

}

```

## src/MatrixDevice.Desktop/Views/ShellView.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private FrameworkElement BuildShellPage()
    {
        var page = Page();
        var toolbar = Toolbar(); var input = BoundInput(nameof(WorkspaceViewModel.ShellInput)); input.Width = 470; input.FontFamily = new FontFamily("Consolas"); input.KeyDown += ShellHistoryKey; toolbar.Children.Add(input); toolbar.Children.Add(Button("\uE768", "执行", ViewModel.ShellCommand));
        DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var history = new ListBox { ItemsSource = ViewModel.ShellHistory, BorderThickness = new Thickness(0), HorizontalContentAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 10, 0, 0) };
        var panel = new FrameworkElementFactory(typeof(StackPanel)); panel.SetValue(MarginProperty, new Thickness(0, 0, 0, 15));
        var command = new FrameworkElementFactory(typeof(TextBlock)); command.SetBinding(TextBlock.TextProperty, new Binding(nameof(ShellEntry.Command))); command.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold); command.SetValue(TextBlock.FontFamilyProperty, new FontFamily("Consolas")); panel.AppendChild(command);
        var time = new FrameworkElementFactory(typeof(TextBlock)); time.SetBinding(TextBlock.TextProperty, new Binding(nameof(ShellEntry.Context))); time.SetValue(TextBlock.ForegroundProperty, Brushes.Gray); panel.AppendChild(time);
        var output = new FrameworkElementFactory(typeof(TextBox)); output.SetBinding(TextBox.TextProperty, new Binding(nameof(ShellEntry.Output))); output.SetValue(TextBox.IsReadOnlyProperty, true); output.SetValue(TextBox.TextWrappingProperty, TextWrapping.Wrap); output.SetValue(TextBox.AcceptsReturnProperty, true); output.SetValue(FontFamilyProperty, new FontFamily("Consolas")); panel.AppendChild(output); history.ItemTemplate = new DataTemplate { VisualTree = panel };
        history.MouseDoubleClick += (_, _) => { if (history.SelectedItem is ShellEntry entry) ViewModel.ShellInput = entry.Command; }; page.Children.Add(history); return page;
    }

    private void ShellHistoryKey(object sender, KeyEventArgs args)
    {
        if (args.Key == Key.Enter) { if (ViewModel.ShellCommand.CanExecute(null)) ViewModel.ShellCommand.Execute(null); shellHistoryCursor = -1; args.Handled = true; return; }
        if (args.Key is not Key.Up and not Key.Down || ViewModel.ShellHistory.Count == 0) return;
        shellHistoryCursor = Math.Clamp(shellHistoryCursor + (args.Key == Key.Up ? 1 : -1), -1, ViewModel.ShellHistory.Count - 1); ViewModel.ShellInput = shellHistoryCursor < 0 ? "" : ViewModel.ShellHistory[shellHistoryCursor].Command; if (sender is TextBox input) input.CaretIndex = input.Text.Length; args.Handled = true;
    }

}

```

## src/MatrixDevice.Desktop/Views/TextView.cs

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MatrixDevice.Core;
using Microsoft.Win32;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private FrameworkElement BuildTextPage()
    {
        var page = Page(); var toolbar = Toolbar(); toolbar.Children.Add(Button("\uE8F4", "发送文本", ViewModel.TextCommand)); toolbar.Children.Add(Button("\uE8A5", "粘贴", () => { if (Clipboard.ContainsText()) ViewModel.TextInput = Clipboard.GetText(); }));
        toolbar.Children.Add(Button("\uE8E5", "加载文本文件", async () => { var dialog = new OpenFileDialog { Filter = "文本|*.txt;*.md;*.json|所有文件|*.*" }; if (dialog.ShowDialog(this) == true) await ViewModel.ExecuteAsync("加载文本", async () => ViewModel.TextInput = await File.ReadAllTextAsync(dialog.FileName)); })); DockPanel.SetDock(toolbar, Dock.Top); page.Children.Add(toolbar);
        var target = BoundText(nameof(WorkspaceViewModel.TargetContext), true); target.Margin = new Thickness(0, 8, 0, 8); DockPanel.SetDock(target, Dock.Top); page.Children.Add(target);
        var input = BoundInput(nameof(WorkspaceViewModel.TextInput), multiline: true); input.VerticalContentAlignment = VerticalAlignment.Top; page.Children.Add(input); return page;
    }

}

```

## src/MatrixDevice.Desktop/Views/UiControlAudit.cs

```csharp
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private async Task VerifyOneClickProjectionAsync()
    {
        NavigateTo("设备墙 / 主控"); UpdateUnifiedDirectory();
        await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
        var originalDevices = ViewModel.Devices.Count;
        var originalSelection = unifiedDevices.ToDictionary(row => row.Key, row => row.IsSelected);
        if (unifiedDevices.Select(row => row.Key).Distinct().Count() != unifiedDevices.Count || wallWorkspace.RowDefinitions.Count != 1) throw new InvalidOperationException("统一目录重复或仍有底部独立卡片区。");
        if (!unifiedDevices.Any(row => row.Instance is { AndroidStarted: false } && row.Device is null)) throw new InvalidOperationException("统一目录遗漏关闭实例。");
        try
        {
            foreach (var row in unifiedDevices.ToArray()) row.IsSelected = false;
            var cold = unifiedDevices.First(row => row.Instance is { Index: > 0, AndroidStarted: false } && row.Device is null);
            cold.Update(cold.Instance, null, false, "已连接");
            if (cold.State == "已连接") throw new InvalidOperationException("连接失效后仍显示历史已连接阶段。");
            UpdateUnifiedDirectory();
            cold.IsSelected = true;
            if (!workspaceCounts.Text.EndsWith("已选 1", StringComparison.Ordinal)) throw new InvalidOperationException("关闭实例的选择数量没有立即更新。");
            if (ViewModel.Targets().Count != 0 || ViewModel.CanWriteTargets()) throw new InvalidOperationException("只选择关闭实例时仍回落到旧当前设备。");
            var transfers = ViewModel.Transfers.Count;
            try { ViewModel.QueuePush(typeof(MainWindow).Assembly.Location, false); throw new InvalidOperationException("未连接选择被静默忽略。"); }
            catch (MatrixDevice.Core.DeviceException) { }
            if (ViewModel.Transfers.Count != transfers) throw new InvalidOperationException("未连接选择导致其它设备被加入传输。");
            ViewModel.Devices[0].IsSelected = true;
            if (ViewModel.Targets().Count != 0 || ViewModel.CanWriteTargets()) throw new InvalidOperationException("混合选择静默遗漏未连接设备。");
            var all = VisualDescendants<Button>(Shell).Single(button => button.ToolTip as string == "选择当前筛选中的全部设备");
            all.Command!.Execute(null);
            if (unifiedDevices.Any(row => row.CanSelect && !row.IsSelected) || ViewModel.Devices.Count != originalDevices) throw new InvalidOperationException("全选未涵盖关闭实例或伪造了可控设备。");
            UpdateUnifiedDirectory();
            if (unifiedDevices.Any(row => row.CanSelect && !row.IsSelected)) throw new InvalidOperationException("自动目录更新丢失选择。");
        }
        finally { foreach (var row in unifiedDevices.ToArray()) row.IsSelected = originalSelection.GetValueOrDefault(row.Key); }
        var originalConfirm = ViewModel.Confirm; var confirmations = 0;
        ViewModel.Confirm = (_, _) => { confirmations++; return false; };
        try
        {
            foreach (var invalid in new Dictionary<int, string>[] { new(), new() { [1] = "demo", [2] = "extra" }, new() { [1] = "" } })
            {
                try { await ConnectLdInstancesAsync([1], invalid); throw new InvalidOperationException("不完整的预批准串号进入启动流程。"); }
                catch (MatrixDevice.Core.DeviceException ex) when (ex.Status == 400 && ex.Code == 1007) { }
            }
            try { await ConnectLdInstancesAsync([1]); throw new InvalidOperationException("演示模式进入真实启动。"); }
            catch (MatrixDevice.Core.DeviceException ex) when (ex.Message.Contains("演示", StringComparison.Ordinal)) { }
            if (confirmations != 0) throw new InvalidOperationException("普通启动仍弹出确认框。");
        }
        finally { ViewModel.Confirm = originalConfirm; }
    }

    private async Task VerifyOperationGatesAsync()
    {
        var current = ViewModel.CurrentDevice!; var originalInfo = current.Info;
        try
        {
            current.Info = originalInfo with { State = "Offline" };
            if (!ViewModel.DeviceContext.Contains("离线", StringComparison.Ordinal) || ViewModel.CanUseDevice()) throw new InvalidOperationException("当前设备标题或命令仍显示旧在线状态");
        }
        finally { current.Info = originalInfo; }
        if (ViewModel.Devices.Count > 1)
        {
            var selections = ViewModel.Devices.Select(row => row.IsSelected).ToArray(); var previousText = ViewModel.TextInput;
            try
            {
                foreach (var row in ViewModel.Devices) row.IsSelected = false;
                var other = ViewModel.Devices.First(row => !ReferenceEquals(row, current)); other.IsSelected = true;
                current.Info = originalInfo with { State = "Offline" }; ViewModel.TextInput = "测试"; ViewModel.UpdateSelection();
                if (!ViewModel.CanWriteTargets() || ViewModel.CanWriteDevice() || !ViewModel.TextCommand.CanExecute(null)) throw new InvalidOperationException("批量目标和当前设备的门控作用域混淆");
                NavigateTo("文件"); await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
                var download = VisualDescendants<Button>(Shell).Single(button => button.ToolTip as string == "添加下载");
                var upload = VisualDescendants<Button>(Shell).Single(button => button.ToolTip as string == "添加上传");
                if (download.Command!.CanExecute(null) || !upload.Command!.CanExecute(null)) throw new InvalidOperationException("上传/下载控件作用域与执行目标不一致");
            }
            finally { current.Info = originalInfo; ViewModel.TextInput = previousText; for (var i = 0; i < selections.Length; i++) ViewModel.Devices[i].IsSelected = selections[i]; ViewModel.UpdateSelection(); }
        }
        var pending = new TransferRow { Serial = ViewModel.CurrentDevice!.Serial, Direction = "上传", LocalPath = "offline-gate.bin", RemotePath = "/sdcard/MatrixAcceptance/offline-gate.bin" };
        ViewModel.Transfers.Add(pending);
        try
        {
            ViewModel.SetExternalBusy(true);
            try
            {
                if (!ViewModel.Busy || !ViewModel.Runtime.Api.UiWriteBusy || ViewModel.StartTransfersCommand.CanExecute(null) || ViewModel.CanWriteDevice())
                    throw new InvalidOperationException("雷电忙状态没有阻止普通操作或API写入");
            }
            finally { ViewModel.SetExternalBusy(false); }
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var busy = ViewModel.ExecuteAsync("忙状态退出与传输检查", () => release.Task);
            try
            {
                if (ViewModel.StartTransfersCommand.CanExecute(null) || !ViewModel.HasActiveOperations)
                    throw new InvalidOperationException("普通忙操作没有阻止传输开始或未纳入退出活动守卫");
            }
            finally { release.SetResult(); await busy; }
            pending.State = "已停止";
            if (!ViewModel.StartTransfersCommand.CanExecute(null)) throw new InvalidOperationException("已停止的文件传输不能重试");
        }
        finally { ViewModel.Transfers.Remove(pending); ViewModel.UpdateSelection(); }
        apiProtocol.SelectedIndex = 2;
        if (apiSend.CanExecute(null)) throw new InvalidOperationException("未启动的接口允许发送请求");
        var serial = ViewModel.CurrentDevice!.Serial;
        await ViewModel.Runtime.Api.StartDeviceAsync(serial);
        try
        {
            apiMethod.SelectedIndex = 1; apiRoute.Text = "/tap"; apiPayload.Text = "{\"X\":20,\"Y\":20}"; apiSend.Refresh();
            await apiSend.ExecuteAsync();
            if (!apiResponse.Text.StartsWith("HTTP 200", StringComparison.Ordinal) || !string.IsNullOrEmpty(ViewModel.Error)) throw new InvalidOperationException("API面板忙状态阻止了自己的写请求");
        }
        finally { await ViewModel.Runtime.Api.StopDeviceAsync(serial); apiMethod.SelectedIndex = 0; apiRoute.Text = "/health"; apiPayload.Text = "{}"; apiResponse.Clear(); apiProtocol.SelectedIndex = 0; }
    }

    private object[] CaptureControlStates(string page, string state, FrameworkElement surface)
    {
        return VisualDescendants<FrameworkElement>(surface)
            .Where(control => control is ButtonBase or TextBox or Selector or RangeBase)
            .Select((control, index) =>
            {
                var property = control switch
                {
                    TextBox => TextBox.TextProperty,
                    ToggleButton => ToggleButton.IsCheckedProperty,
                    Selector => Selector.SelectedItemProperty,
                    RangeBase => RangeBase.ValueProperty,
                    _ => null
                };
                var binding = property is null ? null : BindingOperations.GetBinding(control, property)?.Path?.Path;
                var command = (control as ICommandSource)?.Command;
                return (object)new
                {
                    caseId = $"UI-04-{page}-{index}", page, state, type = control.GetType().Name,
                    label = control.ToolTip as string ?? (control as ContentControl)?.Content as string ?? binding ?? control.Name,
                    binding, control.IsEnabled, control.IsVisible,
                    commandAvailable = command?.CanExecute((control as ICommandSource)?.CommandParameter),
                    result = "OBSERVED-NOT-EXERCISED", mode = "demo"
                };
            }).ToArray();
    }
}

```

## src/MatrixDevice.Desktop/Views/UnifiedDevicesAcceptance.cs

```csharp
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using MatrixDevice.Core;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    internal async Task RunUnifiedAccessAcceptanceAsync(string output, string approvedCandidate, bool verifyBackgroundWindow = false)
    {
        if (ViewModel.Demo || !Path.IsPathFullyQualified(output) || !ViewModel.DataDirectory.Contains(Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("接入验收必须使用artifacts下的独立配置及绝对证据路径。");
        var runtime = ViewModel.Runtime;
        var service = runtime.LdPlayer ?? throw new InvalidOperationException("未配置雷电。");
        if (runtime.Settings.AdbAllowedSerials.Length != 1 || runtime.Settings.LdSerialBindings.Count != 1 || !runtime.Settings.LdSerialBindings.ContainsKey(2))
            throw new InvalidOperationException("验收初始配置必须仅授权独立测试实例2。");
        var target = service.Instances.Single(item => item.Index == 1);
        if (!target.Name.StartsWith("MatrixAcceptance-", StringComparison.Ordinal) || !target.AndroidStarted)
            throw new InvalidOperationException("只允许已运行的独立验收实例1；本验收不会启动模拟器。");
        var previouslyOnline = ViewModel.Devices.Where(row => row.Info.Online).Select(row => row.Serial).ToArray();
        if (previouslyOnline.Length != 1) throw new InvalidOperationException("原授权实例2未就绪。");
        Directory.CreateDirectory(output);
        var completed = false;
        await RunLdWorkspaceOperationAsync("独立实例接入验收", async ct =>
        {
            using var process = Process.GetProcessById(target.VBoxProcessId);
            var proofs = await LdConnectionAdmission.ResolveAsync((AdbDeviceBackend)runtime.Backend, service, [1], [target], new Dictionary<int, long> { [1] = process.StartTime.ToUniversalTime().Ticks }, ct);
            if (proofs.Single().Serial != approvedCandidate) throw new InvalidOperationException("验收目标串号与复制配置中预先批准的串号不符。");
            if (proofs.Single().Serial == previouslyOnline[0]) throw new InvalidOperationException("接入目标与原设备串号重复。");
            var before = ViewModel.Runtime;
            using (var cancelled = new CancellationTokenSource())
            {
                cancelled.Cancel();
                try { await ViewModel.ApplyLdConnectionsAsync(proofs, cancelled.Token); throw new InvalidOperationException("取消未生效。"); }
                catch (OperationCanceledException) when (cancelled.IsCancellationRequested) { }
            }
            if (!ReferenceEquals(before, ViewModel.Runtime) || ViewModel.Runtime.Settings.AdbAllowedSerials.Length != 1)
                throw new InvalidOperationException("取消接入改变了旧授权或运行时。");
            completed = true;
        });
        if (!completed) throw new InvalidOperationException("接入取消验收未完成：" + ldFeedback.Text);
        var previousRuntime = ViewModel.Runtime;
        var previousConfirm = ViewModel.Confirm; var confirmations = 0;
        ViewModel.Confirm = (_, _) => { confirmations++; throw new InvalidOperationException("一键启动弹出了确认框。"); };
        try { await ConnectLdInstancesAsync([1], new Dictionary<int, string> { [1] = approvedCandidate }); }
        finally { ViewModel.Confirm = previousConfirm; }
        {
            if (confirmations != 0 || ReferenceEquals(previousRuntime, ViewModel.Runtime) || ViewModel.Runtime.Settings.LdSerialBindings.GetValueOrDefault(1) != approvedCandidate) throw new InvalidOperationException("一键接入没有通过原批准目标的核验。");
            if (ViewModel.Runtime.Settings.LdSerialBindings.Count != 2 || ViewModel.Devices.Count(row => row.Info.Online) != 2)
                throw new InvalidOperationException("运行时未完成接入替换。");
            foreach (var index in new[] { 1, 2 }) if (!LdInstanceConnected(index)) throw new InvalidOperationException("实例未进入已连接状态：" + index);
            await ViewModel.RefreshAutomaticScreensAsync(ViewModel.Devices.Select(row => row.Serial).ToArray(), CancellationToken.None);
            if (ViewModel.Devices.Any(row => row.Screenshot is null)) throw new InvalidOperationException("接入后截图为空。");
            var hiddenAndRestored = false;
            if (verifyBackgroundWindow)
            {
                var player = ViewModel.Runtime.LdPlayer!.Instances.Single(item => item.Index == 1);
                using var process = Process.GetProcessById(player.ProcessId);
                ldWindowVisibility ??= new LdWindowVisibility(Path.Combine(ViewModel.DataDirectory, "window-recovery"), Path.GetDirectoryName(ViewModel.Runtime.FrozenConnectionSettings.LdConsolePath)!);
                try
                {
                    if (!await ldWindowVisibility.HideAsync(player, process.StartTime.ToUniversalTime().Ticks)) throw new InvalidOperationException("目标窗口原本不可见，本轮不能证明隐藏行为。");
                    if (!ldWindowVisibility.IsHidden(1)) throw new InvalidOperationException("目标窗口未隐藏。");
                    var png = await ViewModel.Runtime.Backend.ScreenshotAsync(approvedCandidate);
                    var size = PngImage.ReadDimensions(png);
                    var colors = Enumerable.Range(0, 16).Select(i => PngImage.ColorAt(png, i * (size.Width - 1) / 15, i * (size.Height - 1) / 15)).Distinct().Count();
                    if (colors < 2) throw new InvalidOperationException("隐藏后的截图未检出有效画面差异。");
                    await File.WriteAllBytesAsync(Path.Combine(output, "hidden-device.png"), png);
                }
                finally { await ldWindowVisibility.ShowAsync(1); }
                hiddenAndRestored = !ldWindowVisibility.IsHidden(1);
            }
            UpdateLdPlayerRows(); UpdateUnifiedDirectory(); NavigateTo("设备墙 / 主控"); SetWallViewMode("list");
            await Dispatcher.InvokeAsync(UpdateLayout, System.Windows.Threading.DispatcherPriority.Render);
            SaveRender(Path.Combine(output, "connected.png"));
            await File.WriteAllTextAsync(Path.Combine(output, "result.json"), JsonSerializer.Serialize(new
            {
                passed = true, existingIndex = 2, admittedIndex = 1, cancellationPreservedRuntime = true,
                originalAuthorizationPreserved = true, localInstances = service.Instances.Count,
                connectedDevices = ViewModel.Devices.Count(row => row.Info.Online),
                runtimeReplaced = true, screenshotsAvailable = true,
                emulatorLaunchNotRequested = true, scopeLimitedToApprovedIndices = true,
                startupConfirmationDialogs = confirmations, backgroundWindowVerified = hiddenAndRestored
            }, JsonDefaults.Options));
        }
    }
}

```

## src/MatrixDevice.Desktop/Views/UnifiedDevicesView.cs

```csharp
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using MatrixDevice.Core;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private readonly CancellationTokenSource directoryCancellation = new();
    private Task? directoryOperation;
    private CancellationTokenSource? ldOperationCancellation;
    private readonly List<TextBlock> deviceSummaries = [];
    private readonly List<(string Tag, ToggleButton Button)> deviceModes = [];
    private readonly ObservableCollection<UnifiedDeviceRow> unifiedDevices = [];
    private ICollectionView unifiedDeviceView = null!;
    private readonly Dictionary<int, string> launchPhases = [];
    private readonly List<TextBlock> launchFeedback = [];
    private readonly List<AsyncCommand> launchCommands = [];
    private readonly List<ActionCommand> launchCancelCommands = [];
    private bool updatingUnifiedDirectory;
    private bool backgroundLaunch = true;
    private bool operationBackgroundLaunch;
    private readonly HashSet<int> coldLaunched = [];
    private readonly Dictionary<int, Task> backgroundHides = [];
    private LdWindowVisibility? ldWindowVisibility;
    private Exception? backgroundHideError;
    private DateTimeOffset lastGuestDiscovery;

    private FrameworkElement BuildDeviceModes()
    {
        var row = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
        foreach (var (tag, glyph, label) in new[] { ("设备墙 / 主控", "\uE8A9", "全部设备"), ("雷电实例", "\uE7F4", "高级管理") })
        {
            var choice = WallToggle(glyph, label);
            choice.Click += (_, _) => NavigateTo(tag);
            deviceModes.Add((tag, choice)); row.Children.Add(choice);
        }
        var summary = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Foreground = WallMuted, Margin = new Thickness(8, 0, 0, 6), TextWrapping = TextWrapping.Wrap };
        deviceSummaries.Add(summary); row.Children.Add(summary);
        return row;
    }

    private void InitializeUnifiedDevices()
    {
        unifiedDeviceView = CollectionViewSource.GetDefaultView(unifiedDevices);
        unifiedDeviceView.Filter = item => item is UnifiedDeviceRow row
            && (string.IsNullOrWhiteSpace(ViewModel.Search) || row.Name.Contains(ViewModel.Search, StringComparison.OrdinalIgnoreCase) || row.Serial.Contains(ViewModel.Search, StringComparison.OrdinalIgnoreCase) || row.Number.Contains(ViewModel.Search, StringComparison.OrdinalIgnoreCase))
            && (ViewModel.GroupFilter == "全部分组" || row.Group == ViewModel.GroupFilter)
            && (ViewModel.StatusFilter == "全部状态" || row.Device?.Info.State == ViewModel.StatusFilter);
        ViewModel.PropertyChanged += (_, args) => { if (args.PropertyName is nameof(WorkspaceViewModel.Counts) or nameof(WorkspaceViewModel.Search) or nameof(WorkspaceViewModel.GroupFilter) or nameof(WorkspaceViewModel.StatusFilter)) UpdateUnifiedDirectory(); RefreshLaunchCommands(); };
    }

    internal void UpdateUnifiedDirectory()
    {
        if (updatingUnifiedDirectory || unifiedDeviceView is null) return;
        updatingUnifiedDirectory = true;
        try
        {
        var instances = ViewModel.Runtime.LdPlayer?.Instances ?? [];
        var admitted = instances.Count(instance => ViewModel.Runtime.Settings.LdSerialBindings.ContainsKey(instance.Index));
        foreach (var mode in deviceModes) mode.Button.IsChecked = Equals((tabs.SelectedItem as TabItem)?.Tag, mode.Tag);
        var expected = new HashSet<string>(StringComparer.Ordinal);
        var boundSerials = new HashSet<string>(StringComparer.Ordinal);
        foreach (var instance in instances)
        {
            ViewModel.Runtime.Settings.LdSerialBindings.TryGetValue(instance.Index, out var serial);
            var device = ViewModel.Devices.FirstOrDefault(row => row.Serial == serial);
            if (device is not null) boundSerials.Add(device.Serial);
            Upsert("ld:" + instance.Index, instance, device, LdInstanceConnected(instance.Index));
        }
        foreach (var device in ViewModel.Devices.Where(row => !boundSerials.Contains(row.Serial))) Upsert("device:" + device.Serial, null, device, device.Info.Online);
        foreach (var stale in unifiedDevices.Where(row => !expected.Contains(row.Key)).ToArray()) unifiedDevices.Remove(stale);
        foreach (var summary in deviceSummaries) summary.Text = $"本机 {instances.Count} 台 · 已接入 {admitted} 台 · 已连接 {unifiedDevices.Count(row => row.Connected)} 台";
        unifiedDeviceView.Refresh(); SynchronizeUnifiedSelection();
        void Upsert(string key, LdPlayerInstance? instance, DeviceRow? device, bool connected)
        {
            expected.Add(key);
            var row = unifiedDevices.FirstOrDefault(item => item.Key == key);
            if (row is null)
            {
                row = new UnifiedDeviceRow(key, ActivateUnifiedRowAsync, CanActivateUnifiedRow, ShowUnifiedWindowAsync, item => !ldBusy && !ViewModel.Busy && item.Index is { } index && ldWindowVisibility?.IsHidden(index) == true, item => item.Device is null ? Task.CompletedTask : ViewModel.SetMainAsync(item.Device));
                row.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(UnifiedDeviceRow.IsSelected)) SynchronizeUnifiedSelection(); };
                unifiedDevices.Add(row);
            }
            row.Update(instance, device, connected, instance is not null && launchPhases.TryGetValue(instance.Index, out var phase) ? phase : "");
        }
        }
        finally { updatingUnifiedDirectory = false; }
    }

    private bool CanStartInstances() => !ldBusy && !captureInProgress && !ViewModel.HasActiveOperations && ViewModel.ConnectionReloadAvailable && scanOperation is not { IsCompleted: false };
    private bool CanActivateUnifiedRow(UnifiedDeviceRow row) => row.Connected ? !ViewModel.Busy : row.Index is > 0 and < 99999 && !ViewModel.Demo && CanStartInstances();
    private async Task ActivateUnifiedRowAsync(UnifiedDeviceRow row)
    {
        try
        {
            if (row.Connected && row.Device is not null) { ViewModel.CurrentDevice = row.Device; NavigateTo("节点"); }
            else if (row.Index is { } index) await ConnectLdInstancesAsync([index]);
        }
        catch (Exception ex) { SetLaunchFeedback("操作失败：" + ViewModel.PresentError(ex)); }
    }
    private async Task ShowUnifiedWindowAsync(UnifiedDeviceRow row)
    {
        try { if (row.Index is { } index && ldWindowVisibility is not null) await ldWindowVisibility.ShowAsync(index); UpdateUnifiedDirectory(); }
        catch (Exception ex) { SetLaunchFeedback("显示窗口失败：" + ViewModel.PresentError(ex)); }
    }
    private void SetLaunchFeedback(string text) { if (ldFeedback is not null) ldFeedback.Text = text; foreach (var feedback in launchFeedback) feedback.Text = text; }
    private void SynchronizeUnifiedSelection()
    {
        ViewModel.SetPendingLaunchSelection(unifiedDevices.Count(row => row.IsSelected && !row.Connected));
        workspaceCounts.Text = $"目录 {unifiedDevices.Count} · 已连接 {unifiedDevices.Count(row => row.Connected)} · 筛选 {unifiedDeviceView.Cast<UnifiedDeviceRow>().Count()} · 已选 {unifiedDevices.Count(row => row.IsSelected)}";
        RefreshLaunchCommands();
    }
    private void RefreshLaunchCommands() { foreach (var command in launchCommands) command.Refresh(); foreach (var command in launchCancelCommands) command.Refresh(); foreach (var row in unifiedDevices) row.RefreshCommands(); }

    private FrameworkElement BuildOneClickLaunchTools()
    {
        var section = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };
        var tools = Toolbar();
        tools.Children.Add(LaunchButton("\uE768", "启动所选", () => ConnectLdInstancesAsync(unifiedDevices.Where(row => row.IsSelected && !row.Connected && row.Index is > 0 and < 99999).Select(row => row.Index!.Value).ToArray()), () => !ViewModel.Demo && CanStartInstances() && unifiedDevices.Any(row => row.IsSelected && !row.Connected && row.Index is > 0 and < 99999), true));
        tools.Children.Add(LaunchButton("\uE768", "一键启动全部", StartAllLdInstancesAsync, () => !ViewModel.Demo && CanStartInstances() && unifiedDevices.Any(row => !row.Connected && row.Index is > 0 and < 99999)));
        var background = WallToggle("\uE7F4", "后台启动"); background.IsChecked = backgroundLaunch; background.ToolTip = "隐藏本次启动的模拟器窗口；并非无头模式，启动初期可能短暂显示";
        background.Checked += (_, _) => backgroundLaunch = true; background.Unchecked += (_, _) => backgroundLaunch = false; tools.Children.Add(background);
        tools.Children.Add(LaunchButton("\uE7F4", "显示窗口", async () => { foreach (var row in unifiedDevices.Where(row => row.IsSelected).ToArray()) await ShowUnifiedWindowAsync(row); }, () => !ldBusy && unifiedDevices.Any(row => row.IsSelected && row.Index is { } index && ldWindowVisibility?.IsHidden(index) == true)));
        var stop = new ActionCommand(() => ldOperationCancellation?.Cancel(), () => ldOperationCancellation is not null);
        launchCancelCommands.Add(stop);
        var stopButton = WallButton("\uE71A", "取消启动", stop); stopButton.Content = WallGlyph("\uE71A"); stopButton.Width = 34; stopButton.MinWidth = 0; stopButton.Padding = new Thickness(0); tools.Children.Add(stopButton);
        ViewModel.PropertyChanged += (_, _) => stop.Refresh();
        section.Children.Add(tools);
        var feedback = new TextBlock { Foreground = WallMuted, TextWrapping = TextWrapping.Wrap, MinHeight = 19, Margin = new Thickness(0, 0, 0, 4) }; launchFeedback.Add(feedback); section.Children.Add(feedback);
        return section;
    }
    private Button LaunchButton(string glyph, string label, Func<Task> operation, Func<bool> enabled, bool primary = false)
    {
        var command = new AsyncCommand(async () => { try { await operation(); } catch (Exception ex) { SetLaunchFeedback(label + "失败：" + ViewModel.PresentError(ex)); } }, enabled);
        launchCommands.Add(command); var button = WallButton(glyph, label, command, primary);
        button.ToolTip = label.StartsWith("启动", StringComparison.Ordinal) || label == "一键启动全部" ? label + "，完成核验后自动连接并加入群控" : label;
        return button;
    }

    private bool LdInstanceConnected(int index) => ViewModel.Runtime.Settings.LdSerialBindings.TryGetValue(index, out var serial)
        && ViewModel.Runtime.Api.LdSerialForWriteGuard?.Invoke(index) == serial
        && ViewModel.Devices.Any(row => row.Serial == serial && row.Info.Online);

    private string InstanceConnectionState(LdPlayerInstance instance) => instance.Index == 0 && !ViewModel.Demo ? "受保护" : LdInstanceConnected(instance.Index) ? "已连接"
        : instance.IsStarting || (!instance.AndroidStarted && (instance.ProcessId > 0 || instance.VBoxProcessId > 0)) ? "启动中"
        : !instance.AndroidStarted ? "已关闭" : ViewModel.Runtime.Settings.LdSerialBindings.ContainsKey(instance.Index) ? "需重新接入" : "待接入";

    private Task UpdateDeviceDirectoryAsync()
    {
        if (ViewModel.Demo || closing || directoryCancellation.IsCancellationRequested || ldBusy || captureInProgress || ViewModel.HasActiveOperations || directoryOperation is { IsCompleted: false }) return Task.CompletedTask;
        directoryOperation = RefreshDirectoryCoreAsync();
        return directoryOperation;
    }

    private async Task RefreshDirectoryCoreAsync()
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(directoryCancellation.Token);
        deadline.CancelAfter(TimeSpan.FromSeconds(20));
        try
        {
            if (ViewModel.Runtime.LdPlayer is not null) await ViewModel.Runtime.RefreshLdPlayerAsync(deadline.Token);
            UpdateLdPlayerRows();
            if (DateTimeOffset.UtcNow - lastGuestDiscovery >= TimeSpan.FromSeconds(15))
            {
                lastGuestDiscovery = DateTimeOffset.UtcNow;
                await ViewModel.RefreshDeviceSnapshotAsync(deadline.Token);
            }
            if (ldFeedback.Text.StartsWith("自动更新失败：", StringComparison.Ordinal)) ldFeedback.Text = "";
            UpdateUnifiedDirectory();
        }
        catch (OperationCanceledException) when (directoryCancellation.IsCancellationRequested) { }
        catch (Exception ex)
        {
            ViewModel.Runtime.Log.Write("directory-refresh-error", ex.ToString());
            SetLaunchFeedback("自动更新失败：" + ViewModel.PresentError(ex));
        }
    }

    private async Task RunLdWorkspaceOperationAsync(string title, Func<CancellationToken, Task> operation)
    {
        if (ldBusy || captureInProgress || ViewModel.HasActiveOperations || scanOperation is { IsCompleted: false }) throw new DeviceException("请等待当前操作或网络扫描结束。", 409, 1003);
        if (!ViewModel.ConnectionReloadAvailable) throw new DeviceException("请先停止接口服务、脚本和文件传输。", 409, 1003);
        if (ViewModel.Demo) throw new DeviceException("演示模式不会操作真实模拟器。", 409, 1003);
        ViewModel.Runtime.RequireFrozenConnectionSettings();
        ldBusy = true; ViewModel.SetExternalBusy(true); RefreshLdButtons();
        using var cancellation = new CancellationTokenSource(); ldOperationCancellation = cancellation;
        coldLaunched.Clear(); backgroundHides.Clear(); backgroundHideError = null; operationBackgroundLaunch = backgroundLaunch; RefreshLaunchCommands();
        var complete = false;
        try
        {
            if (directoryOperation is not null) await directoryOperation;
            SetLaunchFeedback(title);
            await operation(cancellation.Token);
            complete = true;
            ViewModel.Runtime.Log.Write("ld-workspace-complete", title);
        }
        catch (OperationCanceledException) { SetLaunchFeedback(backgroundHideError is null ? "已停止等待及后续任务，已启动的实例保持运行。" : "后台隐藏失败：" + ViewModel.PresentError(backgroundHideError)); ViewModel.Runtime.Log.Write("ld-workspace-cancel", title); }
        catch (Exception ex) { SetLaunchFeedback(title + "失败：" + ViewModel.PresentError(ex)); ViewModel.Runtime.Log.Write("ld-workspace-error", ex.ToString()); }
        finally
        {
            if (!complete)
            {
                try { await Task.WhenAll(backgroundHides.Values); } catch (Exception ex) { ViewModel.Runtime.Log.Write("ld-background-error", ex.ToString()); }
                foreach (var index in launchPhases.Keys.ToArray())
                    if (launchPhases[index] is not ("已连接" or "启动失败")) launchPhases[index] = cancellation.IsCancellationRequested ? "已取消" : "未完成";
                foreach (var index in coldLaunched)
                {
                    try { if (ldWindowVisibility is not null) await ldWindowVisibility.ShowAsync(index); }
                    catch (Exception ex) { SetLaunchFeedback("窗口恢复失败：" + ViewModel.PresentError(ex)); }
                }
            }
            ldOperationCancellation = null; ldBusy = false; ViewModel.SetExternalBusy(false);
            UpdateLdPlayerRows(); UpdateUnifiedDirectory(); RefreshLdButtons();
        }
    }

    private sealed class InlineLaunchProgress(MainWindow owner) : IProgress<LdLaunchProgress>
    {
        public void Report(LdLaunchProgress value) => owner.Dispatcher.Invoke(() => owner.OnLaunchProgress(value));
    }
    private IProgress<LdLaunchProgress> LaunchProgress() => new InlineLaunchProgress(this);
    private void OnLaunchProgress(LdLaunchProgress progress)
    {
        var phase = progress.State switch { "checking-resources" => "检查资源", "starting" => "正在启动", "waiting" => "等待安卓就绪", "ready" or "skipped" => "等待连接", "cancelled" => "已取消", _ => "启动失败" };
        launchPhases[progress.Index] = phase;
        if (progress.State == "starting") coldLaunched.Add(progress.Index);
        if (operationBackgroundLaunch && coldLaunched.Contains(progress.Index) && !backgroundHides.ContainsKey(progress.Index) && progress.Instance is { ProcessId: > 0, TopWindow: not 0 } instance)
            backgroundHides[progress.Index] = HideStartedWindowAsync(instance);
        SetLaunchFeedback($"实例 {progress.Index}：{phase}");
        ViewModel.Runtime.Log.Write("ld-launch-" + progress.State, $"index={progress.Index}; {progress.Message}");
        UpdateLdPlayerRows(); UpdateUnifiedDirectory();
    }

    private async Task HideStartedWindowAsync(LdPlayerInstance instance)
    {
        try
        {
            ldWindowVisibility ??= new LdWindowVisibility(System.IO.Path.Combine(ViewModel.DataDirectory, "window-recovery"), System.IO.Path.GetDirectoryName(ViewModel.Runtime.FrozenConnectionSettings.LdConsolePath)!);
            using var process = System.Diagnostics.Process.GetProcessById(instance.ProcessId);
            await ldWindowVisibility.HideAsync(instance, process.StartTime.ToUniversalTime().Ticks);
            ViewModel.Runtime.Log.Write("ld-background-hidden", $"index={instance.Index}; pid={instance.ProcessId}");
        }
        catch (Exception ex) { backgroundHideError = ex; ldOperationCancellation?.Cancel(); throw; }
    }

    internal async Task RecoverBackgroundWindowsAsync()
    {
        if (ViewModel.Demo || ViewModel.Runtime.LdPlayer is null || string.IsNullOrWhiteSpace(ViewModel.Runtime.FrozenConnectionSettings.LdConsolePath)) return;
        ldWindowVisibility ??= new LdWindowVisibility(System.IO.Path.Combine(ViewModel.DataDirectory, "window-recovery"), System.IO.Path.GetDirectoryName(ViewModel.Runtime.FrozenConnectionSettings.LdConsolePath)!);
        var errors = await ldWindowVisibility.RecoverPendingAsync();
        if (errors.Count > 0) { SetLaunchFeedback("部分后台窗口恢复失败，请检查日志。"); foreach (var error in errors) ViewModel.Runtime.Log.Write("ld-window-recovery-error", error); }
    }

    private async Task StartAllLdInstancesAsync()
    {
        var confirmedInstances = ViewModel.Runtime.LdPlayer?.Instances.Where(item => item.Index > 0 && item.Index != 99999 && !LdInstanceConnected(item.Index)).ToArray() ?? [];
        var indices = confirmedInstances.Select(item => item.Index).ToArray();
        if (indices.Length == 0) throw new DeviceException("没有可启动的独立实例。", 404, 1002);
        await ConnectLdInstancesAsync(indices);
    }

    private Task ConnectRunningLdInstancesAsync()
    {
        var indices = ViewModel.Runtime.LdPlayer?.Instances.Where(item => item.Index > 0 && item.Index != 99999 && item.AndroidStarted && !LdInstanceConnected(item.Index)).Select(item => item.Index).ToArray() ?? [];
        return ConnectLdInstancesAsync(indices);
    }

    private async Task ConnectLdInstancesAsync(int[] indices, IReadOnlyDictionary<int, string>? expectedSerials = null)
    {
        if (indices.Length == 0) throw new DeviceException("没有待接入的已运行实例。", 404, 1002);
        if (indices.Any(index => index <= 0 || index == 99999)) throw new DeviceException("实例0受保护，不能自动接入。", 403, 1003);
        var approvedSerials = expectedSerials is null ? null : new Dictionary<int, string>(expectedSerials);
        if (approvedSerials is not null && (approvedSerials.Count != indices.Length || indices.Any(index => !approvedSerials.TryGetValue(index, out var serial) || string.IsNullOrWhiteSpace(serial))))
            throw new DeviceException("验收预批准串号必须与目标实例逐一对应。", 400, 1007);
        var service = ViewModel.Runtime.LdPlayer ?? throw new DeviceException("雷电服务未配置。", 503, 2001);
        var confirmedInstances = indices.Select(index => service.Instances.Single(item => item.Index == index)).ToArray();
        var expectedBirths = confirmedInstances.Where(item => item.VBoxProcessId > 0).ToDictionary(item => item.Index, VBoxBirth);
        await RunLdWorkspaceOperationAsync("接入雷电设备", async ct =>
        {
            foreach (var index in indices) launchPhases[index] = "排队中";
            var started = await new LdLaunchCoordinator(service).StartAsync(indices, EnsureLaunchResourcesAsync, LaunchProgress(), ct, expectedInstances: confirmedInstances);
            await Task.WhenAll(backgroundHides.Values);
            if (backgroundHideError is not null) throw backgroundHideError;
            var expected = started.Select(result => result.Instance ?? throw new DeviceException("启动结果缺少实例快照。", 409, 1003)).ToArray();
            foreach (var instance in expected)
                if (!expectedBirths.ContainsKey(instance.Index)) expectedBirths.Add(instance.Index, VBoxBirth(instance));
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct); deadline.CancelAfter(TimeSpan.FromSeconds(90));
            IReadOnlyList<LdConnectionProof> proofs;
            while (true)
            {
                try { proofs = await LdConnectionAdmission.ResolveAsync((AdbDeviceBackend)ViewModel.Runtime.Backend, service, indices, expected, expectedBirths, deadline.Token); break; }
                catch (DeviceException ex) when (ex.Status == 409 && ex.Code == 2002)
                { SetLaunchFeedback("等待雷电注册ADB连接…"); await Task.Delay(1500, deadline.Token); }
            }
            if (approvedSerials is not null && proofs.Any(proof => !approvedSerials.TryGetValue(proof.Index, out var serial) || serial != proof.Serial))
                throw new DeviceException("实际连接与验收预批准串号不符，未读取设备或保存授权。", 403, 1003);
            SetLaunchFeedback("正在核验安卓启动与截图…");
            await ViewModel.ApplyLdConnectionsAsync(proofs, deadline.Token);
            foreach (var index in indices) launchPhases[index] = "已连接";
            UpdateLdPlayerRows(); UpdateUnifiedDirectory();
            SetLaunchFeedback($"已启动并连接 {proofs.Count} 台设备。");
        });
    }

    private static long VBoxBirth(LdPlayerInstance instance)
    {
        using var process = System.Diagnostics.Process.GetProcessById(instance.VBoxProcessId);
        var birth = process.StartTime.ToUniversalTime().Ticks;
        if (process.HasExited) throw new DeviceException($"实例{instance.Index}的虚拟机进程已退出。", 409, 1003);
        return birth;
    }

    private static Task EnsureLaunchResourcesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var memory = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        if (!GlobalMemoryStatusEx(ref memory)) throw new DeviceException("无法读取系统内存，未启动新实例。", 503, 2000);
        if (memory.AvailablePhysical < 11UL * 1024 * 1024 * 1024 || memory.MemoryLoad >= 80)
            throw new DeviceException($"资源不足：可用内存 {memory.AvailablePhysical / 1073741824d:0.0} GiB，负载 {memory.MemoryLoad}%；至少需要11 GiB可用内存且负载低于80%。已运行实例保持不变。", 409, 1003);
        return Task.CompletedTask;
    }

    private async Task EditLdInstanceAsync(bool create)
    {
        var selected = ldSelection;
        if (!create && selected is null) throw new DeviceException("请选择已停止的实例。", 400, 1007);
        var name = new TextBox { Text = create ? "矩阵测试-" + DateTime.Now.ToString("MMdd-HHmmss") : selected!.Name, IsReadOnly = !create };
        var cpu = new ComboBox { ItemsSource = new[] { 1, 2, 3, 4 }, SelectedItem = create ? 2 : (object?)null };
        var memory = new ComboBox { ItemsSource = new[] { 256, 512, 768, 1024, 1536, 2048, 4096, 8192 }, SelectedItem = create ? 2048 : (object?)null };
        var width = new TextBox { Text = (selected?.Instance.Width ?? 1280).ToString() };
        var height = new TextBox { Text = (selected?.Instance.Height ?? 720).ToString() };
        var dpi = new TextBox { Text = (selected?.Instance.Dpi ?? 240).ToString() };
        var content = new StackPanel { Margin = new Thickness(20) };
        foreach (var field in new[] { Field("名称", name, 90), Field("新CPU核数", cpu, 90), Field("新内存 MiB", memory, 90), Field("宽度", width, 90), Field("高度", height, 90), Field("DPI", dpi, 90) }) content.Children.Add(field);
        var error = new TextBlock { TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Firebrick, Margin = new Thickness(0, 0, 0, 8) }; content.Children.Add(error);
        var dialog = new Window { Owner = this, Title = create ? "新建空白模拟器" : "冷态资源配置", Width = 420, SizeToContent = SizeToContent.Height, ResizeMode = ResizeMode.NoResize, WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = content };
        LdInstanceConfiguration? configuration = null;
        var submit = new Button { Content = create ? "创建并配置" : "保存配置", IsDefault = true, Margin = new Thickness(0, 6, 0, 0) };
        submit.Click += (_, _) =>
        {
            try
            {
                if (!int.TryParse(width.Text, out var w) || !int.TryParse(height.Text, out var h) || !int.TryParse(dpi.Text, out var d)) throw new DeviceException("宽度、高度和DPI必须是整数。");
                if (cpu.SelectedItem is null || memory.SelectedItem is null) throw new DeviceException("请选择新的CPU核数和内存值。");
                configuration = new((int)cpu.SelectedItem, (int)memory.SelectedItem, w, h, d); LdInstanceEditor.Validate(configuration);
                dialog.DialogResult = true;
            }
            catch (Exception ex) { error.Text = ViewModel.PresentError(ex); }
        };
        content.Children.Add(submit);
        if (dialog.ShowDialog() != true || configuration is null) return;
        if (!ViewModel.Confirm(create ? "新建空白雷电实例" : "修改雷电实例资源", create ? $"创建空白实例“{name.Text.Trim()}”，配置 {configuration.Cpu}核 / {configuration.MemoryMb} MiB / {configuration.Width}×{configuration.Height}。\n不会复制账号或数据，不自动启动。继续？" : $"仅修改实例 {selected!.Index}：{selected.Name} 的CPU、内存和分辨率；不会修改设备身份。配置后需启动验证。继续？")) return;
        await RunLdWorkspaceOperationAsync(create ? "新建空白实例" : "配置实例", async ct =>
        {
            var editor = ViewModel.Runtime.LdEditor ?? throw new DeviceException("雷电编辑能力未配置，请重启程序。", 503, 2001);
            var target = selected?.Instance;
            if (create)
            {
                var result = await editor.CreateAsync(name.Text.Trim(), ct); target = result.Instance;
                ViewModel.Runtime.Log.Write("ld-create", result.Message);
            }
            LdPlayerCommandResult configured;
            try { configured = await editor.ConfigureAsync(target!.Index, target.Generation, configuration, ct); }
            catch (Exception ex) when (create && ex is not OperationCanceledException)
            { throw new DeviceException($"已创建实例 {target!.Index}：{target.Name}，但配置失败：{ex.Message}。未删除或重复创建，请检查目录。", 409, 1003); }
            ViewModel.Runtime.Log.Write("ld-configure", configured.Message);
            ldFeedback.Text = $"实例 {target.Index} 已提交资源配置，启动后核验生效情况。";
        });
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus { public uint Length, MemoryLoad; public ulong TotalPhysical, AvailablePhysical, TotalPageFile, AvailablePageFile, TotalVirtual, AvailableVirtual, AvailableExtendedVirtual; }
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatus status);
}

```

## src/MatrixDevice.Desktop/Views/UnifiedDeviceTile.cs

```csharp
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace MatrixDevice.Desktop;

public sealed class UnifiedDeviceTile : Border
{
    public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register(nameof(Owner), typeof(MainWindow), typeof(UnifiedDeviceTile), new PropertyMetadata(null, OnChanged));
    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(nameof(Mode), typeof(string), typeof(UnifiedDeviceTile), new PropertyMetadata("cards", OnChanged));
    public MainWindow? Owner { get => (MainWindow?)GetValue(OwnerProperty); set => SetValue(OwnerProperty, value); }
    public string Mode { get => (string)GetValue(ModeProperty); set => SetValue(ModeProperty, value); }
    private UnifiedDeviceRow? row;
    public UnifiedDeviceTile()
    {
        DataContextChanged += (_, _) => { if (row is not null) row.PropertyChanged -= RowChanged; row = DataContext as UnifiedDeviceRow; if (IsLoaded && row is not null) row.PropertyChanged += RowChanged; Rebuild(); };
        Loaded += (_, _) => { if (row is not null) { row.PropertyChanged -= RowChanged; row.PropertyChanged += RowChanged; } Rebuild(); };
        Unloaded += (_, _) => { if (row is not null) row.PropertyChanged -= RowChanged; };
    }
    private static void OnChanged(DependencyObject value, DependencyPropertyChangedEventArgs _) => ((UnifiedDeviceTile)value).Rebuild();
    private void RowChanged(object? sender, PropertyChangedEventArgs args) { if (args.PropertyName is "" or nameof(UnifiedDeviceRow.Device)) Rebuild(); }
    private void Rebuild()
    {
        if (row is null || Owner is null) { Child = null; return; }
        BorderThickness = new Thickness(0); Padding = new Thickness(0); Margin = new Thickness(0);
        if (row.Connected && row.Device is not null && Mode != "list")
        {
            Child = new WallDeviceTile { Owner = Owner, Mode = Mode, DataContext = row.Device }; return;
        }
        if (Mode == "list") { Child = Actions(); return; }
        var content = new StackPanel { Margin = new Thickness(Mode == "screens" ? 8 : 18) };
        var check = new CheckBox { IsEnabled = row.CanSelect, Margin = new Thickness(0, 0, 0, 10) };
        check.SetBinding(ToggleButton.IsCheckedProperty, new Binding(nameof(UnifiedDeviceRow.IsSelected)) { Mode = BindingMode.TwoWay });
        check.Content = new TextBlock { Text = row.Name, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis, ToolTip = row.Name }; content.Children.Add(check);
        var state = new TextBlock { Foreground = Brushes.DimGray, Margin = new Thickness(0, 6, 0, 12), TextWrapping = TextWrapping.Wrap };
        state.SetBinding(TextBlock.TextProperty, new Binding(nameof(UnifiedDeviceRow.State))); content.Children.Add(state);
        content.Children.Add(new TextBlock { Text = "实例 " + row.Number, Margin = new Thickness(0, 0, 0, 12) });
        var spacer = new Border { Height = Mode == "screens" ? 170 : 132, Background = new SolidColorBrush(Color.FromRgb(247, 249, 252)), Margin = new Thickness(0, 0, 0, 12), Child = new TextBlock { Text = "\uE7F4", FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 32, Foreground = Brushes.LightSlateGray, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center } };
        content.Children.Add(spacer); content.Children.Add(Actions());
        Child = new Border { Child = content, Background = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromRgb(228, 232, 240)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Margin = new Thickness(0, 0, 12, 12) };
    }
    private FrameworkElement Actions()
    {
        var actions = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
        var label = new TextBlock { VerticalAlignment = VerticalAlignment.Center }; label.SetBinding(TextBlock.TextProperty, new Binding(nameof(UnifiedDeviceRow.PrimaryLabel)));
        var glyph = new TextBlock { FontFamily = new FontFamily("Segoe MDL2 Assets"), Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center }; glyph.SetBinding(TextBlock.TextProperty, new Binding(nameof(UnifiedDeviceRow.PrimaryGlyph)));
        var body = new StackPanel { Orientation = Orientation.Horizontal }; body.Children.Add(glyph); body.Children.Add(label);
        var start = new Button { Content = body, Command = row!.PrimaryCommand, Height = 32, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 0, 6, 0), ToolTip = row.PrimaryLabel };
        start.SetResourceReference(StyleProperty, "SubtleButton"); actions.Children.Add(start);
        if (row.Connected && row.Device is not null)
        {
            var main = new Button { Content = new TextBlock { Text = "\uE7F4", FontFamily = new FontFamily("Segoe MDL2 Assets") }, Command = row.MainCommand, ToolTip = "设为主控", Margin = new Thickness(0, 0, 6, 0) };
            main.SetResourceReference(StyleProperty, "IconButton"); actions.Children.Add(main);
        }
        var show = new Button { Content = new TextBlock { Text = "\uE8A7", FontFamily = new FontFamily("Segoe MDL2 Assets") }, Command = row.ShowWindowCommand, ToolTip = "显示模拟器窗口", Visibility = row.ShowWindowCommand.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed };
        show.SetResourceReference(StyleProperty, "IconButton"); actions.Children.Add(show);
        return actions;
    }
}

```

## src/MatrixDevice.Desktop/Views/WallDeviceTile.cs

```csharp
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MatrixDevice.Desktop;

public sealed class WallDeviceTile : Border
{
    public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register(nameof(Owner), typeof(MainWindow), typeof(WallDeviceTile), new PropertyMetadata(null, RebuildOnChange));
    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(nameof(Mode), typeof(string), typeof(WallDeviceTile), new PropertyMetadata("cards", RebuildOnChange));
    private static readonly Brush Ink = new SolidColorBrush(Color.FromRgb(24, 34, 57));
    private static readonly Brush Muted = new SolidColorBrush(Color.FromRgb(99, 113, 138));
    private static readonly Brush Line = new SolidColorBrush(Color.FromRgb(228, 232, 240));
    private static readonly Brush Accent = new SolidColorBrush(Color.FromRgb(50, 116, 246));
    private static readonly Brush AccentText = new SolidColorBrush(Color.FromRgb(33, 90, 206));
    private DeviceRow? row;
    private WorkspaceViewModel? subscribed;
    private readonly List<ICommand> commands = [];
    private TextBlock? status;
    private TextBlock? mainBadge;
    public MainWindow? Owner { get => (MainWindow?)GetValue(OwnerProperty); set => SetValue(OwnerProperty, value); }
    public string Mode { get => (string)GetValue(ModeProperty); set => SetValue(ModeProperty, value); }

    public WallDeviceTile()
    {
        DataContextChanged += (_, _) => Rebuild();
        Loaded += (_, _) => { Subscribe(); RefreshState(); }; Unloaded += (_, _) => Unsubscribe();
        MouseLeftButtonDown += (_, e) =>
        {
            if (Owner is null || row is null || FindParent<ButtonBase>(e.OriginalSource as DependencyObject) is not null) return;
            Owner.ViewModel.CurrentDevice = row;
        };
    }

    private static void RebuildOnChange(DependencyObject target, DependencyPropertyChangedEventArgs args) => ((WallDeviceTile)target).Rebuild();
    private void Rebuild()
    {
        Unsubscribe(); row = DataContext as DeviceRow;
        if (row is null || Owner is null) { Child = null; return; }
        commands.Clear(); status = null; mainBadge = null;
        Background = Brushes.White; CornerRadius = new CornerRadius(8); BorderThickness = Mode == "list" ? new Thickness(0) : new Thickness(1); BorderBrush = Line;
        Margin = Mode == "cards" ? new Thickness(0, 0, 14, 14) : Mode == "screens" ? new Thickness(0, 0, 8, 12) : new Thickness(0);
        VerticalAlignment = VerticalAlignment.Top; Padding = Mode == "cards" ? new Thickness(19, 16, 19, 16) : Mode == "screens" ? new Thickness(7) : new Thickness(0);
        Child = Mode switch { "screens" => BuildScreen(), "list" => BuildActions(compact: true), _ => BuildCard() };
        if (IsLoaded) Subscribe(); RefreshState();
    }

    private FrameworkElement BuildCard()
    {
        var body = new StackPanel();
        var top = new DockPanel { Margin = new Thickness(0, 0, 0, 11) };
        var selection = Selection(); DockPanel.SetDock(selection, Dock.Left); top.Children.Add(selection);
        status = new TextBlock { FontSize = 11, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) }; DockPanel.SetDock(status, Dock.Right); top.Children.Add(status);
        top.Children.Add(Bound(nameof(DeviceRow.Name), 14, bold: true)); body.Children.Add(top);
        var model = new DockPanel { Margin = new Thickness(0, 0, 0, 9) };
        var phone = new TextBlock { Text = "\uE8EA", FontFamily = new FontFamily("Segoe MDL2 Assets"), Foreground = Accent, FontSize = 23, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(14, 0, 0, 0) }; DockPanel.SetDock(phone, Dock.Right); model.Children.Add(phone);
        var modelText = new StackPanel(); modelText.Children.Add(new TextBlock { Text = "设备型号", Foreground = Muted, FontSize = 11, Margin = new Thickness(0, 0, 0, 3) }); modelText.Children.Add(Bound("Info.Model", 17, bold: true)); model.Children.Add(modelText); body.Children.Add(model);
        var badges = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
        badges.Children.Add(Badge("Info.Brand", false)); badges.Children.Add(Badge("Info.AndroidVersion", true));
        mainBadge = new TextBlock { FontSize = 11, Foreground = AccentText, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(2, 0, 0, 0) }; badges.Children.Add(mainBadge); body.Children.Add(badges);
        body.Children.Add(Detail("序列号", nameof(DeviceRow.Serial))); body.Children.Add(Detail("分辨率", nameof(DeviceRow.Resolution))); body.Children.Add(Detail("分组", nameof(DeviceRow.Group))); body.Children.Add(Detail("脚本", nameof(DeviceRow.ScriptState)));
        body.Children.Add(new Border { Background = Line, Height = 1, Margin = new Thickness(0, 13, 0, 13) });
        body.Children.Add(BuildActions()); return body;
    }

    private FrameworkElement BuildScreen()
    {
        var body = new StackPanel();
        var heading = new DockPanel { Margin = new Thickness(0, 0, 0, 6) }; heading.Children.Add(Selection()); heading.Children.Add(Bound(nameof(DeviceRow.Name), 11, bold: true)); body.Children.Add(heading);
        var screen = new Grid { Height = 230, Background = new SolidColorBrush(Color.FromRgb(244, 247, 251)) };
        var image = new Image { Stretch = Stretch.Uniform, Cursor = Cursors.Hand }; image.SetBinding(Image.SourceProperty, new Binding(nameof(DeviceRow.Screenshot))); screen.Children.Add(image);
        var empty = new TextBlock { Text = "无截图", FontSize = 11, Foreground = Muted, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        empty.SetBinding(VisibilityProperty, new Binding(nameof(DeviceRow.Screenshot)) { Converter = new WallNullVisibilityConverter() }); screen.Children.Add(empty); body.Children.Add(screen);
        var state = new DockPanel { Margin = new Thickness(0, 6, 0, 4) }; status = new TextBlock { FontSize = 10 }; state.Children.Add(status);
        mainBadge = new TextBlock { FontSize = 10, Foreground = AccentText, HorizontalAlignment = HorizontalAlignment.Right }; state.Children.Add(mainBadge); body.Children.Add(state);
        var frame = Bound(nameof(DeviceRow.FrameLabel), 10); frame.Margin = new Thickness(0, 0, 0, 5); frame.Foreground = Muted; body.Children.Add(frame);
        var script = Bound(nameof(DeviceRow.ScriptState), 10); script.Foreground = Muted; script.Margin = new Thickness(0, 0, 0, 5); body.Children.Add(script);
        body.Children.Add(BuildActions(compact: true)); return body;
    }

    private FrameworkElement BuildActions(bool compact = false)
    {
        var grid = new Grid(); grid.ColumnDefinitions.Add(new ColumnDefinition()); grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var debug = new ActionCommand(() => { if (row is null || Owner is null) return; Owner.ViewModel.CurrentDevice = row; Owner.NavigateTo("节点"); }, () => Owner is not null && !Owner.ViewModel.Busy);
        commands.Add(debug);
        var inspect = TileButton("\uE946", compact ? "调试" : "开始调试", debug); inspect.Margin = new Thickness(0, 0, 6, 0); grid.Children.Add(inspect);
        var control = new AsyncCommand(async () => { if (row is not null && Owner is not null) await Owner.ViewModel.SetMainAsync(row); }, () => row?.Info.Online == true && Owner is not null && !Owner.ViewModel.Busy);
        commands.Add(control);
        var main = TileButton("\uE7F4", "设为主控", control); main.ToolTip = "设为主控";
        if (compact && Mode == "screens") { main.Content = new TextBlock { Text = "\uE7F4", FontFamily = new FontFamily("Segoe MDL2 Assets"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }; main.Width = 28; main.Padding = new Thickness(0); }
        Grid.SetColumn(main, 1); grid.Children.Add(main); return grid;
    }

    private Button TileButton(string glyph, string caption, ICommand command)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        content.Children.Add(new TextBlock { Text = glyph, FontFamily = new FontFamily("Segoe MDL2 Assets"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) }); content.Children.Add(new TextBlock { Text = caption, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button { Content = content, Command = command, ToolTip = caption, Height = Mode == "cards" ? 34 : 28, Margin = new Thickness(0), Padding = new Thickness(8, 0, 8, 0), FontSize = Mode == "screens" ? 10 : 12 };
        if (Owner?.TryFindResource("SubtleButton") is Style style) button.Style = style; return button;
    }

    private static CheckBox Selection()
    {
        var check = new CheckBox { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0), ToolTip = "选择设备" };
        check.SetBinding(ToggleButton.IsCheckedProperty, new Binding(nameof(DeviceRow.IsSelected)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }); return check;
    }
    private static TextBlock Bound(string path, double size, bool bold = false)
    {
        var text = new TextBlock { FontSize = size, Foreground = Ink, FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal, TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center };
        text.SetBinding(TextBlock.TextProperty, new Binding(path) { Converter = new WallValueConverter() }); text.SetBinding(ToolTipProperty, new Binding(path)); return text;
    }
    private static FrameworkElement Detail(string title, string path)
    {
        var grid = new Grid { Margin = new Thickness(0, 0, 0, 5) }; grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) }); grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.Children.Add(new TextBlock { Text = title, FontSize = 11, Foreground = Muted, VerticalAlignment = VerticalAlignment.Center }); var value = Bound(path, 11); Grid.SetColumn(value, 1); grid.Children.Add(value); return grid;
    }
    private static FrameworkElement Badge(string path, bool android)
    {
        var text = Bound(path, 11); text.Foreground = android ? AccentText : Muted;
        text.SetBinding(TextBlock.TextProperty, new Binding(path) { Converter = new WallValueConverter(android ? "安卓 " : "") });
        return new Border { Child = text, Background = new SolidColorBrush(android ? Color.FromRgb(239, 245, 255) : Color.FromRgb(244, 246, 249)), CornerRadius = new CornerRadius(5), Padding = new Thickness(7, 3, 7, 3), Margin = new Thickness(0, 0, 7, 0) };
    }

    private void Subscribe()
    {
        if (subscribed is not null || Owner is null || row is null) return;
        subscribed = Owner.ViewModel; subscribed.PropertyChanged += ViewModelChanged; row.PropertyChanged += RowChanged;
    }
    private void Unsubscribe()
    {
        if (subscribed is not null) subscribed.PropertyChanged -= ViewModelChanged;
        if (row is not null) row.PropertyChanged -= RowChanged; subscribed = null;
    }
    private void ViewModelChanged(object? sender, PropertyChangedEventArgs args) => RefreshState();
    private void RowChanged(object? sender, PropertyChangedEventArgs args) => RefreshState();
    private void RefreshState()
    {
        if (row is null || Owner is null) return;
        BorderBrush = ReferenceEquals(Owner.ViewModel.CurrentDevice, row) ? Accent : Line;
        if (status is not null) { status.Text = row.State; status.Foreground = row.Info.Online ? new SolidColorBrush(Color.FromRgb(8, 115, 76)) : Muted; }
        if (mainBadge is not null) mainBadge.Text = ReferenceEquals(Owner.ViewModel.MainDevice, row) ? "主控" : "";
        foreach (var command in commands) { if (command is ActionCommand action) action.Refresh(); if (command is AsyncCommand asyncCommand) asyncCommand.Refresh(); }
    }
    private static T? FindParent<T>(DependencyObject? value) where T : DependencyObject
    {
        while (value is not null) { if (value is T parent) return parent; value = value is Visual ? VisualTreeHelper.GetParent(value) : LogicalTreeHelper.GetParent(value); }
        return null;
    }
}

public sealed class WallValueConverter(string prefix = "") : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => prefix + (value is null || string.IsNullOrWhiteSpace(value.ToString()) ? "未核验" : value);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

```

## src/MatrixDevice.Desktop/Views/WallView.cs

```csharp
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MatrixDevice.Core;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private string wallViewMode = "list";
    private readonly Dictionary<string, ToggleButton> wallViewChoices = [];
    private Grid wallWorkspace = null!;
    private FrameworkElement wallCards = null!;
    private FrameworkElement wallScreens = null!;
    private FrameworkElement wallList = null!;
    private FrameworkElement wallMainPanel = null!;
    private FrameworkElement wallDensity = null!;
    private ToggleButton wallMainToggle = null!;

    internal string WallViewMode => wallViewMode;

    private FrameworkElement BuildWallPage()
    {
        var page = new DockPanel { LastChildFill = true, Background = Brushes.White };
        var top = new StackPanel { Margin = new Thickness(22, 18, 22, 0) };
        DockPanel.SetDock(top, Dock.Top); page.Children.Add(top);
        top.Children.Add(BuildDeviceModes());
        var heading = new DockPanel { Margin = new Thickness(0, 0, 0, 18) };
        var modes = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var modeFrame = new Border { Child = modes, Background = new SolidColorBrush(Color.FromRgb(245, 247, 251)), CornerRadius = new CornerRadius(6), Padding = new Thickness(3) }; DockPanel.SetDock(modeFrame, Dock.Right); heading.Children.Add(modeFrame);
        foreach (var (mode, glyph, title) in new[] { ("cards", "\uE8A9", "设备卡片"), ("screens", "\uE80A", "屏幕视图"), ("list", "\uE8EF", "列表视图") })
        {
            var choice = WallToggle(glyph, title); choice.Margin = new Thickness(0);
            choice.Click += (_, _) => SetWallViewMode(mode);
            wallViewChoices.Add(mode, choice); modes.Children.Add(choice);
        }
        heading.Children.Add(new TextBlock { Text = "设备群控", FontSize = 20, FontWeight = FontWeights.SemiBold, Foreground = WallInk, VerticalAlignment = VerticalAlignment.Center });
        top.Children.Add(heading);
        top.Children.Add(BuildOneClickLaunchTools());
        var tools = new WrapPanel { Margin = new Thickness(0, 0, 0, 12) };
        var select = WallButton("\uE73A", "全选", new ActionCommand(() => { foreach (var row in unifiedDeviceView.Cast<UnifiedDeviceRow>().ToArray()) row.IsSelected = true; RefreshLaunchCommands(); })); select.ToolTip = "选择当前筛选中的全部设备"; tools.Children.Add(select);
        tools.Children.Add(WallButton("\uE711", "清空", new ActionCommand(ClearWallSelection)));
        var mouse = WallToggle("\uE962", "同步鼠标"); mouse.SetBinding(ToggleButton.IsCheckedProperty, TwoWay(nameof(WorkspaceViewModel.SyncMouse))); tools.Children.Add(mouse);
        var keyboard = WallToggle("\uE765", "同步键盘"); keyboard.SetBinding(ToggleButton.IsCheckedProperty, TwoWay(nameof(WorkspaceViewModel.SyncKeyboard))); tools.Children.Add(keyboard);
        var automatic = WallToggle("\uE787", "自动截图"); automatic.IsChecked = true; automatic.Checked += (_, _) => { autoCapture = true; }; automatic.Unchecked += (_, _) => { autoCapture = false; }; tools.Children.Add(automatic);
        wallMainToggle = WallToggle("\uE7F4", "主控面板"); wallMainToggle.Checked += (_, _) => UpdateWallMainPanel(); wallMainToggle.Unchecked += (_, _) => UpdateWallMainPanel(); tools.Children.Add(wallMainToggle);
        tools.Children.Add(BuildWallDensity()); top.Children.Add(tools);
        top.Children.Add(new Border { Height = 1, Background = WallBorder });
        wallWorkspace = new Grid { Margin = new Thickness(22, 18, 22, 18) };
        wallWorkspace.RowDefinitions.Add(new RowDefinition());
        wallWorkspace.ColumnDefinitions.Add(new ColumnDefinition()); wallWorkspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0) });
        var views = new Grid(); wallWorkspace.Children.Add(views);
        wallCards = BuildWallItems("cards"); views.Children.Add(wallCards);
        wallScreens = BuildWallItems("screens"); views.Children.Add(wallScreens);
        wallList = BuildWallList(); views.Children.Add(wallList);
        wallMainPanel = BuildWallMainPanel(); Grid.SetColumn(wallMainPanel, 1); wallWorkspace.Children.Add(wallMainPanel);
        page.Children.Add(wallWorkspace); SetWallViewMode("list"); return page;
    }

    private FrameworkElement BuildWallDensity()
    {
        var density = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 6), ToolTip = "屏幕视图目标列数；窄窗口自动减少列数" };
        density.Children.Add(new TextBlock { Text = "屏数", Foreground = WallMuted, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 9, 0) });
        var slider = new Slider { Minimum = 5, Maximum = 20, TickFrequency = 1, IsSnapToTickEnabled = true, Width = 94, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 9, 0) };
        slider.SetBinding(RangeBase.ValueProperty, TwoWay(nameof(WorkspaceViewModel.WallColumns))); density.Children.Add(slider);
        var decreaseCommand = new ActionCommand(() => ViewModel.WallColumns--, () => ViewModel.WallColumns > 5);
        var increaseCommand = new ActionCommand(() => ViewModel.WallColumns++, () => ViewModel.WallColumns < 20);
        var decrease = WallButton("\uE738", "减少屏数", decreaseCommand); decrease.Content = WallGlyph("\uE738"); decrease.Width = 28; decrease.MinWidth = 0; decrease.Padding = new Thickness(0); density.Children.Add(decrease);
        var count = BoundText(nameof(WorkspaceViewModel.WallColumns)); count.Width = 24; count.TextAlignment = TextAlignment.Center; count.FontWeight = FontWeights.SemiBold; count.Margin = new Thickness(0, 0, 5, 0); density.Children.Add(count);
        var increase = WallButton("\uE710", "增加屏数", increaseCommand); increase.Content = WallGlyph("\uE710"); increase.Width = 28; increase.MinWidth = 0; increase.Padding = new Thickness(0); density.Children.Add(increase);
        ViewModel.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(WorkspaceViewModel.WallColumns)) { decreaseCommand.Refresh(); increaseCommand.Refresh(); } };
        wallDensity = density; return density;
    }

    private FrameworkElement BuildWallItems(string mode)
    {
        var grid = new Grid();
        var items = new ItemsControl { ItemsSource = unifiedDeviceView, HorizontalContentAlignment = HorizontalAlignment.Stretch };
        var panel = new FrameworkElementFactory(typeof(UniformGrid));
        var columns = new MultiBinding { Converter = new WallColumnsConverter(mode == "screens" ? 118 : 344, mode == "screens") };
        columns.Bindings.Add(new Binding(nameof(WorkspaceViewModel.WallColumns)) { Source = ViewModel });
        columns.Bindings.Add(new Binding(nameof(ActualWidth)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ScrollViewer), 1) });
        panel.SetBinding(UniformGrid.ColumnsProperty, columns); items.ItemsPanel = new ItemsPanelTemplate(panel);
        var tile = new FrameworkElementFactory(typeof(UnifiedDeviceTile)); tile.SetBinding(UnifiedDeviceTile.OwnerProperty, new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MainWindow), 1) }); tile.SetValue(UnifiedDeviceTile.ModeProperty, mode);
        items.ItemTemplate = new DataTemplate { VisualTree = tile };
        grid.Children.Add(new ScrollViewer { Content = items, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
        var empty = new TextBlock { Text = "当前筛选没有设备", Foreground = WallMuted, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        empty.SetBinding(VisibilityProperty, new Binding(nameof(ItemsControl.HasItems)) { Source = items, Converter = new WallEmptyVisibilityConverter() }); grid.Children.Add(empty); return grid;
    }

    private FrameworkElement BuildWallList()
    {
        var grid = new DataGrid { ItemsSource = unifiedDeviceView, AutoGenerateColumns = false, CanUserAddRows = false, CanUserDeleteRows = false, SelectionMode = DataGridSelectionMode.Single, HeadersVisibility = DataGridHeadersVisibility.Column, RowHeight = 48, ColumnHeaderHeight = 40, Background = Brushes.White, BorderThickness = new Thickness(0), GridLinesVisibility = DataGridGridLinesVisibility.Horizontal, HorizontalGridLinesBrush = WallBorder, IsReadOnly = true };
        grid.SelectionChanged += (_, _) => { if (wallViewMode == "list" && grid.SelectedItem is UnifiedDeviceRow { Device: { } device }) ViewModel.CurrentDevice = device; };
        var checkbox = new FrameworkElementFactory(typeof(CheckBox)); checkbox.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center); checkbox.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center); checkbox.SetBinding(ToggleButton.IsCheckedProperty, TwoWay(nameof(UnifiedDeviceRow.IsSelected))); checkbox.SetBinding(IsEnabledProperty, new Binding(nameof(UnifiedDeviceRow.CanSelect)));
        grid.Columns.Add(new DataGridTemplateColumn { Header = "选择", Width = 54, CellTemplate = new DataTemplate { VisualTree = checkbox } });
        TextColumn(grid, "编号", nameof(UnifiedDeviceRow.Number), 58);
        TextColumn(grid, "设备名称", nameof(UnifiedDeviceRow.Name), 175); TextColumn(grid, "状态", nameof(UnifiedDeviceRow.State), 110);
        var actions = new FrameworkElementFactory(typeof(UnifiedDeviceTile)); actions.SetBinding(UnifiedDeviceTile.OwnerProperty, new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MainWindow), 1) }); actions.SetValue(UnifiedDeviceTile.ModeProperty, "list");
        grid.Columns.Add(new DataGridTemplateColumn { Header = "操作", Width = 214, CellTemplate = new DataTemplate { VisualTree = actions } });
        TextColumn(grid, "型号", nameof(UnifiedDeviceRow.Model), 145); TextColumn(grid, "品牌", nameof(UnifiedDeviceRow.Brand), 110); TextColumn(grid, "安卓", nameof(UnifiedDeviceRow.Android), 80);
        TextColumn(grid, "设备序列号", nameof(UnifiedDeviceRow.Serial), 185); TextColumn(grid, "分辨率", nameof(UnifiedDeviceRow.Resolution), 120); TextColumn(grid, "分组", nameof(UnifiedDeviceRow.Group), 110);
        var page = new Grid(); page.Children.Add(grid);
        var empty = new TextBlock { Text = "当前筛选没有设备", Foreground = WallMuted, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        empty.SetBinding(VisibilityProperty, new Binding(nameof(ItemsControl.HasItems)) { Source = grid, Converter = new WallEmptyVisibilityConverter() }); page.Children.Add(empty); return page;
    }

    private FrameworkElement BuildWallMainPanel()
    {
        var main = new DockPanel { LastChildFill = true, MinHeight = 360 };
        var viewport = new ScrollViewer { Content = main, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Margin = new Thickness(18, 0, 0, 0) };
        main.SetBinding(HeightProperty, new Binding(nameof(ActualHeight)) { Source = viewport });
        var heading = BoundText(nameof(WorkspaceViewModel.MainContext), true); heading.FontWeight = FontWeights.SemiBold; heading.Foreground = WallInk; heading.Margin = new Thickness(0, 0, 0, 12); DockPanel.SetDock(heading, Dock.Top); main.Children.Add(heading);
        var bottom = new StackPanel { Margin = new Thickness(0, 12, 0, 0) }; DockPanel.SetDock(bottom, Dock.Bottom); main.Children.Add(bottom);
        var keys = Toolbar(); keys.Children.Add(Button("\uE72B", "返回", () => ViewModel.KeyAsync(4), canExecute: ViewModel.CanControlMainInput)); keys.Children.Add(Button("\uE80F", "主页", () => ViewModel.KeyAsync(3), canExecute: ViewModel.CanControlMainInput)); keys.Children.Add(Button("\uE7C4", "任务", () => ViewModel.KeyAsync(187), canExecute: ViewModel.CanControlMainInput)); bottom.Children.Add(keys);
        var cast = new Expander { Header = "当前设备投屏", Margin = new Thickness(0, 8, 0, 0) };
        var castActions = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
        castActions.Children.Add(Button("\uE714", "启动 scrcpy 投屏", OpenScreenAsync, canExecute: CanLaunchScrcpy));
        castActions.Children.Add(Button("\uE8F2", "嵌入自有投屏窗口", EmbedScreenAsync, canExecute: CanEmbedScrcpy));
        castActions.Children.Add(Button("\uE8A7", "弹出投屏窗口", RestoreScreensAsync)); cast.Content = castActions; bottom.Children.Add(cast);
        var image = new Image { Stretch = Stretch.Uniform, MinHeight = 100, Focusable = true, Cursor = Cursors.Cross };
        image.SetBinding(Image.SourceProperty, new Binding("MainDevice.Screenshot")); image.MouseLeftButtonDown += MainImageDown; image.MouseLeftButtonUp += MainImageUp;
        var screen = new Grid { Background = new SolidColorBrush(Color.FromRgb(247, 249, 252)) }; screen.Children.Add(image);
        var empty = new TextBlock { Text = "未指定主控", Foreground = WallMuted, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        empty.SetBinding(VisibilityProperty, new Binding(nameof(WorkspaceViewModel.MainDevice)) { Converter = new WallNullVisibilityConverter() }); screen.Children.Add(empty); main.Children.Add(screen); return viewport;
    }

    internal void ClearWallSelection()
    {
        foreach (var row in unifiedDevices.ToArray()) row.IsSelected = false;
        foreach (var row in ViewModel.Devices) row.IsSelected = false;
        ViewModel.UpdateSelection(); RefreshLaunchCommands();
    }

    internal void SetWallViewMode(string mode)
    {
        if (mode is not ("cards" or "screens" or "list")) throw new ArgumentException("设备视图不合法：" + mode, nameof(mode));
        wallViewMode = mode;
        wallCards.Visibility = mode == "cards" ? Visibility.Visible : Visibility.Collapsed;
        wallScreens.Visibility = mode == "screens" ? Visibility.Visible : Visibility.Collapsed;
        wallList.Visibility = mode == "list" ? Visibility.Visible : Visibility.Collapsed;
        foreach (var choice in wallViewChoices) choice.Value.IsChecked = choice.Key == mode;
        wallDensity.IsEnabled = mode == "screens"; wallMainToggle.IsChecked = mode == "screens"; UpdateWallMainPanel();
        wallDensity.Visibility = wallMainToggle.Visibility = mode == "screens" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateWallMainPanel()
    {
        if (wallMainPanel is null || wallWorkspace is null) return;
        var visible = wallMainToggle.IsChecked == true;
        wallMainPanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed; wallWorkspace.ColumnDefinitions[1].Width = new GridLength(visible ? 272 : 0);
    }

    private static readonly Brush WallInk = new SolidColorBrush(Color.FromRgb(24, 34, 57));
    private static readonly Brush WallMuted = new SolidColorBrush(Color.FromRgb(99, 113, 138));
    private static readonly Brush WallBorder = new SolidColorBrush(Color.FromRgb(228, 232, 240));
    private static TextBlock WallGlyph(string glyph) => new() { Text = glyph, FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 14, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
    private Button WallButton(string glyph, string title, ICommand command, bool primary = false)
    {
        var button = Button(glyph, title, command); button.Height = 34; button.Margin = new Thickness(0, 0, 8, 6); button.Padding = new Thickness(11, 0, 11, 0);
        if (TryFindResource(primary ? "PrimaryButton" : "SubtleButton") is Style style) button.Style = style; return button;
    }
    private ToggleButton WallToggle(string glyph, string title)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center }; content.Children.Add(WallGlyph(glyph)); content.Children.Add(new TextBlock { Text = title, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(7, 0, 0, 0) });
        var toggle = new ToggleButton { Content = content, ToolTip = title, Height = 34, Padding = new Thickness(11, 0, 11, 0), Margin = new Thickness(0, 0, 8, 6) };
        if (TryFindResource("SegmentedToggle") is Style style) toggle.Style = style; return toggle;
    }

    private void MainImageDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Image image || ViewModel.MainDevice is not { } row || !row.Info.Online || ViewModel.Busy) return;
        if (MapImage(image, e.GetPosition(image)) is not { } point) return;
        gestureStart = point; gestureDevice = row; gestureGeneration = ViewModel.Runtime.Input.CurrentGeneration; gestureDeviceGeneration = row.Info.Generation;
        gestureTargets = ViewModel.InputTargets(row.Serial, true).ToArray(); gestureTargetGenerations.Clear();
        foreach (var serial in gestureTargets) if (ViewModel.Devices.FirstOrDefault(device => device.Serial == serial) is { } target) gestureTargetGenerations[serial] = target.Info.Generation;
        image.CaptureMouse(); e.Handled = true;
    }
    private async void MainImageUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Image image || gestureStart is not { } start || gestureDevice is not { } row) return;
        var end = MapImage(image, e.GetPosition(image)); gestureStart = null; gestureDevice = null; image.ReleaseMouseCapture();
        if (end is null || !ReferenceEquals(row, ViewModel.MainDevice) || row.Info.Generation != gestureDeviceGeneration || ViewModel.Runtime.Input.CurrentGeneration != gestureGeneration) return;
        if (gestureTargetGenerations.Any(saved => ViewModel.Devices.FirstOrDefault(target => target.Serial == saved.Key)?.Info.Generation != saved.Value)) return;
        var swipe = (start - end.Value).Length > 8;
        await ViewModel.SendInputAsync(new InputSnapshot { SourceSerial = row.Serial, Kind = swipe ? InputKind.Swipe : InputKind.Tap, X = (int)start.X, Y = (int)start.Y, X2 = (int)end.Value.X, Y2 = (int)end.Value.Y, DurationMs = 300, SourceWidth = (image.Source as BitmapSource)?.PixelWidth ?? row.Info.Width, SourceHeight = (image.Source as BitmapSource)?.PixelHeight ?? row.Info.Height, SourceGeneration = gestureDeviceGeneration, Generation = gestureGeneration, TargetSerials = gestureTargets }); e.Handled = true;
    }
}

public sealed class WallColumnsConverter(double minimumWidth, bool usePreferred) : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var preferred = values.FirstOrDefault() is int count ? count : 5;
        var available = values.Length > 1 && values[1] is double width && double.IsFinite(width) && width > 0 ? width : minimumWidth;
        var responsive = Math.Max(1, (int)Math.Floor(available / minimumWidth));
        return usePreferred ? Math.Min(preferred, responsive) : responsive;
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class WallEmptyVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is true ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class WallNullVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is null ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

```

## src/MatrixDevice.Desktop/Views/WorkspaceShell.cs

```csharp
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace MatrixDevice.Desktop;

public partial class MainWindow
{
    private ColumnDefinition sidebarColumn = null!;
    private Grid workspace = null!;
    private readonly Dictionary<string, ToggleButton> navigation = [];
    private readonly List<FrameworkElement> sidebarCaptions = [];
    private FrameworkElement sidebarStatistics = null!;
    private TextBlock collapseGlyph = null!;
    private TextBlock workspaceCounts = null!;
    private Window? directoryWindow;
    private Window? scannerWindow;
    private bool sidebarCollapsed;
    private static readonly (string Tag, string Label, string Icon)[] WorkspacePages =
    [
        ("设备墙 / 主控", "设备管理", "\uE8EA"), ("雷电实例", "雷电实例", "\uE7F4"),
        ("节点", "节点调试", "\uE8A5"), ("截图 / 取色", "截图取色", "\uE722"),
        ("Shell", "命令终端", "\uE756"), ("应用", "应用管理", "\uE71D"),
        ("文件", "文件传输", "\uE8B7"), ("文本", "文本输入", "\uE8D2"),
        ("脚本", "脚本管理", "\uE8A5"), ("API", "接口服务", "\uE9D9"),
        ("扩展", "扩展工具", "\uE713"), ("设置", "环境设置", "\uE713")
    ];

    private static Brush WorkspaceBrush(string value) => (Brush)new BrushConverter().ConvertFromString(value)!;
    private static TextBlock ShellGlyph(string value, double size = 17) => new()
    {
        Text = value, FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = size,
        VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center
    };

    private void BuildWorkspaceLayout()
    {
        sidebarColumn = new ColumnDefinition { Width = new GridLength(212) };
        Shell.ColumnDefinitions.Add(sidebarColumn);
        Shell.ColumnDefinitions.Add(new ColumnDefinition());
        Shell.Children.Add(BuildNavigation());
        workspace = new Grid();
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        workspace.RowDefinitions.Add(new RowDefinition());
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Grid.SetColumn(workspace, 1); Shell.Children.Add(workspace);
        workspace.Children.Add(BuildSearchBar());

        var notice = new Border { Background = WorkspaceBrush("#FFF4F2"), Padding = new Thickness(20, 9, 20, 9) };
        var error = BoundText(nameof(WorkspaceViewModel.Error), true); error.Foreground = WorkspaceBrush("#AE3F39"); notice.Child = error;
        notice.SetBinding(VisibilityProperty, new Binding(nameof(WorkspaceViewModel.Error)) { Converter = new NonEmptyVisibilityConverter() });
        Grid.SetRow(notice, 1); workspace.Children.Add(notice);
        var context = BuildWorkspaceContext(); Grid.SetRow(context, 2); workspace.Children.Add(context);
        var footer = new Grid { Margin = new Thickness(20, 8, 20, 10) };
        footer.ColumnDefinitions.Add(new ColumnDefinition()); footer.ColumnDefinitions.Add(new ColumnDefinition());
        workspaceCounts = BoundText(nameof(WorkspaceViewModel.Counts)); workspaceCounts.FontSize = 11; workspaceCounts.Foreground = WorkspaceBrush("#63718A"); footer.Children.Add(workspaceCounts);
        var status = BoundText(nameof(WorkspaceViewModel.Status)); status.FontSize = 11; status.TextAlignment = TextAlignment.Right;
        status.TextTrimming = TextTrimming.CharacterEllipsis; status.SetBinding(ToolTipProperty, new Binding(nameof(WorkspaceViewModel.Status)));
        Grid.SetColumn(status, 1); footer.Children.Add(status); Grid.SetRow(footer, 4); workspace.Children.Add(footer);
    }

    private Border BuildNavigation()
    {
        var sidebar = new Border { Background = Brushes.White, BorderBrush = WorkspaceBrush("#E4E8F0"), BorderThickness = new Thickness(0, 0, 1, 0) };
        var dock = new DockPanel(); sidebar.Child = dock;
        var brand = new Grid { Margin = new Thickness(18, 24, 12, 22) };
        brand.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) }); brand.ColumnDefinitions.Add(new ColumnDefinition());
        var mark = new Border { Width = 36, Height = 36, CornerRadius = new CornerRadius(8), Background = WorkspaceBrush("#3274F6") };
        var icon = ShellGlyph("\uE80A", 20); icon.Foreground = Brushes.White; mark.Child = icon; brand.Children.Add(mark);
        var identity = new StackPanel { Margin = new Thickness(9, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        identity.Children.Add(new TextBlock { Text = "矩阵设备", FontSize = 19, FontWeight = FontWeights.SemiBold });
        identity.Children.Add(new TextBlock { Text = "Android 设备工作台", FontSize = 11, Foreground = WorkspaceBrush("#63718A"), Margin = new Thickness(0, 4, 0, 0) });
        Grid.SetColumn(identity, 1); brand.Children.Add(identity); sidebarCaptions.Add(identity);
        DockPanel.SetDock(brand, Dock.Top); dock.Children.Add(brand);

        var bottom = new StackPanel(); DockPanel.SetDock(bottom, Dock.Bottom); dock.Children.Add(bottom);
        var collapseContent = new StackPanel { Orientation = Orientation.Horizontal };
        collapseGlyph = ShellGlyph("\uE76B", 14); collapseGlyph.Width = 24; collapseContent.Children.Add(collapseGlyph);
        var collapseText = new TextBlock { Text = "折叠侧栏", Margin = new Thickness(9, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        collapseContent.Children.Add(collapseText); sidebarCaptions.Add(collapseText);
        var collapse = new Button { Content = collapseContent, ToolTip = "展开或折叠导航", Margin = new Thickness(12, 4, 12, 12), HorizontalContentAlignment = HorizontalAlignment.Left };
        collapse.SetResourceReference(StyleProperty, "SubtleButton"); collapse.Click += (_, _) => SetSidebarCollapsed(!sidebarCollapsed); bottom.Children.Add(collapse);
        var statistics = new StackPanel { Margin = new Thickness(18, 15, 14, 16) };
        statistics.Children.Add(new TextBlock { Text = "本机工作区", FontWeight = FontWeights.SemiBold, FontSize = 12 });
        var mode = BoundText(nameof(WorkspaceViewModel.ModeLabel), true); mode.Foreground = WorkspaceBrush("#63718A"); mode.FontSize = 11; mode.Margin = new Thickness(0, 7, 0, 5); statistics.Children.Add(mode);
        var summary = BoundText(nameof(WorkspaceViewModel.ApiSummary), true); summary.FontSize = 11; summary.Foreground = WorkspaceBrush("#63718A"); statistics.Children.Add(summary);
        sidebarStatistics = new Border { BorderBrush = WorkspaceBrush("#E4E8F0"), BorderThickness = new Thickness(0, 1, 0, 0), Child = statistics }; bottom.Children.Add(sidebarStatistics);

        var links = new StackPanel { Margin = new Thickness(10, 0, 10, 8) };
        foreach (var item in WorkspacePages)
        {
            if (item.Tag == "雷电实例") continue;
            var content = new StackPanel { Orientation = Orientation.Horizontal };
            var glyph = ShellGlyph(item.Icon); glyph.Width = 28; content.Children.Add(glyph);
            var caption = new TextBlock { Text = item.Label, Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            content.Children.Add(caption); sidebarCaptions.Add(caption);
            var link = new ToggleButton { Content = content, ToolTip = item.Label, Margin = new Thickness(0, 2, 0, 2), MinHeight = 40, HorizontalContentAlignment = HorizontalAlignment.Left };
            link.SetResourceReference(StyleProperty, "NavigationButton");
            System.Windows.Automation.AutomationProperties.SetName(link, item.Label);
            link.Click += (_, _) => NavigateTo(item.Tag);
            navigation.Add(item.Tag, link); links.Children.Add(link);
        }
        dock.Children.Add(new ScrollViewer { Content = links, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
        return sidebar;
    }

    private FrameworkElement BuildSearchBar()
    {
        var surface = new Border { Background = Brushes.White, BorderBrush = WorkspaceBrush("#E4E8F0"), BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(20, 15, 20, 15) };
        var row = new Grid(); surface.Child = row;
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(158) }); row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var group = new ComboBox { ItemsSource = ViewModel.Groups, ItemTemplate = CaptionChoices(), ToolTip = "设备分组", Margin = new Thickness(0, 0, 12, 0), MinHeight = 38 };
        group.SetBinding(Selector.SelectedItemProperty, TwoWay(nameof(WorkspaceViewModel.GroupFilter))); row.Children.Add(group);

        var search = new Grid { Margin = new Thickness(0, 0, 12, 0) };
        var input = BoundInput(nameof(WorkspaceViewModel.Search)); input.MinHeight = 38; input.Padding = new Thickness(33, 6, 30, 6);
        input.ToolTip = "搜索设备名称或序列号"; System.Windows.Automation.AutomationProperties.SetName(input, "搜索设备"); search.Children.Add(input);
        var searchIcon = ShellGlyph("\uE721", 14); searchIcon.Margin = new Thickness(12, 0, 0, 0); searchIcon.HorizontalAlignment = HorizontalAlignment.Left;
        searchIcon.Foreground = WorkspaceBrush("#8B96AB"); searchIcon.IsHitTestVisible = false; search.Children.Add(searchIcon);
        var hint = new TextBlock { Text = "搜索设备名称、序列号", Foreground = WorkspaceBrush("#63718A"), Margin = new Thickness(33, 0, 28, 0), VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false, TextTrimming = TextTrimming.CharacterEllipsis };
        search.Children.Add(hint); input.TextChanged += (_, _) => hint.Visibility = input.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        hint.Visibility = input.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        Grid.SetColumn(search, 1); row.Children.Add(search);
        var settings = new Button { Content = ShellGlyph("\uE713"), ToolTip = "环境设置", MinHeight = 38 };
        settings.SetResourceReference(StyleProperty, "IconButton"); settings.Click += (_, _) => NavigateTo("设置"); Grid.SetColumn(settings, 4); row.Children.Add(settings);
        var advanced = new ContextMenu();
        var network = new MenuItem { Header = "网络连接（高级）" }; network.Click += (_, _) => ShowScanner(); advanced.Items.Add(network);
        var refresh = new MenuItem { Header = "立即更新目录" }; refresh.Click += async (_, _) => await UpdateDeviceDirectoryAsync(); advanced.Items.Add(refresh);
        settings.ContextMenu = advanced;
        return surface;
    }

    private FrameworkElement BuildWorkspaceContext()
    {
        var surface = new Border { Background = Brushes.White, BorderBrush = WorkspaceBrush("#E4E8F0"), BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(20, 10, 20, 10) };
        var row = new Grid(); surface.Child = row;
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }); row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        foreach (var (label, path, index) in new[] { ("当前设备", nameof(WorkspaceViewModel.DeviceContext), 0), ("主控设备", nameof(WorkspaceViewModel.MainContext), 1), ("操作目标", nameof(WorkspaceViewModel.TargetContext), 2) })
        {
            var content = new StackPanel { Margin = new Thickness(0, 0, 15, 0) };
            content.Children.Add(new TextBlock { Text = label, FontSize = 11, Foreground = WorkspaceBrush("#63718A"), Margin = new Thickness(0, 0, 0, 4) });
            var value = BoundText(path); value.FontSize = 12; value.TextTrimming = TextTrimming.CharacterEllipsis; value.SetBinding(ToolTipProperty, new Binding(path)); content.Children.Add(value);
            Grid.SetColumn(content, index); row.Children.Add(content);
        }
        var details = new Button { Content = ShellGlyph("\uE8A7"), ToolTip = "设备详情与分组", VerticalAlignment = VerticalAlignment.Center };
        details.SetResourceReference(StyleProperty, "IconButton"); details.Click += (_, _) => ShowDeviceDirectory(); Grid.SetColumn(details, 3); row.Children.Add(details);
        return surface;
    }

    private static ControlTemplate WorkspaceContentTemplate()
    {
        var presenter = new FrameworkElementFactory(typeof(ContentPresenter), "PART_SelectedContentHost");
        presenter.SetBinding(ContentPresenter.ContentProperty, new Binding(nameof(TabControl.SelectedContent)) { RelativeSource = RelativeSource.TemplatedParent });
        presenter.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding(nameof(TabControl.SelectedContentTemplate)) { RelativeSource = RelativeSource.TemplatedParent });
        presenter.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Stretch); presenter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Stretch);
        return new ControlTemplate(typeof(TabControl)) { VisualTree = presenter };
    }

    private void AttachWorkspacePages()
    {
        Grid.SetRow(tabs, 3); workspace.Children.Add(tabs); UpdateNavigation();
    }

    internal void NavigateTo(string tag)
    {
        var page = tabs.Items.Cast<TabItem>().SingleOrDefault(item => Equals(item.Tag, tag))
            ?? throw new ArgumentException("导航页面不存在：" + tag);
        tabs.SelectedItem = page; UpdateNavigation();
    }

    private void UpdateNavigation()
    {
        var selected = (tabs.SelectedItem as TabItem)?.Tag?.ToString();
        var activeTag = selected == "雷电实例" ? "设备墙 / 主控" : selected;
        foreach (var item in navigation) item.Value.IsChecked = item.Key == activeTag;
        foreach (var mode in deviceModes) mode.Button.IsChecked = mode.Tag == selected;
        if (activeTag is not null && navigation.TryGetValue(activeTag, out var active) && active.IsLoaded) active.BringIntoView();
        var label = WorkspacePages.FirstOrDefault(page => page.Tag == selected).Label ?? "设备管理";
        Title = "矩阵设备 · " + label + (ViewModel.Demo ? " · 演示模式" : "");
    }

    private void SetSidebarCollapsed(bool collapsed)
    {
        sidebarCollapsed = collapsed; sidebarColumn.Width = new GridLength(collapsed ? 72 : 212);
        foreach (var caption in sidebarCaptions) caption.Visibility = collapsed ? Visibility.Collapsed : Visibility.Visible;
        sidebarStatistics.Visibility = collapsed ? Visibility.Collapsed : Visibility.Visible;
        collapseGlyph.Text = collapsed ? "\uE76C" : "\uE76B";
    }

    private void ShowDeviceDirectory()
    {
        if (directoryWindow is null)
        {
            directoryWindow = new Window { Owner = this, DataContext = ViewModel, Title = "设备与分组", Width = 420, Height = 640, MinWidth = 360, MinHeight = 480, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            directoryWindow.Content = BuildDirectory();
            directoryWindow.Closing += (_, args) => { if (!closing) { args.Cancel = true; directoryWindow.Hide(); } };
        }
        directoryWindow.Show(); directoryWindow.Activate();
    }

    private void ShowScanner()
    {
        if (scannerWindow is null)
        {
            var scanner = BuildScanner();
            if (scanner is Expander expander) expander.IsExpanded = true;
            scannerWindow = new Window { Owner = this, DataContext = ViewModel, Title = "网络连接（高级）", Width = 460, Height = 420, MinWidth = 360, MinHeight = 360, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            scannerWindow.Content = new ScrollViewer { Content = new Border { Padding = new Thickness(20), Child = scanner }, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
            scannerWindow.Closing += (_, args) => { if (!closing) { scanCancellation?.Cancel(); args.Cancel = true; scannerWindow.Hide(); } };
        }
        scannerWindow.Show(); scannerWindow.Activate();
    }
}

```

## src/MatrixDevice.Desktop/WorkspaceViewModel.cs

```csharp
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

namespace MatrixDevice.Desktop;

public sealed class WorkspaceViewModel : ObservableObject
{
    private MatrixRuntime runtime;
    private readonly string dataDirectory;
    private readonly bool demo;
    private readonly List<object> commands = [];
    private CancellationTokenSource? transferCancellation;
    private DeviceRow? currentDevice;
    private DeviceRow? mainDevice;
    private string search = "";
    private string groupFilter = "全部分组";
    private string statusFilter = "全部状态";
    private bool busy;
    private bool externalBusy;
    private int pendingLaunchSelection;
    private bool blockApiWhileBusy = true;
    private string status = "等待发现设备";
    private string error = "";
    private BitmapSource? debugImage;
    private AndroidNode? nodeRoot;
    private NodeRow? selectedNode;
    private string nodeSearch = "";
    private string shellInput = "";
    private string textInput = "";
    private string remotePath = "/sdcard/MatrixExchange/";
    private bool transferring;
    private bool showSystemApps;
    private string appSearch = "";
    private AppInfo? selectedApp;
    private ScriptPackage? package;
    private bool syncMouse;
    private bool syncKeyboard;
    private int wallColumns;
    private string deviceName = "";
    private string deviceGroup = "";
    private string settingsNotice = "";

    public WorkspaceViewModel(MatrixRuntime initialRuntime, string dataDirectory, bool demo)
    {
        runtime = initialRuntime;
        this.dataDirectory = dataDirectory;
        this.demo = demo;
        error = string.Join("\n", new[] { runtime.DiscoveryError, runtime.ApiStartupError }.Where(message => message is not null));
        syncMouse = runtime.Settings.SyncMouse;
        syncKeyboard = runtime.Settings.SyncKeyboard;
        wallColumns = runtime.Settings.WallColumns;
        Settings = new SettingsEditor(runtime.Settings);
        DeviceView = CollectionViewSource.GetDefaultView(Devices);
        DeviceView.Filter = item => item is DeviceRow row && Matches(row);
        AppsView = CollectionViewSource.GetDefaultView(Apps);
        AppsView.Filter = item => item is AppInfo app && (string.IsNullOrWhiteSpace(AppSearch) || app.PackageName.Contains(AppSearch, StringComparison.OrdinalIgnoreCase) || (app.Name?.Contains(AppSearch, StringComparison.OrdinalIgnoreCase) ?? false));
        RefreshCommand = Async(RefreshDevicesAsync, () => !Busy);
        ScreenshotCommand = Async(() => CaptureAsync(), CanUseDevice);
        WallRefreshCommand = Async(RefreshWallAsync, () => !Busy && Devices.Any(row => row.Info.Online));
        DumpCommand = Async(LoadNodesAsync, CanUseDevice);
        ShellCommand = Async(ExecuteShellAsync, () => CanWriteDevice() && !string.IsNullOrWhiteSpace(ShellInput));
        RefreshAppsCommand = Async(LoadAppsAsync, CanUseDevice);
        LaunchAppCommand = Async(() => ChangeAppAsync("launch"), () => CanWriteTargets() && SelectedApp is not null);
        StopAppCommand = Async(() => ChangeAppAsync("stop"), () => CanWriteTargets() && SelectedApp is not null);
        ClearAppCommand = Async(() => ChangeAppAsync("clear"), () => CanWriteTargets() && SelectedApp is not null);
        UninstallAppCommand = Async(() => ChangeAppAsync("uninstall"), () => CanWriteTargets() && SelectedApp is not null);
        TextCommand = Async(SendTextAsync, () => CanWriteTargets() && !string.IsNullOrWhiteSpace(TextInput));
        StartTransfersCommand = Async(StartTransfersAsync, () => !Busy && !Transferring && Transfers.Any(PendingTransfer));
        CancelTransfersCommand = Action(() => transferCancellation?.Cancel(), () => Transferring);
        StartScriptCommand = Async(StartScriptAsync, () => !Busy && !Transferring && package is not null && Targets().Any() && Targets().All(row => row.Info.Online && !ScriptBusy(row.Serial)));
        StopScriptCommand = Action(StopScripts, () => Targets().Any(row => ScriptBusy(row.Serial)));
        StartApiCommand = Async(StartApisAsync, () => !Busy && (!runtime.Api.BasicRunning || !runtime.Api.AutomatorRunning));
        StopApiCommand = Async(StopApisAsync, () => !Busy && (runtime.Api.BasicRunning || runtime.Api.AutomatorRunning));
        StartDeviceApiCommand = Async(StartDeviceApisAsync, () => !Busy && Targets().Any() && Targets().All(row => row.Info.Online && !ScriptBusy(row.Serial)));
        StopDeviceApiCommand = Async(StopDeviceApisAsync, () => !Busy && Targets().Any(row => runtime.Api.DevicePorts.ContainsKey(row.Serial)));
        SaveMetadataCommand = Async(SaveMetadataAsync, () => !Busy && CurrentDevice is not null);
        SaveSettingsCommand = Async(SaveSettingsAsync, () => !Busy);
        runtime.Scripts.EntryAdded += OnScriptLog;
        runtime.Scripts.StatusChanged += OnScriptStatus;
        LocalizationService.Current.LanguageChanged += OnLanguageChanged;
    }

    public MatrixRuntime Runtime => runtime;
    internal string DataDirectory => dataDirectory;
    public bool Demo => demo;
    public string ModeLabel => LocalizationService.Current.Get(demo ? "演示设备 · 不连接真实 ADB" : "真实设备模式");
    public ObservableCollection<DeviceRow> Devices { get; } = [];
    public System.ComponentModel.ICollectionView DeviceView { get; }
    public ObservableCollection<string> Groups { get; } = ["全部分组", "未分组"];
    public ObservableCollection<NodeRow> Nodes { get; } = [];
    public ObservableCollection<ShellEntry> ShellHistory { get; } = [];
    public ObservableCollection<PixelSample> Colors { get; } = [];
    public ObservableCollection<AppInfo> Apps { get; } = [];
    public System.ComponentModel.ICollectionView AppsView { get; }
    public ObservableCollection<TransferRow> Transfers { get; } = [];
    public ObservableCollection<ParameterEditor> Parameters { get; } = [];
    public ObservableCollection<string> ScriptLogs { get; } = [];
    public ObservableCollection<string> Activity { get; } = [];
    public SettingsEditor Settings { get; }
    public AsyncCommand RefreshCommand { get; }
    public AsyncCommand ScreenshotCommand { get; }
    public AsyncCommand WallRefreshCommand { get; }
    public AsyncCommand DumpCommand { get; }
    public AsyncCommand ShellCommand { get; }
    public AsyncCommand RefreshAppsCommand { get; }
    public AsyncCommand LaunchAppCommand { get; }
    public AsyncCommand StopAppCommand { get; }
    public AsyncCommand ClearAppCommand { get; }
    public AsyncCommand UninstallAppCommand { get; }
    public AsyncCommand TextCommand { get; }
    public AsyncCommand StartTransfersCommand { get; }
    public ActionCommand CancelTransfersCommand { get; }
    public AsyncCommand StartScriptCommand { get; }
    public ActionCommand StopScriptCommand { get; }
    public AsyncCommand StartApiCommand { get; }
    public AsyncCommand StopApiCommand { get; }
    public AsyncCommand StartDeviceApiCommand { get; }
    public AsyncCommand StopDeviceApiCommand { get; }
    public AsyncCommand SaveMetadataCommand { get; }
    public AsyncCommand SaveSettingsCommand { get; }
    public Action? NodesChanged { get; set; }
    public Action? DeviceChanged { get; set; }
    public Action? WallChanged { get; set; }
    public Action? NodeSelectionChanged { get; set; }
    public Func<string, string, bool> Confirm { get; set; } = (title, message) => MessageBox.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK;

    public DeviceRow? CurrentDevice
    {
        get => currentDevice;
        set
        {
            if (!Set(ref currentDevice, value)) return;
            DeviceName = value?.Name ?? "";
            DeviceGroup = value?.Info.GroupId ?? "";
            DebugImage = value?.Screenshot;
            ClearNodes();
            Apps.Clear();
            LoadScriptLogs();
            LoadParameters();
            Changed(nameof(DeviceContext));
            RefreshCommands();
            DeviceChanged?.Invoke();
        }
    }
    public DeviceRow? MainDevice { get => mainDevice; private set { Set(ref mainDevice, value); Changed(nameof(MainContext)); WallChanged?.Invoke(); } }
    public string DeviceContext => CurrentDevice is { } row ? $"{row.Name} · {row.Serial} · {LocalizationService.Current.Get(row.State)}" : LocalizationService.Current.Get("未选择设备");
    public string MainContext => MainDevice is { } row ? $"{LocalizationService.Current.Get("主控")}: {row.Name} · {row.Serial}" : $"{LocalizationService.Current.Get("主控")}: {LocalizationService.Current.Get("未指定")}";
    public string Search { get => search; set { Set(ref search, value); RefreshFilter(); } }
    public string GroupFilter { get => groupFilter; set { Set(ref groupFilter, value); RefreshFilter(); } }
    public string StatusFilter { get => statusFilter; set { Set(ref statusFilter, value); RefreshFilter(); } }
    public bool Busy { get => busy || externalBusy; private set { Set(ref busy, value); SyncApiWriteGuard(); RefreshCommands(); } }
    internal void SetExternalBusy(bool value) { externalBusy = value; Changed(nameof(Busy)); SyncApiWriteGuard(); RefreshCommands(); }
    private void SyncApiWriteGuard() => runtime.Api.UiWriteBusy = externalBusy || (busy && blockApiWhileBusy) || Transferring;
    public string Status { get => LocalizationService.Current.Get(status); private set => Set(ref status, value); }
    public string Error { get => error; private set => Set(ref error, value); }
    public string Counts => $"{LocalizationService.Current.Get("发现")} {Devices.Count}   {LocalizationService.Current.Get("就绪")} {Devices.Count(row => row.Info.Online)}   {LocalizationService.Current.Get("筛选")} {DeviceView.Cast<object>().Count()}   {LocalizationService.Current.Get("选中")} {Devices.Count(row => row.IsSelected)}";
    public string TargetContext => pendingLaunchSelection > 0 ? $"已选目标包含 {pendingLaunchSelection} 台未连接设备" : $"{LocalizationService.Current.Get("目标")} {Targets().Count}: {string.Join(", ", Targets().Select(row => row.Name))}";
    public BitmapSource? DebugImage { get => debugImage; private set => Set(ref debugImage, value); }
    public NodeRow? SelectedNode { get => selectedNode; set { Set(ref selectedNode, value); Changed(nameof(NodeProperties)); NodeSelectionChanged?.Invoke(); } }
    public string NodeProperties => SelectedNode is { } row ? JsonSerializer.Serialize(row.Source with { ChildNodeList = [] }, JsonDefaults.Options) : "未选中节点";
    public string NodeSearch { get => nodeSearch; set { Set(ref nodeSearch, value); NodesChanged?.Invoke(); } }
    public string ShellInput { get => shellInput; set { Set(ref shellInput, value); RefreshCommands(); } }
    public string TextInput { get => textInput; set { Set(ref textInput, value); RefreshCommands(); } }
    public string RemotePath { get => remotePath; set => Set(ref remotePath, value); }
    public bool Transferring { get => transferring; private set { Set(ref transferring, value); SyncApiWriteGuard(); RefreshCommands(); } }
    public double OverallProgress => Transfers.Count == 0 ? 0 : Transfers.Average(row => row.Progress);
    public bool ShowSystemApps { get => showSystemApps; set => Set(ref showSystemApps, value); }
    public string AppSearch { get => appSearch; set { Set(ref appSearch, value); AppsView.Refresh(); } }
    public AppInfo? SelectedApp { get => selectedApp; set { Set(ref selectedApp, value); RefreshCommands(); } }
    public string PackageSummary => package is { } value ? $"{value.Manifest.Name} · {value.Manifest.Version}\n{value.Manifest.Description}" : LocalizationService.Current.Get("未导入脚本");
    public bool SyncMouse { get => syncMouse; set => Set(ref syncMouse, value); }
    public bool SyncKeyboard { get => syncKeyboard; set => Set(ref syncKeyboard, value); }
    public int WallColumns { get => wallColumns; set { Set(ref wallColumns, Math.Clamp(value, 5, 20)); WallChanged?.Invoke(); } }
    public string DeviceName { get => deviceName; set => Set(ref deviceName, value); }
    public string DeviceGroup { get => deviceGroup; set => Set(ref deviceGroup, value); }
    public string SettingsNotice { get => settingsNotice; private set => Set(ref settingsNotice, value); }
    public string ApiSummary => $"普通接口：{(runtime.Api.RunningBasicPort is { } basic ? $"http://127.0.0.1:{basic}" : "未启动")}    自动化接口：{(runtime.Api.RunningAutomatorPort is { } automator ? $"http://127.0.0.1:{automator}" : "未启动")}";
    public bool CanUseDevice() => !Busy && CurrentDevice?.Info.Online == true;
    public bool CanWriteDevice() => CanUseDevice() && !Transferring && !ScriptBusy(CurrentDevice!.Serial);
    public bool CanWriteTargets() => !Busy && !Transferring && Targets().Count > 0 && Targets().All(row => row.Info.Online && !ScriptBusy(row.Serial));
    public bool CanControlMainInput()
    {
        if (pendingLaunchSelection > 0 && (SyncMouse || SyncKeyboard)) return false;
        var source = MainDevice ?? CurrentDevice;
        return !Busy && !Transferring && source?.Info.Online == true && !ScriptBusy(source.Serial) &&
            InputTargets(source.Serial, false).All(serial => Devices.Any(row => row.Serial == serial && row.Info.Online && !ScriptBusy(serial)));
    }
    public IReadOnlyList<DeviceRow> Targets() { if (pendingLaunchSelection > 0) return []; var selected = Devices.Where(row => row.IsSelected).ToList(); return selected.Count > 0 ? selected : CurrentDevice is null ? [] : [CurrentDevice]; }
    internal void SetPendingLaunchSelection(int count)
    {
        if (pendingLaunchSelection == count) return;
        pendingLaunchSelection = count; RefreshCommands();
    }

    private AsyncCommand Async(Func<Task> action, Func<bool> canExecute) { var command = new AsyncCommand(action, canExecute); commands.Add(command); return command; }
    private ActionCommand Action(Action action, Func<bool> canExecute) { var command = new ActionCommand(action, canExecute); commands.Add(command); return command; }
    private void RefreshCommands() { foreach (var command in commands) { if (command is AsyncCommand asyncCommand) asyncCommand.Refresh(); else if (command is ActionCommand actionCommand) actionCommand.Refresh(); } Changed(nameof(TargetContext)); }
    private bool ScriptBusy(string serial) => runtime.Scripts.GetStatus(serial).State.ToString() is "Running" or "Cancelling";
    private bool Matches(DeviceRow row) => (string.IsNullOrWhiteSpace(Search) || row.Name.Contains(Search, StringComparison.OrdinalIgnoreCase) || row.Serial.Contains(Search, StringComparison.OrdinalIgnoreCase)) && (GroupFilter == "全部分组" || row.Group == GroupFilter) && (StatusFilter == "全部状态" || row.Info.State == StatusFilter);
    private void RefreshFilter() { DeviceView.Refresh(); Changed(nameof(Counts)); WallChanged?.Invoke(); }
    public void SelectFiltered(bool selected) { foreach (var row in DeviceView.Cast<DeviceRow>()) row.IsSelected = selected; UpdateSelection(); }
    public void UpdateSelection() { Changed(nameof(Counts)); RefreshCommands(); }

    public async Task ExecuteAsync(string label, Func<Task> action, bool blocksApiWrites = true)
    {
        if (Busy) return;
        blockApiWhileBusy = blocksApiWrites; Busy = true;
        Error = "";
        Status = $"{label}…";
        try { await action(); Status = $"{label}完成"; AddActivity(Status); }
        catch (OperationCanceledException) { Status = $"{label}已停止"; AddActivity(Status); }
        catch (Exception ex) { Error = $"{label}失败：{PresentError(ex)}"; Status = "操作失败"; AddActivity(Error); }
        finally { Busy = false; blockApiWhileBusy = true; }
    }

    private void AddActivity(string message) { Activity.Insert(0, $"{DateTime.Now:HH:mm:ss}  {message}"); while (Activity.Count > 500) Activity.RemoveAt(Activity.Count - 1); }
    public string PresentError(Exception error)
    {
        try { runtime.Log.Write("desktop-error", error.ToString(), CurrentDevice?.Serial); }
        catch (Exception logError) { System.Diagnostics.Trace.WriteLine("界面诊断日志写入失败：" + logError); }
        return UiErrorPresentation.Describe(error);
    }
    public Task RefreshDevicesAsync() => ExecuteAsync("发现设备", () => RefreshDeviceSnapshotAsync());

    internal async Task RefreshDeviceSnapshotAsync(CancellationToken ct = default)
    {
        await runtime.RefreshAsync(ct);
        await ApplyCachedDeviceSnapshotAsync();
    }

    private async Task ApplyCachedDeviceSnapshotAsync()
    {
        var rows = runtime.GetDevices();
        foreach (var info in rows)
        {
            var row = Devices.FirstOrDefault(item => item.Serial == info.Serial);
            if (row is null)
            {
                row = new DeviceRow(info);
                row.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(DeviceRow.IsSelected)) UpdateSelection();
                    if (args.PropertyName == nameof(DeviceRow.Info)) { Changed(nameof(DeviceContext)); Changed(nameof(MainContext)); Changed(nameof(Counts)); RefreshCommands(); WallChanged?.Invoke(); }
                };
                Devices.Add(row);
            }
            else row.Info = info;
        }
        foreach (var row in Devices.Where(row => !rows.Any(info => info.Serial == row.Serial)).ToList()) Devices.Remove(row);
        Groups.Clear(); Groups.Add("全部分组"); Groups.Add("未分组");
        foreach (var group in Devices.Select(row => row.Group).Distinct().Where(group => group != "未分组").Order()) Groups.Add(group);
        if (!Groups.Contains(GroupFilter)) GroupFilter = "全部分组";
        if (CurrentDevice is not null && !Devices.Contains(CurrentDevice)) CurrentDevice = null;
        if (MainDevice is not null && !Devices.Contains(MainDevice)) { await runtime.Input.HandoffAsync(null); MainDevice = null; }
        RefreshApiRows();
        RefreshFilter();
        if (demo && CurrentDevice is null) CurrentDevice = Devices.FirstOrDefault();
    }

    internal bool ConnectionReloadAvailable => !Transferring && !Devices.Any(row => ScriptBusy(row.Serial)) && !runtime.Api.BasicRunning && !runtime.Api.AutomatorRunning && runtime.Api.DevicePorts.Count == 0 && !runtime.Api.HasActiveRequests;

    internal async Task ApplyLdConnectionsAsync(IReadOnlyList<LdConnectionProof> proofs, CancellationToken ct)
    {
        if (!ConnectionReloadAvailable) throw new DeviceException("请先停止接口服务、脚本和文件传输，再接入设备。", 409, 1003);
        runtime.RequireFrozenConnectionSettings();
        var old = runtime;
        var source = old.Backend as AdbDeviceBackend ?? throw new DeviceException("演示模式不接入真实设备。", 409, 1003);
        var service = old.LdPlayer ?? throw new DeviceException("雷电服务未配置。", 409, 1003);
        var settings = LdConnectionAdmission.Apply(old.Settings, proofs);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct); deadline.CancelAfter(TimeSpan.FromSeconds(60));
        await old.Input.HandoffAsync(null, deadline.Token);
        await using (var probe = new AdbDeviceBackend(settings with { AdbAllowedSerials = proofs.Select(proof => proof.Serial).ToArray(), CommandTimeoutSeconds = 15 }))
        {
            probe.InheritServerEpoch(source);
            foreach (var proof in proofs)
                probe.RegisterIdentityGuard(proof.Serial, token => LdPlayerAdbBinding.VerifyAsync(proof.Serial, proof.VBoxPid, proof.VBoxBirth, token));
            var observed = await probe.DiscoverAsync(deadline.Token);
            foreach (var proof in proofs)
            {
                if (!observed.Any(device => device.Serial == proof.Serial && device.Online && device.TransportId == proof.TransportId)) throw new DeviceException("已核验实例的ADB连接已变化。", 409, 1003);
                while ((await probe.ShellAsync(proof.Serial, "getprop sys.boot_completed", ct: deadline.Token)).Trim() != "1") await Task.Delay(500, deadline.Token);
                PngImage.ReadDimensions(await probe.ScreenshotAsync(proof.Serial, deadline.Token));
                var current = (await service.RefreshAsync(deadline.Token)).Single(item => item.Index == proof.Index);
                if (current.Generation != proof.Generation) throw new DeviceException("接入过程中实例发生变化，未保存新授权。", 409, 1003);
            }
        }
        MatrixRuntime? replacement = null;
        try
        {
            replacement = await MatrixRuntime.CreateAsync(settings, dataDirectory, false, proofs, source, deadline.Token);
            foreach (var proof in proofs)
                if (!replacement.GetDevices().Any(device => device.Serial == proof.Serial && device.Online && device.TransportId == proof.TransportId)) throw new DeviceException("新连接未就绪，原工作区保持不变。", 409, 1003);
            if (!ConnectionReloadAvailable) throw new DeviceException("工作区出现活动任务，取消接入。", 409, 1003);
            deadline.Token.ThrowIfCancellationRequested();
            replacement.Settings = replacement.SettingsStore.Save(settings);
        }
        catch { if (replacement is not null) await replacement.DisposeAsync(); throw; }
        old.Scripts.EntryAdded -= OnScriptLog; old.Scripts.StatusChanged -= OnScriptStatus;
        runtime = replacement;
        runtime.Scripts.EntryAdded += OnScriptLog; runtime.Scripts.StatusChanged += OnScriptStatus;
        SyncApiWriteGuard();
        Settings.AdbSerialsJson = JsonSerializer.Serialize(runtime.Settings.AdbAllowedSerials, JsonDefaults.Options);
        Settings.LdBindingsJson = JsonSerializer.Serialize(runtime.Settings.LdSerialBindings, JsonDefaults.Options);
        Settings.Changed(nameof(SettingsEditor.AdbSerialsJson)); Settings.Changed(nameof(SettingsEditor.LdBindingsJson));
        MainDevice = null; ClearNodes(); Apps.Clear();
        try { await old.DisposeAsync(); } catch (Exception ex) { runtime.Log.Write("previous-runtime-cleanup-error", ex.ToString()); }
        await ApplyCachedDeviceSnapshotAsync();
        Changed(nameof(ApiSummary)); AddActivity($"已接入{proofs.Count}台雷电设备");
    }

    public Task SaveMetadataAsync() => ExecuteAsync("保存设备信息", async () =>
    {
        var row = CurrentDevice ?? throw new InvalidOperationException("未选择设备");
        runtime.Metadata.UpsertMetadata(row.Serial, DeviceName.Trim(), DeviceGroup.Trim());
        row.Info = row.Info with { Name = DeviceName.Trim(), GroupId = DeviceGroup.Trim() };
        if (!string.IsNullOrWhiteSpace(DeviceGroup) && !Groups.Contains(DeviceGroup.Trim())) Groups.Add(DeviceGroup.Trim());
        RefreshFilter();
        Changed(nameof(DeviceContext));
        await Task.CompletedTask;
    });

    public Task CaptureAsync() => ExecuteAsync("刷新截图", async () =>
    {
        await RefreshGeometryAsync();
        var row = CurrentDevice ?? throw new InvalidOperationException("未选择设备");
        var bytes = await runtime.Backend.ScreenshotAsync(row.Serial);
        row.Screenshot = DecodeImage(bytes);
        row.CapturedAt = DateTimeOffset.UtcNow;
        row.ScreenState = "截图可用";
        if (ReferenceEquals(CurrentDevice, row))
        {
            ClearNodes();
            DebugImage = row.Screenshot;
        }
        WallChanged?.Invoke();
    });

    public Task RefreshWallAsync() => ExecuteAsync("刷新设备墙", async () =>
    {
        await RefreshGeometryAsync();
        foreach (var row in Devices.Where(row => row.Info.Online))
        {
            try { row.Screenshot = DecodeImage(await runtime.Backend.ScreenshotAsync(row.Serial)); row.CapturedAt = DateTimeOffset.UtcNow; row.ScreenState = "截图可用"; }
            catch (Exception ex) { row.ScreenState = $"截图失败：{PresentError(ex)}"; }
        }
        if (CurrentDevice is not null)
        {
            ClearNodes();
            DebugImage = CurrentDevice.Screenshot;
        }
        WallChanged?.Invoke();
    });

    internal async Task RefreshAutomaticScreensAsync(string[] serials, CancellationToken ct)
    {
        foreach (var serial in serials)
        {
            ct.ThrowIfCancellationRequested();
            var row = Devices.FirstOrDefault(item => item.Serial == serial && item.Info.Online);
            if (row is null) continue;
            try { row.Screenshot = DecodeImage(await runtime.Backend.ScreenshotAsync(serial, ct)); row.CapturedAt = DateTimeOffset.UtcNow; row.ScreenState = "截图可用"; }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { row.ScreenState = "截图失败：" + PresentError(ex); runtime.Log.Write("auto-screenshot-error", ex.Message, serial); }
        }
        if (CurrentDevice is not null) { ClearNodes(); DebugImage = CurrentDevice.Screenshot; }
        WallChanged?.Invoke();
    }

    public async Task SetMainAsync(DeviceRow row)
    {
        await ExecuteAsync("切换主控", async () => { await runtime.Input.HandoffAsync(row.Serial); MainDevice = row; CurrentDevice = row; });
    }

    public Task SendInputAsync(InputSnapshot input) => ExecuteAsync("发送输入", async () =>
    {
        if (pendingLaunchSelection > 0 && (input.Kind is InputKind.Key or InputKind.Text ? SyncKeyboard : SyncMouse)) throw new DeviceException("同步目标中存在未连接设备，请先连接或取消选择。", 409, 1003);
        if (Transferring) throw new DeviceException("文件传输进行中，完成或停止传输后再控制");
        var source = Devices.FirstOrDefault(row => row.Serial == input.SourceSerial);
        if (input.Kind is InputKind.Tap or InputKind.Swipe && source is not null &&
            (input.SourceWidth != source.Info.Width || input.SourceHeight != source.Info.Height))
            throw new DeviceException("截图与设备当前尺寸不一致，请刷新截图后重试；未发送手势");
        if (ScriptBusy(input.SourceSerial) || input.TargetSerials.Any(ScriptBusy)) throw new DeviceException("目标设备正在执行脚本，停止脚本后再控制");
        await runtime.Input.SendAsync(input);
    });

    public IReadOnlyList<string> InputTargets(string source, bool mouse) => (mouse ? SyncMouse : SyncKeyboard) ? Devices.Where(row => row.IsSelected && row.Serial != source).Select(row => row.Serial).ToArray() : [];
    public Task KeyAsync(int code)
    {
        var row = MainDevice ?? CurrentDevice;
        if (row is null) return Task.CompletedTask;
        return SendInputAsync(new InputSnapshot { SourceSerial = row.Serial, SourceGeneration = row.Info.Generation, Generation = runtime.Input.CurrentGeneration, Kind = InputKind.Key, KeyCode = code, TargetSerials = InputTargets(row.Serial, false) });
    }

    public Task SendTextAsync() => ExecuteAsync("发送文本", async () =>
    {
        var targets = RequireTargets();
        var text = TextInput;
        foreach (var row in targets) await runtime.Backend.TextAsync(row.Serial, text);
    });

    public Task LoadNodesAsync() => ExecuteAsync("刷新截图与节点", async () =>
    {
        await RefreshGeometryAsync();
        var row = CurrentDevice ?? throw new InvalidOperationException("未选择设备");
        var bytes = await runtime.Backend.ScreenshotAsync(row.Serial);
        row.Screenshot = DecodeImage(bytes);
        row.CapturedAt = DateTimeOffset.UtcNow;
        row.ScreenState = "截图可用";
        if (ReferenceEquals(CurrentDevice, row))
        {
            ClearNodes();
            DebugImage = row.Screenshot;
        }
        var freshRoot = await runtime.Backend.DumpAsync(row.Serial);
        row.AutomationState = "节点可用";
        if (!ReferenceEquals(CurrentDevice, row)) return;
        nodeRoot = freshRoot;
        Nodes.Clear(); Nodes.Add(new NodeRow(nodeRoot));
        SelectedNode = null;
        NodesChanged?.Invoke();
    });

    private async Task RefreshGeometryAsync()
    {
        await runtime.RefreshAsync();
        var current = runtime.GetDevices();
        foreach (var row in Devices)
            row.Info = current.FirstOrDefault(device => device.Serial == row.Serial) ?? row.Info with { State = "Offline" };
    }

    private void ClearNodes()
    {
        if (CurrentDevice is { } row) row.AutomationState = "尚未刷新节点";
        nodeRoot = null;
        Nodes.Clear();
        SelectedNode = null;
        NodesChanged?.Invoke();
    }

    public Task ExportNodesAsync(string path) => ExecuteAsync("导出节点", async () =>
    {
        if (nodeRoot is null) throw new InvalidOperationException("尚未刷新节点");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(nodeRoot, JsonDefaults.Options));
    });

    public IEnumerable<NodeRow> FlattenNodes()
    {
        IEnumerable<NodeRow> Traverse(NodeRow row) { yield return row; foreach (var child in row.Children) foreach (var nested in Traverse(child)) yield return nested; }
        return Nodes.SelectMany(Traverse);
    }

    public void SamplePixel(int x, int y)
    {
        if (DebugImage is null || x < 0 || y < 0 || x >= DebugImage.PixelWidth || y >= DebugImage.PixelHeight) return;
        var converted = new FormatConvertedBitmap(DebugImage, PixelFormats.Bgra32, null, 0);
        var pixel = new byte[4];
        converted.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);
        Colors.Insert(0, new PixelSample(x, y, pixel[2], pixel[1], pixel[0]));
        while (Colors.Count > 100) Colors.RemoveAt(Colors.Count - 1);
    }

    public Task SaveScreenshotAsync(string path, Int32Rect? crop = null) => ExecuteAsync(crop is null ? "保存截图" : "保存裁剪", async () =>
    {
        if (DebugImage is null) throw new InvalidOperationException("尚未加载截图");
        BitmapSource source = crop is { } region ? new CroppedBitmap(DebugImage, region) : DebugImage;
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(source));
        using var memory = new MemoryStream(); encoder.Save(memory);
        await File.WriteAllBytesAsync(path, memory.ToArray());
    });

    public Task ExecuteShellAsync() => ExecuteAsync("执行 Shell", async () =>
    {
        var row = CurrentDevice ?? throw new InvalidOperationException("未选择设备");
        var command = ShellInput.Trim();
        if (command.Length == 0) return;
        if (ScriptBusy(row.Serial)) throw new DeviceException("设备正在执行脚本");
        try
        {
            var output = await runtime.Backend.ShellAsync(row.Serial, command);
            ShellHistory.Insert(0, new ShellEntry(ShellHistory.Count + 1, DateTime.Now.ToString("HH:mm:ss"), command, output, row.Serial));
        }
        catch (Exception ex)
        {
            ShellHistory.Insert(0, new ShellEntry(ShellHistory.Count + 1, DateTime.Now.ToString("HH:mm:ss"), command, "执行失败：" + PresentError(ex), row.Serial));
            throw;
        }
        finally { ShellInput = ""; }
    });

    public Task LoadAppsAsync() => ExecuteAsync("读取应用列表", async () =>
    {
        var row = CurrentDevice ?? throw new InvalidOperationException("未选择设备");
        var apps = await runtime.Backend.ListAppsAsync(row.Serial, ShowSystemApps);
        if (!ReferenceEquals(CurrentDevice, row)) return;
        Apps.Clear(); foreach (var app in apps.OrderBy(app => app.PackageName)) Apps.Add(app);
        SelectedApp = null;
    });

    public Task InstallAppAsync(string path) => ExecuteAsync("安装 APK", async () =>
    {
        var targets = RequireTargets();
        var results = new List<string>();
        foreach (var row in targets)
        {
            try { await runtime.Backend.InstallAsync(row.Serial, path); results.Add($"{row.Serial}：安装完成"); }
            catch (Exception ex) { results.Add($"{row.Serial}：失败 {PresentError(ex)}"); }
        }
        AddActivity(string.Join("；", results));
        if (results.Any(result => result.Contains("：失败", StringComparison.Ordinal))) throw new InvalidOperationException(string.Join("；", results));
    });

    private Task ChangeAppAsync(string operation) => ExecuteAsync("应用操作", async () =>
    {
        var app = SelectedApp ?? throw new InvalidOperationException("未选择应用");
        var targets = RequireTargets();
        var label = operation switch { "clear" => "清除应用数据", "uninstall" => "卸载应用", "launch" => "启动应用", _ => "停止应用" };
        if (operation is "clear" or "uninstall")
        {
            var detail = $"{label}\n包名：{app.PackageName}\n设备数量：{targets.Count}\n{string.Join("\n", targets.Select(row => $"{row.Name} ({row.Serial})"))}\n此操作会删除应用数据。";
            if (!Confirm(label, detail)) return;
        }
        var failures = new List<string>();
        foreach (var row in targets)
        {
            try
            {
                switch (operation)
                {
                    case "launch": await runtime.Backend.StartAppAsync(row.Serial, app.PackageName); break;
                    case "stop": await runtime.Backend.StopAppAsync(row.Serial, app.PackageName); break;
                    case "clear": if (!await runtime.Backend.ClearAppAsync(row.Serial, app.PackageName)) throw new InvalidOperationException("设备未确认清除完成"); break;
                    case "uninstall": await runtime.Backend.UninstallAsync(row.Serial, app.PackageName); break;
                }
                AddActivity($"{label} · {row.Serial} · {app.PackageName}");
            }
            catch (Exception ex) { failures.Add($"{row.Serial}：{PresentError(ex)}"); }
        }
        if (failures.Count > 0) throw new InvalidOperationException(string.Join("；", failures));
        if (operation == "uninstall") Apps.Remove(app);
    });

    private IReadOnlyList<DeviceRow> RequireTargets()
    {
        if (pendingLaunchSelection > 0) throw new DeviceException($"选中目标包含 {pendingLaunchSelection} 台未连接设备，请先连接或取消选择；未操作其它设备。", 409, 1003);
        if (Transferring) throw new DeviceException("文件传输进行中，完成或停止传输后再操作");
        var targets = Targets();
        if (targets.Count == 0) throw new InvalidOperationException("未选择设备");
        var invalid = targets.FirstOrDefault(row => !row.Info.Online || ScriptBusy(row.Serial));
        if (invalid is not null) throw new DeviceException($"设备 {invalid.Serial} 离线或正在执行脚本");
        return targets;
    }

    public void QueuePush(string path, bool autoInstall)
    {
        foreach (var row in RequireTargets()) Transfers.Add(new TransferRow { Serial = row.Serial, Direction = "上传", LocalPath = path, RemotePath = RemotePath.TrimEnd('/') + "/" + Path.GetFileName(path), AutoInstall = autoInstall && Path.GetExtension(path).Equals(".apk", StringComparison.OrdinalIgnoreCase) });
        RefreshCommands();
    }

    public void QueuePull(string localPath)
    {
        var row = CurrentDevice ?? throw new InvalidOperationException("未选择设备");
        Transfers.Add(new TransferRow { Serial = row.Serial, Direction = "下载", LocalPath = localPath, RemotePath = RemotePath });
        RefreshCommands();
    }

    public async Task StartTransfersAsync()
    {
        if (Busy || Transferring) return;
        transferCancellation = new CancellationTokenSource();
        Transferring = true;
        Error = "";
        try
        {
            foreach (var row in Transfers.Where(PendingTransfer).ToList())
            {
                if (transferCancellation.IsCancellationRequested) break;
                row.State = "传输中";
                row.Progress = 0;
                try
                {
                    if (ScriptBusy(row.Serial)) throw new DeviceException("设备正在执行脚本");
                    var progress = new Progress<double>(value => { row.Progress = Math.Clamp(value > 1 ? value : value * 100, 0, 100); Changed(nameof(OverallProgress)); });
                    if (row.Direction == "上传") await runtime.Backend.PushAsync(row.Serial, row.LocalPath, row.RemotePath, progress, transferCancellation.Token);
                    else await runtime.Backend.PullAsync(row.Serial, row.RemotePath, row.LocalPath, progress, transferCancellation.Token);
                    row.Progress = 100;
                    row.State = "传输完成";
                    if (row.AutoInstall) { row.State = "已传输 · 安装中"; await runtime.Backend.InstallAsync(row.Serial, row.LocalPath, transferCancellation.Token); row.State = "已传输 · 已安装"; }
                }
                catch (OperationCanceledException) { row.State = "已停止"; break; }
                catch (Exception ex) { row.State = $"失败：{PresentError(ex)}"; Error = $"传输 {row.Serial} / {row.Name}：{PresentError(ex)}"; }
            }
            Changed(nameof(OverallProgress));
        }
        finally { Transferring = false; transferCancellation.Dispose(); transferCancellation = null; }
    }

    private static bool PendingTransfer(TransferRow row) => row.State is "待传输" or "已停止" || row.State.StartsWith("失败", StringComparison.Ordinal);

    public Task ImportScriptAsync(string path) => ExecuteAsync("导入脚本", async () =>
    {
        package = ScriptPackageService.Load(path);
        LoadParameters();
        Changed(nameof(PackageSummary));
        await Task.CompletedTask;
    });

    private void LoadParameters()
    {
        Parameters.Clear();
        if (package is null) return;
        var saved = CurrentDevice is null ? null : runtime.Metadata.GetAll<ScriptDeviceParameters>("ScriptDeviceConfiguration").FirstOrDefault(value => value.Serial == CurrentDevice.Serial && value.ScriptId == package.Manifest.Id && value.Version == package.Manifest.Version);
        foreach (var parameter in package.Parameters)
        {
            var value = saved is not null && saved.Values.TryGetValue(parameter.Key, out var stored) ? stored.GetRawText() : (parameter.Value ?? parameter.DefaultValue)?.GetRawText() ?? "null";
            Parameters.Add(new ParameterEditor { Key = parameter.Key, Type = parameter.Type ?? "未声明", Label = parameter.LabelZh ?? parameter.LabelEn ?? parameter.Key, Value = value, Range = $"{parameter.Min} .. {parameter.Max} {parameter.Unit}" });
        }
    }

    private ScriptPackage ParameterizedPackage()
    {
        if (package is null) throw new InvalidOperationException("未导入脚本");
        var clone = JsonNode.Parse(JsonSerializer.Serialize(package, JsonDefaults.Options))!.AsObject();
        if (clone["parameters"] is JsonArray array)
        {
            foreach (var node in array.OfType<JsonObject>())
            {
                var key = node["key"]?.GetValue<string>();
                var editor = Parameters.FirstOrDefault(parameter => parameter.Key == key);
                if (editor is not null) node["value"] = JsonNode.Parse(editor.Value);
            }
        }
        return clone.Deserialize<ScriptPackage>(JsonDefaults.Options) ?? throw new InvalidOperationException("脚本参数无效");
    }

    public Task StartScriptAsync()
    {
        try
        {
            var targets = RequireTargets().ToList();
            var selectedPackage = ParameterizedPackage();
            ScriptPackageService.Validate(selectedPackage);
            var values = selectedPackage.Parameters.ToDictionary(parameter => parameter.Key, parameter => parameter.Value ?? parameter.DefaultValue ?? JsonSerializer.SerializeToElement<object?>(null));
            foreach (var row in targets) runtime.Metadata.Put("ScriptDeviceConfiguration", row.Serial + "\u001f" + selectedPackage.Manifest.Id, new ScriptDeviceParameters(row.Serial, selectedPackage.Manifest.Id, selectedPackage.Manifest.Version, values));
            Error = "";
            foreach (var row in targets) row.ScriptState = "准备执行";
            RefreshCommands();
            foreach (var row in targets) _ = ObserveScriptAsync(row.Serial, selectedPackage);
        }
        catch (Exception ex) { Error = $"脚本未启动：{PresentError(ex)}"; }
        finally { RefreshCommands(); }
        return Task.CompletedTask;
    }

    private async Task ObserveScriptAsync(string serial, ScriptPackage selectedPackage)
    {
        try { await runtime.Scripts.RunAsync(serial, selectedPackage); }
        catch (Exception ex) { await Application.Current.Dispatcher.InvokeAsync(() => { Error = $"脚本 {serial}：{PresentError(ex)}"; AddActivity(Error); }); }
    }

    private void StopScripts() { foreach (var row in Targets()) runtime.Scripts.Stop(row.Serial); RefreshCommands(); }
    private void OnScriptLog(string serial, ScriptLogEntry entry) => Application.Current.Dispatcher.BeginInvoke(() => { if (CurrentDevice?.Serial == serial) { ScriptLogs.Add(FormatScriptLog(entry)); while (ScriptLogs.Count > 500) ScriptLogs.RemoveAt(0); } });
    private void OnScriptStatus(ScriptRunStatus state) => Application.Current.Dispatcher.BeginInvoke(() => { var row = Devices.FirstOrDefault(row => row.Serial == state.Serial); if (row is not null) row.ScriptState = LocalizationService.Current.Get(state.State.ToString()); RefreshCommands(); });
    private void LoadScriptLogs() { ScriptLogs.Clear(); if (CurrentDevice is not null) foreach (var entry in runtime.Scripts.GetLogs(CurrentDevice.Serial)) ScriptLogs.Add(FormatScriptLog(entry)); }
    private static string FormatScriptLog(ScriptLogEntry entry)
    {
        var message = entry.Message;
        foreach (var prefix in new[] { "执行失败：", "状态监听失败：" })
        {
            if (!message.StartsWith(prefix, StringComparison.Ordinal)) continue;
            message = prefix + UiErrorPresentation.Describe(new InvalidOperationException(message[prefix.Length..]));
            break;
        }
        return $"{entry.Timestamp:HH:mm:ss}  {message}";
    }

    public Task StartApisAsync() => ExecuteAsync("启动 API", async () => { await runtime.StartApisAsync(CancellationToken.None); RefreshApiRows(); });
    public Task StopApisAsync() => ExecuteAsync("停止 API", async () => { await runtime.StopApisAsync(CancellationToken.None); RefreshApiRows(); });
    private Task StartDeviceApisAsync() => ExecuteAsync("启动设备 API", async () => { foreach (var row in RequireTargets()) await runtime.Api.StartDeviceAsync(row.Serial, CancellationToken.None); RefreshApiRows(); });
    private Task StopDeviceApisAsync() => ExecuteAsync("停止设备 API", async () => { foreach (var row in Targets()) await runtime.Api.StopDeviceAsync(row.Serial, CancellationToken.None); RefreshApiRows(); });
    private void RefreshApiRows() { foreach (var row in Devices) row.ApiAddress = runtime.Api.DevicePorts.TryGetValue(row.Serial, out var port) ? $"http://127.0.0.1:{port}" : "未启动"; Changed(nameof(ApiSummary)); RefreshCommands(); }

    public Task SaveSettingsAsync() => ExecuteAsync("保存设置", async () =>
    {
        var settings = Settings.ToSettings() with { WallColumns = WallColumns, SyncMouse = SyncMouse, SyncKeyboard = SyncKeyboard };
        if ((runtime.Api.BasicRunning && settings.BasicApiPort != runtime.Settings.BasicApiPort) || (runtime.Api.AutomatorRunning && settings.AutomatorApiPort != runtime.Settings.AutomatorApiPort)) throw new InvalidOperationException("请先停止正在运行的 API，再修改端口");
        runtime.Settings = runtime.SettingsStore.Save(settings);
        SettingsNotice = "设置已保存；设备调试服务、自动化服务与雷电连接配置重启后生效";
        AddActivity(SettingsNotice);
        await Task.CompletedTask;
    });

    public async Task ShutdownAsync()
    {
        transferCancellation?.Cancel();
        foreach (var row in Devices) runtime.Scripts.Stop(row.Serial);
        await runtime.Input.ReleaseAsync(CancellationToken.None);
        runtime.Scripts.EntryAdded -= OnScriptLog;
        runtime.Scripts.StatusChanged -= OnScriptStatus;
        LocalizationService.Current.LanguageChanged -= OnLanguageChanged;
        await runtime.DisposeAsync();
    }
    private void OnLanguageChanged(object? sender, EventArgs args)
    {
        Changed(nameof(ModeLabel)); Changed(nameof(Counts)); Changed(nameof(DeviceContext)); Changed(nameof(MainContext));
        Changed(nameof(TargetContext)); Changed(nameof(PackageSummary)); Changed(nameof(ApiSummary));
        Changed(nameof(Status));
    }

    public bool HasActiveOperations => Busy || Transferring || Devices.Any(row => ScriptBusy(row.Serial));
    public static BitmapSource DecodeImage(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        var image = new BitmapImage(); image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad; image.StreamSource = stream; image.EndInit(); image.Freeze(); return image;
    }
}

public sealed class ParameterEditor : ObservableObject
{
    private string value = "null";
    public required string Key { get; init; }
    public required string Type { get; init; }
    public required string Label { get; init; }
    public string Range { get; init; } = "";
    public string Value { get => value; set => Set(ref this.value, value); }
}

public sealed record ScriptDeviceParameters(string Serial, string ScriptId, string Version, Dictionary<string, JsonElement> Values);

public sealed class SettingsEditor : ObservableObject
{
    private readonly AppSettings original;
    public SettingsEditor(AppSettings settings)
    {
        original = settings;
        AdbPath = settings.AdbPath; AdbSerialsJson = JsonSerializer.Serialize(settings.AdbAllowedSerials, JsonDefaults.Options); ScrcpyPath = settings.ScrcpyPath; LdConsolePath = settings.LdConsolePath; LdBindingsJson = JsonSerializer.Serialize(settings.LdSerialBindings, JsonDefaults.Options); AgentBaseUrl = settings.AgentBaseUrl ?? ""; AgentSerial = settings.AgentSerial ?? "";
        BasicApiPort = settings.BasicApiPort; AutomatorApiPort = settings.AutomatorApiPort; AutoConnect = settings.AutoConnect; AutoStartApi = settings.AutoStartApi;
        Language = "zh-CN"; MainMaxSize = settings.MainMaxSize; MiniMaxSize = settings.MiniMaxSize; MainMaxFps = settings.MainMaxFps; MiniMaxFps = settings.MiniMaxFps;
        MainBitRate = settings.MainBitRate; MiniBitRate = settings.MiniBitRate; ForceHardwareDecode = settings.ForceHardwareDecode; ShowFps = settings.ShowFps;
        WindowWidth = settings.WindowWidth; WindowHeight = settings.WindowHeight; FileRoot = settings.FileRoot; CommandTimeoutSeconds = settings.CommandTimeoutSeconds;
    }
    public string AdbPath { get; set; }
    public string AdbSerialsJson { get; set; }
    public string ScrcpyPath { get; set; }
    public string LdConsolePath { get; set; }
    public string LdBindingsJson { get; set; }
    public string AgentBaseUrl { get; set; }
    public string AgentSerial { get; set; }
    public int BasicApiPort { get; set; }
    public int AutomatorApiPort { get; set; }
    public bool AutoConnect { get; set; }
    public bool AutoStartApi { get; set; }
    public string Language { get; set; }
    public int MainMaxSize { get; set; }
    public int MiniMaxSize { get; set; }
    public int MainMaxFps { get; set; }
    public int MiniMaxFps { get; set; }
    public int MainBitRate { get; set; }
    public int MiniBitRate { get; set; }
    public bool ForceHardwareDecode { get; set; }
    public bool ShowFps { get; set; }
    public double WindowWidth { get; set; }
    public double WindowHeight { get; set; }
    public string FileRoot { get; set; }
    public int CommandTimeoutSeconds { get; set; }
    public AppSettings ToSettings()
    {
        if (BasicApiPort is < 1024 or > 65535 || AutomatorApiPort is < 1024 or > 65535 || BasicApiPort == AutomatorApiPort) throw new InvalidOperationException("API 端口必须为不同的 1024–65535 整数");
        if (MainMaxSize is < 64 or > 8192 || MiniMaxSize is < 64 or > 8192 || MainMaxFps is < 1 or > 240 || MiniMaxFps is < 1 or > 240) throw new InvalidOperationException("投屏尺寸或帧率超出有效范围");
        if (MainBitRate is < 10000 or > 100000000 || MiniBitRate is < 10000 or > 100000000) throw new InvalidOperationException("码率必须为 10000–100000000 bit/s");
        if (WindowWidth < 1000 || WindowHeight < 700) throw new InvalidOperationException("窗体至少为 1000 × 700");
        if (CommandTimeoutSeconds is < 1 or > 600) throw new InvalidOperationException("命令超时必须为 1–600 秒");
        var bindings = JsonSerializer.Deserialize<Dictionary<int, string>>(LdBindingsJson, JsonDefaults.Options) ?? [];
        var adbSerials = JsonSerializer.Deserialize<string[]>(AdbSerialsJson, JsonDefaults.Options) ?? throw new InvalidOperationException("ADB授权串号必须是JSON数组");
        if (adbSerials.Any(serial => string.IsNullOrWhiteSpace(serial) || serial.StartsWith('-') || serial.Contains('\0') || serial.Any(char.IsWhiteSpace)) || adbSerials.Distinct(StringComparer.Ordinal).Count() != adbSerials.Length)
            throw new InvalidOperationException("ADB授权串号必须是非空、不重复且不含空白的数组");
        if (bindings.Any(binding => binding.Key < 0 || binding.Key == 99999 || string.IsNullOrWhiteSpace(binding.Value))) throw new InvalidOperationException("雷电绑定必须使用有效实例编号与非空设备序列号");
        if (bindings.Values.Distinct(StringComparer.OrdinalIgnoreCase).Count() != bindings.Count) throw new InvalidOperationException("不同雷电实例不能绑定相同设备序列号");
        if (!string.IsNullOrWhiteSpace(AgentBaseUrl) && string.IsNullOrWhiteSpace(AgentSerial)) throw new InvalidOperationException("配置 Agent 地址时必须同时指定唯一设备 serial");
        if (!string.IsNullOrWhiteSpace(AgentBaseUrl) && !adbSerials.Contains(AgentSerial.Trim(), StringComparer.Ordinal)) throw new InvalidOperationException("Agent串号必须位于ADB授权串号数组中");
        return original with { AdbPath = AdbPath.Trim(), AdbAllowedSerials = adbSerials, ScrcpyPath = ScrcpyPath.Trim(), LdConsolePath = LdConsolePath.Trim(), LdSerialBindings = bindings, AgentBaseUrl = string.IsNullOrWhiteSpace(AgentBaseUrl) ? null : AgentBaseUrl.Trim(), AgentSerial = string.IsNullOrWhiteSpace(AgentSerial) ? null : AgentSerial.Trim(), BasicApiPort = BasicApiPort, AutomatorApiPort = AutomatorApiPort, AutoConnect = AutoConnect, AutoStartApi = AutoStartApi, Language = "zh-CN", MainMaxSize = MainMaxSize, MiniMaxSize = MiniMaxSize, MainMaxFps = MainMaxFps, MiniMaxFps = MiniMaxFps, MainBitRate = MainBitRate, MiniBitRate = MiniBitRate, ForceHardwareDecode = ForceHardwareDecode, ShowFps = ShowFps, WindowWidth = WindowWidth, WindowHeight = WindowHeight, FileRoot = FileRoot.Trim(), CommandTimeoutSeconds = CommandTimeoutSeconds };
    }
}

```

## src/MatrixDevice.Features/Applications/ApplicationService.cs

```csharp
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public sealed class ApplicationService(IDeviceBackend backend)
{
    public async Task<IReadOnlyList<AppInfo>> ListAsync(string serial, string? filter = null, bool showSystem = false, CancellationToken ct = default)
    {
        var applications = await backend.ListAppsAsync(serial, showSystem, true, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(filter)) return applications;
        return applications.Where(app => app.PackageName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            (app.Name?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false)).ToArray();
    }
    public Task<string> StartAsync(string serial, string packageName, string? activity = null, CancellationToken ct = default)
    {
        ScriptPackageService.ValidatePackageName(packageName);
        return backend.StartAppAsync(serial, packageName, activity, ct);
    }
    public Task<string> StopAsync(string serial, string packageName, CancellationToken ct = default)
    {
        ScriptPackageService.ValidatePackageName(packageName);
        return backend.StopAppAsync(serial, packageName, ct);
    }
    public Task InstallAsync(string serial, string path, CancellationToken ct = default)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("APK does not exist.", path);
        return backend.InstallAsync(serial, Path.GetFullPath(path), ct);
    }
    public async Task<IReadOnlyDictionary<string, bool>> ClearAsync(IReadOnlyList<string> serials, string packageName, bool confirmed, CancellationToken ct = default)
    {
        Confirm(serials, packageName, confirmed);
        var results = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var serial in serials.Distinct(StringComparer.Ordinal))
            results[serial] = await backend.ClearAppAsync(serial, packageName, ct).ConfigureAwait(false);
        return results;
    }
    public async Task UninstallAsync(IReadOnlyList<string> serials, string packageName, bool confirmed, CancellationToken ct = default)
    {
        Confirm(serials, packageName, confirmed);
        foreach (var serial in serials.Distinct(StringComparer.Ordinal))
            await backend.UninstallAsync(serial, packageName, ct).ConfigureAwait(false);
    }
    private static void Confirm(IReadOnlyList<string> serials, string packageName, bool confirmed)
    {
        ArgumentNullException.ThrowIfNull(serials);
        ScriptPackageService.ValidatePackageName(packageName);
        if (serials.Count == 0 || serials.Any(string.IsNullOrWhiteSpace)) throw new DeviceException("At least one explicit device is required.", 400, 1007);
        if (!confirmed) throw new DeviceException($"Confirmation is required for '{packageName}' on {serials.Count} device(s).", 409, 1003);
    }
}

```

## src/MatrixDevice.Features/Automation/AutomationService.cs

```csharp
using System.Diagnostics;
using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public sealed class AutomationService(IDeviceBackend backend, DeviceCatalog catalog)
{
    public static IReadOnlySet<string> Routes { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "device/dump", "device/screenshot", "device/shell", "device/execute", "device/set-prop",
        "device/set-timezone", "device/set-sim-info", "app/list", "app/list-packages", "app/start", "app/stop",
        "app/clear", "input/tap", "input/swipe", "input/keyevent", "service/info", "service/stop", "function/sleep",
        "function/sleep-ms", "element/find", "element/find-all", "element/wait-exists", "element/click-if-exists",
        "element/click-text", "element/set-text", "element/wait-any", "element/click-node"
    };
    public Func<byte[], int?, int?, int?, int?, int?, byte[]>? ScreenshotTransform { get; set; }
    public Func<string, CancellationToken, Task<object?>>? ServiceInfoProvider { get; set; }
    public Func<string, CancellationToken, Task>? ServiceStop { get; set; }

    public async Task<object?> ExecuteAsync(string route, JsonElement body, CancellationToken ct = default)
    {
        route = NormalizeRoute(route);
        if (!Routes.Contains(route)) throw new DeviceException($"Unknown Automator route: '{route}'.", 404, 1006);
        if (body.ValueKind != JsonValueKind.Object) throw Invalid("Request body must be a JSON object.");
        var serial = RequiredString(body, "serial");
        var device = catalog.GetRequired(serial);
        if (!device.Online) throw new DeviceException($"Device '{serial}' is not Ready: {device.State}.", 409, 1003);
        switch (route)
        {
            case "device/dump": return await backend.DumpAsync(serial, ct).ConfigureAwait(false);
            case "device/screenshot":
                var x = OptionalInt(body, "x"); var y = OptionalInt(body, "y");
                var width = OptionalInt(body, "width"); var height = OptionalInt(body, "height"); var quality = OptionalInt(body, "quality");
                if (x < 0 || y < 0 || width <= 0 || height <= 0 || quality < 0 || quality > 100) throw Invalid("Invalid screenshot crop or quality.");
                var png = await backend.ScreenshotAsync(serial, ct).ConfigureAwait(false);
                if (x is not null || y is not null || width is not null || height is not null || quality is not null)
                {
                    var dimensions = PngImage.ReadDimensions(png);
                    var left = x ?? 0; var top = y ?? 0;
                    var cropWidth = width ?? dimensions.Width - left;
                    var cropHeight = height ?? dimensions.Height - top;
                    if (left >= dimensions.Width || top >= dimensions.Height || cropWidth <= 0 || cropHeight <= 0 ||
                        (long)left + cropWidth > dimensions.Width || (long)top + cropHeight > dimensions.Height)
                        throw Invalid($"Screenshot crop ({left},{top},{cropWidth},{cropHeight}) is outside {dimensions.Width}x{dimensions.Height}.");
                    if (ScreenshotTransform is null) throw Unsupported("Screenshot cropping requires the registered Windows image adapter.");
                    png = ScreenshotTransform(png, x, y, width, height, quality);
                }
                return new { format = "png", base64 = Convert.ToBase64String(png) };
            case "device/shell": case "device/execute":
                return new { output = await backend.ShellAsync(serial, RequiredString(body, "command"), ct: ct).ConfigureAwait(false) };
            case "device/set-prop":
                var prop = RequiredString(body, "prop");
                if (prop.Length > 128 || prop.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '.' and not '_' and not '-'))
                    throw Invalid($"Invalid Android property name: '{prop}'.");
                var value = RequiredString(body, "value", allowEmpty: true);
                await backend.ShellAsync(serial, $"setprop {Quote(prop)} {Quote(value)}", true, ct).ConfigureAwait(false);
                var actual = (await backend.ShellAsync(serial, $"getprop {Quote(prop)}", ct: ct).ConfigureAwait(false)).TrimEnd('\r', '\n');
                if (!string.Equals(actual, value, StringComparison.Ordinal))
                    throw new DeviceException($"Property verification failed for '{prop}'.", 409, 1003);
                return new { value = true };
            case "device/set-timezone":
                var timezone = RequiredString(body, "timezone");
                try { TimeZoneInfo.FindSystemTimeZoneById(timezone); }
                catch (TimeZoneNotFoundException) { throw Invalid($"Unknown timezone: '{timezone}'."); }
                await backend.ShellAsync(serial, $"setprop persist.sys.timezone {Quote(timezone)}", true, ct).ConfigureAwait(false);
                var zone = (await backend.ShellAsync(serial, "getprop persist.sys.timezone", ct: ct).ConfigureAwait(false)).Trim();
                if (zone != timezone) throw new DeviceException($"Timezone verification failed: '{zone}'.", 409, 1003);
                return new { value = true };
            case "device/set-sim-info": throw Unsupported("SIM information requires the undocumented device ROM service.");
            case "app/list":
                return await backend.ListAppsAsync(serial, OptionalBool(body, "showSystem") ?? false, OptionalBool(body, "needVersion") ?? true, ct).ConfigureAwait(false);
            case "app/list-packages": return (await backend.ListAppsAsync(serial, true, false, ct).ConfigureAwait(false)).Select(app => app.PackageName).ToArray();
            case "app/start": return new { output = await backend.StartAppAsync(serial, Package(body), ct: ct).ConfigureAwait(false) };
            case "app/stop": return new { output = await backend.StopAppAsync(serial, Package(body), ct).ConfigureAwait(false) };
            case "app/clear": return new { value = await backend.ClearAppAsync(serial, Package(body), ct).ConfigureAwait(false) };
            case "input/tap":
                await backend.TapAsync(serial, RequiredInt(body, "x"), RequiredInt(body, "y"), ct).ConfigureAwait(false);
                return new { value = true };
            case "input/swipe":
                var steps = OptionalInt(body, "steps") ?? 55; var stepMs = OptionalInt(body, "stepMs") ?? 5;
                if (steps <= 0 || stepMs <= 0 || (long)steps * stepMs > 60000) throw Invalid($"Invalid swipe timing: steps={steps}, stepMs={stepMs}; duration must be in 1..60000ms.");
                await backend.SwipeAsync(serial, RequiredInt(body, "x1"), RequiredInt(body, "y1"), RequiredInt(body, "x2"), RequiredInt(body, "y2"), checked(steps * stepMs), ct).ConfigureAwait(false);
                return new { value = true };
            case "input/keyevent":
                await backend.KeyEventAsync(serial, RequiredInt(body, "code"), ct).ConfigureAwait(false);
                return new { value = true };
            case "service/info":
                if (ServiceInfoProvider is not null) return await ServiceInfoProvider(serial, ct).ConfigureAwait(false);
                return new { serial, state = device.State, backend = backend.GetType().Name, agentServiceAvailable = false };
            case "service/stop":
                if (ServiceStop is null) throw Unsupported("The backend has no private Automator agent lifecycle adapter.");
                await ServiceStop(serial, ct).ConfigureAwait(false); return new { value = true };
            case "function/sleep":
                var seconds = OptionalDouble(body, "seconds") ?? throw Invalid("seconds is required; the original nullable default was not recovered.");
                if (seconds < 0 || seconds > 600) throw Invalid($"seconds must be in 0..600: {seconds}.");
                await Task.Delay(TimeSpan.FromSeconds(seconds), ct).ConfigureAwait(false); return new { value = true };
            case "function/sleep-ms":
                var milliseconds = OptionalInt(body, "milliseconds") ?? throw Invalid("milliseconds is required; the original nullable default was not recovered.");
                if (milliseconds < 0 || milliseconds > 600000) throw Invalid($"milliseconds must be in 0..600000: {milliseconds}.");
                await Task.Delay(milliseconds, ct).ConfigureAwait(false); return new { value = true };
            case "element/find": return NodeSearch.Find(await backend.DumpAsync(serial, ct).ConfigureAwait(false), Selector(body));
            case "element/find-all": return NodeSearch.FindAll(await backend.DumpAsync(serial, ct).ConfigureAwait(false), Selector(body));
            case "element/wait-exists": return new { value = await WaitNodeAsync(serial, Selector(body), Timeout(body), Poll(body), ct).ConfigureAwait(false) is not null };
            case "element/click-if-exists":
                var found = await WaitNodeAsync(serial, Selector(body), Timeout(body), Poll(body), ct).ConfigureAwait(false);
                if (found is null) return new { value = false };
                await ClickNodeAsync(serial, found, ct).ConfigureAwait(false); return new { value = true };
            case "element/click-text":
                var textNode = await WaitNodeAsync(serial, new NodeSelector { Text = RequiredString(body, "text", true) }, Timeout(body), Poll(body), ct).ConfigureAwait(false);
                if (textNode is null) return new { value = false };
                await ClickNodeAsync(serial, textNode, ct).ConfigureAwait(false); return new { value = true };
            case "element/set-text":
                await backend.SetNodeTextAsync(serial, Selector(body), RequiredString(body, "text", true), ct).ConfigureAwait(false); return new { value = true };
            case "element/wait-any":
                var selectorsElement = RequiredProperty(body, "selectors");
                if (selectorsElement.ValueKind != JsonValueKind.Array || selectorsElement.GetArrayLength() is 0 or > 256)
                    throw Invalid("selectors must be an array containing 1..256 selectors.");
                var selectors = selectorsElement.EnumerateArray().Select(Deserialize<NodeSelector>).ToArray();
                return await WaitAnyAsync(serial, selectors, Timeout(body), Poll(body), ct).ConfigureAwait(false);
            case "element/click-node":
                await ClickNodeAsync(serial, Deserialize<AndroidNode>(RequiredProperty(body, "node")), ct).ConfigureAwait(false); return new { value = true };
            default: throw new UnreachableException();
        }
    }

    public async Task<AndroidNode?> WaitNodeAsync(string serial, NodeSelector selector, int timeoutMs = 3000, int pollMs = 200, CancellationToken ct = default)
    {
        ValidatePolling(timeoutMs, pollMs);
        var timer = Stopwatch.StartNew();
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var node = NodeSearch.Find(await backend.DumpAsync(serial, ct).ConfigureAwait(false), selector);
            if (node is not null) return node;
            if (timer.ElapsedMilliseconds >= timeoutMs) return null;
            var remaining = timeoutMs - timer.ElapsedMilliseconds;
            if (remaining <= 0) return null;
            await Task.Delay((int)Math.Min(pollMs, remaining), ct).ConfigureAwait(false);
        }
    }

    public async Task<object?> WaitAnyAsync(string serial, IReadOnlyList<NodeSelector> selectors, int timeoutMs = 3000, int pollMs = 200, CancellationToken ct = default)
    {
        if (selectors.Count == 0 || selectors.Any(selector => selector is null)) throw Invalid("selectors must be nonempty and contain no null.");
        ValidatePolling(timeoutMs, pollMs);
        var timer = Stopwatch.StartNew();
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            for (var index = 0; index < selectors.Count; index++)
            {
                var node = NodeSearch.Find(await backend.DumpAsync(serial, ct).ConfigureAwait(false), selectors[index]);
                if (node is not null) return new { index, selector = selectors[index], node };
            }
            if (timer.ElapsedMilliseconds >= timeoutMs) return null;
            var remaining = timeoutMs - timer.ElapsedMilliseconds;
            if (remaining <= 0) return null;
            await Task.Delay((int)Math.Min(pollMs, remaining), ct).ConfigureAwait(false);
        }
    }

    public Task ClickNodeAsync(string serial, AndroidNode node, CancellationToken ct = default)
    {
        if (node.Bounds.Width <= 0 || node.Bounds.Height <= 0) throw Invalid("Node bounds must have positive width and height.");
        return backend.TapAsync(serial, node.Bounds.CenterX, node.Bounds.CenterY, ct);
    }

    private static void ValidatePolling(int timeout, int poll)
    {
        if (timeout < 0 || timeout > 600000 || poll <= 0 || poll > 60000) throw Invalid($"Invalid polling: timeoutMs={timeout}, pollMs={poll}.");
    }
    private static int Timeout(JsonElement body) => OptionalInt(body, "timeoutMs") ?? 3000;
    private static int Poll(JsonElement body) => OptionalInt(body, "pollMs") ?? 200;
    private static string Package(JsonElement body) { var name = RequiredString(body, "packageName"); ScriptPackageService.ValidatePackageName(name); return name; }
    private static NodeSelector Selector(JsonElement body) => Deserialize<NodeSelector>(RequiredProperty(body, "selector"));
    public static string NormalizeRoute(string route)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(route);
        route = route.Trim().Trim('/');
        if (route.StartsWith("api/", StringComparison.OrdinalIgnoreCase)) route = route[4..];
        return route.ToLowerInvariant();
    }
    public static string Quote(string value) => "'" + value.Replace("'", "'\"'\"'", StringComparison.Ordinal) + "'";
    private static JsonElement RequiredProperty(JsonElement body, string name) => Property(body, name) ?? throw Invalid($"Missing property: {name}.");
    private static JsonElement? Property(JsonElement body, string name)
    {
        JsonElement? found = null;
        foreach (var property in body.EnumerateObject())
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                if (found is not null) throw Invalid($"Duplicate property: {name}.");
                found = property.Value;
            }
        return found;
    }
    private static string RequiredString(JsonElement body, string name, bool allowEmpty = false)
    {
        var value = RequiredProperty(body, name);
        if (value.ValueKind != JsonValueKind.String) throw Invalid($"Property {name} must be a string.");
        var text = value.GetString()!;
        if (!allowEmpty && string.IsNullOrWhiteSpace(text)) throw Invalid($"Property {name} must not be blank.");
        return text;
    }
    private static int RequiredInt(JsonElement body, string name) => OptionalInt(body, name) ?? throw Invalid($"Missing integer property: {name}.");
    private static int? OptionalInt(JsonElement body, string name)
    {
        var value = Property(body, name);
        if (value is null || value.Value.ValueKind == JsonValueKind.Null) return null;
        if (value.Value.ValueKind != JsonValueKind.Number || !value.Value.TryGetInt32(out var result)) throw Invalid($"Property {name} must be an Int32.");
        return result;
    }
    private static double? OptionalDouble(JsonElement body, string name)
    {
        var value = Property(body, name);
        if (value is null || value.Value.ValueKind == JsonValueKind.Null) return null;
        if (value.Value.ValueKind != JsonValueKind.Number || !value.Value.TryGetDouble(out var result) || !double.IsFinite(result)) throw Invalid($"Property {name} must be a finite number.");
        return result;
    }
    private static bool? OptionalBool(JsonElement body, string name)
    {
        var value = Property(body, name);
        if (value is null || value.Value.ValueKind == JsonValueKind.Null) return null;
        if (value.Value.ValueKind is not JsonValueKind.True and not JsonValueKind.False) throw Invalid($"Property {name} must be boolean.");
        return value.Value.GetBoolean();
    }
    private static T Deserialize<T>(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object) throw Invalid($"{typeof(T).Name} must be an object.");
        try { return element.Deserialize<T>(JsonDefaults.Options) ?? throw Invalid($"{typeof(T).Name} must not be null."); }
        catch (JsonException error) { throw Invalid($"Invalid {typeof(T).Name}: {error.Message}"); }
    }
    private static DeviceException Invalid(string message) => new(message, 400, 1007);
    private static DeviceException Unsupported(string message) => new(message, 501, 2001);
}

```

## src/MatrixDevice.Features/Automation/NodeSearch.cs

```csharp
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public static class NodeSearch
{
    public static IReadOnlyList<AndroidNode> FindAll(AndroidNode root, NodeSelector selector)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(selector);
        var found = new List<AndroidNode>();
        var pending = new Stack<AndroidNode>();
        pending.Push(root);
        var visited = new HashSet<AndroidNode>(ReferenceEqualityComparer.Instance);
        while (pending.TryPop(out var node))
        {
            if (!visited.Add(node)) throw new DeviceException("Node tree contains a repeated reference or cycle.", 400, 1007);
            if (visited.Count > 100000) throw new DeviceException("Node tree exceeds 100000 nodes.", 400, 1007);
            if (Matches(node, selector)) found.Add(node);
            for (var index = node.ChildNodeList.Count - 1; index >= 0; index--)
                if (node.ChildNodeList[index] is { } child) pending.Push(child);
        }
        return found;
    }

    public static AndroidNode? Find(AndroidNode root, NodeSelector selector) => FindAll(root, selector).FirstOrDefault();

    public static bool Matches(AndroidNode node, NodeSelector selector) =>
        StringMatch(node.Text, selector.Text) &&
        (selector.TextContains is null || (node.Text?.Contains(selector.TextContains, StringComparison.Ordinal) ?? false)) &&
        StringMatch(node.ResourceId, selector.ResourceId) && StringMatch(node.ClassName, selector.ClassName) &&
        StringMatch(node.PackageName, selector.PackageName) &&
        Match(node.Index, selector.Index) && Match(node.Instance, selector.Instance) &&
        Match(node.Clickable, selector.Clickable) && Match(node.Enabled, selector.Enabled) &&
        Match(node.Focusable, selector.Focusable) && Match(node.Focused, selector.Focused) &&
        Match(node.Scrollable, selector.Scrollable) && Match(node.LongClickable, selector.LongClickable) &&
        Match(node.Checkable, selector.Checkable) && Match(node.Checked, selector.Checked) &&
        Match(node.Selected, selector.Selected) && Match(node.VisibleToUser, selector.VisibleToUser);

    private static bool StringMatch(string? actual, string? expected) => expected is null || string.Equals(actual, expected, StringComparison.Ordinal);
    private static bool Match<T>(T actual, T? expected) where T : struct => expected is null || EqualityComparer<T>.Default.Equals(actual, expected.Value);
}

```

## src/MatrixDevice.Features/Clipboard/ClipboardService.cs

```csharp
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public sealed class ClipboardService(IDeviceBackend backend)
{
    public Task SendTextAsync(string serial, string text, CancellationToken ct = default) => backend.TextAsync(serial, text, ct);
    public Task PushAsync(string serial, string text, CancellationToken ct = default) =>
        throw new DeviceException("Private JiXingKeyboard clipboard protocol was not recovered; sending input text is available separately.", 501, 2001);
    public Task<string> PullAsync(string serial, CancellationToken ct = default) =>
        throw new DeviceException("Reading the device clipboard requires an undocumented device-side service.", 501, 2001);
    public Task InstallKeyboardAsync(string serial, string apkPath, CancellationToken ct = default)
    {
        if (!File.Exists(apkPath)) throw new FileNotFoundException("A caller-provided compatible keyboard APK is required.", apkPath);
        return backend.InstallAsync(serial, apkPath, ct);
    }
}

```

## src/MatrixDevice.Features/Debug/DebugWorkspace.cs

```csharp
using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public sealed record NodeOverlay(AndroidNode SourceNode, int Depth, int RenderOrder)
{
    public NodeBounds Bounds => SourceNode.Bounds;
    public bool IsSelected { get; internal set; }
}
public sealed record ShellHistoryEntry(int Index, DateTimeOffset Time, string CommandText, string OutputText, string? Error = null);
public sealed record DebugRefreshResult(string Serial, byte[]? Screenshot, AndroidNode? Root, string? ScreenshotError, string? DumpError);

public sealed class DebugWorkspace(IDeviceBackend backend)
{
    private readonly SemaphoreSlim refreshGate = new(1, 1);
    private readonly SemaphoreSlim shellGate = new(1, 1);
    private readonly object gate = new();
    private List<NodeOverlay> overlays = [];
    private readonly List<ShellHistoryEntry> history = [];
    private byte[]? screenshot;
    private AndroidNode? root;
    private string? serial;
    private AndroidNode? selectedNode;
    public bool IsLoading => refreshGate.CurrentCount == 0;
    public bool IsExecuting => shellGate.CurrentCount == 0;
    public string? Serial { get { lock (gate) return serial; } }
    public byte[]? Screenshot { get { lock (gate) return screenshot?.ToArray(); } }
    public AndroidNode? Root { get { lock (gate) return root; } }
    public AndroidNode? SelectedNode { get { lock (gate) return selectedNode; } }
    public IReadOnlyList<NodeOverlay> Overlays { get { lock (gate) return overlays.ToArray(); } }
    public IReadOnlyList<ShellHistoryEntry> ShellHistory { get { lock (gate) return history.ToArray(); } }
    public event Action<AndroidNode?>? SelectionChanged;

    public async Task<DebugRefreshResult> RefreshAsync(string deviceSerial, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceSerial);
        if (!await refreshGate.WaitAsync(0, ct).ConfigureAwait(false)) throw new DeviceException("A debug refresh is already in progress.");
        try
        {
            byte[]? image = null; AndroidNode? tree = null;
            string? imageError = null; string? treeError = null;
            try { image = await backend.ScreenshotAsync(deviceSerial, ct).ConfigureAwait(false); }
            catch (Exception error) when (error is not OperationCanceledException) { imageError = error.Message; }
            try { tree = await backend.DumpAsync(deviceSerial, ct).ConfigureAwait(false); }
            catch (Exception error) when (error is not OperationCanceledException) { treeError = error.Message; }
            var rectangles = tree is null ? [] : BuildOverlays(tree);
            lock (gate)
            {
                serial = deviceSerial;
                screenshot = image;
                root = tree;
                overlays = rectangles;
                selectedNode = null;
            }
            SelectionChanged?.Invoke(null);
            return new(deviceSerial, image, tree, imageError, treeError);
        }
        finally { refreshGate.Release(); }
    }

    public void SelectNode(AndroidNode? node)
    {
        lock (gate)
        {
            if (node is not null && !overlays.Any(overlay => ReferenceEquals(overlay.SourceNode, node)))
                throw new DeviceException("The selected node does not belong to the current debug snapshot.", 400, 1007);
            selectedNode = node;
            foreach (var overlay in overlays) overlay.IsSelected = ReferenceEquals(overlay.SourceNode, node);
        }
        SelectionChanged?.Invoke(node);
    }

    public void SelectOverlay(NodeOverlay overlay) => SelectNode(overlay.SourceNode);

    public async Task<ShellHistoryEntry> ExecuteShellAsync(string deviceSerial, string command, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceSerial);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        if (!await shellGate.WaitAsync(0, ct).ConfigureAwait(false)) throw new DeviceException("A Shell command is already in progress.");
        var text = command.Trim();
        try
        {
            var output = await backend.ShellAsync(deviceSerial, text, ct: ct).ConfigureAwait(false);
            lock (gate)
            {
                var entry = new ShellHistoryEntry(history.Count + 1, DateTimeOffset.UtcNow, text, output);
                history.Add(entry);
                return entry;
            }
        }
        catch (Exception error)
        {
            lock (gate) history.Add(new(history.Count + 1, DateTimeOffset.UtcNow, text, "", error.Message));
            throw;
        }
        finally { shellGate.Release(); }
    }

    public string ExportStructure()
    {
        lock (gate)
            return JsonSerializer.Serialize(root ?? throw new DeviceException("There is no node snapshot to export.", 400, 1007), JsonDefaults.Options);
    }

    public void ExportStructure(string path) => File.WriteAllText(path, ExportStructure(), new System.Text.UTF8Encoding(false));

    public byte[] CropScreenshot(int x, int y, int width, int height)
    {
        var image = Screenshot ?? throw new DeviceException("There is no screenshot to crop.", 400, 1007);
        return PngImage.Crop(image, x, y, width, height);
    }

    public (byte R, byte G, byte B, string Hex) ColorAt(int x, int y)
    {
        var image = Screenshot ?? throw new DeviceException("There is no screenshot for color selection.", 400, 1007);
        var color = PngImage.ColorAt(image, x, y);
        return (color.R, color.G, color.B, $"#{color.R:X2}{color.G:X2}{color.B:X2}");
    }

    private static List<NodeOverlay> BuildOverlays(AndroidNode root)
    {
        var result = new List<NodeOverlay>();
        var pending = new Stack<(AndroidNode Node, int Depth)>();
        var visited = new HashSet<AndroidNode>(ReferenceEqualityComparer.Instance);
        pending.Push((root, 0));
        while (pending.TryPop(out var item))
        {
            if (!visited.Add(item.Node)) throw new DeviceException("Node tree contains a cycle or repeated node.", 400, 1007);
            if (visited.Count > 100000) throw new DeviceException("Node snapshot is too large.", 400, 1007);
            result.Add(new(item.Node, item.Depth, result.Count));
            for (var index = item.Node.ChildNodeList.Count - 1; index >= 0; index--)
                if (item.Node.ChildNodeList[index] is { } child) pending.Push((child, item.Depth + 1));
        }
        return result;
    }
}

```

## src/MatrixDevice.Features/Extensions/ApplicationBackupService.cs

```csharp
using System.Formats.Tar;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public sealed record AppBackupRecord(Guid Id, string Serial, string PackageName, string ArchivePath, DateTimeOffset CreatedUtc, string? Notes = null);
public sealed record BackupProgress(double Percent, string Stage);
public sealed record AppBackupOptions
{
    public bool RequireRoot { get; init; } = true;
    public bool ValidateImportedArchive { get; init; } = true;
    public bool RestoreGoogleAccountManagerData { get; init; } = true;
    public string ArchiveExtension { get; init; } = ".tar.gz";
}

public sealed class ApplicationBackupService(IDeviceBackend backend, string storageDirectory)
{
    private readonly string directory = Path.GetFullPath(storageDirectory);
    private readonly SemaphoreSlim operationGate = new(1, 1);
    public const long MaximumArchiveBytes = 1024L * 1024 * 1024;
    public const long MaximumUncompressedBytes = 4L * 1024 * 1024 * 1024;

    public async Task<AppBackupRecord> BackupAsync(string serial, string packageName, IProgress<BackupProgress>? progress = null, CancellationToken ct = default)
    {
        ScriptPackageService.ValidatePackageName(packageName);
        await operationGate.WaitAsync(ct).ConfigureAwait(false);
        var id = Guid.NewGuid();
        var remote = "/data/local/tmp/matrix-backup-" + id.ToString("N") + ".tar.gz";
        var local = Path.Combine(directory, id.ToString("N") + ".tar.gz");
        try
        {
            Directory.CreateDirectory(directory);
            progress?.Report(new(0, "Checking root and application paths"));
            await EnsureRootAsync(serial, ct).ConfigureAwait(false);
            var privatePath = await ResolvePrivatePathAsync(serial, packageName, ct).ConfigureAwait(false);
            await backend.ShellAsync(serial, "command -v tar", true, ct).ConfigureAwait(false);
            progress?.Report(new(20, "Creating device archive"));
            await backend.ShellAsync(serial, $"tar -czf {Quote(remote)} -C / {Quote(privatePath.TrimStart('/'))}", true, ct).ConfigureAwait(false);
            await backend.ShellAsync(serial, $"chmod 0644 {Quote(remote)} && test -s {Quote(remote)}", true, ct).ConfigureAwait(false);
            progress?.Report(new(50, "Downloading archive"));
            await backend.PullAsync(serial, remote, local, ct: ct).ConfigureAwait(false);
            ValidateImportedArchive(local, packageName);
            var record = new AppBackupRecord(id, serial, packageName, local, DateTimeOffset.UtcNow);
            SaveRecord(record);
            progress?.Report(new(100, "Backup completed"));
            return record;
        }
        catch
        {
            if (File.Exists(local)) File.Delete(local);
            throw;
        }
        finally
        {
            try { await backend.ShellAsync(serial, $"rm -f -- {Quote(remote)}", true, CancellationToken.None).ConfigureAwait(false); }
            catch (Exception error) { System.Diagnostics.Trace.WriteLine($"Backup remote cleanup failed: {error.Message}"); }
            operationGate.Release();
        }
    }

    public async Task RestoreAsync(string serial, AppBackupRecord record, bool confirmed, AppBackupOptions? options = null,
        IProgress<BackupProgress>? progress = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        ScriptPackageService.ValidatePackageName(record.PackageName);
        if (!confirmed) throw new DeviceException($"Restore clears '{record.PackageName}' on device '{serial}'; explicit confirmation is required.", 409, 1003);
        options ??= new AppBackupOptions();
        if (options.ArchiveExtension != ".tar.gz") throw new DeviceException($"Unsupported backup extension: '{options.ArchiveExtension}'.", 501, 2001);
        if (options.RestoreGoogleAccountManagerData)
            throw new DeviceException("Account-manager archive semantics were not recovered; set RestoreGoogleAccountManagerData=false for application-only restore.", 501, 2001);
        ValidateImportedArchive(record.ArchivePath, record.PackageName);
        await operationGate.WaitAsync(ct).ConfigureAwait(false);
        var remote = "/data/local/tmp/matrix-restore-" + Guid.NewGuid().ToString("N") + ".tar.gz";
        try
        {
            progress?.Report(new(0, "Checking restore prerequisites"));
            await EnsureRootAsync(serial, ct).ConfigureAwait(false);
            var privatePath = await ResolvePrivatePathAsync(serial, record.PackageName, ct).ConfigureAwait(false);
            var uidText = (await backend.ShellAsync(serial, $"stat -c %u {Quote(privatePath)}", true, ct).ConfigureAwait(false)).Trim();
            if (!int.TryParse(uidText, NumberStyles.None, CultureInfo.InvariantCulture, out var uid) || uid <= 0)
                throw new DeviceException($"Cannot resolve application owner UID: '{uidText}'.", 409, 1003);
            await backend.ShellAsync(serial, "command -v tar && command -v restorecon && command -v chown", true, ct).ConfigureAwait(false);
            await backend.PushAsync(serial, record.ArchivePath, remote, ct: ct).ConfigureAwait(false);
            progress?.Report(new(30, "Stopping and clearing application"));
            await backend.StopAppAsync(serial, record.PackageName, ct).ConfigureAwait(false);
            if (!await backend.ClearAppAsync(serial, record.PackageName, ct).ConfigureAwait(false))
                throw new DeviceException($"Clear data failed for '{record.PackageName}'; archive was not extracted.", 409, 1003);
            progress?.Report(new(60, "Extracting archive"));
            await backend.ShellAsync(serial, $"tar -xzf {Quote(remote)} -C /", true, ct).ConfigureAwait(false);
            await backend.ShellAsync(serial, $"chown -R {uid}:{uid} {Quote(privatePath)} && restorecon -RF {Quote(privatePath)}", true, ct).ConfigureAwait(false);
            progress?.Report(new(100, "Restore completed"));
        }
        finally
        {
            try { await backend.ShellAsync(serial, $"rm -f -- {Quote(remote)}", true, CancellationToken.None).ConfigureAwait(false); }
            catch (Exception error) { System.Diagnostics.Trace.WriteLine($"Restore remote cleanup failed: {error.Message}"); }
            operationGate.Release();
        }
    }

    public AppBackupRecord Import(string path, string packageName, string? notes = null)
    {
        ValidateImportedArchive(path, packageName);
        Directory.CreateDirectory(directory);
        var id = Guid.NewGuid();
        var destination = Path.Combine(directory, id.ToString("N") + ".tar.gz");
        File.Copy(path, destination);
        var record = new AppBackupRecord(id, "", packageName, destination, DateTimeOffset.UtcNow, notes);
        try { SaveRecord(record); }
        catch { File.Delete(destination); throw; }
        return record;
    }

    public IReadOnlyList<AppBackupRecord> List()
    {
        if (!Directory.Exists(directory)) return [];
        return Directory.EnumerateFiles(directory, "*.json", SearchOption.TopDirectoryOnly)
            .Select(path => JsonSerializer.Deserialize<AppBackupRecord>(File.ReadAllText(path), JsonDefaults.Options)
                ?? throw new InvalidDataException($"Null backup metadata: {path}"))
            .OrderByDescending(record => record.CreatedUtc).ToArray();
    }

    public void Export(AppBackupRecord record, string destination)
    {
        ArgumentNullException.ThrowIfNull(record);
        ValidateImportedArchive(record.ArchivePath, record.PackageName);
        var path = Path.GetFullPath(destination);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.Copy(record.ArchivePath, path, false);
    }

    public void Delete(AppBackupRecord record, bool confirmed)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (!confirmed) throw new DeviceException("Backup deletion requires confirmation.", 409, 1003);
        var expected = Path.Combine(directory, record.Id.ToString("N") + ".tar.gz");
        if (!Path.GetFullPath(record.ArchivePath).Equals(expected, StringComparison.OrdinalIgnoreCase))
            throw new DeviceException("Only a managed backup archive can be deleted.", 400, 1007);
        File.Delete(expected);
        File.Delete(Path.Combine(directory, record.Id.ToString("N") + ".json"));
    }

    public static void ValidateImportedArchive(string path, string packageName)
    {
        ScriptPackageService.ValidatePackageName(packageName);
        using var file = File.OpenRead(path);
        if (file.Length is <= 0 or > MaximumArchiveBytes) throw Invalid($"Archive size is outside 1..{MaximumArchiveBytes}: {file.Length}.");
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var bounded = new LimitedReadStream(gzip, MaximumUncompressedBytes);
        using var reader = new TarReader(bounded);
        var count = 0;
        long total = 0;
        var names = new HashSet<string>(StringComparer.Ordinal);
        TarEntry? entry;
        while ((entry = reader.GetNextEntry()) is not null)
        {
            if (++count > 100000) throw Invalid("Archive contains more than 100000 entries.");
            var name = entry.Name.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(name) || name.StartsWith('/') || name.Contains('\\') || name.Contains(':') ||
                name.Split('/').Any(component => component is ".." or ".") || name.IndexOfAny(['\r', '\n', '\0']) >= 0)
                throw Invalid($"Unsafe archive path: '{name}'.");
            if (!names.Add(name)) throw Invalid($"Duplicate archive entry: '{name}'.");
            var allowed = new[] { "data/data/" + packageName, "data/user/0/" + packageName };
            if (!allowed.Any(prefix => name == prefix || name.StartsWith(prefix + "/", StringComparison.Ordinal)))
                throw Invalid($"Archive path is outside application '{packageName}': '{name}'.");
            if (entry.EntryType is not TarEntryType.Directory and not TarEntryType.RegularFile and not TarEntryType.V7RegularFile)
                throw Invalid($"Unsupported archive entry type at '{name}': {entry.EntryType}.");
            total = checked(total + entry.Length);
            if (entry.Length < 0 || total > MaximumUncompressedBytes) throw Invalid("Uncompressed archive content exceeds its size limit.");
        }
        if (count == 0) throw Invalid("Archive is empty.");
        bounded.CopyTo(Stream.Null);
    }

    private void SaveRecord(AppBackupRecord record)
    {
        var path = Path.Combine(directory, record.Id.ToString("N") + ".json");
        var temporary = path + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(record, JsonDefaults.Options), new System.Text.UTF8Encoding(false));
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    private async Task EnsureRootAsync(string serial, CancellationToken ct)
    {
        if ((await backend.ShellAsync(serial, "id -u", true, ct).ConfigureAwait(false)).Trim() != "0")
            throw new DeviceException($"Root is required for application backup on '{serial}'.", 409, 1003);
    }
    private async Task<string> ResolvePrivatePathAsync(string serial, string packageName, CancellationToken ct)
    {
        var primary = "/data/user/0/" + packageName; var fallback = "/data/data/" + packageName;
        var output = (await backend.ShellAsync(serial,
            $"if [ -d {Quote(primary)} ]; then printf '%s' {Quote(primary)}; elif [ -d {Quote(fallback)} ]; then printf '%s' {Quote(fallback)}; else exit 1; fi", true, ct).ConfigureAwait(false)).Trim();
        if (output != primary && output != fallback) throw new DeviceException($"Application data path is unavailable for '{packageName}'.", 409, 1003);
        return output;
    }
    private static string Quote(string value) => AutomationService.Quote(value);
    private static DeviceException Invalid(string message) => new(message, 400, 1007);

    private sealed class LimitedReadStream(Stream source, long limit) : Stream
    {
        private long consumed;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => consumed; set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override int Read(Span<byte> buffer)
        {
            var count = source.Read(buffer);
            consumed = checked(consumed + count);
            if (consumed > limit) throw Invalid("Uncompressed archive stream exceeds its read limit.");
            return count;
        }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}

```

## src/MatrixDevice.Features/Extensions/ProxyService.cs

```csharp
using System.Text.Json;
using System.Web;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public enum ProxyType { Socks5, Vless, Hysteria2 }
public sealed record ProxyProfile
{
    public ProxyType ProxyType { get; init; }
    public required string Host { get; init; }
    public required int Port { get; init; }
    public string? RawUrl { get; init; }
    public string? User { get; init; }
    public string? Password { get; init; }
    public string? Area { get; init; }
    public string? Sni { get; init; }
    public bool Insecure { get; init; }
    public override string ToString() => $"{ProxyType}://{Host}:{Port}";
}

public static class ProxyProfileParser
{
    public static ProxyProfile Parse(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || string.IsNullOrWhiteSpace(uri.Host) || uri.Port is < 1 or > 65535)
            throw Invalid("Proxy URL requires a host and an explicit port in 1..65535.");
        var type = uri.Scheme.ToLowerInvariant() switch
        {
            "socks5" or "socks" => ProxyType.Socks5,
            "vless" => ProxyType.Vless,
            "hy2" or "hysteria2" => ProxyType.Hysteria2,
            _ => throw Invalid($"Unsupported proxy scheme: '{uri.Scheme}'.")
        };
        var credentials = uri.UserInfo.Split(':', 2).Select(Uri.UnescapeDataString).ToArray();
        var query = HttpUtility.ParseQueryString(uri.Query);
        string? user = null; string? password = null;
        switch (type)
        {
            case ProxyType.Socks5:
                user = credentials[0].Length == 0 ? null : credentials[0];
                password = credentials.Length == 2 ? credentials[1] : null;
                break;
            case ProxyType.Vless:
                user = credentials[0];
                if (!Guid.TryParse(user, out _)) throw Invalid("VLESS user information must contain a UUID.");
                break;
            case ProxyType.Hysteria2:
                password = credentials[0];
                if (string.IsNullOrWhiteSpace(password)) throw Invalid("Hysteria2 user information must contain a password.");
                break;
        }
        var insecureText = query["insecure"];
        if (insecureText is not null && insecureText is not "0" and not "1" && !bool.TryParse(insecureText, out _))
            throw Invalid("insecure must be 0, 1, true, or false.");
        return new ProxyProfile
        {
            ProxyType = type, Host = uri.IdnHost, Port = uri.Port, RawUrl = url,
            User = user, Password = password, Area = query["area"], Sni = query["sni"],
            Insecure = insecureText == "1" || bool.TryParse(insecureText, out var insecure) && insecure
        };
    }

    public static string ExportProfileJson(ProxyProfile profile, bool includeCredentials = false)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (profile.Port is < 1 or > 65535 || string.IsNullOrWhiteSpace(profile.Host)) throw Invalid("Invalid profile host or port.");
        if (!includeCredentials) profile = profile with { User = null, Password = null, RawUrl = profile.ToString() };
        return JsonSerializer.Serialize(profile, JsonDefaults.Options);
    }

    // JSON is a structured YAML 1.2 representation; this is a profile, not a recovered runner configuration.
    public static string ExportProfileYaml(ProxyProfile profile, bool includeCredentials = false) => ExportProfileJson(profile, includeCredentials);
    private static DeviceException Invalid(string message) => new(message, 400, 1007);
}

public sealed class ProxyService(IDeviceBackend backend)
{
    public async Task EnsureRootAsync(string serial, CancellationToken ct = default)
    {
        var uid = (await backend.ShellAsync(serial, "id -u", true, ct).ConfigureAwait(false)).Trim();
        if (uid != "0") throw new DeviceException($"Root is unavailable for device '{serial}'.", 409, 1003);
    }
    public Task StartAsync(string serial, ProxyProfile profile, CancellationToken ct = default) =>
        throw new DeviceException("Proxy runner binaries and device routing rules were not recovered; profile parsing and export are available.", 501, 2001);
    public Task StopAsync(string serial, CancellationToken ct = default) =>
        throw new DeviceException("No proxy runner owned by this application is deployed.", 501, 2001);
}

public sealed record EnvironmentConfiguration
{
    public string? Country { get; init; }
    public string? Language { get; init; }
    public string? Timezone { get; init; }
    public string? OperatorName { get; init; }
    public string? Mcc { get; init; }
    public string? Mnc { get; init; }
    public string? Iccid { get; init; }
    public string? Imsi { get; init; }
    public string? PhoneNumber { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? KeyboxXml { get; init; }
    public string? PifJson { get; init; }
}

public sealed class EnvironmentConfigurationService(IDeviceBackend backend)
{
    public async Task ApplyAsync(string serial, EnvironmentConfiguration configuration, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (configuration.Country is not null || configuration.Language is not null || configuration.OperatorName is not null ||
            configuration.Mcc is not null || configuration.Mnc is not null || configuration.Iccid is not null || configuration.Imsi is not null ||
            configuration.PhoneNumber is not null || configuration.Latitude is not null || configuration.Longitude is not null ||
            configuration.KeyboxXml is not null || configuration.PifJson is not null)
            throw new DeviceException("These environment fields require undocumented ROM APIs; no fields were applied.", 501, 2001);
        if (string.IsNullOrWhiteSpace(configuration.Timezone)) throw new DeviceException("A timezone is required.", 400, 1007);
        try { TimeZoneInfo.FindSystemTimeZoneById(configuration.Timezone); }
        catch (TimeZoneNotFoundException) { throw new DeviceException($"Unknown timezone: '{configuration.Timezone}'.", 400, 1007); }
        await backend.ShellAsync(serial, "setprop persist.sys.timezone " + AutomationService.Quote(configuration.Timezone), true, ct).ConfigureAwait(false);
        var actual = (await backend.ShellAsync(serial, "getprop persist.sys.timezone", ct: ct).ConfigureAwait(false)).Trim();
        if (actual != configuration.Timezone) throw new DeviceException($"Timezone verification failed: '{actual}'.", 409, 1003);
    }
}

```

## src/MatrixDevice.Features/Files/TransferQueue.cs

```csharp
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public enum TransferDirection { Push, Pull }
public enum TransferState { Pending, Transferring, Installing, Completed, Failed, Cancelled }
public sealed record TransferItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Serial { get; init; }
    public required string LocalPath { get; init; }
    public required string RemotePath { get; init; }
    public TransferDirection Direction { get; init; }
    public bool AutoInstall { get; init; }
    public bool IsApk => LocalPath.EndsWith(".apk", StringComparison.OrdinalIgnoreCase);
    public string FileName => Path.GetFileName(LocalPath);
    public TransferState State { get; internal set; }
    public double ProgressPercent { get; internal set; }
    public string? Error { get; internal set; }
}

public sealed class TransferQueue(IDeviceBackend backend) : IAsyncDisposable
{
    private sealed class DirectProgress(Action<double> report) : IProgress<double>
    { public void Report(double value) => report(value); }
    private readonly object gate = new();
    private readonly List<TransferItem> items = [];
    private CancellationTokenSource? current;
    private Task? active;
    private bool disposed;
    public bool IsTransferring { get { lock (gate) return current is not null; } }
    public IReadOnlyList<TransferItem> Items { get { lock (gate) return items.Select(item => item with { }).ToArray(); } }
    public double OverallProgressPercent { get { lock (gate) return items.Count == 0 ? 0 : items.Average(item => item.ProgressPercent); } }
    public event Action<TransferItem>? ItemChanged;

    public Guid Add(TransferItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.Serial);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.LocalPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.RemotePath);
        if (!Enum.IsDefined(item.Direction)) throw new DeviceException($"Unknown transfer direction: {item.Direction}.", 400, 1007);
        if (!item.RemotePath.StartsWith('/') || item.RemotePath.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new DeviceException($"Remote path must be absolute and contain no control characters: '{item.RemotePath}'.", 400, 1007);
        if (item.Direction == TransferDirection.Push && !File.Exists(item.LocalPath)) throw new FileNotFoundException("Transfer source does not exist.", item.LocalPath);
        if (item.AutoInstall && (item.Direction != TransferDirection.Push || !item.IsApk)) throw new DeviceException("AutoInstall requires an APK upload.", 400, 1007);
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (items.Any(existing => existing.Id == item.Id)) throw new DeviceException($"Transfer id already exists: {item.Id}.");
            items.Add(item with { LocalPath = Path.GetFullPath(item.LocalPath), State = TransferState.Pending, ProgressPercent = 0, Error = null });
        }
        return item.Id;
    }

    public Task RunAsync(CancellationToken ct = default)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (current is not null) throw new DeviceException("The transfer queue is already running.");
            current = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var source = current;
            return active = Task.Run(() => ProcessAsync(source));
        }
    }

    public bool Cancel()
    {
        lock (gate)
        {
            if (current is null) return false;
            current.Cancel(); return true;
        }
    }

    private async Task ProcessAsync(CancellationTokenSource source)
    {
        try
        {
            while (true)
            {
                source.Token.ThrowIfCancellationRequested();
                TransferItem? item;
                lock (gate) item = items.FirstOrDefault(candidate => candidate.State == TransferState.Pending);
                if (item is null) return;
                Update(item, TransferState.Transferring, 0, null);
                try
                {
                    var progress = new DirectProgress(value =>
                    {
                        if (!double.IsFinite(value)) throw new DeviceException($"Transfer reported a nonfinite progress value: {value}.", 500, 2000);
                        Update(item, TransferState.Transferring, Math.Clamp(value, 0, 1) * 100, null);
                    });
                    if (item.Direction == TransferDirection.Push)
                        await backend.PushAsync(item.Serial, item.LocalPath, item.RemotePath, progress, source.Token).ConfigureAwait(false);
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(item.LocalPath)!);
                        await backend.PullAsync(item.Serial, item.RemotePath, item.LocalPath, progress, source.Token).ConfigureAwait(false);
                    }
                    source.Token.ThrowIfCancellationRequested();
                    if (item.AutoInstall)
                    {
                        Update(item, TransferState.Installing, 100, null);
                        await backend.InstallAsync(item.Serial, item.LocalPath, source.Token).ConfigureAwait(false);
                    }
                    Update(item, TransferState.Completed, 100, null);
                }
                catch (OperationCanceledException) when (source.IsCancellationRequested)
                {
                    Update(item, TransferState.Cancelled, item.ProgressPercent, "Cancelled.");
                    throw;
                }
                catch (Exception error) { Update(item, TransferState.Failed, item.ProgressPercent, error.Message); }
            }
        }
        finally
        {
            lock (gate)
            {
                if (ReferenceEquals(current, source)) current = null;
                source.Dispose();
            }
        }
    }

    private void Update(TransferItem item, TransferState state, double progress, string? error)
    {
        TransferItem snapshot;
        lock (gate)
        {
            item.State = state;
            item.ProgressPercent = Math.Max(item.ProgressPercent, progress);
            item.Error = error;
            snapshot = item with { };
        }
        if (ItemChanged is { } changed)
            foreach (Action<TransferItem> handler in changed.GetInvocationList())
                try { handler(snapshot); }
                catch (Exception observerError) { System.Diagnostics.Trace.WriteLine($"Transfer observer failed: {observerError.Message}"); }
    }

    public async ValueTask DisposeAsync()
    {
        Task? pending;
        lock (gate)
        {
            if (disposed) return;
            disposed = true;
            current?.Cancel(); pending = active;
        }
        if (pending is not null)
            try { await pending.ConfigureAwait(false); }
            catch (OperationCanceledException) { }
    }
}

```

## src/MatrixDevice.Features/MatrixDevice.Features.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup><ProjectReference Include="../MatrixDevice.Core/MatrixDevice.Core.csproj" /></ItemGroup>
</Project>

```

## src/MatrixDevice.Features/packages.lock.json

```json
{
  "version": 1,
  "dependencies": {
    "net10.0": {
      "AdvancedSharpAdbClient": {
        "type": "Transitive",
        "resolved": "3.6.16",
        "contentHash": "w5p3s+9Rkx9tmn5S5KU0XSTap5YWGHOvQOw/40M0d7eGOHf7idhX+kizWPmnP1ahbkic/yiX03SMYNR1mJSAVw=="
      },
      "SkiaSharp": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "TghYLOUaP3q6rTjQw7U9uQaaXh8lEs9rfA1U7/VtmyAptVDtj/SmjtcWzFWmcC/VMWxe3QgbV9uyvqDL7ySFtQ==",
        "dependencies": {
          "SkiaSharp.NativeAssets.Win32": "4.153.1",
          "SkiaSharp.NativeAssets.macOS": "4.153.1"
        }
      },
      "SkiaSharp.NativeAssets.macOS": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "YTojWeWIhnpF7uC8vpMxB5wRONKkDB2t3r/nNPA3DL2pgj9fqMXRevA/rIanYazWnco4jo1ufgp3HkXuh1Tz8A=="
      },
      "SkiaSharp.NativeAssets.Win32": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "Yqg6rChTVh0BUE8QQxtOA3uUBnihfgID4yex/AvDa0+os2bVcBjmI3Qj9tnOB+R1hd/Jry1Gwl8qw5JI9HZG8Q=="
      },
      "matrixdevice.core": {
        "type": "Project",
        "dependencies": {
          "AdvancedSharpAdbClient": "[3.6.16, )",
          "SkiaSharp": "[4.153.1, )"
        }
      }
    }
  }
}
```

## src/MatrixDevice.Features/Scripts/ScriptPackageService.cs

```csharp
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public sealed record ScriptManifest
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Version { get; init; } = "";
    public string Description { get; init; } = "";
    public string? Entry { get; init; }
    public string? MinimumAppVersion { get; init; }
    public string? AppName { get; init; }
    public string? Category { get; init; }
    public string? AppDownloadUrl { get; init; }
}

public sealed record ScriptStep
{
    public string Action { get; init; } = "";
    public int DurationMs { get; init; }
    public double? Probability { get; init; }
    public string? Value { get; init; }
}

public sealed record ScriptParameter
{
    public string Key { get; init; } = "";
    public string? Type { get; init; }
    public string? LabelZh { get; init; }
    public string? LabelEn { get; init; }
    public string? Unit { get; init; }
    public double? Min { get; init; }
    public double? Max { get; init; }
    public double? Step { get; init; }
    public JsonElement? DefaultValue { get; init; }
    public JsonElement? Value { get; init; }
}

public sealed record ScriptPackage
{
    public ScriptManifest Manifest { get; init; } = new();
    public List<ScriptStep> Steps { get; init; } = [];
    public List<ScriptParameter> Parameters { get; init; } = [];
}

public static class ScriptPackageService
{
    public const int MaximumArchiveBytes = 16 * 1024 * 1024;
    public const int MaximumJsonBytes = 4 * 1024 * 1024;
    private static readonly HashSet<string> Actions = new(StringComparer.OrdinalIgnoreCase)
    { "launchapp", "launch", "wait", "sleep", "click", "swipe" };

    public static ScriptPackage Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var file = File.OpenRead(path);
        if (file.Length > MaximumArchiveBytes) throw Invalid($"Script archive exceeds {MaximumArchiveBytes} bytes.");
        using var archive = new ZipArchive(file, ZipArchiveMode.Read);
        if (archive.Entries.Count > 128) throw Invalid("Script archive has more than 128 entries.");
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        long total = 0;
        foreach (var entry in archive.Entries)
        {
            ValidateEntryName(entry.FullName);
            if (!names.Add(entry.FullName)) throw Invalid($"Duplicate ZIP entry: {entry.FullName}");
            total += entry.Length;
            if (entry.Length > MaximumArchiveBytes || total > 32L * 1024 * 1024)
                throw Invalid($"Uncompressed ZIP limit exceeded at: {entry.FullName}");
        }
        var manifest = Read<ScriptManifest>(archive, "manifest.json");
        var package = Read<ScriptPackage>(archive, "script.json") with { Manifest = manifest };
        package = package with { Steps = package.Steps ?? [], Parameters = package.Parameters ?? [] };
        Validate(package);
        return package;
    }

    public static void Save(string path, ScriptPackage package)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Validate(package);
        var manifest = JsonSerializer.SerializeToUtf8Bytes(package.Manifest, JsonDefaults.Options);
        var script = JsonSerializer.SerializeToUtf8Bytes(package, JsonDefaults.Options);
        if (manifest.Length > MaximumJsonBytes || script.Length > MaximumJsonBytes)
            throw Invalid("Serialized script JSON exceeds the package limit.");
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporary = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var file = File.Create(temporary))
            using (var archive = new ZipArchive(file, ZipArchiveMode.Create))
            {
                Write(archive, "manifest.json", manifest);
                Write(archive, "script.json", script);
            }
            File.Move(temporary, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    public static void Validate(ScriptPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        if (package.Manifest is null || string.IsNullOrWhiteSpace(package.Manifest.Id))
            throw Invalid("manifest.json must contain a nonblank Id.");
        if (package.Steps is null || package.Parameters is null) throw Invalid("Steps and Parameters must be arrays.");
        if (package.Steps.Count > 10000 || package.Parameters.Count > 1000) throw Invalid("Script item count limit exceeded.");
        for (var index = 0; index < package.Steps.Count; index++)
        {
            var step = package.Steps[index];
            if (step is null) throw Invalid($"Step {index} is null.");
            var action = step.Action?.Trim() ?? "";
            if (!Actions.Contains(action)) throw Invalid($"Unknown action at step {index}: '{step.Action}'.");
            if (step.DurationMs < 0) throw Invalid($"Step {index} DurationMs is negative: {step.DurationMs}.");
            if (step.Probability is { } probability && !double.IsFinite(probability))
                throw Invalid($"Step {index} Probability is not finite.");
            switch (action.ToLowerInvariant())
            {
                case "launch": case "launchapp":
                    ValidatePackageName(step.Value);
                    break;
                case "click":
                    ParseIntegers(step.Value, 2, index);
                    break;
                case "swipe":
                    var values = ParseIntegers(step.Value, 5, index);
                    if (values[4] <= 0) throw Invalid($"Step {index} swipe duration must be positive: {values[4]}.");
                    break;
            }
        }
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var parameter in package.Parameters)
        {
            if (parameter is null || string.IsNullOrWhiteSpace(parameter.Key)) throw Invalid("A parameter has a blank Key.");
            if (!keys.Add(parameter.Key)) throw Invalid($"Duplicate parameter Key: {parameter.Key}.");
            if (parameter.Min is { } min && !double.IsFinite(min) || parameter.Max is { } max && !double.IsFinite(max))
                throw Invalid($"Parameter {parameter.Key} bounds must be finite.");
            if (parameter.Min > parameter.Max) throw Invalid($"Parameter {parameter.Key} Min exceeds Max.");
            if (parameter.Step is { } interval && (!double.IsFinite(interval) || interval <= 0))
                throw Invalid($"Parameter {parameter.Key} Step must be positive: {interval}.");
            ValidateValue(parameter, parameter.DefaultValue, "DefaultValue");
            ValidateValue(parameter, parameter.Value, "Value");
        }
    }

    public static int[] ParseIntegers(string? value, int count, int stepIndex = -1)
    {
        var parts = (value ?? "").Split(',');
        if (parts.Length != count) throw Invalid($"Step {stepIndex} requires {count} comma-separated integers: '{value}'.");
        var result = new int[count];
        for (var index = 0; index < count; index++)
            if (!int.TryParse(parts[index].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result[index]))
                throw Invalid($"Step {stepIndex} has an invalid integer: '{parts[index]}'.");
        return result;
    }

    public static void ValidatePackageName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 255 ||
            value.Split('.').Any(part => part.Length == 0 || !char.IsAsciiLetter(part[0]) ||
                part.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '_')))
            throw Invalid($"Invalid Android package name: '{value}'.");
    }

    private static void ValidateValue(ScriptParameter parameter, JsonElement? value, string field)
    {
        if (value is not { } element || element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) return;
        var type = parameter.Type?.Trim().ToLowerInvariant();
        bool compatible = type switch
        {
            "int" or "integer" => element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out _),
            "number" or "double" or "float" => element.ValueKind == JsonValueKind.Number,
            "bool" or "boolean" => element.ValueKind is JsonValueKind.True or JsonValueKind.False,
            "string" or "text" => element.ValueKind == JsonValueKind.String,
            _ => true
        };
        if (!compatible) throw Invalid($"Parameter {parameter.Key} {field} does not match declared Type '{parameter.Type}'.");
        if (element.ValueKind == JsonValueKind.Number)
        {
            if (!element.TryGetDouble(out var number) || !double.IsFinite(number)) throw Invalid($"Parameter {parameter.Key} {field} is not finite.");
            if (parameter.Min is { } min && number < min || parameter.Max is { } max && number > max)
                throw Invalid($"Parameter {parameter.Key} {field} is outside its declared bounds: {number}.");
        }
    }

    private static void ValidateEntryName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains('\\') || name.Contains(':') || name.StartsWith('/') ||
            name.Split('/').Any(part => part is ".." or "."))
            throw Invalid($"Unsafe ZIP entry name: '{name}'.");
    }

    private static T Read<T>(ZipArchive archive, string name)
    {
        var entry = archive.GetEntry(name) ?? throw Invalid($"Required ZIP entry is missing: {name}.");
        if (entry.Length > MaximumJsonBytes) throw Invalid($"ZIP JSON entry is too large: {name}.");
        using var stream = entry.Open();
        using var bytes = new MemoryStream();
        var buffer = new byte[8192];
        while (true)
        {
            var read = stream.Read(buffer);
            if (read == 0) break;
            if (bytes.Length + read > MaximumJsonBytes) throw Invalid($"ZIP JSON entry exceeds its read limit: {name}.");
            bytes.Write(buffer, 0, read);
        }
        try { return JsonSerializer.Deserialize<T>(bytes.ToArray(), JsonDefaults.Options) ?? throw Invalid($"ZIP JSON is null: {name}."); }
        catch (JsonException error) { throw Invalid($"Invalid JSON in {name}: {error.Message}"); }
    }

    private static void Write(ZipArchive archive, string name, byte[] bytes)
    {
        using var stream = archive.CreateEntry(name, CompressionLevel.Optimal).Open();
        stream.Write(bytes);
    }

    private static DeviceException Invalid(string message) => new(message, 400, 1007);
}

```

## src/MatrixDevice.Features/Scripts/ScriptScheduler.cs

```csharp
using System.Text.Json;
using MatrixDevice.Core;

namespace MatrixDevice.Features;

public enum ScriptRunState { Idle, Running, Cancelling, Completed, Cancelled, Failed }
public sealed record ScriptLogEntry(DateTimeOffset Timestamp, string Message);
public sealed record ScriptRunStatus(string Serial, ScriptRunState State, string? PackageId = null,
    string? Error = null, DateTimeOffset? StartedUtc = null, DateTimeOffset? EndedUtc = null);

public sealed class ScriptScheduler(IDeviceBackend backend) : IAsyncDisposable
{
    private sealed class Run(CancellationTokenSource cancellation)
    {
        public CancellationTokenSource Cancellation { get; } = cancellation;
        public Task Task { get; set; } = Task.CompletedTask;
        public TaskCompletionSource Activation { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    private readonly object gate = new();
    private readonly Queue<ScriptRunStatus> notifications = new();
    private int publishing;
    private readonly Dictionary<string, Run> runs = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ScriptRunStatus> statuses = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<ScriptLogEntry>> logs = new(StringComparer.Ordinal);
    private bool disposed;
    public event Action<ScriptRunStatus>? StatusChanged;
    public event Action<string, ScriptLogEntry>? EntryAdded;

    public Task RunAsync(string serial, ScriptPackage package, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serial);
        ScriptPackageService.Validate(package);
        var snapshot = JsonSerializer.Deserialize<ScriptPackage>(JsonSerializer.Serialize(package, JsonDefaults.Options), JsonDefaults.Options)!;
        Task task;
        ScriptRunStatus? started = null;
        Run? launched = null;
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (runs.ContainsKey(serial)) task = Task.CompletedTask;
            else
            {
                var run = new Run(CancellationTokenSource.CreateLinkedTokenSource(ct));
                logs[serial] = [];
                started = new ScriptRunStatus(serial, ScriptRunState.Running, snapshot.Manifest.Id, StartedUtc: DateTimeOffset.UtcNow);
                statuses[serial] = started;
                notifications.Enqueue(started);
                runs[serial] = run;
                launched = run;
                task = run.Task = Task.Run(() => ExecuteAsync(serial, snapshot, run));
            }
        }
        if (started is null) Append(serial, "脚本正在运行，未重复启动。");
        else
        {
            PublishStatuses();
            Append(serial, $"开始运行：{snapshot.Manifest.Name} ({snapshot.Manifest.Id})。");
            launched!.Activation.TrySetResult();
        }
        return task;
    }

    public bool Stop(string serial)
    {
        ScriptRunStatus? status;
        lock (gate)
        {
            if (!runs.TryGetValue(serial, out var run)) return false;
            status = statuses[serial] with { State = ScriptRunState.Cancelling };
            statuses[serial] = status;
            notifications.Enqueue(status);
            run.Cancellation.Cancel();
        }
        PublishStatuses();
        Append(serial, "已请求取消脚本。");
        return true;
    }

    public bool OnDeviceDisconnected(string serial) => Stop(serial);
    public IReadOnlyList<ScriptLogEntry> GetLogs(string serial)
    {
        lock (gate) return logs.TryGetValue(serial, out var entries) ? entries.ToArray() : [];
    }
    public ScriptRunStatus GetStatus(string serial)
    {
        lock (gate) return statuses.GetValueOrDefault(serial) ?? new(serial, ScriptRunState.Idle);
    }

    private async Task ExecuteAsync(string serial, ScriptPackage package, Run run)
    {
        await run.Activation.Task.ConfigureAwait(false);
        var state = ScriptRunState.Completed;
        string? failure = null;
        try
        {
            for (var index = 0; index < package.Steps.Count; index++)
            {
                var ct = run.Cancellation.Token;
                ct.ThrowIfCancellationRequested();
                var step = package.Steps[index];
                var actionName = step.Action.Trim().ToLowerInvariant() switch { "launch" or "launchapp" => "启动应用", "wait" or "sleep" => "等待", "click" => "点击", "swipe" => "滑动", _ => step.Action };
                Append(serial, $"步骤 {index + 1}/{package.Steps.Count}：{actionName} {step.Value}");
                switch (step.Action.Trim().ToLowerInvariant())
                {
                    case "launch": case "launchapp":
                        await backend.StartAppAsync(serial, step.Value!, ct: ct).ConfigureAwait(false);
                        break;
                    case "wait": case "sleep":
                        await Task.Delay(step.DurationMs, ct).ConfigureAwait(false);
                        break;
                    case "click":
                        var point = ScriptPackageService.ParseIntegers(step.Value, 2, index);
                        await backend.TapAsync(serial, point[0], point[1], ct).ConfigureAwait(false);
                        break;
                    case "swipe":
                        var swipe = ScriptPackageService.ParseIntegers(step.Value, 5, index);
                        await backend.SwipeAsync(serial, swipe[0], swipe[1], swipe[2], swipe[3], swipe[4], ct).ConfigureAwait(false);
                        break;
                }
            }
            run.Cancellation.Token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException) when (run.Cancellation.IsCancellationRequested)
        {
            state = ScriptRunState.Cancelled;
            Append(serial, "脚本已取消。");
        }
        catch (Exception error)
        {
            state = ScriptRunState.Failed;
            failure = error.Message;
            Append(serial, $"执行失败：{error.Message}");
            throw;
        }
        finally
        {
            ScriptRunStatus ended;
            var stateName = state switch { ScriptRunState.Completed => "已完成", ScriptRunState.Cancelled => "已取消", ScriptRunState.Failed => "失败", _ => "已结束" };
            Append(serial, $"运行结束：{stateName}。");
            lock (gate)
            {
                ended = statuses[serial] with { State = state, Error = failure, EndedUtc = DateTimeOffset.UtcNow };
                if (runs.TryGetValue(serial, out var current) && ReferenceEquals(current, run))
                {
                    runs.Remove(serial);
                    statuses[serial] = ended;
                    notifications.Enqueue(ended);
                }
                run.Cancellation.Dispose();
            }
            PublishStatuses();
        }
    }

    private void Append(string serial, string message)
    {
        var entry = new ScriptLogEntry(DateTimeOffset.UtcNow, message);
        lock (gate)
        {
            if (!logs.TryGetValue(serial, out var entries)) logs[serial] = entries = [];
            entries.Add(entry);
            if (entries.Count > 500) entries.RemoveRange(0, entries.Count - 500);
        }
        if (EntryAdded is { } added)
            foreach (Action<string, ScriptLogEntry> handler in added.GetInvocationList())
                try { handler(serial, entry); } catch (Exception error) { RecordObserverFailure(serial, error); }
    }

    private void PublishStatuses()
    {
        while (true)
        {
            if (Interlocked.CompareExchange(ref publishing, 1, 0) != 0) return;
            try
            {
                while (true)
                {
                    ScriptRunStatus status;
                    lock (gate)
                    {
                        if (notifications.Count == 0) break;
                        status = notifications.Dequeue();
                    }
                    if (StatusChanged is { } changed)
                        foreach (Action<ScriptRunStatus> handler in changed.GetInvocationList())
                            try { handler(status); } catch (Exception error) { RecordObserverFailure(status.Serial, error); }
                }
            }
            finally { Volatile.Write(ref publishing, 0); }
            lock (gate) if (notifications.Count == 0) return;
        }
    }

    private void RecordObserverFailure(string serial, Exception error)
    {
        lock (gate)
        {
            if (!logs.TryGetValue(serial, out var entries)) logs[serial] = entries = [];
            entries.Add(new(DateTimeOffset.UtcNow, $"状态监听失败：{error.Message}"));
            if (entries.Count > 500) entries.RemoveAt(0);
        }
    }

    public async ValueTask DisposeAsync()
    {
        Task[] pending;
        lock (gate)
        {
            if (disposed) return;
            disposed = true;
            foreach (var run in runs.Values) run.Cancellation.Cancel();
            pending = runs.Values.Select(run => run.Task).ToArray();
        }
        try { await Task.WhenAll(pending).ConfigureAwait(false); }
        catch (Exception error)
        {
            // Run failures remain in device status and are rethrown to callers.
            System.Diagnostics.Trace.WriteLine($"Script shutdown observed: {error.Message}");
        }
    }
}

```

## src/MatrixDevice.Host/Api/ApiServiceManager.cs

```csharp
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Core;
using MatrixDevice.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MatrixDevice.Host;

public sealed class ApiServiceManager(DeviceCatalog catalog, IDeviceBackend backend, AutomationService automation, RollingLog log) : IAsyncDisposable
{
    private readonly SemaphoreSlim _lifecycle = new(1, 1);
    private WebApplication? _basic;
    private WebApplication? _automator;
    private readonly ConcurrentDictionary<string, (int Port, WebApplication App)> _devices = new(StringComparer.Ordinal);
    private readonly Dictionary<int, DateTimeOffset> _cooldown = [];
    private int _basicPort;
    private int _automatorPort;
    private int activeRequests;
    public bool HasActiveRequests => Volatile.Read(ref activeRequests) != 0;
    private async Task TrackRequestAsync(Func<Task> action)
    {
        Interlocked.Increment(ref activeRequests);
        try { await action(); }
        finally { Interlocked.Decrement(ref activeRequests); }
    }
    public bool BasicRunning => _basic is not null;
    public bool AutomatorRunning => _automator is not null;
    public int? RunningBasicPort => BasicRunning ? _basicPort : null;
    public int? RunningAutomatorPort => AutomatorRunning ? _automatorPort : null;
    public Func<IReadOnlyList<DeviceInfo>>? DeviceSnapshotProvider { get; set; }
    public LdPlayerService? LdPlayer { get; set; }
    private int uiWriteBusy;
    public bool UiWriteBusy { get => Volatile.Read(ref uiWriteBusy) != 0; set => Volatile.Write(ref uiWriteBusy, value ? 1 : 0); }
    public Func<string, bool>? ScriptWriteBusy { get; set; }
    public Func<bool>? AnyScriptWriteBusy { get; set; }
    public Func<int, string?>? LdSerialForWriteGuard { get; set; }
    public Func<int, string, CancellationToken, Task>? LdCommandIdentityGuard { get; set; }
    private void RequireWriteAvailable(string? serial = null)
    {
        if (UiWriteBusy || (serial is null ? AnyScriptWriteBusy?.Invoke() == true : ScriptWriteBusy?.Invoke(serial) == true))
            throw new DeviceException("界面操作或脚本正在执行，请等待完成或停止后再通过 API 写入", 409, 1003);
    }
    public IReadOnlyDictionary<string, int> DevicePorts => _devices.ToDictionary(x => x.Key, x => x.Value.Port, StringComparer.Ordinal);

    public async Task StartBasicAsync(int port, CancellationToken ct = default)
    {
        await _lifecycle.WaitAsync(ct);
        try
        {
            if (_basic is not null)
            {
                if (_basicPort != port) throw new DeviceException("Stop the basic gateway before changing its port");
                return;
            }
            ValidatePort(port);
            if (_automatorPort == port || _devices.Values.Any(x => x.Port == port)) throw new DeviceException("Port already reserved: " + port);
            var app = CreateServer(port);
            app.Run(context => TrackRequestAsync(() => HandleBasicGatewayAsync(context, port)));
            await StartServerAsync(app, ct);
            _basic = app;
            _basicPort = port;
        }
        finally { _lifecycle.Release(); }
    }
    public async Task StartAutomatorAsync(int port, CancellationToken ct = default)
    {
        await _lifecycle.WaitAsync(ct);
        try
        {
            if (_automator is not null)
            {
                if (_automatorPort != port) throw new DeviceException("Stop Automator before changing its port");
                return;
            }
            ValidatePort(port);
            if (_basicPort == port || _devices.Values.Any(x => x.Port == port)) throw new DeviceException("Port already reserved: " + port);
            var app = CreateServer(port);
            app.Run(context => TrackRequestAsync(() => HandleAutomatorAsync(context)));
            await StartServerAsync(app, ct);
            _automator = app;
            _automatorPort = port;
        }
        finally { _lifecycle.Release(); }
    }
    public async Task<int> StartDeviceAsync(string serial, CancellationToken ct = default)
    {
        await _lifecycle.WaitAsync(ct);
        try
        {
            catalog.GetRequired(serial);
            if (_devices.TryGetValue(serial, out var existing)) return existing.Port;
            foreach (var expired in _cooldown.Where(x => x.Value <= DateTimeOffset.UtcNow).Select(x => x.Key).ToArray()) _cooldown.Remove(expired);
            Exception? lastError = null;
            for (var port = 8001; port <= 8999; port++)
            {
                if (port == _basicPort || port == _automatorPort || _devices.Values.Any(x => x.Port == port) || _cooldown.ContainsKey(port)) continue;
                var app = CreateServer(port);
                var reservedPort = port;
                app.Run(context => TrackRequestAsync(() => HandleDeviceAsync(context, serial, reservedPort)));
                try
                {
                    await app.StartAsync(ct);
                    _devices[serial] = (port, app);
                    return port;
                }
                catch (IOException e)
                {
                    lastError = e;
                    _cooldown[port] = DateTimeOffset.UtcNow.AddSeconds(30);
                    await app.DisposeAsync();
                }
                catch { await app.DisposeAsync(); throw; }
            }
            throw new DeviceException("No available device API port in 8001..8999: " + lastError?.Message, 503, 2000);
        }
        finally { _lifecycle.Release(); }
    }
    public async Task StopDeviceAsync(string serial, CancellationToken ct = default)
    {
        await _lifecycle.WaitAsync(ct);
        try
        {
            if (_devices.TryRemove(serial, out var server))
            {
                try { await server.App.StopAsync(ct); }
                finally { await server.App.DisposeAsync(); _cooldown[server.Port] = DateTimeOffset.UtcNow.AddSeconds(30); }
            }
        }
        finally { _lifecycle.Release(); }
    }
    public async Task StopAsync(CancellationToken ct = default)
    {
        await _lifecycle.WaitAsync(ct);
        try
        {
            List<Exception> errors = [];
            foreach (var server in _devices.Values.Select(x => x.App).Concat(new[] { _basic, _automator }).OfType<WebApplication>().ToArray())
            {
                try { await server.StopAsync(ct); }
                catch (Exception e) { errors.Add(e); }
                finally { try { await server.DisposeAsync(); } catch (Exception e) { errors.Add(e); } }
            }
            foreach (var port in _devices.Values.Select(x => x.Port)) _cooldown[port] = DateTimeOffset.UtcNow.AddSeconds(30);
            _devices.Clear();
            _basic = null;
            _automator = null;
            _basicPort = _automatorPort = 0;
            if (errors.Count > 0) throw new AggregateException("API shutdown errors", errors);
        }
        finally { _lifecycle.Release(); }
    }

    private object Snapshot(string serial, int port)
    {
        var device = (DeviceSnapshotProvider?.Invoke() ?? catalog.Snapshot).FirstOrDefault(x => x.Serial == serial) ?? new DeviceInfo { Serial = serial, State = "Offline" };
        return new { device.Serial, device.Name, device.Brand, device.Model, device.AndroidVersion, device.ConnectionType, device.GroupId, port, baseUrl = $"http://localhost:{port}/" };
    }
    public static string NormalizePath(string? path)
    {
        var normalized = (path ?? "").Trim().Trim('/').ToLowerInvariant();
        return normalized.Length == 0 ? "/" : "/" + normalized;
    }
    private async Task HandleBasicGatewayAsync(HttpContext context, int port)
    {
        var path = NormalizePath(context.Request.Path.Value);
        if (path.StartsWith("/ldplayer", StringComparison.Ordinal))
        {
            await HandleLdPlayerAsync(context, path);
            return;
        }
        if (context.Request.Method == "GET" && path is "/" or "/health")
            await WriteAsync(context, 200, new { ok = true, data = new { port, runningCount = _devices.Count, status = "running" } });
        else if (context.Request.Method == "GET" && path == "/devices")
            await WriteAsync(context, 200, new { ok = true, data = _devices.Select(x => Snapshot(x.Key, x.Value.Port)).ToArray() });
        else await WriteAsync(context, 404, new { ok = false, message = "Route not found: " + path });
    }
    private async Task HandleLdPlayerAsync(HttpContext context, string path)
    {
        try
        {
            object data;
            if (path == "/ldplayer/capabilities" && context.Request.Method == "GET")
                data = LdPlayerService.Capabilities;
            else if (path == "/ldplayer/instances" && context.Request.Method == "GET")
            {
                var service = LdPlayer ?? throw new DeviceException("尚未配置雷电命令行程序", 503, 2001);
                await service.RefreshAsync(context.RequestAborted);
                data = service.Instances;
            }
            else
            {
                var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 4 || parts[0] != "ldplayer" || parts[1] != "instances" ||
                    !int.TryParse(parts[2], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var index))
                    throw new DeviceException("雷电接口不存在：" + path, 404, 1006);
                if (context.Request.Method != "POST") throw new DeviceException("此接口仅接受 POST 请求", 405, 1005);
                ValidateWriteRequest(context);
                var service = LdPlayer ?? throw new DeviceException("尚未配置雷电命令行程序", 503, 2001);
                var body = await ReadBodyAsync(context);
                RequireWriteAvailable(LdSerialForWriteGuard?.Invoke(index));
                if (LdCommandIdentityGuard is not null) await LdCommandIdentityGuard(index, parts[3], context.RequestAborted);
                data = await service.ExecuteAsync(index, parts[3], JsonNode.Parse(body.GetRawText())!.AsObject(), context.RequestAborted);
                log.Write("ldplayer-command", $"index={index}, operation={parts[3]}");
            }
            await WriteAsync(context, 200, new { ok = true, data });
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (Exception e)
        {
            log.Write("ldplayer-api-error", e.Message);
            await WriteAsync(context, ErrorStatus(e), new { ok = false, message = e.Message });
        }
    }
    private async Task HandleDeviceAsync(HttpContext context, string serial, int port)
    {
        try
        {
            var device = catalog.Snapshot.FirstOrDefault(x => x.Serial == serial);
            if (device?.Online != true) throw new DeviceException("Device unavailable: " + serial, 410, 1003);
            var path = NormalizePath(context.Request.Path.Value);
            if (context.Request.Method == "GET")
            {
                object response = path switch
                {
                    "/" or "/health" => new { ok = true, data = new { serial, port, status = "running" } },
                    "/device" => new { ok = true, data = Snapshot(serial, port) },
                    _ => throw new DeviceException("Route not found: " + path, 404, 1006)
                };
                await WriteAsync(context, 200, response);
                return;
            }
            if (context.Request.Method != "POST") throw new DeviceException("Method not allowed", 405, 1005);
            if (path is not ("/shell" or "/tap" or "/swipe" or "/text" or "/app/start" or "/app/stop"))
                throw new DeviceException("Route not found: " + path, 404, 1006);
            ValidateWriteRequest(context);
            var body = await ReadBodyAsync(context);
            RequireWriteAvailable(serial);
            var ct = context.RequestAborted;
            object? data = null;
            switch (path)
            {
                case "/shell": data = new { output = await backend.ShellAsync(serial, RequiredString(body, "Command"), OptionalBool(body, "UseRoot"), ct) }; break;
                case "/tap": await backend.TapAsync(serial, RequiredInt(body, "X"), RequiredInt(body, "Y"), ct); break;
                case "/swipe":
                    var duration = OptionalInt(body, "DurationMs") ?? 300;
                    await backend.SwipeAsync(serial, RequiredInt(body, "StartX"), RequiredInt(body, "StartY"), RequiredInt(body, "EndX"), RequiredInt(body, "EndY"), duration <= 0 ? 300 : duration, ct);
                    break;
                case "/text": await backend.TextAsync(serial, RequiredString(body, "Text"), ct); break;
                case "/app/start": data = new { output = await backend.StartAppAsync(serial, RequiredString(body, "PackageName"), OptionalString(body, "Activity"), ct) }; break;
                case "/app/stop": data = new { output = await backend.StopAppAsync(serial, RequiredString(body, "PackageName"), ct) }; break;
            }
            await WriteAsync(context, 200, new { ok = true, data });
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (Exception e)
        {
            log.Write("basic-api-error", e.Message, serial);
            await WriteAsync(context, ErrorStatus(e), new { ok = false, message = e.Message });
        }
    }
    private async Task HandleAutomatorAsync(HttpContext context)
    {
        try
        {
            var path = NormalizePath(context.Request.Path.Value).Trim('/');
            if (path.StartsWith("api/", StringComparison.Ordinal)) path = path[4..];
            if (context.Request.Method == "GET")
            {
                object data = path switch
                {
                    "health" => new { running = true, prefix = $"http://127.0.0.1:{_automatorPort}/" },
                    "devices" => DeviceSnapshotProvider?.Invoke() ?? catalog.Snapshot,
                    _ => throw new DeviceException("Route not found: " + path, 404, 1006)
                };
                await WriteAsync(context, 200, new { code = 0, message = "ok", data });
                return;
            }
            if (context.Request.Method != "POST") throw new DeviceException("Method not allowed", 405, 1005);
            if (!AutomationService.Routes.Contains(path)) throw new DeviceException("Route not found: " + path, 404, 1006);
            ValidateWriteRequest(context);
            var body = await ReadBodyAsync(context);
            if (path is "device/shell" or "device/execute" or "device/set-prop" or "device/set-timezone" or "device/set-sim-info" or
                "app/start" or "app/stop" or "app/clear" or "input/tap" or "input/swipe" or "input/keyevent" or "service/stop" or
                "element/click-if-exists" or "element/click-text" or "element/set-text" or "element/click-node") RequireWriteAvailable(RequiredString(body, "serial"));
            var dataResult = await automation.ExecuteAsync(path, body, context.RequestAborted);
            await WriteAsync(context, 200, new { code = 0, message = "ok", data = dataResult });
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (Exception e)
        {
            log.Write("automator-api-error", e.Message);
            var code = e is DeviceException de ? de.Code : e is JsonException or ArgumentException ? 1007 : 2000;
            await WriteAsync(context, ErrorStatus(e), new { code, message = e.Message });
        }
    }
    private static void ValidateWriteRequest(HttpContext context)
    {
        if (!context.Request.HasJsonContentType()) throw new DeviceException("请求必须使用 application/json", 415, 1007);
        if (context.Request.Headers.TryGetValue("Origin", out var origin) &&
            (!Uri.TryCreate(origin.ToString(), UriKind.Absolute, out var source) || source.Scheme != "http" ||
             source.Port != context.Connection.LocalPort || source.Host is not ("localhost" or "127.0.0.1" or "[::1]")))
            throw new DeviceException("拒绝跨站设备控制请求", 403, 1007);
    }
    private static WebApplication CreateServer(int port)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [] });
        builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel(options => { options.ListenLocalhost(port); options.Limits.MaxRequestBodySize = 1024 * 1024; });
        return builder.Build();
    }
    private static async Task StartServerAsync(WebApplication app, CancellationToken ct)
    {
        try { await app.StartAsync(ct); }
        catch { await app.DisposeAsync(); throw; }
    }
    private static void ValidatePort(int port)
    {
        if (port is < 1 or > 65535) throw new ArgumentException("Invalid port: " + port);
    }
    private static int ErrorStatus(Exception e) => e switch
    {
        DeviceException de => de.Status,
        JsonException or ArgumentException or BadHttpRequestException => 400,
        TimeoutException => 504,
        _ => 500
    };
    private static async Task<JsonElement> ReadBodyAsync(HttpContext context)
    {
        using var document = await JsonDocument.ParseAsync(context.Request.Body, new JsonDocumentOptions { MaxDepth = 64 }, context.RequestAborted);
        if (document.RootElement.ValueKind != JsonValueKind.Object) throw new ArgumentException("Request body must be a JSON object");
        return document.RootElement.Clone();
    }
    private static JsonElement? Property(JsonElement body, string name)
    {
        JsonElement? result = null;
        foreach (var item in body.EnumerateObject())
        {
            if (!item.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) continue;
            if (result is not null) throw new ArgumentException("Duplicate property: " + name);
            result = item.Value;
        }
        return result;
    }
    private static string RequiredString(JsonElement body, string name) => OptionalString(body, name) is { Length: > 0 } value && !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Required string: " + name);
    private static string? OptionalString(JsonElement body, string name)
    {
        var value = Property(body, name);
        if (value is null || value.Value.ValueKind == JsonValueKind.Null) return null;
        if (value.Value.ValueKind != JsonValueKind.String) throw new ArgumentException("Expected string: " + name);
        return value.Value.GetString();
    }
    private static int RequiredInt(JsonElement body, string name) => OptionalInt(body, name) ?? throw new ArgumentException("Required integer: " + name);
    private static int? OptionalInt(JsonElement body, string name)
    {
        var value = Property(body, name);
        if (value is null || value.Value.ValueKind == JsonValueKind.Null) return null;
        return value.Value.ValueKind == JsonValueKind.Number && value.Value.TryGetInt32(out var number) ? number : throw new ArgumentException("Expected integer: " + name);
    }
    private static bool OptionalBool(JsonElement body, string name)
    {
        var value = Property(body, name);
        if (value is null) return false;
        if (value.Value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw new ArgumentException("Expected boolean: " + name);
        return value.Value.GetBoolean();
    }
    private static Task WriteAsync(HttpContext context, int status, object value)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json; charset=utf-8";
        return context.Response.WriteAsync(JsonSerializer.Serialize(value, JsonDefaults.Options), context.RequestAborted);
    }
    public async ValueTask DisposeAsync() { await StopAsync(); _lifecycle.Dispose(); }
}

```

## src/MatrixDevice.Host/Diagnostics/RollingLog.cs

```csharp
using System.IO.Compression;
using System.Text;
using System.Threading.Channels;

namespace MatrixDevice.Host;

public sealed class RollingLog : IAsyncDisposable
{
    private readonly string _directory;
    private readonly Channel<string> _queue = Channel.CreateBounded<string>(new BoundedChannelOptions(4096) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true });
    private readonly Task _writer;
    private DateTimeOffset _lastCleanup;
    public string? LastError { get; private set; }
    public event Action<string>? EntryAdded;
    public RollingLog(string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(directory);
        _writer = WriteLoopAsync();
    }
    public void Write(string category, string message, string? serial = null)
    {
        var line = $"{DateTimeOffset.UtcNow:O} [{category}] [{serial ?? "host"}] {message.Replace('\r', ' ').Replace('\n', ' ')}";
        if (!_queue.Writer.TryWrite(line)) LastError = "Log queue is full or closed";
        EntryAdded?.Invoke(line);
    }
    private async Task WriteLoopAsync()
    {
        await foreach (var line in _queue.Reader.ReadAllAsync())
        {
            try
            {
                if (DateTimeOffset.UtcNow - _lastCleanup > TimeSpan.FromMinutes(5)) { Cleanup(); _lastCleanup = DateTimeOffset.UtcNow; }
                var stem = DateTime.UtcNow.ToString("yyyy-MM-dd");
                var file = Path.Combine(_directory, stem + ".log");
                var size = Encoding.UTF8.GetByteCount(line) + 1;
                for (var i = 1; File.Exists(file) && new FileInfo(file).Length + size > 10 * 1024 * 1024; i++)
                    file = Path.Combine(_directory, stem + $".{i}.log");
                await File.AppendAllTextAsync(file, line + "\n", new UTF8Encoding(false));
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { LastError = e.Message; }
        }
    }
    private void Cleanup()
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd") + ".log";
        foreach (var path in Directory.EnumerateFiles(_directory))
        {
            var file = new FileInfo(path);
            if (file.Name == today) continue;
            if (file.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-14)) { file.Delete(); continue; }
            if (file.Extension == ".log" && file.Length >= 5 * 1024 * 1024)
            {
                var destination = path + ".gz";
                using (var input = file.OpenRead())
                using (var output = File.Create(destination))
                using (var gzip = new GZipStream(output, CompressionLevel.Optimal)) input.CopyTo(gzip);
                file.Delete();
            }
        }
        var remaining = new DirectoryInfo(_directory).GetFiles().OrderBy(f => f.LastWriteTimeUtc).ToList();
        var total = remaining.Sum(f => f.Length);
        foreach (var file in remaining)
        {
            if (total <= 200L * 1024 * 1024) break;
            if (file.Name == today) continue;
            total -= file.Length;
            file.Delete();
        }
    }
    public async ValueTask DisposeAsync()
    {
        _queue.Writer.TryComplete();
        await _writer.WaitAsync(TimeSpan.FromSeconds(3));
    }
}

```

## src/MatrixDevice.Host/LdConnectionAdmission.cs

```csharp
using System.Diagnostics;
using System.Net;
using MatrixDevice.Core;

namespace MatrixDevice.Host;

public sealed record LdConnectionProof(int Index, string Name, long Generation, string Serial, uint TransportId, int VBoxPid, long VBoxBirth);

public static class LdConnectionAdmission
{
    public static async Task<IReadOnlyList<LdConnectionProof>> ResolveAsync(AdbDeviceBackend backend, LdPlayerService service, int[] indices, IReadOnlyList<LdPlayerInstance> expectedInstances, IReadOnlyDictionary<int, long> expectedBirths, CancellationToken ct = default)
    {
        if (indices.Length == 0 || indices.Any(index => index <= 0 || index == 99999) || indices.Distinct().Count() != indices.Length)
            throw new DeviceException("请选择不重复的独立雷电实例，受保护实例0不能自动接入。", 400, 1007);
        var instances = await service.RefreshAsync(ct).ConfigureAwait(false);
        RequireExpectedInstances(instances, indices, expectedInstances);
        var registered = await backend.ListRegisteredDevicesAsync(ct).ConfigureAwait(false);
        var proofs = new List<LdConnectionProof>();
        foreach (var index in indices)
        {
            var instance = instances.SingleOrDefault(item => item.Index == index) ?? throw new DeviceException($"实例已不存在：{index}", 404, 1002);
            if (!instance.AndroidStarted || instance.ProcessId <= 0 || instance.VBoxProcessId <= 0) throw new DeviceException($"实例{index}尚未完成启动。", 409, 1003);
            using var process = Process.GetProcessById(instance.VBoxProcessId);
            var birth = process.StartTime.ToUniversalTime().Ticks;
            if (!expectedBirths.TryGetValue(index, out var expectedBirth) || birth != expectedBirth) throw new DeviceException($"实例{index}的虚拟机进程已变化，必须重新确认。", 409, 1003);
            var matches = new List<(AdbRegisteredDevice Device, string Endpoint)>();
            foreach (var device in registered.Where(item => item.State == "device" && item.TransportId is > 0).OrderBy(item => item.Serial.StartsWith("emulator-", StringComparison.Ordinal) ? 0 : 1))
            {
                var endpoint = LocalEndpoint(device.Serial);
                if (endpoint is null) continue;
                try { await LdPlayerAdbBinding.VerifyAsync(device.Serial, instance.VBoxProcessId, birth, ct).ConfigureAwait(false); }
                catch (DeviceException) { continue; }
                matches.Add((device, endpoint));
            }
            var endpoints = matches.GroupBy(item => item.Endpoint, StringComparer.Ordinal).ToArray();
            if (endpoints.Length == 0) throw new DeviceException($"实例{index}尚未注册可核验的ADB连接。", 409, 2002);
            if (endpoints.Length != 1) throw new DeviceException($"实例{index}存在多个ADB端点，不能唯一接入。", 409, 1003);
            var chosen = endpoints[0].First().Device;
            var current = (await service.RefreshAsync(ct).ConfigureAwait(false)).Single(item => item.Index == index);
            if (current.Generation != instance.Generation || process.HasExited || process.StartTime.ToUniversalTime().Ticks != birth)
                throw new DeviceException($"实例{index}在接入核验期间发生变化，请重试接入。", 409, 1003);
            proofs.Add(new(index, instance.Name, instance.Generation, chosen.Serial, chosen.TransportId!.Value, instance.VBoxProcessId, birth));
        }
        if (proofs.Select(proof => proof.Serial).Distinct(StringComparer.Ordinal).Count() != proofs.Count) throw new DeviceException("多个实例不能接入同一设备连接。", 409, 1003);
        return proofs.AsReadOnly();
    }

    public static void RequireExpectedInstances(IReadOnlyList<LdPlayerInstance> actual, int[] indices, IReadOnlyList<LdPlayerInstance> expected)
    {
        if (expected.Count != indices.Length || expected.Select(item => item.Index).Distinct().Count() != expected.Count || expected.Any(item => !indices.Contains(item.Index))) throw new DeviceException("接入实例快照不完整或重复。", 409, 1003);
        foreach (var frozen in expected)
        {
            var current = actual.SingleOrDefault(item => item.Index == frozen.Index);
            if (current is null || current.Generation != frozen.Generation || current.Name != frozen.Name || current.ProcessId != frozen.ProcessId || current.VBoxProcessId != frozen.VBoxProcessId)
                throw new DeviceException($"实例 {frozen.Index} 在确认后发生变化，必须重新确认接入。", 409, 1003);
        }
    }

    private static string? LocalEndpoint(string serial)
    {
        if (serial.StartsWith("emulator-", StringComparison.Ordinal) && int.TryParse(serial[9..], out var port) && port is > 0 and < 65535) return "127.0.0.1:" + (port + 1);
        return IPEndPoint.TryParse(serial, out var endpoint) && IPAddress.IsLoopback(endpoint.Address) && endpoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? endpoint.ToString() : null;
    }

    public static AppSettings Apply(AppSettings settings, IReadOnlyList<LdConnectionProof> proofs)
    {
        if (proofs.Count == 0 || proofs.Any(proof => proof.Index <= 0 || proof.Index == 99999 || proof.Generation <= 0 || string.IsNullOrWhiteSpace(proof.Name) || LocalEndpoint(proof.Serial) is null || proof.TransportId == 0 || proof.VBoxPid <= 0 || proof.VBoxBirth <= 0)) throw new DeviceException("接入凭据无效。", 400, 1007);
        if (proofs.Select(proof => proof.Index).Distinct().Count() != proofs.Count || proofs.Select(proof => proof.Serial).Distinct(StringComparer.Ordinal).Count() != proofs.Count) throw new DeviceException("接入凭据存在重复实例或串号。", 409, 1003);
        var bindings = new Dictionary<int, string>(settings.LdSerialBindings);
        var replaced = proofs.Where(proof => bindings.ContainsKey(proof.Index)).Select(proof => bindings[proof.Index]).ToHashSet(StringComparer.Ordinal);
        foreach (var proof in proofs) bindings[proof.Index] = proof.Serial;
        if (bindings.Values.Distinct(StringComparer.Ordinal).Count() != bindings.Count) throw new DeviceException("该连接已绑定其它实例，请先核对原绑定。", 409, 1003);
        return settings with { LdSerialBindings = bindings, AdbAllowedSerials = settings.AdbAllowedSerials.Where(serial => !replaced.Contains(serial)).Concat(proofs.Select(proof => proof.Serial)).Distinct(StringComparer.Ordinal).ToArray(), AutoStartApi = false };
    }
}

```

## src/MatrixDevice.Host/MatrixDevice.Host.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
    <ProjectReference Include="../MatrixDevice.Features/MatrixDevice.Features.csproj" />
    <PackageReference Include="Microsoft.Data.Sqlite" Version="10.0.12" />
  </ItemGroup>
</Project>

```

## src/MatrixDevice.Host/MatrixRuntime.cs

```csharp
using MatrixDevice.Core;
using MatrixDevice.Features;

namespace MatrixDevice.Host;

public sealed class MatrixRuntime : IAsyncDisposable
{
    public required IDeviceBackend Backend { get; init; }
    public required DeviceCatalog Devices { get; init; }
    public required ScriptScheduler Scripts { get; init; }
    public required InputCoordinator Input { get; init; }
    public required ApiServiceManager Api { get; init; }
    public required SettingsStore SettingsStore { get; init; }
    public required CatalogRepository Metadata { get; init; }
    public required AutomationService Automation { get; init; }
    public required RollingLog Log { get; init; }
    public required AppSettings Settings { get; set; }
    public required AppSettings FrozenConnectionSettings { get; init; }
    public LdPlayerService? LdPlayer { get; init; }
    public LdInstanceEditor? LdEditor { get; init; }
    public string? LdPlayerError { get; private set; }
    public bool IsDemo { get; init; }
    public string? DiscoveryError { get; private set; }
    public string? ApiStartupError { get; private set; }

    public static async Task<MatrixRuntime> CreateAsync(AppSettings settings, string dataDirectory, bool demo = false, IReadOnlyList<LdConnectionProof>? admissions = null, AdbDeviceBackend? epochSource = null, CancellationToken ct = default)
    {
        settings = settings with { Language = "zh-CN" };
        Directory.CreateDirectory(dataDirectory);
        var store = new SettingsStore(Path.Combine(dataDirectory, "AppConfig.json"));
        store.Load();
        if (!string.IsNullOrWhiteSpace(settings.AgentBaseUrl) && string.IsNullOrWhiteSpace(settings.AgentSerial))
            throw new ArgumentException("AgentSerial is required when AgentBaseUrl is configured");
        if (!string.IsNullOrWhiteSpace(settings.AgentBaseUrl) && (!Uri.TryCreate(settings.AgentBaseUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")))
            throw new ArgumentException("Invalid AgentBaseUrl: " + settings.AgentBaseUrl);
        CatalogRepository? metadata = null;
        RollingLog? log = null;
        IDeviceBackend? backend = null;
        ScriptScheduler? scripts = null;
        InputCoordinator? input = null;
        ApiServiceManager? api = null;
        LdPlayerService? ldPlayer = null;
        LdInstanceEditor? ldEditor = null;
        string? ldPlayerError = null;
        try
        {
        metadata = new CatalogRepository(Path.Combine(dataDirectory, "db.sqlite3"));
        log = new RollingLog(Path.Combine(dataDirectory, "logs"));
        backend = demo ? (IDeviceBackend)new DemoDeviceBackend() : new AdbDeviceBackend(settings);
        if (backend is AdbDeviceBackend inheritedBackend && epochSource is not null) inheritedBackend.InheritServerEpoch(epochSource);
        var catalog = new DeviceCatalog(backend);
        var automation = new AutomationService(backend, catalog);
        if (backend is AdbDeviceBackend adb && !string.IsNullOrWhiteSpace(settings.AgentBaseUrl))
        {
            var client = new PhoneAgentClient(settings.AgentBaseUrl);
            adb.RegisterAgent(settings.AgentSerial!, client);
            automation.ServiceInfoProvider = async (serial, ct) =>
            {
                if (serial != settings.AgentSerial) throw new DeviceException("No agent is bound to serial=" + serial, 501, 2001);
                if (settings.LdSerialBindings.Values.Contains(serial, StringComparer.Ordinal)) await adb.VerifyIdentityAsync(serial, ct);
                return await client.ServiceInfoAsync(ct);
            };
            automation.ServiceStop = async (serial, ct) =>
            {
                if (serial != settings.AgentSerial) throw new DeviceException("No agent is bound to serial=" + serial, 501, 2001);
                if (settings.LdSerialBindings.Values.Contains(serial, StringComparer.Ordinal)) await adb.VerifyIdentityAsync(serial, ct);
                await client.StopServiceAsync(ct);
            };
        }
        automation.ScreenshotTransform = (png, x, y, width, height, quality) =>
        {
            var size = PngImage.ReadDimensions(png);
            var left = x ?? 0;
            var top = y ?? 0;
            return PngImage.Crop(png, left, top, width ?? size.Width - left, height ?? size.Height - top);
        };
        scripts = new ScriptScheduler(backend);
        input = new InputCoordinator(catalog, backend);
        if (demo) ldPlayer = new LdPlayerService(new DemoLdConsoleRunner(), true);
        else if (!string.IsNullOrWhiteSpace(settings.LdConsolePath))
        {
            try
            {
                var runner = new LdConsoleRunner(settings.LdConsolePath, settings.CommandTimeoutSeconds);
                ldPlayer = new LdPlayerService(runner);
                ldEditor = new LdInstanceEditor(runner, ldPlayer);
            }
            catch (DeviceException e) { ldPlayerError = e.Message; log.Write("ldplayer-configuration-error", e.Message); }
        }
        api = new ApiServiceManager(catalog, backend, automation, log) { LdPlayer = ldPlayer };
        if (admissions is { Count: > 0 } && (ldPlayer is null || admissions.Any(proof => !settings.LdSerialBindings.TryGetValue(proof.Index, out var serial) || serial != proof.Serial || !settings.AdbAllowedSerials.Contains(proof.Serial, StringComparer.Ordinal))))
            throw new DeviceException("接入凭据与运行时授权不一致。", 409, 1003);
        bool ScriptBusy(string serial) => scripts.GetStatus(serial).State is ScriptRunState.Running or ScriptRunState.Cancelling;
        api.ScriptWriteBusy = ScriptBusy;
        api.AnyScriptWriteBusy = () => catalog.Snapshot.Any(device => ScriptBusy(device.Serial));
        var runtime = new MatrixRuntime
        {
            Backend = backend, Devices = catalog, Automation = automation, Scripts = scripts,
            Input = input, Api = api,
            SettingsStore = store, Settings = settings, FrozenConnectionSettings = settings with { AdbAllowedSerials = settings.AdbAllowedSerials.ToArray(), LdSerialBindings = new(settings.LdSerialBindings) }, Metadata = metadata,
            Log = log, IsDemo = demo, LdPlayer = ldPlayer, LdEditor = ldEditor, LdPlayerError = ldPlayerError
        };
        catalog.DeviceDisconnected += serial => { scripts.Stop(serial); log.Write("device-offline", "Device disconnected; script cancelled", serial); };
        runtime.Api.DeviceSnapshotProvider = runtime.GetDevices;
        scripts.EntryAdded += (serial, entry) => log.Write("script", entry.Message, serial);
        if (demo)
        {
            try { await runtime.RefreshAsync(ct); }
            catch (DeviceException e) { log.Write("discovery-error", e.Message); runtime.DiscoveryError = e.Message; }
        }
        if (ldPlayer is not null)
        {
            try { await ldPlayer.RefreshAsync(ct); }
            catch (OperationCanceledException) { throw; }
            catch (Exception e) { runtime.LdPlayerError = e.Message; log.Write("ldplayer-discovery-error", e.Message); }
            var writeBindings = new Dictionary<int, (string Serial, long Generation)>();
            api.LdSerialForWriteGuard = index => writeBindings.TryGetValue(index, out var binding)
                && ldPlayer.Instances.Any(instance => instance.Index == index && instance.Generation == binding.Generation)
                ? binding.Serial : null;
            if (!demo && backend is AdbDeviceBackend identityBackend)
                api.LdCommandIdentityGuard = async (index, operation, token) =>
                {
                    if (index <= 0) throw new DeviceException("原实例0受保护，未发送雷电命令。", 403, 1003);
                    if (operation is "launch" or "quit" or "reboot") return;
                    var serial = api.LdSerialForWriteGuard(index) ?? throw new DeviceException("请先接入并核验该实例。", 403, 1003);
                    await identityBackend.VerifyIdentityAsync(serial, token).ConfigureAwait(false);
                };
            if (demo)
                foreach (var binding in settings.LdSerialBindings.Where(pair => settings.AdbAllowedSerials.Contains(pair.Value, StringComparer.Ordinal)
                    && settings.LdSerialBindings.Count(other => other.Value == pair.Value) == 1))
                {
                    var instance = ldPlayer.Instances.FirstOrDefault(item => item.Index == binding.Key && item.AndroidStarted);
                    if (instance is not null && catalog.Snapshot.Any(device => device.Serial == binding.Value && device.Online))
                        writeBindings[binding.Key] = (binding.Value, instance.Generation);
                }
            if (!demo && backend is AdbDeviceBackend adbBackend)
            {
                var boundService = ldPlayer;
                foreach (var binding in settings.LdSerialBindings.Where(pair => settings.AdbAllowedSerials.Contains(pair.Value, StringComparer.Ordinal)).GroupBy(pair => pair.Value, StringComparer.Ordinal).Select(group => group.First()))
                {
                    var instance = ldPlayer.Instances.FirstOrDefault(item => item.Index == binding.Key && item.AndroidStarted);
                    var expectedGeneration = instance?.Generation ?? 0;
                    long expectedStarted = 0;
                    var identityReady = false;
                    adbBackend.RegisterIdentityGuard(binding.Value, async ct =>
                    {
                        if (!identityReady || instance is null || !boundService.Instances.Any(row => row.Index == binding.Key && row.Generation == expectedGeneration))
                            throw new DeviceException("雷电连接身份已变化或尚未核验，请在设备管理中重新接入。", 409, 1003);
                        await LdPlayerAdbBinding.VerifyAsync(binding.Value, instance.VBoxProcessId, expectedStarted, ct).ConfigureAwait(false);
                    });
                    if (instance is null || settings.LdSerialBindings.Count(pair => pair.Value == binding.Value) != 1) continue;
                    try
                    {
                        using var process = System.Diagnostics.Process.GetProcessById(instance.VBoxProcessId);
                        expectedStarted = process.StartTime.ToUniversalTime().Ticks;
                        if (process.HasExited) continue;
                    }
                    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
                    { log.Write("ldplayer-binding-error", "Cannot freeze the configured instance process: " + ex.Message, binding.Value); continue; }
                    var proof = admissions?.SingleOrDefault(item => item.Index == binding.Key);
                    if (epochSource is not null && proof is null)
                    {
                        try { await epochSource.VerifyIdentityAsync(binding.Value, ct); }
                        catch (DeviceException ex) { log.Write("ldplayer-binding-error", "Existing authorization was not renewed: " + ex.Message, binding.Value); continue; }
                    }
                    try { await LdPlayerAdbBinding.VerifyAsync(binding.Value, instance.VBoxProcessId, expectedStarted, ct); }
                    catch (DeviceException ex) { log.Write("ldplayer-binding-error", ex.Message, binding.Value); continue; }
                    if (proof is not null && (proof.Serial != binding.Value || proof.Name != instance.Name || proof.VBoxPid != instance.VBoxProcessId || proof.VBoxBirth != expectedStarted))
                        throw new DeviceException("实例在接入期间发生变化，旧授权未替换。", 409, 1003);
                    identityReady = true;
                    writeBindings[binding.Key] = (binding.Value, expectedGeneration);
                    adbBackend.RegisterUnicodeInput(binding.Value, async (text, ct) =>
                    {
                        await LdPlayerAdbBinding.VerifyAsync(binding.Value, instance.VBoxProcessId, expectedStarted, ct).ConfigureAwait(false);
                        await boundService.ExecuteAsync(binding.Key, "input", new System.Text.Json.Nodes.JsonObject
                        { ["expectedGeneration"] = expectedGeneration, ["text"] = text }, ct).ConfigureAwait(false);
                    });
                }
            }
        }
        if (!demo && settings.AdbAllowedSerials.Length > 0)
        {
            try { await runtime.RefreshAsync(ct); }
            catch (DeviceException e) { log.Write("discovery-error", e.Message); runtime.DiscoveryError = e.Message; }
        }
        else if (!demo) runtime.DiscoveryError = "请选择需要接入的本机实例";
        if (settings.AutoStartApi)
        {
            try { await runtime.StartApisAsync(ct); }
            catch (OperationCanceledException) { throw; }
            catch (Exception e) { runtime.ApiStartupError = e.Message; log.Write("api-startup-error", e.Message); }
        }
        return runtime;
        }
        catch
        {
            async Task Cleanup(Func<Task> action) { try { await action(); } catch { /* Preserve the construction error. */ } }
            if (api is not null) await Cleanup(() => api.DisposeAsync().AsTask());
            if (ldPlayer is not null) await Cleanup(() => ldPlayer.DisposeAsync().AsTask());
            if (input is not null) await Cleanup(() => input.DisposeAsync().AsTask());
            if (scripts is not null) await Cleanup(() => scripts.DisposeAsync().AsTask());
            if (backend is not null) await Cleanup(() => backend.DisposeAsync().AsTask());
            metadata?.Dispose();
            if (log is not null) await Cleanup(() => log.DisposeAsync().AsTask());
            throw;
        }
    }
    public async Task RefreshAsync(CancellationToken ct = default)
    {
        try { await Devices.RefreshAsync(ct); DiscoveryError = null; }
        catch (Exception e) { DiscoveryError = e.Message; throw; }
    }
    public void RequireFrozenConnectionSettings()
    {
        var frozen = FrozenConnectionSettings;
        if (Settings.LdConsolePath != frozen.LdConsolePath || Settings.AdbPath != frozen.AdbPath || Settings.CommandTimeoutSeconds != frozen.CommandTimeoutSeconds || Settings.AgentBaseUrl != frozen.AgentBaseUrl || Settings.AgentSerial != frozen.AgentSerial || !Settings.AdbAllowedSerials.SequenceEqual(frozen.AdbAllowedSerials) || !Settings.LdSerialBindings.OrderBy(pair => pair.Key).SequenceEqual(frozen.LdSerialBindings.OrderBy(pair => pair.Key)))
            throw new DeviceException("连接配置已保存但尚未生效，请重新启动矩阵设备后再接入或管理实例。", 409, 1003);
    }
    public async Task RefreshLdPlayerAsync(CancellationToken ct = default)
    {
        if (LdPlayer is null) throw new DeviceException("请先配置雷电命令行程序路径，并重新启动本程序", 503, 2001);
        try { await LdPlayer.RefreshAsync(ct); LdPlayerError = null; }
        catch (Exception e) { LdPlayerError = e.Message; throw; }
    }
    public IReadOnlyList<DeviceInfo> GetDevices() => Devices.Snapshot.Select(device =>
    {
        var metadata = Metadata.GetMetadata(device.Serial);
        return device with { Name = string.IsNullOrWhiteSpace(metadata.Name) ? device.Name : metadata.Name, GroupId = metadata.GroupId };
    }).ToArray();
    public async Task StartApisAsync(CancellationToken ct = default)
    {
        try
        {
            await Api.StartBasicAsync(Settings.BasicApiPort, ct);
            await Api.StartAutomatorAsync(Settings.AutomatorApiPort, ct);
            ApiStartupError = null;
        }
        catch { await Api.StopAsync(CancellationToken.None); throw; }
    }
    public Task StopApisAsync(CancellationToken ct = default) => Api.StopAsync(ct);
    public async ValueTask DisposeAsync()
    {
        List<Exception> errors = [];
        async Task Try(Func<Task> action) { try { await action(); } catch (Exception e) { errors.Add(e); } }
        await Try(() => Api.DisposeAsync().AsTask());
        if (LdPlayer is not null) await Try(() => LdPlayer.DisposeAsync().AsTask());
        await Try(() => Input.DisposeAsync().AsTask());
        await Try(() => Scripts.DisposeAsync().AsTask());
        await Try(() => Backend.DisposeAsync().AsTask());
        try { Metadata.Dispose(); } catch (Exception e) { errors.Add(e); }
        await Try(() => Log.DisposeAsync().AsTask());
        if (errors.Count > 0) throw new AggregateException("Runtime cleanup errors", errors);
    }
}

```

## src/MatrixDevice.Host/packages.lock.json

```json
{
  "version": 1,
  "dependencies": {
    "net10.0": {
      "Microsoft.Data.Sqlite": {
        "type": "Direct",
        "requested": "[10.0.12, )",
        "resolved": "10.0.12",
        "contentHash": "xUq9xUc653ue7ac2rMfCaf8UQN+U3zS8poWYlugycpvJllplMM0Mk7v/+MM0CTlu9zGvbGML4BPH8YWhrUOfGw==",
        "dependencies": {
          "Microsoft.Data.Sqlite.Core": "10.0.12",
          "SQLitePCLRaw.bundle_e_sqlite3": "2.1.12",
          "SQLitePCLRaw.core": "2.1.12"
        }
      },
      "AdvancedSharpAdbClient": {
        "type": "Transitive",
        "resolved": "3.6.16",
        "contentHash": "w5p3s+9Rkx9tmn5S5KU0XSTap5YWGHOvQOw/40M0d7eGOHf7idhX+kizWPmnP1ahbkic/yiX03SMYNR1mJSAVw=="
      },
      "Microsoft.Data.Sqlite.Core": {
        "type": "Transitive",
        "resolved": "10.0.12",
        "contentHash": "2aL9eL5HQr0V4b8V909SD+jQAx6qnTTl7sT/v3D8dIvmr2UGdpZIQxqm1pj2Js3e4EYTvpw0oMH7DjapJJuPPw==",
        "dependencies": {
          "SQLitePCLRaw.core": "2.1.12"
        }
      },
      "SkiaSharp": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "TghYLOUaP3q6rTjQw7U9uQaaXh8lEs9rfA1U7/VtmyAptVDtj/SmjtcWzFWmcC/VMWxe3QgbV9uyvqDL7ySFtQ==",
        "dependencies": {
          "SkiaSharp.NativeAssets.Win32": "4.153.1",
          "SkiaSharp.NativeAssets.macOS": "4.153.1"
        }
      },
      "SkiaSharp.NativeAssets.macOS": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "YTojWeWIhnpF7uC8vpMxB5wRONKkDB2t3r/nNPA3DL2pgj9fqMXRevA/rIanYazWnco4jo1ufgp3HkXuh1Tz8A=="
      },
      "SkiaSharp.NativeAssets.Win32": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "Yqg6rChTVh0BUE8QQxtOA3uUBnihfgID4yex/AvDa0+os2bVcBjmI3Qj9tnOB+R1hd/Jry1Gwl8qw5JI9HZG8Q=="
      },
      "SQLitePCLRaw.bundle_e_sqlite3": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "mAgscpQMLw5/nfA1Q5oJVAT29yROUo1ifZGbbTpx/lwZpSxMUGoYbKfmvdm8oXER+RzxqBmmQzeBEVKfeHv2nw==",
        "dependencies": {
          "SQLitePCLRaw.lib.e_sqlite3": "2.1.12",
          "SQLitePCLRaw.provider.e_sqlite3": "2.1.12"
        }
      },
      "SQLitePCLRaw.core": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "ETpNw9DY3ckWLgRRAeCHj+GKOuPi61aeczkXhgHexUvqoZBAYg8RYESE2J7O1M7+o6QbdSEZwrw9bfqztUVWXg=="
      },
      "SQLitePCLRaw.lib.e_sqlite3": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "fWi8Dbknuhgg72fWinIdjXVaqO1hHL4YBBwVLnr7e1c9TAZwJ0QE38j9syW1hwx6HaqEVTwI+O07WPdZn8Rp0w=="
      },
      "SQLitePCLRaw.provider.e_sqlite3": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "W3oH4XIfCzFrgUSDKHhN6N+dgzA5YHOR2VxX8GB6Qy7CyrJJgxPEG8NirgYWlPQC5P2jz2knSsexWu4tDUL33g==",
        "dependencies": {
          "SQLitePCLRaw.core": "2.1.12"
        }
      },
      "matrixdevice.core": {
        "type": "Project",
        "dependencies": {
          "AdvancedSharpAdbClient": "[3.6.16, )",
          "SkiaSharp": "[4.153.1, )"
        }
      },
      "matrixdevice.features": {
        "type": "Project",
        "dependencies": {
          "MatrixDevice.Core": "[1.0.0, )"
        }
      }
    }
  }
}
```

## src/MatrixDevice.Host/Persistence/CatalogRepository.cs

```csharp
using System.Text.Json;
using Microsoft.Data.Sqlite;
using MatrixDevice.Core;

namespace MatrixDevice.Host;

public sealed record DeviceMetadata(string Serial, string Name = "", string GroupId = "", string Remark = "");
public sealed record GroupRecord(string Id, string Name);

public sealed class CatalogRepository : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly object _gate = new();
    private static readonly HashSet<string> Categories = ["Group", "DeviceGroupRelation", "DeviceRemarkRelation", "EthernetScan", "CustomDeviceCommand", "ScriptDeviceConfiguration", "EnvironmentConfig", "Proxy", "Server", "AppCk", "User"];
    public CatalogRepository(string path)
    {
        path = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path, Pooling = false }.ToString());
        try
        {
        _connection.Open();
        using var integrity = _connection.CreateCommand();
        integrity.CommandText = "PRAGMA integrity_check;";
        if (!string.Equals(integrity.ExecuteScalar()?.ToString(), "ok", StringComparison.OrdinalIgnoreCase))
            throw new IOException("SQLite integrity check failed: " + path);
        using var version = _connection.CreateCommand();
        version.CommandText = "PRAGMA user_version;";
        var previous = Convert.ToInt32(version.ExecuteScalar());
        if (previous > 1) throw new IOException("Unsupported database schema version: " + previous);
        if (previous == 0)
        {
            if (new FileInfo(path).Length > 0)
            {
                using var backup = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path + ".pre-migration.bak", Pooling = false }.ToString());
                backup.Open();
                _connection.BackupDatabase(backup);
            }
            using var transaction = _connection.BeginTransaction();
            foreach (var category in Categories)
            {
                using var create = _connection.CreateCommand();
                create.Transaction = transaction;
                create.CommandText = $"CREATE TABLE IF NOT EXISTS [{category}] (Id TEXT PRIMARY KEY NOT NULL, Payload TEXT NOT NULL, UpdatedUtc TEXT NOT NULL);";
                create.ExecuteNonQuery();
            }
            using var migration = _connection.CreateCommand();
            migration.Transaction = transaction;
            migration.CommandText = "CREATE TABLE IF NOT EXISTS DeviceMetadata(Serial TEXT PRIMARY KEY, Name TEXT NOT NULL, GroupId TEXT NOT NULL, Remark TEXT NOT NULL); PRAGMA user_version=1;";
            migration.ExecuteNonQuery();
            transaction.Commit();
        }
        }
        catch { _connection.Dispose(); throw; }
    }

    public DeviceMetadata GetMetadata(string serial)
    {
        lock (_gate)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT Serial, Name, GroupId, Remark FROM DeviceMetadata WHERE Serial=$serial;";
            command.Parameters.AddWithValue("$serial", serial);
            using var reader = command.ExecuteReader();
            return reader.Read() ? new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)) : new(serial);
        }
    }
    public IReadOnlyList<DeviceMetadata> GetAllMetadata()
    {
        lock (_gate)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT Serial, Name, GroupId, Remark FROM DeviceMetadata ORDER BY Serial;";
            using var reader = command.ExecuteReader();
            List<DeviceMetadata> rows = [];
            while (reader.Read()) rows.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
            return rows;
        }
    }
    public void UpsertMetadata(string serial, string name, string groupId)
    {
        if (string.IsNullOrWhiteSpace(serial)) throw new ArgumentException("Serial is required", nameof(serial));
        lock (_gate)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "INSERT INTO DeviceMetadata VALUES($serial,$name,$group,'') ON CONFLICT(Serial) DO UPDATE SET Name=excluded.Name,GroupId=excluded.GroupId;";
            command.Parameters.AddWithValue("$serial", serial);
            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$group", groupId);
            command.ExecuteNonQuery();
        }
    }
    public void SetRemark(string serial, string remark)
    {
        lock (_gate)
        {
            var old = GetMetadata(serial);
            UpsertMetadata(serial, old.Name, old.GroupId);
            using var command = _connection.CreateCommand();
            command.CommandText = "UPDATE DeviceMetadata SET Remark=$remark WHERE Serial=$serial;";
            command.Parameters.AddWithValue("$remark", remark);
            command.Parameters.AddWithValue("$serial", serial);
            command.ExecuteNonQuery();
        }
    }
    public void Put<T>(string category, string id, T value)
    {
        ValidateCategory(category);
        lock (_gate)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = $"INSERT INTO [{category}] VALUES($id,$payload,$at) ON CONFLICT(Id) DO UPDATE SET Payload=excluded.Payload,UpdatedUtc=excluded.UpdatedUtc;";
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$payload", JsonSerializer.Serialize(value, JsonDefaults.Options));
            command.Parameters.AddWithValue("$at", DateTimeOffset.UtcNow.ToString("O"));
            command.ExecuteNonQuery();
        }
    }
    public IReadOnlyList<T> GetAll<T>(string category)
    {
        ValidateCategory(category);
        lock (_gate)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = $"SELECT Payload FROM [{category}] ORDER BY Id;";
            using var reader = command.ExecuteReader();
            List<T> rows = [];
            while (reader.Read()) rows.Add(JsonSerializer.Deserialize<T>(reader.GetString(0), JsonDefaults.Options)!);
            return rows;
        }
    }
    public void Delete(string category, string id)
    {
        ValidateCategory(category);
        lock (_gate)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = $"DELETE FROM [{category}] WHERE Id=$id;";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }
    }
    private static void ValidateCategory(string category)
    {
        if (!Categories.Contains(category)) throw new ArgumentException("Unknown entity category: " + category);
    }
    public void Dispose() => _connection.Dispose();
}

```

## src/MatrixDevice.Host/Persistence/SettingsStore.cs

```csharp
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Core;

namespace MatrixDevice.Host;

public sealed class SettingsStore(string path)
{
    private static readonly JsonSerializerOptions MergeJson = new(JsonDefaults.Options) { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never };
    private readonly string _path = Path.GetFullPath(path);
    private AppSettings? _baseline;
    public List<string> Warnings { get; } = [];

    public AppSettings Load()
    {
        Warnings.Clear();
        var settings = Read(_path);
        if (settings is null)
        {
            settings = Read(_path + ".bak");
            if (settings is not null)
            {
                settings = settings with { AdbAllowedSerials = [], LdSerialBindings = [], AgentSerial = null, AgentBaseUrl = null };
                Warnings.Add("已从备份恢复一般设置；设备授权与雷电绑定已清空，请重新确认。");
            }
        }
        settings ??= new AppSettings();
        var normalized = Normalize(settings);
        _baseline = Normalize(normalized);
        return normalized;
    }

    public AppSettings Save(AppSettings current)
    {
        ArgumentNullException.ThrowIfNull(current);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var name = "MatrixDevice.Settings." + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(_path.ToUpperInvariant())));
        using var gate = new Mutex(false, name);
        var acquired = false;
        try
        {
            try { acquired = gate.WaitOne(TimeSpan.FromSeconds(10)); }
            catch (AbandonedMutexException) { acquired = true; }
            if (!acquired) throw new IOException("Settings file is busy: " + _path);
            var disk = Read(_path);
            var merged = current;
            if (_baseline is not null && disk is not null)
            {
                var baselineJson = JsonSerializer.SerializeToNode(_baseline, MergeJson)!.AsObject();
                var localJson = JsonSerializer.SerializeToNode(current, MergeJson)!.AsObject();
                var diskJson = JsonSerializer.SerializeToNode(disk, MergeJson)!.AsObject();
                foreach (var property in localJson)
                {
                    if (property.Key == "ldSerialBindings" && property.Value is JsonObject localBindings && baselineJson[property.Key] is JsonObject originalBindings)
                    {
                        var bindings = diskJson[property.Key] as JsonObject;
                        if (bindings is null) { bindings = new JsonObject(); diskJson[property.Key] = bindings; }
                        foreach (var key in originalBindings.Select(x => x.Key).Union(localBindings.Select(x => x.Key)).ToArray())
                            if (!JsonNode.DeepEquals(localBindings[key], originalBindings[key]))
                            {
                                if (localBindings.TryGetPropertyValue(key, out var changed)) bindings[key] = changed?.DeepClone();
                                else bindings.Remove(key);
                            }
                    }
                    else if (!JsonNode.DeepEquals(property.Value, baselineJson[property.Key]))
                        diskJson[property.Key] = property.Value?.DeepClone();
                }
                merged = diskJson.Deserialize<AppSettings>(JsonDefaults.Options)!;
            }
            merged = Normalize(merged);
            var temp = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    JsonSerializer.Serialize(stream, merged, JsonDefaults.Options);
                    stream.Flush(true);
                }
                if (File.Exists(_path)) File.Replace(temp, _path, _path + ".bak", true);
                else File.Move(temp, _path);
                _baseline = Normalize(merged);
                return merged;
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
        finally { if (acquired) gate.ReleaseMutex(); }
    }

    private AppSettings? Read(string file)
    {
        if (!File.Exists(file)) return null;
        try
        {
            return Normalize(JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(file, Encoding.UTF8), JsonDefaults.Options)
                ?? throw new JsonException("Configuration is null"));
        }
        catch (Exception e) when (e is IOException or JsonException or ArgumentException or NotSupportedException)
        {
            Warnings.Add(file + ": " + e.Message);
            return null;
        }
    }

    public static AppSettings Normalize(AppSettings value) => value with
    {
        AdbAllowedSerials = NormalizeAuthorizedSerials(value.AdbAllowedSerials),
        BasicApiPort = value.BasicApiPort is >= 1 and <= 65535 ? value.BasicApiPort : 8080,
        AutomatorApiPort = value.AutomatorApiPort is >= 1 and <= 65535 ? value.AutomatorApiPort : 17171,
        WallColumns = Math.Clamp(value.WallColumns, 5, 20),
        MainMaxSize = Math.Clamp(value.MainMaxSize, 180, 4096),
        MiniMaxSize = Math.Clamp(value.MiniMaxSize, 180, 4096),
        MainMaxFps = Math.Clamp(value.MainMaxFps, 1, 120),
        MiniMaxFps = Math.Clamp(value.MiniMaxFps, 1, 120),
        WindowWidth = double.IsFinite(value.WindowWidth) ? Math.Clamp(value.WindowWidth, 1100, 7680) : 1440,
        WindowHeight = double.IsFinite(value.WindowHeight) ? Math.Clamp(value.WindowHeight, 720, 4320) : 900,
        CommandTimeoutSeconds = Math.Clamp(value.CommandTimeoutSeconds, 1, 300),
        LdSerialBindings = (value.LdSerialBindings ?? []).Where(x => x.Key >= 0 && !string.IsNullOrWhiteSpace(x.Value))
            .ToDictionary(x => x.Key, x => x.Value.Trim()),
        Language = "zh-CN"
    };
    private static string[] NormalizeAuthorizedSerials(string[]? serials)
    {
        if (serials is null) return [];
        if (serials.Any(serial => string.IsNullOrWhiteSpace(serial) || serial.StartsWith('-') || serial.Any(char.IsWhiteSpace) || serial.Contains('\0')))
            throw new JsonException("授权设备串号包含无效值，拒绝此配置");
        return serials.Distinct(StringComparer.Ordinal).ToArray();
    }
}

```

## tests/MatrixDevice.Tests/ExistingAdbTests.cs

```csharp
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Globalization;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MatrixDevice.Core;

internal static class ExistingAdbTests
{
    public static void Register(Action<string, Func<Task>> test)
    {
        test("existing ADB process replacement cannot reuse old transport or receive a request", ProcessReplacementAsync);
        test("host ADB registration inventory never authorizes or queries guests", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings(), server.Endpoint);
            var registered = await backend.ListRegisteredDevicesAsync();
            Assert(registered.Any(item => item.Serial == "allowed" && item.TransportId > 0), "Host inventory missing registration.");
            Assert(server.Requests.All(request => request is "host:version" or "host:devices-l"), "Inventory queried guest.");
            await Expect(403, () => backend.KeyEventAsync("allowed", 3));
        });
        test("frozen identity guard rejects ASCII files screenshots apps and Unicode before guest I/O", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
            var valid = true; var unicode = 0;
            backend.RegisterIdentityGuard("allowed", _ => valid ? Task.CompletedTask : Task.FromException(new DeviceException("rejoin required", 409, 1003)));
            backend.RegisterUnicodeInput("allowed", (_, _) => { unicode++; return Task.CompletedTask; });
            await backend.DiscoverAsync();
            valid = false;
            var count = server.Requests.Count;
            var local = typeof(ExistingAdbTests).Assembly.Location;
            foreach (var operation in new Func<Task>[] { () => backend.KeyEventAsync("allowed", 3), () => backend.TextAsync("allowed", "ASCII"), () => backend.TextAsync("allowed", "中文"), () => backend.ScreenshotAsync("allowed"), () => backend.PushAsync("allowed", local, "/sdcard/test.bin"), () => backend.InstallAsync("allowed", local), () => backend.VerifyIdentityAsync("allowed") })
                await Expect(409, operation);
            Assert(unicode == 0 && server.Requests.Count == count, "Identity rejection reached guest/Unicode callback.");
            var rows = await backend.DiscoverAsync();
            Assert(rows.Single().State == "Failed", "Changed identity remained controllable.");
        });
        test("connection epoch inheritance requires an observed server and preserves its identity", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var source = new AdbDeviceBackend(new AppSettings(), server.Endpoint);
            await using var next = new AdbDeviceBackend(new AppSettings(), server.Endpoint);
            await Expect(409, () => { next.InheritServerEpoch(source); return Task.CompletedTask; });
            await source.ListRegisteredDevicesAsync(); next.InheritServerEpoch(source);
            Assert((await next.ListRegisteredDevicesAsync()).Count > 0, "Observed epoch was not reusable.");
        });
        test("LDPlayer binding candidate must belong to the expected process and sends no ADB request", async () =>
        {
            await using var peer = new FakeAdbServer(); using var process = Process.GetCurrentProcess();
            var serial = "127.0.0.1:" + peer.Endpoint.Port;
            await LdPlayerAdbBinding.VerifyAsync(serial, process.Id, process.StartTime.ToUniversalTime().Ticks);
            await Expect(409, () => LdPlayerAdbBinding.VerifyAsync(serial, process.Id, process.StartTime.ToUniversalTime().Ticks + 1));
            await Expect(409, () => LdPlayerAdbBinding.VerifyAsync(serial, int.MaxValue, process.StartTime.ToUniversalTime().Ticks));
            await Expect(501, () => LdPlayerAdbBinding.VerifyAsync("USB-unknown", process.Id, process.StartTime.ToUniversalTime().Ticks));
            Assert(peer.Requests.IsEmpty, "Binding probe sent an ADB or guest command.");
        });
        test("explicit Unicode input binding requires a discovered transport and verified server before callback", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
            var calls = 0; string? sent = null;
            backend.RegisterUnicodeInput("allowed", (text, ct) => { ct.ThrowIfCancellationRequested(); sent = text; calls++; return Task.CompletedTask; });
            await Expect(503, () => backend.TextAsync("allowed", "中文 测试")); Assert(calls == 0, "Cold input callback executed.");
            await backend.DiscoverAsync(); await backend.TextAsync("allowed", "中文 测试");
            Assert(calls == 1 && sent == "中文 测试" && server.Requests.Last() == "shell,v2,raw::", "Bound input did not verify the pinned connection first.");
            await Expect(400, () => backend.TextAsync("allowed", "中文\n测试")); Assert(calls == 1, "Invalid input callback executed.");
            server.InvalidIdentity = "absent"; await backend.DiscoverAsync();
            await Expect(503, () => backend.TextAsync("allowed", "中文")); Assert(calls == 1, "Missing target callback executed.");
        });
        test("UI dump without a confirmed file fails before parsing and cleans only its own path", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
            await backend.DiscoverAsync();
            await Expect(409, () => backend.DumpAsync("allowed"));
            Assert(!server.Requests.Any(request => request.StartsWith("shell,v2,raw:cat ", StringComparison.Ordinal)), "Unconfirmed UI file was read.");
            Assert(server.Requests.Any(request => request.StartsWith("shell,v2,raw:rm -f '/data/local/tmp/matrix-ui-", StringComparison.Ordinal)), "Owned dump file cleanup missing.");
        });
        test("ADB activity start does not require monkey and rejects semantic launch failure", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
            await backend.DiscoverAsync();
            await backend.StartAppAsync("allowed", "com.matrix.test");
            Assert(server.Requests.Any(request => request.Contains("-n com.matrix.test/.LauncherAlias", StringComparison.Ordinal)) && !server.Requests.Any(request => request.Contains("monkey", StringComparison.Ordinal)), "Launcher did not resolve the explicit alias.");
            server.LaunchOutput = "Starting: Intent\nError: Activity not found\n";
            await Expect(409, () => backend.StartAppAsync("allowed", "com.matrix.test"));
            server.LaunchOutput = "Status: timeout\n";
            await Expect(409, () => backend.StartAppAsync("allowed", "com.matrix.test"));
            server.LaunchOutput = "Starting: Intent\n";
            await Expect(502, () => backend.StartAppAsync("allowed", "com.matrix.test", ".Main"));
        });
        test("existing ADB server epoch rejects PID and start-time reuse before any guest request", async () =>
        {
            var epoch = new AdbServerEpoch();
            epoch.Require(new(42, 100)); epoch.Require(new(42, 100));
            await Expect(409, () => { epoch.Require(new(42, 101)); return Task.CompletedTask; });
            await Expect(409, () => { epoch.Require(new(43, 100)); return Task.CompletedTask; });
            epoch.Require(new(42, 100));
        });
        test("existing ADB reconnect pins queued transport and invalidates catalog generation", ReconnectAsync);
        test("existing ADB unknown invalid offline and missing identities never execute guest writes", InvalidIdentityAsync);
        test("existing ADB discovery touches only authorized guest and preserves binary PNG", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
            var devices = await backend.DiscoverAsync();
            Assert(devices.Count == 1 && devices[0].Serial == "allowed" && devices[0].Width == 4 && devices[0].Height == 3, "Discovery did not filter or parse authorized device");
            var png = await backend.ScreenshotAsync("allowed");
            Assert(png.SequenceEqual(server.Png), "Binary screenshot was transformed");
            await Expect(403, () => backend.KeyEventAsync("private", 3));
            await Expect(403, () => backend.SetNodeTextAsync("private", new(), "private"));
            Assert(!server.Requests.Any(request => request.Contains("private", StringComparison.Ordinal)), "Unauthorized guest was queried");
            Assert(!server.Requests.Any(request => request.Contains("kill", StringComparison.Ordinal) || request.Contains("start-server", StringComparison.Ordinal)), "Server control request emitted");
        });
        test("existing ADB empty authorization never queries guest and wrong version stays untouched", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var empty = new AdbDeviceBackend(new AppSettings(), server.Endpoint);
            Assert((await empty.DiscoverAsync()).Count == 0, "Empty allowlist exposed guest");
            Assert(server.Requests.All(request => request is "host:version" or "host:devices-l"), "Empty allowlist queried guest");
            server.Version = "0028";
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
            await Expect(503, () => backend.DiscoverAsync());
            Assert(server.Requests.Last() == "host:version", "Version rejection emitted a device/server write");
        });
        test("missing existing ADB server fails without executing configured binary", async () =>
        {
            var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start(); var endpoint = (IPEndPoint)listener.LocalEndpoint; listener.Stop();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbPath = Environment.ProcessPath!, AdbAllowedSerials = ["allowed"] }, endpoint);
            await Expect(503, () => backend.DiscoverAsync());
        });
        test("existing ADB shell checks exit codes truncation output bounds and recovers after cancellation", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"], CommandTimeoutSeconds = 1 }, server.Endpoint);
            await backend.DiscoverAsync();
            await Expect(503, () => backend.ShellAsync("allowed", "fail"));
            await Expect(503, () => backend.ShellAsync("allowed", "truncated"));
            await Expect(413, () => backend.ShellAsync("allowed", "huge"));
            await Expect(504, () => backend.ShellAsync("allowed", "blocked"));
            using var cancel = new CancellationTokenSource(50);
            try { await backend.ShellAsync("allowed", "blocked", ct: cancel.Token); throw new Exception("Cancellation accepted"); }
            catch (OperationCanceledException) when (cancel.IsCancellationRequested) { }
            Assert((await backend.ShellAsync("allowed", "wm size")).Contains("4x3", StringComparison.Ordinal), "Queue failed to recover");
        });
        test("existing ADB sync roundtrip is byte exact and failed pull cannot replace local file", async () =>
        {
            await using var server = new FakeAdbServer();
            await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
            await backend.DiscoverAsync();
            var directory = Path.Combine(Path.GetTempPath(), "MatrixDevice.Tests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
            var upload = Path.Combine(directory, "upload.bin"); var download = Path.Combine(directory, "download.bin");
            var bytes = Enumerable.Range(0, 200000).Select(index => (byte)(index % 251)).ToArray();
            await File.WriteAllBytesAsync(upload, bytes);
            await backend.PushAsync("allowed", upload, "/sdcard/test.bin");
            await backend.PullAsync("allowed", "/sdcard/test.bin", download);
            Assert((await File.ReadAllBytesAsync(download)).SequenceEqual(bytes), "Sync content differs");
            server.TruncatePull = true;
            await Expect(503, () => backend.PullAsync("allowed", "/sdcard/test.bin", download));
            Assert((await File.ReadAllBytesAsync(download)).SequenceEqual(bytes), "Failed pull replaced local file");
            Assert(!Directory.EnumerateFiles(directory, "*.matrix-part-*").Any(), "Partial files leaked");
            server.TruncateStat = true;
            await Expect(503, () => backend.PullAsync("allowed", "/sdcard/test.bin", download));
            server.TruncateStat = false; server.TruncatePull = false; server.EarlyDone = true;
            await Expect(502, () => backend.PullAsync("allowed", "/sdcard/test.bin", download));
            Assert((await File.ReadAllBytesAsync(download)).SequenceEqual(bytes) && !Directory.EnumerateFiles(directory, "*.matrix-part-*").Any(), "Early DONE replaced or leaked local data.");
        });
    }

    private static async Task ProcessReplacementAsync()
    {
        var directory = Path.Combine(Path.GetTempPath(), "MatrixDevice.Tests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        async Task<Process> Start(int port, string receipt)
        {
            var info = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardOutput = true, RedirectStandardError = true };
            if (string.Equals(Path.GetFileNameWithoutExtension(Environment.ProcessPath), "dotnet", StringComparison.OrdinalIgnoreCase)) info.ArgumentList.Add(typeof(ExistingAdbTests).Assembly.Location);
            foreach (var value in new[] { "--adb-owned-test-server", port.ToString(CultureInfo.InvariantCulture), receipt }) info.ArgumentList.Add(value);
            var child = Process.Start(info)!;
            try { var ready = await child.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10)); Assert(int.TryParse(ready, out var actual) && actual == port, "Owned fixture did not become ready."); return child; }
            catch { if (!child.HasExited) child.Kill(false); await child.WaitForExitAsync(); child.Dispose(); throw; }
        }
        async Task Stop(Process child) { if (!child.HasExited) child.Kill(false); await child.WaitForExitAsync(); child.Dispose(); }
        var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start(); var port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop();
        var first = await Start(port, Path.Combine(directory, "first.txt")); Process? second = null;
        try
        {
            await using var old = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, new(IPAddress.Loopback, port));
            await old.DiscoverAsync(); await Stop(first); first = null!;
            var receipt = Path.Combine(directory, "second.txt"); second = await Start(port, receipt);
            await Expect(409, () => old.KeyEventAsync("allowed", 3)); await Expect(409, () => old.DiscoverAsync());
            await using var inherited = new AdbDeviceBackend(new AppSettings(), new(IPAddress.Loopback, port));
            inherited.InheritServerEpoch(old);
            await Expect(409, () => inherited.ListRegisteredDevicesAsync());
            Assert(!File.Exists(receipt) || new FileInfo(receipt).Length == 0, "Replacement server received a request from the old client.");
            await using var fresh = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, new(IPAddress.Loopback, port));
            Assert((await fresh.DiscoverAsync()).Single().Online, "Fresh discovery could not establish a new epoch.");
        }
        finally { if (first is not null) await Stop(first); if (second is not null) await Stop(second); }
    }
    private static async Task InvalidIdentityAsync()
    {
        await using var server = new FakeAdbServer();
        await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
        var handler = new CaptureHandler();
        backend.RegisterAgent("allowed", new PhoneAgentClient("http://127.0.0.1:9999", handler));
        await Expect(503, () => backend.KeyEventAsync("allowed", 3));
        await Expect(503, () => backend.SetNodeTextAsync("allowed", new(), "text"));
        Assert(server.Requests.Count == 0 && handler.Path.Length == 0, "Cold guest action performed I/O");
        foreach (var identity in new[] { "", "0", "4294967296", "invalid", "offline", "absent" })
        {
            server.InvalidIdentity = null; await backend.DiscoverAsync();
            server.InvalidIdentity = identity;
            var devices = await backend.DiscoverAsync();
            Assert(devices.All(device => !device.Online), "Invalid identity remained Ready");
            var requests = server.Requests.Count;
            await Expect(503, () => backend.KeyEventAsync("allowed", 3));
            await Expect(503, () => backend.SetNodeTextAsync("allowed", new(), "text"));
            Assert(server.Requests.Count == requests && handler.Path.Length == 0, "Invalidated guest identity performed I/O");
        }
    }
    private static async Task ReconnectAsync()
    {
        await using var server = new FakeAdbServer();
        await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = ["allowed"] }, server.Endpoint);
        var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
        var generation = catalog.GetRequired("allowed").Generation;
        var blocked = backend.ShellAsync("allowed", "gated"); await server.GateEntered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var key = backend.KeyEventAsync("allowed", 3);
        server.TransportId = 2;
        var refresh = catalog.RefreshAsync();
        try
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            while (backend.ObservedTransportId("allowed") != 2)
            {
                if (timer.Elapsed > TimeSpan.FromSeconds(2)) throw new TimeoutException("New transport not observed");
                await Task.Delay(5);
            }
        }
        finally { server.GateRelease.TrySetResult(); }
        await blocked;
        await Expect(409, () => key);
        await refresh;
        Assert(catalog.GetRequired("allowed").Generation > generation, "Transport reconnect retained generation");
        Assert(!server.Requests.Any(request => request.Contains("input keyevent", StringComparison.Ordinal) || request.StartsWith("host:transport:", StringComparison.Ordinal)), "Stale input or serial fallback emitted");
    }
    private static void Assert(bool value, string message) { if (!value) throw new Exception(message); }
    private static async Task Expect(int status, Func<Task> action)
    {
        try { await action(); }
        catch (DeviceException ex) when (ex.Status == status) { return; }
        throw new Exception($"Expected DeviceException status={status}");
    }
}

internal sealed class FakeAdbServer : IAsyncDisposable
{
    private readonly TcpListener listener;
    private readonly string? receipt;
    private readonly CancellationTokenSource stop = new();
    private readonly ConcurrentBag<Task> clients = [];
    private readonly Task accepting;
    public ConcurrentQueue<string> Requests { get; } = [];
    public string Version { get; set; } = "0029";
    public string LaunchOutput { get; set; } = "Starting: Intent\nStatus: ok\n";
    public uint TransportId { get; set; } = 1;
    public string? InvalidIdentity { get; set; }
    public TaskCompletionSource GateEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource GateRelease { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public bool TruncatePull { get; set; }
    public bool TruncateStat { get; set; }
    public bool EarlyDone { get; set; }
    public byte[] Png { get; } = PngImage.Create(4, 3, (x, y) => ((byte)(x * 20), (byte)(y * 40), (byte)90));
    private byte[] file = [];
    public IPEndPoint Endpoint => (IPEndPoint)listener.LocalEndpoint;
    public FakeAdbServer(int port = 0, string? receipt = null) { this.receipt = receipt; listener = new(IPAddress.Loopback, port); listener.Start(); accepting = AcceptAsync(); }
    private async Task AcceptAsync()
    {
        try
        {
            while (!stop.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(stop.Token);
                clients.Add(HandleAsync(client));
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) when (stop.IsCancellationRequested) { }
    }
    private async Task HandleAsync(TcpClient client)
    {
        using (client)
        {
            try
            {
                var stream = client.GetStream();
                while (!stop.IsCancellationRequested)
                {
                    var prefix = new byte[4]; await stream.ReadExactlyAsync(prefix, stop.Token);
                    var length = int.Parse(Encoding.ASCII.GetString(prefix), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    var body = new byte[length]; await stream.ReadExactlyAsync(body, stop.Token);
                    var request = Encoding.UTF8.GetString(body); Requests.Enqueue(request);
                    if (receipt is not null) await File.AppendAllTextAsync(receipt, request + "\n", stop.Token);
                    if (request == "host:version") { await HostAsync(stream, Version); return; }
                    if (request == "host:devices-l")
                    {
                        var allowed = InvalidIdentity == "absent" ? "" :
                            $"allowed\t{(InvalidIdentity == "offline" ? "offline" : "device")} model:Test" +
                            (InvalidIdentity == "" ? "" : " transport_id:" + (InvalidIdentity is null or "offline" ? TransportId.ToString(CultureInfo.InvariantCulture) : InvalidIdentity)) + "\n";
                        await HostAsync(stream, allowed + "private\tdevice model:Private transport_id:99\n"); return;
                    }
                    if (request.StartsWith("host:transport-id:", StringComparison.Ordinal)) { await stream.WriteAsync("OKAY"u8.ToArray(), stop.Token); continue; }
                    await stream.WriteAsync("OKAY"u8.ToArray(), stop.Token);
                    if (request == "sync:") { await SyncAsync(stream); return; }
                    if (request.StartsWith("exec:", StringComparison.Ordinal)) { await stream.WriteAsync(Png, stop.Token); return; }
                    if (!request.StartsWith("shell,v2,raw:", StringComparison.Ordinal)) throw new IOException("Unexpected fixture request: " + request);
                    var closeInput = new byte[5]; await stream.ReadExactlyAsync(closeInput, stop.Token);
                    var command = request[13..];
                    if (command == "gated") { GateEntered.TrySetResult(); await GateRelease.Task.WaitAsync(stop.Token); }
                    if (command == "blocked") { await Task.Delay(Timeout.Infinite, stop.Token); return; }
                    if (command == "truncated") { await stream.WriteAsync(new byte[] { 1, 20, 0, 0, 0, 1 }, stop.Token); return; }
                    if (command == "huge") { var header = new byte[5]; header[0] = 1; BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(1), AdbDeviceBackend.MaximumStdoutBytes + 1); await stream.WriteAsync(header, stop.Token); return; }
                    var text = command.StartsWith("cmd package resolve-activity ", StringComparison.Ordinal) ? "priority=0 isDefault=false\ncom.matrix.test/.LauncherAlias\n" : command.StartsWith("am start -W ", StringComparison.Ordinal) ? LaunchOutput : command switch { "getprop" => "[ro.product.brand]: [Fixture]\n[ro.product.model]: [Test]\n[ro.build.version.release]: [14]\n", "wm size" => "Physical size: 4x3\n", "dumpsys input" => "SurfaceOrientation: 0\n", _ => "fixture\n" };
                    await ShellPacketAsync(stream, command == "fail" ? (byte)2 : (byte)1, Encoding.UTF8.GetBytes(text));
                    await ShellPacketAsync(stream, 3, [command == "fail" ? (byte)7 : (byte)0]);
                    return;
                }
            }
            catch (Exception ex) when (ex is IOException or SocketException or OperationCanceledException) { }
        }
    }
    private async Task HostAsync(Stream stream, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        await stream.WriteAsync(Encoding.ASCII.GetBytes("OKAY" + bytes.Length.ToString("X4", CultureInfo.InvariantCulture)), stop.Token);
        await stream.WriteAsync(bytes, stop.Token);
    }
    private async Task ShellPacketAsync(Stream stream, byte channel, byte[] value)
    {
        var header = new byte[5]; header[0] = channel; BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(1), value.Length);
        await stream.WriteAsync(header, stop.Token); await stream.WriteAsync(value, stop.Token);
    }
    private async Task SyncAsync(Stream stream)
    {
        using var upload = new MemoryStream();
        while (!stop.IsCancellationRequested)
        {
            var header = new byte[8]; await stream.ReadExactlyAsync(header, stop.Token);
            var command = Encoding.ASCII.GetString(header, 0, 4); var length = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(4));
            if (command == "DONE") { file = upload.ToArray(); await SyncPacketAsync(stream, "OKAY", []); continue; }
            if (command == "QUIT") return;
            var bytes = new byte[length]; await stream.ReadExactlyAsync(bytes, stop.Token);
            if (command == "DATA") { await upload.WriteAsync(bytes, stop.Token); continue; }
            if (command == "SEND") continue;
            if (command == "STAT")
            {
                if (TruncateStat) { await stream.WriteAsync("STAT"u8.ToArray(), stop.Token); return; }
                var response = new byte[16]; "STAT"u8.CopyTo(response); BinaryPrimitives.WriteInt32LittleEndian(response.AsSpan(4), 33206); BinaryPrimitives.WriteInt32LittleEndian(response.AsSpan(8), file.Length);
                await stream.WriteAsync(response, stop.Token); continue;
            }
            if (command == "RECV")
            {
                if (EarlyDone) { await SyncPacketAsync(stream, "DATA", file.Take(100).ToArray()); await SyncPacketAsync(stream, "DONE", []); continue; }
                for (var offset = 0; offset < file.Length; offset += 65536)
                    await SyncPacketAsync(stream, "DATA", file.AsSpan(offset, Math.Min(65536, file.Length - offset)).ToArray());
                if (TruncatePull) { await stream.WriteAsync("DONE"u8.ToArray(), stop.Token); return; }
                await SyncPacketAsync(stream, "DONE", []); continue;
            }
            throw new IOException("Unexpected sync command: " + command);
        }
    }
    private async Task SyncPacketAsync(Stream stream, string command, byte[] value)
    {
        var header = new byte[8]; Encoding.ASCII.GetBytes(command).CopyTo(header, 0); BinaryPrimitives.WriteInt32LittleEndian(header.AsSpan(4), value.Length);
        await stream.WriteAsync(header, stop.Token); await stream.WriteAsync(value, stop.Token);
    }
    public async ValueTask DisposeAsync()
    {
        stop.Cancel(); listener.Stop(); await accepting;
        await Task.WhenAll(clients); stop.Dispose();
    }
}

```

## tests/MatrixDevice.Tests/LdWorkspaceTests.cs

```csharp
using System.Globalization;
using MatrixDevice.Core;

public static class LdWorkspaceTests
{
    public static void Register(Action<string, Func<Task>> test)
    {
        test("LD workspace launch validates the whole batch before touching the console", LaunchValidationAsync);
        test("LD workspace launch validates the complete confirmation snapshot before console calls", ConfirmationValidationAsync);
        test("LD workspace launch rejects replacement after user confirmation before launching", ConfirmationReplacementAsync);
        test("LD workspace launch starts once per instance and waits sequentially", SequentialLaunchAsync);
        test("LD workspace launch skips ready instances and waits for existing processes", ExistingProcessesAsync);
        test("LD workspace launch timeout never retries or starts later instances", LaunchTimeoutAsync);
        test("LD workspace launch cancellation leaves launched instances alone", LaunchCancellationAsync);
        test("LD workspace launch resource rejection stops the remaining batch", ResourceRejectionAsync);
        test("LD workspace launch rejects changed generations after the resource callback", ResourceGenerationAsync);
        test("LD workspace launch rejects changed instance identity while waiting", LaunchIdentityAsync);
        test("LD workspace coordinator serializes overlapping batches", OverlappingBatchesAsync);
        test("LD workspace create validates names before console calls", CreateValidationAsync);
        test("LD workspace create confirms one new index and never copies or deletes", CreateAsync);
        test("LD workspace create retains a nonzero command warning after unique confirmation", CreateReportedFailureAsync);
        test("LD workspace create rejects ambiguous changes without retrying", CreateAmbiguityAsync);
        test("LD workspace create does not swallow cancellation or command timeouts", CreateInterruptionAsync);
        test("LD workspace configure validates all resource values before console calls", ConfigureValidationAsync);
        test("LD workspace configure requires fully stopped and current instances", ConfigurePreconditionsAsync);
        test("LD workspace configure sends only whitelisted resource parameters", ConfigureAsync);
    }

    private static async Task LaunchValidationAsync()
    {
        var runner = new FakeConsole();
        await using var service = new LdPlayerService(runner);
        var coordinator = new LdLaunchCoordinator(service);
        var resources = 0;
        foreach (var indices in new[] { Array.Empty<int>(), new[] { 0 }, new[] { 99999 }, new[] { -1 }, new[] { 1, 1 }, new[] { 1, 0 } })
            await Expect(400, () => coordinator.StartAsync(indices, _ => { resources++; return Task.CompletedTask; }));
        Assert(runner.Calls.Count == 0 && resources == 0, "Invalid launch batch reached the console or resource callback.");
    }

    private static async Task SequentialLaunchAsync()
    {
        var runner = new FakeConsole(); runner.Instances.Add(new(2, "B"));
        runner.OnLaunch = index =>
        {
            if (index == 2) Assert(runner.Instances.Single(item => item.Index == 1).Flag == 1, "Second instance started before the first was ready.");
        };
        await using var service = new LdPlayerService(runner);
        var resources = 0; var progress = new RecordingProgress();
        var result = await Coordinator(service).StartAsync([1, 2], _ => { resources++; return Task.CompletedTask; }, progress);
        Assert(resources == 2 && runner.Count("launch") == 2, "Launch or resource checks were skipped or repeated.");
        Assert(result.Select(item => item.Index).SequenceEqual([1, 2]) && result.All(item => item.State == "ready"), "Sequential results were incomplete.");
        Assert(progress.Items.Any(item => item.State == "waiting") && result.All(item => item.Message.Contains("ADB", StringComparison.Ordinal)), "Progress incorrectly claimed guest readiness.");
        Assert(!runner.Calls.Any(call => call.Contains("0") || call[0] is "adb" or "quit" or "quitall"), "Batch touched protected or global operations.");
    }

    private static async Task ConfirmationValidationAsync()
    {
        var runner = new FakeConsole(); runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        var snapshot = (await service.RefreshAsync()).ToArray();
        var before = runner.Calls.Count;
        var resources = 0;
        var invalidSnapshots = new IReadOnlyList<LdPlayerInstance>[]
        {
            [], [snapshot[0]], [snapshot[0], snapshot[0]],
            [snapshot[0], snapshot[1], snapshot[0] with { Index = 3 }],
            [snapshot[0], snapshot[1] with { Index = 3 }]
        };
        var coordinator = Coordinator(service);
        foreach (var invalid in invalidSnapshots)
            await Expect(400, () => coordinator.StartAsync([1, 2], _ => { resources++; return Task.CompletedTask; }, expectedInstances: invalid));
        Assert(runner.Calls.Count == before && resources == 0, "Invalid confirmation snapshot reached the console or resource callback.");
        var result = await coordinator.StartAsync([1, 2], _ => { resources++; return Task.CompletedTask; }, expectedInstances: snapshot.Reverse().ToArray());
        Assert(result.Count == 2 && result.All(item => item.State == "ready") && runner.Count("launch") == 2, "Valid unordered confirmation snapshot blocked normal launch generations.");
    }

    private static async Task ConfirmationReplacementAsync()
    {
        foreach (var rename in new[] { true, false })
        {
            var runner = new FakeConsole(); runner.Instances.Add(new(2, "B"));
            await using var service = new LdPlayerService(runner);
            var snapshot = (await service.RefreshAsync()).ToArray();
            if (rename) runner.Instances[0].Name = "replacement";
            else
            {
                runner.Instances[0].Width++;
                await service.RefreshAsync();
                runner.Instances[0].Width--;
                await service.RefreshAsync();
            }
            var resources = 0;
            await Expect(409, () => Coordinator(service).StartAsync([1, 2], _ => { resources++; return Task.CompletedTask; }, expectedInstances: snapshot));
            Assert(resources == 0 && runner.Count("launch") == 0, "Replacement after confirmation launched a changed or later instance.");
        }
    }

    private static async Task ExistingProcessesAsync()
    {
        var runner = new FakeConsole();
        runner.Instances[0] = new(1, "A", 1, 101, 201);
        runner.Instances.Add(new(2, "B", 2, 102, -1) { BootReads = 3 });
        await using var service = new LdPlayerService(runner);
        var result = await Coordinator(service).StartAsync([1, 2], _ => throw new InvalidOperationException("Existing processes must not reserve another launch."));
        Assert(result[0].State == "skipped" && result[1].State == "ready" && runner.Count("launch") == 0, "Existing processes were relaunched or marked ready too early.");
        var zeroPid = new FakeConsole(); zeroPid.Instances[0] = new(1, "A", 2) { BootReads = 3 };
        await using var zeroPidService = new LdPlayerService(zeroPid);
        var progress = new RecordingProgress();
        var zeroPidResult = await Coordinator(zeroPidService).StartAsync([1], _ => throw new InvalidOperationException("Starting flag must not reserve another launch."), progress);
        Assert(zeroPidResult[0].State == "ready" && zeroPid.Count("launch") == 0 && progress.Items.Any(item => item.Instance is { IsStarting: true, ProcessId: <= 0, VBoxProcessId: <= 0 }), "Zero-PID startup flag was relaunched or never observed while waiting.");
    }

    private static async Task LaunchTimeoutAsync()
    {
        var runner = new FakeConsole { NeverReady = true }; runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        await Expect(504, () => new LdLaunchCoordinator(service, TimeSpan.FromMilliseconds(40), TimeSpan.FromMilliseconds(2))
            .StartAsync([1, 2], _ => Task.CompletedTask));
        Assert(runner.Count("launch") == 1 && runner.Calls.Where(call => call[0] == "launch").All(call => call[2] == "1"), "Timed out launch was retried or the next instance started.");
        Assert(runner.Count("quit") == 0, "Timed out launch closed a guest.");
    }

    private static async Task LaunchCancellationAsync()
    {
        var runner = new FakeConsole { NeverReady = true }; runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        using var cancellation = new CancellationTokenSource();
        var progress = new RecordingProgress(item => { if (item.State == "waiting") cancellation.Cancel(); });
        try { await Coordinator(service).StartAsync([1, 2], _ => Task.CompletedTask, progress, cancellation.Token); throw new InvalidOperationException("Cancellation was ignored."); }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        Assert(runner.Count("launch") == 1 && runner.Count("quit") == 0, "Cancellation retried, continued, or closed the owned launch.");
    }

    private static async Task ResourceRejectionAsync()
    {
        var runner = new FakeConsole(); runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        var resources = 0;
        await Expect(503, () => Coordinator(service).StartAsync([1, 2], _ => ++resources == 1 ? Task.CompletedTask : throw new DeviceException("reserve rejected", 503, 2000)));
        Assert(resources == 2 && runner.Count("launch") == 1 && runner.Count("quit") == 0, "Resource rejection did not stop after the first ready instance.");
    }

    private static async Task LaunchIdentityAsync()
    {
        var runner = new FakeConsole { RenameAfterLaunch = true }; runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        await Expect(409, () => Coordinator(service).StartAsync([1, 2], _ => Task.CompletedTask));
        Assert(runner.Count("launch") == 1, "Changed target identity was retried or another target started.");
    }

    private static async Task ResourceGenerationAsync()
    {
        var runner = new FakeConsole(); runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        var resourceChecks = 0;
        await Expect(409, () => Coordinator(service).StartAsync([1, 2], async ct =>
        {
            resourceChecks++;
            var instance = runner.Instances[0];
            var width = instance.Width;
            instance.Width++;
            await service.RefreshAsync(ct);
            instance.Width = width;
            await service.RefreshAsync(ct);
        }));
        Assert(resourceChecks == 1 && runner.Count("launch") == 0, "Changed original generation was silently replaced or a later target started.");
    }

    private static async Task OverlappingBatchesAsync()
    {
        var runner = new FakeConsole(); runner.Instances.Add(new(2, "B"));
        await using var service = new LdPlayerService(runner);
        var coordinator = Coordinator(service);
        await Task.WhenAll(coordinator.StartAsync([1, 2], _ => Task.CompletedTask), coordinator.StartAsync([1, 2], _ => Task.CompletedTask));
        Assert(runner.Count("launch") == 2, "Overlapping batches launched an instance more than once.");
    }

    private static async Task CreateValidationAsync()
    {
        var runner = new FakeConsole();
        await using var service = new LdPlayerService(runner);
        var editor = new LdInstanceEditor(runner, service);
        foreach (var name in new[] { "", " ", "--option", "bad\nname", "bad\n", "\tbad", "bad\0name", "bad,name", new string('a', 81) })
            await Expect(400, () => editor.CreateAsync(name));
        Assert(runner.Calls.Count == 0, "Invalid creation name reached the CLI.");
        await Expect(409, () => editor.CreateAsync("A"));
        Assert(runner.Count("add") == 0, "Duplicate name created another instance.");
    }

    private static async Task CreateAsync()
    {
        var runner = new FakeConsole();
        await using var service = new LdPlayerService(runner);
        var result = await new LdInstanceEditor(runner, service).CreateAsync("测试实例 B");
        Assert(result.Instance.Index == 2 && result.Instance.Name == "测试实例 B" && !result.CommandReportedFailure, "Creation result was not the unique new instance.");
        Assert(runner.Count("add") == 1 && runner.Calls.All(call => call[0] is "list2" or "add"), "Creation retried or used unrelated operations.");
    }

    private static async Task CreateReportedFailureAsync()
    {
        var runner = new FakeConsole { AddReportsFailure = true };
        await using var service = new LdPlayerService(runner);
        var result = await new LdInstanceEditor(runner, service).CreateAsync("B");
        Assert(result.CommandReportedFailure && result.Instance.Index == 2 && result.Message.Contains("退出码=1", StringComparison.Ordinal), "Creation command failure warning was lost.");
        Assert(runner.Count("add") == 1, "Nonzero add was blindly retried.");
    }

    private static async Task CreateAmbiguityAsync()
    {
        var runner = new FakeConsole { AddExtraInstance = true };
        await using var service = new LdPlayerService(runner);
        await Expect(409, () => new LdInstanceEditor(runner, service).CreateAsync("B"));
        Assert(runner.Count("add") == 1 && runner.Count("remove") == 0, "Ambiguous creation retried or deleted data.");
        var wrongNameRunner = new FakeConsole { CreatedNameOverride = "external" };
        await using var wrongNameService = new LdPlayerService(wrongNameRunner);
        await Expect(409, () => new LdInstanceEditor(wrongNameRunner, wrongNameService).CreateAsync("B"));
        Assert(wrongNameRunner.Count("add") == 1, "Wrong-name creation was accepted or retried.");
    }

    private static async Task CreateInterruptionAsync()
    {
        var timedOut = new FakeConsole { AddReportsTimeout = true };
        await using var timeoutService = new LdPlayerService(timedOut);
        await Expect(504, () => new LdInstanceEditor(timedOut, timeoutService).CreateAsync("B"));
        Assert(timedOut.Count("add") == 1 && timedOut.Count("list2") == 1, "Timed out creation issued more CLI commands.");
        using var cancellation = new CancellationTokenSource();
        var cancelled = new FakeConsole { OnAdd = cancellation.Cancel };
        await using var cancelledService = new LdPlayerService(cancelled);
        try { await new LdInstanceEditor(cancelled, cancelledService).CreateAsync("B", cancellation.Token); throw new InvalidOperationException("Create cancellation was ignored."); }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        Assert(cancelled.Count("add") == 1 && cancelled.Count("list2") == 1 && cancelled.Count("remove") == 0, "Cancelled creation retried, queried again, or deleted an instance.");
    }

    private static async Task ConfigureValidationAsync()
    {
        var runner = new FakeConsole();
        await using var service = new LdPlayerService(runner);
        var editor = new LdInstanceEditor(runner, service);
        var valid = new LdInstanceConfiguration(2, 2048, 720, 1280, 240);
        foreach (var configuration in new[] { valid with { Cpu = 0 }, valid with { Cpu = 6 }, valid with { MemoryMb = 0 }, valid with { MemoryMb = 3072 }, valid with { Width = 0 }, valid with { Height = 100_000 }, valid with { Dpi = 0 } })
            await Expect(400, () => editor.ConfigureAsync(1, 1, configuration));
        foreach (var index in new[] { 0, -1, 99999 }) await Expect(400, () => editor.ConfigureAsync(index, 1, valid));
        await Expect(400, () => editor.ConfigureAsync(1, 0, valid));
        Assert(runner.Calls.Count == 0, "Invalid configuration reached the console.");
    }

    private static async Task ConfigurePreconditionsAsync()
    {
        var runner = new FakeConsole();
        await using var service = new LdPlayerService(runner);
        var editor = new LdInstanceEditor(runner, service);
        var configuration = new LdInstanceConfiguration(2, 2048, 720, 1280, 240);
        foreach (var instance in new[] { new FakeInstance(1, "A", 1, 101, 201), new FakeInstance(1, "A", 2, 101, -1), new FakeInstance(1, "A", 2), new FakeInstance(1, "A", 0, -1, 201), new FakeInstance(1, "A") { TopWindow = 1 } })
        {
            runner.Instances[0] = instance;
            var current = (await service.RefreshAsync()).Single();
            await Expect(409, () => editor.ConfigureAsync(1, current.Generation, configuration));
        }
        runner.Instances[0] = new(1, "A");
        var stopped = (await service.RefreshAsync()).Single(); runner.Instances[0].Width++;
        await Expect(409, () => editor.ConfigureAsync(1, stopped.Generation, configuration));
        Assert(runner.Count("modify") == 0, "Running, starting, or stale instance was configured.");
    }

    private static async Task ConfigureAsync()
    {
        var runner = new FakeConsole();
        await using var service = new LdPlayerService(runner);
        var current = (await service.RefreshAsync()).Single();
        var result = await new LdInstanceEditor(runner, service).ConfigureAsync(1, current.Generation, new(4, 4096, 1080, 1920, 480));
        Assert(result.Accepted && !result.Verified && result.Instance?.Width == 1080 && result.Instance.Height == 1920, "Resource command result overclaimed verification or lost the observation.");
        var call = runner.Calls.Single(args => args[0] == "modify");
        Assert(call.SequenceEqual(["modify", "--index", "1", "--cpu", "4", "--memory", "4096", "--resolution", "1080,1920,480"]), "Unapproved configuration arguments were sent.");
    }

    private static LdLaunchCoordinator Coordinator(LdPlayerService service) => new(service, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(1));
    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static async Task Expect(int status, Func<Task> action)
    {
        try { await action(); }
        catch (DeviceException error) when (error.Status == status) { return; }
        throw new InvalidOperationException($"Expected DeviceException status={status}.");
    }

    private sealed class RecordingProgress(Action<LdLaunchProgress>? observer = null) : IProgress<LdLaunchProgress>
    {
        public List<LdLaunchProgress> Items { get; } = [];
        public void Report(LdLaunchProgress value) { Items.Add(value); observer?.Invoke(value); }
    }

    private sealed class FakeInstance(int index, string name, int flag = 0, int processId = -1, int vboxPid = -1)
    {
        public int Index { get; } = index;
        public string Name { get; set; } = name;
        public int Flag { get; set; } = flag;
        public int ProcessId { get; set; } = processId;
        public int VBoxPid { get; set; } = vboxPid;
        public long TopWindow { get; set; }
        public int Width { get; set; } = 720;
        public int Height { get; set; } = 1280;
        public int Dpi { get; set; } = 240;
        public int BootReads { get; set; }
        public string List2() => string.Join(',', Index, Name, TopWindow, 0, Flag, ProcessId, VBoxPid, Width, Height, Dpi);
    }

    private sealed class FakeConsole : ILdConsoleRunner
    {
        public List<FakeInstance> Instances { get; } = [new(1, "A")];
        public List<string[]> Calls { get; } = [];
        public bool NeverReady { get; init; }
        public bool RenameAfterLaunch { get; init; }
        public bool AddReportsFailure { get; init; }
        public bool AddReportsTimeout { get; init; }
        public bool AddExtraInstance { get; init; }
        public string? CreatedNameOverride { get; init; }
        public Action<int>? OnLaunch { get; set; }
        public Action? OnAdd { get; init; }
        public int Count(string command) => Calls.Count(call => call[0] == command);
        public Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var args = arguments.ToArray(); Calls.Add(args);
            if (args[0] == "list2")
            {
                foreach (var instance in Instances.Where(item => item.Flag == 2 && item.BootReads > 0))
                    if (--instance.BootReads == 0) { instance.Flag = 1; instance.ProcessId = 100 + instance.Index; instance.VBoxPid = 200 + instance.Index; }
                return Task.FromResult(string.Join('\n', Instances.Select(item => item.List2())));
            }
            if (args[0] == "launch")
            {
                var index = int.Parse(args[2], CultureInfo.InvariantCulture); OnLaunch?.Invoke(index);
                var instance = Instances.Single(item => item.Index == index);
                instance.Flag = 2; instance.ProcessId = 100 + index; instance.VBoxPid = 200 + index;
                instance.BootReads = NeverReady ? 0 : 3;
                if (RenameAfterLaunch) instance.Name = "changed";
            }
            else if (args[0] == "add")
            {
                Instances.Add(new(Instances.Max(item => item.Index) + 1, CreatedNameOverride ?? args[2]));
                if (AddExtraInstance) Instances.Add(new(Instances.Max(item => item.Index) + 1, "external"));
                OnAdd?.Invoke();
                if (AddReportsTimeout) throw new DeviceException("雷电 CLI 超时：命令=add，结果未知", 504, 2000);
                if (AddReportsFailure) throw new DeviceException("雷电 CLI 执行失败：命令=add，退出码=1", 503, 2000);
            }
            else if (args[0] == "modify")
            {
                var instance = Instances.Single(item => item.Index == int.Parse(args[2], CultureInfo.InvariantCulture));
                var size = args[8].Split(',').Select(value => int.Parse(value, CultureInfo.InvariantCulture)).ToArray();
                instance.Width = size[0]; instance.Height = size[1]; instance.Dpi = size[2];
            }
            else throw new InvalidOperationException("Unexpected fake console command: " + args[0]);
            return Task.FromResult("");
        }
    }
}

```

## tests/MatrixDevice.Tests/LiveAcceptance.cs

```csharp
using System.Diagnostics;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

internal static class LiveAcceptance
{
    private const string Package = "com.ss.android.ugc.aweme";
    public static async Task RunAsync(string[] args)
    {
        if (args.Length != 3) throw new ArgumentException("--live-acceptance <verified-target.json> <phase> <output-directory>");
        var target = JsonSerializer.Deserialize<Target>(File.ReadAllText(args[0]), JsonDefaults.Options)!;
        if (target.Index <= 0 || string.IsNullOrWhiteSpace(target.Serial) || target.VBoxPid <= 0 || !Path.IsPathFullyQualified(args[2]))
            throw new ArgumentException("A verified nonzero independent target and absolute evidence directory are required.");
        var output = Path.GetFullPath(args[2]); Directory.CreateDirectory(output);
        var phase = args[1];
        var options = new JsonSerializerOptions(JsonDefaults.Options) { WriteIndented = false };
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(AdbDeviceBackend).Assembly.Location)));
        var sequence = 0;
        DeviceInfo? observedIdentity = null;
        async Task<T> Case<T>(string id, string expected, Func<Task<T>> operation, string passStatus = "PASS-LIVE")
        {
            var watch = Stopwatch.StartNew(); var utc = DateTimeOffset.UtcNow;
            try
            {
                var actual = await operation();
                var evidence = Path.Combine(output, $"{++sequence:D3}-{id}.json");
                await File.WriteAllTextAsync(evidence, JsonSerializer.Serialize(actual, JsonDefaults.Options));
                await File.AppendAllTextAsync(Path.Combine(output, "results.jsonl"), JsonSerializer.Serialize(new { caseId = id, phase, utc, buildSha256 = hash, target.Index, target.Serial, generation = observedIdentity?.Generation, transportId = observedIdentity?.TransportId, expected, status = passStatus, durationMs = watch.ElapsedMilliseconds, evidence = Path.GetFileName(evidence), evidenceSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(evidence))) }, options) + "\n");
                Console.WriteLine($"{passStatus} {id} {watch.ElapsedMilliseconds}ms"); return actual;
            }
            catch (Exception ex)
            {
                await File.AppendAllTextAsync(Path.Combine(output, "results.jsonl"), JsonSerializer.Serialize(new { caseId = id, phase, utc, buildSha256 = hash, target.Index, target.Serial, expected, status = "FAIL", durationMs = watch.ElapsedMilliseconds, error = ex.Message }, options) + "\n");
                throw;
            }
        }
        static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        static bool SearchField(AndroidNode node) => node.ResourceId == "android:id/search_src_text" && node.PackageName is "com.android.settings" or "com.android.settings.intelligence" && node.Enabled;
        var runner = new LdConsoleRunner(target.ConsolePath, 300);
        var instances = LdPlayerService.ParseList2(await runner.RunAsync(["list2"]));
        var instance = instances.Single(x => x.Index == target.Index);
        Require(instance.Name == target.Name && instance.VBoxProcessId == target.VBoxPid && instance.AndroidStarted, "Target instance identity or readiness changed.");
        using (var process = Process.GetProcessById(target.VBoxPid)) Require(process.StartTime.ToUniversalTime().Ticks == target.VBoxStartedUtcTicks, "VBox PID was reused.");
        using (var process = Process.GetProcessById(target.ServerPid)) Require(process.StartTime.ToUniversalTime().Ticks == target.ServerStartedUtcTicks, "ADB service identity changed.");
        Require(instances.Single(x => x.Index == 0).ProcessId == target.OriginalPid && instances.Single(x => x.Index == 0).VBoxProcessId == target.OriginalVBoxPid, "Original host instance changed.");
        var settings = new AppSettings { AdbAllowedSerials = [target.Serial], LdConsolePath = target.ConsolePath,
            LdSerialBindings = new() { [target.Index] = target.Serial }, CommandTimeoutSeconds = 300,
            BasicApiPort = 18080, AutomatorApiPort = 18171, AutoStartApi = false };
        await using var runtime = await MatrixRuntime.CreateAsync(settings, Path.Combine(output, "runtime"));
        var backend = runtime.Backend;
        var devices = await Case("connect", "Exactly the authorized independent guest is Ready", () =>
        {
            var observed = runtime.Devices.Snapshot;
            Require(observed.Count == 1 && observed[0].Serial == target.Serial && observed[0].Online, "Authorized guest did not become Ready: " + runtime.DiscoveryError);
            observedIdentity = observed[0];
            return Task.FromResult(observed);
        });
        async Task<string> Shell(string command)
        {
            var start = Stopwatch.StartNew();
            try
            {
                var value = await backend.ShellAsync(target.Serial, command);
                await File.AppendAllTextAsync(Path.Combine(output, "shell-trace.jsonl"), JsonSerializer.Serialize(new { utc = DateTimeOffset.UtcNow, target.Serial, command, output = value, durationMs = start.ElapsedMilliseconds }, options) + "\n");
                return value;
            }
            catch (Exception ex) { await File.AppendAllTextAsync(Path.Combine(output, "shell-trace.jsonl"), JsonSerializer.Serialize(new { utc = DateTimeOffset.UtcNow, target.Serial, command, error = ex.Message, durationMs = start.ElapsedMilliseconds }, options) + "\n"); throw; }
        }
        async Task<object> Screen(string name)
        {
            var png = await backend.ScreenshotAsync(target.Serial); var size = PngImage.ReadDimensions(png);
            var path = Path.Combine(output, name + ".png"); await File.WriteAllBytesAsync(path, png);
            Require(size.Width > 0 && size.Height > 0, "Empty screenshot.");
            return new { size.Width, size.Height, bytes = png.Length, file = Path.GetFileName(path), sha256 = Convert.ToHexString(SHA256.HashData(png)) };
        }
        if (phase is "install" or "launch")
        {
            await Case("abi", "Android API >=23 with arm64 or documented native bridge", async () =>
            {
                var value = await Shell("getprop ro.product.cpu.abilist; getprop ro.dalvik.vm.native.bridge; getprop ro.build.version.sdk; getprop sys.boot_completed");
                Require(value.Contains("arm64", StringComparison.Ordinal) || value.Contains("libhoudini", StringComparison.Ordinal), "No supported ARM64 ABI/native bridge: " + value);
                return value;
            });
            if (phase == "install") await Case("install", "APK SHA256 and installed version match supplied APK", async () =>
            {
                Require(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(target.Apk))) == "DA4ED3EBCD2F76EEAA1061FD07BA10FDA5DA1C0173D22344C23A8ECAF675B8FA", "APK digest changed.");
                var existing = await Shell("pm list packages " + Package);
                Require(!existing.Split('\n').Any(line => line.Trim() == "package:" + Package), "Package already exists; no unapproved replacement.");
                await backend.InstallAsync(target.Serial, target.Apk);
                var installed = await Shell("dumpsys package " + Package);
                Require(installed.Contains("versionCode=400701", StringComparison.Ordinal) && installed.Contains("versionName=40.7.0", StringComparison.Ordinal), "Installed version mismatch.");
                return new { version = "40.7.0", code = 400701, path = await Shell("pm path " + Package) };
            });
            await Case("launch", "Target package has a PID and foreground activity", async () =>
            {
                var result = await backend.StartAppAsync(target.Serial, Package); await Task.Delay(4000);
                var pid = await Shell("pidof " + Package); var activity = await Shell("dumpsys activity activities");
                Require(!string.IsNullOrWhiteSpace(pid) && activity.Contains(Package, StringComparison.Ordinal), "App did not launch.");
                return new { result, pid, activity };
            });
            await Case("first-screen", "Nonempty PNG of installed app", () => Screen("first-screen"));
            await Case("first-nodes", "Parsed real node tree", () => backend.DumpAsync(target.Serial));
        }
        else if (phase == "dump-probe")
        {
            await Case("dump-diagnostic", "Capture uiautomator output and owned XML read result without accepting terms", async () =>
            {
                var path = "/data/local/tmp/matrix-acceptance-" + Guid.NewGuid().ToString("N") + ".xml";
                try
                {
                    var result = await Shell("uiautomator dump " + AdbDeviceBackend.Quote(path));
                    string xml;
                    try { xml = await Shell("cat " + AdbDeviceBackend.Quote(path)); } catch (DeviceException ex) { xml = ex.Message; }
                    return new { result, xml };
                }
                finally { await Shell("rm -f " + AdbDeviceBackend.Quote(path)); }
            });
        }
        else if (phase == "probe")
        {
            await Case("package-info", "Installed component diagnostic; no launch or mutation", () => Shell("dumpsys package " + Package));
            await Case("resolve", "Launcher intent resolution diagnostic", () => Shell("cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.LAUNCHER -p " + Package));
            await Case("probe-screen", "Current guest PNG", () => Screen("probe-screen"));
            await Case("probe-nodes", "Current guest node tree", () => backend.DumpAsync(target.Serial));
        }
        else if (phase is "core" or "io-api")
        {
            if (phase == "core") await Case("static-settings", "Safe static settings page for node and API testing", async () => { var result = await Shell("am start -W -a android.settings.SETTINGS"); Require(result.Contains("Status: ok", StringComparison.Ordinal), "Settings did not open."); await Task.Delay(1500); return result; });
            await Case("screen", "Screenshot crop and color use actual PNG", async () =>
            {
                var png = await backend.ScreenshotAsync(target.Serial); var crop = PngImage.Crop(png, 0, 0, 40, 40);
                await File.WriteAllBytesAsync(Path.Combine(output, "crop.png"), crop);
                var size = PngImage.ReadDimensions(png); var color = PngImage.ColorAt(png, 1, 1); var cropSize = PngImage.ReadDimensions(crop);
                return new { size.Width, size.Height, color = new { color.R, color.G, color.B }, cropWidth = cropSize.Width, cropHeight = cropSize.Height };
            });
            await Case("nodes", "Real nonempty node tree", async () => { var root = await backend.DumpAsync(target.Serial); Require(root.ChildNodeList.Count > 0, "Empty tree."); return root; });
            await Case("apps", "Supplied app listed with correct version", async () => { var apps = await backend.ListAppsAsync(target.Serial); Require(apps.Any(x => x.PackageName == Package && x.VersionName == "40.7.0"), "App/version missing."); return apps; });
            if (phase == "core")
            {
                await Case("stop", "App processes absent; installation retained", async () => { await backend.StopAppAsync(target.Serial, Package); var names = (await Shell("ps -A -o NAME")).Split('\n').Select(line => line.Trim()); Require(!names.Any(name => name == Package || name.StartsWith(Package + ":", StringComparison.Ordinal)), "App still running."); var path = await Shell("pm path " + Package); Require(path.StartsWith("package:", StringComparison.Ordinal), "Installation missing after stop."); return path; });
                await Case("home", "Launcher/Settings safe page only", async () => { await backend.KeyEventAsync(target.Serial, 3); return await Shell("dumpsys activity activities"); });
            }
            var remote = "/sdcard/MatrixAcceptance-" + Guid.NewGuid().ToString("N") + ".txt";
            await Case("file-roundtrip", "Push/pull SHA256 exact; only owned temporary file removed", async () =>
            {
                var source = Path.Combine(output, "transfer.txt"); var dest = Path.Combine(output, "pulled.txt");
                await File.WriteAllTextAsync(source, "matrix-test-中文-" + Guid.NewGuid());
                try { await backend.PushAsync(target.Serial, source, remote); await backend.PullAsync(target.Serial, remote, dest); Require(File.ReadAllBytes(source).SequenceEqual(File.ReadAllBytes(dest)), "File transfer mismatch."); return new { sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(dest))), bytes = new FileInfo(dest).Length }; }
                finally { await Shell("rm -f " + AdbDeviceBackend.Quote(remote)); }
            });
            if (phase == "core") await Case("script", "Actual app launch and script cancellation with no later steps", async () =>
            {
                var package = new ScriptPackage { Manifest = new() { Id = "live-acceptance", Name = "独立实例测试", Version = "1.0" }, Steps = [new() { Action = "launch", Value = Package }, new() { Action = "wait", DurationMs = 10000 }, new() { Action = "click", Value = "1,1" }] };
                var path = Path.Combine(output, "test-script.zip"); ScriptPackageService.Save(path, package);
                var loaded = ScriptPackageService.Load(path); var running = runtime.Scripts.RunAsync(target.Serial, loaded);
                await Task.Delay(4000); await runtime.Scripts.RunAsync(target.Serial, loaded);
                Require(!string.IsNullOrWhiteSpace(await Shell("pidof " + Package)), "Script launch did not affect app.");
                Require(runtime.Scripts.Stop(target.Serial), "Script could not be cancelled."); await running;
                var logs = runtime.Scripts.GetLogs(target.Serial);
                Require(runtime.Scripts.GetStatus(target.Serial).State == ScriptRunState.Cancelled && !logs.Any(x => x.Message.Contains("步骤 3/", StringComparison.Ordinal)), "Post-cancel step ran."); return logs;
            });
            await Case("api", "Real loopback API target/crop/tree/shell and guarded writes", async () =>
            {
                await runtime.StartApisAsync(); var port = await runtime.Api.StartDeviceAsync(target.Serial);
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
                var device = await client.GetStringAsync($"http://127.0.0.1:{port}/device"); Require(device.Contains(target.Serial, StringComparison.Ordinal), "Per-device API target mismatch.");
                var records = new List<object>();
                foreach (var (route, fields) in new (string, Dictionary<string, object>)[] { ("device/dump", new()), ("device/screenshot", new() { ["x"] = 0, ["y"] = 0, ["width"] = 40, ["height"] = 40 }), ("device/shell", new() { ["command"] = "getprop sys.boot_completed" }), ("app/list-packages", new()), ("function/sleep-ms", new() { ["milliseconds"] = 10 }) })
                {
                    fields["serial"] = target.Serial;
                    records.Add(await Case("api-" + route.Replace('/', '-'), "Real API response with root code 0", async () =>
                    {
                        using var response = await client.PostAsJsonAsync("http://127.0.0.1:18171/" + route, fields);
                        var body = await response.Content.ReadAsStringAsync(); var root = JsonSerializer.Deserialize<JsonElement>(body);
                        Require(response.IsSuccessStatusCode && root.GetProperty("code").GetInt32() == 0, "API failure " + route + ": " + body);
                        return new { route, status = (int)response.StatusCode, body };
                    }));
                }
                await runtime.StopApisAsync(); return new { device, records };
            });
            if (phase == "core") { await backend.StopAppAsync(target.Serial, Package); await backend.KeyEventAsync(target.Serial, 3); }
        }
        else if (phase is "settings" or "input")
        {
            await Case("settings-open", "Only safe system settings becomes foreground with search task reset", async () => { await backend.StopAppAsync(target.Serial, "com.android.settings.intelligence"); await backend.StopAppAsync(target.Serial, "com.android.settings"); var result = await Shell("am start -W -a android.settings.SETTINGS"); Require(result.Contains("Status: ok", StringComparison.Ordinal), "Settings did not open."); await Task.Delay(1500); return result; });
            var root = await Case("settings-nodes", "Static real settings tree", () => backend.DumpAsync(target.Serial));
            await Case("settings-screen", "Real safe settings screen", () => Screen("settings-screen"));
            if (phase == "input")
            {
                await Case("swipe", "Safe settings list moves; node text sequence changes", async () =>
                {
                    var before = NodeSearch.FindAll(root, new()).Where(n => !string.IsNullOrEmpty(n.Text)).Select(n => n.Text).ToArray();
                    var size = PngImage.ReadDimensions(await backend.ScreenshotAsync(target.Serial));
                    await backend.SwipeAsync(target.Serial, size.Width / 2, size.Height * 4 / 5, size.Width / 2, size.Height * 2 / 5, 400); await Task.Delay(700);
                    var after = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(n => !string.IsNullOrEmpty(n.Text)).Select(n => n.Text).ToArray();
                    Require(!before.SequenceEqual(after), "Settings list did not move.");
                    await backend.SwipeAsync(target.Serial, size.Width / 2, size.Height * 2 / 5, size.Width / 2, size.Height * 4 / 5, 400); await Task.Delay(700);
                    return new { before, after };
                });
                root = await backend.DumpAsync(target.Serial);
                var search = NodeSearch.FindAll(root, new()).FirstOrDefault(n => n.PackageName == "com.android.settings" && n.Bounds.Width > 0 &&
                    ((n.Text ?? "").Contains("搜索", StringComparison.Ordinal) || (n.Text ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase) || (n.ContentDesc ?? "").Contains("搜索", StringComparison.Ordinal) || (n.ContentDesc ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase)));
                Require(search is not null, "No verified settings search control; input blocked.");
                await Case("tap", "Known settings search control opens an editable field", async () =>
                {
                    await backend.TapAsync(target.Serial, search!.Bounds.CenterX, search.Bounds.CenterY); await Task.Delay(700);
                    var nodes = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(SearchField).ToArray();
                    Require(nodes.Any(n => n.Enabled && n.Bounds.Width > 0), "Search tap did not open EditText.");
                    var field = nodes.First(n => n.Enabled && n.Bounds.Width > 0); await backend.TapAsync(target.Serial, field.Bounds.CenterX, field.Bounds.CenterY);
                    return field;
                });
                const string text = "Matrix Test 42!?";
                await Case("ascii-input", "ASCII spaces and punctuation appear exactly without submission", async () =>
                {
                    await backend.TextAsync(target.Serial, text); await Task.Delay(600);
                    var fields = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(SearchField).ToArray();
                    Require(fields.Any(n => n.Text == text), "Text readback mismatch."); return fields;
                });
                await Case("ld-chinese-input", "LD call.input adds exact Chinese and space to the same safe field", async () =>
                {
                    var ld = runtime.LdPlayer!; await ld.RefreshAsync(); var current = ld.Instances.Single(x => x.Index == target.Index);
                    const string chinese = "中文 测试";
                    var result = await ld.ExecuteAsync(target.Index, "input", new JsonObject { ["expectedGeneration"] = current.Generation, ["text"] = chinese });
                    await Task.Delay(700); var fields = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(SearchField).ToArray();
                    Require(fields.Any(n => n.Text == text + chinese), "LD Chinese readback mismatch."); return new { result, fields };
                });
                await Case("bound-global-chinese", "Production TextAsync uses only the explicitly bound LD instance", async () =>
                {
                    const string value = " 已绑定"; await backend.TextAsync(target.Serial, value); await Task.Delay(600);
                    var fields = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(SearchField).ToArray();
                    Require(fields.Any(n => n.Text == text + "中文 测试" + value), "Bound Unicode text readback mismatch."); return fields;
                });
                await Case("back", "Back dismisses input and returns to safe settings", async () => { await backend.KeyEventAsync(target.Serial, 4); await backend.KeyEventAsync(target.Serial, 4); await Task.Delay(500); return await backend.DumpAsync(target.Serial); });
            }
        }
        else if (phase == "script-actions")
        {
            await backend.StopAppAsync(target.Serial, "com.android.settings.intelligence"); await backend.StopAppAsync(target.Serial, "com.android.settings");
            await Shell("am start -W -a android.settings.SETTINGS"); await Task.Delay(800);
            var size = PngImage.ReadDimensions(await backend.ScreenshotAsync(target.Serial));
            var control = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).First(n => n.PackageName == "com.android.settings" && (n.Text ?? "").Contains("搜索", StringComparison.Ordinal) && n.Bounds.Width > 0);
            await Case("script-all-actions", "Launch/wait/swipe/click complete and open only safe system search", async () =>
            {
                var package = new ScriptPackage { Manifest = new() { Id = "all-safe-actions", Name = "系统页面步骤验收", Version = "1.0" }, Steps =
                [new() { Action = "launch", Value = "com.android.settings" }, new() { Action = "wait", DurationMs = 600 },
                 new() { Action = "swipe", Value = $"{size.Width / 2},{size.Height * 4 / 5},{size.Width / 2},{size.Height * 2 / 5},400" }, new() { Action = "wait", DurationMs = 600 },
                 new() { Action = "swipe", Value = $"{size.Width / 2},{size.Height * 2 / 5},{size.Width / 2},{size.Height * 4 / 5},400" }, new() { Action = "wait", DurationMs = 600 },
                 new() { Action = "click", Value = $"{control.Bounds.CenterX},{control.Bounds.CenterY}" }] };
                await runtime.Scripts.RunAsync(target.Serial, package);
                Require(runtime.Scripts.GetStatus(target.Serial).State == ScriptRunState.Completed && NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Any(SearchField), "Script actions did not complete with the safe search effect.");
                await Screen("script-finished"); return runtime.Scripts.GetLogs(target.Serial);
            });
        }
        else if (phase == "ld-api")
        {
            await runtime.StartApisAsync(); using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
            async Task<JsonElement> Ld(string id, string action, JsonObject body)
            {
                await runtime.LdPlayer!.RefreshAsync(); body["expectedGeneration"] = runtime.LdPlayer.Instances.Single(x => x.Index == target.Index).Generation;
                await File.WriteAllTextAsync(Path.Combine(output, "request-" + id + ".json"), body.ToJsonString(JsonDefaults.Options));
                using var response = await client.PostAsJsonAsync($"http://127.0.0.1:18080/ldplayer/instances/{target.Index}/{action}", body);
                var result = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync()); Require(response.IsSuccessStatusCode && result.GetProperty("ok").GetBoolean(), "LD API rejected " + action + ": " + result);
                await File.WriteAllTextAsync(Path.Combine(output, "response-" + id + ".json"), result.GetRawText()); return result;
            }
            await Case("L04", "LD API runapp makes provided app foreground", async () => { var result = await Ld("L04", "runapp", new() { ["packageName"] = Package }); await Task.Delay(1500); var pid = await Shell("pidof " + Package); Require(!string.IsNullOrWhiteSpace(pid), "LD runapp did not start app."); await Screen("ld-runapp"); return new { result, pid, activity = await Shell("dumpsys activity activities") }; });
            await Case("L05", "LD API killapp removes only provided app processes", async () => { var result = await Ld("L05", "killapp", new() { ["packageName"] = Package }); var names = await Shell("ps -A -o NAME"); Require(!names.Split('\n').Any(x => x.Trim().StartsWith(Package, StringComparison.Ordinal)), "LD killapp left processes."); return new { result, installation = await Shell("pm path " + Package) }; });
            await backend.StopAppAsync(target.Serial, "com.android.settings.intelligence"); await backend.StopAppAsync(target.Serial, "com.android.settings"); await Shell("am start -W -a android.settings.SETTINGS"); await Task.Delay(800);
            var control = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).First(n => n.PackageName == "com.android.settings" && (n.Text ?? "").Contains("搜索", StringComparison.Ordinal) && n.Bounds.Width > 0);
            await backend.TapAsync(target.Serial, control.Bounds.CenterX, control.Bounds.CenterY); await Task.Delay(500);
            await Case("L07", "LD HTTP input exact Chinese/space readback on safe system search", async () => { const string text = "雷电接口 测试"; var result = await Ld("L07", "input", new() { ["text"] = text }); await Task.Delay(500); var fields = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(SearchField).ToArray(); Require(fields.Any(n => n.Text == text), "LD HTTP text mismatch."); return new { result, fields }; });
            await Case("L08", "LD HTTP Home leaves settings page", async () => { var result = await Ld("L08", "keyevent", new() { ["key"] = "home" }); await Task.Delay(500); var tree = await backend.DumpAsync(target.Serial); Require(!NodeSearch.FindAll(tree, new()).Any(n => n.PackageName is "com.android.settings" or "com.android.settings.intelligence"), "LD Home had no effect."); return new { result, tree }; });
            await Shell("am start -W -a android.settings.SETTINGS"); await runtime.StopApisAsync();
        }
        else if (phase == "api-matrix")
        {
            async Task<AndroidNode> Reset()
            {
                await backend.StopAppAsync(target.Serial, "com.android.settings.intelligence"); await backend.StopAppAsync(target.Serial, "com.android.settings");
                await Shell("am start -W -a android.settings.SETTINGS"); await Task.Delay(800); return await backend.DumpAsync(target.Serial);
            }
            async Task<AndroidNode> SearchControl()
            {
                var tree = await Reset(); return NodeSearch.FindAll(tree, new()).First(n => n.PackageName == "com.android.settings" && (n.Text ?? "").Contains("搜索", StringComparison.Ordinal) && n.Bounds.Width > 0);
            }
            async Task RequireSearch() { Require(NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Any(SearchField), "API click did not open the safe search field."); }
            await Reset(); await runtime.RefreshAsync(); observedIdentity = runtime.Devices.Snapshot.Single();
            await runtime.StartApisAsync(); var port = await runtime.Api.StartDeviceAsync(target.Serial);
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
            async Task<JsonElement> Post(string id, string path, Dictionary<string, object> fields, int status = 200, bool basic = false, Func<JsonElement, Task>? verify = null)
            {
                if (!basic) fields["serial"] = target.Serial;
                await File.WriteAllTextAsync(Path.Combine(output, "request-" + id + ".json"), JsonSerializer.Serialize(fields, JsonDefaults.Options));
                return await Case(id, status == 200 ? "Root success envelope and specified real postcondition" : "Rejection without guest mutation; not business feature success", async () =>
                {
                    using var response = await client.PostAsJsonAsync($"http://127.0.0.1:{(basic ? port : 18171)}/{path}", fields);
                    var root = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
                    await File.WriteAllTextAsync(Path.Combine(output, "response-" + id + ".json"), root.GetRawText());
                    Require((int)response.StatusCode == status, $"{id} expected HTTP {status}, actual={response.StatusCode}: {root}");
                    if (status == 200) Require(basic ? root.GetProperty("ok").GetBoolean() : root.GetProperty("code").GetInt32() == 0, "Root envelope is not success.");
                    if (verify is not null) await verify(root);
                    if (status == 200 && path is "input/tap" or "input/swipe" or "input/keyevent" or "element/click-if-exists" or "element/click-text" or "element/click-node" or "text" or "tap" or "swipe" or "app/start" or "app/stop")
                        await Screen("after-" + id);
                    return root;
                }, status == 200 ? id == "A16-diagnostic" ? "PASS-DIAGNOSTIC" : "PASS-LIVE" : "PASS-ERROR");
            }
            foreach (var (id, url) in new[] { ("B01", "http://127.0.0.1:18080/health"), ("B02", "http://127.0.0.1:18080/devices"), ("B03", $"http://127.0.0.1:{port}/health"), ("B04", $"http://127.0.0.1:{port}/device") })
                await Case(id, "Bound device or gateway metadata has a valid success envelope", async () => { var root = JsonSerializer.Deserialize<JsonElement>(await client.GetStringAsync(url)); Require(root.GetProperty("ok").GetBoolean(), "GET failed."); if (id != "B01") Require(root.GetRawText().Contains(target.Serial, StringComparison.Ordinal), "Wrong serial."); return root; });
            await Post("A01", "device/dump", new(), verify: root => { Require(root.GetProperty("data").GetProperty("childNodeList").GetArrayLength() > 0, "Empty API tree."); return Task.CompletedTask; });
            await Post("A02", "device/screenshot", new() { ["width"] = 40, ["height"] = 40 }, verify: root => { var size = PngImage.ReadDimensions(Convert.FromBase64String(root.GetProperty("data").GetProperty("base64").GetString()!)); Require(size == (40, 40), "API crop mismatch."); return Task.CompletedTask; });
            foreach (var (id, route) in new[] { ("A03", "device/shell"), ("A04", "device/execute") }) await Post(id, route, new() { ["command"] = "getprop sys.boot_completed" }, verify: root => { Require(root.GetProperty("data").GetProperty("output").GetString()!.Trim() == "1", "Wrong guest output."); return Task.CompletedTask; });
            await Post("A05-error", "device/set-prop", new() { ["prop"] = "invalid space", ["value"] = "test" }, 400);
            await Post("A06-error", "device/set-timezone", new() { ["timezone"] = "Matrix/InvalidZone" }, 400);
            await Post("A07-error", "device/set-sim-info", new(), 501);
            await Post("A08", "app/list", new() { ["showSystem"] = false, ["needVersion"] = true }, verify: root => { Require(root.GetProperty("data").EnumerateArray().Any(x => x.GetProperty("packageName").GetString() == Package && x.GetProperty("versionName").GetString() == "40.7.0"), "Target app missing."); return Task.CompletedTask; });
            await Post("A09", "app/list-packages", new(), verify: root => { Require(root.GetProperty("data").EnumerateArray().Any(x => x.GetString() == "android"), "System package android missing."); return Task.CompletedTask; });
            await Post("A10", "app/start", new() { ["packageName"] = Package }, verify: async root => { Require(!string.IsNullOrWhiteSpace(await Shell("pidof " + Package)), "API app did not launch."); });
            await Post("A11", "app/stop", new() { ["packageName"] = Package }, verify: async root => { Require(!(await Shell("ps -A -o NAME")).Split('\n').Any(x => x.Trim().StartsWith(Package, StringComparison.Ordinal)), "API stop left processes."); });
            await Post("A12-error", "app/clear", new() { ["packageName"] = "-invalid" }, 400);
            var control = await SearchControl();
            await Post("A13", "input/tap", new() { ["x"] = control.Bounds.CenterX, ["y"] = control.Bounds.CenterY }, verify: root => RequireSearch());
            var staticTree = await Reset(); var dimensions = PngImage.ReadDimensions(await backend.ScreenshotAsync(target.Serial));
            var beforeTexts = NodeSearch.FindAll(staticTree, new()).Select(n => n.Text).ToArray();
            await Post("A14", "input/swipe", new() { ["x1"] = dimensions.Width / 2, ["y1"] = dimensions.Height * 4 / 5, ["x2"] = dimensions.Width / 2, ["y2"] = dimensions.Height * 2 / 5 }, verify: async root => { var after = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Select(n => n.Text); Require(!beforeTexts.SequenceEqual(after), "API swipe had no list effect."); });
            await Post("A15", "input/keyevent", new() { ["code"] = 3 }, verify: async root => { var nodes = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()); Require(!nodes.Any(n => n.PackageName == "com.android.settings"), "Home did not leave Settings."); });
            await Post("A16-diagnostic", "service/info", new(), verify: root => { Require(!root.GetProperty("data").GetProperty("agentServiceAvailable").GetBoolean(), "Unexpected agent."); return Task.CompletedTask; });
            await Post("A17-error", "service/stop", new(), 501);
            await Post("A18", "function/sleep", new() { ["seconds"] = 0.01 }); await Post("A19", "function/sleep-ms", new() { ["milliseconds"] = 10 });
            control = await SearchControl(); var selector = new NodeSelector { Text = control.Text, PackageName = "com.android.settings" };
            await Post("A20", "element/find", new() { ["selector"] = selector }, verify: root => { Require(root.GetProperty("data").GetProperty("text").GetString() == control.Text, "Wrong matched node."); return Task.CompletedTask; });
            await Post("A21", "element/find-all", new() { ["selector"] = selector }, verify: root => { Require(root.GetProperty("data").GetArrayLength() > 0, "No matched nodes."); return Task.CompletedTask; });
            await Post("A22", "element/wait-exists", new() { ["selector"] = selector, ["timeoutMs"] = 0 }, verify: root => { Require(root.GetProperty("data").GetProperty("value").GetBoolean(), "Known node absent."); return Task.CompletedTask; });
            await Post("A23", "element/click-if-exists", new() { ["selector"] = selector, ["timeoutMs"] = 0 }, verify: root => RequireSearch());
            control = await SearchControl(); await Post("A24", "element/click-text", new() { ["text"] = control.Text!, ["timeoutMs"] = 0 }, verify: root => RequireSearch());
            await Post("A25-error", "element/set-text", new() { ["selector"] = new NodeSelector { ResourceId = "android:id/search_src_text" }, ["text"] = "测试" }, 501);
            control = await SearchControl(); selector = new() { Text = control.Text, PackageName = "com.android.settings" };
            await Post("A26", "element/wait-any", new() { ["selectors"] = new[] { selector, new NodeSelector { Text = "not-present-matrix-test" } }, ["timeoutMs"] = 0 }, verify: root => { Require(root.GetProperty("data").GetProperty("index").GetInt32() == 0, "Selector priority mismatch."); return Task.CompletedTask; });
            await Post("A27", "element/click-node", new() { ["node"] = control }, verify: root => RequireSearch());
            await Post("B05", "shell", new() { ["Command"] = "getprop sys.boot_completed" }, basic: true, verify: root => { Require(root.GetProperty("data").GetProperty("output").GetString()!.Trim() == "1", "Basic shell mismatch."); return Task.CompletedTask; });
            await Post("B08", "text", new() { ["Text"] = "API中文 Text 7!?" }, basic: true, verify: async root => { var fields = NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Where(SearchField); Require(fields.Any(n => n.Text == "API中文 Text 7!?"), "Basic Unicode text exact readback mismatch."); });
            control = await SearchControl(); await Post("B06", "tap", new() { ["X"] = control.Bounds.CenterX, ["Y"] = control.Bounds.CenterY }, basic: true, verify: root => RequireSearch());
            staticTree = await Reset(); beforeTexts = NodeSearch.FindAll(staticTree, new()).Select(n => n.Text).ToArray(); dimensions = PngImage.ReadDimensions(await backend.ScreenshotAsync(target.Serial));
            await Post("B07", "swipe", new() { ["StartX"] = dimensions.Width / 2, ["StartY"] = dimensions.Height * 4 / 5, ["EndX"] = dimensions.Width / 2, ["EndY"] = dimensions.Height * 2 / 5, ["DurationMs"] = 400 }, basic: true, verify: async root => { Require(!beforeTexts.SequenceEqual(NodeSearch.FindAll(await backend.DumpAsync(target.Serial), new()).Select(n => n.Text)), "Basic swipe no effect."); });
            await Post("B09", "app/start", new() { ["PackageName"] = Package }, basic: true, verify: async root => { Require(!string.IsNullOrWhiteSpace(await Shell("pidof " + Package)), "Basic app start failed."); });
            await Post("B10", "app/stop", new() { ["PackageName"] = Package }, basic: true, verify: async root => { Require(!(await Shell("ps -A -o NAME")).Split('\n').Any(x => x.Trim().StartsWith(Package, StringComparison.Ordinal)), "Basic stop failed."); });
            await Reset(); await runtime.StopApisAsync();
        }
        else if (phase == "soak")
        {
            var clock = Stopwatch.StartNew(); var reads = 0; var screens = 0; var dumps = 0;
            for (var i = 0; clock.Elapsed < TimeSpan.FromMinutes(30) || reads < 200; i++)
            {
                await Case("read-" + i, "Actual boot property equals 1", async () => { var value = (await Shell("getprop sys.boot_completed")).Trim(); Require(value == "1", "Guest not booted."); reads++; return value; });
                if (i % 6 == 0 && screens < 30) { await Case("screen-" + i, "Live PNG", () => Screen("screen-" + i)); screens++; }
                if (i % 18 == 0 && dumps < 10) { await Case("dump-" + i, "Live node tree", () => backend.DumpAsync(target.Serial)); dumps++; }
                if (i % 30 == 0) await Case("resources-" + i, "Host original instance unchanged; sample owned runtime resources", async () =>
                {
                    var current = LdPlayerService.ParseList2(await runner.RunAsync(["list2"])); var original = current.Single(x => x.Index == 0);
                    Require(original.ProcessId == target.OriginalPid && original.VBoxProcessId == target.OriginalVBoxPid, "Original instance changed during soak.");
                    using var process = Process.GetCurrentProcess(); return new { elapsedMs = clock.ElapsedMilliseconds, workingSet = process.WorkingSet64, privateBytes = process.PrivateMemorySize64, handles = process.HandleCount, threads = process.Threads.Count, serverPid = target.ServerPid, originalPid = original.ProcessId, originalVBoxPid = original.VBoxProcessId };
                });
                await Task.Delay(TimeSpan.FromSeconds(8));
            }
            await Case("soak-summary", "30min,200 reads,30 screens,10 dumps without failure", () => { Require(screens >= 30 && dumps >= 10, "Insufficient soak samples."); return Task.FromResult(new { elapsedMs = clock.ElapsedMilliseconds, reads, screens, dumps, memory = Process.GetCurrentProcess().WorkingSet64 }); });
        }
        else throw new ArgumentException("Unsupported acceptance phase: " + phase);
    }
    private sealed record Target(int Index, string Name, string Serial, int VBoxPid, long VBoxStartedUtcTicks, int ServerPid,
        long ServerStartedUtcTicks, int OriginalPid, int OriginalVBoxPid, string ConsolePath, string Apk);
}

```

## tests/MatrixDevice.Tests/MatrixDevice.Tests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType></PropertyGroup>
  <ItemGroup><ProjectReference Include="../../src/MatrixDevice.Host/MatrixDevice.Host.csproj" /></ItemGroup>
</Project>

```

## tests/MatrixDevice.Tests/MultiInstanceAcceptance.cs

```csharp
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

internal static class MultiInstanceAcceptance
{
    private const string Package = "com.ss.android.ugc.aweme";
    internal sealed record Target(int Index, string Name, string Serial, string Model, string Manufacturer,
        int MemoryMb = 0, int VBoxPid = 0, long VBoxBirth = 0, uint TransportId = 0, int PlayerPid = 0, long PlayerBirth = 0);
    internal sealed record Spec(string RunId, string ConsolePath, string Apk, string ApkSha256, int OriginalPid,
        int OriginalVBoxPid, int ServerPid, long ServerBirth, Target[] Targets, int Minutes = 15, int ProxyPort = 0);
    public static async Task RunAsync(string[] args)
    {
        if (args.Length != 3 || args[1] is not ("prepare" or "workload" or "diagnose")) throw new ArgumentException("--multi-acceptance <spec.json> prepare|workload|diagnose <absolute-output>");
        var spec = JsonSerializer.Deserialize<Spec>(File.ReadAllText(args[0]), JsonDefaults.Options)!;
        Require(Path.IsPathFullyQualified(args[2]) && spec.Targets.Length >= 2 && spec.Targets.All(x => x.Index > 0 && !string.IsNullOrWhiteSpace(x.Serial)), "Only explicit independent targets and absolute output allowed");
        Require(spec.Targets.Select(x => x.Index).Distinct().Count() == spec.Targets.Length && spec.Targets.Select(x => x.Serial).Distinct(StringComparer.Ordinal).Count() == spec.Targets.Length, "Duplicate index/serial");
        Require(spec.Targets.All(x => x.Model.Length is > 0 and <= 64 && x.Manufacturer.Length is > 0 and <= 64
            && !x.Model.StartsWith('-') && !x.Manufacturer.StartsWith('-') && !x.Model.Any(char.IsControl) && !x.Manufacturer.Any(char.IsControl)), "Invalid model/manufacturer strings");
        Require(spec.RunId.All(x => char.IsAsciiLetterOrDigit(x) || x == '-'), "Invalid run ID");
        var output = Path.GetFullPath(args[2]); Directory.CreateDirectory(output);
        await File.WriteAllTextAsync(Path.Combine(output, "run-manifest.json"), JsonSerializer.Serialize(new
        {
            utc = DateTimeOffset.UtcNow, phase = args[1], runtime = Environment.Version.ToString(), specSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[0]))),
            assemblies = new[] { typeof(MultiInstanceAcceptance).Assembly, typeof(AdbDeviceBackend).Assembly, typeof(ScriptScheduler).Assembly, typeof(MatrixRuntime).Assembly }
                .Select(assembly => new { file = Path.GetFileName(assembly.Location), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))) })
        }, JsonDefaults.Options));
        var journal = new Journal(output);
        var runner = new RecordingRunner(new LdConsoleRunner(spec.ConsolePath, 300), journal);
        await using var ld = new LdPlayerService(runner);
        async Task CheckHost()
        {
            var rows = await ld.RefreshAsync(); var original = rows.Single(x => x.Index == 0);
            Require(original.ProcessId == spec.OriginalPid && original.VBoxProcessId == spec.OriginalVBoxPid, "Original instance host identity changed");
            using var server = Process.GetProcessById(spec.ServerPid);
            Require(server.StartTime.ToUniversalTime().Ticks == spec.ServerBirth && !server.HasExited, "Shared ADB epoch changed; stop without restarting it");
            foreach (var target in spec.Targets)
            {
                var row = rows.Single(x => x.Index == target.Index);
                Require(row.Name == target.Name, "Target name/index mismatch");
                if (target.VBoxPid > 0)
                {
                    Require(row.AndroidStarted && row.VBoxProcessId == target.VBoxPid, "Running target instance identity changed");
                    using var process = Process.GetProcessById(target.VBoxPid);
                    Require(process.StartTime.ToUniversalTime().Ticks == target.VBoxBirth, "Running VBox PID was reused");
                    Require(row.ProcessId == target.PlayerPid, "Target player process changed");
                    using var player = Process.GetProcessById(target.PlayerPid);
                    Require(player.StartTime.ToUniversalTime().Ticks == target.PlayerBirth, "Running player PID was reused");
                }
            }
        }
        await CheckHost();
        if (args[1] == "prepare")
        {
            var prepared = new List<Target>();
            foreach (var target in spec.Targets)
            {
                await journal.Case(target, "cold-profile", async () =>
                {
                    var before = (await ld.RefreshAsync()).Single(x => x.Index == target.Index);
                    if (before.AndroidStarted || before.ProcessId > 0 || before.VBoxProcessId > 0)
                    {
                        await ld.ExecuteAsync(target.Index, "quit", new JsonObject { ["expectedGeneration"] = before.Generation, ["confirmed"] = true });
                        await Wait(() => ld.RefreshAsync(), rows => rows.Single(x => x.Index == target.Index) is { AndroidStarted: false, ProcessId: <= 0, VBoxProcessId: <= 0 }, 60);
                    }
                    await CheckHost();
                    var stopped = (await ld.RefreshAsync()).Single(x => x.Index == target.Index);
                    Require(!stopped.AndroidStarted && stopped.ProcessId <= 0 && stopped.VBoxProcessId <= 0, "Profile edit requires a fully stopped target");
                    List<string> command = ["modify", "--index", target.Index.ToString(), "--manufacturer", target.Manufacturer, "--model", target.Model];
                    if (target.MemoryMb > 0) command.AddRange(["--memory", target.MemoryMb.ToString(), "--cpu", "2"]);
                    var result = await runner.RunAsync(command);
                    return new { before, requestedModel = target.Model, requestedManufacturer = target.Manufacturer, result, identifierFlagsRequested = false };
                });
                await journal.Case(target, "launch", async () =>
                {
                    var memory = ReadMemory();
                    await journal.Event("prepare-resources.jsonl", new { utc = DateTimeOffset.UtcNow, target.Index, memory.AvailablePhysical, memory.TotalPhysical, memory.AvailablePageFile, memory.TotalPageFile });
                    var required = (3UL * 1024 * 1024 * 1024) + (ulong)target.MemoryMb * 1024 * 1024;
                    Require(memory.AvailablePhysical >= required && (1.0 - (double)memory.AvailablePageFile / memory.TotalPageFile) < .80, "Insufficient launch reserve; do not add load");
                    var row = (await ld.RefreshAsync()).Single(x => x.Index == target.Index);
                    var result = await ld.ExecuteAsync(target.Index, "launch", new JsonObject { ["expectedGeneration"] = row.Generation });
                    await Wait(() => ld.RefreshAsync(), rows => rows.Single(x => x.Index == target.Index).AndroidStarted, 90);
                    await CheckHost(); return result;
                });
                var current = (await ld.RefreshAsync()).Single(x => x.Index == target.Index);
                using var vbox = Process.GetProcessById(current.VBoxProcessId);
                using var player = Process.GetProcessById(current.ProcessId);
                var fixedTarget = target with { VBoxPid = current.VBoxProcessId, VBoxBirth = vbox.StartTime.ToUniversalTime().Ticks, PlayerPid = current.ProcessId, PlayerBirth = player.StartTime.ToUniversalTime().Ticks };
                // The formula is only a candidate supplied by the receipt. Actual TCP ownership is mandatory before any guest read.
                await Wait(async () => { try { await LdPlayerAdbBinding.VerifyAsync(fixedTarget.Serial, fixedTarget.VBoxPid, fixedTarget.VBoxBirth); return true; } catch (DeviceException) { return false; } }, ready => ready, 120);
                await using var backend = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = [fixedTarget.Serial], CommandTimeoutSeconds = 300 });
                var devices = await Wait(() => backend.DiscoverAsync(), rows => rows.Any(x => x.Serial == fixedTarget.Serial && x.Online), 120);
                fixedTarget = fixedTarget with { TransportId = checked((uint)devices.Single(x => x.Serial == fixedTarget.Serial).TransportId!.Value) };
                await journal.Case(fixedTarget, "boot-model", async () =>
                {
                    await Wait(() => backend.ShellAsync(fixedTarget.Serial, "getprop sys.boot_completed"), value => value.Trim() == "1", 120);
                    var model = (await backend.ShellAsync(fixedTarget.Serial, "getprop ro.product.model")).Trim();
                    var manufacturer = (await backend.ShellAsync(fixedTarget.Serial, "getprop ro.product.manufacturer")).Trim();
                    Require(model == fixedTarget.Model && manufacturer == fixedTarget.Manufacturer, $"Profile not effective: model={model}, manufacturer={manufacturer}");
                    var png = await backend.ScreenshotAsync(fixedTarget.Serial); var size = PngImage.ReadDimensions(png);
                    await File.WriteAllBytesAsync(Path.Combine(output, $"instance-{fixedTarget.Index}-first.png"), png);
                    var observed = (await backend.DiscoverAsync()).Single(x => x.Serial == fixedTarget.Serial);
                    Require(observed.Model == model, "Catalog model did not refresh");
                    return new { model, manufacturer, size, observed, fixedTarget };
                });
                prepared.Add(fixedTarget);
                await File.WriteAllTextAsync(Path.Combine(output, "targets.json"), JsonSerializer.Serialize(spec with { Targets = prepared.Concat(spec.Targets.Skip(prepared.Count)).ToArray() }, JsonDefaults.Options));
            }
            await CheckHost(); return;
        }
        Require(spec.Minutes is >= 1 and <= 30, "Duration must be 1..30 minutes");
        foreach (var target in spec.Targets)
        {
            using var process = Process.GetProcessById(target.VBoxPid);
            Require(process.StartTime.ToUniversalTime().Ticks == target.VBoxBirth, "Target process changed since receipt");
            await LdPlayerAdbBinding.VerifyAsync(target.Serial, target.VBoxPid, target.VBoxBirth);
        }
        if (args[1] == "diagnose")
        {
            await using var diagnostic = new AdbDeviceBackend(new AppSettings { AdbAllowedSerials = spec.Targets.Select(x => x.Serial).ToArray(), CommandTimeoutSeconds = 10 });
            var observed = await diagnostic.DiscoverAsync();
            foreach (var target in spec.Targets)
            {
                Require(observed.Single(x => x.Serial == target.Serial).TransportId == target.TransportId, "Diagnostic transport changed");
                foreach (var command in new[] { "dumpsys activity activities", "dumpsys activity lastanr", "dumpsys window windows", "ps -A -o PID,NAME" })
                {
                    try { await journal.Event("diagnostics.jsonl", new { target.Index, target.Serial, command, utc = DateTimeOffset.UtcNow, output = await diagnostic.ShellAsync(target.Serial, command) }); }
                    catch (Exception ex) { await journal.Event("diagnostics.jsonl", new { target.Index, target.Serial, command, utc = DateTimeOffset.UtcNow, error = ex.Message }); }
                }
                try
                {
                    var nodes = NodeSearch.FindAll(await diagnostic.DumpAsync(target.Serial), new());
                    await journal.Event("diagnostic-nodes.jsonl", new { target.Index, target.Serial, utc = DateTimeOffset.UtcNow, nodes });
                    await File.WriteAllBytesAsync(Path.Combine(output, $"instance-{target.Index}.png"), await diagnostic.ScreenshotAsync(target.Serial));
                }
                catch (Exception ex) { await journal.Event("diagnostic-nodes.jsonl", new { target.Index, target.Serial, utc = DateTimeOffset.UtcNow, error = ex.Message }); }
            }
            await CheckHost(); return;
        }
        Require(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(spec.Apk))) == spec.ApkSha256, "Supplied APK hash mismatch");
        var settings = new AppSettings { LdConsolePath = spec.ConsolePath, AdbAllowedSerials = spec.Targets.Select(x => x.Serial).ToArray(),
            LdSerialBindings = spec.Targets.ToDictionary(x => x.Index, x => x.Serial), CommandTimeoutSeconds = 300, BasicApiPort = 19180, AutomatorApiPort = 19171, AutoStartApi = false };
        await using var runtime = await MatrixRuntime.CreateAsync(settings, Path.Combine(output, "runtime"));
        var backendShared = runtime.Backend;
        Require(runtime.Devices.Snapshot.Count == spec.Targets.Length && runtime.Devices.Snapshot.All(x => x.Online), "Not all authorized guests ready");
        foreach (var target in spec.Targets)
        {
            var device = runtime.Devices.Snapshot.Single(x => x.Serial == target.Serial);
            Require(device.TransportId == target.TransportId && device.Model == target.Model, "Transport or model changed since receipt");
        }
        await runtime.StartApisAsync();
        var ports = new Dictionary<string, int>();
        foreach (var target in spec.Targets) ports[target.Serial] = await runtime.Api.StartDeviceAsync(target.Serial);
        var received = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        var builder = WebApplication.CreateBuilder(); builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:19281");
        await using var sink = builder.Build();
        sink.MapPost("/return", async (HttpContext context) =>
        {
            var record = await context.Request.ReadFromJsonAsync<JsonElement>();
            var serial = record.GetProperty("serial").GetString()!;
            if (!ports.ContainsKey(serial)) { context.Response.StatusCode = 400; return; }
            received.AddOrUpdate(serial, 1, (_, count) => count + 1);
            await journal.Event("returns.jsonl", record);
            await context.Response.WriteAsJsonAsync(new { serial, nonce = record.GetProperty("nonce").GetString(), accepted = true });
        });
        await sink.StartAsync();
        using var handler = new HttpClientHandler { UseProxy = spec.ProxyPort > 0 };
        if (spec.ProxyPort > 0) handler.Proxy = new WebProxy($"http://127.0.0.1:{spec.ProxyPort}") { BypassProxyOnLocal = false };
        using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) };
        using var lifetime = new CancellationTokenSource();
        var resources = Monitor(spec, journal, CheckHost, lifetime);
        async Task<JsonElement> Post(Target target, string path, object body, string task, int expectedStatus = 200, CancellationToken ct = default)
        {
            using var requestBudget = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, ct);
            using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body, options: JsonDefaults.Options) };
            var id = Guid.NewGuid().ToString("N");
            request.Headers.Add("X-Matrix-Run", spec.RunId); request.Headers.Add("X-Matrix-Instance", target.Index.ToString());
            request.Headers.Add("X-Matrix-Task", task); request.Headers.Add("X-Matrix-Request", id);
            var clock = Stopwatch.StartNew(); using var response = await http.SendAsync(request, requestBudget.Token);
            var text = await response.Content.ReadAsStringAsync(requestBudget.Token);
            await journal.Event("http.jsonl", new { utc = DateTimeOffset.UtcNow, target.Index, target.Serial, task, id, path, body, status = (int)response.StatusCode, response = text, durationMs = clock.ElapsedMilliseconds, proxy = spec.ProxyPort });
            Require((int)response.StatusCode == expectedStatus, $"HTTP {response.StatusCode}, expected={expectedStatus}: {text}"); return JsonDocument.Parse(text).RootElement.Clone();
        }
        var start = Stopwatch.StartNew(); var pressure = new Stopwatch();
        var readyWorkers = 0; var allReady = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task Worker(Target target, int role)
        {
            var token = lifetime.Token; var nonce = $"{spec.RunId}-{target.Index}-" + Guid.NewGuid().ToString("N");
            var remote = "/sdcard/MatrixMulti-" + spec.RunId; var local = Path.Combine(output, "instance-" + target.Index); Directory.CreateDirectory(local);
            await journal.Event("workers.jsonl", new { target.Index, target.Serial, nonce, role, state = "start", utc = DateTimeOffset.UtcNow });
            try
            {
                await backendShared.ShellAsync(target.Serial, "mkdir -p " + remote, ct: token);
                var marker = Path.Combine(local, "identity.txt"); await File.WriteAllTextAsync(marker, nonce, token);
                await backendShared.PushAsync(target.Serial, marker, remote + "/identity.txt", ct: token);
                await journal.Case(target, "install", async () =>
                {
                    await backendShared.InstallAsync(target.Serial, spec.Apk, ct: token);
                    var apps = await backendShared.ListAppsAsync(target.Serial, ct: token);
                    Require(apps.Any(x => x.PackageName == Package && x.VersionName == "40.7.0"), "Wrong installed version");
                    await backendShared.StartAppAsync(target.Serial, Package, ct: token);
                    var activity = await backendShared.ShellAsync(target.Serial, "dumpsys activity activities", ct: token);
                    Require(activity.Split('\n').Any(line => (line.Contains("mResumedActivity", StringComparison.Ordinal) || line.Contains("topResumedActivity", StringComparison.Ordinal)) && line.Contains(Package, StringComparison.Ordinal)), "App is not the resumed foreground activity");
                    var png = await backendShared.ScreenshotAsync(target.Serial, token); await File.WriteAllBytesAsync(Path.Combine(local, "app-start.png"), png, token);
                    await backendShared.StopAppAsync(target.Serial, Package, ct: token);
                    Require(!(await backendShared.ShellAsync(target.Serial, "ps -A -o NAME", ct: token)).Split('\n').Any(line => line.Trim().StartsWith(Package, StringComparison.Ordinal)), "App still running");
                    return new { installedVersion = "40.7.0", package = Package, apkSha256 = spec.ApkSha256, foregroundObserved = true, agreementNotAcceptedByHarness = true };
                });
                if (Interlocked.Increment(ref readyWorkers) == spec.Targets.Length) { pressure.Start(); allReady.SetResult(); }
                await allReady.Task.WaitAsync(token);
                var rounds = 0;
                do
                {
                    token.ThrowIfCancellationRequested(); var round = rounds++;
                    await journal.Case(target, $"round-{round}", async () =>
                    {
                        var bytes = RandomNumberGenerator.GetBytes(role == 0 ? 8 * 1024 * 1024 : 256 * 1024);
                        var source = Path.Combine(local, "upload.bin"); var pulled = Path.Combine(local, "pulled.bin"); await File.WriteAllBytesAsync(source, bytes, token);
                        var hash = Convert.ToHexString(SHA256.HashData(bytes));
                        await backendShared.PushAsync(target.Serial, source, remote + "/payload.bin", ct: token);
                        await backendShared.PullAsync(target.Serial, remote + "/payload.bin", pulled, ct: token);
                        Require(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(pulled))) == hash, "Cross-instance payload or truncated transfer");
                        using var uiBudget = CancellationTokenSource.CreateLinkedTokenSource(token);
                        uiBudget.CancelAfter(TimeSpan.FromSeconds(45));
                        var uiToken = uiBudget.Token;
                        await journal.Event("ui-stages.jsonl", new { target.Index, target.Serial, round, stage = "open-settings", utc = DateTimeOffset.UtcNow });
                        await backendShared.KeyEventAsync(target.Serial, 3, uiToken);
                        await backendShared.ShellAsync(target.Serial, "am start -a android.settings.SETTINGS -f 0x14000000", ct: uiToken);
                        while (true)
                        {
                            await Task.Delay(700, uiToken);
                            var foreground = await backendShared.ShellAsync(target.Serial, "dumpsys activity activities", ct: uiToken);
                            if (foreground.Split('\n').Any(line => (line.Contains("mResumedActivity", StringComparison.Ordinal) || line.Contains("topResumedActivity", StringComparison.Ordinal)) && line.Contains("com.android.settings/", StringComparison.Ordinal))) break;
                        }
                        await journal.Event("ui-stages.jsonl", new { target.Index, target.Serial, round, stage = "settings-foreground", utc = DateTimeOffset.UtcNow });
                        var root = await backendShared.DumpAsync(target.Serial, uiToken);
                        var search = NodeSearch.FindAll(root, new()).FirstOrDefault(node => node.PackageName == "com.android.settings" && node.Bounds.Width > 0 &&
                            ((node.Text ?? "").Contains("搜索") || (node.ContentDesc ?? "").Contains("搜索") || (node.Text ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase) || (node.ContentDesc ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase)));
                        Require(search is not null, "No safe settings search control");
                        var size = PngImage.ReadDimensions(await backendShared.ScreenshotAsync(target.Serial, uiToken));
                        var package = SettingsScript(target.Index, role, search!.Bounds.CenterX, search.Bounds.CenterY, size.Width, size.Height);
                        await runtime.Scripts.RunAsync(target.Serial, package, uiToken);
                        Require(runtime.Scripts.GetStatus(target.Serial).State == ScriptRunState.Completed, "Script incomplete");
                        if (role != 0)
                        {
                            var afterScroll = await backendShared.DumpAsync(target.Serial, uiToken);
                            var fresh = NodeSearch.FindAll(afterScroll, new()).FirstOrDefault(node => node.PackageName == "com.android.settings" && node.Enabled && node.Bounds.Width > 0 &&
                                ((node.Text ?? "").Contains("搜索") || (node.ContentDesc ?? "").Contains("搜索") || (node.Text ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase) || (node.ContentDesc ?? "").Contains("Search", StringComparison.OrdinalIgnoreCase)));
                            Require(fresh is not null, "Search control disappeared after swipe; no stale tap sent");
                            await backendShared.TapAsync(target.Serial, fresh!.Bounds.CenterX, fresh.Bounds.CenterY, uiToken); await Task.Delay(600, uiToken);
                        }
                        var beforeText = await backendShared.DumpAsync(target.Serial, uiToken);
                        var edit = NodeSearch.FindAll(beforeText, new()).FirstOrDefault(node => node.ResourceId == "android:id/search_src_text" && node.Enabled && node.Bounds.Width > 0);
                        await journal.Event("before-input.jsonl", new { target.Index, target.Serial, round, edit });
                        Require(edit is not null, "No verified editable field; text not sent");
                        Require(IsEmptySettingsSearch(edit!), "Search input retained prior text; no appended text sent");
                        await backendShared.TapAsync(target.Serial, edit!.Bounds.CenterX, edit.Bounds.CenterY, uiToken);
                        var text = $"实例{target.Index} Round {round}";
                        await Post(target, $"http://127.0.0.1:{ports[target.Serial]}/text", new { text }, "text-input", ct: uiToken); await Task.Delay(500, uiToken);
                        var fields = NodeSearch.FindAll(await backendShared.DumpAsync(target.Serial, uiToken), new()).Where(node => node.ResourceId == "android:id/search_src_text").ToArray();
                        await File.WriteAllTextAsync(Path.Combine(local, "latest-input-nodes.json"), JsonSerializer.Serialize(fields, JsonDefaults.Options), token);
                        Require(fields.Any(node => node.Text == text), "Wrong instance text/focus");
                        uiToken.ThrowIfCancellationRequested();
                        await journal.Event("ui-stages.jsonl", new { target.Index, target.Serial, round, stage = "input-verified", utc = DateTimeOffset.UtcNow });
                        var reply = await Post(target, $"http://127.0.0.1:{ports[target.Serial]}/shell", new { command = "cat " + remote + "/identity.txt; sha256sum " + remote + "/payload.bin" }, "identity-return");
                        var actual = reply.GetProperty("data").GetProperty("output").GetString()!;
                        Require(actual.StartsWith(nonce, StringComparison.Ordinal) && actual.Contains(hash, StringComparison.OrdinalIgnoreCase), "Returned identity/payload belongs to another instance");
                        var telemetry = new { runId = spec.RunId, target.Index, serial = target.Serial, nonce, round, sha256 = hash, bytes = bytes.Length, text, model = target.Model, script = runtime.Scripts.GetStatus(target.Serial), data = actual };
                        var ack = await Post(target, "http://127.0.0.1:19281/return", telemetry, "data-return"); Require(ack.GetProperty("nonce").GetString() == nonce && ack.GetProperty("serial").GetString() == target.Serial, "Wrong telemetry acknowledgement");
                        var png = await backendShared.ScreenshotAsync(target.Serial, token); await File.WriteAllBytesAsync(Path.Combine(local, "latest.png"), png, token);
                        await journal.Event("script-logs.jsonl", new { target.Index, target.Serial, round, logs = runtime.Scripts.GetLogs(target.Serial) });
                        return telemetry;
                    });
                    await Task.Delay(TimeSpan.FromSeconds(role == 0 ? 4 : 2), token);
                } while (pressure.Elapsed < TimeSpan.FromMinutes(spec.Minutes));
                await journal.Event("workers.jsonl", new { target.Index, target.Serial, state = "end", rounds, returns = received.GetValueOrDefault(target.Serial), utc = DateTimeOffset.UtcNow });
            }
            catch { lifetime.Cancel(); throw; }
        }
        try
        {
            await journal.Case(spec.Targets[1], "cross-instance-LD-api", async () =>
            {
                var first = spec.Targets[0]; var second = spec.Targets[1];
                var pending = runtime.Scripts.RunAsync(first.Serial, new ScriptPackage { Manifest = new() { Id = "multi-busy", Name = "并行门禁验证", Version = "1.0" }, Steps = [new() { Action = "wait", DurationMs = 10000 }] }, lifetime.Token);
                try
                {
                    var rows = await runtime.LdPlayer!.RefreshAsync(lifetime.Token);
                    var resolved = await backendShared.ShellAsync(second.Serial, "cmd package resolve-activity --brief -a android.intent.action.MAIN -c android.intent.category.HOME", ct: lifetime.Token);
                    var home = resolved.Split('\n').Select(line => line.Trim()).LastOrDefault(line => line.Contains('/'));
                    Require(home is not null, "Cannot resolve a verifiable Home activity");
                    var homePackage = home!.Split('/')[0];
                    await Post(first, $"http://127.0.0.1:19180/ldplayer/instances/{first.Index}/keyevent", new { expectedGeneration = rows.Single(x => x.Index == first.Index).Generation, key = "home" }, "same-instance-reject", 409);
                    var result = await Post(second, $"http://127.0.0.1:19180/ldplayer/instances/{second.Index}/keyevent", new { expectedGeneration = rows.Single(x => x.Index == second.Index).Generation, key = "home" }, "other-instance-home");
                    var state = runtime.Scripts.GetStatus(first.Serial).State;
                    Require(state == ScriptRunState.Running, "No actual overlap between A script and B LD API");
                    var activity = await Wait(async () =>
                    {
                        var value = await backendShared.ShellAsync(second.Serial, "dumpsys activity activities", ct: lifetime.Token);
                        await journal.Event("home-diagnostic.jsonl", new { utc = DateTimeOffset.UtcNow, second.Serial, resolved, activity = value });
                        return value;
                    }, value => value.Split('\n').Any(line => (line.Contains("mResumedActivity") || line.Contains("topResumedActivity")) && line.Contains(homePackage, StringComparison.Ordinal)), 30);
                    return new { busySerial = first.Serial, otherSerial = second.Serial, sameInstanceStatus = 409, otherStatus = 200, state, result, home, activity, homeObserved = true };
                }
                finally { runtime.Scripts.Stop(first.Serial); await pending; }
            });
            await Task.WhenAll(spec.Targets.Select((target, index) => Worker(target, index)));
        }
        finally
        {
            lifetime.Cancel(); foreach (var target in spec.Targets) runtime.Scripts.Stop(target.Serial);
            try { await resources; } catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
            await sink.StopAsync();
        }
        await CheckHost();
        await journal.Event("summary.jsonl", new { elapsedMs = start.ElapsedMilliseconds, pressureMs = pressure.ElapsedMilliseconds, targetCount = spec.Targets.Length, returns = received, proxyPort = spec.ProxyPort, status = "PASS-LIVE-MULTI" });
    }
    private static async Task Monitor(Spec spec, Journal journal, Func<Task> checkHost, CancellationTokenSource lifetime)
    {
        var previous = Times(); var clock = Stopwatch.StartNew(); var highCpuSeconds = 0;
        try
        {
        while (!lifetime.IsCancellationRequested)
        {
            var memory = ReadMemory();
            var current = Times(); var total = current.Kernel + current.User - previous.Kernel - previous.User;
            var cpu = total == 0 ? 0 : (1.0 - (double)(current.Idle - previous.Idle) / total) * 100; previous = current;
            var processes = spec.Targets.SelectMany(target => new[] { target.VBoxPid, target.PlayerPid }).Append(Environment.ProcessId).Distinct().Select(pid =>
            { using var process = Process.GetProcessById(pid); return new { pid, process.ProcessName, process.WorkingSet64, process.PrivateMemorySize64, process.HandleCount, cpuTimeMs = process.TotalProcessorTime.TotalMilliseconds }; }).ToArray();
            await checkHost();
            await journal.Event("resources.jsonl", new { utc = DateTimeOffset.UtcNow, elapsedMs = clock.ElapsedMilliseconds, cpuPercent = cpu, memory.AvailablePhysical, memory.TotalPhysical, memory.AvailablePageFile, memory.TotalPageFile, processes });
            highCpuSeconds = cpu > 90 ? highCpuSeconds + 10 : 0;
            if (memory.AvailablePhysical < 2UL * 1024 * 1024 * 1024 || (1.0 - (double)memory.AvailablePageFile / memory.TotalPageFile) > .85 || highCpuSeconds >= 60)
            { lifetime.Cancel(); throw new InvalidOperationException("Resource stop threshold reached; only owned workload cancelled"); }
            await Task.Delay(TimeSpan.FromSeconds(10), lifetime.Token);
        }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await journal.Event("resource-stop.jsonl", new { utc = DateTimeOffset.UtcNow, error = ex.Message });
            lifetime.Cancel(); throw;
        }
    }
    private static async Task<T> Wait<T>(Func<Task<T>> operation, Func<T, bool> ready, int seconds)
    {
        var clock = Stopwatch.StartNew();
        while (clock.Elapsed < TimeSpan.FromSeconds(seconds))
        {
            try { var value = await operation(); if (ready(value)) return value; }
            catch (DeviceException ex) when (ex.Message.StartsWith("list2 ", StringComparison.Ordinal) && ex.Status is 400 or 502)
            { Console.WriteLine("WAIT directory transition: " + ex.Message); }
            await Task.Delay(2000);
        }
        throw new TimeoutException("Explicit target readiness timeout");
    }
    private static void Require(bool valid, string message) { if (!valid) throw new InvalidOperationException(message); }
    internal static bool IsEmptySettingsSearch(AndroidNode node) => node.ResourceId == "android:id/search_src_text"
        && node.PackageName == "com.android.settings.intelligence" && node.ClassName == "android.widget.AutoCompleteTextView"
        && (string.IsNullOrEmpty(node.Text) || node.Text == "\u641c\u7d22\u2026");
    internal static ScriptPackage SettingsScript(int index, int role, int searchX, int searchY, int width, int height)
    {
        var package = new ScriptPackage { Manifest = new() { Id = "multi-" + index, Name = "独立实例任务" + index, Version = "1.0" }, Steps =
            [new() { Action = "wait", DurationMs = role == 0 ? 900 : 1600 }, new() { Action = "click", Value = $"{searchX},{searchY}" }, new() { Action = "wait", DurationMs = 600 }] };
        if (role != 0) package.Steps[1] = new ScriptStep { Action = "swipe", Value = $"{width / 2},{height * 4 / 5},{width / 2},{height * 2 / 5},350" };
        return package;
    }
    private sealed class Journal(string directory)
    {
        private readonly SemaphoreSlim gate = new(1, 1); private int sequence;
        public async Task Event(string file, object value)
        {
            await gate.WaitAsync(); try { await File.AppendAllTextAsync(Path.Combine(directory, file), JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonDefaults.Options) { WriteIndented = false }) + "\n"); } finally { gate.Release(); }
        }
        public async Task Case(Target target, string task, Func<Task<object>> operation)
        {
            var start = DateTimeOffset.UtcNow; var clock = Stopwatch.StartNew();
            try { var value = await operation(); var file = $"{Interlocked.Increment(ref sequence):D4}-instance-{target.Index}-{task}.json"; await File.WriteAllTextAsync(Path.Combine(directory, file), JsonSerializer.Serialize(value, JsonDefaults.Options)); await Event("results.jsonl", new { target.Index, target.Serial, target.TransportId, task, start, end = DateTimeOffset.UtcNow, status = "PASS-LIVE", durationMs = clock.ElapsedMilliseconds, file, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(directory, file)))) }); Console.WriteLine($"PASS {target.Index} {task} {clock.ElapsedMilliseconds}ms"); }
            catch (Exception ex) { await Event("results.jsonl", new { target.Index, target.Serial, task, start, end = DateTimeOffset.UtcNow, status = "FAIL", durationMs = clock.ElapsedMilliseconds, error = ex.Message }); throw; }
        }
    }
    private sealed class RecordingRunner(ILdConsoleRunner inner, Journal journal) : ILdConsoleRunner
    {
        public async Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct = default)
        {
            var start = DateTimeOffset.UtcNow;
            try { var output = await inner.RunAsync(arguments, ct); await journal.Event("ld-console.jsonl", new { start, arguments, output }); return output; }
            catch (Exception ex) { await journal.Event("ld-console.jsonl", new { start, arguments, error = ex.Message }); throw; }
        }
    }
    [StructLayout(LayoutKind.Sequential)] private struct Memory { public uint Length, Load; public ulong TotalPhysical, AvailablePhysical, TotalPageFile, AvailablePageFile, TotalVirtual, AvailableVirtual, AvailableExtended; }
    private static Memory ReadMemory() { var value = new Memory { Length = (uint)Marshal.SizeOf<Memory>() }; if (!GlobalMemoryStatusEx(ref value)) throw new InvalidOperationException("Cannot sample memory"); return value; }
    [DllImport("kernel32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GlobalMemoryStatusEx(ref Memory memory);
    [DllImport("kernel32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetSystemTimes(out long idle, out long kernel, out long user);
    private static (ulong Idle, ulong Kernel, ulong User) Times() { if (!GetSystemTimes(out var idle, out var kernel, out var user)) throw new InvalidOperationException("Cannot sample CPU"); return ((ulong)idle, (ulong)kernel, (ulong)user); }
}

```

## tests/MatrixDevice.Tests/packages.lock.json

```json
{
  "version": 1,
  "dependencies": {
    "net10.0": {
      "AdvancedSharpAdbClient": {
        "type": "Transitive",
        "resolved": "3.6.16",
        "contentHash": "w5p3s+9Rkx9tmn5S5KU0XSTap5YWGHOvQOw/40M0d7eGOHf7idhX+kizWPmnP1ahbkic/yiX03SMYNR1mJSAVw=="
      },
      "Microsoft.Data.Sqlite": {
        "type": "Transitive",
        "resolved": "10.0.12",
        "contentHash": "xUq9xUc653ue7ac2rMfCaf8UQN+U3zS8poWYlugycpvJllplMM0Mk7v/+MM0CTlu9zGvbGML4BPH8YWhrUOfGw==",
        "dependencies": {
          "Microsoft.Data.Sqlite.Core": "10.0.12",
          "SQLitePCLRaw.bundle_e_sqlite3": "2.1.12",
          "SQLitePCLRaw.core": "2.1.12"
        }
      },
      "Microsoft.Data.Sqlite.Core": {
        "type": "Transitive",
        "resolved": "10.0.12",
        "contentHash": "2aL9eL5HQr0V4b8V909SD+jQAx6qnTTl7sT/v3D8dIvmr2UGdpZIQxqm1pj2Js3e4EYTvpw0oMH7DjapJJuPPw==",
        "dependencies": {
          "SQLitePCLRaw.core": "2.1.12"
        }
      },
      "SkiaSharp": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "TghYLOUaP3q6rTjQw7U9uQaaXh8lEs9rfA1U7/VtmyAptVDtj/SmjtcWzFWmcC/VMWxe3QgbV9uyvqDL7ySFtQ==",
        "dependencies": {
          "SkiaSharp.NativeAssets.Win32": "4.153.1",
          "SkiaSharp.NativeAssets.macOS": "4.153.1"
        }
      },
      "SkiaSharp.NativeAssets.macOS": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "YTojWeWIhnpF7uC8vpMxB5wRONKkDB2t3r/nNPA3DL2pgj9fqMXRevA/rIanYazWnco4jo1ufgp3HkXuh1Tz8A=="
      },
      "SkiaSharp.NativeAssets.Win32": {
        "type": "Transitive",
        "resolved": "4.153.1",
        "contentHash": "Yqg6rChTVh0BUE8QQxtOA3uUBnihfgID4yex/AvDa0+os2bVcBjmI3Qj9tnOB+R1hd/Jry1Gwl8qw5JI9HZG8Q=="
      },
      "SQLitePCLRaw.bundle_e_sqlite3": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "mAgscpQMLw5/nfA1Q5oJVAT29yROUo1ifZGbbTpx/lwZpSxMUGoYbKfmvdm8oXER+RzxqBmmQzeBEVKfeHv2nw==",
        "dependencies": {
          "SQLitePCLRaw.lib.e_sqlite3": "2.1.12",
          "SQLitePCLRaw.provider.e_sqlite3": "2.1.12"
        }
      },
      "SQLitePCLRaw.core": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "ETpNw9DY3ckWLgRRAeCHj+GKOuPi61aeczkXhgHexUvqoZBAYg8RYESE2J7O1M7+o6QbdSEZwrw9bfqztUVWXg=="
      },
      "SQLitePCLRaw.lib.e_sqlite3": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "fWi8Dbknuhgg72fWinIdjXVaqO1hHL4YBBwVLnr7e1c9TAZwJ0QE38j9syW1hwx6HaqEVTwI+O07WPdZn8Rp0w=="
      },
      "SQLitePCLRaw.provider.e_sqlite3": {
        "type": "Transitive",
        "resolved": "2.1.12",
        "contentHash": "W3oH4XIfCzFrgUSDKHhN6N+dgzA5YHOR2VxX8GB6Qy7CyrJJgxPEG8NirgYWlPQC5P2jz2knSsexWu4tDUL33g==",
        "dependencies": {
          "SQLitePCLRaw.core": "2.1.12"
        }
      },
      "matrixdevice.core": {
        "type": "Project",
        "dependencies": {
          "AdvancedSharpAdbClient": "[3.6.16, )",
          "SkiaSharp": "[4.153.1, )"
        }
      },
      "matrixdevice.features": {
        "type": "Project",
        "dependencies": {
          "MatrixDevice.Core": "[1.0.0, )"
        }
      },
      "matrixdevice.host": {
        "type": "Project",
        "dependencies": {
          "MatrixDevice.Features": "[1.0.0, )",
          "Microsoft.Data.Sqlite": "[10.0.12, )"
        }
      }
    }
  }
}
```

## tests/MatrixDevice.Tests/Program.cs

```csharp
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixDevice.Core;
using MatrixDevice.Features;
using MatrixDevice.Host;

if (args.Length > 0 && args[0] == "--multi-acceptance")
{
    try { await MultiInstanceAcceptance.RunAsync(args.Skip(1).ToArray()); }
    catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }
    return;
}
if (args.Length > 0 && args[0] == "--live-acceptance")
{
    try { await LiveAcceptance.RunAsync(args.Skip(1).ToArray()); }
    catch (Exception ex) { Console.Error.WriteLine(ex); Environment.ExitCode = 1; }
    return;
}
if (args.Length > 0 && args[0] == "--adb-owned-test-server")
{
    await using var server = new FakeAdbServer(int.Parse(args[1], System.Globalization.CultureInfo.InvariantCulture), args[2]);
    Console.WriteLine(server.Endpoint.Port); Console.Out.Flush();
    await Task.Delay(Timeout.Infinite);
    return;
}

if (args.Length > 0 && args[0] == "--ldconsole-test-child")
{
    if (args[1] == "sleep") { await Task.Delay(TimeSpan.FromSeconds(20)); return; }
    if (args[1] == "spawn")
    {
        var executable = Environment.ProcessPath!;
        var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
        if (Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase)) start.ArgumentList.Add(typeof(ControlledLdRunner).Assembly.Location);
        start.ArgumentList.Add("--ldconsole-test-child"); start.ArgumentList.Add("sleep");
        using var child = Process.Start(start)!;
        await File.WriteAllTextAsync(args[2], child.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await Task.Delay(TimeSpan.FromSeconds(20)); return;
    }
    if (args[1] is "flood" or "stderr-flood")
    {
        var block = new byte[1024 * 1024];
        using var output = args[1] == "flood" ? Console.OpenStandardOutput() : Console.OpenStandardError();
        for (var i = 0; i < (args[1] == "flood" ? 34 : 2); i++) await output.WriteAsync(block);
        return;
    }
    if (args[1] == "gbk")
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var output = Console.OpenStandardOutput();
        await output.WriteAsync(Encoding.GetEncoding(936).GetBytes("0,雷电测试,0,0,0,0,0,1600,900,240\r\n"));
        return;
    }
    if (args[1] == "fail") { Console.Error.Write("测试错误"); Environment.ExitCode = 7; return; }
    Console.Write(JsonSerializer.Serialize(args.Skip(2).ToArray()));
    return;
}

var tests = new List<(string Name, Func<Task> Run)>();
void Test(string name, Func<Task> run) => tests.Add((name, run));
ExistingAdbTests.Register(Test);
LdWorkspaceTests.Register(Test);
Test("LD admission applies only unique explicit local proofs and never mutates old authorization", async () =>
{
    var original = new AppSettings { AdbAllowedSerials = ["emulator-5556"], LdSerialBindings = new() { [1] = "emulator-5556" } };
    var proof = new LdConnectionProof(2, "test", 1, "emulator-5558", 2, 123, 456);
    var applied = LdConnectionAdmission.Apply(original, [proof]);
    Check(applied.AdbAllowedSerials.Length == 2 && applied.LdSerialBindings[2] == proof.Serial && !applied.AutoStartApi, "New authorization missing.");
    Check(original.AdbAllowedSerials.Length == 1 && !original.LdSerialBindings.ContainsKey(2), "Original settings mutated.");
    foreach (var invalid in new[] { proof with { Index = 0 }, proof with { Serial = "" }, proof with { Serial = "10.0.0.1:5555" }, proof with { Generation = 0 }, proof with { TransportId = 0 } })
        await Throws<DeviceException>(() => { LdConnectionAdmission.Apply(original, [invalid]); return Task.CompletedTask; });
    await Throws<DeviceException>(() => { LdConnectionAdmission.Apply(original, [proof, proof]); return Task.CompletedTask; });
    await Throws<DeviceException>(() => { LdConnectionAdmission.Apply(original, [proof with { Serial = "emulator-5556" }]); return Task.CompletedTask; });
});
Test("saved connection path changes require restart before LD workspace edits", async () =>
{
    await using var runtime = await MatrixRuntime.CreateAsync(new(), Temp(), true);
    runtime.RequireFrozenConnectionSettings();
    runtime.Settings = runtime.Settings with { LdConsolePath = "other-installation.exe" };
    await Throws<DeviceException>(() => { runtime.RequireFrozenConnectionSettings(); return Task.CompletedTask; });
});
Test("LD admission cannot replace the confirmed subject while waiting for ADB", async () =>
{
    var frozen = LdPlayerService.ParseList2("1,Original,1,2,1,123,456,1280,720,240").Single();
    LdConnectionAdmission.RequireExpectedInstances([frozen], [1], [frozen]);
    foreach (var changed in new[] { frozen with { Name = "Replacement" }, frozen with { Generation = frozen.Generation + 1 }, frozen with { VBoxProcessId = 999 } })
        await Throws<DeviceException>(() => { LdConnectionAdmission.RequireExpectedInstances([changed], [1], [frozen]); return Task.CompletedTask; });
    await Throws<DeviceException>(() => { LdConnectionAdmission.RequireExpectedInstances([frozen], [1], []); return Task.CompletedTask; });
});
Test("HTTP admission drain counter includes in-flight SDK identity checks", async () =>
{
    var port = Port(); await using var runtime = await MatrixRuntime.CreateAsync(new() { BasicApiPort = port }, Temp(), true);
    await runtime.Api.StartBasicAsync(port);
    var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    runtime.Api.LdCommandIdentityGuard = async (_, _, _) => { entered.TrySetResult(); await release.Task; };
    using var http = new HttpClient();
    var generation = runtime.LdPlayer!.Instances.Single(item => item.Index == 0).Generation;
    var request = http.PostAsJsonAsync($"http://localhost:{port}/ldplayer/instances/0/keyevent", new { expectedGeneration = generation, key = "home" });
    try { await entered.Task.WaitAsync(TimeSpan.FromSeconds(3)); Check(runtime.Api.HasActiveRequests, "In-flight request was not counted."); }
    finally { release.TrySetResult(); }
    using var response = await request; Check(response.IsSuccessStatusCode, "Released request did not complete.");
    var deadline = DateTime.UtcNow.AddSeconds(3);
    while (runtime.Api.HasActiveRequests && DateTime.UtcNow < deadline) await Task.Delay(10);
    Check(!runtime.Api.HasActiveRequests, "Completed request did not drain.");
});
Test("both differentiated workload script roles follow the existing swipe format", () =>
{
    var first = MultiInstanceAcceptance.SettingsScript(1, 0, 150, 50, 360, 640);
    var second = MultiInstanceAcceptance.SettingsScript(2, 1, 150, 50, 360, 640);
    ScriptPackageService.Validate(first); ScriptPackageService.Validate(second);
    Check(first.Steps.Count == 3 && second.Steps.Count == 3 && first.Steps[1].Action == "click" && second.Steps[1].Action == "swipe", "Differentiated scripts lost their distinct actions");
    return Task.CompletedTask;
});
Test("observed settings hint is accepted only in its exact search control", () =>
{
    AndroidNode Search(string text, string package = "com.android.settings.intelligence") => new() { ResourceId = "android:id/search_src_text", ClassName = "android.widget.AutoCompleteTextView", PackageName = package, Text = text };
    Check(MultiInstanceAcceptance.IsEmptySettingsSearch(Search("")) && MultiInstanceAcceptance.IsEmptySettingsSearch(Search("\u641c\u7d22\u2026")), "Observed empty field hint rejected");
    Check(!MultiInstanceAcceptance.IsEmptySettingsSearch(Search("\u5b9e\u4f8b1 Round 3")) && !MultiInstanceAcceptance.IsEmptySettingsSearch(Search("\u641c\u7d22\u2026", "other.package")), "Retained text or unrelated control accepted");
    return Task.CompletedTask;
});
Test("null authorization fails closed and malformed serial configuration recovers to safe defaults", async () =>
{
    Check(SettingsStore.Normalize(new AppSettings { AdbAllowedSerials = null! }).AdbAllowedSerials.Length == 0, "Null authorization was not empty");
    var path = Path.Combine(Temp(), "AppConfig.json"); File.WriteAllText(path, "{\"adbAllowedSerials\":[\"valid\",\"-invalid\"]}");
    var store = new SettingsStore(path); Check(store.Load().AdbAllowedSerials.Length == 0, "Malformed authorization survived recovery");
    File.WriteAllText(path + ".bak", JsonSerializer.Serialize(new AppSettings { AdbAllowedSerials = ["old-authorized"], LdSerialBindings = new() { [1] = "old-authorized" }, AgentSerial = "old-authorized", AgentBaseUrl = "http://127.0.0.1:9008/", BasicApiPort = 18080 }, JsonDefaults.Options));
    var recovered = store.Load();
    Check(recovered.AdbAllowedSerials.Length == 0 && recovered.LdSerialBindings.Count == 0 && recovered.AgentSerial is null && recovered.AgentBaseUrl is null && recovered.BasicApiPort == 18080,
        "Backup silently restored device authority or lost ordinary settings");
    File.Delete(path);
    Check(store.Load().AdbAllowedSerials.Length == 0, "Missing primary restored backup device authority");
    await using var runtime = await MatrixRuntime.CreateAsync(recovered, Temp());
    Check(runtime.Devices.Snapshot.Count == 0 && runtime.DiscoveryError is not null, "Recovered configuration cannot open host-only Settings runtime");
});
void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
async Task Throws<T>(Func<Task> action) where T : Exception
{
    try { await action(); } catch (T) { return; }
    throw new Exception("Expected " + typeof(T).Name);
}
string Temp() { var path = Path.Combine(Path.GetTempPath(), "MatrixDevice.Tests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(path); return path; }
ScriptPackage Package(params ScriptStep[] steps) => new() { Manifest = new() { Id = "test-package", Name = "Test", Version = "1.0" }, Steps = steps.ToList() };
JsonElement Body(object body) => JsonSerializer.SerializeToElement(body, JsonDefaults.Options);
int Port() { var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start(); var port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop(); return port; }

Test("coordinate mapping preserves corners and proportions", () =>
{
    Check(InputCoordinator.ScalePoint(180, 320, 360, 640, 412, 720) == (206, 360), "Center mapped incorrectly");
    Check(InputCoordinator.ScalePoint(359, 639, 360, 640, 412, 720) == (410, 718), "Last pixel mapping changed");
    return Throws<DeviceException>(() => Task.FromResult(InputCoordinator.ScalePoint(-1, 0, 360, 640, 412, 720)));
});
Test("touch packet has exact big-endian field layout", () =>
{
    var packet = ScrcpyProtocol.EncodeTouch(0, 0x0102030405060708, 100, 200, 360, 640, 4);
    Check(packet.Length == 32 && packet[0] == 2 && packet[1] == 0, "Packet header incorrect");
    Check(Convert.ToHexString(packet.AsSpan(2, 8)) == "0102030405060708", "Pointer endian incorrect");
    Check(BinaryPrimitives.ReadInt32BigEndian(packet.AsSpan(10)) == 100 && BinaryPrimitives.ReadUInt16BigEndian(packet.AsSpan(20)) == 640, "Position fields incorrect");
    var up = ScrcpyProtocol.EncodeTouch(1, 1, 0, 0, 360, 640, 4);
    Check(up.AsSpan(22, 2).SequenceEqual(new byte[2]) && up.AsSpan(28, 4).SequenceEqual(new byte[4]), "UP retained pressure/buttons");
    return Throws<InvalidDataException>(() => Task.FromResult(ScrcpyProtocol.EncodeTouch(0, 1, 0, 0, 0, 1)));
});
Test("video reader handles flags, short reads and truncated payloads", async () =>
{
    var bytes = new byte[15]; BinaryPrimitives.WriteUInt64BigEndian(bytes, (1UL << 63) | (1UL << 62) | 99); BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(8), 3);
    bytes[12] = 1; bytes[13] = 2; bytes[14] = 3;
    using var stream = new MemoryStream(bytes);
    var packet = await ScrcpyProtocol.ReadPacketAsync(stream);
    Check(packet!.Value.Header.IsCodecConfig && packet.Value.Header.IsKeyFrame && packet.Value.Payload.SequenceEqual(new byte[] { 1, 2, 3 }), "Video flags/payload incorrect");
    using var truncated = new MemoryStream(bytes[..14]);
    await Throws<EndOfStreamException>(async () => { await ScrcpyProtocol.ReadPacketAsync(truncated); });
});
Test("PNG crop and pixel sampling use decoded coordinates", () =>
{
    var png = PngImage.Create(4, 3, (x, y) => ((byte)(x * 50), (byte)(y * 50), (byte)99));
    var cropped = PngImage.Crop(png, 1, 1, 2, 2);
    Check(PngImage.ReadDimensions(cropped) == (2, 2), "Crop dimensions incorrect");
    Check(PngImage.ColorAt(cropped, 1, 1) == ((byte)100, (byte)100, (byte)99), "Crop selected wrong pixels");
    return Throws<InvalidDataException>(() => Task.FromResult(PngImage.Crop(png, 3, 2, 2, 2)));
});
Test("per-serial queue preserves order and allows other devices", async () =>
{
    await using var queue = new SerialCommandQueue();
    List<int> order = [];
    var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var first = queue.RunAsync("a", async ct => { order.Add(1); await release.Task.WaitAsync(ct); order.Add(2); });
    var second = queue.RunAsync("a", ct => { order.Add(3); return Task.CompletedTask; });
    var different = queue.RunAsync("b", ct => Task.CompletedTask);
    await different.WaitAsync(TimeSpan.FromSeconds(2));
    Check(!second.IsCompleted, "Same-device command overtook blocked predecessor");
    release.SetResult(); await Task.WhenAll(first, second);
    Check(order.SequenceEqual(new[] { 1, 2, 3 }), "Same-device order changed");
});
Test("cancelled queue item cannot let successors overtake", async () =>
{
    await using var queue = new SerialCommandQueue();
    var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var first = queue.RunAsync("a", ct => release.Task.WaitAsync(ct));
    using var cancel = new CancellationTokenSource();
    var middle = queue.RunAsync("a", ct => Task.CompletedTask, cancel.Token); cancel.Cancel();
    var last = queue.RunAsync("a", ct => Task.CompletedTask);
    await Task.Delay(20); Check(!last.IsCompleted, "Cancellation released the queue before predecessor");
    release.SetResult(); await first; await Throws<OperationCanceledException>(() => middle); await last;
});
Test("catalog retains offline identity and changes reconnect generation", async () =>
{
    await using var backend = new DemoDeviceBackend(); var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
    var before = catalog.GetRequired("DEMO-001"); var notifications = 0; catalog.DeviceDisconnected += _ => notifications++;
    backend.SetOnline("DEMO-001", false); await catalog.RefreshAsync();
    Check(catalog.Snapshot.Single(x => x.Serial == "DEMO-001").Generation > before.Generation && notifications == 1, "Offline transition did not invalidate old generation");
    await Throws<DeviceException>(() => Task.FromResult(catalog.GetRequired("DEMO-001")));
    await catalog.RefreshAsync(); Check(notifications == 1, "Offline notification repeated without change");
    backend.SetOnline("DEMO-001", true); await catalog.RefreshAsync();
    Check(catalog.GetRequired("DEMO-001").Generation > before.Generation + 1, "Reconnect retained old generation");
});
Test("sync excludes duplicate source and commits one action per device", async () =>
{
    await using var backend = new DemoDeviceBackend(); var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
    await using var input = new InputCoordinator(catalog, backend);
    await input.HandoffAsync("DEMO-001");
    await input.SendAsync(new() { SourceSerial = "DEMO-001", Kind = InputKind.Tap, TargetSerials = ["DEMO-001", "DEMO-002", "DEMO-002"], X = 30, Y = 40 });
    foreach (var serial in new[] { "DEMO-001", "DEMO-002" })
        Check((await backend.DumpAsync(serial)).ChildNodeList[1].Text == "Tap 1", "Duplicate or missing sync delivery: " + serial);
});
Test("source handoff cancels unfinished gestures and rejects stale actions", async () =>
{
    await using var backend = new DemoDeviceBackend(); var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
    await using var input = new InputCoordinator(catalog, backend); var generation = await input.HandoffAsync("DEMO-001");
    var down = new InputSnapshot { SourceSerial = "DEMO-001", Kind = InputKind.Down, TargetSerials = ["DEMO-002"], X = 20, Y = 20, Generation = generation, GestureId = 12 };
    await input.SendAsync(down); await input.HandoffAsync("DEMO-002");
    await Throws<DeviceException>(() => input.SendAsync(down with { Kind = InputKind.Tap }));
    Check((await backend.DumpAsync("DEMO-001")).ChildNodeList[1].Text == "Tap 0", "Handoff committed an unfinished gesture");
});
Test("node search is preorder, case-sensitive and nullable-aware", () =>
{
    var root = new AndroidNode { Text = "Root", Enabled = true, ChildNodeList = [new() { Text = "Match", Clickable = true }, new() { Text = "match", Clickable = false }] };
    var all = NodeSearch.FindAll(root, new NodeSelector());
    Check(all.Count == 3 && ReferenceEquals(all[0], root), "Preorder root lost");
    Check(ReferenceEquals(NodeSearch.Find(root, new() { Text = "Match", Clickable = true }), root.ChildNodeList[0]), "AND match failed");
    Check(NodeSearch.FindAll(root, new() { Clickable = false }).Count == 2, "Nullable false condition confused with absent");
    Check(NodeSearch.Find(root, new() { Text = "MATCH" }) is null, "Search became case-insensitive");
    return Task.CompletedTask;
});
Test("debug node selection preserves reference and does not tap", async () =>
{
    await using var backend = new DemoDeviceBackend(); var debug = new DebugWorkspace(backend); await debug.RefreshAsync("DEMO-001");
    var overlay = debug.Overlays.First(x => x.SourceNode.Clickable); debug.SelectOverlay(overlay);
    Check(ReferenceEquals(debug.SelectedNode, overlay.SourceNode) && overlay.IsSelected, "Tree/overlay selection disconnected");
    Check((await backend.DumpAsync("DEMO-001")).ChildNodeList[1].Text == "Tap 0", "Selecting overlay injected a tap");
});
Test("script package roundtrip preserves metadata and validates actions", async () =>
{
    var path = Path.Combine(Temp(), "sample.jxscript");
    var package = Package(new ScriptStep { Action = "click", Value = "10,20", Probability = 0.75 });
    ScriptPackageService.Save(path, package); var loaded = ScriptPackageService.Load(path);
    Check(loaded.Manifest.Id == package.Manifest.Id && loaded.Steps.Single().Value == "10,20", "Script package changed");
    await Throws<DeviceException>(() => { ScriptPackageService.Validate(Package(new ScriptStep { Action = "eval", Value = "anything" })); return Task.CompletedTask; });
});
Test("script import rejects ZIP traversal and missing entries", async () =>
{
    var path = Path.Combine(Temp(), "bad.jxscript");
    using (var archive = ZipFile.Open(path, ZipArchiveMode.Create)) archive.CreateEntry("../manifest.json");
    await Throws<DeviceException>(() => Task.FromResult(ScriptPackageService.Load(path)));
    var empty = Path.Combine(Temp(), "empty.jxscript"); using (ZipFile.Open(empty, ZipArchiveMode.Create)) { }
    await Throws<DeviceException>(() => Task.FromResult(ScriptPackageService.Load(empty)));
});
Test("script duplicate start runs only once and stops before next step", async () =>
{
    await using var backend = new DemoDeviceBackend(); await using var scripts = new ScriptScheduler(backend);
    var package = Package(new() { Action = "wait", DurationMs = 10000 }, new() { Action = "click", Value = "20,20" });
    var running = scripts.RunAsync("DEMO-001", package); await Task.Delay(30);
    await scripts.RunAsync("DEMO-001", package);
    var stopwatch = Stopwatch.StartNew(); scripts.Stop("DEMO-001"); await running.WaitAsync(TimeSpan.FromSeconds(2));
    Check(stopwatch.Elapsed < TimeSpan.FromSeconds(2), "Cancellation exceeded two seconds");
    Check(scripts.GetStatus("DEMO-001").State == ScriptRunState.Cancelled, "Cancellation reported success");
    Check((await backend.DumpAsync("DEMO-001")).ChildNodeList[1].Text == "Tap 0", "Cancelled script ran next action");
    await scripts.RunAsync("DEMO-001", Package(new ScriptStep { Action = "click", Value = "20,20" }));
    Check((await backend.DumpAsync("DEMO-001")).ChildNodeList[1].Text == "Tap 1", "Restart after cancellation failed");
});
Test("script errors propagate and per-device logs stay at 500", async () =>
{
    await using var backend = new DemoDeviceBackend(); await using var scripts = new ScriptScheduler(backend);
    await Throws<DeviceException>(() => scripts.RunAsync("DEMO-001", Package(new ScriptStep { Action = "click", Value = "99999,99999" })));
    Check(scripts.GetStatus("DEMO-001").State == ScriptRunState.Failed, "Failed action marked completed");
    var steps = Enumerable.Range(0, 550).Select(_ => new ScriptStep { Action = "wait", DurationMs = 0 }).ToArray();
    await scripts.RunAsync("DEMO-002", Package(steps));
    Check(scripts.GetLogs("DEMO-002").Count == 500, "Log retention differs from 500");
});
Test("configuration recovery and three-way merge preserve other writes", () =>
{
    var path = Path.Combine(Temp(), "AppConfig.json"); var first = new SettingsStore(path); first.Load(); first.Save(new() { BasicApiPort = 18880 });
    var second = new SettingsStore(path); var baseline = second.Load();
    first.Save(new() { BasicApiPort = 18880, Language = "en-US", CommandTimeoutSeconds = 40 });
    var merged = second.Save(baseline with { WallColumns = 8 });
    Check(merged.Language == "zh-CN" && merged.CommandTimeoutSeconds == 40 && merged.WallColumns == 8, "Concurrent configuration write lost or English language was retained");
    File.WriteAllText(path, "{broken");
    var recovery = new SettingsStore(path); Check(recovery.Load().Language == "zh-CN" && recovery.Warnings.Count == 2
        && recovery.Warnings.Any(x => x.Contains("设备授权与雷电绑定已清空", StringComparison.Ordinal)), "Backup recovery failed or authority warning missing");
    Check(!File.ReadAllBytes(path + ".bak").Take(3).SequenceEqual(new byte[] { 239, 187, 191 }), "Settings added UTF-8 BOM");
    return Task.CompletedTask;
});
Test("configuration can clear a previously configured nullable field", () =>
{
    var path = Path.Combine(Temp(), "AppConfig.json"); var store = new SettingsStore(path); store.Load();
    var previous = store.Save(new() { AgentBaseUrl = "http://127.0.0.1:9008/", AgentSerial = "test" });
    store.Save(previous with { AgentBaseUrl = null, AgentSerial = null });
    Check(new SettingsStore(path).Load().AgentBaseUrl is null, "Clearing AgentBaseUrl retained the old endpoint");
    return Task.CompletedTask;
});
Test("unconfigured ADB leaves runtime available for Settings", async () =>
{
    await using var runtime = await MatrixRuntime.CreateAsync(new(), Temp());
    Check(runtime.DiscoveryError is not null && runtime.Devices.Snapshot.Count == 0, "Missing ADB path did not expose a setup state");
    await Throws<ArgumentException>(async () => { await MatrixRuntime.CreateAsync(new() { AgentBaseUrl = "http://127.0.0.1:9008" }, Temp()); });
});
Test("occupied startup gateway leaves the Settings runtime available", async () =>
{
    using var occupied = new TcpListener(IPAddress.Loopback, 0); occupied.Start(); var port = ((IPEndPoint)occupied.LocalEndpoint).Port;
    await using var runtime = await MatrixRuntime.CreateAsync(new() { AutoStartApi = true, BasicApiPort = port, AutomatorApiPort = Port() }, Temp(), true);
    Check(runtime.ApiStartupError is not null && !runtime.Api.BasicRunning && runtime.Devices.Snapshot.Count == 2, "API startup failure killed the device runtime or claimed running");
});
Test("corrupt database startup releases its file handle", async () =>
{
    var directory = Temp(); var path = Path.Combine(directory, "db.sqlite3"); await File.WriteAllTextAsync(path, "This is not SQLite");
    await Throws<Microsoft.Data.Sqlite.SqliteException>(async () => { await MatrixRuntime.CreateAsync(new(), directory, true); });
    using var writable = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    Check(writable.Length > 0, "Corrupt database was overwritten");
});
Test("target dimension change invalidates catalog generation", async () =>
{
    await using var backend = new DemoDeviceBackend(); var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
    var before = catalog.GetRequired("DEMO-002"); backend.SetDimensions("DEMO-002", 720, 412); await catalog.RefreshAsync();
    Check(catalog.GetRequired("DEMO-002").Generation > before.Generation, "Rotation retained the input generation");
});
Test("SQLite metadata and entity parameters survive restart", () =>
{
    var path = Path.Combine(Temp(), "db.sqlite3");
    using (var repo = new CatalogRepository(path)) { repo.UpsertMetadata("a' OR 1=1", "Phone", "Group A"); repo.SetRemark("a' OR 1=1", "note"); repo.Put("ScriptDeviceConfiguration", "p/a", new { value = 7 }); }
    using var reopened = new CatalogRepository(path);
    Check(reopened.GetMetadata("a' OR 1=1").Remark == "note" && reopened.GetAllMetadata().Count == 1, "Metadata persistence failed");
    Check(reopened.GetAll<JsonElement>("ScriptDeviceConfiguration").Single().GetProperty("value").GetInt32() == 7, "Entity payload lost");
    return Throws<ArgumentException>(() => Task.FromResult(reopened.GetAll<JsonElement>("User]; DROP TABLE User;--")));
});
Test("phone client maps command/package fields and rejects bad envelopes", async () =>
{
    var handler = new CaptureHandler(); await using var client = new PhoneAgentClient("http://127.0.0.1:9999/", handler);
    await client.ShellAsync("echo 测试");
    Check(handler.Path == "/device/cmd" && handler.Body!.Value.GetProperty("cmd").GetString() == "echo 测试", "Phone protocol command mapped to gateway DTO");
    await client.StartAppAsync("com.matrix.demo"); Check(handler.Body!.Value.TryGetProperty("package", out _), "Phone start lacks package field");
    handler.Response = "not json"; await Throws<DeviceException>(() => client.ServiceInfoAsync());
});
Test("phone client normalizes malformed envelope errors", async () =>
{
    foreach (var json in new[] { "[]", "{\"code\":\"wrong\"}", "{\"code\":7}", "null" })
    {
        var handler = new CaptureHandler { Response = json }; await using var client = new PhoneAgentClient("http://127.0.0.1:9999/", handler);
        await Throws<DeviceException>(() => client.ServiceInfoAsync());
    }
});
Test("all 27 Automator routes have an explicit outcome", async () =>
{
    await using var runtime = await MatrixRuntime.CreateAsync(new(), Temp(), true);
    var samples = new Dictionary<string, object>
    {
        ["device/dump"] = new { }, ["device/screenshot"] = new { }, ["device/shell"] = new { command = "id" }, ["device/execute"] = new { command = "id" },
        ["device/set-prop"] = new { prop = "matrix.test", value = "verified" }, ["device/set-timezone"] = new { timezone = "Asia/Shanghai" }, ["device/set-sim-info"] = new { },
        ["app/list"] = new { }, ["app/list-packages"] = new { }, ["app/start"] = new { packageName = "com.matrix.demo" }, ["app/stop"] = new { packageName = "com.matrix.demo" }, ["app/clear"] = new { packageName = "com.matrix.demo" },
        ["input/tap"] = new { x = 10, y = 20 }, ["input/swipe"] = new { x1 = 10, y1 = 20, x2 = 30, y2 = 40 }, ["input/keyevent"] = new { code = 3 },
        ["service/info"] = new { }, ["service/stop"] = new { }, ["function/sleep"] = new { seconds = 0 }, ["function/sleep-ms"] = new { milliseconds = 0 },
        ["element/find"] = new { selector = new { resourceId = "com.matrix.demo:id/input" } }, ["element/find-all"] = new { selector = new { } },
        ["element/wait-exists"] = new { selector = new { resourceId = "com.matrix.demo:id/input" }, timeoutMs = 0 },
        ["element/click-if-exists"] = new { selector = new { resourceId = "com.matrix.demo:id/input" }, timeoutMs = 0 },
        ["element/click-text"] = new { text = "Settings", timeoutMs = 0 }, ["element/set-text"] = new { selector = new { resourceId = "com.matrix.demo:id/input" }, text = "updated" },
        ["element/wait-any"] = new { selectors = new[] { new { resourceId = "com.matrix.demo:id/input" } }, timeoutMs = 0 },
        ["element/click-node"] = new { node = new AndroidNode { Bounds = new(10, 10, 30, 30) } }
    };
    foreach (var route in AutomationService.Routes)
    {
        var payload = JsonSerializer.SerializeToNode(samples[route], JsonDefaults.Options)!.AsObject();
        payload["serial"] = "DEMO-001";
        try { await runtime.Automation.ExecuteAsync(route, JsonSerializer.SerializeToElement(payload)); }
        catch (DeviceException e) when ((route is "device/set-sim-info" or "service/stop") && e.Status == 501) { }
    }
});
Test("HTTP gateways keep distinct envelopes and invalid requests visible", async () =>
{
    var basicPort = Port(); var autoPort = Port();
    await using var runtime = await MatrixRuntime.CreateAsync(new() { BasicApiPort = basicPort, AutomatorApiPort = autoPort }, Temp(), true);
    await runtime.StartApisAsync(); var devicePort = await runtime.Api.StartDeviceAsync("DEMO-001");
    runtime.Settings = runtime.Settings with { BasicApiPort = basicPort == 65535 ? 65534 : basicPort + 1 };
    Check(runtime.Api.RunningBasicPort == basicPort && runtime.Api.RunningAutomatorPort == autoPort, "Merged settings changed the owned running endpoint");
    Check(await runtime.Api.StartDeviceAsync("DEMO-001") == devicePort, "Repeated start changed serial endpoint");
    using var http = new HttpClient();
    using var health = await http.GetAsync($"http://localhost:{basicPort}/health/"); var basicBody = await health.Content.ReadFromJsonAsync<JsonElement>();
    Check(health.IsSuccessStatusCode && basicBody.GetProperty("ok").GetBoolean() && !basicBody.TryGetProperty("code", out _), "Basic gateway envelope changed");
    using var tap = await http.PostAsJsonAsync($"http://localhost:{devicePort}/tap", new { X = 20, Y = 20 }); Check(tap.IsSuccessStatusCode, "Basic tap failed");
    var beforeBusy = await runtime.Backend.ScreenshotAsync("DEMO-001"); runtime.Api.UiWriteBusy = true;
    using var busyTap = await http.PostAsJsonAsync($"http://localhost:{devicePort}/tap", new { X = 30, Y = 40 }); Check((int)busyTap.StatusCode == 409, "UI busy did not reject API writes");
    using var busyDump = await http.PostAsJsonAsync($"http://localhost:{autoPort}/device/dump", new { serial = "DEMO-001" }); Check(busyDump.IsSuccessStatusCode, "UI busy blocked read-only node queries");
    var afterBusy = await runtime.Backend.ScreenshotAsync("DEMO-001");
    Check(beforeBusy.SequenceEqual(afterBusy), "Busy rejection still mutated the guest"); runtime.Api.UiWriteBusy = false;
    var activeScript = runtime.Scripts.RunAsync("DEMO-001", Package(new ScriptStep { Action = "wait", DurationMs = 10000 }));
    using var scriptTap = await http.PostAsJsonAsync($"http://localhost:{autoPort}/input/tap", new { serial = "DEMO-001", x = 20, y = 20 }); Check((int)scriptTap.StatusCode == 409, "Running script did not reject external writes");
    runtime.Scripts.Stop("DEMO-001"); await activeScript;
    using var malformed = await http.PostAsync($"http://localhost:{devicePort}/tap", new StringContent("{\"X\":\"bad\",\"Y\":20}", Encoding.UTF8, "application/json")); Check((int)malformed.StatusCode == 400, "Bad number became server failure");
    using var autoHealth = await http.GetAsync($"http://localhost:{autoPort}/api/health"); var envelope = await autoHealth.Content.ReadFromJsonAsync<JsonElement>();
    Check(envelope.GetProperty("code").GetInt32() == 0 && !envelope.TryGetProperty("ok", out _), "Automator envelope mixed with basic");
    using var unknown = await http.PostAsJsonAsync($"http://localhost:{autoPort}/unknown", new { serial = "DEMO-001" });
    Check((int)unknown.StatusCode == 404 && (await unknown.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetInt32() == 1006, "Unknown route mapping wrong");
    using var wrongMethod = await http.PutAsJsonAsync($"http://localhost:{autoPort}/input/tap", new { serial = "DEMO-001" }); Check((int)wrongMethod.StatusCode == 405, "Wrong method accepted");
    using var missingSerial = await http.PostAsJsonAsync($"http://localhost:{autoPort}/input/tap", new { x = 1, y = 1 }); Check((int)missingSerial.StatusCode == 400, "Missing serial accepted");
    using var badJson = await http.PostAsync($"http://localhost:{autoPort}/input/tap", new StringContent("{bad", Encoding.UTF8, "application/json")); Check((int)badJson.StatusCode == 400, "Invalid JSON reported success");
    using var sim = await http.PostAsJsonAsync($"http://localhost:{autoPort}/device/set-sim-info", new { serial = "DEMO-001" }); Check((int)sim.StatusCode == 501, "Unknown ROM effect reported success");
    ((DemoDeviceBackend)runtime.Backend).SetOnline("DEMO-001", false); await runtime.RefreshAsync();
    using var offline = await http.GetAsync($"http://localhost:{devicePort}/health"); Check((int)offline.StatusCode == 410, "Offline device reported API ready");
});
Test("Automator crop and ordered wait-any return real backend results", async () =>
{
    await using var runtime = await MatrixRuntime.CreateAsync(new(), Temp(), true);
    var image = Body((await runtime.Automation.ExecuteAsync("device/screenshot", Body(new { serial = "DEMO-001", x = 10, y = 20, width = 30, height = 40 })))!);
    Check(PngImage.ReadDimensions(Convert.FromBase64String(image.GetProperty("base64").GetString()!)) == (30, 40), "API crop ignored request bounds");
    var match = Body((await runtime.Automation.ExecuteAsync("element/wait-any", Body(new { serial = "DEMO-001", selectors = new[] { new NodeSelector { Text = "Settings" }, new NodeSelector { Text = "Tap 0" } }, timeoutMs = 0 })))!);
    Check(match.GetProperty("index").GetInt32() == 0, "WaitAny ignored selector input priority");
    Check(AutomationService.Routes.Count == 27, "Route count differs from API.md");
});
Test("file transfers preserve bytes and device isolation", async () =>
{
    await using var backend = new DemoDeviceBackend(); var folder = Temp(); var original = Path.Combine(folder, "source.bin"); var copied = Path.Combine(folder, "received.bin");
    var payload = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray(); await File.WriteAllBytesAsync(original, payload);
    await backend.PushAsync("DEMO-001", original, "/sdcard/MatrixExchange/test.bin"); await backend.PullAsync("DEMO-001", "/sdcard/MatrixExchange/test.bin", copied);
    Check((await File.ReadAllBytesAsync(copied)).SequenceEqual(payload), "Transfer changed bytes");
    await Throws<DeviceException>(() => backend.PullAsync("DEMO-002", "/sdcard/MatrixExchange/test.bin", copied));
});
Test("occupied device ports are skipped and released ports cool down", async () =>
{
    using var occupied = new TcpListener(IPAddress.Loopback, 8001); try { occupied.Start(); } catch (SocketException) { }
    await using var runtime = await MatrixRuntime.CreateAsync(new(), Temp(), true);
    var first = await runtime.Api.StartDeviceAsync("DEMO-001"); Check(first != 8001, "Occupied port was assigned");
    await runtime.Api.StopDeviceAsync("DEMO-001"); var next = await runtime.Api.StartDeviceAsync("DEMO-002"); Check(next != first, "Cooling endpoint rebound to another serial");
});
Test("ordinary logs flush at shutdown", async () =>
{
    var directory = Temp(); var log = new RollingLog(directory); log.Write("test", "verified message", "DEMO-001"); await log.DisposeAsync();
    Check(Directory.GetFiles(directory, "*.log").Any(path => File.ReadAllText(path).Contains("verified message")), "Queued log did not flush");
});
Test("bounded stream accepts its limit and rejects one extra byte", async () =>
{
    using var exact = new MemoryStream(new byte[8]); Check((await BoundedStreamRead.ReadAsync(exact, 8)).Length == 8, "Exact limit failed");
    using var large = new MemoryStream(new byte[9]); await Throws<DeviceException>(() => BoundedStreamRead.ReadAsync(large, 8));
});
Test("phone screenshot and dump reject malformed data shapes", async () =>
{
    foreach (var json in new[] { "{\"code\":0,\"data\":null}", "{\"code\":0,\"data\":[]}", "{\"code\":0,\"data\":{\"base64\":7}}" })
    {
        var handler = new CaptureHandler { Response = json }; await using var client = new PhoneAgentClient("http://127.0.0.1:9999/", handler);
        await Throws<DeviceException>(() => client.ScreenshotAsync());
    }
    var dumpHandler = new CaptureHandler { Response = "{\"code\":0,\"data\":[]}" };
    await using var dumpClient = new PhoneAgentClient("http://127.0.0.1:9999/", dumpHandler); await Throws<DeviceException>(() => dumpClient.DumpAsync());
});
Test("XML dump rejects external entities and excessive depth", async () =>
{
    await Throws<System.Xml.XmlException>(() => Task.FromResult(AdbDeviceBackend.ParseUiXml("<!DOCTYPE hierarchy [<!ENTITY e SYSTEM 'file:///not-read'>]><hierarchy>&e;</hierarchy>")));
    var deep = "<hierarchy>" + string.Concat(Enumerable.Repeat("<node bounds='[0,0][1,1]'>", 300)) + string.Concat(Enumerable.Repeat("</node>", 300)) + "</hierarchy>";
    await Throws<InvalidDataException>(() => Task.FromResult(AdbDeviceBackend.ParseUiXml(deep)));
});
Test("scan rejects invalid or oversized ranges before touching the network", async () =>
{
    await using var backend = new AdbDeviceBackend(new()); var scanner = new NetworkDeviceScanner(backend);
    await Throws<DeviceException>(() => scanner.ScanAsync("10.0.0.1", "10.0.9.255", 5555));
    await Throws<DeviceException>(() => scanner.ScanAsync("::1", "::1", 5555));
});
Test("unsupported device input uses the documented error code", async () =>
{
    await using var backend = new AdbDeviceBackend(new() { AdbAllowedSerials = ["test"] });
    foreach (var operation in new Func<Task>[] { () => backend.TextAsync("test", "中文"), () => backend.SetNodeTextAsync("test", new(), "中文") })
    {
        try { await operation(); }
        catch (DeviceException e) { Check(e.Status == 501 && e.Code == 2001, "Unsupported input returned an undocumented code"); continue; }
        throw new Exception("Unconfigured Unicode input was accepted");
    }
});
Test("crop outside the device returns a validation error", async () =>
{
    await using var runtime = await MatrixRuntime.CreateAsync(new(), Temp(), true);
    try { await runtime.Automation.ExecuteAsync("device/screenshot", Body(new { serial = "DEMO-001", x = 99999, width = 20, height = 20 })); }
    catch (DeviceException e) { Check(e.Status == 400, "Invalid crop did not return400"); return; }
    throw new Exception("Invalid crop was accepted");
});
Test("180 degree rotation invalidates input even with unchanged dimensions", async () =>
{
    await using var backend = new DemoDeviceBackend(); var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
    var before = catalog.GetRequired("DEMO-001"); backend.SetRotation("DEMO-001", 180); await catalog.RefreshAsync();
    var after = catalog.GetRequired("DEMO-001");
    Check(after.Width == before.Width && after.Height == before.Height && after.Generation > before.Generation, "Same-size rotation retained the old input context");
    var unknown = AdbDeviceBackend.ParseDisplayInfo("Physical size: 360x640", "unrecognized dump"); Check(unknown.Rotation is null, "Unknown orientation silently became zero");
});
Test("queued keys and text cannot cross a source reconnect", async () =>
{
    await using var backend = new KeyGateBackend(); var catalog = new DeviceCatalog(backend); await catalog.RefreshAsync();
    await using var input = new InputCoordinator(catalog, backend); await input.HandoffAsync("DEMO-001");
    var snapshot = new InputSnapshot { SourceSerial = "DEMO-001", TargetSerials = ["DEMO-002"], IncludeSource = false, Kind = InputKind.Key, KeyCode = 4 };
    var first = input.SendAsync(snapshot); await backend.Entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
    var key = input.SendAsync(snapshot with { KeyCode = 3 }); var text = input.SendAsync(snapshot with { Kind = InputKind.Text, Text = "stale" });
    backend.Devices.SetOnline("DEMO-001", false); await catalog.RefreshAsync(); backend.Devices.SetOnline("DEMO-001", true); await catalog.RefreshAsync();
    backend.Release.SetResult(); await first;
    await Throws<DeviceException>(() => key); await Throws<DeviceException>(() => text);
    Check(backend.Actions.Count == 1, "Old source context delivered extra queued actions");
});

Test("LDPlayer parser supports current and legacy Chinese titles", () =>
{
    var latest = LdPlayerService.ParseList2("0,雷电模拟器,4986608,1967810,1,49596,75704,1600,900,240\r\n");
    Check(latest.Count == 1 && latest[0].Name == "雷电模拟器" && latest[0].Width == 1600 && latest[0].VBoxProcessId == 75704, "10-column list changed");
    var legacy = LdPlayerService.ParseList2("2,测试,分身,0,0,0,0,0");
    Check(legacy.Single().Name == "测试,分身" && legacy[0].Width is null, "Legacy comma title lost");
    Check(LdPlayerService.ParseList2("99999,电脑桌面,0,0,0,0,0").Count == 0, "Desktop sentinel became controllable");
    return Task.CompletedTask;
});
Test("observed LDPlayer transition flag two never becomes Android ready", () =>
{
    var row = LdPlayerService.ParseList2("2,MatrixAcceptance,30279482,17367550,2,35824,49164,1280,720,240").Single();
    Check(!row.AndroidStarted && row.ProcessId == 35824 && row.VBoxProcessId == 49164 && row.Width == 1280, "Transition identity was lost or reported ready");
    return Task.CompletedTask;
});
Test("LDPlayer rejects malformed and duplicate instance rows", async () =>
{
    foreach (var malformed in new[] { "0,模拟器,bad,0,1,7,8", "-1,模拟器,0,0,0,0,0", "0,模拟器,0,0,3,0,0", "0,模拟器,0,0,0,0,0\n0,重复,0,0,0,0,0", "0,模拟器,100,200,1,7,8,1,900,240", "0,模拟器,100,200,1,7,8,0,900,240" })
        await Throws<DeviceException>(() => Task.FromResult(LdPlayerService.ParseList2(malformed)));
});
Test("LDPlayer new service epochs reject old clients and remain JSON safe", async () =>
{
    await using var first = new LdPlayerService(new ControlledLdRunner()); await first.RefreshAsync();
    var previous = first.Instances.Single().Generation;
    await using var second = new LdPlayerService(new ControlledLdRunner()); await second.RefreshAsync();
    var current = second.Instances.Single().Generation;
    Check(current != previous && current <= 9_007_199_254_740_991 && (long)(double)current == current, "Epoch reused or cannot roundtrip JavaScript number");
    await Throws<DeviceException>(() => second.ExecuteAsync(0, "input", new JsonObject { ["expectedGeneration"] = previous, ["text"] = "过期服务请求" }));
});
Test("LDPlayer generations are stable but invalidate restarted instances", async () =>
{
    var runner = new ControlledLdRunner(); await using var service = new LdPlayerService(runner);
    await service.RefreshAsync(); var first = service.Instances.Single().Generation;
    await service.RefreshAsync(); Check(service.Instances.Single().Generation == first, "Polling invalidated the UI selection");
    runner.List = "0,雷电测试,100,200,1,8,10,1600,900,240";
    await service.RefreshAsync(); Check(service.Instances.Single().Generation > first, "Restart retained stale generation");
    await Throws<DeviceException>(() => service.ExecuteAsync(0, "input", new JsonObject { ["expectedGeneration"] = first, ["text"] = "中文" }));
    Check(runner.Commands.Count == 0, "Stale command reached console");
});
Test("LDPlayer guards confirmations whitelist and command fields", async () =>
{
    var runner = new ControlledLdRunner(); await using var service = new LdPlayerService(runner); await service.RefreshAsync();
    var generation = service.Instances.Single().Generation;
    foreach (var operation in new[] { "quit", "reboot", "quitall", "remove", "restore", "modify" })
        await Throws<DeviceException>(() => service.ExecuteAsync(0, operation, new JsonObject { ["expectedGeneration"] = generation }));
    await Throws<DeviceException>(() => service.ExecuteAsync(0, "keyevent", new JsonObject { ["expectedGeneration"] = generation, ["key"] = "menu" }));
    await Throws<DeviceException>(() => service.ExecuteAsync(0, "runapp", new JsonObject { ["expectedGeneration"] = generation, ["packageName"] = "--index 7" }));
    await Throws<DeviceException>(() => service.ExecuteAsync(0, "input", new JsonObject { ["text"] = "中文" }));
    Check(runner.Commands.Count == 0, "Invalid request performed a command");
    var result = await service.ExecuteAsync(0, "input", new JsonObject { ["expectedGeneration"] = generation, ["text"] = "中文 空格\"引号" });
    Check(result.Accepted && !result.Verified, "Input claimed verified device effect");
    Check(runner.Commands.Single().SequenceEqual(new[] { "action", "--index", "0", "--key", "call.input", "--value", "中文 空格\"引号" }), "Chinese input arguments changed");
});
Test("LDPlayer removed indices do not reuse old generations", async () =>
{
    var runner = new ControlledLdRunner(); await using var service = new LdPlayerService(runner); await service.RefreshAsync();
    var first = service.Instances.Single().Generation; var original = runner.List;
    runner.List = ""; await service.RefreshAsync(); runner.List = original; await service.RefreshAsync();
    Check(service.Instances.Single().Generation > first, "Recreated instance reused generation");
    await Throws<DeviceException>(() => service.ExecuteAsync(0, "keyevent", new JsonObject { ["expectedGeneration"] = first, ["key"] = "home" }));
    Check(runner.Commands.Count == 0, "Removed instance accepted stale action");
});
Test("LDPlayer queued input rejects restart and allows another instance", async () =>
{
    var runner = new GatedLdRunner(); await using var service = new LdPlayerService(runner); await service.RefreshAsync();
    var generation = service.Instances.First(x => x.Index == 0).Generation;
    var first = service.ExecuteAsync(0, "input", new JsonObject { ["expectedGeneration"] = generation, ["text"] = "第一条" });
    await runner.Entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
    var next = service.ExecuteAsync(0, "input", new JsonObject { ["expectedGeneration"] = generation, ["text"] = "过期条目" });
    await service.ExecuteAsync(1, "keyevent", new JsonObject { ["expectedGeneration"] = service.Instances.First(x => x.Index == 1).Generation, ["key"] = "home" }).WaitAsync(TimeSpan.FromSeconds(2));
    Check(!next.IsCompleted, "Same-instance queue overtook its predecessor");
    runner.Restarted = true; await service.RefreshAsync(); runner.Release.SetResult(); await first;
    await Throws<DeviceException>(() => next);
    Check(runner.Writes.Count == 2, "Stale queued input reached restarted instance");
});
Test("LDPlayer real runner preserves argument boundaries and GBK output", async () =>
{
    var executable = Environment.ProcessPath!; var assembly = typeof(ControlledLdRunner).Assembly.Location;
    var prefix = Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase) ? new[] { assembly, "--ldconsole-test-child" } : new[] { "--ldconsole-test-child" };
    var runner = new LdConsoleRunner(executable, 5);
    var values = new[] { "中文 空格", "a\"b", "C:\\测试 路径\\x.apk", "--index" };
    var output = await runner.RunAsync(prefix.Concat(new[] { "echo" }).Concat(values).ToArray(), CancellationToken.None);
    Check(JsonSerializer.Deserialize<string[]>(output)!.SequenceEqual(values), "Process arguments lost boundaries");
    var gbk = await runner.RunAsync(prefix.Concat(new[] { "gbk" }).ToArray(), CancellationToken.None);
    Check(LdPlayerService.ParseList2(gbk).Single().Name == "雷电测试", "GBK Chinese title corrupted");
    await Throws<DeviceException>(() => runner.RunAsync(prefix.Concat(new[] { "fail" }).ToArray(), CancellationToken.None));
});
Test("LDPlayer runner cancels times out and limits its own output", async () =>
{
    var executable = Environment.ProcessPath!; var assembly = typeof(ControlledLdRunner).Assembly.Location;
    var prefix = Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase) ? new[] { assembly, "--ldconsole-test-child" } : new[] { "--ldconsole-test-child" };
    var runner = new LdConsoleRunner(executable, 1);
    try { await runner.RunAsync(prefix.Concat(new[] { "sleep" }).ToArray(), CancellationToken.None); throw new Exception("Timeout was accepted"); }
    catch (DeviceException e) { Check(e.Status == 504, "Timeout did not preserve its API status"); }
    using var cancellation = new CancellationTokenSource(100);
    await Throws<OperationCanceledException>(() => runner.RunAsync(prefix.Concat(new[] { "sleep" }).ToArray(), cancellation.Token));
    foreach (var stream in new[] { "flood", "stderr-flood" })
    {
        try { await new LdConsoleRunner(executable, 5).RunAsync(prefix.Concat(new[] { stream }).ToArray(), CancellationToken.None); throw new Exception("Output limit accepted"); }
        catch (DeviceException e) { Check(e.Status == 413, "Output size error did not preserve 413"); }
    }
});
Test("LDPlayer confirmed demo lifecycle reports observed states and fresh tokens", async () =>
{
    await using var service = new LdPlayerService(new DemoLdConsoleRunner(), true); await service.RefreshAsync();
    var before = service.Instances.Single(x => x.Index == 1);
    var launch = await service.ExecuteAsync(1, "launch", new JsonObject { ["expectedGeneration"] = before.Generation });
    Check(launch.Accepted && launch.Verified && launch.Instance?.AndroidStarted == true && launch.Instance.Generation > before.Generation, "Launch state not observed");
    var stop = await service.ExecuteAsync(1, "quit", new JsonObject { ["expectedGeneration"] = launch.Instance!.Generation, ["confirmed"] = true });
    Check(stop.Accepted && stop.Verified && stop.Instance?.AndroidStarted == false && stop.Instance.Generation > launch.Instance.Generation, "Confirmed quit not observed");
    await Throws<DeviceException>(() => service.ExecuteAsync(1, "input", new JsonObject { ["expectedGeneration"] = stop.Instance!.Generation, ["text"] = "停机拒绝" }));
});
Test("LDPlayer timeout never kills descendants of its CLI process", async () =>
{
    var executable = Environment.ProcessPath!; var assembly = typeof(ControlledLdRunner).Assembly.Location;
    var prefix = Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase) ? new[] { assembly, "--ldconsole-test-child" } : new[] { "--ldconsole-test-child" };
    var pidFile = Path.Combine(Temp(), "test-child.pid"); Process? child = null;
    try
    {
        try { await new LdConsoleRunner(executable, 2).RunAsync(prefix.Concat(new[] { "spawn", pidFile }).ToArray(), CancellationToken.None); throw new Exception("CLI did not time out"); }
        catch (DeviceException e) { Check(e.Status == 504, "Wrong timeout status"); }
        Check(File.Exists(pidFile), "Test descendant was not started");
        child = Process.GetProcessById(int.Parse(await File.ReadAllTextAsync(pidFile)));
        Check(!child.HasExited, "Timeout killed a descendant which may represent the emulator");
    }
    finally
    {
        if (child is null && File.Exists(pidFile)) { try { child = Process.GetProcessById(int.Parse(await File.ReadAllTextAsync(pidFile))); } catch (ArgumentException) { } }
        if (child is not null) { if (!child.HasExited) { child.Kill(entireProcessTree: false); await child.WaitForExitAsync(); } child.Dispose(); }
    }
});
Test("LDPlayer HTTP routes separate support from verified effects", async () =>
{
    var port = Port(); await using var runtime = await MatrixRuntime.CreateAsync(new() { BasicApiPort = port }, Temp(), true);
    await runtime.Api.StartBasicAsync(port); using var http = new HttpClient { BaseAddress = new Uri($"http://localhost:{port}/") };
    using var instances = await http.GetAsync("ldplayer/instances"); var body = await instances.Content.ReadFromJsonAsync<JsonElement>();
    Check(body.GetProperty("ok").GetBoolean() && body.GetProperty("data").GetArrayLength() == 2, "Demo LD instances unavailable");
    var selected = body.GetProperty("data")[0]; var generation = selected.GetProperty("generation").GetInt64(); var index = selected.GetProperty("index").GetInt32();
    using var capabilities = await http.GetAsync("ldplayer/capabilities"); Check(capabilities.IsSuccessStatusCode, "Capabilities unavailable");
    using var missingConfirm = await http.PostAsJsonAsync($"ldplayer/instances/{index}/quit", new { expectedGeneration = generation }); Check(!missingConfirm.IsSuccessStatusCode, "Quit lacked confirmation");
    using var text = await http.PostAsJsonAsync($"ldplayer/instances/{index}/input", new { expectedGeneration = generation, text = "测试" });
    Check(text.IsSuccessStatusCode && !(await text.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("verified").GetBoolean(), "Text API claimed guest effect");
    using var malformed = await http.PostAsJsonAsync($"ldplayer/instances/{index}/input", new { expectedGeneration = "bad", text = "测试" }); Check((int)malformed.StatusCode == 400, "Malformed generation not rejected");
    using var wrongContent = await http.PostAsync($"ldplayer/instances/{index}/input", new StringContent("{}")); Check((int)wrongContent.StatusCode == 415, "Simple cross-site content accepted");
    using var crossSite = new HttpRequestMessage(HttpMethod.Post, $"ldplayer/instances/{index}/input") { Content = JsonContent.Create(new { expectedGeneration = generation, text = "测试" }) };
    crossSite.Headers.Add("Origin", "http://example.invalid"); using var rejected = await http.SendAsync(crossSite); Check((int)rejected.StatusCode == 403, "Cross-site controller accepted");
    using var blocked = await http.PostAsJsonAsync($"ldplayer/instances/{index}/quitall", new { expectedGeneration = generation, confirmed = true }); Check(!blocked.IsSuccessStatusCode, "Global quit exposed");
});
Test("invalid LDPlayer configuration leaves Chinese settings available", async () =>
{
    await using var runtime = await MatrixRuntime.CreateAsync(new() { LdConsolePath = "missing-ldconsole.exe", Language = "en-US" }, Temp());
    Check(runtime.LdPlayer is null && runtime.LdPlayerError is not null, "Bad CLI path destroyed settings runtime");
    Check(SettingsStore.Normalize(runtime.Settings).Language == "zh-CN", "English persisted");
});
Test("LDPlayer API script write guard isolates uniquely bound instances and keeps unknown mappings conservative", async () =>
{
    var basicPort = Port(); var autoPort = Port();
    await using var runtime = await MatrixRuntime.CreateAsync(new() { BasicApiPort = basicPort, AutomatorApiPort = autoPort,
        AdbAllowedSerials = ["DEMO-001", "DEMO-002"], LdSerialBindings = new() { [0] = "DEMO-001", [1] = "DEMO-002" } }, Temp(), true);
    await runtime.StartApisAsync();
    var initial = await runtime.LdPlayer!.RefreshAsync();
    await runtime.LdPlayer.ExecuteAsync(1, "launch", new JsonObject { ["expectedGeneration"] = initial.Single(x => x.Index == 1).Generation });
    runtime.Api.LdSerialForWriteGuard = index => index switch { 0 => "DEMO-001", 1 => "DEMO-002", _ => null };
    runtime.Api.ScriptWriteBusy = serial => serial == "DEMO-001";
    runtime.Api.AnyScriptWriteBusy = () => true;
    using var http = new HttpClient();
    var rows = await runtime.LdPlayer!.RefreshAsync();
    async Task<HttpResponseMessage> Send(int index) => await http.PostAsJsonAsync($"http://localhost:{basicPort}/ldplayer/instances/{index}/keyevent",
        new { expectedGeneration = rows.Single(x => x.Index == index).Generation, key = "home" });
    using var first = await Send(0); Check((int)first.StatusCode == 409, "Busy instance write was not rejected");
    using var second = await Send(1); Check(second.IsSuccessStatusCode, "Script on A blocked unrelated B");
    runtime.Api.LdSerialForWriteGuard = null;
    using var unknown = await Send(1); Check((int)unknown.StatusCode == 409, "Unknown mapping bypassed conservative script guard");
    runtime.Api.UiWriteBusy = true;
    using var uiBusy = await Send(1); Check((int)uiBusy.StatusCode == 409, "Global UI busy protection was lost");
});
Test("all device writes reject browser simple and cross-site requests", async () =>
{
    var basicPort = Port(); var autoPort = Port();
    await using var runtime = await MatrixRuntime.CreateAsync(new() { BasicApiPort = basicPort, AutomatorApiPort = autoPort }, Temp(), true);
    await runtime.StartApisAsync(); var devicePort = await runtime.Api.StartDeviceAsync("DEMO-001"); using var http = new HttpClient();
    foreach (var url in new[] { $"http://localhost:{devicePort}/tap", $"http://localhost:{autoPort}/input/tap" })
    {
        using var plain = await http.PostAsync(url, new StringContent("{\"serial\":\"DEMO-001\",\"x\":20,\"y\":20}"));
        Check((int)plain.StatusCode == 415, "Simple cross-site content accepted: " + url);
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(new { serial = "DEMO-001", x = 20, y = 20 }) };
        request.Headers.Add("Origin", "http://example.invalid"); using var crossSite = await http.SendAsync(request);
        Check((int)crossSite.StatusCode == 403, "Cross-site Origin accepted: " + url);
    }
    Check((await runtime.Backend.DumpAsync("DEMO-001")).ChildNodeList[1].Text == "Tap 0", "Rejected request had guest effect");
});
Test("serial binding edits cannot mutate the configuration merge baseline", () =>
{
    var path = Path.Combine(Temp(), "AppConfig.json"); var first = new SettingsStore(path); first.Load();
    first.Save(new() { LdSerialBindings = new() { [0] = "emulator-5554" } });
    var second = new SettingsStore(path); var loaded = second.Load();
    loaded.LdSerialBindings[0] = "explicit-new-serial";
    first.Save(first.Load() with { WallColumns = 8 });
    var merged = second.Save(loaded);
    Check(merged.LdSerialBindings[0] == "explicit-new-serial" && merged.WallColumns == 8, "In-place binding edit lost during merge");
    merged.LdSerialBindings[1] = "serial-1"; second.Save(merged);
    Check(new SettingsStore(path).Load().LdSerialBindings[1] == "serial-1", "Save result exposed internal baseline map");
    return Task.CompletedTask;
});
Test("concurrent serial binding edits merge by instance index", () =>
{
    var path = Path.Combine(Temp(), "AppConfig.json"); var first = new SettingsStore(path); var second = new SettingsStore(path);
    var a = first.Load(); var b = second.Load(); a.LdSerialBindings[0] = "serial-a"; first.Save(a);
    b.LdSerialBindings[1] = "serial-b"; var merged = second.Save(b);
    Check(merged.LdSerialBindings.Count == 2 && merged.LdSerialBindings[0] == "serial-a" && merged.LdSerialBindings[1] == "serial-b", "Independent instance binding was lost");
    a = first.Load(); b = second.Load(); a.LdSerialBindings.Remove(0); first.Save(a);
    b.LdSerialBindings[1] = "serial-b-new"; merged = second.Save(b);
    Check(!merged.LdSerialBindings.ContainsKey(0) && merged.LdSerialBindings[1] == "serial-b-new", "Untouched local key resurrected a removed binding");
    return Task.CompletedTask;
});

var liveIndex = Array.IndexOf(args, "--ldplayer-live-query");
if (liveIndex >= 0)
{
    var path = args.ElementAtOrDefault(liveIndex + 1) ?? throw new ArgumentException("Missing ldconsole path");
    Test("LIVE read-only LDPlayer HTTP list without ADB client", async () =>
    {
        var port = Port(); await using var runtime = await MatrixRuntime.CreateAsync(new() { BasicApiPort = port, LdConsolePath = path }, Temp());
        await runtime.Api.StartBasicAsync(port); using var http = new HttpClient();
        using var result = await http.GetAsync($"http://localhost:{port}/ldplayer/instances");
        Check(result.IsSuccessStatusCode, "Real read-only API query failed");
        Console.WriteLine("LIVE-LDPLAYER " + await result.Content.ReadAsStringAsync());
        Check(runtime.Devices.Snapshot.Count == 0 && runtime.Settings.AdbPath == "", "Read-only query entered ADB discovery");
    });
}

var failures = 0; var totalTimer = Stopwatch.StartNew();
foreach (var (name, run) in tests)
{
    try { await run().WaitAsync(TimeSpan.FromSeconds(30)); Console.WriteLine("PASS " + name); }
    catch (Exception error) { failures++; Console.WriteLine("FAIL " + name + "\n" + error); }
}
Console.WriteLine($"RESULT {tests.Count - failures}/{tests.Count} passed in {totalTimer.Elapsed.TotalSeconds:F2}s");
Environment.ExitCode = failures == 0 ? 0 : 1;

sealed class CaptureHandler : HttpMessageHandler
{
    public string Path { get; private set; } = "";
    public JsonElement? Body { get; private set; }
    public string Response { get; set; } = "{\"code\":0,\"message\":\"ok\",\"data\":\"done\"}";
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Path = request.RequestUri!.AbsolutePath;
        Body = request.Content is null ? null : JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken)).RootElement.Clone();
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Response, Encoding.UTF8, "application/json") };
    }
}

sealed class ControlledLdRunner : ILdConsoleRunner
{
    public string List { get; set; } = "0,雷电测试,100,200,1,7,9,1600,900,240";
    public List<string[]> Commands { get; } = [];
    public Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (arguments[0] == "list2") return Task.FromResult(List);
        if (arguments[0] == "isrunning") return Task.FromResult("running");
        Commands.Add(arguments.ToArray()); return Task.FromResult("");
    }
}

sealed class GatedLdRunner : ILdConsoleRunner
{
    public bool Restarted { get; set; }
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public ConcurrentBag<string[]> Writes { get; } = [];
    public async Task<string> RunAsync(IReadOnlyList<string> arguments, CancellationToken ct)
    {
        if (arguments[0] == "list2") return $"0,测试零,100,200,1,{(Restarted ? 17 : 7)},9,1600,900,240\n1,测试一,101,201,1,8,10,1280,720,240";
        Writes.Add(arguments.ToArray());
        if (arguments[2] == "0") { Entered.TrySetResult(); await Release.Task.WaitAsync(ct); }
        return "";
    }
}

sealed class KeyGateBackend : IDeviceBackend
{
    public DemoDeviceBackend Devices { get; } = new();
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public ConcurrentBag<string> Actions { get; } = [];
    public Task<IReadOnlyList<DeviceInfo>> DiscoverAsync(CancellationToken ct = default) => Devices.DiscoverAsync(ct);
    public async Task KeyEventAsync(string serial, int code, CancellationToken ct = default)
    { if (code == 4) { Entered.SetResult(); await Release.Task.WaitAsync(ct); } Actions.Add(serial + ":key:" + code); }
    public Task TextAsync(string serial, string text, CancellationToken ct = default) { Actions.Add(serial + ":text:" + text); return Task.CompletedTask; }
    public Task<string> ShellAsync(string s, string c, bool root = false, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<byte[]> ScreenshotAsync(string s, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<AndroidNode> DumpAsync(string s, CancellationToken ct = default) => throw new NotSupportedException();
    public Task TapAsync(string s, int x, int y, CancellationToken ct = default) => throw new NotSupportedException();
    public Task SwipeAsync(string s, int x, int y, int x2, int y2, int ms, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<string> StartAppAsync(string s, string p, string? a = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<string> StopAppAsync(string s, string p, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ClearAppAsync(string s, string p, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<AppInfo>> ListAppsAsync(string s, bool system = false, bool version = true, CancellationToken ct = default) => throw new NotSupportedException();
    public Task InstallAsync(string s, string p, CancellationToken ct = default) => throw new NotSupportedException();
    public Task UninstallAsync(string s, string p, CancellationToken ct = default) => throw new NotSupportedException();
    public Task PushAsync(string s, string p, string r, IProgress<double>? progress = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task PullAsync(string s, string r, string p, IProgress<double>? progress = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task SetNodeTextAsync(string s, NodeSelector selector, string text, CancellationToken ct = default) => throw new NotSupportedException();
    public ValueTask DisposeAsync() => Devices.DisposeAsync();
}

```

## tools/Build.ps1

```powershell
param([string] $DotnetPath, [switch] $Verify)
$ErrorActionPreference = 'Stop'
$projectPath = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path
if (-not $DotnetPath) {
    $bundled = Join-Path (Split-Path -Parent $projectPath) '.tools/dotnet/dotnet.exe'
    $DotnetPath = if (Test-Path -LiteralPath $bundled) { $bundled } else { (Get-Command dotnet -ErrorAction Stop).Source }
}
$DotnetPath = (Resolve-Path -LiteralPath $DotnetPath).Path
$sdkVersion = & $DotnetPath --version
if ($LASTEXITCODE -ne 0) { throw 'Cannot read .NET SDK version' }
$sdkRoot = Join-Path (Split-Path -Parent $DotnetPath) ('sdk/' + $sdkVersion)
$oldDotnet = $env:MATRIX_DOTNET_EXE
$oldCompiler = $env:MATRIX_CSC_DLL
try {
    $env:MATRIX_DOTNET_EXE = $DotnetPath
    $env:MATRIX_CSC_DLL = Join-Path $sdkRoot 'Roslyn/bincore/csc.dll'
    $buildArgs = @('build', (Join-Path $projectPath 'MatrixDevice.slnx'), '-c', 'Release', '--nologo', '-m:1', '-p:UseSharedCompilation=false', '-p:CscToolExe=csc-managed.cmd', ('-p:CscToolPath=' + $PSScriptRoot))
    & $DotnetPath @buildArgs
    if ($LASTEXITCODE -ne 0) { throw 'Project build failed' }
    if ($Verify) {
        & $DotnetPath (Join-Path $projectPath 'tests/MatrixDevice.Tests/bin/Release/net10.0/MatrixDevice.Tests.dll')
        if ($LASTEXITCODE -ne 0) { throw 'Verification failed' }
    }
} finally {
    $env:MATRIX_DOTNET_EXE = $oldDotnet
    $env:MATRIX_CSC_DLL = $oldCompiler
}

```

## tools/csc-managed.cmd

```batch
@echo off
if not defined MATRIX_DOTNET_EXE exit /b 2
if not defined MATRIX_CSC_DLL exit /b 2
"%MATRIX_DOTNET_EXE%" exec "%MATRIX_CSC_DLL%" %*
exit /b %errorlevel%

```

## tools/Get-AdbHostState.ps1

```powershell
param([Parameter(Mandatory)] [string] $OutputDirectory)
$ErrorActionPreference = 'Stop'
if (-not [IO.Path]::IsPathFullyQualified($OutputDirectory)) { throw '证据目录必须为绝对路径。' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
function Read-Exact([IO.Stream] $Stream, [int] $Length) {
    $bytes = [byte[]]::new($Length)
    $offset = 0
    while ($offset -lt $Length) {
        $count = $Stream.Read($bytes, $offset, $Length - $offset)
        if ($count -eq 0) { throw 'ADB host响应截断。' }
        $offset += $count
    }
    return ,$bytes
}
function Read-HostQuery([string] $Request) {
    if ($Request -notin @('host:version', 'host:devices-l')) { throw '仅允许主机只读查询。' }
    $client = [Net.Sockets.TcpClient]::new()
    try {
        $connection = $client.ConnectAsync([Net.IPAddress]::Loopback, 5037)
        if (-not $connection.Wait(3000)) { throw '现有ADB服务连接超时；不启动服务。' }
        $stream = $client.GetStream()
        $stream.ReadTimeout = 3000
        $stream.WriteTimeout = 3000
        $body = [Text.Encoding]::UTF8.GetBytes($Request)
        $prefix = [Text.Encoding]::ASCII.GetBytes($body.Length.ToString('X4'))
        $stream.Write($prefix)
        $stream.Write($body)
        $status = [Text.Encoding]::ASCII.GetString((Read-Exact $stream 4))
        $length = [Convert]::ToInt32([Text.Encoding]::ASCII.GetString((Read-Exact $stream 4)), 16)
        $result = [Text.Encoding]::UTF8.GetString((Read-Exact $stream $length))
        if ($status -ne 'OKAY') { throw "ADB host拒绝：$result" }
        return $result
    }
    finally { $client.Dispose() }
}
$owners = @(Get-NetTCPConnection -LocalPort 5037 -State Listen -ErrorAction SilentlyContinue |
    Select-Object -ExpandProperty OwningProcess -Unique | ForEach-Object {
        $process = Get-CimInstance Win32_Process -Filter "ProcessId=$_"
        [ordered]@{
            processId = $_
            executable = $process.ExecutablePath
            createdUtc = $process.CreationDate.ToUniversalTime().ToString('o')
            sha256 = (Get-FileHash -LiteralPath $process.ExecutablePath -Algorithm SHA256).Hash
        }
    })
if ($owners.Count -gt 1) { throw "ADB5037所有者不唯一，实际=$($owners.Count)；不创建或停止服务。" }
$snapshot = [ordered]@{
    capturedUtc = [DateTime]::UtcNow.ToString('o')
    owner = if ($owners.Count -eq 1) { $owners[0] } else { $null }
    protocol = if ($owners.Count -eq 1) { Read-HostQuery 'host:version' } else { $null }
    devices = if ($owners.Count -eq 1) { Read-HostQuery 'host:devices-l' } else { $null }
    guestCommands = 0
}
$path = Join-Path $OutputDirectory ('adb-host-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff') + '.json')
$snapshot | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $path -Encoding utf8
$snapshot | ConvertTo-Json -Depth 6

```

## tools/Invoke-HttpToolkitMcp.ps1

```powershell
param([Parameter(Mandatory)][string] $OutputPath, [string] $ToolName = '', [string] $ArgumentsJson = '{}')
$ErrorActionPreference = 'Stop'
if (-not [IO.Path]::IsPathFullyQualified($OutputPath)) { throw '输出路径须为绝对路径。' }
$command = Join-Path $env:LOCALAPPDATA 'Programs\HTTP Toolkit\resources\httptoolkit-mcp.cmd'
if (-not (Test-Path -LiteralPath $command -PathType Leaf)) { throw '未安装官方HTTP Toolkit MCP CLI。' }
$start = [Diagnostics.ProcessStartInfo]::new('cmd.exe')
$start.Arguments = '/d /s /c ""' + $command + '""'
$start.UseShellExecute = $false; $start.CreateNoWindow = $true; $start.WindowStyle = 'Hidden'
$start.RedirectStandardInput = $true; $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
$process = [Diagnostics.Process]::Start($start)
$errorRead = $process.StandardError.ReadToEndAsync()
$responses = [Collections.Generic.List[object]]::new()
function Call-Rpc([int] $Id, [string] $Method, [object] $Parameters) {
    $request = @{jsonrpc='2.0'; id=$Id; method=$Method; params=$Parameters}
    $process.StandardInput.WriteLine(($request | ConvertTo-Json -Depth 40 -Compress)); $process.StandardInput.Flush()
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    while ([DateTime]::UtcNow -lt $deadline) {
        $read = $process.StandardOutput.ReadLineAsync()
        if (-not $read.Wait(45000)) { throw "HTTP Toolkit MCP超时：$Method" }
        if ($null -eq $read.Result) { throw 'HTTP Toolkit MCP输出提前结束。' }
        $response = $read.Result | ConvertFrom-Json -Depth 80
        $responses.Add($response)
        if ($response.id -eq $Id) { return $response }
    }
    throw "HTTP Toolkit MCP超时：$Method"
}
try {
    [void](Call-Rpc 1 'initialize' @{protocolVersion='2025-11-25'; capabilities=@{}; clientInfo=@{name='matrix-multi-acceptance';version='1.0'}})
    $process.StandardInput.WriteLine('{"jsonrpc":"2.0","method":"notifications/initialized"}'); $process.StandardInput.Flush()
    $listed = Call-Rpc 2 'tools/list' @{}
    if ($ToolName) {
        $tool = @($listed.result.tools | Where-Object name -eq $ToolName)
        if ($tool.Count -ne 1 -or $tool[0].annotations.readOnlyHint -ne $true) { throw '仅允许当前官方列表中明确标记只读的工具。' }
        $arguments = $ArgumentsJson | ConvertFrom-Json -AsHashtable -Depth 80
        $called = Call-Rpc 3 'tools/call' @{name=$ToolName;arguments=$arguments}
        if ($called.error -or $called.result.isError) { throw 'HTTP Toolkit拒绝此只读调用，结果未计为捕获成功。' }
    }
} finally {
    New-Item -ItemType Directory -Path (Split-Path -Parent $OutputPath) -Force | Out-Null
    $responses | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $OutputPath -Encoding utf8NoBOM
    try { $process.StandardInput.Close() } catch [IO.IOException] { Write-Warning $_.Exception.Message }
    if (-not $process.WaitForExit(3000)) { $process.Kill() }
    if ($errorRead.Wait(3000)) { $errorRead.Result | Set-Content -LiteralPath ($OutputPath + '.stderr.txt') -Encoding utf8NoBOM }
    $process.Dispose()
}
if ($ToolName) { $called | ConvertTo-Json -Depth 40 }
else { $listed.result.tools | Select-Object name,description,annotations,inputSchema | ConvertTo-Json -Depth 30 }

```

## tools/Measure-MultiEvidence.ps1

```powershell
param([Parameter(Mandatory)][string] $Directory, [Parameter(Mandatory)][string] $SpecFile)
$ErrorActionPreference = 'Stop'
$directoryPath = (Resolve-Path -LiteralPath $Directory).Path
function Read-Journal([string] $Name) {
    @(Get-Content -LiteralPath (Join-Path $directoryPath $Name) | Where-Object { $_.Trim() } | ConvertFrom-Json)
}
function Percentile([double[]] $Values, [double] $Fraction) {
    if ($Values.Count -eq 0) { throw 'No values for percentile' }
    $ordered = @($Values | Sort-Object)
    $ordered[[math]::Max(0, [math]::Ceiling($ordered.Count * $Fraction) - 1)]
}
$summaries = Read-Journal 'summary.jsonl'
if ($summaries.Count -ne 1 -or $summaries[0].status -ne 'PASS-LIVE-MULTI') { throw 'Only a completed successful run can be analyzed as acceptance evidence' }
$summary = $summaries[0]
$specPath = (Resolve-Path -LiteralPath $SpecFile).Path
$spec = Get-Content -LiteralPath $specPath -Raw | ConvertFrom-Json
$manifest = Get-Content -LiteralPath (Join-Path $directoryPath 'run-manifest.json') -Raw | ConvertFrom-Json
if ((Get-FileHash -LiteralPath $specPath -Algorithm SHA256).Hash -ne $manifest.specSha256) { throw 'Run manifest does not match the supplied specification' }
if ($spec.minutes -lt 1 -or $spec.minutes -gt 30 -or $summary.pressureMs -lt $spec.minutes * 60000 -or $summary.targetCount -ne $spec.targets.Count) { throw 'Completed duration or target count is below specification' }
$cases = Read-Journal 'results.jsonl'
if (@($cases | Where-Object { $_.status -ne 'PASS-LIVE' }).Count) { throw 'Failed cases cannot be counted as successful acceptance' }
foreach ($case in $cases) {
    $file = [IO.Path]::GetFullPath((Join-Path $directoryPath $case.file))
    if (-not $file.StartsWith($directoryPath + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) { throw 'Case path is outside evidence directory' }
    if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $case.sha256) { throw "Case hash mismatch: $($case.file)" }
}
$rounds = @($cases | Where-Object { $_.task -match '^round-\d+$' })
$returns = Read-Journal 'returns.jsonl'
if ($rounds.Count -ne $returns.Count) { throw 'Round and return counts differ' }
$http = Read-Journal 'http.jsonl'
$requestIds = @($http.id | Select-Object -Unique)
if ($requestIds.Count -ne $http.Count) { throw 'Duplicate HTTP request ID' }
$keys = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($returned in $returns) {
    $target = @($spec.targets | Where-Object { $_.serial -eq $returned.serial -and $_.index -eq $returned.index })
    if ($target.Count -ne 1) { throw 'Return is not from a unique specified target' }
    if (-not $keys.Add("$($returned.serial):$($returned.round)")) { throw 'Duplicate device/round return' }
    $case = @($rounds | Where-Object { $_.serial -eq $returned.serial -and $_.task -eq "round-$($returned.round)" })
    if ($case.Count -ne 1) { throw 'Return has no unique successful round' }
    $payload = Get-Content -LiteralPath (Join-Path $directoryPath $case[0].file) -Raw | ConvertFrom-Json
    if ($payload.nonce -ne $returned.nonce -or $payload.sha256 -ne $returned.sha256 -or $payload.text -ne $returned.text) { throw 'Return does not match recorded actual round' }
    $ack = @($http | Where-Object { $_.task -eq 'data-return' -and $_.body.serial -eq $returned.serial -and $_.body.round -eq $returned.round })
    if ($ack.Count -ne 1 -or $ack[0].status -ne 200) { throw 'Missing unique HTTP acknowledgement' }
    if ($ack[0].body.sha256 -ne $returned.sha256 -or $ack[0].body.nonce -ne $returned.nonce -or $ack[0].body.text -ne $returned.text) { throw 'HTTP return payload does not match actual round' }
    $reply = $ack[0].response | ConvertFrom-Json
    if (-not $reply.accepted -or $reply.serial -ne $returned.serial -or $reply.nonce -ne $returned.nonce) { throw 'Incorrect HTTP acknowledgement' }
    $textRequests = @($http | Where-Object { $_.task -eq 'text-input' -and $_.serial -eq $returned.serial -and $_.body.text -eq $returned.text })
    if ($textRequests.Count -ne 1 -or $textRequests[0].status -ne 200) { throw 'Missing unique successful text request' }
    $identity = @($http | Where-Object { $_.task -eq 'identity-return' -and $_.serial -eq $returned.serial -and $_.response.Contains($returned.sha256, [StringComparison]::OrdinalIgnoreCase) })
    if ($identity.Count -ne 1 -or $identity[0].status -ne 200) { throw 'Missing unique successful device readback' }
    $readback = ($identity[0].response | ConvertFrom-Json).data.output
    if (-not $readback.StartsWith($returned.nonce, [StringComparison]::Ordinal) -or -not $readback.Contains($returned.sha256, [StringComparison]::OrdinalIgnoreCase)) { throw 'Device readback does not match instance nonce and file hash' }
}
$pressureStart = @($rounds | Sort-Object { [datetimeoffset] $_.start })[0].start
$resources = @(Read-Journal 'resources.jsonl' | Where-Object { [datetimeoffset] $_.utc -ge [datetimeoffset] $pressureStart })
if ($resources.Count -lt 2) { throw 'Not enough pressure resource samples' }
$workers = Read-Journal 'workers.jsonl'
$deviceStats = foreach ($group in $returns | Group-Object serial) {
    $serial = $group.Name
    $items = @($rounds | Where-Object { $_.serial -eq $serial } | Sort-Object { [datetimeoffset] $_.start })
    $end = @($workers | Where-Object { $_.serial -eq $serial -and $_.state -eq 'end' })
    if ($end.Count -ne 1 -or $end[0].rounds -ne $items.Count -or $end[0].returns -ne $items.Count) { throw 'Worker totals do not match successful rounds' }
    $summaryCount = @($summary.returns.PSObject.Properties | Where-Object { $_.Name -eq $serial })
    if ($summaryCount.Count -ne 1 -or $summaryCount[0].Value -ne $items.Count) { throw 'Summary return totals do not match successful rounds' }
    $nonce = @($group.Group.nonce | Select-Object -Unique)
    if ($nonce.Count -ne 1) { throw 'Worker nonce changed' }
    $gaps = @(for ($i = 1; $i -lt $items.Count; $i++) { ([datetimeoffset]$items[$i].end - [datetimeoffset]$items[$i-1].end).TotalMilliseconds })
    [pscustomobject]@{
        Serial = $serial; Index = $group.Group[0].index; Rounds = $items.Count
        UploadMiB = ($group.Group.bytes | Measure-Object -Sum).Sum / 1MB
        MedianRoundMs = Percentile $items.durationMs .5; P95RoundMs = Percentile $items.durationMs .95
        MaxRoundMs = ($items.durationMs | Measure-Object -Maximum).Maximum
        MaxReturnGapMs = ($gaps | Measure-Object -Maximum).Maximum
        FirstStart = $items[0].start; LastEnd = $items[-1].end
    }
}
if (@($deviceStats | Select-Object -ExpandProperty Index -Unique).Count -ne $summary.targetCount) { throw 'Target count mismatch' }
if (@($returns | Group-Object nonce).Count -ne $summary.targetCount) { throw 'Different devices shared a nonce' }
if (@($http | Where-Object { $_.proxy -ne $spec.proxyPort }).Count) { throw 'HTTP proxy port does not match specification' }
$processStats = foreach ($pidGroup in @($resources.processes | Group-Object pid)) {
    $items = @($pidGroup.Group)
    [pscustomobject]@{
        Pid = [int]$pidGroup.Name; Name = $items[0].processName
        FirstWorkingSetMiB = $items[0].workingSet64 / 1MB; LastWorkingSetMiB = $items[-1].workingSet64 / 1MB
        PeakWorkingSetMiB = ($items.workingSet64 | Measure-Object -Maximum).Maximum / 1MB
        FirstPrivateMiB = $items[0].privateMemorySize64 / 1MB; LastPrivateMiB = $items[-1].privateMemorySize64 / 1MB
        PeakPrivateMiB = ($items.privateMemorySize64 | Measure-Object -Maximum).Maximum / 1MB
        FirstHandles = $items[0].handleCount; LastHandles = $items[-1].handleCount
        PeakHandles = ($items.handleCount | Measure-Object -Maximum).Maximum
        CpuTimeDeltaMs = $items[-1].cpuTimeMs - $items[0].cpuTimeMs
    }
}
[pscustomobject]@{
    Summary = $summary; SpecSha256 = $manifest.specSha256; VerifiedCaseHashes = $cases.Count; RoundReturnPairs = $returns.Count
    DeviceStats = @($deviceStats); HttpCount = $http.Count
    HttpStatuses = @($http | Group-Object status | Select-Object Name,Count)
    HttpProxyPorts = @($http.proxy | Select-Object -Unique)
    HttpMedianMs = Percentile $http.durationMs .5; HttpP95Ms = Percentile $http.durationMs .95
    PressureResourceSamples = $resources.Count
    CpuMedianPercent = Percentile $resources.cpuPercent .5; CpuP95Percent = Percentile $resources.cpuPercent .95
    CpuMaxPercent = ($resources.cpuPercent | Measure-Object -Maximum).Maximum
    MinAvailableGiB = ($resources.availablePhysical | Measure-Object -Minimum).Minimum / 1GB
    MaxCommitPercent = (@($resources | ForEach-Object { 100 * (1 - $_.availablePageFile / $_.totalPageFile) }) | Measure-Object -Maximum).Maximum
    ProcessStats = @($processStats)
    Limits = @('File-only validation; no live host or guest checks or current DLL hash verification', 'Direct HTTP journals are not HTTP Toolkit captures', 'Per-process working sets are not summed as deduplicated physical RAM', 'No GPU or long-term leak conclusion')
} | ConvertTo-Json -Depth 10

```

## tools/New-AcceptanceInstance.ps1

```powershell
param([Parameter(Mandatory)] [string] $OutputDirectory,
      [string] $ConsolePath = 'E:\leidian\LDPlayer14\ldconsole.exe')
$ErrorActionPreference = 'Stop'
if (-not [IO.Path]::IsPathFullyQualified($OutputDirectory)) { throw '证据目录必须为绝对路径。' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$name = 'MatrixAcceptance-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
$before = @(& $ConsolePath list2)
if ($LASTEXITCODE -ne 0) { throw '创建前list2失败。' }
$before | Set-Content -LiteralPath (Join-Path $OutputDirectory 'ld-before.txt') -Encoding utf8
$indices = @($before | Where-Object { $_ -match '^\d+,' } | ForEach-Object { [int]($_ -split ',')[0] })
$output = @(& $ConsolePath add --name $name)
$code = $LASTEXITCODE
[IO.File]::WriteAllLines((Join-Path $OutputDirectory 'ld-add.txt'), [string[]]$output)
$after = @(& $ConsolePath list2)
if ($LASTEXITCODE -ne 0) { throw '创建后list2失败；不重试add。' }
$after | Set-Content -LiteralPath (Join-Path $OutputDirectory 'ld-after-add.txt') -Encoding utf8
$added = @($after | Where-Object { $_ -match '^\d+,' -and [int]($_ -split ',')[0] -notin $indices })
if ($added.Count -ne 1) { throw "新增实例不唯一，actual=$($added.Count)；不启动。" }
$fields = $added[0] -split ','
if ($fields[1] -ne $name) { throw '新增实例名称不匹配；不启动。' }
$index = [int]$fields[0]
$receipt = [ordered]@{ createdUtc = [DateTime]::UtcNow.ToString('o'); index = $index; name = $name; beforeIndices = $indices; createdRow = $added[0]; addExitCode = $code; result = if ($code -eq 0) { 'CREATED' } else { 'CREATED-WITH-NONZERO-EXIT' }; copied = $false; retained = $true }
$receipt | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $OutputDirectory 'instance.json') -Encoding utf8
$receipt | ConvertTo-Json -Depth 5

```

## tools/New-SourcePackage.ps1

```powershell
param([string] $ProjectRoot = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = 'Stop'
$projectPath = (Resolve-Path -LiteralPath $ProjectRoot).Path
$artifactDirectory = Join-Path $projectPath 'artifacts'
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
$sources = @(Get-ChildItem -LiteralPath $projectPath -Recurse -File | Where-Object {
    $relative = [IO.Path]::GetRelativePath($projectPath, $_.FullName)
    $relative -notmatch '(^|[\\/])(bin|obj|artifacts|data|\.git)([\\/]|$)' -and
    $_.Name -notin @('SOURCE_BUNDLE.md', 'PROJECT_TREE.md', 'SOURCE_MANIFEST.json') -and
    $_.Name -notlike 'desktop-english*'
} | Sort-Object FullName)
$tree = @('# 完整项目文件目录', '', '```text', 'MatrixDevice.Reconstructed/')
$relativePaths = @($sources | ForEach-Object { [IO.Path]::GetRelativePath($projectPath, $_.FullName).Replace('\', '/') })
function Format-Tree([string[]] $Paths, [string] $Prefix = '') {
    $parts = @($Paths | ForEach-Object { $_.Split('/')[0] } | Sort-Object -Unique)
    for ($index = 0; $index -lt $parts.Count; $index++) {
        $name = $parts[$index]
        $children = @($Paths | Where-Object { $_.StartsWith($name + '/', [StringComparison]::Ordinal) } | ForEach-Object { $_.Substring($name.Length + 1) })
        $last = $index -eq $parts.Count - 1
        $branch = if ($last) { '+-- ' } else { '|-- ' }
        $suffix = if ($children.Count -gt 0) { '/' } else { '' }
        $Prefix + $branch + $name + $suffix
        if ($children.Count -gt 0) {
            $continuation = if ($last) { '    ' } else { '|   ' }
            Format-Tree -Paths $children -Prefix ($Prefix + $continuation)
        }
    }
}
$tree += @(Format-Tree -Paths ($relativePaths + @('PROJECT_TREE.md','SOURCE_MANIFEST.json','SOURCE_BUNDLE.md')))
$manifest = foreach ($source in $sources) {
    $relative = [IO.Path]::GetRelativePath($projectPath, $source.FullName).Replace('\', '/')
    [pscustomobject]@{ Path = $relative; Bytes = $source.Length; Sha256 = (Get-FileHash -LiteralPath $source.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
}
$tree += '```'
Set-Content -LiteralPath (Join-Path $projectPath 'PROJECT_TREE.md') -Value $tree -Encoding utf8NoBOM
$manifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $projectPath 'SOURCE_MANIFEST.json') -Encoding utf8NoBOM
$bundle = [Text.StringBuilder]::new()
[void] $bundle.AppendLine('# 全部源代码与构建文件')
foreach ($source in $sources | Where-Object { $_.Extension -in @('.cs','.xaml','.csproj','.props','.slnx','.ps1','.cmd','.manifest','.json','.Config') }) {
    $relative = [IO.Path]::GetRelativePath($projectPath, $source.FullName).Replace('\', '/')
    $language = switch ($source.Extension.ToLowerInvariant()) { '.cs' { 'csharp' } '.ps1' { 'powershell' } '.cmd' { 'batch' } '.json' { 'json' } default { 'xml' } }
    [void] $bundle.AppendLine("`n## $relative`n")
    [void] $bundle.AppendLine('```' + $language)
    [void] $bundle.AppendLine([IO.File]::ReadAllText($source.FullName))
    [void] $bundle.AppendLine('```')
}
[IO.File]::WriteAllText((Join-Path $projectPath 'SOURCE_BUNDLE.md'), $bundle.ToString(), [Text.UTF8Encoding]::new($false))
$archivePath = Join-Path $artifactDirectory ('MatrixDevice.Source.' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.zip')
$stream = [IO.File]::Open($archivePath, [IO.FileMode]::CreateNew)
try {
    $archive = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create, $true)
    try {
        foreach ($source in $sources) {
            $relative = [IO.Path]::GetRelativePath($projectPath, $source.FullName).Replace('\', '/')
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $source.FullName, 'MatrixDevice.Reconstructed/' + $relative) | Out-Null
        }
        foreach ($name in @('PROJECT_TREE.md','SOURCE_MANIFEST.json','SOURCE_BUNDLE.md')) {
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, (Join-Path $projectPath $name), 'MatrixDevice.Reconstructed/' + $name) | Out-Null
        }
    } finally { $archive.Dispose() }
} finally { $stream.Dispose() }
[pscustomobject]@{ Archive = $archivePath; SourceFiles = $sources.Count; Sha256 = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash } | ConvertTo-Json

```

## tools/Start-Desktop.ps1

```powershell
param([switch] $Demo, [string] $LdConsole = '', [string] $DataDirectory = '')
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$portable = Join-Path (Split-Path -Parent $root) '.tools\dotnet\dotnet.exe'
$dotnet = if (Test-Path -LiteralPath $portable -PathType Leaf) { $portable } else { (Get-Command dotnet -CommandType Application -ErrorAction Stop).Source }
$assembly = Join-Path $root 'src\MatrixDevice.Desktop\bin\Release\net10.0-windows\MatrixDevice.Desktop.dll'
if (-not (Test-Path -LiteralPath $assembly -PathType Leaf)) { throw '请先运行 tools/Build.ps1 构建桌面程序。' }
$arguments = @($assembly)
if ($Demo) { $arguments += '--demo' }
if ($DataDirectory) {
    if (-not [IO.Path]::IsPathFullyQualified($DataDirectory)) { throw '配置目录必须是绝对路径。' }
    $arguments += @('--data-dir', $DataDirectory)
}
if ($LdConsole) {
    if (-not [IO.Path]::IsPathFullyQualified($LdConsole) -or -not (Test-Path -LiteralPath $LdConsole -PathType Leaf)) { throw '雷电命令行程序必须是已存在的绝对路径。' }
    $arguments += @('--ldconsole', $LdConsole)
}
& $dotnet @arguments
if ($LASTEXITCODE -ne 0) { throw "桌面程序异常退出，退出码：$LASTEXITCODE" }

```

## tools/Start-IsolatedHttpToolkit.ps1

```powershell
param([Parameter(Mandatory)][string] $EvidenceDirectory)
$ErrorActionPreference = 'Stop'
if (-not [IO.Path]::IsPathFullyQualified($EvidenceDirectory)) { throw '证据目录须为绝对路径。' }
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$missingSdk = Join-Path $root 'artifacts\http-toolkit-no-android-sdk'
if (Test-Path -LiteralPath $missingSdk) { throw '隔离SDK路径必须不存在，不能让HTTP Toolkit找到adb。' }
if (Get-NetTCPConnection -State Listen -LocalPort 65534 -ErrorAction SilentlyContinue) { throw 'ADB隔离端口65534已被占用。' }
if (Get-Process -Name 'HTTP Toolkit' -ErrorAction SilentlyContinue) { throw 'HTTP Toolkit已运行，不能保证其继承隔离环境。' }
$exe = Join-Path $env:LOCALAPPDATA 'Programs\HTTP Toolkit\HTTP Toolkit.exe'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) { throw '未安装HTTP Toolkit，本脚本不自动安装。' }
New-Item -ItemType Directory -Path $EvidenceDirectory -Force | Out-Null
$start = [Diagnostics.ProcessStartInfo]::new($exe)
$start.UseShellExecute = $false
$start.WindowStyle = [Diagnostics.ProcessWindowStyle]::Hidden
$start.Environment['ANDROID_ADB_SERVER_PORT'] = '65534'
$start.Environment['ANDROID_HOME'] = $missingSdk
$start.Environment['ADB_SERVER_SOCKET'] = 'tcp:127.0.0.1:65534'
$process = [Diagnostics.Process]::Start($start)
if ($null -eq $process) { throw 'HTTP Toolkit未启动。' }
$receipt = [pscustomobject]@{ StartedUtc = [DateTime]::UtcNow.ToString('O'); ProcessId = $process.Id;
    StartedUtcTicks = $process.StartTime.ToUniversalTime().Ticks; Executable = $exe;
    Sha256 = (Get-FileHash -LiteralPath $exe).Hash; AdbPort = 65534; AndroidHome = $missingSdk;
    ChangedGlobalEnvironment = $false; InstalledCertificate = $false; ChangedSystemProxy = $false }
$receipt | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'isolated-start.json') -Encoding utf8NoBOM
$receipt | ConvertTo-Json

```

## tools/Start-MultiVerifiedDesktop.cmd

```batch
@echo off
chcp 65001 >nul
where pwsh.exe >nul 2>nul
if errorlevel 1 (
    echo 需要 PowerShell 7。请在 PowerShell 7 中运行 Start-MultiVerifiedDesktop.ps1。
    pause
    exit /b 1
)
pwsh.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0Start-MultiVerifiedDesktop.ps1"
if errorlevel 1 pause

```

## tools/Start-MultiVerifiedDesktop.ps1

```powershell
param()
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$directory = Join-Path $root 'data\multi-verified'
$configuration = Join-Path $directory 'AppConfig.json'
if (-not (Test-Path -LiteralPath $configuration -PathType Leaf)) { throw '没有本机多实例授权配置。请先按README确认串号和索引绑定，不从公式猜测设备。' }
$settings = Get-Content -LiteralPath $configuration -Raw | ConvertFrom-Json
$serials = @($settings.adbAllowedSerials)
$bindings = @($settings.ldSerialBindings.PSObject.Properties)
if ($serials.Count -lt 2 -or $bindings.Count -ne $serials.Count -or @($serials | Select-Object -Unique).Count -ne $serials.Count) { throw '多实例配置必须有至少两个不同串号及一一对应绑定。' }
foreach ($binding in $bindings) {
    $index = 0
    if (-not [int]::TryParse($binding.Name, [ref] $index) -or $index -le 0 -or $index -ge 99999 -or $binding.Name -ne $index.ToString([Globalization.CultureInfo]::InvariantCulture) -or $binding.Value -notin $serials) { throw '该启动入口只允许规范索引的非零独立测试实例，不允许桌面哨兵或重复索引别名。' }
}
if (@($bindings.Value | Select-Object -Unique).Count -ne $serials.Count) { throw '多个索引不能绑定同一串号。' }
& (Join-Path $PSScriptRoot 'Start-Desktop.ps1') -DataDirectory $directory

```

## tools/Start-VerifiedDesktop.cmd

```batch
@echo off
where pwsh.exe >nul 2>nul
if errorlevel 1 (
    echo PowerShell 7 is required. Run Start-VerifiedDesktop.ps1 in PowerShell 7.
    pause
    exit /b 1
)
pwsh.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0Start-VerifiedDesktop.ps1"
if errorlevel 1 pause

```

## tools/Start-VerifiedDesktop.ps1

```powershell
param()
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$directory = Join-Path $root 'data\verified-acceptance'
$configuration = Join-Path $directory 'AppConfig.json'
if (-not (Test-Path -LiteralPath $configuration -PathType Leaf)) { throw '没有本机已核验的独立实例配置。请先按README配置，不能从索引公式猜测串号。' }
$settings = Get-Content -LiteralPath $configuration -Raw | ConvertFrom-Json
if (@($settings.adbAllowedSerials).Count -ne 1 -or @($settings.ldSerialBindings.PSObject.Properties).Count -ne 1) { throw '独立测试配置缺少唯一授权串号及显式索引绑定。' }
& (Join-Path $PSScriptRoot 'Start-Desktop.ps1') -DataDirectory $directory

```

## tools/Write-AcceptanceResult.ps1

```powershell
param(
    [Parameter(Mandatory)] [string] $OutputDirectory,
    [Parameter(Mandatory)] [string] $CaseId,
    [Parameter(Mandatory)] [ValidateSet('PASS-OFFLINE','PASS-LIVE','FAIL','BLOCKED','NOT-IMPLEMENTED','N/A')] [string] $Status,
    [Parameter(Mandatory)] [string] $Expected,
    [Parameter(Mandatory)] [string] $Actual,
    [string] $Phase = 'T02', [string] $Authorization = 'offline',
    [string] $Serial = '', [int] $Index = -1, [long] $Generation = 0,
    [string] $DefectId = '', [int] $Round = 1, [long] $DurationMs = 0,
    [string[]] $EvidenceFiles = @()
)
$ErrorActionPreference = 'Stop'
if (-not [IO.Path]::IsPathFullyQualified($OutputDirectory)) { throw '测试日志目录必须为绝对路径。' }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$assembly = Join-Path $root 'src\MatrixDevice.Core\bin\Release\net10.0\MatrixDevice.Core.dll'
$hash = if (Test-Path -LiteralPath $assembly) { (Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash } else { $null }
$artifacts = @($EvidenceFiles | ForEach-Object {
    $file = Get-Item -LiteralPath $_ -ErrorAction Stop
    [ordered]@{ path = [IO.Path]::GetRelativePath($root, $file.FullName); bytes = $file.Length; sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash }
})
$record = [ordered]@{
    caseId = $CaseId; recordedUtc = [DateTime]::UtcNow.ToString('o'); phase = $Phase
    recordingBuildSha256 = $hash; executionBuildSha256 = $null; canonicalSerial = $Serial; targetIndex = $Index; generation = $Generation
    authorization = $Authorization; expected = $Expected; actual = $Actual; status = $Status
    durationMs = $DurationMs; defectId = $DefectId; round = $Round; artifacts = $artifacts
    cleanup = '见对应阶段记录'; measurement = 'manual-summary'
}
$record | ConvertTo-Json -Depth 8 -Compress | Add-Content -LiteralPath (Join-Path $OutputDirectory 'results.jsonl') -Encoding utf8

```
