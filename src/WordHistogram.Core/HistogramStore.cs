using System.Text.Json;

namespace WordHistogram.Core;

/// <summary>
/// Saves and loads histograms as indented JSON. Loading never returns a
/// half-loaded result: a missing file, malformed JSON, a null document, or
/// a negative count throws, so a caller can keep its current histogram
/// untouched on failure.
/// </summary>
public static class HistogramStore
{
    public static void Save(IReadOnlyDictionary<string, int> counts, string path)
    {
        ArgumentNullException.ThrowIfNull(counts);
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("A file path is required.", nameof(path));
        }

        // Deterministic file layout: keys sorted case-insensitively.
        var ordered = counts
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);

        string json = JsonSerializer.Serialize(ordered, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Loads a histogram saved by <see cref="Save"/>. The result is always
    /// case-insensitive, matching the in-memory histogram.
    /// </summary>
    public static Dictionary<string, int> Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("A file path is required.", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Histogram file not found: {path}", path);
        }

        string json = File.ReadAllText(path);

        Dictionary<string, int>? data;
        try
        {
            data = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"File '{path}' is not a valid histogram JSON file.", ex);
        }

        if (data is null)
        {
            throw new InvalidDataException($"File '{path}' does not contain a histogram object.");
        }

        if (data.Values.Any(value => value < 0))
        {
            throw new InvalidDataException($"File '{path}' contains a negative word count.");
        }

        // Merge case variants ("We" and "we") instead of letting one win.
        // The addition is checked: without it, two large case variants
        // would wrap to a negative count, which AddCounts would then
        // reject with an exception the caller does not expect from Load.
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, int> pair in data)
        {
            int merged;
            try
            {
                merged = result.TryGetValue(pair.Key, out int existing)
                    ? checked(existing + pair.Value)
                    : pair.Value;
            }
            catch (OverflowException ex)
            {
                throw new InvalidDataException(
                    $"File '{path}' contains a word count too large to load.", ex);
            }

            result[pair.Key] = merged;
        }

        return result;
    }
}
