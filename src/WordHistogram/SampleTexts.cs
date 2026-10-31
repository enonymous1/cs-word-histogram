namespace WordHistogram;

/// <summary>
/// The demo texts for the interactive menu, loaded from the embedded
/// copies of the repo-root sample files (see the console project's
/// <c>EmbeddedResource</c> items). There is deliberately one source
/// of truth: the previous design kept the same text both as a
/// constant in code and as a .txt file, and nothing stopped the two
/// from drifting apart. Do not add attribution text to the .txt
/// files themselves — any extra line would change their word counts
/// and break the README's sample output. Attribution lives in the
/// README and in the interactive menu's sample-choice text.
/// </summary>
internal static class SampleTexts
{
    private static string? _lyrics;
    private static string? _speech;

    internal static string Lyrics => _lyrics ??= Load("WordHistogram.sample-lyrics.txt");

    internal static string Speech => _speech ??= Load("WordHistogram.sample-mlk.txt");

    private static string Load(string resourceName)
    {
        using Stream? stream = typeof(SampleTexts).Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Embedded sample text '{resourceName}' was not found.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
