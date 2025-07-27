#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Updates the version in AcmeshWrapper.csproj file.

.DESCRIPTION
    This script updates the version number in the project file. It can extract the version
    from a git tag or accept a version as a parameter.

.PARAMETER Version
    The version to set. If not provided, will attempt to get from current git tag.

.PARAMETER ProjectPath
    Path to the csproj file. Defaults to src/AcmeshWrapper/AcmeshWrapper.csproj

.EXAMPLE
    .\Update-Version.ps1 -Version 1.2.3
    Updates the version to 1.2.3

.EXAMPLE
    .\Update-Version.ps1
    Gets version from current git tag (if on a tag)
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$Version,
    
    [Parameter()]
    [string]$ProjectPath = "src/AcmeshWrapper/AcmeshWrapper.csproj"
)

$ErrorActionPreference = 'Stop'

function Test-SemanticVersion {
    param([string]$Version)
    
    # Semantic versioning regex pattern
    $pattern = '^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$'
    
    return $Version -match $pattern
}

function Get-GitTagVersion {
    try {
        # Check if we're on a tag
        $currentTag = git describe --exact-match --tags 2>$null
        
        if ($currentTag) {
            # Remove 'v' prefix if present
            $version = $currentTag -replace '^v', ''
            return $version
        }
        
        # If not on a tag, get the latest tag
        $latestTag = git describe --tags --abbrev=0 2>$null
        
        if ($latestTag) {
            Write-Warning "Not on a tag. Latest tag is: $latestTag"
            return $null
        }
        
        Write-Warning "No git tags found in repository"
        return $null
    }
    catch {
        Write-Warning "Failed to get git tag: $_"
        return $null
    }
}

# Main script
try {
    # Get version if not provided
    if (-not $Version) {
        Write-Host "No version specified. Attempting to get from git tag..." -ForegroundColor Cyan
        $Version = Get-GitTagVersion
        
        if (-not $Version) {
            throw "No version provided and could not determine version from git tag"
        }
        
        Write-Host "Using version from git tag: $Version" -ForegroundColor Green
    }
    
    # Validate version format
    if (-not (Test-SemanticVersion -Version $Version)) {
        throw "Invalid version format: '$Version'. Expected semantic versioning (e.g., 1.2.3, 1.2.3-beta.1)"
    }
    
    # Resolve project path
    $resolvedPath = Resolve-Path $ProjectPath -ErrorAction SilentlyContinue
    
    if (-not $resolvedPath) {
        # Try from script root
        $scriptRoot = Split-Path -Parent $PSScriptRoot
        $altPath = Join-Path $scriptRoot $ProjectPath
        $resolvedPath = Resolve-Path $altPath -ErrorAction SilentlyContinue
        
        if (-not $resolvedPath) {
            throw "Project file not found at: $ProjectPath or $altPath"
        }
    }
    
    $ProjectPath = $resolvedPath.Path
    Write-Host "Updating version in: $ProjectPath" -ForegroundColor Cyan
    
    # Read the project file
    $content = Get-Content $ProjectPath -Raw
    
    # Check current version
    if ($content -match '<Version>([^<]+)</Version>') {
        $currentVersion = $Matches[1]
        Write-Host "Current version: $currentVersion" -ForegroundColor Yellow
    }
    
    # Update version
    $updatedContent = $content -replace '<Version>[^<]+</Version>', "<Version>$Version</Version>"
    
    # Save the file
    Set-Content -Path $ProjectPath -Value $updatedContent -NoNewline
    
    Write-Host "Successfully updated version to: $Version" -ForegroundColor Green
    
    # Verify the update
    $verifyContent = Get-Content $ProjectPath -Raw
    if ($verifyContent -match '<Version>([^<]+)</Version>') {
        $newVersion = $Matches[1]
        if ($newVersion -eq $Version) {
            Write-Host "Version verified: $newVersion" -ForegroundColor Green
        }
        else {
            throw "Version verification failed. Expected: $Version, Found: $newVersion"
        }
    }
}
catch {
    Write-Error "Failed to update version: $_"
    exit 1
}