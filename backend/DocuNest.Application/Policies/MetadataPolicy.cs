using System.Globalization;
using System.Text.Json;
using DocuNest.Domain.Entities;
using DocuNest.Application.Common;

namespace DocuNest.Application.Policies;

public static class MetadataPolicy
{
    private static readonly HashSet<string> EditableFields = ["title", "description", "vendor", "amount", "currency", "issueDate", "documentDate", "expirationDate"];

    public static void Edit(Document document, IReadOnlyDictionary<string, string?> fields, long? expectedVersion, DateTimeOffset now)
    {
        CommandPreconditions.RequireVersion(expectedVersion, document.Version);
        var confirmed = JsonSerializer.Deserialize<HashSet<string>>(document.ConfirmedFieldsJson) ?? [];
        // Validate and stage all values before mutating the aggregate, so invalid input cannot leave a partial edit.
        var staged = fields.ToDictionary(x => x.Key, x => Validate(x.Key, x.Value));
        foreach (var (field, value) in staged)
        {
            Set(document, field, value);
            confirmed.Add(field);
        }
        document.ConfirmedFieldsJson = JsonSerializer.Serialize(confirmed.Order(StringComparer.Ordinal));
        document.UpdatedAt = now;
    }

    public static bool AcceptSuggestion(Document document, ExtractionSuggestion suggestion, long? expectedVersion, DateTimeOffset now)
    {
        CommandPreconditions.RequireVersion(expectedVersion, document.Version);
        if (suggestion.WorkspaceId != document.WorkspaceId || suggestion.DocumentId != document.Id
            || suggestion.Generation != document.ProcessingGeneration || suggestion.Dismissed || suggestion.Accepted) return false;
        // Explicit acceptance is a confirmed edit; background processing only creates suggestions.
        Edit(document, new Dictionary<string, string?> { [suggestion.Field] = suggestion.Value }, expectedVersion, now);
        suggestion.Accepted = true;
        return true;
    }

    public static bool CanSuggest(Document document, string field, long generation) =>
        generation == document.ProcessingGeneration && EditableFields.Contains(field)
        && !(JsonSerializer.Deserialize<HashSet<string>>(document.ConfirmedFieldsJson) ?? []).Contains(field);

    private static string? Validate(string field, string? value)
    {
        if (!EditableFields.Contains(field)) throw new UseCaseException("invalid_metadata_field", 400, "Unknown metadata field.");
        if (value?.Length > (field == "description" ? 10_000 : 512)) throw new UseCaseException("metadata_too_long", 400, "Metadata exceeds its length limit.");
        if (field == "title" && string.IsNullOrWhiteSpace(value)) throw new UseCaseException("title_required", 400, "A title is required.");
        if (field == "amount" && !string.IsNullOrWhiteSpace(value) && (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount < 0 || amount > 9999999999999999.99m))
            throw new UseCaseException("invalid_amount", 400, "The amount must be a nonnegative decimal.");
        if (field == "currency" && !string.IsNullOrWhiteSpace(value) && (value.Length != 3 || value.Any(c => c is < 'A' or > 'Z')))
            throw new UseCaseException("invalid_currency", 400, "Currency must be a three-letter uppercase code.");
        if (field.EndsWith("Date", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(value)
            && !DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            throw new UseCaseException("invalid_date", 400, "Dates must use YYYY-MM-DD.");
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void Set(Document document, string field, string? value)
    {
        switch (field)
        {
            case "title": document.Title = value!; break;
            case "description": document.Description = value ?? ""; break;
            case "vendor": document.Vendor = value; break;
            case "amount": document.Amount = value is null ? null : decimal.Parse(value, CultureInfo.InvariantCulture); break;
            case "currency": document.Currency = value; break;
            case "issueDate": document.IssueDate = ParseDate(value); break;
            case "documentDate": document.DocumentDate = ParseDate(value); break;
            case "expirationDate": document.ExpirationDate = ParseDate(value); break;
        }
    }

    private static DateOnly? ParseDate(string? value) => value is null ? null : DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
}
