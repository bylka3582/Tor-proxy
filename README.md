# Tor-proxy

> Открывайте заблокированные сайты через сеть **Tor** с мостом **Snowflake**.
> Без аккаунтов и настроек: скачал, включил, работает. **by bylka**

![license](https://img.shields.io/badge/license-MIT-green)
![Windows](https://img.shields.io/badge/Windows-доступно-2FA84F)
![Android](https://img.shields.io/badge/Android-доступно-2FA84F)
![macOS](https://img.shields.io/badge/macOS-в_планах-9A9AA2)
![iOS](https://img.shields.io/badge/iOS-в_планах-9A9AA2)

Tor-proxy запускает Tor и Snowflake локально, у вас на устройстве, и даёт
браузеру и приложениям доступ к заблокированным сайтам. Работает даже там, где
обычные мосты заблокированы по IP: рабочий способ подключения подбирается
автоматически. Скорость ~1–5 Мбит/с — отлично для сайтов, форумов, мессенджеров
и приложений, но не для видео.

> Это средство обхода цензуры, а не гарантия анонимности.

## Платформы

| Платформа | Статус | Исходники |
|-----------|--------|-----------|
| Windows   | ✅ Доступна | [`platforms/windows`](platforms/windows) |
| Android   | ✅ Доступна | [отдельный репозиторий](https://github.com/bylka3582/Tor-proxy-android) — форк [Orbot](https://github.com/guardianproject/orbot-android) (см. ниже) |
| macOS     | 🚧 В планах | `platforms/macos` |
| iOS       | 🚧 В планах | `platforms/ios` |

У обеих версий одна цель — открыть заблокированное через Tor без настроек, — но
устроены они по-разному, см. раздел «Чем версии различаются».

## Скачать

Обе версии — в [последнем релизе](../../releases/latest).

**Windows** — [`Tor-proxy.exe`](../../releases/latest/download/Tor-proxy.exe)
(78 МБ), один файл. Ни установки, ни прав администратора, ни .NET. Запустите,
нажмите «Запустить», дождитесь «Готово» и укажите в браузере SOCKS5
`127.0.0.1:9150`. Подробная инструкция — в
[README для Windows](platforms/windows/README.md).

**Android** — [`Tor-proxy-by-bylka-arm64.apk`](../../releases/latest/download/Tor-proxy-by-bylka-arm64.apk)
(41 МБ) для современных телефонов или
[`Tor-proxy-by-bylka-universal.apk`](../../releases/latest/download/Tor-proxy-by-bylka-universal.apk)
(123 МБ), если первый не ставится. Android 7.0+. Установите, нажмите
«Подключить», дождитесь «Подключено» и отметьте приложения, которым нужен Tor.
Пошаговая инструкция — во вкладке «Инструкция» внутри приложения.

## Чем версии различаются

| | Windows | Android |
|---|---|---|
| Механизм | локальный SOCKS5-прокси на `127.0.0.1:9150` | VPN-служба + маршрутизация по приложениям |
| Выбор приложений | прописать прокси в каждом приложении | отметить галочками в списке |
| Собрана из | этого репозитория | [форка Orbot](https://github.com/bylka3582/Tor-proxy-android) |

На Windows приложения сами «приходят» к прокси — вы указываете им адрес. На
Android через Tor идут **только отмеченные** приложения, остальные продолжают
работать через обычное соединение: весь телефон в Tor не заворачивается никогда.

## Android и Orbot

Android-версия — **изменённая сборка [Orbot](https://github.com/guardianproject/orbot-android)**
(© 2009–2026 Nathan Freitas, The Guardian Project, лицензия 3-clause BSD):
ребренд в Tor-proxy, маршрутизация только выбранных приложений, подключение в
режиме Smart из коробки и пошаговая инструкция на русском. Исходники и полный
список изменений — в [Tor-proxy-android](https://github.com/bylka3582/Tor-proxy-android).

Это **не** официальное приложение Orbot; The Guardian Project и Tor Project к
нему отношения не имеют и не поддерживают его. Официальный Orbot — на
[orbot.app](https://orbot.app/). Копирайт и лицензия Orbot воспроизведены внутри
приложения («Ещё» → «О приложении») и в
[`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md).

## Структура репозитория

```
tor-proxy/
├─ platforms/            по папке на каждую ОС (каждая самодостаточна)
│  └─ windows/           WPF / .NET 8, сборка в один файл
│     ├─ src/ tests/ scripts/ third_party/
│     └─ README.md       сборка и заметки для разработки
├─ shared/               общее для всех платформ
│  ├─ bridges/           канонический список мостов Snowflake
│  └─ branding/          иконка и бренд-ассеты
├─ docs/                 скриншоты и кроссплатформенные заметки
├─ LICENSE               MIT
└─ THIRD-PARTY-NOTICES.md  Tor, lyrebird и Orbot (BSD) — распространяемые бинарники
```

Android-версии в этом дереве нет: это форк целого существующего приложения,
поэтому он живёт в своём репозитории и сохраняет историю Orbot. Новые платформы,
написанные с нуля, добавляются как `platforms/<os>/` и берут мосты и бренд
из `shared/`.

## Лицензия и сторонние компоненты

Copyright (c) 2026 bylka. Лицензия **MIT** — см. [`LICENSE`](LICENSE).

MIT покрывает код этого репозитория, изменения Tor-proxy, интерфейс и оформление.
Распространяемые сторонние компоненты сохраняют свои лицензии: `tor.exe`
(BSD-3-Clause), `lyrebird` (BSD-2-Clause) и, для Android-сборки, Orbot
(BSD-3-Clause). Их уведомления — в
[`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md). Бинарники для Windows
скачиваются из официальных релизов и проверяются по хешу, в репозиторий они
не коммитятся.

Не аффилировано с The Tor Project, Inc. и The Guardian Project.

## Оговорка

Предоставляется как есть, для законного обхода цензуры. Трафик идёт через
публичную сеть Tor: это скрывает от провайдера, куда вы заходите, а от сайта —
ваш IP, но не является гарантией анонимности.

---

## English

**Tor-proxy** opens blocked sites through the **Tor** network with a **Snowflake**
bridge. No accounts, no setup. Speed is ~1–5 Mbit/s: pages, forums, messengers —
not video. A censorship-circumvention tool, not a full anonymity solution.
The interface and documentation are in Russian.

**Windows** — grab [`Tor-proxy.exe`](../../releases/latest/download/Tor-proxy.exe)
from the [latest release](../../releases/latest): a single file, no install and no
admin rights. Run it, click «Запустить», wait for «Готово», then point your
browser at SOCKS5 `127.0.0.1:9150`. Details: [`platforms/windows`](platforms/windows).

**Android** — grab [`Tor-proxy-by-bylka-arm64.apk`](../../releases/latest/download/Tor-proxy-by-bylka-arm64.apk)
(or the `...-universal.apk` if that one refuses to install). Sideload it, tap
Connect, then tick the apps that need Tor. **Only ticked apps** are routed; the
rest of the phone keeps using the normal connection, and the whole device is
never routed.

The Android client is a modified build of
[Orbot](https://github.com/guardianproject/orbot-android) (© 2009–2026 Nathan
Freitas, The Guardian Project, 3-clause BSD) — source in
[Tor-proxy-android](https://github.com/bylka3582/Tor-proxy-android). It is **not**
the official Orbot app and is not endorsed by or affiliated with The Guardian
Project or the Tor Project.

Licensed under MIT, © 2026 bylka — see [`LICENSE`](LICENSE) and
[`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md). macOS and iOS are planned.

---

Tor-proxy · **by bylka** · based on Tor, Snowflake and Orbot.
