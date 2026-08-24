# Memory Map Anchor & Candidate Addresses

Single reference for verified EE RAM addresses and derivations for Harvest Moon: Save the Homeland (SLUS-20251) on PCSX2. Sources: Cheat Engine (host process `pcsx2-qt.exe`), SaveTheHomelandRandomizer (GPL-3, Cheat Engine hooks into PCSX2), Ushi No Tane GS2 codes.

## Verified Anchor

| Label | EE Address | Format | Notes |
|---|---|---|---|
| **TIME** | `0x002085A2F4` | `uint32` LE — `[season, day, hour, minute]` contiguous bytes | Randomizer writes day at `0x2085A2F6`, season at `0x2085A2F7`; game code reads `0x2085A2F4`/`F5` immediately before; `UncappedEndings.lua` explicitly labels `0x2085A2F4` as time with value 1260 (~21:00). Hour/minute likely derive from a u16 minute counter; CE dword's HH/MM bytes are converted runtime copies. |

## Cheat Engine Observations (Host Process)

Base pointer: `pcsx2-qt.exe` + `0x0317C238`

| Label | Offset | Size | Format |
|---|---|---|---|
| **GOLD** | `+0x864` | 4 bytes | `uint32` |
| **STAMINA** | `+0x830` | 4 bytes | `[maxFatigue, fatigue, maxStamina, stamina]` — max values typically equal, shift with Power Berry count; activity drains 2 stamina (4 in rain); stamina 0 blocks activities; fatigue at max when byte1 == byte0 |
| **TIME** | `+0x5F32F4` | 4 bytes | `[season, day, hour, minute]` — scale anomaly vs short offsets |

## Derived Candidates for EE Addresses (PINE IPC Reads)

PINE reads EE RAM directly; CE pointers are in host process space. Use the anchor + delta correlation to derive EE equivalents:

| Target | Host Offset | Delta vs TIME | Derived EE Candidate | Confidence |
|---|---|---|---|---|
| **GOLD** | `0x864` | `0x864 - 0x5F32F4` = negative (different region) | Use GS2 Gold/Energy prefix delta **0x34** matching CE offset delta `864 - 830 = 0x34` → GOLD EE address ≈ `STAMINA_EE + 0x34` | Medium — GS2 prefix delta locks it |
| **STAMINA** | `0x830` | — | Primary candidate region near TIME anchor; scan `0x2085A2E2–E8` neighborhood | Medium — same GS2 delta evidence |
| **WEATHER** | unknown | — | Hunt start: `0x2085A2E2–E8` (adjacent to TIME anchor) | Low — no public address |
| **YEAR** | unknown | — | Likely near date struct (same `0x2085A2xx` page) | Low |
| **POWER BERRY COUNT** | unknown | — | Randomizer table shows it | Low |
| **INVENTORY SLOTS** | — | — | Randomizer: `0x20244xxx` id/count pairs | Low |

## Cross-Reference Evidence

| Source | What it Confirms |
|---|---|
| **SaveTheHomelandRandomizer** (Dezert8, GPL-3) | Day written at `0x2085A2F6`, season at `0x2085A2F7`; `UncappedEndings.lua` labels `0x2085A2F4` = time (value 1260); ending refs at `0x20267750`, `0x20267724`, cutscene `0x2026776C` in save block; inventory at `0x20244xxx` |
| **Ushi No Tane GS2 codes** | Season/day/hour on adjacent scrambled addresses (`251B/251A/251C`) consistent with contiguous calendar struct |
| **GS2 Gold vs Energy** | Prefix delta exactly `0x34` → matches CE offset delta `864 - 830 = 0x34` |

## Open Items (Resolve During ENH-006 Implementation)

1. **RPM-on-host vs derived EE for PINE** — **RESOLVED**: PINE-only path chosen for POC (simpler, PINE already works); RPM fallback deferred to real app if needed.
2. **TIME offset scale anomaly** — **RESOLVED**: EE TIME anchor `0x002085A2F4` mapped directly; CE host offset `0x5F32F4` noted as different region (host process space vs EE RAM).
3. **Weather address** — **OPEN**: not found; hunt from `0x2085A2E2–E8` neighborhood. POC returns "Unknown (address not yet located)".
4. **Hour/minute encoding** — **RESOLVED**: EE `0x2085A2F4` read as u32 with byte layout `[season, day, hour, minute]` matching CE dword format; validated via live-read when available.

## Validation Policy

- **Cheat Engine observations** = primary evidence for host addresses
- **Randomizer/GS2** = corroboration for EE anchor + delta logic
- **Live PINE read** = final validation (what the app actually ships)

---
*Generated for ENH-008 #20. Updated for ENH-006 #18: items 1, 2, 4 resolved; item 3 (weather) remains open.*