# xiaozaclaw product baseline

xiaozaclaw is an independently branded Windows desktop product built on the
official OpenClaw Windows Hub. The upstream protocol, gateway client, security
checks, and test suites stay intact. Product-specific behavior is added through
an explicit build identity and focused extension layers.

## Build identity

Use these commands from the repository root:

```powershell
.\build.ps1 -Project WinUI -Configuration Debug -XiaozaBuild
.\run-app-local.ps1 -Xiaoza -Isolated -AllowNonMain
.\scripts\build-inno-local.ps1 -Arch x64 -Xiaoza -Fast
```

The xiaozaclaw build has its own:

- display name and Windows AppUserModelID
- local and roaming data directory
- startup registration and single-instance mutex
- deep-link protocol (`xiaozaclaw://`)
- app-owned WSL gateway distribution
- default embedded gateway port (`18889`)
- installer identity and filename

This isolation prevents an installed upstream OpenClaw Companion or an older
tutuclaw build from sharing credentials, settings, ports, or startup entries
with xiaozaclaw.

## Architecture boundaries

- OpenClaw Gateway remains the source of truth for sessions, agents, channels,
  skills, tools, and provider-backed chat models.
- The Windows Hub connection manager remains the only owner of operator and
  node connection state.
- Provider-specific image, video, speech, embedding, and reranking features
  belong in capability-specific adapters. They must not be inserted into the
  chat model picker unless the provider exposes a compatible chat endpoint.
- Every configured model records its provider, remote model or endpoint ID,
  capability type, verification status, and last verified time.
- A model is shown as available only after a real provider request succeeds.
- Product extensions should use plugins, MCP, or narrow services instead of
  growing the application composition root.

## Delivery order

1. Reproducible x64 build and installer.
2. Chinese product shell, icon, first-run setup, and migration safeguards.
3. Reliable chat and session management through the official gateway.
4. Capability-aware model catalog and provider verification.
5. Image generation with preview, copy, save, and provider-specific sizing.
6. Skills, scheduled tasks, diagnostics, backup, and update delivery.
7. Real API integration tests and Windows UI regression tests.

## Attribution and updates

The upstream MIT license and attribution remain in the repository. Until a
dedicated xiaozaclaw release repository and signing identity exist, automatic
updates must not replace a xiaozaclaw binary with an upstream OpenClaw build.
