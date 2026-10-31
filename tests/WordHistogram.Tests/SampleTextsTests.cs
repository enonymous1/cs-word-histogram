using Xunit;

namespace WordHistogram.Tests;

public class SampleTextsTests
{
    [Fact]
    public void Lyrics_ContainsOriginalLyrics()
    {
        Assert.Contains("figgy pudding", SampleTexts.Lyrics, StringComparison.Ordinal);
        Assert.Contains("We wish you a merry Christmas", SampleTexts.Lyrics, StringComparison.Ordinal);
    }

    [Fact]
    public void Speech_ContainsDreamExcerpt()
    {
        Assert.Contains("I have a dream", SampleTexts.Speech, StringComparison.Ordinal);
        Assert.Contains("Free at last", SampleTexts.Speech, StringComparison.Ordinal);
    }
}
