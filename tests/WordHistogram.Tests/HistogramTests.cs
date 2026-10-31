using WordHistogram.Core;
using Xunit;

namespace WordHistogram.Tests;

public class HistogramTests
{
    [Fact]
    public void Search_IsCaseInsensitive_AndNullWhenAbsent()
    {
        var histogram = new Histogram();
        histogram.AddText("Hello hello");

        Assert.Equal(2, histogram.Search("HELLO"));
        Assert.Null(histogram.Search("goodbye"));
        Assert.Null(histogram.Search("  "));
    }

    [Fact]
    public void Remove_ReturnsTrueOnlyWhenPresent()
    {
        // The original RemoveWord printed "successfully removed" even for
        // words that were never in the histogram.
        var histogram = new Histogram();
        histogram.AddText("hello world");

        Assert.True(histogram.Remove("HELLO"));
        Assert.False(histogram.Remove("hello"));
        Assert.False(histogram.Remove("missing"));
        Assert.Equal(1, histogram.UniqueWords);
    }

    [Fact]
    public void Top_OrdersByCountThenWord()
    {
        var histogram = new Histogram();
        histogram.AddText("b a b c a b");

        Assert.Equal(
            new[] { ("b", 3), ("a", 2), ("c", 1) },
            histogram.Top(3).Select(pair => (pair.Key, pair.Value)));
    }

    [Fact]
    public void Top_TiesBreakByWordAscending()
    {
        var histogram = new Histogram();
        histogram.AddText("b a c");

        Assert.Equal(
            new[] { ("a", 1), ("b", 1), ("c", 1) },
            histogram.Top(3).Select(pair => (pair.Key, pair.Value)));
    }

    [Fact]
    public void AddCounts_RejectsNegativeCounts()
    {
        var histogram = new Histogram();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            histogram.AddCounts(new Dictionary<string, int> { ["bad"] = -1 }));
    }

    [Fact]
    public void Totals_CountUniqueAndTotalSeparately()
    {
        var histogram = new Histogram();
        histogram.AddText("one two two three three three");

        Assert.Equal(3, histogram.UniqueWords);
        Assert.Equal(6, histogram.TotalWords);
    }
}
