# Changelog

Notable changes to Tiny11 GUI are documented here. The project follows semantic versioning for published releases.

## Unreleased

## 1.2.3 — 2026-09-18

### Fixed

- Query Hyper-V availability in the selected image before removal. An absent feature (`0x800F080C`, common on Windows Home) is reported and skipped; other query/removal errors retain DISM diagnostics. Removal exit code 3010 is accepted.
- OneDrive setup deletion handles leaf reparse files without following their targets and retries protected files with file-specific ownership, permissions, and attribute changes. Failures include the exact path and native diagnostics; directories and parent reparse points are rejected.
- Protected OneDrive files use `takeown` when `icacls /setowner` cannot take ownership from TrustedInstaller. This fallback is restricted to regular files and verified WIM/WOF reparse tags. Windows PowerShell native stderr no longer bypasses exit-code handling in this step.
- AppX inventory now uses the selected DISM executable (including a newer ADK) instead of the host PowerShell DISM module. Inventory failures include native output.
- Unload offline registry hives before DISM deep cleanup to avoid sharing violations (exit code 32).
- Clear the copied answer file's read-only attribute before saving OOBE settings; honor XML encoding and reject invalid roots or namespaces with a clear error.
- CORE package removal reports `0x800F0805` as a warning and continues without claiming the package was removed. Other removal failures remain fatal, with DISM output included; reboot-required success (3010) is accepted for this operation.
- Added PowerShell regression tests for servicing order, read-only Chinese XML, malformed XML, and CORE exit-code handling.

### Added

- Minimal x64 OOBE answer file at `docs/autounattend.xml` and troubleshooting guidance in `SUPPORT.md`.

### Changed

- Display the informational version, including prerelease identifiers, so test builds can be distinguished in the UI.
- Clarify that compatibility with trimmed Windows hosts is not guaranteed, even with a newer ADK.

### Validation and known limitations

- 60 automated tests cover generated scripts, native error handling, XML, and localization.
- Protected OneDrive setup deletion was verified on a disposable copy of an official English Windows 11 WIM. A stock Windows 11 VM passed AppX removal, OneDrive removal, offline registry operations, and the absent-Hyper-V check and reached component-store cleanup.
- Complete WIM/ESD build and Windows installation/OOBE validation remain pending.
- The tested Tiny10 host still fails DISM initialization with error 87 and ADK DISM 10.0.26100.8972. The responsible host component has not been identified.

## 1.2.2 — 2026-09-17

### Fixed

- Startup registry cleanup skips absent offline hives instead of aborting the build in Windows PowerShell. Failures unloading existing hives still stop the build with native error details.
- Build failure logs now include the failing script location, error ID, and script stack trace.
- Updated application version metadata to match the release.

### Tests

- Added Windows PowerShell regression coverage for absent hives, successful unloads, native failures, and preserving the caller's error preference.

## 1.2.0 — 2026-09-06

### Added

- Russian, Japanese, German, French, Spanish, and Simplified Chinese UI resources with English fallback.
- Ownership-based build state and recovery for interrupted Tiny11 GUI runs.
- Automated tests for generated PowerShell safety, path escaping, ESD index handling, atomic output, native exit-code checks, and PowerShell syntax.
- GitHub Actions workflows for continuous integration and tagged, checksummed Windows release packages.
- Contribution, security, support, issue, pull-request, and release documentation.

### Changed

- Critical DISM, registry, compression, and ISO-generation operations now fail the build when their native exit code indicates an error.
- Generated scripts now use structured `try/catch/finally` cleanup limited to resources owned by the current build.
- ESD exports reset the destination WIM image index to `1` before servicing it.
- Final ISOs are built to a unique temporary path and atomically published only after validation.

### Removed

- Automatic termination of unrelated PowerShell processes.
- Global mounted-image discard and global DISM cleanup during normal build startup.

## 1.1.1

- Added custom `autounattend.xml` selection.
- Improved cleanup of stale Tiny11 GUI working directories.
- Redesigned the log panel layout.

## 1.1.0

- Added configurable deep-cleanup options and improved DISM compatibility.

## 1.0.0

- Initial public release.
