# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.2] - 2025-05-29

### Added

- Added `ConnectionFactory` option to `SloopOptions` for customizing how `NpgsqlConnection` instances are created.

---

## [1.0.1] - 2025-05-27

### Added

- Made background cleanup interval configurable via `SloopOptions.CleanupInterval`.
- Introduced startup validation for `SloopOptions`. Misconfigured values now throw `OptionsValidationException`.

### Fixed

- `SloopOptions` properties are now publicly settable, ensuring compatibility with configuration binding.

---

## [1.0.0] - 2025-05-23

### Added

- Initial release of Sloop — a PostgreSQL-backed `IDistributedCache` implementation.
- Supports absolute and sliding expiration.
- Includes background cleanup service, command-based architecture, and integration test coverage.
