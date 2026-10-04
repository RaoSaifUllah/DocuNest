using DocuNest.Application.Common;

namespace DocuNest.Application.Policies;

public sealed record ImportLimits(int MaximumFiles = 1000, long MaximumBytes = 1024L * 1024 * 1024,
    long MaximumFileBytes = 50L * 1024 * 1024, int MaximumDepth = 8, int MaximumPathLength = 240,
    double MaximumCompressionRatio = 100);

public static class ImportPolicy
{
    private static readonly HashSet<string> ReservedNames = ["CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"];

    public static string ValidateRelativePath(string path, ImportLimits limits, bool isSymbolicLink = false)
    {
        if (isSymbolicLink || string.IsNullOrWhiteSpace(path) || path.Length > limits.MaximumPathLength
            || path.StartsWith('/') || path.Contains('\\') || path.Contains(':') || path.Any(char.IsControl))
            throw new UseCaseException("unsafe_import_path", 400, "The import path must be a bounded relative file path without symlinks.");
        var segments = path.Split('/');
        if (segments.Length > limits.MaximumDepth || segments.Any(s => string.IsNullOrWhiteSpace(s) || s is "." or ".."
            || s.EndsWith('.') || s.EndsWith(' ') || s.IndexOfAny(['<', '>', '"', '|', '?', '*']) >= 0
            || ReservedNames.Contains(s.Split('.')[0].ToUpperInvariant())))
            throw new UseCaseException("unsafe_import_path", 400, "The import path contains unsafe or excessive segments.");
        return string.Join('/', segments);
    }

    public static void ValidateEntry(long uncompressedBytes, long compressedBytes, int fileCount, long accumulatedBytes, ImportLimits limits)
    {
        if (uncompressedBytes < 0 || uncompressedBytes > limits.MaximumFileBytes || compressedBytes < 0
            || fileCount <= 0 || fileCount > limits.MaximumFiles || accumulatedBytes < 0
            || accumulatedBytes > limits.MaximumBytes || uncompressedBytes > limits.MaximumBytes - accumulatedBytes)
            throw new UseCaseException("import_limit_exceeded", 413, "The import exceeds its file or byte limits.");
        if (uncompressedBytes > 0 && (compressedBytes == 0 || (double)uncompressedBytes / compressedBytes > limits.MaximumCompressionRatio))
            throw new UseCaseException("import_compression_limit", 413, "The archive compression ratio exceeds its limit.");
    }
}
