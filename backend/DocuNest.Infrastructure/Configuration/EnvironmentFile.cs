namespace DocuNest.Infrastructure.Configuration;

/// <summary>Development-only literal dotenv reader. Never evaluates shell syntax or logs values.</summary>
public static class EnvironmentFile
{
    public static void LoadDevelopment(string contentRoot, bool isDevelopment)
    {
        if (!isDevelopment) return;
        var directory = new DirectoryInfo(contentRoot);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "DocuNest.slnx")))
            {
                var path = Path.Combine(directory.FullName, ".env");
                if (!File.Exists(path)) return;
                foreach (var line in File.ReadLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#')) continue;
                    var separator = line.IndexOf('=');
                    if (separator < 1) continue;
                    var key = line[..separator].Trim();
                    if (Environment.GetEnvironmentVariable(key) is null)
                        Environment.SetEnvironmentVariable(key, line[(separator + 1)..]);
                }
                Environment.SetEnvironmentVariable("DOCUNEST_PROJECT_ROOT", directory.FullName);
                return;
            }
            directory = directory.Parent;
        }
    }
}
