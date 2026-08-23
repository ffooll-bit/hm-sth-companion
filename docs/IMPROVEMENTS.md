# IMPROVEMENTS

_The tracker for feature ideas, found bugs, and optimization plans. Each finding is recorded as an item under Items: copy the template below, fill it in, and place the item at the very bottom of the Items section._

## Item Template

```markdown
### <ID> — <Title>
- **Status:** `recorded` | `verified` | `rejected` | `implemented`
- **Issue:** <#NN> | `—`
- **Recorded:** YYYY-MM-DD HH:MM
- **Implemented:** YYYY-MM-DD HH:MM | `—`
- **Problem:** ...
- **Possible Fix:** ...
- **Actual Fix:** ...
- **Rejection Reason:** ...
- **Actual Implemented:** ...
- **Changes:** ...
```

Item IDs follow the format `<LABEL_CODE>-<NNN>` built from the default GitHub labels, with numbers counted per label code:

| GitHub Label | Code |
|--------------|------|
| `bug` | BUG |
| `documentation` | DOC |
| `enhancement` | ENH |
| `duplicate` | DUP |
| `good first issue` | GFI |
| `help wanted` | HW |
| `invalid` | INV |
| `question` | QST |
| `wontfix` | WFX |

## Items

### ENH-001 — Repository structure not verified against the standard
- **Status:** `implemented`
- **Issue:** `—`
- **Recorded:** 2026-08-22 17:02
- **Implemented:** 2026-08-23 17:29
- **Problem:** The repository is newly created and its folders, guardrail files, and settings have not yet been confirmed against the workflow standard.
- **Possible Fix:** Run the verification checks from the workflow before the first commit.
- **Actual Fix:** Verification was performed during bootstrap interaction 1 against the Project Bootstrap standard; the violations found then (CHANGELOG placement, hardwrapped documents, CoC placeholder) were fixed and merged via PR #1. Re-checked on 2026-08-22: structure still conforms.
- **Rejection Reason:** `—`
- **Actual Implemented:** The repository structure was created during Project Bootstrap interaction 1 and merged via PR #1; the review-gate violations (CHANGELOG placement, hardwrapped documents, CoC front matter and placeholder) were fixed in the same pull request. Structure re-verified against the bootstrap standard on 2026-08-23.
- **Changes:** Initial repository skeleton: README.md, CHANGELOG.md, LICENSE, CONTRIBUTING.md, CODE_OF_CONDUCT.md, SECURITY.md, .github/ workflows and templates, docs/IMPROVEMENTS.md, src/.gitkeep.

### ENH-002 — Application UI design specification
- **Status:** `implemented`
- **Issue:** #8
- **Recorded:** 2026-08-22 19:55
- **Implemented:** 2026-08-23 18:09
- **Problem:** The application has no defined interface plan yet — without an agreed set of screens, layouts, and a visual theme, feature implementation would be unguided and inconsistent.
- **Possible Fix:** Produce a UI design specification covering the required interfaces (live HUD dashboard, memory monitor, guide browser, settings), the layout of each screen, and the visual theme, reviewed against the data actually available from the memory reading mechanism (ENH-003) before implementation starts.
- **Actual Fix:** Produce the UI design specification after ENH-003, mapping every interface element to gameplay values actually readable from PCSX2 via PINE IPC. Constraint discovered during verification: WinForms on .NET 8 ships no built-in dark mode, so the visual theme needs a custom palette/renderer; the spec must define the screens (HUD dashboard, memory monitor, guide browser, settings), their layouts, and that theme explicitly.
- **Rejection Reason:** `—`
- **Actual Implemented:** `docs/UI_DESIGN_SPEC.md` defines the main window: three application states (disconnected / wrong game / playing), a two-column layout mirroring the social preview mock window (Game HUD + Memory Monitor, full-width Guide), a data contract mapping every element to proven PINE IPC reads with graceful `--` rendering for gameplay addresses not yet located, and a theme token table copied verbatim from the social preview palette (dark surfaces, accent #58a55c, Segoe UI + Consolas). WinForms dark-mode caveat recorded for re-check at implementation time.
- **Changes:** Added docs/UI_DESIGN_SPEC.md; docs/IMPROVEMENTS.md marks this item implemented; CHANGELOG.md gains a release note under [Unreleased].

### ENH-003 — PCSX2 connection, game detection & memory reading POC
- **Status:** `implemented`
- **Issue:** #9
- **Recorded:** 2026-08-22 19:55
- **Implemented:** 2026-08-23 17:47
- **Problem:** The core pipeline does not exist yet — the application cannot attach to the PCSX2 process, cannot confirm that the running game is Harvest Moon: Save the Homeland, and cannot read gameplay values from memory.
- **Possible Fix:** Console-app proof of concept using ReadProcessMemory (P/Invoke) against pcsx2.exe: locate the PS2 EE RAM base via a pointer chain (Cheat Engine table style), detect the game by scanning for the disc serial SLUS-20141 at its standard EE RAM location (fallback: PCSX2 window title), then read known addresses for stamina/money/weather and print live values. Priority 1 — every other item depends on this working.
- **Actual Fix:** Console-app proof of concept using the PINE IPC interface built into PCSX2 (TCP 127.0.0.1:28011 on Windows, enabled by the user in Settings): detect the game from the PINE metadata opcodes (serial/title/CRC) instead of memory scanning — correct US serial is SLUS-20251 (ELF SLUS_202.51), confirmed via redump.org, psxdatacenter, GameFAQs; then Read32 gameplay values at EE RAM addresses for stamina/money/weather once located with Cheat Engine. PINE reads EE addresses directly, eliminating fragile base-address discovery; keep ReadProcessMemory+EEmem only as fallback when IPC is disabled. Caveats: IPC must be toggled on per user, requests should stay sequential (queue drops past ~7 in-flight), no bulk-read opcode (~52 ms per 4 KiB). Priority 1 — every other item depends on this working.
- **Rejection Reason:** `—`
- **Actual Implemented:** A .NET 8 console project `src/HmSth.Poc` speaks the PINE IPC protocol directly over TCP 127.0.0.1:28011: strict sequential request/reply framing ([u16 size][opcode] -> [u16 size][result][data]), string metadata reads for emulator version / title / serial (ID opcode), a Read32 demo at an aligned EE RAM address, and game verification against serial SLUS-20251. Exit codes separate not-connected (prints the enablement hint), wrong-game, and success; build passes and the not-connected path was exercised locally - the live pass against running PCSX2 happens on the user's machine by design of a proof of concept.
- **Changes:** Added src/HmSth.Poc (csproj + Program.cs, zero NuGet packages); removed src/.gitkeep; docs/IMPROVEMENTS.md marks this item implemented; CHANGELOG.md gains a release note under [Unreleased].

### ENH-004 — Local AI tooling artifacts not gitignored
- **Status:** `implemented`
- **Issue:** #10
- **Recorded:** 2026-08-22 20:15
- **Implemented:** 2026-08-23 17:33
- **Problem:** Local AI tooling directories such as `.cortexkit/` keep showing up as untracked changes. This keeps `git status` dirty and risks an accidental commit of internal tooling data to the public repository.
- **Possible Fix:** Add tooling artifact entries (`.cortexkit/`, plus `.playwright-mcp/` for future web testing sessions) to `.gitignore`.
- **Actual Fix:** Add an 'AI tooling' section to `.gitignore` with entries `.cortexkit/` and `.playwright-mcp/`. Verified against official gitignore semantics (git-scm.com): trailing-slash patterns match directories only, so both tooling roots are ignored entirely regardless of nested content — the plugin's own nested `.gitignore` does not prevent the untracked-directory report. No application code involved.
- **Rejection Reason:** `—`
- **Actual Implemented:** An 'AI tooling' section was appended to `.gitignore` containing `.cortexkit/` and `.playwright-mcp/`; both directories no longer appear as untracked.
- **Changes:** .gitignore gained the 'AI tooling' section; docs/IMPROVEMENTS.md marks this item implemented; CHANGELOG.md gains a release note under [Unreleased].

### ENH-005 — CI never builds the application code
- **Status:** `verified`
- **Issue:** `—`
- **Recorded:** 2026-08-23 18:59
- **Implemented:** `—`
- **Problem:** The required `build` check only validates markdown hygiene (CRLF/BOM). Since PR #14 the repository contains a real .NET application, but no automation compiles it - a broken commit can merge with green checks.
- **Possible Fix:** Extend ci.yml so the build job also runs `dotnet build` (and a formatting verification) on the solution, making the required check actually gate application code.
- **Actual Fix:** Verified against ci.yml (only markdown CRLF/BOM checks exist) and the official actions/setup-dotnet README (current major v6; pin `dotnet-version: '8.0.x'` or the runner silently uses its preinstalled latest SDK). Add `actions/setup-dotnet@v6` pinned to 8.0.x plus a solution-wide `dotnet build` to the existing build job. The formatting verification from the initial plan is dropped: `dotnet build` alone is the compilation gate, formatting stays a local pre-flight before each commit.
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### DOC-001 — README has no getting-started instructions
- **Status:** `verified`
- **Issue:** `—`
- **Recorded:** 2026-08-23 18:59
- **Implemented:** `—`
- **Problem:** The README is marketing-style (introduction and badges only). There is no getting-started section: prerequisites (.NET 8 SDK, PCSX2 with PINE IPC enabled), build/run commands, or expected output. Anyone wanting to try the proof of concept must read the source to figure out how.
- **Possible Fix:** Add a Getting Started section to README.md covering prerequisites, clone/build/run commands for src/HmSth.Poc, and what output to expect.
- **Actual Fix:** Verified: the problem is narrower than recorded - README already has Requirements and Quick start sections, but neither helps try the POC (Quick start still says "Not available yet" and points to Releases; .NET 8 SDK prerequisite and PINE IPC enabling are unmentioned). Revise the existing sections instead of adding new ones: Requirements gains ".NET 8 SDK"; Quick start becomes developer instructions - enable PINE IPC in PCSX2 settings (TCP 127.0.0.1:28011, verified during ENH-003), run `dotnet run --project src/HmSth.Poc`, document expected output and exit codes 0/1/2.
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-006 — Integrate reverse-engineered gameplay memory locations
- **Status:** `verified`
- **Issue:** `—`
- **Recorded:** 2026-08-23 18:59
- **Implemented:** `—`
- **Problem:** The UI design spec renders stamina/money/weather as `--` because gameplay addresses were unknown, but valid Cheat Engine locations against running pcsx2-qt.exe have since been found (base `"pcsx2-qt.exe"+0317C238`): GOLD at offset `864`, STAMINA at `830`, TIME at `5F32F4`. Documented formats: STAMINA is 4 bytes `[maxFatigue, fatigue, maxStamina, stamina]` where max values normally match and shift with Power Berry count (e.g. `8C 00 8C 8C`; normal activity costs -2 stamina, rain -4 and raises fatigue; stamina 0 blocks activities, fatigue at max when YY=XX); TIME is `[season, day, hour, minute]` (e.g. `00 07 06 00`). Weather remains unfound.
- **Possible Fix:** Document this memory map in its own doc, then add a reading layer that maps the three values into the app. Open items to resolve during implementation: the CE pointers live in host-process address space while PINE reads EE addresses, so choose between direct ReadProcessMemory on the host addresses (the already-planned fallback path) or deriving equivalent EE addresses for PINE; confirm the offset scale anomaly of TIME (`5F32F4` vs short offsets `864`/`830`); locate the weather address.
- **Actual Fix:** Online verification corroborates without contradicting: Ushi No Tane GS2 codes place day/hour on adjacent addresses (consistent with the contiguous `[season, day, hour, minute]` layout), and NTSC-U Never Tired / Infinite Money cheat listings (IGN, Almar's Guides, supercheats) confirm stamina/gold targets exist in this memory region. The user's live Cheat Engine observations remain primary evidence; final validation happens when the implementation reads live values. Plan unchanged: document the memory map in its own doc, then add a reading layer mapping GOLD/STAMINA/TIME into the app, resolving the three open items (RPM-on-host vs derived EE addresses for PINE, the TIME offset scale anomaly, the missing weather address).
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-007 — PINE framing logic has zero tests
- **Status:** `verified`
- **Issue:** `—`
- **Recorded:** 2026-08-23 18:59
- **Implemented:** `—`
- **Problem:** The POC's protocol logic (Request/ReadString/ReadU32: packet framing, result-code handling, string parsing) is pure and testable without an emulator, but no tests exist; this code will be carried into the real client unchanged.
- **Possible Fix:** Add a small xUnit project covering framing and parsing edge cases (short reads, non-zero result codes, malformed string payloads), so the tests travel with the code into the real client.
- **Actual Fix:** Verified: no test project exists anywhere in the repository. Use xUnit v3 (official README: supports .NET 8.0+) in a small test project covering framing and parsing edge cases (short reads, non-zero result codes, malformed string payloads), executed via `dotnet test`. Implement after ENH-005 so the tests run automatically in CI.
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-008 — Verified EE anchor for TIME plus candidate map for further monitors
- **Status:** `verified`
- **Issue:** `—`
- **Recorded:** 2026-08-23 19:52
- **Implemented:** `—`
- **Problem:** Only three gameplay values are known to the project (GOLD, STAMINA, TIME), while community artifacts surfaced raw EE addresses that were never cross-checked against the companion's own Cheat Engine findings; without a confirmed host-to-EE correspondence every future monitor has to be hunted blind.
- **Possible Fix:** Record the verified anchor from the open-source Save The Homeland Randomizer (GPL-3, hooks PCSX2): its scripts write day at EE `0x2085A2F6` and season at `0x2085A2F7`, and game code reads `0x2085A2F4`/`F5` immediately before them - almost certainly matching the discovered contiguous `[season, day, hour, minute]` dword, which makes `0x002085A2F4` the likely EE equivalent of the host pointer target. Use it as the bridge to derive EE addresses for GOLD/STAMINA and as the starting neighborhood (`0x2085A2E2-E8`) for the weather hunt. Additional candidates visible in the randomizer table: year (likely near the date struct), power berry count, item inventory slots at `0x20244xxx`.
- **Actual Fix:** Verified via three independent sources that lock together: (1) SaveTheHomelandRandomizer source writes day=`0x2085A2F6`, season=`0x2085A2F7`, and `UncappedEndings.lua` explicitly labels `0x2085A2F4` as time (writes value 1260); (2) Ushi No Tane GS2 season/day/hour codes sit on adjacent scrambled addresses (251B/251A/251C), consistent with one contiguous calendar struct; (3) GS2 Gold vs Energy prefixes differ by exactly 0x34, matching the discovered CE offsets delta (864 - 830 = 0x34). Final plan: record the anchor table in a dedicated memory-map doc during ENH-006 implementation; treat the `0x002085A2F4` region as primary candidate for deriving EE equivalents of GOLD/STAMINA; note hour/minute likely derive from a u16 minute counter (1260 = about 21:00) while the CE dword's HH/MM bytes are converted runtime copies; Cheat Engine observations remain primary evidence until live-read validation.
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-009 — Daily briefing: weather today and tomorrow, shop open days
- **Status:** `verified`
- **Issue:** `—`
- **Recorded:** 2026-08-23 19:52
- **Implemented:** `—`
- **Problem:** Players must boot the in-game TV forecast every morning to learn today's and tomorrow's weather, and shop closures still surprise them mid-trip; neither is available at a glance while playing.
- **Possible Fix:** Monitor today's weather and tomorrow's forecast once a weather address is found (no public raw address exists; hunt via Cheat Engine starting from the ENH-008 anchor neighborhood). Shop open/holiday status is served from curated online data instead of memory reading, since schedules are static.
- **Actual Fix:** Verified with a correction from live gameplay observation: the seasonal Dry/Mild/Wet calendar (Ushi No Tane `weather.php`) defines each day's probability distribution rather than a fixed outcome - actual weather is rolled randomly within those ranges, and the in-game TV forecast is only a prediction that can occasionally miss (percentage-based). Consequently tomorrow's weather cannot be computed reliably from the date alone; accurate display requires locating the weather values in memory via Cheat Engine - today's actual state and, if the game pre-rolls it, the next-day value. The static calendar stays useful as a fallback estimate and for presenting the odds alongside the monitored truth. Shop data confirmed complete: Ushi No Tane `townshops.php` lists all 8 shops with opening hours and closed weekdays, suitable as curated static content.
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-010 — Active item and tool slot monitor
- **Status:** `verified`
- **Issue:** `—`
- **Recorded:** 2026-08-23 19:52
- **Implemented:** `—`
- **Problem:** HM:StH keeps two equip slots (active item and active tool); the active tool is invisible during gameplay and only revealed after pausing, which breaks flow whenever the player must confirm what is equipped.
- **Possible Fix:** Read both equip slots live and display item/tool identities in the HUD. Slot addresses unknown; hunt via Cheat Engine by switching equipment and diffing.
- **Actual Fix:** Verified: no public raw address exists for the equip slots; the problem stands and the approach is unchanged - Cheat Engine diff-hunt by switching equipment. The randomizer's runtime clusters (for example `0x202729xx` in its table) provide starting neighborhoods for the scan.
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-011 — Save profile dashboard
- **Status:** `verified`
- **Issue:** `—`
- **Recorded:** 2026-08-23 19:52
- **Implemented:** `—`
- **Problem:** Long-term progress facts are scattered across pause menus or locked behind endings: livestock product levels (none/small/medium/large/golden), pet names and hearts, horse race time-attack results, fish catch counts including the three legendary fish and the biggest-catch record, completed endings out of nine, unlocked character profiles, and full bag and fridge inventories.
- **Possible Fix:** Locate and read the save-state block to surface all listed facts in one panel. Character profiles are deliberately read from game memory rather than online lists, because unlock state differs per save file.
- **Actual Fix:** Verified with strengthened anchors: the randomizer's ending scripts write ending progression refs (`0x20267750`, `0x20267724`, cutscene status `0x2026776C`) inside the save-persistent struct region around `0x2085A2xx`, localizing where endings state lives. Inventory id/count pairs are anchored at `0x20244xxx`. Livestock products, pets/hearts, race results, fishing records, and character profiles have no public map and proceed as planned via Cheat Engine hunts against that save block; reading profiles from game memory rather than online lists is confirmed as the right approach since unlock state differs per save.
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-012 — Farm operations monitor: crops, animal care, tool condition, fodder
- **Status:** `verified`
- **Issue:** `—`
- **Recorded:** 2026-08-23 19:52
- **Implemented:** `—`
- **Problem:** Field and barn upkeep state requires walking everywhere or opening many menus: whether each planted crop is watered or harvest-ready, whether each cow/chicken was brushed, milked, or fed (outdoor animals need no feeding), remaining uses of depletable tools such as the watering can and chicken feed, sickle/fishing-rod upgrade tiers, and the barn fodder counter that grows by cutting grass plots.
- **Possible Fix:** Map the crop plot array and animal state records, then display an upkeep checklist. Barn fodder is expected to live in a separate counter distinct from inventory fodder.
- **Actual Fix:** Verified: no public raw addresses exist for crop plots, animal care flags, tool condition, or the barn fodder counter; none of the community artifacts touch these systems. Approach unchanged - Cheat Engine hunts with state transitions (water/don't water, brush/milk/feed, cut grass). The inventory-side fodder count is expected at the `0x20244xxx` id/count pairs, while the barn counter is anticipated as a separate value; the distinction will be confirmed during the hunt.
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-013 — Live world map with real-time NPC and animal positions
- **Status:** `verified`
- **Issue:** `—`
- **Recorded:** 2026-08-23 19:52
- **Implemented:** `—`
- **Problem:** Finding specific villagers without memorizing their daily schedules wastes significant playtime; nothing in-game shows where characters currently are.
- **Possible Fix:** Render an in-app map with live positions of villagers and animals. This is the highest-difficulty item of the batch: entity coordinates may be transient or unstable in memory - flagged as exploratory and may be descoped after initial Cheat Engine probing.
- **Actual Fix:** Verified as exploratory: no public source documents entity coordinate storage, and none of the community artifacts (randomizer, cheat listings) touch NPC or animal positions. The high-risk assessment stands; initial Cheat Engine probing against moving villagers will decide whether the item proceeds or is descoped.
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`
