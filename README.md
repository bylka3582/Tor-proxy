# Tor-proxy

> Open censored sites through the **Tor** network using a **Snowflake** bridge —
> a simple local proxy at `127.0.0.1:9150`. No VPN, no admin rights. **by bylka**

![license](https://img.shields.io/badge/license-MIT-green)
![Windows](https://img.shields.io/badge/Windows-available-2FA84F)
![Android](https://img.shields.io/badge/Android-available-2FA84F)
![macOS](https://img.shields.io/badge/macOS-planned-9A9AA2)
![iOS](https://img.shields.io/badge/iOS-planned-9A9AA2)

Tor-proxy runs Tor + Snowflake locally and exposes a SOCKS5 endpoint your
browser and apps can use to reach blocked sites. It works even where plain
bridges are IP-blocked, needs no account, and is meant to be dead simple.
Speed is ~1–5 Mbit/s: great for pages, forums, messengers and APIs — not video.

> A censorship-circumvention tool, not a full anonymity solution.

## Platforms

| Platform | Status | Source |
|----------|--------|--------|
| Windows  | ✅ Available | [`platforms/windows`](platforms/windows) |
| Android  | ✅ Available | separate repo — a fork of [Orbot](https://github.com/guardianproject/orbot) (see below) |
| macOS    | 🚧 Planned | `platforms/macos` |
| iOS      | 🚧 Planned | `platforms/ios` |

The two clients share the goal (reach blocked sites over Tor with a Snowflake
bridge, no account, no setup) but not the mechanism — see below.

## Download

Both clients ship from the [latest release](../../releases/latest).

**Windows** — `Tor-proxy.exe`, a single 78 MB file. No install, no admin rights,
no .NET needed. Run it, wait for "Готово", point your browser at SOCKS5
`127.0.0.1:9150`. Full guide: [Windows README](platforms/windows/README.md).

**Android** — `Tor-proxy-by-bylka-arm64.apk` for modern phones, or
`Tor-proxy-by-bylka-universal.apk` if the first one refuses to install. Sideload
it, tap Connect, then pick which apps go through Tor. The in-app Guide tab walks
through it.

## How the two differ

| | Windows | Android |
|---|---|---|
| Mechanism | local SOCKS5 proxy on `127.0.0.1:9150` | VPN service + per-app routing |
| Per-app control | configure the proxy in each app | tick apps in a list |
| Built from | this repository | a fork of Orbot (see below) |

On Windows, apps opt in by pointing at the proxy. On Android, only the apps you
tick are routed into Tor; everything else keeps using the normal connection —
the whole device is never routed.

## Android and Orbot

The Android client is **a modified build of [Orbot](https://github.com/guardianproject/orbot)**
(© 2009–2026 Nathan Freitas, The Guardian Project — 3-clause BSD), rebranded as
Tor-proxy and changed to route only explicitly selected apps, connect with Smart
mode out of the box, and carry a Russian step-by-step guide.

It is **not** the official Orbot app, and it is not endorsed by or affiliated
with The Guardian Project or the Tor Project. If you want upstream Orbot, get it
from [orbot.app](https://orbot.app/). Orbot's copyright notice and license are
reproduced in the app (More → About) and in
[`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md).

## Repository layout

```
tor-proxy/
├─ platforms/            one folder per OS client (self-contained)
│  └─ windows/           WPF / .NET 8 single-file app
│     ├─ src/ tests/ scripts/ third_party/
│     └─ README.md       build & dev notes
├─ shared/               platform-agnostic, reused by every client
│  ├─ bridges/           canonical Snowflake bridge list
│  └─ branding/          app icon / brand assets
├─ docs/                 screenshots & cross-platform notes
├─ LICENSE               MIT
└─ THIRD-PARTY-NOTICES.md  Tor, lyrebird & Orbot (BSD) — redistributed binaries
```

The Android client is not in this tree: it is a fork of a whole existing app, so
it lives in its own repository and keeps Orbot's history intact. New from-scratch
platforms slot in as `platforms/<os>/` and pull bridges/branding from `shared/`.

## License & third-party

Copyright (c) 2026 bylka. Licensed under the **MIT License** — see
[`LICENSE`](LICENSE).

The MIT license covers the code in this repository and the Tor-proxy changes,
interface and branding. Redistributed third-party components keep their own
licenses: `tor.exe` (BSD-3-Clause), `lyrebird` (BSD-2-Clause) and, for the
Android build, Orbot (BSD-3-Clause). Their notices are in
[`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md). The Windows binaries are
fetched from official releases and verified by hash, never committed.

Not affiliated with or endorsed by The Tor Project, Inc. or The Guardian Project.

## Disclaimer

Provided as-is, for lawful circumvention of censorship. Traffic goes over the
public Tor network: this hides the destination from your ISP and your IP from the
site, but is not a guarantee of anonymity.

---

## Русский

**Tor-proxy** — простой способ открыть заблокированные сайты через сеть **Tor**
(мост **Snowflake**). Без аккаунтов и настроек. Скорость ~1–5 Мбит/с: сайты,
форумы, мессенджеры; не для видео.

**Windows** — скачайте `Tor-proxy.exe` из [релизов](../../releases/latest): один
файл, без установки и прав администратора. Выключите VPN, нажмите «Запустить»,
дождитесь «Готово» и укажите в браузере SOCKS5 `127.0.0.1:9150`. Подробности —
[`platforms/windows`](platforms/windows).

**Android** — скачайте `Tor-proxy-by-bylka-arm64.apk` (или `...-universal.apk`,
если первый не ставится) из [релизов](../../releases/latest). Установите, нажмите
«Подключить», дождитесь «Подключено» и отметьте приложения, которым нужен Tor.
Через Tor пойдут **только отмеченные** приложения, остальной трафик телефона не
затрагивается. Пошаговая инструкция — во вкладке «Инструкция» внутри приложения.

Android-версия — изменённая сборка на основе
[Orbot](https://github.com/guardianproject/orbot) (© 2009–2026 Nathan Freitas,
The Guardian Project, лицензия 3-clause BSD). Это **не** официальный Orbot;
The Guardian Project и Tor Project к ней отношения не имеют.

Версии для macOS и iOS — в планах.

---

Tor-proxy · **by bylka** · based on Tor, Snowflake and Orbot.
