# third_party/tor

The Tor binaries are **not** committed to this repository (see `.gitignore`).
They are downloaded from an **official** Tor Expert Bundle and verified by
SHA-256, then embedded into the built `Tor-proxy.exe`.

## What lives here

- `hashes.txt` — committed. Pins the SHA-256 of `tor.exe` that the app verifies
  at runtime.
- `tor.exe`, `pluggable_transports/lyrebird.exe` — **fetched, not committed.**

## How to fetch (before building)

1. Go to <https://www.torproject.org/download/tor/> → **Windows Expert Bundle**
   (x86_64). Copy the download URL and its published SHA-256.
2. Run:

   ```powershell
   powershell -ExecutionPolicy Bypass -File scripts\fetch-tor.ps1 `
       -Url "<official .tar.gz url>" -Sha256 "<official sha256>"
   ```

This stages `tor.exe` + `lyrebird.exe` here and (re)writes `hashes.txt`.

See `../../THIRD-PARTY-NOTICES.md` for the licenses of these binaries.
