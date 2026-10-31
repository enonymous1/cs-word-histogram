namespace WordHistogram;

/// <summary>
/// How <see cref="ArgumentParser.TryParse"/> classified the arguments.
/// This replaces the previous sentinel scheme, where help was signalled
/// by a null <c>Options</c> and interactive mode by an empty one — a
/// distinction that only worked because a separate validation step
/// happened to reject batch-only flags with no input, an invariant
/// that was written nowhere and would silently break if that
/// validation was ever relaxed.
/// </summary>
internal enum ParseOutcome
{
    Interactive,
    Batch,
    Help,
    Error,
}

/// <summary>
/// The result of parsing. <see cref="Options"/> is set only for
/// <see cref="ParseOutcome.Batch"/>; help and error carry no options,
/// and error carries a message for <c>Console.Error</c> plus whether
/// the usage text should follow it (usage follows unknown flags and
/// missing values, as before, but not validation errors such as a
/// negative <c>--top</c>).
/// </summary>
internal sealed record ParseResult(ParseOutcome Outcome, Options? Options, string? Error, bool ShowUsage);

/// <summary>
/// Batch-mode options. Interactive mode needs none of these.
/// </summary>
internal sealed class Options
{
    public string? FilePath { get; set; }
    public string? Text { get; set; }
    public bool FromStdin { get; set; }
    public int? Top { get; set; }
    public string? SavePath { get; set; }
    public string Format { get; set; } = "text";
}

/// <summary>
/// Parses command-line arguments. This class does no console I/O:
/// it returns a <see cref="ParseResult"/> and lets <see cref="Program"/>
/// decide what to print and which exit code to return, so the parser
/// is unit-testable without a terminal. Exit codes, also stated in
/// <see cref="Usage"/>: 0 for success or --help, 1 for an input or
/// save failure (in <see cref="Program"/>), 2 for an argument error.
/// </summary>
internal static class ArgumentParser
{
    internal const string Usage = """
        Usage:
          word-histogram                                Interactive menu over a sample text
          word-histogram --file <path> [--top N] [--save <path>] [--format text|json]
          word-histogram --text "<text>" [--top N] [--save <path>] [--format text|json]
          word-histogram --stdin [--top N] [--save <path>] [--format text|json]

        Options:
          --file <path>    Count the words in a text file.
          --text <text>    Count the words in the given text.
          --stdin          Read text from standard input.
          --top N          Show only the N most frequent words.
          --save <path>    Save the histogram as JSON (a .json extension is appended if missing).
          --format         text (default) or json, for batch output.
          --help           Show this help.

        Exit codes:
          0  Success, or --help.
          1  An input file could not be read, or a histogram could not be saved.
          2  An argument error.
        """;

    internal static ParseResult TryParse(string[] args)
    {
        if (args.Length == 0)
        {
            return new ParseResult(ParseOutcome.Interactive, null, null, false);
        }

        var parsed = new Options();
        bool formatSeen = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--help":
                case "-h":
                    return new ParseResult(ParseOutcome.Help, null, null, false);
                case "--file":
                    if (parsed.FilePath is not null)
                    {
                        return Error("error: --file was given more than once.", true);
                    }

                    if (!TryTakeValue(args, ref i, out string? file, out string? fileError))
                    {
                        return Error(fileError!, true);
                    }

                    parsed.FilePath = file;
                    break;
                case "--text":
                    if (parsed.Text is not null)
                    {
                        return Error("error: --text was given more than once.", true);
                    }

                    if (!TryTakeValue(args, ref i, out string? text, out string? textError))
                    {
                        return Error(textError!, true);
                    }

                    parsed.Text = text;
                    break;
                case "--stdin":
                    if (parsed.FromStdin)
                    {
                        return Error("error: --stdin was given more than once.", true);
                    }

                    parsed.FromStdin = true;
                    break;
                case "--top":
                    if (parsed.Top is not null)
                    {
                        return Error("error: --top was given more than once.", true);
                    }

                    if (!TryTakeValue(args, ref i, out string? topText, out string? topError)
                        || !int.TryParse(topText, out int top)
                        || top < 0)
                    {
                        // A missing value is reported by TryTakeValue;
                        // an unparsable or negative value is reported here.
                        // Either way, no usage text follows: the error
                        // line alone says what is wrong.
                        return Error(topError ?? "error: --top needs a non-negative integer.", false);
                    }

                    parsed.Top = top;
                    break;
                case "--save":
                    if (parsed.SavePath is not null)
                    {
                        return Error("error: --save was given more than once.", true);
                    }

                    if (!TryTakeValue(args, ref i, out string? save, out string? saveError))
                    {
                        return Error(saveError!, true);
                    }

                    parsed.SavePath = save;
                    break;
                case "--format":
                    if (formatSeen)
                    {
                        return Error("error: --format was given more than once.", true);
                    }

                    if (!TryTakeValue(args, ref i, out string? format, out string? formatError)
                        || (format != "text" && format != "json"))
                    {
                        return Error(formatError ?? "error: --format must be 'text' or 'json'.", false);
                    }

                    formatSeen = true;
                    parsed.Format = format;
                    break;
                default:
                    return Error($"error: unknown argument '{args[i]}'.", true);
            }
        }

        int inputSources = (parsed.FilePath is not null ? 1 : 0)
            + (parsed.Text is not null ? 1 : 0)
            + (parsed.FromStdin ? 1 : 0);
        if (inputSources > 1)
        {
            return Error("error: use only one of --file, --text, or --stdin.", false);
        }

        if (inputSources == 0
            && (parsed.Top is not null || parsed.SavePath is not null || formatSeen))
        {
            // Previously, a bare "--format text" slipped through this
            // check (because Format equalled its default) and started
            // an interactive session, silently ignoring the flag.
            // Any batch-only flag with no input is now an error.
            return Error("error: --top, --save, and --format need an input source (--file, --text, or --stdin).", false);
        }

        return inputSources == 0
            ? new ParseResult(ParseOutcome.Interactive, null, null, false)
            : new ParseResult(ParseOutcome.Batch, parsed, null, false);
    }

    private static ParseResult Error(string message, bool showUsage) =>
        new(ParseOutcome.Error, null, message, showUsage);

    private static bool TryTakeValue(string[] args, ref int index, out string? value, out string? error)
    {
        value = null;
        error = null;
        if (index + 1 >= args.Length)
        {
            error = $"error: '{args[index]}' needs a value.";
            return false;
        }

        value = args[++index];
        return true;
    }
}
