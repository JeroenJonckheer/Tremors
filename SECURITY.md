# Security and privacy

Tremors installs a global mouse hook, which is a sensitive capability. This is what it
does and does not do:

- It reads mouse events only. There is no keyboard hook.
- It keeps no history: each event is used to compute the next cursor position and
  then forgotten.
- It has no network code and makes no connections.
- It writes only `%AppData%\Tremors\settings.json` and, when **Start with Windows**
  is on, one value named `Tremors` under
  `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`.
- While it filters, it switches off Windows mouse acceleration ("Enhance pointer
  precision") for the current session only. This is not saved to the user profile and is
  undone when Tremors is paused or closed, and in any case at the next sign-in.
- It needs no administrator rights.

Every release is built from this repository by GitHub Actions and comes with a SHA-256
checksum.

## Reporting a vulnerability

Please do not open a public issue. Use
[GitHub private vulnerability reporting](https://github.com/JeroenJonckheer/Tremors/security/advisories/new)
instead. You will get a reply within a week.
