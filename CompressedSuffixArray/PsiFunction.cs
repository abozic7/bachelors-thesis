using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedSuffixArray
{
    /// <summary>
    /// Class which contains methods used for generating Psi function from a suffix array, generating suffix array from Psi function
    /// and getting increasing sequences of Psi function.
    /// </summary>

    public class PsiFunction
    {
        public static int[] psi;
        public static int missingNumber;

        /// <summary>
        /// Generates Psi function from suffix array.
        /// </summary>
        /// <param name="suffixArray">Suffix array.</param>
        /// <param name="input">Original input.</param>
        /// <returns>Array of integers which is used as Psi function.</returns>

        public static int[] GeneratePsiFunction(int[] suffixArray, string input)
        {
            psi = new int[suffixArray.Length];
            int n = suffixArray.Length;
            Dictionary<int, int> successorIndexes = new Dictionary<int, int>();

            for (int i = 0; i < n; i++)
            {
                if(suffixArray[i] == 0)
                {
                    missingNumber = i;
                }
                if(suffixArray[i] - 1 >= 0)
                {
                    successorIndexes.Add(suffixArray[i] - 1, i);
                }
            }

            for (int i = 0; i < n; i++)
            {
                if (suffixArray[i] == n - 1 && input[n - 1].Equals('$'))
                {
                    psi[i] = -1;
                    continue;
                }

                psi[i] = successorIndexes[suffixArray[i]];
            }

            return psi;
        }

        /// <summary>
        /// Gets increasing sequences of Psi function.
        /// </summary>
        /// <param name="input">Original input.</param>
        /// <returns>Dictionary of increasing sequences divided by characters from the input.</returns>

        public static Dictionary<char, List<int>> GetIncreasingSequences(string input)
        {
            Dictionary<char, List<int>> incSequences = new Dictionary<char, List<int>>();
            List<char> alphabet = GetAlphabet(input);
            int j = 0;
            List<int> sequence = new List<int>();

            for (int i = 1; i < psi.Length; i++)
            {
                if ((i - 1) != 0)
                {
                    if (psi[i - 1] > psi[i])
                    {
                        incSequences.Add(alphabet.ElementAt(j), sequence);
                        sequence = new List<int>();
                        j++;
                    }
                }
                sequence.Add(psi[i]);
            }

            incSequences.Add(alphabet.ElementAt(j), sequence);
            return incSequences;
        }

        /// <summary>
        /// Gets alphabet from the original input.
        /// </summary>
        /// <param name="input">Original input.</param>
        /// <returns>List of characters found in input. This excludes the character '$'.</returns>

        public static List<char> GetAlphabet(string input)
        {
            List<char> alphabet = input.Distinct().ToList();
            alphabet.Sort();
            alphabet.RemoveAt(0);
            return alphabet;
        }

        /// <summary>
        /// Generates suffix array from Psi function.
        /// </summary>
        /// <param name="psi">Psi function.</param>
        /// <returns>Suffix array.</returns>

        public static int[] PsiToSA(int[] psi)
        {
            int[] SA = new int[psi.Length];
            int j = missingNumber;
            SA[j] = 0;

            for (int i = 1; i < psi.Length; i++)
            {
                j = psi[j];
                SA[j] = i;
            }

            return SA;
        }
    }
}
