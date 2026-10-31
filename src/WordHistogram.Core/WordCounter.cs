namespace WordHistogram.Core;

/// <summary>
/// Splits text into words and counts them, ignoring case.
/// </summary>
public static class WordCounter
{
    /// <summary>
    /// Splits <paramref name="text"/> into words. A word is a run of letters
    /// or digits; an apostrophe inside a word is kept, so "won't" stays one
    /// word instead of splitting into "won" and "t". The curly apostrophe
    /// (U+2019) counts the same way. All other punctuation and whitespace
    /// separates words.
    /// </summary>
    public static IReadOnlyList<string> Tokenize(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var words = new List<string>();
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (char.IsLetterOrDigit(c))
            {
                current.Append(c);
            }
            else if ((c == '\'' || c == '\u2019')
                && current.Length > 0
                && i + 1 < text.Length
                && char.IsLetter(text[i + 1]))
            {
                // Internal apostrophe, ASCII or curly (U+2019, as pasted
                // from Word or the web): don't, won't, it's, don’t.
                current.Append(c);
            }
            else if (current.Length > 0)
            {
                words.Add(current.ToString());
                current.Clear();
            }
        }

        if (current.Length > 0)
        {
            words.Add(current.ToString());
        }

        return words;
    }

    /// <summary>
    /// Counts the words in <paramref name="text"/>. Keys keep the casing of
    /// their first occurrence but compare case-insensitively, so "We", "we"
    /// and "WE" all count under one entry.
    /// </summary>
    public static Dictionary<string, int> Count(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (string word in Tokenize(text))
        {
            counts[word] = counts.TryGetValue(word, out int existing) ? existing + 1 : 1;
        }

        return counts;
    }
}
