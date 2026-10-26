using WordHistogram.Core;
using Xunit;

namespace WordHistogram.Tests;

public class HistogramStoreTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    public HistogramStoreTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public void RoundTrip_PreservesCountsAndCaseInsensitivity()
    {
        string path = Path.Combine(_tempDir, "hist.json");
        var original = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["We"] = 5,
            ["hello"] = 2,
        };

        HistogramStore.Save(original, path);
        Dictionary<string, int> loaded = HistogramStore.Load(path);

        Assert.Equal(5, loaded["we"]);
        Assert.Equal(2, loaded["HELLO"]);
        Assert.True(File.ReadAllText(path).Contains("\"We\"", StringComparison.Ordinal));
    }

    [Fact]
    public void Load_MissingFile_ThrowsFileNotFound()
    {
        Assert.Throws<FileNotFoundException>(() =>
            HistogramStore.Load(Path.Combine(_tempDir, "nope.json")));
    }

    [Fact]
    public void Load_MalformedJson_ThrowsInvalidData()
    {
        string path = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(path, "{ not json");

        Assert.Throws<InvalidDataException>(() => HistogramStore.Load(path));
    }

    [Fact]
    public void Load_NullDocument_ThrowsInvalidData()
    {
        string path = Path.Combine(_tempDir, "null.json");
        File.WriteAllText(path, "null");

        Assert.Throws<InvalidDataException>(() => HistogramStore.Load(path));
    }

    [Fact]
    public void Load_NegativeCount_ThrowsInvalidData()
    {
        string path = Path.Combine(_tempDir, "negative.json");
        File.WriteAllText(path, """{ "hello": -1 }""");

        Assert.Throws<InvalidDataException>(() => HistogramStore.Load(path));
    }

    [Fact]
    public void Load_CaseVariants_AreMerged()
    {
        // A hand-written file may hold both "We" and "we"; they are one word.
        string path = Path.Combine(_tempDir, "cases.json");
        File.WriteAllText(path, """{ "We": 2, "we": 3 }""");

        Dictionary<string, int> loaded = HistogramStore.Load(path);

        Assert.Single(loaded);
        Assert.Equal(5, loaded["we"]);
    }
}
