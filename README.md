# Word Histogram

[![CI](https://github.com/enonymous1/cs-word-histogram/actions/workflows/ci.yml/badge.svg)](https://github.com/enonymous1/cs-word-histogram/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)

You have a pile of text. This tells you which words show up, and how often.

Word Histogram is a small C# console app that counts words in text and
shows the result as a bar chart. Run it interactively for the menu, or
point it at a file from the command line and get the counts out.

## Quick start

Prerequisite: the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```sh
# Interactive menu over a sample text (lyrics by default,
# or the "I Have a Dream" excerpt with option 2)
dotnet run --project src/WordHistogram

# Count a file, most frequent first
dotnet run --project src/WordHistogram -- --file sample-lyrics.txt --top 10

# Count the folded-in 2021 PG2 lab sample
dotnet run --project src/WordHistogram -- --file sample-mlk.txt --top 10

# Count from stdin, as JSON
cat sample-lyrics.txt | dotnet run --project src/WordHistogram -- --stdin --format json

# Count and save the histogram for later
dotnet run --project src/WordHistogram -- --file sample-lyrics.txt --save my-counts
```

The interactive menu (the original 2024 interface, kept intact) offers:
show histogram, search for a word, save, load, remove a word, exit.
It first asks which sample text to use: the lyrics (default) or the
"I Have a Dream" excerpt folded in from the 2021 PG2 lab (see History).
Saved histograms are JSON files, so a saved count can be loaded back
in a later session.

## Sample output

Real output from `dotnet run --project src/WordHistogram -- --file sample-lyrics.txt --top 10`:

```text
We: 23
a: 16
you: 14
and: 11
Christmas: 10
merry: 10
wish: 10
bring: 9
figgy: 6
happy: 6
```

Real output from `dotnet run --project src/WordHistogram -- --file sample-mlk.txt --top 10`
(612 words, 216 unique, counted with the modern tokenizer):

```text
of: 34
the: 30
and: 21
be: 19
to: 18
a: 16
will: 16
freedom: 13
from: 12
I: 12
```

## How counting works

- Words are runs of letters or digits. Punctuation and whitespace
  separate words, except an apostrophe *inside* a word is kept, so
  "won't" counts as one word, not "won" and "t". The curly
  apostrophe (’) counts the same way.
- Counting is case-insensitive: "We", "we", and "WE" are one entry.
- In batch mode, `--top N` shows the N most frequent words, ordered
  by count descending, then word ascending.

## Project layout

```text
src/WordHistogram.Core/    Counting, the histogram model, JSON save/load
src/WordHistogram/         Console UI + argument parsing only
tests/WordHistogram.Tests/ xUnit tests for the core
sample-lyrics.txt          Demo text (the original Christmas lyrics)
sample-mlk.txt             Demo text (the 2021 PG2 lab's speech excerpt)
```

Keeping the console out of the core is the whole point of the split:
everything about counting can be tested without ever touching a
terminal, and the batch mode reuses the exact same code as the menu.

## Tests

```sh
dotnet test WordHistogram.sln
```

## Non-goals

- Not a text-analysis suite: no stemming, stop-word removal, or
  n-grams. It counts words, faithfully.
- Not a file manager: save/load work on one histogram JSON file at
  a time, in the current directory unless you give a path.

## History

Originally written in February 2024 as a single-file C# console app.
The October 2026 modernization (see [CHANGELOG.md](CHANGELOG.md))
moved it to .NET 10, split the core from the UI, added batch mode and
tests, dropped the Newtonsoft.Json dependency for built-in
`System.Text.Json`, and fixed the remove/load/tokenizer bugs the
original had. The menu and the sample lyrics are unchanged on purpose.

There is also an earlier predecessor: the August 2021 PG2 Lab 1
histogram, a .NET Framework 4.7.2 console app with the same 6-option
menu over an excerpt of Martin Luther King Jr.'s "I Have a Dream"
speech. It is not published as a separate repo — its save, load, and
remove options were print-only stubs, and its tokenizer, bar display,
and menu are all already covered, fixed, by this codebase. What was
folded in on 2026-10-30, as content only (none of that lab's git
history was imported, so its personal-email authorship does not come
with it), is the speech excerpt itself, as `sample-mlk.txt` and the
interactive menu's second sample choice, plus a test locking in the
tokenizer behaviour its punctuation needs.
