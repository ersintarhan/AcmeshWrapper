# Version Update Scripts

This directory contains scripts to update the version in the AcmeshWrapper.csproj file. These scripts are used by the GitHub Actions workflow and can also be used locally.

## Available Scripts

### PowerShell Script (Cross-platform)
`Update-Version.ps1` - Works on Windows, Linux, and macOS with PowerShell Core

```powershell
# Update to specific version
./Update-Version.ps1 -Version 1.2.3

# Get version from current git tag
./Update-Version.ps1

# Update a different project file
./Update-Version.ps1 -Version 1.2.3 -ProjectPath path/to/project.csproj

# Get help
./Update-Version.ps1 -?
```

### Bash Script (Linux/macOS)
`update-version.sh` - Native shell script for Unix-like systems

```bash
# Update to specific version
./update-version.sh 1.2.3

# Get version from current git tag
./update-version.sh

# Update a different project file
./update-version.sh -p path/to/project.csproj 1.2.3

# Get help
./update-version.sh --help
```

### C# Script (Cross-platform)
`UpdateVersion.cs` - Can be run with dotnet-script

First, install dotnet-script globally:
```bash
dotnet tool install -g dotnet-script
```

Then run:
```bash
# Update to specific version
dotnet script UpdateVersion.cs 1.2.3

# Get version from current git tag
dotnet script UpdateVersion.cs

# Update a different project file
dotnet script UpdateVersion.cs 1.2.3 --project path/to/project.csproj

# Get help
dotnet script UpdateVersion.cs --help
```

## Version Format

All scripts validate that the version follows [Semantic Versioning](https://semver.org/):
- Format: `MAJOR.MINOR.PATCH[-PRERELEASE][+BUILD]`
- Examples:
  - `1.2.3`
  - `1.2.3-beta.1`
  - `1.2.3-rc.1+build.123`

## Git Tag Integration

When no version is specified, the scripts will:
1. Check if the current commit has a tag
2. If yes, use that tag's version (removing 'v' prefix if present)
3. If no, show the latest tag for reference and exit with error

This is particularly useful in CI/CD pipelines where the version should match the git tag.

## Usage in GitHub Actions

The GitHub Actions workflow uses these scripts to update the version before building:

```yaml
- name: Update project version
  run: |
    sed -i "s/<Version>.*<\/Version>/<Version>${{ steps.version.outputs.VERSION }}<\/Version>/" "$PROJECT_FILE"
```

## Local Development

For local development, you can:
1. Create a tag: `git tag v1.2.3`
2. Push the tag: `git push origin v1.2.3`
3. The GitHub Actions workflow will automatically build and publish to NuGet

Or manually update the version:
```bash
# Update version locally
./scripts/update-version.sh 1.2.3-beta.1

# Build and pack
dotnet build -c Release
dotnet pack -c Release
```