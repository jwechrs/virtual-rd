---
layout: default
title: Usage
nav_order: 2
has_children: false
---
# VNC connection
## VNC servers tested
This is the list of the VNC servers tested.
All VNC servers that supports the specifications would work fine.

| VNC Server              | VNC server version | OS           |
|-------------------------|--------------------|--------------|
| TigerVNC                | 1.7.0              | Ubuntu 18.04 |
| TightVNC                | 2.8.27             | Windows 10   |
| Build-in screen sharing |                    | macOS        |

{% include_relative usage.md %}
## VNC connection over USB
You can use `adb reverse` command to establish the VNC connection over the USB without WiFi.
### Example (assuming you are using port 5900)
- Run `adb reverse tcp:5900 tcp:5900` on your VNC server PC
- Launch VRD app and access host name: `127.0.0.1`, port: 5900 from VRD.

**NOTE**: Using "localhost" as a host name may cause `SocketException`. Please use `127.0.0.1` instead. 
