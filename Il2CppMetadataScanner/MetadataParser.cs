using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Il2CppMetadataScanner;

/// <summary>
/// Parses Unity IL2CPP global-metadata.dat.
///
/// IL2CPP metadata layout is composed of a fixed header followed by a series of
/// named sections. Each section begins with a 16-byte header:
///   - 4 bytes: section index/type in older layouts or sentinel in newer layouts
///   - 4 bytes: offset from start of file to section data
///   - 4 bytes: size of the section data in bytes
///   - 4 bytes: flags / reserved
///
/// The string literals of interest for static analysis are stored in the
/// string section. Exact section semantics vary across Unity versions, so this
/// parser is intentionally conservative: it validates well-known header values,
/// detects section boundaries, and extracts ascii/utf-8 strings rather than
/// assuming a single metadata version.
///
/// This scanner performs static file analysis only and does not load or run any
/// executable code extracted from the metadata.
/// </summary>
public sealed class MetadataParser
{
    private readonly string _metadataPath;

    public MetadataParser(string metadataPath)
    {
        _metadataPath = metadataPath ?? throw new ArgumentNullException(nameof(metadataPath));
    }

    public MetadataHeader ReadHeader()
    {
        using var stream = File.OpenRead(_metadataPath);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        var header = new MetadataHeader
        {
            FilePath = _metadataPath,
            Length = stream.Length
        };

        if (stream.Length < 48)
        {
            throw new InvalidDataException("global-metadata.dat is too small to contain an IL2CPP metadata header.");
        }

        header.RawHeader = reader.ReadBytes(48);

        uint magic = BitConverter.ToUInt32(header.RawHeader, 0);
        if (magic != 0xFFFEEDBC)
        {
            throw new InvalidDataException($"Invalid IL2CPP magic: 0x{magic:X8} (expected 0xFFFEEDBC).");
        }

        header.Version = BitConverter.ToUInt16(header.RawHeader, 4);
        header.StringCount = BitConverter.ToUInt32(header.RawHeader, 8);
        header.StringDataSize = BitConverter.ToUInt32(header.RawHeader, 12);
        header.StringDataOffset = BitConverter.ToUInt32(header.RawHeader, 16);

        if (header.StringDataOffset <= 0 || header.StringDataSize <= 0)
        {
            header.StringDataOffset = 0;
            header.StringDataSize = 0;
        }
        else if (header.StringDataOffset + header.StringDataSize > stream.Length)
        {
            header.StringDataOffset = 0;
            header.StringDataSize = 0;
        }

        return header;
    }

    public IReadOnlyList<string> ExtractAllStrings(int maxStringLength = 512)
    {
        var results = new List<string>();
        var seen = new HashSet<string>();

        using var stream = File.OpenRead(_metadataPath);
        var allBytes = new byte[stream.Length];
        stream.Read(allBytes, 0, allBytes.Length);

        int runStart = -1;
        int bytePos = 0;

        foreach (byte b in allBytes)
        {
            if (b < 32 || b > 126)
            {
                if (runStart >= 0)
                {
                    int runLength = bytePos - runStart;
                    if (runLength >= 3 && runLength <= maxStringLength)
                    {
                        string? candidate = DecodeRun(allBytes, runStart, runLength);
                        if (!string.IsNullOrEmpty(candidate) && !seen.Contains(candidate))
                        {
                            seen.Add(candidate);
                            results.Add(candidate);
                        }
                    }
                    runStart = -1;
                }
            }
            else
            {
                if (runStart < 0) runStart = bytePos;
            }
            bytePos++;
        }

        if (runStart >= 0)
        {
            int runLength = bytePos - runStart;
            if (runLength >= 3 && runLength <= maxStringLength)
            {
                string? candidate = DecodeRun(allBytes, runStart, runLength);
                if (!string.IsNullOrEmpty(candidate) && !seen.Contains(candidate))
                {
                    seen.Add(candidate);
                    results.Add(candidate);
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Extracts candidate strings from the string section.
    /// Falls back to full-file extraction if header section boundaries are invalid.
    /// </summary>
    public IReadOnlyList<string> ExtractStrings(MetadataHeader header, int maxStringLength = 512)
    {
        if (header is null) throw new ArgumentNullException(nameof(header));
        if (maxStringLength <= 0) throw new ArgumentOutOfRangeException(nameof(maxStringLength));

        if (header.StringDataOffset <= 0 || header.StringDataSize <= 0 ||
            header.StringDataOffset + header.StringDataSize > header.Length)
        {
            return ExtractAllStrings(maxStringLength);
        }

        var results = new List<string>();

        using var stream = File.OpenRead(_metadataPath);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        stream.Seek(header.StringDataOffset, SeekOrigin.Begin);

        var sectionBytes = reader.ReadBytes((int)header.StringDataSize);
        var decoder = Encoding.UTF8.GetDecoder();

        int runStart = -1;
        int bytePos = 0;

        foreach (byte b in sectionBytes)
        {
            if (b == 0)
            {
                if (runStart >= 0)
                {
                    int runLength = bytePos - runStart;
                    if (runLength > 0 && runLength <= maxStringLength)
                    {
                        string? candidate = DecodeRun(sectionBytes, runStart, runLength, decoder);
                        if (!string.IsNullOrEmpty(candidate))
                        {
                            results.Add(candidate);
                        }
                    }

                    runStart = -1;
                }
            }
            else if (runStart < 0)
            {
                runStart = bytePos;
            }

            bytePos++;
        }

        if (runStart >= 0)
        {
            int runLength = bytePos - runStart;
            if (runLength > 0 && runLength <= maxStringLength)
            {
                string? candidate = DecodeRun(sectionBytes, runStart, runLength, decoder);
                if (!string.IsNullOrEmpty(candidate))
                {
                    results.Add(candidate);
                }
            }
        }

        return results;
    }

    private static string? DecodeRun(byte[] buffer, int start, int length, Decoder decoder)
    {
        try
        {
            return Encoding.UTF8.GetString(buffer, start, length);
        }
        catch
        {
            try
            {
                byte[] slice = new byte[length];
                Buffer.BlockCopy(buffer, start, slice, 0, length);
                return Encoding.UTF8.GetString(slice);
            }
            catch
            {
                return null;
            }
        }
    }

    private static string? DecodeRun(byte[] buffer, int start, int length)
    {
        try
        {
            return Encoding.ASCII.GetString(buffer, start, length);
        }
        catch
        {
            return null;
        }
    }

    private static uint ReadUInt32(BinaryReader reader, Stream stream)
    {
        if (stream.Position + 4 > stream.Length)
        {
            throw new InvalidDataException("Metadata file ended unexpectedly while reading a UInt32.");
        }

        uint value = reader.ReadUInt32();
        return BinaryPrimitives.ReverseEndianness(value);
    }
}

public sealed class MetadataHeader
{
    public string FilePath { get; set; } = string.Empty;
    public long Length { get; set; }
    public byte[] RawHeader { get; set; } = Array.Empty<byte>();
    public ushort Version { get; set; }
    public uint StringCount { get; set; }
    public uint StringDataSize { get; set; }
    public uint StringDataOffset { get; set; }
}
