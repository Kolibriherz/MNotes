# MNoteProvider.Common.Abstractions

Transport-neutral shared contracts for the MNoteProvider package family:
DTO interfaces, event contracts, the failure model, and shared error
messages.

> **Pre-release.** Public APIs may change between pre-release versions.

- Target framework: `net10.0`
- Dependencies: none

## Install

```bash
dotnet package add MNoteProvider.Common.Abstractions --prerelease
```

## Contents

- DTO contracts for notes, folders, comments, tags and note-tag assignments
- Event contracts for the note event stream
- `MNotesFailType` and the failure model used across the family

Most consumers install `MNoteProvider.ClientService` instead, which brings
this package in as a dependency.

## License

MNoteProvider packages are licensed under the MIT License. Third-party components are listed in
`THIRD-PARTY-NOTICES.md`.
