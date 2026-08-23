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
- **Status:** `verified`
- **Issue:** `—`
- **Recorded:** 2026-08-22 17:02
- **Implemented:** `—`
- **Problem:** The repository is newly created and its folders, guardrail files, and settings have not yet been confirmed against the workflow standard.
- **Possible Fix:** Run the verification checks from the workflow before the first commit.
- **Actual Fix:** Verification was performed during bootstrap interaction 1 against the Project Bootstrap standard; the violations found then (CHANGELOG placement, hardwrapped documents, CoC placeholder) were fixed and merged via PR #1. Re-checked on 2026-08-22: structure still conforms.
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-002 — Application UI design specification
- **Status:** `verified`
- **Issue:** #8
- **Recorded:** 2026-08-22 19:55
- **Implemented:** `—`
- **Problem:** The application has no defined interface plan yet — without an agreed set of screens, layouts, and a visual theme, feature implementation would be unguided and inconsistent.
- **Possible Fix:** Produce a UI design specification covering the required interfaces (live HUD dashboard, memory monitor, guide browser, settings), the layout of each screen, and the visual theme, reviewed against the data actually available from the memory reading mechanism (ENH-003) before implementation starts.
- **Actual Fix:** Produce the UI design specification after ENH-003, mapping every interface element to gameplay values actually readable from PCSX2 via PINE IPC. Constraint discovered during verification: WinForms on .NET 8 ships no built-in dark mode, so the visual theme needs a custom palette/renderer; the spec must define the screens (HUD dashboard, memory monitor, guide browser, settings), their layouts, and that theme explicitly.
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-003 — PCSX2 connection, game detection & memory reading POC
- **Status:** `verified`
- **Issue:** #9
- **Recorded:** 2026-08-22 19:55
- **Implemented:** `—`
- **Problem:** The core pipeline does not exist yet — the application cannot attach to the PCSX2 process, cannot confirm that the running game is Harvest Moon: Save the Homeland, and cannot read gameplay values from memory.
- **Possible Fix:** Console-app proof of concept using ReadProcessMemory (P/Invoke) against pcsx2.exe: locate the PS2 EE RAM base via a pointer chain (Cheat Engine table style), detect the game by scanning for the disc serial SLUS-20141 at its standard EE RAM location (fallback: PCSX2 window title), then read known addresses for stamina/money/weather and print live values. Priority 1 — every other item depends on this working.
- **Actual Fix:** Console-app proof of concept using the PINE IPC interface built into PCSX2 (TCP 127.0.0.1:28011 on Windows, enabled by the user in Settings): detect the game from the PINE metadata opcodes (serial/title/CRC) instead of memory scanning — correct US serial is SLUS-20251 (ELF SLUS_202.51), confirmed via redump.org, psxdatacenter, GameFAQs; then Read32 gameplay values at EE RAM addresses for stamina/money/weather once located with Cheat Engine. PINE reads EE addresses directly, eliminating fragile base-address discovery; keep ReadProcessMemory+EEmem only as fallback when IPC is disabled. Caveats: IPC must be toggled on per user, requests should stay sequential (queue drops past ~7 in-flight), no bulk-read opcode (~52 ms per 4 KiB). Priority 1 — every other item depends on this working.
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

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
