using System.Text.Json;
using WordHistogram.Core;

namespace WordHistogram;

/// <summary>
/// Console front end for <see cref="Histogram"/>. All counting, searching,
/// removing and file logic lives in WordHistogram.Core; this file only
/// parses arguments, renders output, and reads user input.
/// </summary>
internal static class Program
{
    private const string Usage = """
        Usage:
          word-histogram                                Interactive menu over the sample lyrics
          word-histogram --file <path> [--top N] [--save <path>] [--format text|json]
          word-histogram --text "<text>" [--top N] [--save <path>] [--format text|json]
          word-histogram --stdin [--top N] [--save <path>] [--format text|json]

        Options:
          --file <path>    Count the words in a text file.
          --text <text>    Count the words in the given text.
          --stdin          Read text from standard input.
          --top N          Show only the N most frequent words.
          --save <path>    Save the histogram as JSON (extension forced to .json).
          --format         text (default) or json, for batch output.
          --help           Show this help.
        """;

    private sealed class Options
    {
        public string? FilePath { get; set; }
        public string? Text { get; set; }
        public bool FromStdin { get; set; }
        public int? Top { get; set; }
        public string? SavePath { get; set; }
        public string Format { get; set; } = "text";
    }

    private static int Main(string[] args)
    {
        if (!TryParseArgs(args, out Options? options))
        {
            return 2;
        }

        if (options is null)
        {
            // --help: usage already printed.
            return 0;
        }

        if (options.FilePath is not null || options.Text is not null || options.FromStdin)
        {
            return RunBatch(options);
        }

        RunInteractive();
        return 0;
    }

    // ----- Batch mode -------------------------------------------------------

    private static int RunBatch(Options options)
    {
        string text;
        try
        {
            text = options.FilePath is not null
                ? File.ReadAllText(options.FilePath)
                : options.Text ?? Console.In.ReadToEnd();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Could not read input: {ex.Message}");
            return 1;
        }

        var histogram = new Histogram();
        histogram.AddText(text);

        if (options.SavePath is not null)
        {
            string savePath = Path.ChangeExtension(options.SavePath, ".json");
            try
            {
                HistogramStore.Save(histogram.Counts, savePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Console.Error.WriteLine($"Could not save histogram: {ex.Message}");
                return 1;
            }
        }

        IEnumerable<KeyValuePair<string, int>> entries = options.Top is { } top
            ? histogram.Top(top)
            : histogram.ByCountDescending();

        if (options.Format == "json")
        {
            var ordered = entries.ToDictionary(
                pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
            Console.WriteLine(JsonSerializer.Serialize(
                ordered, new JsonSerializerOptions { WriteIndented = true }));
        }
        else
        {
            foreach (KeyValuePair<string, int> entry in entries)
            {
                Console.WriteLine($"{entry.Key}: {entry.Value}");
            }
        }

        return 0;
    }

    // ----- Interactive mode -------------------------------------------------

    private static void RunInteractive()
    {
        var histogram = new Histogram();
        histogram.AddText(SampleLyrics.Text);

        string[] menu =
        {
            "1. Show Histogram",
            "2. Search for Word",
            "3. Save the Histogram",
            "4. Load the Histogram",
            "5. Remove Word",
            "6. Exit",
        };

        while (true)
        {
            TryClear();

            foreach (string option in menu)
            {
                Console.WriteLine(option);
            }

            int choice = ReadMenuChoice("\nPlease input your selection: ");
            switch (choice)
            {
                case 1:
                    ShowHistogram(histogram);
                    break;
                case 2:
                    SearchForWord(histogram);
                    break;
                case 3:
                    SaveHistogram(histogram);
                    break;
                case 4:
                    LoadHistogram(histogram);
                    break;
                case 5:
                    RemoveWord(histogram);
                    break;
                case 6:
                    Console.WriteLine("Option 6: Exit");
                    return;
                default:
                    Console.WriteLine("Please enter a valid choice.");
                    Pause();
                    break;
            }
        }
    }

    private static void ShowHistogram(Histogram histogram)
    {
        Console.WriteLine("\nOption 1: Show Histogram\n");

        if (histogram.UniqueWords == 0)
        {
            Console.WriteLine("\t(empty histogram)");
        }
        else if (Console.IsOutputRedirected)
        {
            // No cursor tricks or colours when output is piped to a file.
            foreach (KeyValuePair<string, int> entry in histogram.ByCountDescending())
            {
                Console.WriteLine($"\t{entry.Key}: {entry.Value}");
            }
        }
        else
        {
            const int maxBarWidth = 50;
            int maxCount = histogram.ByCountDescending().First().Value;

            foreach (KeyValuePair<string, int> entry in histogram.ByCountDescending())
            {
                int barLength = entry.Value == maxCount
                    ? maxBarWidth
                    : Math.Max(1, entry.Value * maxBarWidth / maxCount);

                Console.Write("\t" + entry.Key.PadRight(24));

                ConsoleColor savedBackground = Console.BackgroundColor;
                try
                {
                    Console.BackgroundColor = ConsoleColor.Cyan;
                    Console.Write(new string(' ', barLength));
                }
                finally
                {
                    Console.BackgroundColor = savedBackground;
                }

                Console.WriteLine(" " + entry.Value);
            }
        }

        Console.WriteLine($"\n\tUnique words: {histogram.UniqueWords}");
        Console.WriteLine($"\tTotal words:  {histogram.TotalWords}");
        Pause();
    }

    private static void SearchForWord(Histogram histogram)
    {
        Console.WriteLine("\nOption 2: Search for Word?");
        string? word = ReadNonEmptyString("Enter the word you want to search for: ");
        if (word is null)
        {
            return;
        }

        int? count = histogram.Search(word);
        Console.WriteLine(count is { } found
            ? $"The word '{word}' appears {found} times."
            : $"The word '{word}' was not found.");
        Pause();
    }

    private static void SaveHistogram(Histogram histogram)
    {
        Console.WriteLine("\nOption 3: Save Histogram");
        string? saveName = ReadNonEmptyString("input desired name: ");
        if (saveName is null)
        {
            return;
        }

        saveName = Path.ChangeExtension(saveName, ".json");
        try
        {
            HistogramStore.Save(histogram.Counts, saveName);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"Could not save histogram: {ex.Message}");
            Pause();
            return;
        }

        Console.WriteLine($"\n{histogram.UniqueWords} words saved in {saveName}");
        Pause();
    }

    private static void LoadHistogram(Histogram histogram)
    {
        Console.WriteLine("\nOption 4: Load Histogram");
        string? loadName = ReadNonEmptyString("input name of save file you wish to load: ");
        if (loadName is null)
        {
            return;
        }

        loadName = Path.ChangeExtension(loadName, ".json");

        Dictionary<string, int> loaded;
        try
        {
            loaded = HistogramStore.Load(loadName);
        }
        catch (Exception ex) when (ex is FileNotFoundException
            or InvalidDataException
            or IOException
            or UnauthorizedAccessException)
        {
            // Keep the current histogram: a failed load must not wipe it.
            Console.WriteLine($"Could not load histogram: {ex.Message}");
            Pause();
            return;
        }

        histogram.Clear();
        histogram.AddCounts(loaded);
        Console.WriteLine($"\n{histogram.UniqueWords} words were successfully loaded from {loadName}.");
        Pause();
    }

    private static void RemoveWord(Histogram histogram)
    {
        Console.WriteLine("\nOption 5: Remove Word");
        string? removeWord = ReadNonEmptyString("what word would you like to remove?  ");
        if (removeWord is null)
        {
            return;
        }

        Console.WriteLine(histogram.Remove(removeWord)
            ? $"\n{removeWord} was successfully removed."
            : $"\n<{removeWord}> was not found.");
        Pause();
    }

    // ----- Input helpers ----------------------------------------------------

    private static int ReadMenuChoice(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? line = Console.ReadLine();
            if (line is null)
            {
                return 6; // Input closed (Ctrl+D / piped EOF): exit cleanly.
            }

            if (int.TryParse(line, out int choice) && choice is >= 1 and <= 6)
            {
                return choice;
            }

            Console.WriteLine("error: invalid input detected, please enter a number from 1 to 6.");
        }
    }

    /// <summary>Reads a non-empty line, or null if input is closed.</summary>
    private static string? ReadNonEmptyString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? line = Console.ReadLine();
            if (line is null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(line))
            {
                return line;
            }

            Console.WriteLine("\nerror: invalid input detected, please enter a non-empty value.");
        }
    }

    private static void Pause()
    {
        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            return;
        }

        Console.WriteLine("\nPress any key to continue.");
        Console.ReadKey(intercept: true);
    }

    private static void TryClear()
    {
        if (Console.IsOutputRedirected)
        {
            return;
        }

        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // No console attached; just keep going without clearing.
        }
    }

    // ----- Argument parsing -------------------------------------------------

    private static bool TryParseArgs(string[] args, out Options? options)
    {
        options = null;

        if (args.Length == 0)
        {
            // Interactive mode: options stays null, but that is not --help.
            // Signal interactive mode with a non-null empty Options instead.
            options = new Options();
            // Mark interactive by leaving all inputs unset; Main checks that.
            return true;
        }

        var parsed = new Options();
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--help":
                case "-h":
                    Console.WriteLine(Usage);
                    options = null;
                    return true;
                case "--file":
                    if (!TakeValue(args, ref i, out string? file))
                    {
                        return false;
                    }

                    parsed.FilePath = file;
                    break;
                case "--text":
                    if (!TakeValue(args, ref i, out string? text))
                    {
                        return false;
                    }

                    parsed.Text = text;
                    break;
                case "--stdin":
                    parsed.FromStdin = true;
                    break;
                case "--top":
                    if (!TakeValue(args, ref i, out string? topText)
                        || !int.TryParse(topText, out int top)
                        || top < 0)
                    {
                        Console.Error.WriteLine("error: --top needs a non-negative integer.");
                        return false;
                    }

                    parsed.Top = top;
                    break;
                case "--save":
                    if (!TakeValue(args, ref i, out string? save))
                    {
                        return false;
                    }

                    parsed.SavePath = save;
                    break;
                case "--format":
                    if (!TakeValue(args, ref i, out string? format)
                        || (format != "text" && format != "json"))
                    {
                        Console.Error.WriteLine("error: --format must be 'text' or 'json'.");
                        return false;
                    }

                    parsed.Format = format;
                    break;
                default:
                    Console.Error.WriteLine($"error: unknown argument '{args[i]}'.");
                    Console.Error.WriteLine(Usage);
                    return false;
            }
        }

        int inputSources = (parsed.FilePath is not null ? 1 : 0)
            + (parsed.Text is not null ? 1 : 0)
            + (parsed.FromStdin ? 1 : 0);
        if (inputSources > 1)
        {
            Console.Error.WriteLine("error: use only one of --file, --text, or --stdin.");
            return false;
        }

        if (inputSources == 0
            && (parsed.Top is not null || parsed.SavePath is not null || parsed.Format != "text"))
        {
            Console.Error.WriteLine("error: --top, --save, and --format need an input source (--file, --text, or --stdin).");
            return false;
        }

        options = parsed;
        return true;
    }

    private static bool TakeValue(string[] args, ref int index, out string? value)
    {
        value = null;
        if (index + 1 >= args.Length)
        {
            Console.Error.WriteLine($"error: '{args[index]}' needs a value.");
            Console.Error.WriteLine(Usage);
            return false;
        }

        value = args[++index];
        return true;
    }
}
