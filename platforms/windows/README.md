# Tor-proxy — Windows

The Windows client: a WPF / .NET 8 app that ships as a **single self-contained
`.exe`**. It bundles `tor.exe` + `lyrebird` inside the executable and unpacks
them on first run, so there is nothing to install and no admin rights are needed.

Download the ready-made exe from the repository
[Releases](../../../../releases/latest). To build it yourself:

## Requirements

- Windows 10 / 11 (x64)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)

## Build

Run from the repository root:

```powershell
# 1. Fetch the official Tor binaries (URL + SHA-256 from torproject.org)
powershell -ExecutionPolicy Bypass -File platforms\windows\scripts\fetch-tor.ps1 `
    -Url "<official Tor Expert Bundle .tar.gz>" -Sha256 "<sha256>"

# 2. Build + sign + install one self-contained exe (+ Desktop shortcut)
powershell -ExecutionPolicy Bypass -File platforms\windows\scripts\build-and-install.ps1
```

The distributable is written to `platforms\windows\publish-torapp\Tor-proxy.exe`
(plus `README.txt`). Signing uses a self-signed `CN=bylka` certificate created
once in your user store — on other machines SmartScreen will still say "unknown
publisher".

Build / test only:

```powershell
dotnet build platforms\windows\Tor-proxy.sln -c Release
dotnet test  platforms\windows\tests\Tor.Tests\DpiBypass.Tor.Tests.csproj -c Release
```

## Layout

```
platforms/windows/
  Tor-proxy.sln
  src/
    Tor/        engine-independent Tor core (process, torrc, bridges) — unit-tested
    TorApp/     WPF app (MVVM, tray, autostart, bridge editor, self-test)
  tests/Tor.Tests/
  scripts/      fetch-tor.ps1, build-and-install.ps1
  third_party/tor/   fetched binaries (git-ignored) + committed hashes.txt
```

The end-user instruction sheet shipped next to the exe is
[`src/TorApp/README.md`](src/TorApp/README.md).

> Note: the C# `src/Tor` core is a candidate for sharing with future **.NET**
> clients (e.g. a .NET MAUI macOS/Android app). If that path is taken it can be
> promoted to a top-level `shared/` project. Non-.NET clients (native Swift /
> Kotlin) reimplement the same small logic against their own Tor integration.
