## App usage

1. Download apk from <a href="https://sidequestvr.com/app/1123/vrd" >Sidequest</a> or from <a href="https://github.com/jwechrs/virtual-rd/releases">Github</a>
2. Install the application to Oculus device by running `adb install /path/to/build.apk`
3. Start VNC server on your PC
4. From Oculus device, go to "unknown sources" page and start VRD
5. Access your host name / port of a VNC server.
   1. example: host name=192.168.0.2, port=5900

**NOTE:** C# TcpClient module appears to reject string host names such as `localhost`. Using integer host names (e.g. 127.0.0.1) is recommended.
