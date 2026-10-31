using Xunit;

namespace WordHistogram.Tests;

public class ArgumentParserTests
{
    [Fact]
    public void Parse_NoArgs_IsInteractive()
    {
        ParseResult result = ArgumentParser.TryParse([]);

        Assert.Equal(ParseOutcome.Interactive, result.Outcome);
        Assert.Null(result.Options);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Parse_Help_IsHelp()
    {
        Assert.Equal(ParseOutcome.Help, ArgumentParser.TryParse(["--help"]).Outcome);
        Assert.Equal(ParseOutcome.Help, ArgumentParser.TryParse(["-h"]).Outcome);
    }

    [Fact]
    public void Parse_FileWithTopSaveAndFormat_IsBatch()
    {
        ParseResult result = ArgumentParser.TryParse(
            ["--file", "a.txt", "--top", "5", "--save", "out", "--format", "json"]);

        Assert.Equal(ParseOutcome.Batch, result.Outcome);
        Assert.Equal("a.txt", result.Options!.FilePath);
        Assert.Equal(5, result.Options.Top);
        Assert.Equal("out", result.Options.SavePath);
        Assert.Equal("json", result.Options.Format);
    }

    [Fact]
    public void Parse_TextAndStdin_AreBatch()
    {
        Assert.Equal(ParseOutcome.Batch, ArgumentParser.TryParse(["--text", "hello"]).Outcome);
        Assert.Equal(ParseOutcome.Batch, ArgumentParser.TryParse(["--stdin"]).Outcome);
    }

    [Fact]
    public void Parse_UnknownArgument_IsErrorWithUsage()
    {
        ParseResult result = ArgumentParser.TryParse(["--nope"]);

        Assert.Equal(ParseOutcome.Error, result.Outcome);
        Assert.True(result.ShowUsage);
    }

    [Fact]
    public void Parse_MissingValue_IsError()
    {
        AssertError(["--file"]);
        AssertError(["--top"]);
        AssertError(["--format"]);
    }

    [Fact]
    public void Parse_DuplicateFlags_AreError()
    {
        AssertError(["--file", "a.txt", "--file", "b.txt"]);
        AssertError(["--text", "a", "--text", "b"]);
        AssertError(["--stdin", "--stdin"]);
        AssertError(["--top", "1", "--top", "2"]);
        AssertError(["--save", "a", "--save", "b"]);
        AssertError(["--format", "text", "--format", "json"]);
    }

    [Fact]
    public void Parse_TwoInputSources_IsError()
    {
        AssertError(["--file", "a.txt", "--text", "hi"]);
        AssertError(["--file", "a.txt", "--stdin"]);
    }

    [Fact]
    public void Parse_BadTop_IsErrorWithoutUsage()
    {
        ParseResult negative = ArgumentParser.TryParse(["--top", "-1"]);
        ParseResult notANumber = ArgumentParser.TryParse(["--top", "not-a-number"]);

        Assert.Equal(ParseOutcome.Error, negative.Outcome);
        Assert.False(negative.ShowUsage);
        Assert.Equal(ParseOutcome.Error, notANumber.Outcome);
    }

    [Fact]
    public void Parse_BatchOnlyFlagsWithoutInput_AreError()
    {
        // Includes a bare "--format text": it used to slip through
        // (because Format equalled its default) and start an
        // interactive session, silently ignoring the flag.
        AssertError(["--top", "5"]);
        AssertError(["--save", "out"]);
        AssertError(["--format", "json"]);
        AssertError(["--format", "text"]);
        AssertError(["--format", "yaml"]);
    }

    private static void AssertError(string[] args)
    {
        ParseResult result = ArgumentParser.TryParse(args);

        Assert.Equal(ParseOutcome.Error, result.Outcome);
        Assert.Null(result.Options);
        Assert.NotNull(result.Error);
    }
}
