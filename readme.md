
# Oculus VNC Client

-   Remote desktop client using RFB protocol for Linux(/macOS)
-   VNC and RFB are trademarks of RealVNC Limited.

<div style="text-align: center;">
<a href="https://jwechrs.github.io/virtual-rd/">Document page</a>
</div>

## How to install

-   Download \`build.apk\` from [github release page](https://github.com/jwechrs/virtual-rd/releases) or from [SideQuesst](https://sidequestvr.com/app/1123/vrd).
-   Install \`adb\`. See [android document](https://developer.android.com/studio/command-line/adb?hl=ja) for more detail.
-   Install by running \`adb install build.apk\`
-   Start VNC server on your computer.
-   Access your VNC server by specifying host/port.



### (Optional) If you wish to connect to your computer over USB, you can use \`adb reverse\` command.

Example:

-   \`adb reverse tcp:5900 tcp:5900\`
-   Access host name: \`127.0.0.1\`, port: 5900 from VRD.



## Supported operations

-   Screen display
-   Pointer control using Oculus controller
-   Scroll using touchpad on Oculus Controller


## Project page
[Github project page](https://jwechrs.github.io/virtual-rd/)

## VRD-core
This directory contains the bare-bone VRD unity project.
### Requirements
- Oculus Integration (from unity assets store)
- Oculus toolkit OVRInputSelection (from [this documentation](https://developer.oculus.com/blog/easy-controller-selection/)). Please follow the documentation to attach necessary scripts to OVRCameraRig etc.
They are licensed under the [Oculus SDK License Agreement](http://www.oculusvr.com/licenses/)(Copyright © Facebook Technologies, LLC and its affiliates. All rights reserved).

`VRD-core/Assets/Scripts` is licensed under the MIT license.

