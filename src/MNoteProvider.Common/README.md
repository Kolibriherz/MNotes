# MNoteProvider.Common

Concrete data transfer records, the shared route table, and the failure
type used by the MNoteProvider client family.

> **Pre-release.** Public APIs may change between pre-release versions.

- Target framework: `net10.0`
- Dependencies: `MNoteProvider.Common.Abstractions`

## Install

```bash
dotnet package add MNoteProvider.Common
```

## Contents

- DTO records for notes, folders, comments, tags and note-tag assignments
- `MNotesRoutes` — the route constants shared by server and client, so both
  sides refer to the same definitions instead of duplicated strings
- `MNoteProcessFail` — the failure result returned by client operations

Most consumers install `MNoteProvider.ClientService` instead, which brings
this package in as a dependency.

## License

MIT — see `LICENSE.txt`. Third-party components are listed in
`THIRD-PARTY-NOTICES.md`.
