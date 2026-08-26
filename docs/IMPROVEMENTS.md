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
- **Status:** `implemented`
- **Issue:** #17
- **Recorded:** 2026-08-23 18:59
- **Implemented:** 2026-08-24 01:46
- **Problem:** The required `build` check only validates markdown hygiene (CRLF/BOM). Since PR #14 the repository contains a real .NET application, but no automation compiles it - a broken commit can merge with green checks.
- **Possible Fix:** Extend ci.yml so the build job also runs `dotnet build` (and a formatting verification) on the solution, making the required check actually gate application code.
- **Actual Fix:** Verified against ci.yml (only markdown CRLF/BOM checks exist) and the official actions/setup-dotnet README (current major v6; pin `dotnet-version: '8.0.x'` or the runner silently uses its preinstalled latest SDK). Add `actions/setup-dotnet@v6` pinned to 8.0.x plus a solution-wide `dotnet build` to the existing build job. The formatting verification from the initial plan is dropped: `dotnet build` alone is the compilation gate, formatting stays a local pre-flight before each commit.
- **Rejection Reason:** `—`
- **Actual Implemented:** A minimal solution `HmSth.sln` was created at the repository root containing `src/HmSth.Poc`, so `dotnet build` resolves without an explicit path and future projects (the planned test project) join by one `dotnet sln add`. The CI `build` job gained `actions/setup-dotnet@v6` pinned to `8.0.x` followed by a solution-wide `dotnet build` step; the markdown hygiene checks are unchanged. Verified locally with the same commands before commit.
- **Changes:** Added HmSth.sln; .github/workflows/ci.yml gained the setup-dotnet and Build solution steps; docs/IMPROVEMENTS.md marks this item implemented; CHANGELOG.md gains a release note under [Unreleased].

### DOC-001 — README has no getting-started instructions
- **Status:** `implemented`
- **Issue:** #16
- **Recorded:** 2026-08-23 18:59
- **Implemented:** 2026-08-24 12:28
- **Problem:** The README is marketing-style (introduction and badges only). There is no getting-started section: prerequisites (.NET 8 SDK, PCSX2 with PINE IPC enabled), build/run commands, or expected output. Anyone wanting to try the proof of concept must read the source to figure out how.
- **Possible Fix:** Add a Getting Started section to README.md covering prerequisites, clone/build/run commands for src/HmSth.Poc, and what output to expect.
- **Actual Fix:** Verified: the problem is narrower than recorded - README already has Requirements and Quick start sections, but neither helps try the POC (Quick start still says "Not available yet" and points to Releases; .NET 8 SDK prerequisite and PINE IPC enabling are unmentioned). Revise the existing sections instead of adding new ones: Requirements gains ".NET 8 SDK"; Quick start becomes developer instructions - enable PINE IPC in PCSX2 settings (TCP 127.0.0.1:28011, verified during ENH-003), run `dotnet run --project src/HmSth.Poc`, document expected output and exit codes 0/1/2.
- **Rejection Reason:** `—`
- **Actual Implemented:** Revised the existing Requirements and Quick start sections in README.md. Requirements now lists .NET 8 SDK and PINE IPC enablement with the TCP endpoint. Quick start provides the exact `dotnet run` command, expected success output (emulator version, title, serial, demo memory read), and documents exit codes 0/1/2 with their meanings.
- **Changes:** README.md updated (Requirements + Quick start sections rewritten); docs/IMPROVEMENTS.md marks this item implemented; CHANGELOG.md gains a release note under [Unreleased].

### ENH-006 — Integrate reverse-engineered gameplay memory locations
- **Status:** `implemented`
- **Issue:** #18
- **Recorded:** 2026-08-23 18:59
- **Implemented:** 2026-08-24 13:15
- **Problem:** The UI design spec renders stamina/money/weather as `--` because gameplay addresses were unknown, but valid Cheat Engine locations against running pcsx2-qt.exe have since been found (base `"pcsx2-qt.exe"+0317C238`): GOLD at offset `864`, STAMINA at `830`, TIME at `5F32F4`. Documented formats: STAMINA is 4 bytes `[maxFatigue, fatigue, maxStamina, stamina]` where max values normally match and shift with Power Berry count (e.g. `8C 00 8C 8C`; normal activity costs -2 stamina, rain -4 and raises fatigue; stamina 0 blocks activities, fatigue at max when YY=XX); TIME is `[season, day, hour, minute]` (e.g. `00 07 06 00`). Weather remains unfound.
- **Possible Fix:** Document this memory map in its own doc, then add a reading layer that maps the three values into the app. Open items to resolve during implementation: the CE pointers live in host-process address space while PINE reads EE addresses, so choose between direct ReadProcessMemory on the host addresses (the already-planned fallback path) or deriving equivalent EE addresses for PINE; confirm the offset scale anomaly of TIME (`5F32F4` vs short offsets `864`/`830`); locate the weather address.
- **Actual Fix:** Online verification corroborates without contradicting: Ushi No Tane GS2 codes place day/hour on adjacent addresses (consistent with the contiguous `[season, day, hour, minute]` layout), and NTSC-U Never Tired / Infinite Money cheat listings (IGN, Almar's Guides, supercheats) confirm stamina/gold targets exist in this memory region. The user's live Cheat Engine observations remain primary evidence; final validation happens when the implementation reads live values. Resolved: PINE-only path (no RPM fallback for POC); EE TIME anchor `0x002085A2F4` used directly; GOLD derived as STAMINA_EE + 0x34 (GS2/CE delta); STAMINA candidate `0x002085A2E5`; weather returns "Unknown" pending address hunt. Added `GameMemoryReader` with `ReadGold()`, `ReadStamina()`, `ReadTime()`, `ReadWeather()` using `PineClient.ReadU32`; tests via `FakePineServer`; `MEMORY_MAP.md` updated with resolution status.
- **Rejection Reason:** `—`
- **Actual Implemented:** Created `src/HmSth.Poc/GameMemoryReader.cs` with PINE-only reads for Gold, Stamina, Time, Weather using derived EE addresses from MEMORY_MAP.md; updated `Program.cs` to print all four values after serial verification; added `tests/HmSth.Poc.Tests/GameMemoryReaderTests.cs` with 6 facts covering parsing and error paths; updated `docs/MEMORY_MAP.md` marking items 1, 2, 4 resolved and item 3 (weather) open; docs/IMPROVEMENTS.md marks this item implemented; CHANGELOG.md gains a release note under [Unreleased].
- **Changes:** Added src/HmSth.Poc/GameMemoryReader.cs, tests/HmSth.Poc.Tests/GameMemoryReaderTests.cs; modified src/HmSth.Poc/Program.cs, docs/MEMORY_MAP.md; docs/IMPROVEMENTS.md marks this item implemented; CHANGELOG.md gains a release note under [Unreleased].

### ENH-007 — PINE framing logic has zero tests
- **Status:** `implemented`
- **Issue:** #19
- **Recorded:** 2026-08-23 18:59
- **Implemented:** 2026-08-24 02:19
- **Problem:** The POC's protocol logic (Request/ReadString/ReadU32: packet framing, result-code handling, string parsing) is pure and testable without an emulator, but no tests exist; this code will be carried into the real client unchanged.
- **Possible Fix:** Add a small xUnit project covering framing and parsing edge cases (short reads, non-zero result codes, malformed string payloads), so the tests travel with the code into the real client.
- **Actual Fix:** Verified: no test project exists anywhere in the repository. Use xUnit v3 (official README: supports .NET 8.0+) in a small test project covering framing and parsing edge cases (short reads, non-zero result codes, malformed string payloads), executed via `dotnet test`. Implement after ENH-005 so the tests run automatically in CI.
- **Rejection Reason:** `—`
- **Actual Implemented:** A test project `tests/HmSth.Poc.Tests` (xUnit v3 stable 4.0.0 via official templates, Microsoft Testing Platform) was created and added to the solution with a ProjectReference to the POC. `PineClient`/`PineCommand` moved to their own file as `public` (only production-code change; zero behaviour change). Thirteen facts exercise framing and parsing through an in-process loopback fake PCSX2: request frame shape for Read32 vs metadata opcodes, response sizes below six, non-zero result codes, connection closed before header and mid-body, string payloads of zero length / length beyond data / missing null terminator, and truncated u32 payloads. CI gained a `dotnet test --no-build` step after the build step, so the suite runs on every push.
- **Changes:** Added tests/HmSth.Poc.Tests; src/HmSth.Poc/PineClient.cs split out of Program.cs (made public); HmSth.sln includes the test project; .github/workflows/ci.yml gained the Run tests step; docs/IMPROVEMENTS.md marks this item implemented; CHANGELOG.md gains a release note under [Unreleased].

### ENH-008 — Verified EE anchor for TIME plus candidate map for further monitors
- **Status:** `implemented`
- **Issue:** #20
- **Recorded:** 2026-08-23 19:52
- **Implemented:** 2026-08-24 12:47
- **Problem:** Only three gameplay values are known to the project (GOLD, STAMINA, TIME), while community artifacts surfaced raw EE addresses that were never cross-checked against the companion's own Cheat Engine findings; without a confirmed host-to-EE correspondence every future monitor has to be hunted blind.
- **Possible Fix:** Record the verified anchor from the open-source Save The Homeland Randomizer (GPL-3, hooks PCSX2): its scripts write day at EE `0x2085A2F6` and season at `0x2085A2F7`, and game code reads `0x2085A2F4`/`F5` immediately before them - almost certainly matching the discovered contiguous `[season, day, hour, minute]` dword, which makes `0x002085A2F4` the likely EE equivalent of the host pointer target. Use it as the bridge to derive EE addresses for GOLD/STAMINA and as the starting neighborhood (`0x2085A2E2-E8`) for the weather hunt. Additional candidates visible in the randomizer table: year (likely near the date struct), power berry count, item inventory slots at `0x20244xxx`.
- **Actual Fix:** Verified via three independent sources that lock together: (1) SaveTheHomelandRandomizer source writes day=`0x2085A2F6`, season=`0x2085A2F7`, and `UncappedEndings.lua` explicitly labels `0x2085A2F4` as time (writes value 1260); (2) Ushi No Tane GS2 season/day/hour codes sit on adjacent scrambled addresses (251B/251A/251C), consistent with one contiguous calendar struct; (3) GS2 Gold vs Energy prefixes differ by exactly 0x34, matching the discovered CE offsets delta (864 - 830 = 0x34). Created `docs/MEMORY_MAP.md` recording the verified TIME anchor, the GS2/CE delta correlation for deriving GOLD/STAMINA EE candidates, the weather hunt neighborhood, additional candidates (year, power berry, inventory), and the open items to resolve during ENH-006 (RPM vs derived EE, TIME scale anomaly, weather address, hour/minute encoding).
- **Rejection Reason:** `—`
- **Actual Implemented:** Created `docs/MEMORY_MAP.md` as a dedicated memory-map reference documenting the verified EE TIME anchor at `0x002085A2F4` (contiguous `[season, day, hour, minute]` dword), the GS2/CE delta `0x34` correlation for deriving GOLD/STAMINA EE candidates, the weather hunt neighborhood `0x2085A2E2–E8`, additional candidates from the randomizer (year, power berry count, inventory at `0x20244xxx`), and a validation policy distinguishing Cheat Engine primary evidence from live-read final validation. Four open items recorded for ENH-006 resolution.
- **Changes:** Added docs/MEMORY_MAP.md; docs/IMPROVEMENTS.md marks this item implemented; CHANGELOG.md gains a release note under [Unreleased].

### ENH-009 — Daily briefing: weather today and tomorrow, shop open days
- **Status:** `verified`
- **Issue:** #21
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
- **Issue:** #22
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
- **Issue:** #23
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
- **Issue:** #24
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
- **Issue:** #25
- **Recorded:** 2026-08-23 19:52
- **Implemented:** `—`
- **Problem:** Finding specific villagers without memorizing their daily schedules wastes significant playtime; nothing in-game shows where characters currently are.
- **Possible Fix:** Render an in-app map with live positions of villagers and animals. This is the highest-difficulty item of the batch: entity coordinates may be transient or unstable in memory - flagged as exploratory and may be descoped after initial Cheat Engine probing.
- **Actual Fix:** Verified as exploratory: no public source documents entity coordinate storage, and none of the community artifacts (randomizer, cheat listings) touch NPC or animal positions. The high-risk assessment stands; initial Cheat Engine probing against moving villagers will decide whether the item proceeds or is descoped.
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### BUG-001 — PINE IPC timeout causes unhandled exception when PCSX2 connected but unresponsive
- **Status:** `implemented`
- **Issue:** #33
- **Recorded:** 2026-08-24 14:35
- **Implemented:** 2026-08-24 15:45
- **Problem:** When PCSX2 is running with a game loaded in-game and PINE IPC enabled (TCP 127.0.0.1:28011), the TCP connection succeeds but PCSX2 does not respond to PINE requests within the 5-second read timeout. This throws an unhandled `SocketException` (error code 10060, "connection timed out") wrapped in `IOException` from `NetworkStream.Read()`, which bubbles up as an unhandled exception crash instead of a graceful error message with a distinct exit code. The user sees a stack trace rather than actionable guidance.
- **Possible Fix:** In `PineClient.ReadExact()` or `PineClient.Request()`, catch `IOException`/`SocketException` where the inner exception is a timeout (SocketErrorCode `TimedOut` / 10060). Throw a custom `PineConnectionException` with a descriptive message: "Connected to PCSX2 but no PINE response received — ensure the game is fully in-game (not paused/menu) and PINE IPC is fully initialized in PCSX2 settings." In `Program.Main()`, catch this exception and return exit code 3 with the friendly message. Keep the existing exit codes: 1 = connection refused, 2 = wrong game serial, 3 = connected but no response.
- **Actual Fix:** Catch `IOException`/`SocketException` timeout in `PineClient.ReadExact()`, throw custom `PineConnectionException` with actionable message. In `Program.Main()`, catch this exception and return exit code 3. Added test for timeout scenario via `FakePineServer.ServeOneTimeout()`. Files: `src/HmSth.Poc/PineClient.cs` (add custom exception + catch in ReadExact), `src/HmSth.Poc/Program.cs` (add catch for exit code 3), `tests/HmSth.Poc.Tests/PineClientTests.cs` (timeout test), `tests/HmSth.Poc.Tests/FakePineServer.cs` (timeout simulation).
- **Rejection Reason:** `—`
- **Actual Implemented:** Added `PineConnectionException` class; modified `ReadExact()` to catch `SocketException.TimedOut` and wrap in `PineConnectionException`; modified `Program.Main()` to catch `PineConnectionException` and return exit code 3; added `FakePineServer.ServeOneTimeout()` for test simulation; added test `ReadString_Timeout_ThrowsPineConnectionException`. Updated IMPROVEMENTS.md BUG-001 to implemented; CHANGELOG.md gains release note under [Unreleased].
- **Changes:** `src/HmSth.Poc/PineClient.cs`, `src/HmSth.Poc/Program.cs`, `tests/HmSth.Poc.Tests/PineClientTests.cs`, `tests/HmSth.Poc.Tests/FakePineServer.cs`

### BUG-002 — PINE IPC read timeout too aggressive for initial metadata requests
- **Status:** `implemented`
- **Issue:** #36
- **Recorded:** 2026-08-24 16:30
- **Implemented:** 2026-08-24 19:15
- **Problem:** The 5-second read timeout (`ReadTimeoutMs = 5000`) triggers on initial metadata requests (Version, Title, Id) after game load because PINE IPC takes longer than 5s to initialize and respond. Users see "Connected to PCSX2 but no PINE response received..." even with correct game loaded and IPC enabled — it's a false positive timeout, not an actual connection failure.
- **Possible Fix:** Increase read timeout for metadata phase to 15000-30000ms (PINE IPC startup latency), optionally with 1 retry on timeout. Options: (1) Global timeout increase to 15000ms; (2) Separate timeouts: 15s for metadata reads (ReadString), 5s for gameplay reads (ReadU32); (3) Retry logic: on timeout, wait 1s and retry once before giving up.
- **Actual Fix:** Implement separate timeouts: 15s for metadata reads (`ReadString`), 5s for gameplay reads (`ReadU32`). Modify `ReadString()` and `ReadU32()` to set `stream.ReadTimeout` per-request type (metadata vs gameplay), then restore original timeout in `finally` block. `ReadExact()` uses the already-set stream timeout (no logic change needed). Additionally fixed protocol mismatch: PCSX2 v2.6.3 expects 4-byte size header (u32 LE) + 1-byte opcode/resultCode; updated request/response framing and `FakePineServer`/tests to match pine-client protocol.
- **Rejection Reason:** `—`
- **Actual Implemented:** (1) Separate timeouts: `ReadMetadataTimeoutMs = 15000`, `ReadGameplayTimeoutMs = 5000`; per-request timeout in `ReadString()`/`ReadU32()` with `finally` restore. (2) Protocol fixed to 4-byte size header (u32 LE) + 1-byte opcode/resultCode; updated `FakePineServer` and all 20 tests to match pine-client protocol. (3) Debug logging added (toggleable via `DebugLog` constant). Verified working with PCSX2 v2.6.3.
- **Changes:** `src/HmSth.Poc/PineClient.cs`, `tests/HmSth.Poc.Tests/FakePineServer.cs`, `tests/HmSth.Poc.Tests/PineClientTests.cs`

### ENH-014 — Build the WinForms companion application
- **Status:** `implemented`
- **Issue:** #39
- **Recorded:** 2026-08-25 14:30
- **Implemented:** 2026-08-26 15:29
- **Problem:** The POC console app proves the data path (PINE IPC → GameMemoryReader), but there is no shell application that renders the HUD specified in `docs/UI_DESIGN_SPEC.md`.
- **Possible Fix:** Create `src/HmSth.App` (net8.0 WinForms) referencing `PineClient` and `GameMemoryReader` via project reference; implement the three application states (Disconnected / Wrong game / Playing), the two-column layout (Game HUD + Memory Monitor, full-width Guide), and the custom dark theme palette copied from the social preview.
- **Actual Fix:** Create `src/HmSth.App` (net8.0 WinForms), `ProjectReference` to `HmSth.Poc`, render 3 states (Disconnected / Wrong game / Playing) + 2-column HUD/Memory Monitor layout + custom dark theme from `docs/UI_DESIGN_SPEC.md`; add to `HmSth.sln`.
- **Rejection Reason:** `—`
- **Actual Implemented:** Added `src/HmSth.App` (net8.0-windows WinExe with `UseWindowsForms` and a `ProjectReference` to `HmSth.Poc`) containing `Program.cs` (WinForms entry), `Theme.cs` (dark palette tokens from `docs/UI_DESIGN_SPEC.md`), and `MainForm.cs` (two-column HUD + Memory Monitor, full-width Guide, connection strip, DWM immersive dark title bar, and a 1.5s sequential refresh timer). Added the project to `HmSth.sln`. Updated `.github/workflows/ci.yml` so the Linux `build` job builds only the cross-platform projects while a new `build-windows` job builds the full solution (validating the WinForms app compiles).
- **Changes:** A Windows companion app now renders the live Game HUD (Stamina bar, Money, Weather), a Memory Monitor of known EE addresses (Gold/Stamina/Time; FPS shows `--`), a Guide placeholder (save profile pending), and a connection strip with version/serial/state. It connects to PCSX2 PINE IPC and shows the three states (Disconnected / Wrong game / Playing). Weather, FPS, and Guide remain placeholders pending the ENH-009–011 CE hunts.

### DOC-002 — Two Magic Context documents untracked need a track/ignore decision
- **Status:** `implemented`
- **Issue:** #40
- **Recorded:** 2026-08-25 14:30
- **Implemented:** 2026-08-26 15:02
- **Problem:** `ARCHITECTURE.md` and `STRUCTURE.md` were created by Magic Context and remain untracked. A decision is needed: commit them to the repository (and maintain them) or ignore them via `.gitignore`.
- **Possible Fix:** Add both to `.gitignore` (they are session artifacts, not project deliverables) to keep `git status` clean. Alternative: track them and align with repo conventions.
- **Actual Fix:** Add `ARCHITECTURE.md` and `STRUCTURE.md` to `.gitignore` under an "AI/session artifacts" section (mirrors ENH-004's `.cortexkit/` entry) — they are session-generated, not project deliverables.
- **Rejection Reason:** `—`
- **Actual Implemented:** Added `ARCHITECTURE.md` and `STRUCTURE.md` to `.gitignore` under an "AI / session artifacts" section, mirroring ENH-004's `.cortexkit/` entry. They remain on disk locally for reference but are no longer tracked or flagged by `git status`.
- **Changes:** `git status` is now clean — the two Magic Context-generated documents are ignored. They are still present locally but excluded from the repository.

### BUG-003 — Gameplay memory reads (GOLD/TIME/STAMINA) return incorrect values
- **Status:** `implemented`
- **Issue:** #41
- **Recorded:** 2026-08-25 14:30
- **Implemented:** 2026-08-25 21:50
- **Problem:** `GameMemoryReader` reads derived EE addresses (TIME `0x002085A2F4`, STAMINA `0x002085A2E5`, GOLD `0x002085A319`) but the values do not match the live game. The user's authoritative Cheat Engine addresses are in host process space (`pcsx2-qt.exe`+0317C238 + offsets 864/830/5F32F4) and PINE reads EE RAM — the CE-to-EE translation used is incorrect.
- **Possible Fix:** Establish the correct CE-host-to-EE mapping (via SaveTheHomelandRandomizer source or live Cheat Engine session), then correct the EE addresses and byte decoding for TIME, STAMINA, GOLD. Note: a CE live session is already planned for ENH-009–013 and can resolve this in parallel.
- **Actual Fix:** Establish correct CE-host→EE mapping, then correct `GameMemoryReader` addresses + byte decoding. Two paths: (a) derive EE addresses from SaveTheHomelandRandomizer source (primary EE map, GPL-3) for the same game structs; (b) live CE session to locate EE equivalents of `pcsx2-qt.exe`+0317C238+{864,830,5F32F4}. Re-validate TIME decoding (EE dword `[?,?,day,season]` ≠ CE `[SS,DD,HH,MM]`). Resolvable alongside the ENH-009–013 CE hunt.
- **Rejection Reason:** `—`
- **Actual Implemented:** Hardcoded ground-truth EE addresses in `GameMemoryReader` (GOLD `0x20267864`, STAMINA `0x20267830`, TIME `0x2085A2F4`), resolved via the CE→EE translation (`EEmem` base `0x7FF740000000`). The existing `StaminaReading`/`TimeReading` byte decode was already correct (MSB-first: `max_fatigue`/`season` = MSB … `stamina`/`minute` = LSB) and was kept unchanged. Removed the guessed `WeatherAddress`/`DecodeWeather`; `ReadWeather` returns `"Unknown"`.
- **Changes:** Gameplay reads now return correct in-game GOLD, STAMINA, and TIME via PINE IPC (no RPM). Previously read derived/guessed EE addresses (correct decode, wrong locations) and returned garbage; the corrected addresses match the user's Cheat Engine layout. Weather stays `"Unknown"` pending the ENH-009 CE hunt.

### ENH-015 — Near-real-time HUD value updates
- **Status:** `implemented`
- **Issue:** #46
- **Recorded:** 2026-08-26 16:27
- **Implemented:** 2026-08-26 17:12
- **Problem:** The WinForms companion refreshes gameplay values only every 1500 ms (`src/HmSth.App/MainForm.cs:55`, `_timer.Interval = 1500`), so the Game HUD, Memory Monitor, and Guide feel stale. Lowering the interval is blocked by a hidden cost: all 7 PINE reads per tick (3 metadata: version/serial/title; 4 gameplay: gold/stamina/time/weather) run synchronously on the UI thread, so an over-short interval freezes the UI during each cycle. PINE's strictly-sequential, ~7-in-flight constraint also forbids parallelizing reads within a cycle.
- **Possible Fix:** Reduce the refresh interval to a safe near-real-time floor (e.g. 250–500 ms) and move the read loop off the UI thread (async/`Task`/`BackgroundWorker`) with UI-bound painting, or cache the 3 static metadata reads and re-read them only periodically. Keep PINE requests strictly sequential; respect the ~7-in-flight ceiling.
- **Actual Fix:** Verified: `_timer.Interval = 1500` ms (MainForm.cs:55) and `OnTick` (MainForm.cs:208) runs on the UI thread (Microsoft docs: System.Windows.Forms.Timer.Tick fires on the UI thread), performing 7 synchronous PINE reads (3 metadata + 4 gameplay) directly in the handler — blocking network I/O on the UI thread. Lower interval alone freezes the UI. Fix: run the read loop on a background thread (System.Threading.Timer / Task / BackgroundWorker) and marshal UI updates back via Control.Invoke/BeginInvoke; cache the 3 static metadata reads (version/serial/title) re-reading only on (re)connect; keep PINE requests strictly sequential to respect the ~7-in-flight ceiling. Target interval 250–500 ms.
- **Rejection Reason:** `—`
- **Actual Implemented:** `src/HmSth.App/MainForm.cs` now runs the 7 PINE reads on a background `Task` loop (`RefreshLoopAsync` at ~400 ms) instead of the UI-thread `System.Windows.Forms.Timer`; the 3 static metadata reads (version/serial/title) are cached per connection and re-read only on (re)connect; UI updates are marshaled back via `Control.BeginInvoke` (`RunOnUi`). The window no longer freezes during reads and refreshes ~3.75x more often.
- **Changes:** The HUD, Memory Monitor, and Guide refresh about every 400 ms (was 1500 ms) and stay responsive while PINE reads run on a background thread; emulator version/serial/title are read once per connection instead of every tick. Wrong-game and disconnected handling are unchanged in behaviour.

### ENH-016 — Rework companion UI/UX to desktop best practices
- **Status:** `implemented`
- **Issue:** #47
- **Recorded:** 2026-08-26 16:27
- **Implemented:** 2026-08-26 17:36
- **Problem:** The `src/HmSth.App` WinForms UI (dark `Theme`, hand-built `TableLayoutPanel` across Game HUD / Memory Monitor / Guide / status strip, built during ENH-014) was produced without a formal best-practice pass. Accessibility, layout consistency, and HUD readability over busy backgrounds are not verified against Windows/Fluent design guidance or WCAG 2.2.
- **Possible Fix:** Audit the current UI against researched best practices (Fluent/Windows design guidelines, WCAG 2.2 contrast, keyboard navigation, text scaling, design-system/component consistency, HUD readability over noisy backgrounds). Apply targeted fixes (accessibility contrast/scale, layout hygiene, theming tokens) rather than a full reskin. Scope (full reskin vs targeted compliance) to be confirmed during verification.
- **Actual Fix:** Verified: the src/HmSth.App UI (Theme.cs dark palette + MainForm.cs TableLayoutPanel) was built during ENH-014 without a formal accessibility/layout audit. Primary text contrast is high, but fonts are fixed 10 pt (no scaling) and there is no explicit keyboard/AT or WCAG pass. Fix: audit against Fluent/Windows guidelines + WCAG 2.2 (contrast 4.5:1 text / 3:1 UI, text scaling, never color-alone, focus order) and apply targeted fixes (scalable fonts, contrast edge checks, layout hygiene, theming tokens) — not a full reskin.
- **Rejection Reason:** `—`
- **Actual Implemented:** `src/HmSth.App/MainForm.cs` sets `AutoScaleMode = AutoScaleMode.Font` so the window scales with OS text-size/DPI settings; adds `AccessibleName` to the form and the Game HUD / Memory Monitor / Guide panels and the connection strip, with `AccessibleRole.Pane` on the three panels; adds `MinimumSize = 420×300` so the window cannot be shrunk to clip content. The dark palette already met WCAG AA contrast (verified ≥ 4.5:1) and the state indicator pairs color with a text label, so no contrast/color-only changes were needed.
- **Changes:** The companion window now honors OS text-size and display-scaling (readable, non-clipped UI on high-DPI / large-text setups) and is announced by screen readers per panel; the window enforces a minimum size. No visual restyle; contrast and colour usage were already compliant.

### DOC-003 — Audit public documents for core-policy compliance
- **Status:** `implemented`
- **Issue:** #48
- **Recorded:** 2026-08-26 16:27
- **Implemented:** 2026-08-26 17:52
- **Problem:** Public-facing documents (README, CHANGELOG, CONTRIBUTING, CODE_OF_CONDUCT, SECURITY, ARCHITECTURE, STRUCTURE, `docs/IMPROVEMENTS.md`, `docs/MEMORY_MAP.md`, `docs/UI_DESIGN_SPEC.md`) may not uniformly meet the five core policies (International English, no hardwrap except LICENSE/standard-formatted docs, LF line endings, atomic-commit discipline). Inconsistent docs risk a poor first public impression.
- **Possible Fix:** Run an audit pass over all public docs against the five core policies; fix deviations (language, hardwrap, line endings, formatting). Exclude release/tagging work — that is handled by a separate workflow.
- **Actual Fix:** Verified: a public-doc audit against the five core policies is outstanding. CRLF/BOM scan of all 10 public docs found no BOM and no CRLF — line-ending compliance is already clean. Remaining audit scope: International English usage, hardwrap (prose line length), and formatting/placeholder hygiene. Fix: run the audit, correct deviations; exclude release/tagging (handled by a separate workflow).
- **Rejection Reason:** `—`
- **Actual Implemented:** Audited all 10 public documents against the five core policies; no content deviations found. Removed the two italic placeholder scaffolding lines from CHANGELOG.md (under Changed/Fixed) now that real entries exist.
- **Changes:** CHANGELOG.md lost its placeholder guidance lines; all 10 public documents otherwise already comply (LF/no BOM, no hardwrap, International English, no placeholders/PII). The tracker records the audit outcome.
