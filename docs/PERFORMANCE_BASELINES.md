# Local performance baselines

`scripts/measure-startup.ps1` measures time from process start to the first
visible tray window. It creates a fresh temporary data directory, suppresses
update checks, reports JSON, then terminates only the process it launched.

Build the identity before measuring:

```powershell
.\build.ps1 -Project WinUI -XiaozaBuild -NoTrustRepository
.\scripts\measure-startup.ps1 -Xiaoza
```

The output includes `timeToFirstWindowMs`, `workingSetMiB`, and process CPU
time. Take several measurements on the same machine and compare medians. This
is a local regression signal, not a cross-machine benchmark or a release SLO.

The product should track these scenarios over time:

- first window with fresh local data;
- first window with an existing gateway registry;
- ready-to-chat after reconnect;
- 24-hour idle memory and handle growth;
- high-frequency activity updates and large session lists.

Do not record gateway addresses, tokens, prompts, chat content, screenshots, or
other user data in performance artifacts.
