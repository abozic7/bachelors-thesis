using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedSuffixArray
{
    /// <summary>
    /// Class which contains methods used for compression and decompression of a suffix array.
    /// Compression and decompression are based on Re-Pair algorithm.
    /// </summary>
    
    public static class RePair
    {
        private static Dictionary<String, List<int>> pairIndexes = new Dictionary<String, List<int>>();
        public static Dictionary<int, String> pairSubstitute = new Dictionary<int, String>();
        private static Dictionary<String, int> pairFrequency = new Dictionary<String, int>();
        private static int substitute;

        /// <summary>
        /// Main method for Re-Pair compression algorithm.
        /// </summary>
        /// <param name="suffixArray">Array of integers that represent indexes in original input.</param>
        /// <returns>Returns compressed suffix array.</returns>

        public static List<int> RePairMethod(int[] suffixArray)
        {
            List<int> helperSuffixArray = GenerateHelperSuffixArray(suffixArray);
            substitute = helperSuffixArray.Count;

            while(true)
            {
                FillInPairIndexes(helperSuffixArray);
                FillInPairFrequency(helperSuffixArray);
                var pair = FindPairToReplace();
                if (pair == null) break;

                AddPairSubstitute(pair);
                helperSuffixArray = SubstitutePair(helperSuffixArray, pair);
                ClearStructures();
            }

            //WritePairSubstitute();
            ClearStructures();
            return helperSuffixArray;
        }

        /// <summary>
        /// Generates helper suffix array SA' used in Re-Pair algorithm.
        /// This algorithm iterates through suffix array and subtracts every two neighboring values.
        /// </summary>
        /// <param name="suffixArray">Array of integers that represent indexes in original input.</param>
        /// <returns>Returns list SA'.</returns>

        public static List<int> GenerateHelperSuffixArray(int[] suffixArray)
        {
            List<int> helperSuffixArray = new List<int>();
            helperSuffixArray.Add(suffixArray[0]);

            for (int i = 1; i < suffixArray.Length; i++)
            {
                helperSuffixArray.Add(suffixArray[i] - suffixArray[i - 1]);
            }

            return helperSuffixArray;
        }

        /// <summary>
        /// Fills in a dictionary with pairs from SA' and their position in SA'.
        /// </summary>
        /// <param name="helperSA">A list of integers that represents helper suffix array SA'.</param>

        public static void FillInPairIndexes(List<int> helperSA)
        {
            List<int> indexes;
            String key;
            for (int i = 0; i < helperSA.Count - 1; i++)
            {
                key = helperSA[i] + ":" + helperSA[i + 1];
                indexes = new List<int>();
                if (pairIndexes.ContainsKey(key))
                {
                    indexes = pairIndexes[key];
                }
                indexes.Add(i);
                pairIndexes[key] = indexes;
            }
        }

        /// <summary>
        /// Fills in a dictionary with pairs and their frequency based on SA'.
        /// </summary>
        /// <param name="helperSA">A list of integers that represents helper suffix array SA'.</param>

        public static void FillInPairFrequency(List<int> helperSA)
        {
            foreach (var entry in pairIndexes)
            {
                pairFrequency.Add(entry.Key, entry.Value.Count);
            }
        }

        /// <summary>
        /// Finds pair with a highest frequency value that will be replaced in SA' with a new value.
        /// </summary>
        /// <returns>A string that contatins pair which needs to be replaced.</returns>

        public static String FindPairToReplace()
        {
            String pair = null;
            int frequency = 1;
            foreach (var pf in pairFrequency)
            {
                if (pf.Value > 1 && pf.Value > frequency)
                {
                    pair = pf.Key;
                    frequency = pf.Value;
                }
            }

            return pair;
        }

        /// <summary>
        /// Adds pair and value that will be used to replace a pair to dictionary.
        /// </summary>
        /// <param name="pair">A string that contains pair which needs to be replaced.</param>

        public static void AddPairSubstitute(String pair)
        {
            //Console.WriteLine("Supstitucija: " + substitute);
            pairSubstitute.Add(substitute, pair);
        }

        /// <summary>
        /// Method which substitutes every occurance of a given pair in SA'.
        /// </summary>
        /// <param name="helperSA">A list of integers that represents helper suffix array SA'.</param>
        /// <param name="pair">A string that contains pair which needs to be replaced.</param>
        /// <returns>A list of integers that represents helper suffix array SA'.</returns>

        public static List<int> SubstitutePair(List<int> helperSA, String pair)
        {
            if (pairIndexes.ContainsKey(pair))
            {
                List<int> indexes = pairIndexes[pair];
                var noOfAppearence = 0;
                foreach (var ind in indexes)
                {
                    helperSA[ind - noOfAppearence] = substitute;
                    helperSA.RemoveAt(ind + 1 - noOfAppearence);
                    noOfAppearence++;
                }
            }

            substitute++;
            return helperSA;
        }

        /// <summary>
        /// Clears dictionaries which contain pair frequencies and pair indexes.
        /// </summary>

        public static void ClearStructures()
        {
            pairFrequency.Clear();
            pairIndexes.Clear();
        }

        /// <summary>
        /// Method used for decompressing compressed suffix array constructed through Re-Pair algorithm.
        /// </summary>
        /// <param name="csa">List that represents compressed suffix array.</param>
        /// <param name="saLength">Number of elements in original suffix array.</param>
        /// <returns>List that represents suffix array.</returns>

        public static List<int> DecompressRePair(List<int> csa, int saLength)
        {
            int changes;
            String pair;

            do
            {
                changes = 0;
                for (int i = 0; i < csa.Count; i++)
                {
                    if (csa[i] >= saLength)
                    {
                        pair = pairSubstitute[csa[i]];
                        string[] pairValues = pair.Split(':').ToArray();
                        csa[i] = Int32.Parse(pairValues[0]);
                        csa.Insert(i + 1, Int32.Parse(pairValues[1]));
                        changes++;
                    }
                }
            } while (changes > 0);

            pair = null;
            List<int> SA = HelperSAToSA(csa);
            return SA;
        }

        /// <summary>
        /// Generates suffix array from SA'.
        /// </summary>
        /// <param name="helperSA">A list of integers that represents helper suffix array SA'.</param>
        /// <returns>List that represents suffix array.</returns>

        public static List<int> HelperSAToSA(List<int> helperSA)
        {
            for (int i = 1; i < helperSA.Count; i++)
            {
                helperSA[i] = helperSA[i] + helperSA[i - 1];
            }
            return helperSA;
        }

        /// <summary>
        /// Method used for writing entries from pairSubstitute dictionary to console.
        /// </summary>

        public static void WritePairSubstitute()
        {
            Console.WriteLine("Rjecnik zamjena:");
            foreach (var key in pairSubstitute.Keys)
            {
                string[] pair = pairSubstitute[key].Split(':').ToArray();
                Console.WriteLine(key + " -> (" + pair[0] + ", " + pair[1] + ")");
            }
            Console.WriteLine();
        }
    }
}
