namespace DocuNest.Infrastructure.Configuration;

public sealed class RuntimeOptions
{
    public string Origin { get; set; } = "https://localhost:8443";
    public string StorageRoot { get; set; } = "/data/docunest";
    public string JwtPrivateKeyPath { get; set; } = "/run/secrets/jwt.pem";
    public string EncryptionKeyPath { get; set; } = "/run/secrets/encryption.key";
    public string JwtIssuer { get; set; } = "DocuNest";
    public string JwtAudience { get; set; } = "DocuNest.Web";
    public string RegistrationMode { get; set; } = "InviteOnly";
    public string NotificationMode { get; set; } = "LocalOnly";
    public string ScannerHost { get; set; } = "scanner";
    public int ScannerPort { get; set; } = 3310;
    public long MaxFileBytes { get; set; } = 50L * 1024 * 1024;
    public int MaxPages { get; set; } = 200;
    public long MaxImagePixels { get; set; } = 40_000_000;
    public string[] OcrLanguages { get; set; } = ["eng"];
    public string ProcessingImage { get; set; } = "docunest-processor:local";
    public string? BootstrapTokenHash { get; set; }

    public static string ResolvePath(string path)
    {
        var root = Environment.GetEnvironmentVariable("DOCUNEST_PROJECT_ROOT");
        return Path.GetFullPath(path, root ?? Environment.CurrentDirectory);
    }
}
