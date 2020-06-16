# Oculus VNC © Client
- RFB protocol for Linux(/macOS)
- VNC and RFB are trademarks of RealVNC Limited.
- apk file coming soon
## How to install
- Download `build.apk` from [github release page][https://github.com/jwechrs/virtual-rd/releases] or from [SideQuesst][https://sidequestvr.com/app/1123/vrd].
- Install `adb`. See [android document][https://developer.android.com/studio/command-line/adb?hl=ja] for more detail.
- Install by running `adb install build.apk`
- Start VNC server on your computer. 
- (Optional) If you wish to connect to your computer over USB, you can use `adb reverse` command.
### example (when VNC is running on port 5900)
- `adb reverse tcp:5900 tcp:5900`
- Access host name: `127.0.0.1`, port: 5900 from VRD.

## Supported operations
- Screen display
- Pointer control using oculus controller
## Project page
[Github project page](https://jwechrs.github.io/virtual-rd/)
