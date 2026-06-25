using System;

namespace Il2CppMetadataScanner;

/// <summary>
/// Severity levels for discovered vulnerabilities.
/// </summary>
public enum Severity
{
    Critical,
    High,
    Medium,
    Low,
    Info
}

/// <summary>
/// Category of the detected security issue.
/// </summary>
public enum FindingCategory
{
    HardcodedCredential,
    ApiEndpoint,
    DebugCommand,
    ApiKey,
    InternalIpAddress,
    DatabaseConnectionString,
    SuspiciousConfiguration,
    Other
}

/// <summary>
/// Represents a single security finding discovered during metadata string scanning.
/// </summary>
public sealed class SecurityFinding
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public Severity Severity { get; set; }
    public FindingCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MatchedValue { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public int Offset { get; set; }
    public string? Context { get; set; }
    public string? Remediation { get; set; }
    public string SourceFile { get; set; } = string.Empty;
}

/// <summary>
/// High-level summary metrics for report generation.
/// </summary>
public sealed class ScanSummary
{
    public int TotalStrings { get; init; }
    public int ScannedStrings { get; init; }
    public int FindingsCount { get; init; }
    public int CriticalCount { get; init; }
    public int HighCount { get; init; }
    public int MediumCount { get; init; }
    public int LowCount { get; init; }
    public int InfoCount { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset FinishedAt { get; init; }
    public TimeSpan Duration => FinishedAt - StartedAt;
    public string MetadataPath { get; init; } = string.Empty;
    public string? UnityVersion { get; init; }
    public string? ScannerVersion { get; init; } = "1.0.0";
}
