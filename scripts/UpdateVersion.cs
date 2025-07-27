#!/usr/bin/env dotnet-script
#r "nuget: System.CommandLine, 2.0.0-beta4.22272.1"

using System;
using System.CommandLine;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Xml.Linq;

// Create root command
var rootCommand = new RootCommand("Update version in AcmeshWrapper.csproj file");

// Add version argument
var versionArgument = new Argument<string?>(
    name: "version",
    description: "The version to set. If not provided, will attempt to get from current git tag",
    getDefaultValue: () => null);

// Add project path option
var projectOption = new Option<string>(
    new[] { "--project", "-p" },
    getDefaultValue: () => "src/AcmeshWrapper/AcmeshWrapper.csproj",
    description: "Path to the csproj file");

rootCommand.AddArgument(versionArgument);
rootCommand.AddOption(projectOption);

// Set handler
rootCommand.SetHandler(async (string? version, string projectPath) =>
{
    try
    {
        await UpdateVersionAsync(version, projectPath);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
        Environment.Exit(1);
    }
}, versionArgument, projectOption);

// Execute
return await rootCommand.InvokeAsync(args);

// Implementation
async Task UpdateVersionAsync(string? version, string projectPath)
{
    // Get version if not provided
    if (string.IsNullOrEmpty(version))
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("No version specified. Attempting to get from git tag...");
        Console.ResetColor();
        
        version = GetGitTagVersion();
        if (string.IsNullOrEmpty(version))
        {
            throw new InvalidOperationException("No version provided and could not determine version from git tag");
        }
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Using version from git tag: {version}");
        Console.ResetColor();
    }
    
    // Validate version
    if (!IsValidSemanticVersion(version))
    {
        throw new ArgumentException($"Invalid version format: '{version}'. Expected semantic versioning (e.g., 1.2.3, 1.2.3-beta.1)");
    }
    
    // Find project file
    projectPath = ResolveProjectPath(projectPath);
    
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"Updating version in: {projectPath}");
    Console.ResetColor();
    
    // Load and update XML
    var doc = XDocument.Load(projectPath);
    var versionElement = doc.Descendants("Version").FirstOrDefault();
    
    if (versionElement != null)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Current version: {versionElement.Value}");
        Console.ResetColor();
        
        versionElement.Value = version;
    }
    else
    {
        // Add version element if it doesn't exist
        var propertyGroup = doc.Descendants("PropertyGroup").FirstOrDefault();
        if (propertyGroup == null)
        {
            throw new InvalidOperationException("No PropertyGroup found in project file");
        }
        
        propertyGroup.Add(new XElement("Version", version));
    }
    
    // Save with proper formatting
    await using var writer = File.CreateText(projectPath);
    await writer.WriteLineAsync("﻿" + doc.ToString());
    
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Successfully updated version to: {version}");
    Console.ResetColor();
    
    // Verify
    doc = XDocument.Load(projectPath);
    var newVersion = doc.Descendants("Version").FirstOrDefault()?.Value;
    
    if (newVersion == version)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Version verified: {newVersion}");
        Console.ResetColor();
    }
    else
    {
        throw new InvalidOperationException($"Version verification failed. Expected: {version}, Found: {newVersion}");
    }
}

string? GetGitTagVersion()
{
    try
    {
        // Check if we're on a tag
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "describe --exact-match --tags",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        });
        
        if (process != null)
        {
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                var tag = process.StandardOutput.ReadToEnd().Trim();
                // Remove 'v' prefix if present
                return tag.StartsWith("v") ? tag.Substring(1) : tag;
            }
        }
        
        // Try to get latest tag
        process = Process.Start(new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "describe --tags --abbrev=0",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        });
        
        if (process != null)
        {
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                var tag = process.StandardOutput.ReadToEnd().Trim();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Not on a tag. Latest tag is: {tag}");
                Console.ResetColor();
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Failed to get git tag: {ex.Message}");
        Console.ResetColor();
    }
    
    return null;
}

bool IsValidSemanticVersion(string version)
{
    // Semantic versioning regex
    var pattern = @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";
    return Regex.IsMatch(version, pattern);
}

string ResolveProjectPath(string projectPath)
{
    if (File.Exists(projectPath))
    {
        return Path.GetFullPath(projectPath);
    }
    
    // Try from current directory
    var currentDirPath = Path.Combine(Directory.GetCurrentDirectory(), projectPath);
    if (File.Exists(currentDirPath))
    {
        return Path.GetFullPath(currentDirPath);
    }
    
    // Try from script directory
    var scriptDir = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? Directory.GetCurrentDirectory();
    var scriptDirPath = Path.Combine(Path.GetDirectoryName(scriptDir)!, projectPath);
    if (File.Exists(scriptDirPath))
    {
        return Path.GetFullPath(scriptDirPath);
    }
    
    throw new FileNotFoundException($"Project file not found at: {projectPath}");
}