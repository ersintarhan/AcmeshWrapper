# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2025-01-27

### Added
- .NET Standard 2.1 support for broader compatibility
- Multi-targeting support (netstandard2.1 and net9.0)

### Changed
- Updated version to 1.1.0
- Enhanced README with Quick Start section

## [1.0.0] - 2025-01-27

### Added
- Initial release of AcmeshWrapper
- Full support for acme.sh commands:
  - `ListAsync` - List all certificates
  - `IssueAsync` - Issue new certificates
  - `RenewAsync` - Renew specific certificates
  - `RenewAllAsync` - Renew all certificates
  - `InstallCertAsync` - Install certificates
  - `RevokeAsync` - Revoke certificates
  - `RemoveAsync` - Remove certificates
  - `InfoAsync` - Get certificate information
  - `GetCertificateAsync` - Retrieve certificate contents
- Comprehensive MSTest unit tests (69 tests)
- Interactive TestConsole application
- Type-safe options and results
- ProcessX integration for async process execution
- Support for multiple CA providers (Let's Encrypt, ZeroSSL, Buypass)
- ECC certificate support

[1.1.0]: https://github.com/ersintarhan/AcmeshWrapper/releases/tag/v1.1.0
[1.0.0]: https://github.com/ersintarhan/AcmeshWrapper/releases/tag/v1.0.0