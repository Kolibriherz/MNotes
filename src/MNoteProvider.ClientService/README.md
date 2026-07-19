# MNoteProvider.ClientService

Typed HTTP and SignalR client for an MNoteProvider backend, with
dependency-injection registration.

> **Pre-release.** Public APIs may change between pre-release versions.

> **Security.** The MNoteProvider backend does not implement authentication
> or authorization. Do not use this client against a backend exposed to
> untrusted networks.

- Target framework: `net10.0`
- Dependencies: `MNoteProvider.ClientService.Abstractions`,
  `Microsoft.AspNetCore.SignalR.Client`, `Microsoft.Extensions.Http`,
  `Microsoft.Extensions.Hosting.Abstractions`,
  `Microsoft.Extensions.DependencyInjection.Abstractions`,
  `Microsoft.Extensions.Logging.Abstractions`

## Install

```bash
dotnet package add MNoteProvider.ClientService
```

## Usage

```csharp
services.AddMNoteClientService(new Uri("https://localhost:6015/"));
```

Registration takes the base address of the provider; route constants are
combined relative to it. Implementation types are internal — consumers
depend on the interfaces and the registration method only.

Inject `IMNoteClientService` for HTTP operations and `INoteEventRelay` for
real-time note notifications. The SignalR connection is started by a hosted
service during application startup.

## License

MIT — see `LICENSE.txt`. Third-party components are listed in
`THIRD-PARTY-NOTICES.md`.
