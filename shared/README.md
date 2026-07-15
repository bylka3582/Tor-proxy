# shared/

Platform-agnostic assets reused by **every** client (Windows, macOS, Android,
iOS). Keep the single source of truth here so all platforms behave and look the
same; each client copies/embeds from these during its build.

- `bridges/snowflake-bridges.txt` — the canonical Snowflake bridge list every
  client seeds from. Update here first, then re-sync each platform's seed.
- `branding/app.ico` — the app icon / brand mark. Add other formats
  (`app.png`, `app.svg`) here as new platforms need them.

> Current status: the Windows client keeps its own working copies for build
> convenience. As more platforms are added, wire each build to read from here so
> nothing drifts.
