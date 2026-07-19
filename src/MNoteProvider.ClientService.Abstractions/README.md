# MNoteProvider.ClientService.Abstractions

Client-facing contracts for the MNoteProvider API client:
`IMNoteClientService` and `INoteEventRelay`.

> **Pre-release.** Public APIs may change between pre-release versions.

- Target framework: `net10.0`
- Dependencies: `MNoteProvider.Common`, `OneOf`

## Install

```bash
dotnet package add MNoteProvider.ClientService.Abstractions --prerelease
```

## Contents

- `IMNoteClientService` — typed operations for notes, folders, comments,
  tags, note-tag assignments and note history. Operations return
  `OneOf<TResult, MNoteProcessFail>`, so expected failures are part of the
  signature instead of exceptions.
- `INoteEventRelay` — client-side notifications for note creation, updates
  and deletion.

Install this package to depend on the public client contracts without
referencing the concrete client implementation.

## License

MNoteProvider packages are licensed under the MIT License. Third-party components are listed in
`THIRD-PARTY-NOTICES.md`.
