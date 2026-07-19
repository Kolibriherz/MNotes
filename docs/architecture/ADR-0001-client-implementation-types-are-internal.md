# ADR-0001: Client implementation types are internal

- Status: Accepted
- Date: 2026-07-19

## Context

The client library exposes interfaces and a dependency-injection
registration method. Concrete HTTP and SignalR implementation types
do not need to be constructed directly by consumers.

Public implementation types would unnecessarily increase the supported
API surface and restrict future internal refactoring.

## Decision

Concrete client implementation types remain internal. Consumers use
`IMNoteClientService`, `INoteEventRelay`, and the
`AddMNoteClientService` registration method.

## Alternatives considered

Exposing concrete implementation types publicly was rejected because
consumers do not need to construct or replace them directly.

## Consequences

Internal implementations may change without expanding the public API.

Tests access internal implementations through `InternalsVisibleTo`.

Consumers depend on abstractions and dependency-injection registration
instead of concrete client types.