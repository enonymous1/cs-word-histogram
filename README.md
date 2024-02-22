# Word Histogram

This is a C# console application that manages a word histogram. The application provides several functionalities including displaying the histogram, searching for a word, saving the histogram to a file, loading the histogram from a file, and removing a word from the histogram.

## Class: Histogram

The `Histogram` class is the main class of the application. It contains the `Main` method which is the entry point of the application.

### Methods

- `Main(string[] args)`: This is the entry point of the application. It displays a menu to the user and handles the user's choice until they choose to exit.

- `GenerateWordCount(string wordSearch)`: This method generates a word count dictionary from a given string.

- `ShowHistogram(Dictionary<string, int> wordCount)`: This method displays the histogram.

- `SearchForWord(Dictionary<string, int> wordCount)`: This method searches for a word in the word count dictionary.

- `SaveHistogram(Dictionary<string, int> wordCount)`: This method saves the histogram to a file.

- `LoadHistogram()`: This method loads the histogram from a file.

- `RemoveWord(Dictionary<string, int> wordCount)`: This method removes a word from the word count dictionary.

## Usage

To run the application, simply compile and run the `Histogram.cs` file. The application will display a menu with several options. Choose an option by entering the corresponding number.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
