---
layout: default
title: Usage
nav_order: 2
has_children: false
---
# VNC connection
{% include_relative Environment/index.md %}

{% include_relative usage.md %}
## VNC connection over USB
You can use `adb reverse` command to establish the VNC connection over the USB without WiFi.
### Example (assuming you are using port 5900)
- Run `adb reverse tcp:5900 tcp:5900` on your VNC server PC
- Launch VRD app and access host name: `127.0.0.1`, port: 5900 from VRD.

**NOTE**: Using "localhost" as a host name may cause `SocketException`. Please use `127.0.0.1` instead.

## Pointer control
- Trigger index button on the oculus controller is mapped to left click event.
- The touchpad scroll on the oculus controller is mapped to the scroll wheel event.
