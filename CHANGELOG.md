# Changelog

## Unreleased — Tag-triggered release workflow

- **New: `.github/workflows/release.yml`.** Pushing a tag
  like `v1.0.0` (or a prerelease tag like `v1.0.0-rc.1`)
  now creates a GitHub Release with prebuilt binaries.
  The workflow runs the full test suite first — tag
  pushes do not trigger CI, so a broken tag must not
  become a release — then publishes self-contained
  single-file binaries for `win-x64`, `linux-x64`,
  `osx-arm64`, and `osx-x64`, zips each binary at the
  zip root, and attaches the four zips to the release
  with generated notes. Tags containing `-` are marked
  as prereleases.
- **No version bump.** The tag name is the version; no
  assembly or package version was changed in code for
  this workflow. Tag *after* pushing `main`, so the
  workflow file is present in the tagged commit.
- README updated: Installation now mentions the
  prebuilt release binaries, and a new Releases section
  documents the tag flow. No release exists yet until
  the first tag is pushed.

## 2026-10-31 — Staff review, Phase 2 (structural)

- **Argument parsing extracted and tested.** `Program.cs`
  no longer parses arguments: a new internal
  `ArgumentParser` returns a `ParseResult`
  (`Interactive` / `Batch` / `Help` / `Error`) instead of
  the old null-vs-empty-`Options` sentinel, whose
  no-args comment was wrong and whose interactive-vs-batch
  distinction depended on an unwritten validation
  invariant. The parser does no console I/O itself, so it
  is unit-tested (10 new tests) via
  `InternalsVisibleTo`; the test project now also
  references the console project for that purpose.
  Behaviour changes, all intentional: duplicate flags
  (`--file` twice, `--top` twice, and so on) now error
  instead of silently taking the last value, and a bare
  `--format text` with no input now errors like any other
  batch-only flag without an input source (it previously
  slipped through and started an interactive session,
  ignoring the flag). Exit codes are now documented in
  `--help` output: 0 success/help, 1 input/save failure,
  2 argument error.
- **Saves are atomic.** `HistogramStore.Save` writes to a
  temp file next to the destination and renames it over
  the destination, so a kill mid-write cannot leave a
  truncated histogram in place of the previous good one.
  Temp files are cleaned up in a `finally`, and a
  save-failure message now names the destination, not
  the temp file. A test asserts no `.tmp` files remain
  after an overwrite.
- **Sample texts have one source of truth.**
  `SampleLyrics.cs` and `SampleSpeech.cs` are deleted;
  the interactive menu now loads `sample-lyrics.txt`
  and `sample-mlk.txt` as embedded resources
  (`SampleTexts`). Batch and interactive runs therefore
  cannot drift apart. Tests assert both embedded texts
  load and contain their expected phrases.
- **Packability decided, not accidental.** Core and
  console both set `IsPackable=false` until a NuGet
  release with real package metadata and a release
  workflow is intentional; packing today would have
  shipped a bare 1.0.0 package.
- **SDK pinned.** A new `global.json` pins the SDK to
  the 10.0.400 feature band with `latestFeature`
  roll-forward (so 10.0.403 and later 10.0.x feature
  bands work; `latestPatch` from 10.0.100 would not have
  rolled across feature bands, which was caught while
  verifying this change). No assembly or package version
  is adopted yet — that decision is deferred to the
  first release tag, and the README claims no version.
- **Gitignore correction found in verification:**
  the Phase 1 root-only `/*.json` rule also ignored
  `global.json`, so an exception for it was added in
  this phase. Root saves are still ignored; subdirectory
  JSON files are not.
- **Tests:** 34 tests total (was 21 after Phase 1):
  10 parser tests, 2 sample-text tests, 1 atomic-save
  test, plus the Phase 1 additions. The xUnit runner
  itself still cannot run on this box (testhost
  connection timeout, as before), so CI remains the
  xUnit gate; parser behaviour was instead verified
  end-to-end through the built binary (help, unknown,
  missing value, duplicates, two sources, negative top,
  batch-only flags without input, MLK/lyrics batch
  output, curly apostrophe, appended `.json` save name,
  interactive sample counts, and overflow load).

## 2026-10-31 — Staff review, Phase 1 (low-risk hardening)

- **Fixed: crafted histogram JSON no longer crashes the loader.**
  `HistogramStore.Load` now adds merged case-variant counts with
  `checked`, so a file like `{ "We": 2147483647, "we": 1 }`
  throws `InvalidDataException` (and the interactive menu
  reports "Could not load histogram") instead of wrapping to
  a negative count and crashing in `AddCounts`.
- **Build properties are now really centralised.**
  `TargetFramework`, `Nullable`, and `ImplicitUsings` were
  removed from all three `.csproj` files; they live only in
  `Directory.Build.props`, which previously had no effect on
  them. `LangVersion` is pinned to 14 (the version shipped
  with .NET 10) instead of `latest`, so a future SDK cannot
  change language semantics under `TreatWarningsAsErrors`.
  Correction to the staff review, which said C# 13: .NET 10
  ships C# 14.
- **Interactive histogram rendering** materialises the
  by-count ordering once instead of sorting twice, with the
  bar width and label width as named constants.
- **Save/load file naming** is now one helper, `JsonPath`,
  used by batch save, interactive save, and interactive
  load. Behaviour change: a `.json` extension is appended
  if missing, not replaced — `--save report.txt` now saves
  `report.txt.json`, not `report.json`. Files saved under
  the old rule from a dotted name will need renaming to
  load under the new rule.
- **Tokenizer** now treats the curly apostrophe (U+2019)
  like ASCII `'`, so "don’t" pasted from Word or the web
  stays one word.
- **Tests:** overflow-load, curly-apostrophe, and
  `Top` tie-breaking tests added (21 tests total).
- **Repo hygiene:** `.gitignore` now ignores only root
  `/*.json` saves, not every JSON file at any depth;
  CI gains `permissions: contents: read` and a
  cancel-in-progress concurrency group; Dependabot
  (weekly, GitHub Actions + NuGet) and a minimal
  `.editorconfig` added.

## 2026-10-30 — PG2 Lab 1 fold-in (content only, no history import)

The August 2021 PG2 Lab 1 histogram is folded into this repo instead
of being published as a second histogram repo. Its code needed nothing
this codebase did not already have: the same 6-option menu, with save,
load, and remove already implemented here (they were print-only stubs
there), and its tokenizer and fixed-column bar display already fixed
here. So only its distinctive content comes across:

- **Second sample text:** the "I Have a Dream" excerpt the lab used,
  as `sample-mlk.txt` for `--file` runs and as `SampleSpeech` for the
  interactive menu, which now asks at startup whether to use the
  lyrics (default) or the speech excerpt. 612 words, 216 unique, with
  the modern tokenizer (so "God's" stays one word, unlike the lab's
  split on apostrophes).
- **Test:** a word-count test on a PG2 speech passage, locking in
  semicolon/exclamation splitting, the kept internal apostrophe, and
  case-insensitive counting.
- **Docs:** README quick start, sample output, layout, and History
  updated for the second sample.

Deliberately *not* imported: the PG2 lab's git history, which carries
a personal Gmail author. The original PG2 zip is left untouched and
is not pushed.

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
