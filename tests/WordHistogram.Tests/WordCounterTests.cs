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
    public void Count_Pg2SpeechSample_HandlesItsPunctuation()
    {
        // The 2021 PG2 lab's sample text exercises punctuation its own
        // tokenizer mishandled: semicolons, exclamation marks, and an
        // apostrophe in "God's". The modern tokenizer must keep "God's"
        // whole, split on ';' and '!', and count case-insensitively.
        Dictionary<string, int> counts = WordCounter.Count(
            "all of God's children, black men and white men, Jews and " +
            "Gentiles, Protestants and Catholics, will be able to join " +
            "hands and sing in the words of the old Negro spiritual, " +
            "Free at last! free at last! thank God Almighty, we are " +
            "free at last!");

        Assert.Equal(1, counts["God's"]);
        Assert.Equal(1, counts["God"]);
        Assert.Equal(3, counts["free"]);
        Assert.Equal(3, counts["last"]);
        Assert.Equal(3, counts["at"]);
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
