using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DbLinq.Util
{
    public class EnglishWords
    {
        private readonly Dictionary<string, int> wordsWeights = new Dictionary<string, int>();

        private class SingularPlural
        {
            public string Singular;
            public string Plural;
        }

        public EnglishWords()
        {
            Load("EnglishWords.txt");
        }

        public void Load(string resourceName)
        {
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), resourceName))
            {
                using (var resourceReader = new StreamReader(resourceStream))
                {
                    while (!resourceReader.EndOfStream)
                    {
                        string word = resourceReader.ReadLine().Trim().ToLower();
                        if (word.Length == 0 || word[0] == '#')
                            continue;
                        int count = 1;
                        while (word.StartsWith("+"))
                        {
                            count++;
                            word = word.Substring(1);
                        }
                        if (!wordsWeights.ContainsKey(word))
                            wordsWeights[word] = count;
                        else
                            wordsWeights[word] += count;
                    }
                }
            }
        }

        public int GetWeight(string word)
        {
            if (word.Length == 1) // a letter is always 1
                return 1;
            int weight;
            wordsWeights.TryGetValue(word.ToLower(), out weight);
            return weight;
        }

        public bool Exists(string word)
        {
            return GetWeight(word) > 0;
        }

        /// <summary>
        /// using English heuristics, convert 'dogs' to 'dog',
        /// 'categories' to 'category',
        /// 'cat' remains unchanged.
        /// </summary>
        public string Singularize(string word)
        {
            if (word.Length < 2)
                return word;

            foreach (SingularPlural sp in SingularsPlurals)
            {
                string newWord = Try(word, sp.Plural, sp.Singular);
                if (newWord != null)
                    return newWord;
            }

            return word;
        }

        /// <summary>
        /// using English heuristics, convert 'dog' to 'dogs',
        /// 'bass' remains unchanged.
        /// </summary>
        public string Pluralize(string word)
        {
            if (word.Length < 2)
                return word;

            foreach (SingularPlural sp in SingularsPlurals)
            {
                string newWord = Try(word, sp.Singular, sp.Plural);
                if (newWord != null)
                    return newWord;
            }

            return word;
        }

        public string Try(string word, string ending, string newEnding)
        {
            if (word.ToLower().EndsWith(ending))
            {
                string newWord = word.Substring(0, word.Length - ending.Length) + newEnding;
                if (Exists(newWord))
                    return newWord;
            }
            return null;
        }

        private class Context
        {
            public class Split
            {
                public string Magma;
                public IList<string> Words;
                public double Note;
            }

            public IDictionary<string, Split> Splits = new Dictionary<string, Split>();
        }

        public IList<string> GetWords(string text)
        {
            var context = new Context();
            IList<string> words = new List<string>();
            int lastIndex = 0;
            for (int index = 0; index <= text.Length; index++)
            {
                if (index == text.Length || !char.IsLetterOrDigit(text[index]))
                {
                    GetMagmaWords(text.Substring(lastIndex, index - lastIndex), words, context);
                    lastIndex = index + 1;
                }
            }
            return words;
        }

        private void GetMagmaWords(string magma, IList<string> words, Context context)
        {
            foreach (var word in GetMagmaWords(magma, context))
                words.Add(word);
        }

        /// <summary>
        /// Extracts words from a "word magma" by splitting the string on every position and keep the best score.
        /// The method is recursive
        /// </summary>
        /// <param name="magma"></param>
        /// <returns></returns>
        private IList<string> GetMagmaWords(string magma, Context context)
        {
            var foundWords = new List<string>();
            if (magma.Length == 0)
                throw new ArgumentException("magma string must not be empty");
            // initalize matching
            IList<string> bestLeft = new[] { magma };
            IList<string> bestRight = new string[0];
            double bestNote = GetNote(bestLeft);
            if (bestNote > 0) // if we have something here, it is a full word, then don't look any further
                return bestLeft; // note that this may break the weight... for example toothpaste always win vs +++tooth +++paste
            // split and try
            for (int i = 1; i <= magma.Length - 1; i++)
            {
                var left = magma.Substring(0, i);
                var right = magma.Substring(i);
                IList<string> leftWords, rightWords;
                double leftNote = ComputeWords(left, out leftWords, context);
                double rightNote = ComputeWords(right, out rightWords, context);
                double note = leftNote + rightNote;
                if (note > bestNote)
                {
                    bestNote = note;
                    bestLeft = leftWords;
                    bestRight = rightWords;
                }
            }
            foundWords.AddRange(bestLeft);
            foundWords.AddRange(bestRight);
            return foundWords;
        }

        private double ComputeWords(string magma, out IList<string> words, Context context)
        {
            Context.Split split;
            if (!context.Splits.TryGetValue(magma, out split))
            {
                split = new Context.Split
                {
                    Magma = magma,
                    Words = GetMagmaWords(magma, context)
                };
                split.Note = GetNote(split.Words);
                context.Splits[magma] = split;
            }
            words = split.Words;
            return split.Note;
        }

        /// <summary>
        /// Returns a note for a list of words, with the following rules:
        /// - fewer is better
        /// - popular is better
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        private double GetNote(IList<string> words)
        {
            if (words.Count == 0)
                return 0;

            double totalWeight = 0;
            foreach (string word in words)
            {
                double weight = GetWeight(word);
                totalWeight += weight;
            }
            double averageWeight = totalWeight / words.Count;
            return averageWeight / words.Count
                * 1000; // coz it's easier to read
        }

        private void GetMagmaWords1(string magma, IList<string> words)
        {
            for (string remainingMagma = magma; remainingMagma.Length > 0; )
            {
                for (int testLength = remainingMagma.Length; testLength > 0; testLength--)
                {
                    string wordTest = remainingMagma.Substring(0, testLength);
                    if (Exists(wordTest) || testLength == 1)
                    {
                        words.Add(wordTest);
                        remainingMagma = remainingMagma.Substring(testLength);
                        break;
                    }
                }
            }
        }
        // this is at final place, since my poor resharper 3.1 gets lost with the construction

        // important: keep this from most specific to less specific
        private static SingularPlural[] SingularsPlurals =
        {
            new SingularPlural { Singular="ss", Plural="sses" },
            new SingularPlural { Singular="ch", Plural="ches" },
            new SingularPlural { Singular="sh", Plural="shes" },
            new SingularPlural { Singular="zz", Plural="zzes" },
            new SingularPlural { Singular="x", Plural="xes" },
            new SingularPlural { Singular="y", Plural="ies" },
            new SingularPlural { Singular="", Plural="s" },
        };
    }
}
