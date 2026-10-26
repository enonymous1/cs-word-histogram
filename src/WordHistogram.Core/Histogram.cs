namespace WordHistogram.Core;

/// <summary>
/// A mutable, case-insensitive word-count histogram. This is the whole
/// model: the console app only renders it and reads user input, so every
/// behaviour here is unit-testable without a console.
/// </summary>
public sealed class Histogram
{
    private readonly Dictionary<string, int> _counts = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, int> Counts => _counts;

    public int UniqueWords => _counts.Count;

    public int TotalWords => _counts.Values.Sum();

    public void AddText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        AddCounts(WordCounter.Count(text));
    }

    public void AddCounts(IReadOnlyDictionary<string, int> counts)
    {
        ArgumentNullException.ThrowIfNull(counts);

        foreach (KeyValuePair<string, int> pair in counts)
        {
            if (pair.Value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(counts), $"Count for '{pair.Key}' must not be negative.");
            }

            _counts[pair.Key] = _counts.TryGetValue(pair.Key, out int existing)
                ? existing + pair.Value
                : pair.Value;
        }
    }

    /// <summary>Returns the count for <paramref name="word"/>, or null if absent.</summary>
    public int? Search(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return null;
        }

        return _counts.TryGetValue(word, out int count) ? count : null;
    }

    /// <summary>
    /// Removes <paramref name="word"/>. Returns true only if it was present,
    /// so callers can tell "removed" apart from "wasn't there".
    /// </summary>
    public bool Remove(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return false;
        }

        return _counts.Remove(word);
    }

    public void Clear() => _counts.Clear();

    /// <summary>Entries ordered by count descending, then word ascending.</summary>
    public IEnumerable<KeyValuePair<string, int>> ByCountDescending() =>
        _counts
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase);

    public IEnumerable<KeyValuePair<string, int>> Top(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Top count must not be negative.");
        }

        return ByCountDescending().Take(count);
    }
}
