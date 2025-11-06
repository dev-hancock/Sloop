# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [2.1.2] - 2025-11-06

### Added

- Added an overload of the AddCache extension that accepts an Action<IServiceProvider, SloopOptions>,
  allowing the options configuration delegate to resolve services from the container when configuring
  SloopOptions (e.g. to obtain a connection factory or other dependencies).

---

## [2.1.1] - 2025-07-23

### Fixed

- Fixed issue where null values were throwing exceptions with logging when stored in the cache.
- Added tests to encapsulate null value handling in cache operations.

---

## [2.1.0] - 2025-07-23

### Added

- Structured logging across the library via `Microsoft.Extensions.Logging` ([LoggerMessage] extensions).

### Changed

- Support for storing and retrieving `null` values as cache items.

---

## [2.0.0] - 2025-07-21

### Added

- `IDbCacheMigrator` service and ability to run migrations manually via `IDbCacheContext.MigrateAsync`
- `SloopOptions.UseConnectionString(...)` and `UseDataSource(...)` methods to configure and consume an
  `NpgsqlDataSource`

### Changed

- Simplified schema qualification logic in `SloopOptions.GetQualifiedTableName()` now consistently returns a
  fully-qualified, quoted identifier

### Fixed

- Switched options validation to use `.Validate(x => x.Validate())` in DI for immediate startup-time checks

---

## [1.0.3] - 2025-05-29

### Fixed

- Updated README to use absolute image URL for logo rendering on nuget.org

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
