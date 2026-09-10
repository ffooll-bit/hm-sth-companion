# Memory Map — EE RAM Addresses and CE-to-EE Conversion

Single reference for verified EE RAM addresses, the Cheat Engine pointer-to-EE conversion procedure, and value formats for Harvest Moon: Save the Homeland (SLUS-20251) on PCSX2. Sources: Cheat Engine (host process `pcsx2-qt.exe`), SaveTheHomelandRandomizer (GPL-3, Cheat Engine hooks into PCSX2), Ushi No Tane GS2 codes.

## CE Pointer-to-EE Conversion Procedure

PINE IPC reads EE RAM directly. Cheat Engine pointers live in the host process space. To convert a CE offset into an EE address usable by PINE, follow these steps.

### Step 1 — Identify the CE base pointer and offset

The CE base pointer for HM:StH is `pcsx2-qt.exe + 0x0317C238`. Gameplay values live at fixed offsets under this base. Each offset is a 4-byte value at a known byte distance from the base.

### Step 2 — Derive the shared base value

The shared base value maps every CE offset to its EE address. Derive it from any already-validated CE offset and its known EE address:

```
base = EE_address - offset
```

Validated via three independent addresses:

| CE Offset | Known EE Address | base = EE - offset |
|---|---|---|
| `0x830` (Stamina) | `0x20267830` | `0x20267000` |
| `0x864` (Gold) | `0x20267864` | `0x20267000` |
| `0x5F32F4` (Time) | `0x2085A2F4` | `0x20267000` |

All three yield `0x20267000`. This value is reboot-stable (EE space does not shift with Windows ASLR).

### Step 3 — Convert any new offset to its EE address

```
EE = base + offset
```

Example: Weather offset `0x834` → `0x20267000 + 0x834 = 0x20267834`.

### Step 4 — Decode the value for PINE IPC reads

Each address carries a specific byte layout within a `uint32` little-endian read. The value format determines how to extract game state. See the [Value Format Reference](#value-format-reference) below.

### Worked Example — Stamina

1. CE base pointer: `pcsx2-qt.exe + 0x0317C238`, Stamina offset: `0x830`
2. Base value: `0x20267000` (derived from validated addresses)
3. EE address: `0x20267000 + 0x830 = 0x20267830`
4. PINE `ReadU32(0x20267830)` returns `uint32` → unpack bytes:
   - `(packed >> 24) & 0xFF` = MaxFatigue
   - `(packed >> 16) & 0xFF` = Fatigue
   - `(packed >> 8) & 0xFF` = MaxStamina
   - `packed & 0xFF` = Stamina

### Two EE Regions

All CE offsets resolve from the same base (`0x20267000`), but they land in two distinct EE regions:

| Region | CE Offset Range | EE Range | Addresses |
|---|---|---|---|
| Short-offset | `0x83x`–`0x86x` | `0x2026783x`–`0x2026786x` | Gold, Stamina, Weather, Fodder, Watering Can, Active Item, Active Tool |
| Large-offset | `0x5F32xx` | `0x2085A2xx` | Time, Chicken Feed |

The offset magnitude reflects where PCSX2 maps the underlying EE memory pages in host address space; both regions are valid PINE targets.

## Verified EE Addresses

All addresses below are confirmed via the CE-to-EE conversion procedure (base `0x20267000`) and validated by live PINE reads or cross-referenced community artifacts.

| Label | EE Address | CE Offset | Value Format | Notes |
|---|---|---|---|---|
| **TIME** | `0x2085A2F4` | `0x5F32F4` | `uint32` → `[season, day, hour, minute]` | Season: 0=Spring, 1=Summer, 2=Autumn, 3=Winter; day 1-based; hour 0–23; minute 0–59. Randomizer writes day at `0x2085A2F6`, season at `0x2085A2F7`. |
| **STAMINA** | `0x20267830` | `0x830` | `uint32` → `[maxFatigue, fatigue, maxStamina, stamina]` | Each byte is 0–255. Max values shift with Power Berry count. Activity drains 2 stamina (4 in rain). Stamina 0 blocks activities. `IsMaxed` when fatigue == maxFatigue. |
| **GOLD** | `0x20267864` | `0x864` | `uint32` → raw value | Gold = Stamina + 0x34 (EE offset delta matches CE offset delta `864 - 830 = 0x34`). |
| **WEATHER** | `0x20267834` | `0x834` | `uint32` → `0000XXYY` | Today = `value & 0xFF`, Forecast = `(value >> 8) & 0xFF`. Weather is rolled randomly within seasonal probability ranges; TV forecast is a prediction that can miss. |
| **ACTIVE TOOL** | `0x20267844` | `0x844` | `uint32` → `000000ZZ` | Tool ID: `0xFF`=Empty, `0x51`=Sickle, `0x3A`=Chicken Feed, `0x53`=Hoe, `0x54`=Watering Can, `0x55`=Fishing Rod, `0x5A`=Flute. |
| **ACTIVE ITEM** | `0x20267840` | `0x840` | `uint32` → `000000ZZ` | Item ID. Mapping incomplete; display hex ID until full lookup table is built. |
| **FODDER** | `0x20267838` | `0x838` | `uint32` → `000000NN` | Barn fodder count (separate from inventory fodder). |
| **WATERING CAN** | `0x2026783C` | `0x83C` | `uint32` → `000000NN` | Remaining water charges. |
| **CHICKEN FEED** | `0x2085A2D8` | `0x5F32D8` | `uint32` → `00NN0000` | `(value >> 16) & 0xFF` = remaining feed count. Large-offset region (same page as Time). |

## Cheat Engine Observations

Base pointer: `pcsx2-qt.exe` + `0x0317C238`

| Label | Offset | Size | Format |
|---|---|---|---|
| **GOLD** | `+0x864` | 4 bytes | `uint32` |
| **STAMINA** | `+0x830` | 4 bytes | `[maxFatigue, fatigue, maxStamina, stamina]` |
| **TIME** | `+0x5F32F4` | 4 bytes | `[season, day, hour, minute]` |
| **WEATHER** | `+0x834` | 4 bytes | `0000XXYY` (forecast \| today) |
| **ACTIVE TOOL** | `+0x844` | 4 bytes | `000000ZZ` (tool ID) |
| **ACTIVE ITEM** | `+0x840` | 4 bytes | `000000ZZ` (item ID) |
| **FODDER** | `+0x838` | 4 bytes | `000000NN` |
| **WATERING CAN** | `+0x83C` | 4 bytes | `000000NN` |
| **CHICKEN FEED** | `+0x5F32D8` | 4 bytes | `00NN0000` (high byte = count) |

## Cross-Reference Evidence

| Source | What it Confirms |
|---|---|
| **SaveTheHomelandRandomizer** (Dezert8, GPL-3) | Day written at `0x2085A2F6`, season at `0x2085A2F7`; `UncappedEndings.lua` labels `0x2085A2F4` = time (value 1260); ending refs at `0x20267750`, `0x20267724`, cutscene `0x2026776C` in save block; inventory at `0x20244xxx` |
| **Ushi No Tane GS2 codes** | Season/day/hour on adjacent scrambled addresses (`251B/251A/251C`) consistent with contiguous calendar struct |
| **GS2 Gold vs Energy** | Prefix delta exactly `0x34` → matches CE offset delta `864 - 830 = 0x34` |
| **Live Cheat Engine sessions** | Weather, Active Tool, Active Item, Fodder, Watering Can, Chicken Feed offsets validated via pointer reads under `pcsx2-qt.exe + 0x0317C238` |

## Open Items

1. **RPM-on-host vs derived EE for PINE** — **RESOLVED**: PINE-only path chosen for POC (simpler, PINE already works); RPM fallback deferred to real app if needed.
2. **TIME offset scale anomaly** — **RESOLVED**: EE TIME anchor `0x2085A2F4` mapped directly; CE host offset `0x5F32F4` noted as different region (host process space vs EE RAM).
3. **Weather address** — **RESOLVED**: Found at `0x20267834` (CE offset `0x834`), format `0000XXYY`.
4. **Hour/minute encoding** — **RESOLVED**: EE `0x2085A2F4` read as u32 with byte layout `[season, day, hour, minute]` matching CE dword format.
5. **Active Item ID mapping** — **OPEN**: item ID at `0x20267840` reads correctly but full ID-to-name lookup table not yet built; display hex ID until mapped.

## Validation Policy

- **Cheat Engine observations** = primary evidence for host addresses
- **Randomizer/GS2** = corroboration for EE anchor + delta logic
- **Live PINE read** = final validation (what the app actually ships)

---
*Generated for ENH-008 #20. Updated for DOC-004 #57: comprehensive CE-to-EE conversion procedure with all known addresses, value formats, and two-region note.*
