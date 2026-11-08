# What's New

> **WatchTower:** RemoteSensors can be configured with [WatchTower](https://github.com/JesusRomero77/FanControl.WatchTower), a program that configures the whole family of plugins. Its RemoteSensors tab edits the same `RemoteSensorsConfig.xml` that the plugin reads.

## 2.0

- Version 2.0 only works with FanControl.DataServer 2.0 or later, because DataServer now requires a password. Version 1.0 of this plugin cannot connect to DataServer 2.0, and this version cannot connect to DataServer 1.0.
- The plugin now authenticates with the DataServer password using a signed challenge (HMAC-SHA256), so the password itself is never sent over the network.
- `RemoteSensorsConfig.xml` has two new fields, `Port` and `Password`. They must match the ones configured in DataServer. If they are empty, the plugin uses the defaults of DataServer (port 8765 and password `Password1234`, which is public, so change it in both places). Restart the FanControl interface after changing the file.
- Your existing `RemoteSensorsConfig.xml` keeps working if DataServer still uses the default port and password. Otherwise, add the two new lines or use WatchTower.
- If DataServer rejects the password or its limit of clients is reached, the sensors show no value and the plugin writes a warning to FanControl's error log.

## 1.0

- Initial release.
