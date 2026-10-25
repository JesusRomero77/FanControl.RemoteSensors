# FanControl.RemoteSensors

A [FanControl](https://github.com/Rem0o/FanControl.Releases) plugin that shows, inside your FanControl, the sensors of another computer running [FanControl.DataServer](https://github.com/JesusRomero77/FanControl.DataServer). Each remote sensor appears in FanControl with the name `REMOTE <sensor name>`.

## WARNING: FOR MONITORING ONLY

**REMOTE SENSORS ARE FOR MONITORING THE OTHER COMPUTER ONLY. NEVER USE THEM IN CURVES OR CONTROLS OF THE COMPUTER WHERE THIS PLUGIN IS INSTALLED.**

- If the remote data stops, is wrong or the other computer is switched off, this computer could lose the cooling it needs and its hardware could be permanently damaged.
- Local hardware could also be permanently damaged if its cooling depends on a remote sensor instead of on its own sensor.

## What it does

- Reads the sensors of the active profile of the remote FanControl once per second and publishes each of them as a FanControl sensor named `REMOTE <name>`.
- If the remote data stops updating for more than 5 seconds, or the remote computer cannot be reached, the sensors show no value (empty) instead of a frozen reading.
- Only sensors are imported. Remote controls are not imported.
- Read-only: it never sends anything to the remote computer except read requests.

## Requirements

- FanControl installed (the .NET Framework 4.8 version) on this computer.
- On the remote computer: FanControl with the [FanControl.DataServer](https://github.com/JesusRomero77/FanControl.DataServer) plugin, its FanControl interface open and port 8765 reachable (see that project for network permissions and firewall).

## Installation

### Using FanControl's plugin installer (recommended)

1. Download `FanControl.RemoteSensors.dll` from the Releases page.
2. In FanControl, open the plugin installer and select the downloaded DLL.
3. Restart the FanControl interface.

### Manual installation

1. Download `FanControl.RemoteSensors.dll` from the Releases page.
2. Copy it to the `Plugins` folder of your FanControl installation.
3. Restart the FanControl interface.

## Configuration

The first time the plugin loads it creates `RemoteSensorsConfig.xml` in `%LocalAppData%\FanControl`, with empty values. Fill in the computer name and/or the IP address of the remote computer (one of the two is enough) and restart the FanControl interface:

```xml
<?xml version="1.0"?>
<ClientSettings>
  <PcName>PC-NAME</PcName>
  <Ip>192.168.1.50</Ip>
</ClientSettings>
```

If both are filled in, the IP address is used. The port is fixed at 8765, the same as DataServer.

Until it is configured, the plugin only adds a dummy sensor called "Remote Sensors" (FanControl requires every plugin to add at least one sensor), which shows no value and can be ignored.

## Operation

- The FanControl interface must be open on **both** computers: the plugin only works while the interface is open here, and DataServer only serves up-to-date data while the interface is open on the remote computer.
- The list of remote sensors is created when the FanControl interface starts. The plugin remembers the last list it received (in `RemoteSensorsProfileCache.json`, in the same folder), so the sensors exist even if the remote computer is switched off at that moment. They show no value until it answers.
- If the remote profile changes and has different sensors, the plugin writes a warning in FanControl's error log. Restart the FanControl interface to refresh the list of `REMOTE` sensors.

## Known limitations

- Only the sensors that DataServer serves (the custom sensors of the remote profile) are available.
- The list of sensors cannot change while the interface is open; a restart is needed.
- No authentication: it relies on DataServer, which anyone in the network can read. Never expose that port to the internet.
- The port (8765) is fixed in the source code.

## Building from source

Open `Source/FanControl.RemoteSensors.csproj` in Visual Studio with the .NET desktop development workload and the .NET Framework 4.8 targeting pack, with FanControl installed (the project looks for its DLLs in `C:\Program Files (x86)\FanControl\`; if it is installed elsewhere, change the path of each FanControl reference in the `.csproj`). Build in Release for x64.

## Related projects

- [FanControl.DataServer](https://github.com/JesusRomero77/FanControl.DataServer): serves the sensors and controls of a FanControl profile over HTTP. Required on the remote computer.
- [FanControl.CurveReader](https://github.com/JesusRomero77/FanControl.CurveReader): logs temperature and PWM changes to a file.

## Disclaimer

This project is not affiliated with FanControl or its author.

## License

This project is licensed under the [MIT License](LICENSE).

FanControl and the libraries it provides to plugins (such as Newtonsoft.Json) are not part of this repository and remain under their own licenses.

## Version

1.0
