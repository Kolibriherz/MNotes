# Versioning

MNotes follows Semantic Versioning 2.0.0.

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

```text
0.1.0-alpha.42.d20260716.t0905
│     │     │  │          └── commit time UTC
│     │     │  └───────────── commit date UTC
│     │     └──────────────── workflow run number
│     └────────────────────── maturity label
└──────────────────────────── semantic version core
```

The version core and the maturity label are set deliberately by the
maintainer.

The package release workflow adds its run number and the triggering
commit timestamp in UTC. Each new run of that workflow produces a
distinct package version. Re-running the same workflow run intentionally
reproduces the same version.

From `1.0.0` onward, package versions use the SemVer core without
workflow-specific suffixes. Source traceability is recorded through NuGet
repository metadata, including `RepositoryCommit`.

## From 1.0.0

- **MAJOR**: breaking public API or package-boundary changes.
- **MINOR**: backward-compatible features.
- **PATCH**: backward-compatible fixes.

## Immutability

A published version is never republished with different content.

A workflow run may be safely re-run to finish a partial publish: packages
already uploaded are skipped (`--skip-duplicate`) and only the missing ones
are pushed. This re-uploads the identical built packages — it does not reuse
the version for new content.

A defective release is never overwritten. It is unlisted and replaced with
a new version.
