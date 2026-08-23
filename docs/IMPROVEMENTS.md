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
- **Status:** `recorded`
- **Issue:** `—`
- **Recorded:** 2026-08-23 18:59
- **Implemented:** `—`
- **Problem:** The required `build` check only validates markdown hygiene (CRLF/BOM). Since PR #14 the repository contains a real .NET application, but no automation compiles it - a broken commit can merge with green checks.
- **Possible Fix:** Extend ci.yml so the build job also runs `dotnet build` (and a formatting verification) on the solution, making the required check actually gate application code.
- **Actual Fix:** `—`
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### DOC-001 — README has no getting-started instructions
- **Status:** `recorded`
- **Issue:** `—`
- **Recorded:** 2026-08-23 18:59
- **Implemented:** `—`
- **Problem:** The README is marketing-style (introduction and badges only). There is no getting-started section: prerequisites (.NET 8 SDK, PCSX2 with PINE IPC enabled), build/run commands, or expected output. Anyone wanting to try the proof of concept must read the source to figure out how.
- **Possible Fix:** Add a Getting Started section to README.md covering prerequisites, clone/build/run commands for src/HmSth.Poc, and what output to expect.
- **Actual Fix:** `—`
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-006 — Integrate reverse-engineered gameplay memory locations
- **Status:** `recorded`
- **Issue:** `—`
- **Recorded:** 2026-08-23 18:59
- **Implemented:** `—`
- **Problem:** The UI design spec renders stamina/money/weather as `--` because gameplay addresses were unknown, but valid Cheat Engine locations against running pcsx2-qt.exe have since been found (base `"pcsx2-qt.exe"+0317C238`): GOLD at offset `864`, STAMINA at `830`, TIME at `5F32F4`. Documented formats: STAMINA is 4 bytes `[maxFatigue, fatigue, maxStamina, stamina]` where max values normally match and shift with Power Berry count (e.g. `8C 00 8C 8C`; normal activity costs -2 stamina, rain -4 and raises fatigue; stamina 0 blocks activities, fatigue at max when YY=XX); TIME is `[season, day, hour, minute]` (e.g. `00 07 06 00`). Weather remains unfound.
- **Possible Fix:** Document this memory map in its own doc, then add a reading layer that maps the three values into the app. Open items to resolve during implementation: the CE pointers live in host-process address space while PINE reads EE addresses, so choose between direct ReadProcessMemory on the host addresses (the already-planned fallback path) or deriving equivalent EE addresses for PINE; confirm the offset scale anomaly of TIME (`5F32F4` vs short offsets `864`/`830`); locate the weather address.
- **Actual Fix:** `—`
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-007 — PINE framing logic has zero tests
- **Status:** `recorded`
- **Issue:** `—`
- **Recorded:** 2026-08-23 18:59
- **Implemented:** `—`
- **Problem:** The POC's protocol logic (Request/ReadString/ReadU32: packet framing, result-code handling, string parsing) is pure and testable without an emulator, but no tests exist; this code will be carried into the real client unchanged.
- **Possible Fix:** Add a small xUnit project covering framing and parsing edge cases (short reads, non-zero result codes, malformed string payloads), so the tests travel with the code into the real client.
- **Actual Fix:** `—`
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`
