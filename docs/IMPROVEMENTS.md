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
- **Status:** `recorded`
- **Issue:** `—`
- **Recorded:** 2026-08-22 17:02
- **Implemented:** `—`
- **Problem:** The repository is newly created and its folders, guardrail files, and settings have not yet been confirmed against the workflow standard.
- **Possible Fix:** Run the verification checks from the workflow before the first commit.
- **Actual Fix:** `—`
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-002 — Application UI design specification
- **Status:** `recorded`
- **Issue:** `—`
- **Recorded:** 2026-08-22 19:55
- **Implemented:** `—`
- **Problem:** The application has no defined interface plan yet — without an agreed set of screens, layouts, and a visual theme, feature implementation would be unguided and inconsistent.
- **Possible Fix:** Produce a UI design specification covering the required interfaces (live HUD dashboard, memory monitor, guide browser, settings), the layout of each screen, and the visual theme, reviewed against the data actually available from the memory reading mechanism (ENH-003) before implementation starts.
- **Actual Fix:** `—`
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`

### ENH-003 — PCSX2 connection, game detection & memory reading POC
- **Status:** `recorded`
- **Issue:** `—`
- **Recorded:** 2026-08-22 19:55
- **Implemented:** `—`
- **Problem:** The core pipeline does not exist yet — the application cannot attach to the PCSX2 process, cannot confirm that the running game is Harvest Moon: Save the Homeland, and cannot read gameplay values from memory.
- **Possible Fix:** Console-app proof of concept using ReadProcessMemory (P/Invoke) against pcsx2.exe: locate the PS2 EE RAM base via a pointer chain (Cheat Engine table style), detect the game by scanning for the disc serial SLUS-20141 at its standard EE RAM location (fallback: PCSX2 window title), then read known addresses for stamina/money/weather and print live values. Priority 1 — every other item depends on this working.
- **Actual Fix:** `—`
- **Rejection Reason:** `—`
- **Actual Implemented:** `—`
- **Changes:** `—`