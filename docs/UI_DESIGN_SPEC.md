# UI Design Specification

Defines the visual language and layout of the HM STH Companion main window. The theme is identical to the repository's social preview (`docs/social-preview.html`) so the product and its public identity look the same.

## Scope

One WinForms window on .NET 8. WinForms has no built-in dark mode, so the palette below is applied manually to every control; the title bar additionally uses the DWM immersive dark mode attribute via P/Invoke (re-check current .NET APIs when implementation starts).

## Application states

| State | Trigger | Window shows |
|---|---|---|
| Disconnected | PCSX2 not reachable on 127.0.0.1:28011 | HUD and Guide panels dimmed with a hint line: enable PINE IPC in PCSX2 settings |
| Connected, wrong game | Serial differs from SLUS-20251 | Connection panel green, HUD shows the detected serial and a mismatch warning |
| Playing | Serial equals SLUS-20251 | All panels live |

## Layout

The social preview's mock window is the reference sketch: a two-column grid with the Guide panel spanning full width at the bottom.

```
+------------------------------------------------------+
| Game HUD              |  Memory Monitor              |
|                       |                              |
| Stamina  [########--] |  0x021C3A4E   87             |
| Money    [#####-----] |  0x021C3A50   420G           |
| Weather  Sunny        |  FPS          59 - 60 Hz     |
+------------------------------------------------------+
| Guide - Year 2 - Ending A                            |
| [#############--------------------------------]      |
+------------------------------------------------------+
```

- **Game HUD** (left column): stamina bar, money bar, weather text.
- **Memory Monitor** (right column): live hex address/value rows for values whose addresses are known, plus emulator FPS when available.
- **Guide** (bottom): walkthrough step list for the active year/ending with a progress bar.
- **Connection strip**: thin footer row inside the window carrying emulator version, serial badge, and connection state dot.

## Data contract

Every element maps to data proven readable by `src/HmSth.Poc` over PINE IPC:

| Element | Source | Availability |
|---|---|---|
| Emulator version, game serial/title | PINE metadata opcodes | now |
| Any live value row | aligned Read8/16/32/64 EE reads | now |
| Stamina / money / weather | gameplay addresses - located later with Cheat Engine | TBD; rows render as `--` until then |

Requests stay strictly sequential (PCSX2 drops replies beyond ~7 in flight); the UI refreshes on a timer that respects this limit.

## Theme tokens

Identical values to `docs/social-preview.html`:

| Token | Value | Used for |
|---|---|---|
| bg | `#0b0e14` | window background |
| mantle | `#12151e` | panel group backgrounds |
| surface | `#191d2b` | header/footer strips |
| surface1 | `#242a3d` | borders, bar tracks |
| text | `#e6e8ee` | primary text, bold stat labels |
| text-muted | `#9aa1b5` | secondary text, hints |
| accent | `#58a55c` | values, bars, labels, state dot |

Typography: Segoe UI for regular text, Consolas for values, addresses, and uppercase letter-spaced panel labels. Bars are 6 px rounded tracks in surface1 filled with accent. Panel titles are small-caps style mono labels in accent.

## Non-goals

No settings dialog, no theming options, no multi-save-slot views, no window chrome customization beyond the dark title bar. Revisit only when a real need appears.
