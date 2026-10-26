# Changelog

## 2026-10-26 — Modernization (on top of the 2024 history)

The original was a single 440-line `Histogram.cs` on .NET 8.0 with no tests,
no CI, and a committed IDE folder. This pass keeps the same program — a
word histogram over text, with an interactive menu — and brings it to
current standards:

- **.NET 10.0** (LTS), solution file, shared build properties.
- **Split into two projects:** `WordHistogram.Core` (counting, histogram
  model, JSON save/load — all unit-testable) and `WordHistogram` (console
  UI only). The original mixed console I/O into every operation.
- **Batch mode added:** `word-histogram --file <path>`, `--text "..."`,
  or `--stdin`, with `--top N`, `--save <path>`, and `--format text|json`.
  The original ignored its `args` entirely.
- **Newtonsoft.Json removed** in favour of built-in `System.Text.Json`
  (one less dependency; the package was the project's only one).
- **Bugs fixed:**
  - `RemoveWord` printed "successfully removed" even when the word was
    not in the histogram. `Histogram.Remove` now returns whether it
    removed anything.
  - `LoadHistogram` could return a null dictionary, lost the
    case-insensitive comparer, and printed "successfully loaded" after
    a failure or a missing file. Loading now throws on missing /
    malformed / null / negative-count files, and a failed load leaves
    the current histogram untouched.
  - The tokenizer split on apostrophes, so "won't" counted as "won"
    and "t". Internal apostrophes are now kept; all other punctuation
    separates words.
  - `ShowHistogram` used a fixed cursor column and reset the
    background to black, breaking narrow or redirected terminals.
    Bars are now scaled to a max width of 50, colours are restored
    (not reset to black), and piped output prints plain `word: count`
    lines.
  - The unused `GetLyricsFromFile()` and the duplicate hard-coded
    lyrics are gone: the demo text lives in `SampleLyrics` and in
    `sample-lyrics.txt` for `--file` runs.
- **Tests:** xUnit coverage for counting, case-insensitivity,
  contractions, save/load round-trips, and the failure cases above.
- **Repo hygiene:** `.gitignore`, MIT LICENSE, GitHub Actions CI
  (build, test, format check), and a rewritten README. The committed
  `.idea` folder was removed.
- **Build strictness, scoped:** `TreatWarningsAsErrors` is on, except
  NU1900 (NuGet vulnerability audit), which fires whenever audit data
  cannot be fetched and must not fail an otherwise good offline build.
- **Unchanged on purpose:** the interactive menu (show, search, save,
  load, remove, exit) and the sample lyrics, so the program still
  feels like the original.
