<div align="center">

<img src="docs/icon.png" width="96" alt="">

# Tremors

**A steady mouse for shaky hands.**
Free, open source and private. One small program for Windows.

[![CI](https://github.com/JeroenJonckheer/Tremors/actions/workflows/ci.yml/badge.svg)](https://github.com/JeroenJonckheer/Tremors/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/JeroenJonckheer/Tremors)](https://github.com/JeroenJonckheer/Tremors/releases/latest)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

[**Download Tremors.exe**](https://github.com/JeroenJonckheer/Tremors/releases/latest) · [Nederlands](README.nl.md) · [How it works](docs/how-it-works.md)

</div>

---

As we age, our fine motor skills may decline, while our reliance on computers remains
as strong as ever. A trembling hand makes the cursor shake, clicks land next to the
button and the page scrolls by accident.

Tremors sits quietly in the corner of the screen and filters the trembling out of
every mouse movement before Windows sees it. Small, shaky movements are absorbed.
Deliberate movements still go where you want them to go.

Tremors was built for Nico, so he could keep using his computer. It is free for
everyone who needs the same.

## What it does

- **Steadies the cursor.** Trembling is smoothed away; holding still on a button
  keeps the cursor still.
- **Keeps clicks on target.** At the moment you click, the cursor stops gliding.
- **Steadies the scroll wheel.** A trembling flick back and forth is ignored, so a
  page does not jump up and down.
- **One slider.** Choose how strong the filter is, from 0 (off) to 100.
- **Nothing else.** No account, no internet, no ads, no data collection.

## Install

1. Download [**Tremors.exe**](https://github.com/JeroenJonckheer/Tremors/releases/latest).
2. Put it somewhere permanent, for example in `Documents` or `C:\Program Files\Tremors`.
3. Double-click it. A round icon with a green dot appears next to the clock, and the
   mouse is steadied right away.
4. Click the icon and tick **Start with Windows**, so it is always on.

No installation, no administrator rights and no .NET download are needed.

> **Windows may warn you the first time** ("Windows protected your PC"). Tremors is
> not signed with a paid certificate. Click **More info** → **Run anyway**. You can
> verify the download with the `Tremors.exe.sha256` file on the release page, and
> the [source code](src/Tremors) is all here to read.

## Use

| Where | What |
|---|---|
| **Left-click** the icon | Opens the settings window |
| **Right-click** the icon | Menu: **Active** (on/off), **Settings**, **Start with Windows**, **Quit** |
| Green dot | Tremors is steadying the mouse |
| Grey dot | Tremors is paused |

The settings window:

| Setting | Meaning |
|---|---|
| **Strength** (0-100) | How much trembling is filtered. Start at 70. Raise it if the cursor still shakes; lower it if the mouse feels slow. 0 turns the filter off. |
| **Steady scroll wheel** | Ignores a scroll in the opposite direction right after a scroll, which is usually a tremor. |
| **Start with Windows** | Starts Tremors automatically when you sign in. |

Every change takes effect immediately and is remembered.

### Tips for helpers and family

- Set it up once, tick **Start with Windows**, and the person using the computer
  never has to think about it.
- A heavy mouse or a trackball helps as well; Tremors works with both.
- Large Windows text (Settings → Accessibility → Text size) and a larger mouse pointer
  (Settings → Accessibility → Mouse pointer) make targets easier to hit.

## Privacy

Tremors reads mouse movement to filter it, and that is all. It does not read the
keyboard, it does not save or send anything, and it never connects to the internet.
The only file it writes is its settings: `%AppData%\Tremors\settings.json`.

## Limitations

- Windows 10 and 11, 64-bit.
- It cannot steady the mouse on the Windows sign-in screen or in the "Do you want to
  allow this app" (UAC) prompt; Windows does not allow any program there.
- Windows that run as administrator may get unfiltered movement unless Tremors also
  runs as administrator.
- Games that read the mouse directly (raw input) are not filtered.

## Build from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```
dotnet test                                      # build and run the tests
dotnet run --project src/Tremors                 # run it
dotnet publish src/Tremors -c Release            # dist\Tremors.exe, a single file
```

How the filter works is explained in [docs/how-it-works.md](docs/how-it-works.md).
Contributions are welcome; see [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE): free to use, share and change, also commercially.
Tremors is and will remain free.
