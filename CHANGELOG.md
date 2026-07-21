# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added

- Typed HTTP client operations for notes, folders, comments, tags,
  note-tag assignments, and note history.
- Real-time notifications for note creation, updates, and deletion over
  SignalR.
- Append-only update-event stream for note history.

### Security
- ⚠️ Authentication and authorization are not yet implemented – do not expose to untrusted networks
