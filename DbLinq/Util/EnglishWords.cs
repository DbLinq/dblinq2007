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
        private readonly HashSet<string> words = new HashSet<string>();

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
            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetType(), resourceName))
            {
                using (StreamReader resourceReader = new StreamReader(resourceStream))
                {
                    while (!resourceReader.EndOfStream)
                    {
                        string word = resourceReader.ReadLine().Trim().ToLower();
                        words.Add(word);
                    }
                }
            }
        }

        public bool Exists(string word)
        {
            return words.Contains(word.ToLower());
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

        public IList<string> GetWords(string text)
        {
            IList<string> words = new List<string>();
            int lastIndex = 0;
            for (int index = 0; index <= text.Length; index++)
            {
                if (index == text.Length || !char.IsLetterOrDigit(text[index]))
                {
                    GetMagmaWords(text.Substring(lastIndex, index - lastIndex), words);
                    lastIndex = index + 1;
                }
            }
            return words;
        }

        private void GetMagmaWords(string magma, IList<string> words)
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
