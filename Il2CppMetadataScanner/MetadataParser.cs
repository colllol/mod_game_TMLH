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

        if (stream.Length < 32)
        {
            throw new InvalidDataException("global-metadata.dat is too small to contain an IL2CPP metadata header.");
        }

        // Unity metadata header format varies across versions. Many versions start with
        // four bytes that look like a small integer version or a signature. We do not
        // hardcode exact behavior here; instead we store raw header bytes and key
        // offsets so downstream analysis can adapt.
        header.RawHeader = reader.ReadBytes(16);

        header.StringSectionOffset = ReadUInt32(reader, stream);
        header.StringSectionSize = ReadUInt32(reader, stream);

        if (header.StringSectionOffset <= 0 || header.StringSectionSize <= 0)
        {
            throw new InvalidDataException("Metadata string section offset or size is invalid.");
        }

        if (header.StringSectionOffset + header.StringSectionSize > stream.Length)
        {
            throw new InvalidDataException("Metadata string section exceeds file boundaries.");
        }

        return header;
    }

    /// <summary>
    /// Extracts candidate strings from the string section.
    /// </summary>
    /// <param name="maxStringLength">Maximum length for extracted strings.</param>
    public IReadOnlyList<string> ExtractStrings(MetadataHeader header, int maxStringLength = 512)
    {
        if (header is null) throw new ArgumentNullException(nameof(header));
        if (maxStringLength <= 0) throw new ArgumentOutOfRangeException(nameof(maxStringLength));

        var results = new List<string>();

        using var stream = File.OpenRead(_metadataPath);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        if (stream.Length < header.StringSectionOffset + header.StringSectionSize)
        {
            throw new InvalidDataException("Metadata file is smaller than declared string section.");
        }

        stream.Seek(header.StringSectionOffset, SeekOrigin.Begin);

        var sectionBytes = reader.ReadBytes((int)header.StringSectionSize);
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

/// <summary>
/// Metadata file header information and section boundaries.
/// </summary>
public sealed class MetadataHeader
{
    public string FilePath { get; set; } = string.Empty;
    public long Length { get; set; }
    public byte[] RawHeader { get; set; } = Array.Empty<byte>();
    public uint StringSectionOffset { get; set; }
    public uint StringSectionSize { get; set; }
}
