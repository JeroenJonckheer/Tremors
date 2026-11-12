# Changelog

All notable changes to Tremors are listed here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the
project uses [semantic versioning](https://semver.org/).

## [Unreleased]

### Changed

- Starting Tremors by hand opens the settings window, so it is clear that it runs. When
  Windows starts it at sign-in, it stays quietly in the notification area as before.
- The settings window explains that Tremors keeps working after the window is closed.

## [1.2.0] - 2026-11-12

### Added

- "About Tremors" link in the settings window: version, build month, maker, and a link to
  the latest version.

### Changed

- Clearer wording in the README and project description ("a calm mouse for shaky hands").

## [1.1.1] - 2026-11-11

### Changed

- New icon: a calm mouse pointer with the absorbed tremor as two mint echoes behind it,
  clearly visible at the small size of the notification area.

## [1.1.0] - 2026-11-11

### Added

- Hold to click: ignores accidental clicks. A click only counts when the button is held
  for a set time (0-500 ms, off by default). Dragging and double-clicking keep working.
- Language flags in the settings window; the choice is remembered. Switching happens in
  place, without reopening the window.
- Starting `Tremors.exe` while it is already running opens its settings window.

### Changed

- Settings window in two columns, so it fits on small laptop screens, and sized correctly
  at every display scaling.

## [1.0.0] - 2026-11-11

First release.

### Added

- Tremor filter for the mouse cursor: two-stage low-pass that adapts to sustained
  movement, so trembling and sudden jerks are absorbed and deliberate moves stay direct.
- Leash: the cursor stays still while holding still on a target.
- Click lock: the cursor stops gliding when a button is pressed.
- Scroll wheel filter that ignores trembling flicks back and forth.
- Pointer speed and still zone settings for fine-tuning.
- Windows mouse acceleration is switched off while filtering, so shakes are not magnified.
- Tray icon with menu and a settings window.
- Start with Windows, without administrator rights.
- Interface in English, Dutch, French and German, following the Windows display language.
- Single-file `Tremors.exe`, no installation or .NET runtime needed.

[1.2.0]: https://github.com/JeroenJonckheer/Tremors/releases/tag/v1.2.0
[1.1.1]: https://github.com/JeroenJonckheer/Tremors/releases/tag/v1.1.1
[1.1.0]: https://github.com/JeroenJonckheer/Tremors/releases/tag/v1.1.0
[1.0.0]: https://github.com/JeroenJonckheer/Tremors/releases/tag/v1.0.0
