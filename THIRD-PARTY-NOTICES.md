# Third-party notices

Tor-proxy redistributes third-party software in binary form, so the copyright
notices and licenses of those components are reproduced below.

**Windows** (`Tor-proxy.exe`): the bundled binaries are **not** stored in this
repository — they are downloaded from official releases and verified by SHA-256
(`scripts/fetch-tor.ps1`), then embedded into the built executable.

**Android** (`Tor-proxy-by-bylka-*.apk`): the app is a modified build of Orbot
and ships Orbot's code together with Tor and the pluggable transports. See the
Orbot section below.

---

## Tor

- Project: The Tor Project — <https://www.torproject.org/>
- Component: `tor.exe` (from the Windows Expert Bundle)
- License: BSD 3-Clause

```
Copyright (c) 2001-2004, Roger Dingledine
Copyright (c) 2004-2006, Roger Dingledine, Nick Mathewson
Copyright (c) 2007-2019, The Tor Project, Inc.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

  * Redistributions of source code must retain the above copyright notice,
    this list of conditions and the following disclaimer.

  * Redistributions in binary form must reproduce the above copyright notice,
    this list of conditions and the following disclaimer in the documentation
    and/or other materials provided with the distribution.

  * Neither the names of the copyright owners nor the names of its
    contributors may be used to endorse or promote products derived from this
    software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

---

## lyrebird (obfs4proxy / Snowflake pluggable transport)

- Project: The Tor Project / obfs4 — <https://gitlab.torproject.org/tpo/anti-censorship/pluggable-transports/lyrebird>
- Component: `lyrebird.exe` (provides the `snowflake` and `obfs4` transports)
- License: BSD 2-Clause

```
Copyright (c) 2014, Yawning Angel <yawning at torproject dot org>
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

  1. Redistributions of source code must retain the above copyright notice,
     this list of conditions and the following disclaimer.

  2. Redistributions in binary form must reproduce the above copyright notice,
     this list of conditions and the following disclaimer in the documentation
     and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES ARE DISCLAIMED. IN NO EVENT SHALL THE
COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DAMAGES ARISING IN ANY WAY
OUT OF THE USE OF THIS SOFTWARE.
```

---

## Orbot (Android client only)

- Project: The Guardian Project — <https://orbot.app/> ·
  <https://github.com/guardianproject/orbot>
- Component: the Android app itself. `Tor-proxy-by-bylka-*.apk` is a **modified
  build of Orbot**, rebranded as Tor-proxy and changed to route only explicitly
  selected apps, to connect with Smart mode by default, and to carry a Russian
  step-by-step guide.
- License: BSD 3-Clause

```
Copyright (c) 2009-2026, Nathan Freitas, The Guardian Project

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

  * Redistributions of source code must retain the above copyright notice,
    this list of conditions and the following disclaimer.

  * Redistributions in binary form must reproduce the above copyright notice,
    this list of conditions and the following disclaimer in the documentation
    and/or other materials provided with the distribution.

  * Neither the names of the copyright owners nor the names of its
    contributors may be used to endorse or promote products derived from this
    software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

Tor-proxy for Android is **not** the official Orbot app and is not endorsed by
or affiliated with The Guardian Project. Upstream Orbot is at
<https://orbot.app/>. The APK also bundles Tor and the pluggable transports
(IPtProxy / lyrebird / Snowflake) plus hev-socks5-tunnel; their licenses are
reproduced inside the app under More → About.

---

The Snowflake bridges shipped as defaults are the public, built-in Snowflake
bridges distributed by the Tor Project. This application is not affiliated with
or endorsed by The Tor Project, Inc. or The Guardian Project. "Tor" is a
registered trademark of The Tor Project, Inc.
