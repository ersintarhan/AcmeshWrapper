# Version Management Guide for AcmeshWrapper

## Automated Version Control Strategy

### 1. Git Tag-Based Releases

The recommended approach is to use git tags for version management:

```bash
# Create a new version tag
git tag v1.2.0 -m "Release version 1.2.0"
git push origin v1.2.0
```

This will automatically:
- Update the version in `.csproj` file
- Build and test the project
- Create NuGet package
- Publish to NuGet.org
- Create GitHub release

### 2. Manual Version Updates

For local development, use the provided scripts:

```bash
# PowerShell (cross-platform)
./scripts/Update-Version.ps1 -Version 1.2.0

# Bash (Linux/macOS)
./scripts/update-version.sh 1.2.0

# From git tag
./scripts/update-version.sh --from-tag
```

### 3. Version Numbering Guidelines

Follow Semantic Versioning (SemVer):

| Change Type | Version Part | Example | When to Use |
|------------|--------------|---------|-------------|
| Major | X.0.0 | 1.0.0 → 2.0.0 | Breaking API changes |
| Minor | 0.X.0 | 1.2.0 → 1.3.0 | New features (backward compatible) |
| Patch | 0.0.X | 1.2.3 → 1.2.4 | Bug fixes |

### 4. Pre-release Versions

For beta/preview releases:
```bash
git tag v2.0.0-beta.1
git tag v2.0.0-rc.1
```

### 5. Release Process

1. **Prepare Release**
   ```bash
   # Update CHANGELOG.md
   # Commit all changes
   git add .
   git commit -m "Prepare release v1.2.0"
   ```

2. **Create Tag**
   ```bash
   git tag v1.2.0 -m "Release v1.2.0: Add new features"
   git push origin v1.2.0
   ```

3. **Monitor GitHub Actions**
   - Check Actions tab for build status
   - Verify NuGet package publication
   - Review created GitHub release

### 6. Version Conflict Prevention

- Always pull latest changes before tagging
- Use `git tag -l` to check existing tags
- Never reuse or modify existing tags
- If a release fails, increment patch version

### 7. Rollback Strategy

If a release has issues:
1. Create a new patch version with fixes
2. Never delete published NuGet packages
3. Mark problematic versions as deprecated on NuGet.org

### 8. Local Testing

Before creating a release tag:
```bash
# Update version locally
./scripts/Update-Version.ps1 -Version 1.2.0

# Build and test
dotnet build -c Release
dotnet test
dotnet pack -c Release

# If everything passes, create tag
git tag v1.2.0
git push origin v1.2.0
```