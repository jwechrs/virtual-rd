---
layout: default
title: Error messages
nav_order: 2
has_children: false
---

# Error messages
## `Network error occured`
- `SocketException` from C# `System.Net.Sockets`
- The app failed to access the host name/port provided.
- Make sure that your firewall settings is correct.
- If you used string host name, please try using IP address (e.g. `192.168.0.1`, `127.0.0.1`)

## `Socket error, failed to send the password`
- The network is disconencted while trying to send the password

## `login failure`
- Wrong password

## `Please use password authentication`
- Could not understand the requested authentication type.
- This app only supports password authentication.

## `The encoding from the server is not supported: ***`
- The server requested the encodoing type which is not supported on this app.
- This app supports `Hextile` and `Raw` encodings.
