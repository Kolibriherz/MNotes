# Versioning

MNotes follows Semantic Versioning.

## Package family

The following packages are versioned and released together:

- `MNoteProvider.Common.Abstractions`
- `MNoteProvider.Common`
- `MNoteProvider.ClientService.Abstractions`
- `MNoteProvider.ClientService`

All four packages use the same version. Consumers should use matching
versions of the package family.

## Before 1.0.0

Releases are published as pre-release versions.

Public APIs and package boundaries may change between pre-releases.
Notable changes are recorded in `CHANGELOG.md`.

## Pre-release format

Pre-release versions follow this pattern:

```
0.1.0-alpha.42.d20260716.t0905
│     │     │  │          └── commit time (UTC, hour and minute)
│     │     │  └───────────── commit date (UTC)
│     │     └──────────────── build run number
│     └────────────────────── maturity label
└──────────────────────────── semantic version core
```

The version core and the maturity label are set deliberately by the
maintainer. Run number and commit timestamp are added by the build, so
every release is uniquely identifiable and traceable to a single commit.

From 1.0.0 on, the timestamp moves to build metadata (`+sha.<commit>`),
because package feeds ignore build metadata when comparing versions.

## From 1.0.0

- **MAJOR**: breaking public API or package-boundary changes.
- **MINOR**: backward-compatible features.
- **PATCH**: backward-compatible fixes.

## Immutability

A published version is never reused.

A defective release is unlisted and replaced with a new version.
