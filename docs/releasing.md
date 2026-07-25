# Releasing

Operator runbook for publishing the MNoteProvider package family to NuGet.org.
The release pipeline lives in `.github/workflows/package-release.yml`.

## What gets published

Four packages as one release train, all sharing a single version:
`MNoteProvider.Common.Abstractions`, `MNoteProvider.Common`,
`MNoteProvider.ClientService.Abstractions`, `MNoteProvider.ClientService`
(each with a `.snupkg` symbol package).

Version format: `0.1.0-alpha.<run>.d<yyyymmdd>.t<hhmm>` — run number and UTC
commit time are added by the workflow. See [VERSIONING.md](../VERSIONING.md).

## The jobs

| Job | Role |
|---|---|
| `candidate` | Builds, tests, packs and scans. Determines the health state. Runs always. |
| `block_security` | Turns the run red when health is `security-blocked`. |
| `publish_auto` | Publishes when health is `auto-ready`. Environment `nuget-production-auto` (no reviewer). |
| `publish_reviewed` | Publishes when health is `review-required`. Environment `nuget-production-review` — waits for approval. |
| `mark_release` | Tags the commit and creates the GitHub prerelease, only after a successful upload. |
| `summary` | Writes the final tally. Runs always. |

## Health states

| State | Meaning | Result |
|---|---|---|
| `not-required` | No package-relevant change since the last release tag | Nothing is built |
| `validation-failed` | Restore/build/test/pack/smoke failed | No publish |
| `security-blocked` | Scan not performable, or a vulnerability | No publish (red) |
| `review-required` | Something outdated, deprecated or vulnerable | Publish only after manual approval |
| `auto-ready` | All clean | Publishes automatically (if enabled) |

"Package-relevant" = the four `src/` packages, `Directory.Build.*`,
`Directory.Packages.props`, `eng/`, `global.json`, `NuGet.Config`,
`LICENSE.txt`, `docs/nuget/`. Everything else (docs, workflows) never
triggers a release.

## One-time setup

- **Repository variable** `NUGET_PUBLISHING_ENABLED` — the kill switch
  (`true`/`false`). Publishing requires `true`.
- **Environment secret** `NUGET_USER` — the NuGet.org profile name (not the
  e-mail), in *both* environments.
- **Environments** `nuget-production-auto` (no reviewer) and
  `nuget-production-review` (required reviewer = maintainer). Both limited to
  branch `main`.
- **Trusted Publishing policies** on NuGet.org — one per environment,
  workflow file `package-release.yml`.

## Publish a release

1. Ensure the package-relevant change is committed and pushed to `main`.
2. Set `NUGET_PUBLISHING_ENABLED` to `true`.
3. Actions → **Package release** → **Run workflow** → branch `main`,
   `publish = true`.
4. Watch `candidate` → then `publish_auto` (or `publish_reviewed`) → then
   `mark_release`. Approve the review environment if prompted.
5. Set `NUGET_PUBLISHING_ENABLED` back to `false`.

**Dry run:** same, but `publish = false`. Builds and inspects the candidate,
publishes nothing. Note: it does **not** exercise the publish/push path —
those jobs are skipped when `publish` is false.

## After publishing

Check each package page on NuGet.org: version, owner, MIT license, rendered
README, dependencies, symbol package. Confirm the Git tag
(`mnoteprovider-v<version>`) and the GitHub prerelease exist.

## Resuming a partial publish

If a run uploads some but not all four packages (e.g. a network error), simply
**re-run it with the same inputs**. `--skip-duplicate` skips what is already
published and pushes the rest. This re-uploads the *identical* built packages —
it never reuses the version for different content.

## A bad release

A published version is never overwritten. If the published bytes are wrong,
**unlist** the affected packages on NuGet.org and ship a **new** version.

## Evidence

Each run uploads an artifact (`nuget-candidate`, 30 days): the packages,
`SHA256SUMS`, `release-manifest.json`, the scan reports, and — on failure —
build binlogs. Use it to inspect what was built and to answer "why did the
gate decide this?".
