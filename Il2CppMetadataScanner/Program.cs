using System;
using System.IO;

namespace Il2CppMetadataScanner;

/// <summary>
/// Defensive static-analysis entry point for IL2CPP metadata inspection.
///
/// This tool only reads global-metadata.dat, extracts strings, applies regex
/// rules, and writes report files. It does not inject, hook, or modify any
/// running process.
/// </summary>
public static class Program
{
    /// <summary>
    /// CLI usage:
    ///   Il2CppMetadataScanner.exe --metadata path\to\global-metadata.dat --output .\reports
    /// </summary>
    public static int Main(string[] args)
    {
        string? metadataPath = null;
        string? outputPath = null;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg is "--metadata" or "-m")
            {
                if (i + 1 >= args.Length)
                {
                    Console.Error.WriteLine("Missing value for --metadata.");
                    return 1;
                }

                metadataPath = args[i + 1];
                i++;
            }
            else if (arg is "--output" or "-o")
            {
                if (i + 1 >= args.Length)
                {
                    Console.Error.WriteLine("Missing value for --output.");
                    return 1;
                }

                outputPath = args[i + 1];
                i++;
            }
            else if (arg is "--help" or "-h" or "/?")
            {
                PrintHelp();
                return 0;
            }
            else
            {
                Console.Error.WriteLine($"Unknown argument: {arg}");
                return 1;
            }
        }

        if (string.IsNullOrWhiteSpace(metadataPath))
        {
            Console.Error.WriteLine("A metadata path is required. Use --metadata.");
            PrintHelp();
            return 1;
        }

        if (!File.Exists(metadataPath))
        {
            Console.Error.WriteLine($"Metadata file not found: {metadataPath}");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            outputPath = Directory.GetCurrentDirectory();
        }

        Directory.CreateDirectory(outputPath);

        try
        {
            var parser = new MetadataParser(metadataPath);
            MetadataHeader header = parser.ReadHeader();

            var strings = parser.ExtractStrings(header);
            var scanner = new MetadataStringScanner();
            ScanResult scanResult = scanner.Scan(strings);

            var summary = VulnerabilityReport.BuildSummary(header, scanResult);

            string baseName = Path.GetFileNameWithoutExtension(metadataPath);
            string timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss_fff", null);
            string jsonPath = Path.Combine(outputPath, $"{baseName}_{timestamp}.json");
            string htmlPath = Path.Combine(outputPath, $"{baseName}_{timestamp}.html");

            VulnerabilityReport.WriteJsonReport(jsonPath, summary, scanResult);
            VulnerabilityReport.WriteHtmlReport(htmlPath, summary, scanResult);

            PrintSummary(summary, scanResult, strings.Count, jsonPath, htmlPath);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Scan failed: {ex.Message}");
            return 2;
        }
    }

    private static void PrintSummary(ScanSummary summary, ScanResult scanResult, int totalStrings, string jsonPath, string htmlPath)
    {
        Console.WriteLine("Scan complete.");
        Console.WriteLine($"Metadata: {summary.MetadataPath}");
        Console.WriteLine($"Total strings scanned: {totalStrings}");
        Console.WriteLine($"Findings: {summary.FindingsCount}");
        Console.WriteLine($"Critical: {summary.CriticalCount}");
        Console.WriteLine($"High: {summary.HighCount}");
        Console.WriteLine($"Medium: {summary.MediumCount}");
        Console.WriteLine($"Low: {summary.LowCount}");
        Console.WriteLine($"Duration: {summary.Duration:g}");
        Console.WriteLine($"JSON report: {jsonPath}");
        Console.WriteLine($"HTML report: {htmlPath}");

        if (scanResult.Findings.Count == 0)
        {
            Console.WriteLine("No security-sensitive patterns were found. Findings may still require manual review.");
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("IL2CPP Metadata Security Scanner");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  Il2CppMetadataScanner.exe --metadata <global-metadata.dat> --output <report-directory>");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --metadata, -m   Path to global-metadata.dat");
        Console.WriteLine("  --output,   -o   Report output directory");
        Console.WriteLine("  --help,     -h   Show this help");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  Il2CppMetadataScanner.exe --metadata C:\\Games\\MyGame\\MyGame_Data\\il2cpp_data\\global-metadata.dat --output .\\reports");
        Console.WriteLine();
        Console.WriteLine("Legal note:");
        Console.WriteLine("  Only scan files from games or builds you own or for which you have explicit permission.");
    }
}
