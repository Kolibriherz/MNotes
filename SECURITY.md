# Security Policy

## Supported versions

Until the first package release, security fixes are applied to `main`.

During the alpha phase, only the latest published pre-release is supported.

## Reporting a vulnerability

Use **Report a vulnerability** in this repository's GitHub Security tab.

Do not report suspected vulnerabilities in public issues or discussions.

Include the affected version or commit, reproduction steps, expected
impact, and any known mitigation.

## Current security boundary

Authentication and authorization are not implemented.

Do not expose the HTTP API or SignalR hub to untrusted networks or the
public internet.

## Supply-chain and release security

- Dependency vulnerabilities are surfaced by GitHub Dependabot alerts and by
  NuGetAudit during restore; the release pipeline blocks on unresolved findings.
- Dependencies are pinned via `packages.lock.json`; CI restores in locked mode.
- Packages are published from GitHub Actions via NuGet Trusted Publishing
  (OIDC). No long-lived API key exists.
