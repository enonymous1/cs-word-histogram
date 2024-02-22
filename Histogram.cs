using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WordHistogram
{
    class Histogram
    {
        static void Main(string[] args)
        {
            // Define the option number for exiting the program
            const int exitOption = 6;

            // Define the menu options
            string[] menu = new string[] { "1. Show Histogram", "2. Search for Word", "3. Save the Histogram", "4. Load the Histogram", "5. Remove Word", "6. Exit" };

            // Define the prompt for user input
            string prompt = "\nPlease input your selection: ";

            // Get the lyrics for the word search
            // This can be swapped with GetLyricsFromFile() to read from a file
            string wordSearch = GetLyrics();

            // Generate the word count dictionary from the lyrics
            Dictionary<string, int> wordCount = GenerateWordCount(wordSearch);

            // Initialize the menu choice
            int menuChoice = 0;

            // Continue to display the menu and handle the user's choice until they choose to exit
            while (menuChoice != exitOption)
            {
                // Clear the console
                Console.Clear();

                // Display the menu options and read the user's selection
                DisplayOptionsAndReadSelection(prompt, menu, out menuChoice);

                // Handle the user's choice
                switch (menuChoice)
                {
                    case 1:
                        // Show the histogram
                        ShowHistogram(wordCount);
                        break;
                    case 2:
                        // Search for a word
                        SearchForWord(wordCount);
                        break;
                    case 3:
                        // Save the histogram
                        SaveHistogram(wordCount);
                        break;
                    case 4:
                        // Load the histogram
                        wordCount = LoadHistogram();
                        break;
                    case 5:
                        // Remove a word
                        RemoveWord(wordCount);
                        break;
                    case 6:
                        // Exit the program
                        Console.WriteLine("Option 6: Exit");
                        return;
                    default:
                        // Handle invalid choices
                        Console.WriteLine("Please enter a valid choice.");
                        break;
                }
            }
        }

        // This method generates a word count dictionary from a given string
        static Dictionary<string, int> GenerateWordCount(string wordSearch)
        {
            // Define the characters that will be used as delimiters when splitting the string into words
            char[] delimiterChars = { ' ', ',', '.', ':', '\'', '"', '\n', '\t', '\r' };

            // Split the input string into an array of words, removing any empty entries
            string[] wordsArray = wordSearch.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);

            // Convert the array of words into a list and sort it
            List<string> wordsList = new List<string>(wordsArray);
            wordsList.Sort();

            // Initialize a new dictionary to hold the word count, ignoring case when comparing keys
            Dictionary<string, int> wordCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Loop through each word in the sorted list
            foreach (var w in wordsList)
            {
                // If the word is already in the dictionary, increment its count
                if (wordCount.ContainsKey(w))
                {
                    wordCount[w]++;
                }
                else
                {
                    // If the word is not in the dictionary, add it with a count of 1
                    wordCount.Add(w, 1);
                }
            }

            // Return the word count dictionary
            return wordCount;
        }
        
        // This method displays the histogram
        static void ShowHistogram(Dictionary<string, int> wordCount)
        {
            // Display the option to the user
            Console.WriteLine("\nOption 1: Show Histogram\n");

            // Loop through each key-value pair in the word count dictionary
            foreach (KeyValuePair<string, int> words in wordCount)
            {
                // Display the word (key) to the user
                Console.Write("\t" + words.Key);

                // Move the cursor to the 27th column
                Console.CursorLeft = 27;

                // Change the background color to cyan
                Console.BackgroundColor = ConsoleColor.Cyan;

                // Display a space for each count of the word
                for (int i = 0; i < words.Value; i++)
                {
                    Console.Write(" ");
                }

                // Reset the background color to black
                Console.BackgroundColor = ConsoleColor.Black;

                // Display the count of the word (value)
                Console.WriteLine(words.Value);
            }

            // Display the total number of unique words in the histogram
            Console.WriteLine("\n\tWord count:  " + wordCount.Count());

            // Prompt the user to press any key to continue
            Console.WriteLine("\nPress any key to continue.");

            // Wait for the user to press a key
            Console.ReadKey();
        }

        // This method searches for a word in the word count dictionary
        static void SearchForWord(Dictionary<string, int> wordCount)
        {
            // Display the option to the user
            Console.WriteLine("\nOption 2: Search for Word?");

            // Initialize a string to hold the word to be searched
            string word = string.Empty;

            // Prompt the user to enter the word they want to search for
            // The entered word is stored in the 'word' variable
            PromptAndValidateNonEmptyString("Enter the word you want to search for: ", ref word);

            // Check if the word is in the dictionary
            if (wordCount.ContainsKey(word))
            {
                // If the word is found, display the count of that word
                Console.WriteLine($"The word '{word}' appears {wordCount[word]} times.");
            }
            else
            {
                // If the word is not found, inform the user
                Console.WriteLine($"The word '{word}' was not found.");
            }

            // Prompt the user to press any key to continue
            Console.WriteLine("\nPress any key to continue.");

            // Wait for the user to press a key
            Console.ReadKey();
        }

        // This method saves the histogram to a file
        static void SaveHistogram(Dictionary<string, int> wordCount)
        {
            // Display the option to the user
            Console.WriteLine("\nOption 3: Save Histogram");

            // Initialize a string to hold the name of the file to save
            string saveName = string.Empty;

            // Prompt the user to enter the name of the file they want to save
            // The entered name is stored in the 'saveName' variable
            PromptAndValidateNonEmptyString("input desired name: ", ref saveName);

            // Change the extension of the file name to .json
            saveName = Path.ChangeExtension(saveName, ".json");

            // Create a new StreamWriter to write to the file
            using (StreamWriter saver = File.CreateText(saveName))
            {
                // Create a new JsonTextWriter to write JSON to the file
                using (Newtonsoft.Json.JsonTextWriter jsonWriter = new Newtonsoft.Json.JsonTextWriter(saver))
                {
                    // Set the formatting of the JsonTextWriter to indented
                    jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;

                    // Create a new JsonSerializer
                    Newtonsoft.Json.JsonSerializer ser = new Newtonsoft.Json.JsonSerializer();

                    // Serialize the wordCount dictionary to JSON and write it to the file
                    ser.Serialize(jsonWriter, wordCount);

                    // Flush the JsonTextWriter to ensure all data is written to the file
                    jsonWriter.Flush();
                }
            }

            // Display the number of words that were saved to the file
            Console.WriteLine("\n" + wordCount.Count() + " words saved in " + saveName);

            // Prompt the user to press any key to continue
            Console.WriteLine("\nPress any key to continue.");

            // Wait for the user to press a key
            Console.ReadKey();
        }

        // This method loads the histogram from a file
        static Dictionary<string, int> LoadHistogram()
        {
            // Initialize a new dictionary to hold the word count
            Dictionary<string, int> wordCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Display the option to the user
            Console.WriteLine("\nOption 4: Load Histogram");

            // Initialize a string to hold the name of the file to load
            string loadName = string.Empty;

            // Prompt the user to enter the name of the file they want to load
            // The entered name is stored in the 'loadName' variable
            PromptAndValidateNonEmptyString("input name of save file you wish to load: ", ref loadName);

            // Change the extension of the file name to .json
            loadName = Path.ChangeExtension(loadName, ".json");

            // Check if the file exists
            if (File.Exists(loadName))
            {
                // If the file exists, read its content into a string
                string jsonText = File.ReadAllText(loadName);

                // Try to deserialize the content of the file into a dictionary
                try
                {
                    wordCount = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonText);
                }
                catch (Exception ex)
                {
                    // If an exception occurs during deserialization, display the exception to the user
                    Console.WriteLine("Exception: " + ex);
                }
            }
            else
            {
                // If the file does not exist, inform the user
                Console.WriteLine("File doesn't exist");
            }

            // Display the number of words that were loaded from the file
            Console.WriteLine("\n" + wordCount.Count() + " words were successfully loaded from " + loadName + ".");

            // Prompt the user to press any key to continue
            Console.WriteLine("\nPress any key to continue.");

            // Wait for the user to press a key
            Console.ReadKey();

            // Return the loaded word count dictionary
            return wordCount;
        }

        // This method removes a word from the word count dictionary
        static void RemoveWord(Dictionary<string, int> wordCount)
        {
            // Display the option to the user
            Console.WriteLine("\nOption 5: Remove Word");

            // Initialize a string to hold the word to be removed
            string removeWord = string.Empty;

            // Prompt the user to enter the word they want to remove
            // The entered word is stored in the 'removeWord' variable
            PromptAndValidateNonEmptyString("what word would you like to remove?  ", ref removeWord);

            // Try to remove the word from the dictionary
            // If the word is not found in the dictionary, display a message to the user
            if(!wordCount.Remove(removeWord))
            {
                Console.WriteLine("<" + removeWord + ">" + " was not found.");
            }

            // If the word was successfully removed, display a success message to the user
            Console.WriteLine("\n" + removeWord + " was successfully removed.");

            // Prompt the user to press any key to continue
            Console.WriteLine("\nPress any key to continue.");

            // Wait for the user to press a key
            Console.ReadKey();
        }
        
        // This method reads an integer input from the user within a specified range
        static int PromptAndValidateIntegerInRange(string prompt, int min, int max)
        {
            // Display the prompt to the user
            Console.Write(prompt);

            // Initialize a variable to store the user's choice
            int choice = 0;

            // Try to parse the user's input as an integer
            bool readSuccess = int.TryParse(Console.ReadLine(), out choice);

            // Continue to prompt the user until they provide a valid integer within the specified range
            while (!readSuccess || choice < min || choice > max)
            {
                // Display an error message
                Console.WriteLine("error: invalid input detected");

                // Display the prompt again
                Console.WriteLine(prompt);

                // Ask the user to enter a valid input
                Console.WriteLine("please enter a valid input");

                // Try to parse the user's input again
                readSuccess = int.TryParse(Console.ReadLine(), out choice);
            }

            // Return the valid integer input
            return choice;
        }
        
        // This method reads a string input from the user
        static void PromptAndValidateNonEmptyString(string prompt, ref string value)
        {
            // Display the prompt to the user
            Console.Write(prompt);

            // Read the user's input into a temporary string
            string tempString = Console.ReadLine();

            // Continue to prompt the user until they provide a non-empty input
            while (string.IsNullOrWhiteSpace(tempString))
            {
                // Display an error message
                Console.WriteLine("\nerror: invalid input detected");

                // Display the prompt again
                Console.WriteLine(prompt);

                // Ask the user to enter a valid input
                Console.Write("please enter a valid input:  ");

                // Read the user's input again
                tempString = Console.ReadLine();
            }

            // Assign the valid input to the passed in reference string
            value = tempString;
        }

        // This method displays a list of options to the user and reads their selection
        static void DisplayOptionsAndReadSelection(string prompt, string[] options, out int selection)
        {
            // Loop through each option in the options array
            for (int i = 0; i < options.Length; i++)
            {
                // Display the current option to the user
                Console.WriteLine(options[i]);
            }

            // Read the user's selection, ensuring it is a valid integer within the range of options
            selection = PromptAndValidateIntegerInRange(prompt, 1, options.Length);
        }

        // This method returns a string containing the lyrics of a song
        static string GetLyrics()
        {
            // The lyrics of the song are stored in a string
            string text = "We wish you a merry Christmas\n" +
                          "We wish you a merry Christmas\n" +
                          "We wish you a merry Christmas and a happy new year\n" +
                          "Good tidings we bring to you and your kin\n" +
                          "We wish you a merry Christmas and a happy new year\n" +
                          "Oh, bring us some figgy pudding\n" +
                          "Oh, bring us some figgy pudding\n" +
                          "Oh, bring us some figgy pudding\nAnd bring it right here\n" +
                          "Good tidings we bring to you and your kin\n" +
                          "We wish you a merry Christmas and a happy new year\n" +
                          "We won't go until we get some\n" +
                          "We won't go until we get some\n" +
                          "We won't go until we get some\n" +
                          "So bring it right here\n" +
                          "Good tidings we bring to you and your kin\n" +
                          "We wish you a merry Christmas and a happy new year\n" +
                          "We all like our figgy pudding\n" +
                          "We all like our figgy pudding\n" +
                          "We all like our figgy pudding\n" +
                          "With all its good cheers\n" +
                          "Good tidings we bring to you and your kin\n" +
                          "We wish you a merry Christmas and a happy new year\n" +
                          "We wish you a merry Christmas\n" +
                          "We wish you a merry Christmas\n" +
                          "We wish you a merry Christmas and a happy new year";

            // The method returns the lyrics
            return text;
        }

        // This method reads a file and returns its content as a string
        static string GetLyricsFromFile()
        {
            // Declare a string to hold the content of the file
            string speech;

            // Use a StreamReader to read the file
            using (StreamReader reads = new StreamReader("lyrics.csv"))
            {
                // Read the entire content of the file into the string
                speech = reads.ReadToEnd();
            }

            // Return the content of the file
            return speech;
        }
    }
}
