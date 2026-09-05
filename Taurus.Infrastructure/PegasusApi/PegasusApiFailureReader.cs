using System.Net.Http.Json;
using System.Text.Json;
using PegasusApi.Abstractions;

namespace Taurus.Infrastructure.PegasusApi;

internal static class PegasusApiFailureReader
{
    public static async Task<string> ReadAsync(HttpResponseMessage response, string fallbackMessage)
    {
        try {
            var failure = await response.Content.ReadFromJsonAsync<ValidationFailureResponse>();
            if (failure is null)
                return fallbackMessage;

            var errors = JsonSerializer.SerializeToElement(failure.Errors);

            var messages = GetMessages(errors)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct()
                .ToArray();

            return messages.Length > 0
                ? string.Join(" ", messages)
                : fallbackMessage;
        } catch (JsonException) {
            return fallbackMessage;
        }
    }

    private static IEnumerable<string> GetMessages(JsonElement element)
    {
        switch (element.ValueKind) {
            case JsonValueKind.String:
                yield return element.GetString()!;
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    foreach (var message in GetMessages(item))
                        yield return message;
                break;

            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                    foreach (var message in GetMessages(property.Value))
                        yield return message;
                break;
        }
    }
}