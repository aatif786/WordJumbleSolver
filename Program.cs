using System.Globalization;
using System.Runtime.InteropServices;

namespace WordJumbleSolver
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public sealed class JumbleSolver
    {
        /// <summary>
        /// Dictionary of words for efficient lookup.
        /// </summary>
        private readonly Dictionary<string, bool> _wordDictionary = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// List of valid words found from input string.
        /// </summary>
        private List<string> _validWords;

        /// <summary>
        /// Keeps track of total possible words generated from the string.
        /// </summary>
        private int _totalPossibleWordCount;

        /// <summary>
        /// Creates an instance of the <see cref="JumbleSolver"/> class lazily since it requires reading a file.
        /// </summary>
        private static readonly Lazy<JumbleSolver> Singleton = new Lazy<JumbleSolver>(() => new JumbleSolver());

        /// <summary>
        /// Gets the Singleton instance for the <see cref="JumbleSolver"/> class.
        /// </summary>
        public static JumbleSolver Instance { get { return Singleton.Value; } }

        /// <summary>
        /// Main entry point to program.
        /// </summary>
        /// <param name="args">Arguments from command line</param>
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("A single string parameter is required for a word jumble to solve.");
            }

            int totalPossibleWordCount;
            List<string> validWords = Instance.SolveWordJumble(args[0], out totalPossibleWordCount);

            foreach (string validWord in validWords)
            {
                Console.WriteLine(validWord.ToUpper());
            }

            string message = string.Format(
                CultureInfo.InvariantCulture,
                "{0} valid words found out of a possible {1} set of words.",
                validWords.Count,
                totalPossibleWordCount);
            Console.WriteLine(message + "\n");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JumbleSolver"/> class.
        /// </summary>
        private JumbleSolver()
        {
            // Initialize word dictionary from words in file
            using (StreamReader streamReader = new StreamReader("wordList.txt"))
            {
                string word;
                while ((word = streamReader.ReadLine()) != null)
                {
                    if (!_wordDictionary.ContainsKey(word))
                    {
                        _wordDictionary.Add(word, true);
                    }
                }
            }
        }

        /// <summary>
        /// Solves a word jumble given a word.
        /// </summary>
        /// <param name="word">The word jumble to solve.</param>
        /// <param name="totalPossibleWordCount">The total possible word count.</param>
        /// <returns>A list of valid words.</returns>
        public List<string> SolveWordJumble(string word, out int totalPossibleWordCount)
        {
            if (string.IsNullOrEmpty(word))
            {
                throw new ArgumentNullException("word");
            }

            _validWords = new List<string>();

            for (int i = 1; i < word.Length + 1; i++)
            {
                GenerateVariableLengthPermutations(
                    rest: word,
                    generated: string.Empty,
                    chooseLength: i);
            }

            totalPossibleWordCount = _totalPossibleWordCount;
            return _validWords;
        }

        /// <summary>
        /// Generates variable length permutations for a string. For example the "012" word will generate:
        /// 0
        /// 1
        /// 2
        /// 01
        /// 02
        /// 12
        /// 012
        /// And then proceeds to find all of the fixed permutations for that variable permutation
        /// </summary>
        /// <param name="rest">The remaining part of the string to generate.</param>
        /// <param name="generated">The part of the string that has been generated.</param>
        /// <param name="chooseLength">The choose length for the permuation.</param>
        private void GenerateVariableLengthPermutations(
            string rest,
            string generated,
            int chooseLength)
        {
            if (chooseLength == 0)
            {
                // A variable length permutation was found for string, proceed to find all fixed length
                // permutations for this string and check if each of those are valid words.
                GenerateFixedLengthPermutations(prefix: string.Empty, word: generated);

                return;
            }

            for (int i = 0; i < rest.Length - chooseLength + 1; i++)
            {
                GenerateVariableLengthPermutations(
                    rest: rest.Substring(i + 1, rest.Length - i - 1),
                    generated: generated + rest[i],
                    chooseLength: chooseLength - 1);
            }
        }

        /// <summary>
        /// Finds all fixed permutations of a string and checks to see if its a word. For example the string "012" generates:
        /// 012
        /// 021
        /// 102
        /// 120
        /// 201
        /// 210
        /// </summary>
        /// <param name="prefix">The prefix of the generated word.</param>
        /// <param name="word">The word string that is remaining.</param>
        private void GenerateFixedLengthPermutations(string prefix, string word)
        {
            if (word.Length == 1)
            {
                AddFinalWord(word);
                return;
            }

            if (word.Length == 2)
            {
                AddFinalWord(prefix + word[0] + word[1]);
                AddFinalWord(prefix + word[1] + word[0]);
                return;
            }

            for (int i = 0; i < word.Length; i++)
            {
                // Remove a character at each index to generate all possible permutations
                GenerateFixedLengthPermutations(prefix + word[i], RemoveChar(word, i));
            }
        }

        /// <summary>
        /// Removes the character at a specific index and stiches the string back togheter.
        /// </summary>
        /// <param name="word">The word</param>
        /// <param name="indexToRemove">The index to remove</param>
        /// <returns>The stiched together word with the character removed at the specified index.</returns>
        private static string RemoveChar(string word, int indexToRemove)
        {
            if (indexToRemove >= word.Length || indexToRemove < 0)
            {
                throw new ArgumentOutOfRangeException("indexToRemove");
            }

            if (indexToRemove == word.Length - 1)
            {
                return word.Substring(0, word.Length - 1);
            }

            return word.Substring(0, indexToRemove) + word.Substring(indexToRemove + 1, word.Length - indexToRemove - 1);
        }

        /// <summary>
        /// Adds word to valid word list if present in word dictionary.
        /// </summary>
        /// <param name="finalWord">The final word to add to list if it's in the dictionary.</param>
        private void AddFinalWord(string finalWord)
        {
            if (_wordDictionary.ContainsKey(finalWord))
            {
                _validWords.Add(finalWord.ToUpper());
            }

            _totalPossibleWordCount++;
        }
    }
}

// NOTES
//0,1,2,3,4,5
//------ (1)
//0
//1
//2
//3
//4
//5
//------- (2)
//0,1
//0,2
//0,3
//0,4
//0,5
//1,2
//1,3
//1,4
//1,5
//2,3
//2,4
//2,5
//3,4
//3,5
//4,5
//--------(3)

//0 + 1,2,3,4,5 (2)
//     1 + 2,3,4,5 (1)
//     2 + 3,4,5 (1)
//     3 + 4,5 (1)
//     4 + 5 (1)
//1 + 2,3,4,5 (2)
//     2 + 3,4,5 (1)
//     3 + 4,5 (1)
//     4 + 5 (1)
//2 + 3,4,5 (2)
//     3 + 4,5 (1)
//     4 + 5 (1)
//3 + 4,5 (2)
//     4 + 5 (1)
//0,1,2
//0,1,3
//0,1,4
//0,1,5
//0,2,3
//0,2,4
//0,2,5
//0,3,4
//0,3,5
//0,4,5
//1,2,3
//1,2,4
//1,2,5
//1,3,4
//1,3,5
//1,4,5
//2,3,4
//2,3,5
//2,4,5
//3,4,5
//---------(4)

//0 + 1,2,3,4,5 (3)
//    1 + 2,3,4,5 (2)
//        2 + 3,4,5 (1)
//            3 (0)
//            4 (0)
//            5 (0)
//        3 + 4,5 (1)
//            4 (0)
//            5 (0)
//        4 + 5 (1)
//            5 (0)
//    2 + 3,4,5 (2)
//        3 + 4,5 (1)
//            4 (0)
//            5 (0)
//        4 + 5 (1)
//            5 (0)
//    3 + 4,5 (2)
//        4 + 5 (1)
//            5 (0) 
//1 + 2,3,4,5 (3)
//    2 + 3,4,5 (2)
//        3 + 4,5 (1)
//            4 (0)
//            5 (0)
//        4 + 5 (1)
//            5 (0)
//    3 + 4,5 (2)
//        4 + 5 (1)
//            5 (0)
//2 + 3,4,5 (3)
//    3 + 4,5 (2)
//        4 + 5 (1)
//            5 (0)

//0,1,2,3
//0,1,2,4
//0,1,2,5
//0,1,3,4
//0,1,3,5
//0,1,4,5
//0,2,3,4
//0,2,3,5
//0,2,4,5
//0,3,4,5
//1,2,3,4
//1,2,3,5
//1,2,4,5
//1,3,4,5
//2,3,4,5		
//-------(5)
//0,1,2,3,4
//0,1,2,3,5
//0,1,2,4,5
//0,1,3,4,5
//0,2,3,4,5
//1,2,3,4,5