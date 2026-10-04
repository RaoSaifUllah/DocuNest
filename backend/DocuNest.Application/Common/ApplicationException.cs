namespace DocuNest.Application.Common;

public sealed class UseCaseException(string code, int statusCode, string message) : Exception(message)
{
    public string Code { get; } = code;
    public int StatusCode { get; } = statusCode;
}

public static class CommandPreconditions
{
    public static void RequireVersion(long? expectedVersion, long actualVersion)
    {
        if (expectedVersion is null)
            throw new UseCaseException("precondition_required", 428, "An If-Match version is required.");
        if (expectedVersion != actualVersion)
            throw new UseCaseException("version_conflict", 409, "The resource changed. Reload before saving.");
    }
}
