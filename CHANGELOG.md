# Changelog

## [Unreleased]

### Added

- Initial repository structure.
- Ignore local AI tooling directories. (#10)
- Add PCSX2 PINE IPC proof of concept console app. (#9)
- Add UI design specification. (#8)
- Build the .NET solution in CI. (#17)
- Add PINE IPC framing tests. (#19)
- Add getting-started instructions for the POC. (#16)
- Document verified EE memory map anchor for TIME and candidate addresses. (#20)
- Add gameplay memory reading layer (Gold, Stamina, Time) via PINE IPC. (#18)
- Correct gameplay memory read addresses (GOLD/STAMINA/TIME) via verified CE→EE translation. (#41)

### Changed

- _Behaviour changes, including any migration notes._
- Remove `[PINE DEBUG]` logging left in `PineClient` from the BUG-002 investigation.

### Fixed

- _Bug fixes, one line each, referencing the issue if applicable._
- Fix PINE IPC read timeout handling: graceful error message and exit code 3 when PCSX2 connected but unresponsive. (#33)
- Fix PINE IPC protocol mismatch and read timeout: 4-byte size header (u32 LE) + separate 15s/5s timeouts for metadata/gameplay reads. (#36)

[Unreleased]: https://github.com/ffooll-bit/hm-sth-companion/compare/v0.1.0...HEAD
