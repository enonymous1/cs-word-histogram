using WordHistogram.Core;
using Xunit;

namespace WordHistogram.Tests;

public class WordCounterTests
{
    [Fact]
    public void Count_IsCaseInsensitive()
    {
        Dictionary<string, int> counts = WordCounter.Count("We we WE");

        Assert.Single(counts);
        Assert.Equal(3, counts["we"]);
        Assert.Equal(3, counts["We"]);
    }

    [Fact]
    public void Count_IgnoresPunctuationAndWhitespace()
    {
        Dictionary<string, int> counts = WordCounter.Count("Hello, hello! Hello.\n\tHello?");

        Assert.Single(counts);
        Assert.Equal(4, counts["hello"]);
    }

    [Fact]
    public void Tokenize_KeepsContractionsWhole()
    {
        // The original tokenizer split on apostrophes: "won't" -> "won", "t".
        IReadOnlyList<string> words = WordCounter.Tokenize("We won't go");

        Assert.Equal(new[] { "We", "won't", "go" }, words);
    }

    [Fact]
    public void Tokenize_SplitsOnHyphens()
    {
        IReadOnlyList<string> words = WordCounter.Tokenize("well-known fact");

        Assert.Equal(new[] { "well", "known", "fact" }, words);
    }

    [Fact]
    public void Count_EmptyText_IsEmpty()
    {
        Assert.Empty(WordCounter.Count(string.Empty));
        Assert.Empty(WordCounter.Count("   ,,, !!! "));
    }

    [Fact]
    public void Count_SampleLyrics_MatchesHandCount()
    {
        Dictionary<string, int> counts = WordCounter.Count(SampleText);

        Assert.Equal(3, counts["we"]);
        Assert.Equal(1, counts["figgy"]);
        Assert.Equal(1, counts["won't"]);
        Assert.Equal(2, counts["some"]);
    }

    private const string SampleText = """
        We wish you a merry Christmas
        We won't go until we get some
        Oh, bring us some figgy pudding
        """;
}
