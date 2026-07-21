# Contributing

MNotes is currently maintained by a single developer. These notes describe
the expected workflow for changes to this repository.

## Requirements

- The .NET SDK version specified in `global.json`
- PostgreSQL 14 or later for running the provider locally

## Local verification

Run before every commit:

```bash
dotnet restore MNotes.sln
dotnet format MNotes.sln --verify-no-changes --no-restore
dotnet build MNotes.sln --configuration Release --no-restore
dotnet test MNotes.sln --configuration Release --no-build
```

MSBuild determines project build order from the declared
`ProjectReference` graph.

## Changes

- Keep commits atomic: one logical change per commit, build and tests green.
  Trivial incidental fixes — a typo, a wording clarification, a redundant line
  removed — may ride along in a commit that already touches the file. They do
  not deserve a commit of their own.
- Use short English commit messages with a conventional prefix
  (`feat`, `fix`, `refactor`, `test`, `docs`, `build`, `ci`, `style`, `chore`).
- Public API changes require a `CHANGELOG.md` entry.
- New dependencies require a license check. Update
`THIRD-PARTY-NOTICES.md` and, for public package runtime dependencies,
`docs/nuget/THIRD-PARTY-NOTICES.md`.

## Not accepted

- Secrets, credentials, or connection strings in tracked files
