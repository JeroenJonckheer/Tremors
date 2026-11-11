# Changelog

All notable changes to Tremors are listed here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the
project uses [semantic versioning](https://semver.org/).

## [1.0.0] - 2026-11-11

First release.

### Added

- Tremor filter for the mouse cursor: two-stage low-pass that adapts to sustained
  movement, so trembling and sudden jerks are absorbed and deliberate moves stay direct.
- Leash: the cursor stays still while holding still on a target.
- Click lock: the cursor stops gliding when a button is pressed.
- Scroll wheel filter that ignores trembling flicks back and forth.
- Tray icon with menu, settings window with a single strength slider.
- Start with Windows, without administrator rights.
- Interface in English, Dutch, French and German, following the Windows display language.
- Single-file `Tremors.exe`, no installation or .NET runtime needed.

[1.0.0]: https://github.com/JeroenJonckheer/Tremors/releases/tag/v1.0.0
