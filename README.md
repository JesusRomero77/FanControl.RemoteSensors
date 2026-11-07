# FanControl.RemoteSensors

A [FanControl](https://github.com/Rem0o/FanControl.Releases) plugin that shows, inside your FanControl, the sensors of another computer running [FanControl.DataServer](https://github.com/JesusRomero77/FanControl.DataServer). Each remote sensor appears in FanControl with the name `REMOTE <sensor name>`.

## WARNING: FOR MONITORING ONLY

**REMOTE SENSORS ARE FOR MONITORING THE OTHER COMPUTER ONLY. NEVER USE THEM IN CURVES OR CONTROLS OF THE COMPUTER WHERE THIS PLUGIN IS INSTALLED.**

- If the remote data stops, is wrong or the other computer is switched off, this computer could lose the cooling it needs and its hardware could be permanently damaged.
- Local hardware could also be permanently damaged if its cooling depends on a remote sensor instead of on its own sensor.

## What it does

- Reads the sensors of the active profile of the remote FanControl once per second and publishes each of them as a FanControl sensor named `REMOTE <name>`.
- Authenticates with the DataServer password using a signed challenge, so the password itself is never sent over the network.
- If the remote data stops updating for more than 5 seconds, or the remote computer cannot be reached, the sensors show no value (empty) instead of a frozen reading.
- Only sensors are imported. Remote controls are not imported.
- Read-only: it never sends anything to the remote computer except read requests.

## Requirements

- FanControl installed (the .NET Framework 4.8 version) on this computer.
- On the remote computer: FanControl with the [FanControl.DataServer](https://github.com/JesusRomero77/FanControl.DataServer) plugin **version 2.0 or later**, its FanControl interface open and its port reachable (8765 by default; see that project for network permissions and firewall).
- The port and the password configured here must be the same as the ones configured in DataServer.

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

The first time the plugin loads it creates `RemoteSensorsConfig.xml` in `%LocalAppData%\FanControl`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<ClientSettings>
  <!-- Name of the REMOTE PC: type 'hostname' in a console on the remote PC. -->
  <PcName></PcName>
  <!-- IP of the REMOTE PC: type 'ipconfig' in a console on the remote PC. -->
  <Ip></Ip>
  <!-- Port DataServer listens on at the REMOTE PC. Leave empty to use 8765. -->
  <Port></Port>
  <!-- Password set in DataServer on the REMOTE PC. It must be the same. -->
  <Password>Password1234</Password>
</ClientSettings>
```

Fill in the computer name and/or the IP address of the remote computer (one of the two is enough) and restart the FanControl interface.

- **PcName / Ip:** if both are filled in, the IP address is used.
- **Port:** if it is empty or not a valid number, the plugin uses 8765 (an invalid value also writes a warning to FanControl's error log).
- **Password:** it must be the same as the one set in DataServer. If it is empty, the plugin uses `Password1234`, which is also DataServer's default. **That default password is public, so change it in both places.**

You can edit the file by hand or with the RemoteSensors tab of [WatchTower](https://github.com/JesusRomero77/FanControl.WatchTower). Restart the FanControl interface after changing it, because the plugin only reads the file when it starts.

Until it is configured, the plugin only adds a dummy sensor called "Remote Sensors" (FanControl requires every plugin to add at least one sensor), which shows no value and can be ignored.

## Operation

- The FanControl interface must be open on **both** computers: the plugin only works while the interface is open here, and DataServer only serves up-to-date data while the interface is open on the remote computer.
- The list of remote sensors is created when the FanControl interface starts. The plugin remembers the last list it received (in `RemoteSensorsProfileCache.json`, in the same folder), so the sensors exist even if the remote computer is switched off at that moment. They show no value until it answers.
- If the remote profile changes and has different sensors, the plugin writes a warning in FanControl's error log. Restart the FanControl interface to refresh the list of `REMOTE` sensors.
- If DataServer rejects the password, the sensors show no value and the plugin writes a warning in FanControl's error log. Check that the password here is the same as in DataServer.
- DataServer limits how many computers can use it at the same time (see its README). If the limit is reached, the sensors show no value until a place is free.

## Upgrading from 1.0

Version 2.0 only works with FanControl.DataServer 2.0 or later, because DataServer now requires a password. Version 1.0 of this plugin cannot connect to DataServer 2.0, and this version cannot connect to DataServer 1.0.

Your existing `RemoteSensorsConfig.xml` keeps working: it does not need to be changed if DataServer still uses the default password and port. If you changed the password or the port in DataServer, add the `Port` and `Password` lines shown above (or use WatchTower).

## Known limitations

- Only the sensors that DataServer serves (the custom sensors of the remote profile) are available.
- The list of sensors cannot change while the interface is open; a restart is needed.
- The traffic is plain HTTP, not encrypted: anyone who can capture it can read the sensor values. Use a long, unique password, use this only on networks you trust, and never expose the DataServer port to the internet.

## Building from source

Open `Source/FanControl.RemoteSensors.csproj` in Visual Studio with the .NET desktop development workload and the .NET Framework 4.8 targeting pack, with FanControl installed (the project looks for its DLLs in `C:\Program Files (x86)\FanControl\`; if it is installed elsewhere, change the path of each FanControl reference in the `.csproj`). Build in Release for x64.

## Related projects

- [FanControl.DataServer](https://github.com/JesusRomero77/FanControl.DataServer): serves the sensors and controls of a FanControl profile over HTTP. Required on the remote computer.
- [FanControl.CurveReader](https://github.com/JesusRomero77/FanControl.CurveReader): logs temperature and PWM changes to a file.
- [FanControl.WatchTower](https://github.com/JesusRomero77/FanControl.WatchTower): a program to configure these plugins and to record the data of a remote DataServer into a log.

## Disclaimer

This project is not affiliated with FanControl or its author.

## License

This project is licensed under the [MIT License](LICENSE).

FanControl and the libraries it provides to plugins (such as Newtonsoft.Json) are not part of this repository and remain under their own licenses.

## Version

2.0
