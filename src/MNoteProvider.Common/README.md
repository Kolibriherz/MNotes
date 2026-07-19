# MNoteProvider.Common

Concrete data transfer records and shared route definitions used by the
MNoteProvider server and client.

> **Pre-release.** Public APIs may change between pre-release versions.

- Target framework: `net10.0`
- Dependencies: `MNoteProvider.Common.Abstractions`

## Install

```bash
dotnet package add MNoteProvider.Common --prerelease
```

## Contents

- DTO records for notes, folders, comments, tags and note-tag assignments
- `MNotesRoutes` — the route constants shared by server and client, so both
  sides refer to the same definitions instead of duplicated strings

Most consumers install `MNoteProvider.ClientService` instead, which brings
this package in as a dependency.

## License

MNoteProvider packages are licensed under the MIT License.. Third-party components are listed in
`THIRD-PARTY-NOTICES.md`.
