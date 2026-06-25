using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Il2CppMetadataScanner;

/// <summary>
/// Performs static regex-based security scanning on extracted metadata strings.
///
/// This scanner analyzes in-memory strings only. It does not execute any code
/// from the target application and does not perform network requests or system
/// modifications. Findings are intended for review by human analysts before
/// disclosure.
/// </summary>
public sealed class MetadataStringScanner
{
    private static readonly IReadOnlyList<ScannerRule> Rules = new List<ScannerRule>
    {
        new ScannerRule(
            "HardcodedApiEndpoint",
            FindingCategory.ApiEndpoint,
            Severity.High,
            "Potential hardcoded API endpoint",
            "A string literal resembles an API endpoint that references test, staging, dev, or local environments.",
            "Move endpoint URLs to runtime configuration. Do not ship hardcoded internal URLs.",
            @"\bhttps?://[^""'\s]+(?:api|test|staging|dev|localhost)[^""'\s]*\b",
            12,
            false),

        new ScannerRule(
            "HardcodedCredential",
            FindingCategory.HardcodedCredential,
            Severity.Critical,
            "Potential hardcoded credential",
            "A string literal resembles a hardcoded credential or authorization token.",
            "Remove secrets from build artifacts. Use secure credential storage and environment-specific configuration.",
            @"(?:password|passwd|pwd|secret|token|bearer)\s*[=:]\s*[^""'\s]{4,}",
            8,
            true),

        new ScannerRule(
            "DebugCommand",
            FindingCategory.DebugCommand,
            Severity.Medium,
            "Potential debug/admin command string",
            "A string literal resembles debug, GM, or admin command inputs.",
            "Strip debug command handlers from release builds unless intentionally exposed.",
            @"(?:\/|\#|!)[a-zA-Z0-9_\-]{1,64}(?:\s+[^""'\s]{0,64})?",
            3,
            false),

        new ScannerRule(
            "PotentialApiKey",
            FindingCategory.ApiKey,
            Severity.High,
            "Potential API key or token",
            "A long alphanumeric token-like string was found that may be an API key.",
            "Rotate exposed keys and enforce shorter-lived tokens. Avoid embedding them in static metadata.",
            @"\b[a-zA-Z0-9\-_]{32,64}\b",
            32,
            false),

        new ScannerRule(
            "InternalIpAddress",
            FindingCategory.InternalIpAddress,
            Severity.Medium,
            "Potential internal IP address",
            "A string literal resembles a private/internal IP address.",
            "Avoid shipping internal addresses in metadata. Use runtime service discovery when possible.",
            @"\b(?:(?:10|127)\.\d{1,3}\.\d{1,3}\.\d{1,3}|192\.168\.\d{1,3}\.\d{1,3}|172\.(?:1[6-9]|2\d|3[01])\.\d{1,3}\.\d{1,3})\b",
            7,
            false),

        new ScannerRule(
            "DatabaseConnectionString",
            FindingCategory.DatabaseConnectionString,
            Severity.Critical,
            "Potential database connection string",
            "A string literal resembles a database connection string with credentials or host details.",
            "Use secret management and runtime configuration for database connectivity.",
            @"(?:Data Source|Server|Host|Database|User ID|Password|Initial Catalog)\s*[=:;][^""'\s]{4,}",
            8,
            true),

        new ScannerRule(
            "SuspiciousConfiguration",
            FindingCategory.SuspiciousConfiguration,
            Severity.Low,
            "Suspicious configuration indicator",
            "A string literal resembles debugging, vulnerability research, or reverse-engineering configuration.",
            "Confirm whether the string is required for production. Remove developer-only configuration if present.",
            @"(?:debug|cheat|god|gm|tunnel|proxy|mono|il2cpp|dump|inject)\b[^""'\s]{0,128}",
            5,
            true)
    };

    public ScanResult Scan(IEnumerable<string> strings, string sourceName = "global-metadata.dat")
    {
        if (strings is null) throw new ArgumentNullException(nameof(strings));

        var findings = new List<SecurityFinding>();
        int offset = 0;

        foreach (string value in strings)
        {
            foreach (ScannerRule rule in Rules)
            {
                foreach (Match match in rule.Regex.Matches(value))
                {
                    if (match.Success)
                    {
                        string matchedValue = match.Value;

                        if (matchedValue.Length < rule.MinimumMatchLength)
                        {
                            continue;
                        }

                        findings.Add(new SecurityFinding
                        {
                            Severity = rule.Severity,
                            Category = rule.Category,
                            Title = rule.Title,
                            Description = rule.Description,
                            MatchedValue = matchedValue,
                            Pattern = rule.Name,
                            Offset = offset,
                            Context = value,
                            Remediation = rule.Remediation,
                            SourceFile = sourceName
                        });
                    }
                }
            }

            offset++;
        }

        return new ScanResult(findings);
    }

    private sealed class ScannerRule
    {
        public string Name { get; }
        public FindingCategory Category { get; }
        public Severity Severity { get; }
        public string Title { get; }
        public string Description { get; }
        public string Remediation { get; }
        public Regex Regex { get; }

        public ScannerRule(string name, FindingCategory category, Severity severity, string title, string description, string remediation, string pattern, int minimumMatchLength, bool ignoreCase)
        {
            Name = name;
            Category = category;
            Severity = severity;
            Title = title;
            Description = description;
            Remediation = remediation;
            Regex = new Regex(pattern, ignoreCase ? RegexOptions.IgnoreCase | RegexOptions.Compiled : RegexOptions.Compiled);
            MinimumMatchLength = minimumMatchLength;
        }

        public int MinimumMatchLength { get; }
    }
}

/// <summary>
/// Result of a metadata string scan.
/// </summary>
public sealed class ScanResult
{
    public ScanResult(IReadOnlyList<SecurityFinding> findings)
    {
        Findings = findings ?? throw new ArgumentNullException(nameof(findings));
    }

    public IReadOnlyList<SecurityFinding> Findings { get; }
}
