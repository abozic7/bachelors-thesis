using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedSuffixArray
{
    /// <summary>
    /// Class which contains methods used for compression and decompression of a suffix array.
    /// Compression and decompression are based on Delta encoding algorithm.
    /// </summary>
    
    public class DeltaEncoding
    {
        /// <summary>
        /// Method used for compression of a suffix array based on Delta encoding algorithm.
        /// This method is also using Psi function. See <see cref="PsiFunction.GeneratePsiFunction(int[], string)"/>.
        /// </summary>
        /// <param name="input">String which will be compressed.</param>
        /// <returns>Dictionary of compressed values divided by characters from the original input.</returns>
         
        public static Dictionary<char, List<Int16>> DeltaEncodingCompression(string input)
        {
            Dictionary<char, List<int>> incSequences = PsiFunction.GetIncreasingSequences(input);
            Dictionary<char, List<Int16>> deltaEncoding = new Dictionary<char, List<Int16>>();
            List<Int16> deltaSequence;

            foreach (var key in incSequences.Keys)
            {
                deltaSequence = new List<Int16>();
                deltaSequence.Add(Convert.ToInt16(incSequences[key].ElementAt(0)));
                deltaEncoding.Add(key, deltaSequence);
            }

            foreach (var key in incSequences.Keys)
            {
                deltaSequence = new List<Int16>();
                deltaSequence = deltaEncoding[key];
                for (int i = 0; i < incSequences[key].Count - 1; i++)
                {
                    deltaSequence.Add(Convert.ToInt16(incSequences[key].ElementAt(i + 1) - incSequences[key].ElementAt(i)));
                }
                deltaEncoding[key] = deltaSequence;
            }

            return deltaEncoding;
        }

        /// <summary>
        /// Method used for decompression of a compressed suffix array based on Delta encoding algorithm.
        /// This method is also using method which generates suffix array from Psi function.
        /// See <see cref="PsiFunction.PsiToSA(int[])"/>.
        /// </summary>
        /// <param name="delta">Dictionary of compressed values divided by characters from the original input.</param>
        /// <param name="length">Length of the original input.</param>
        /// <returns>Suffix array.</returns>

        public static int[] DecompressDeltaEncoding(Dictionary<char, List<Int16>> delta, int length)
        {
            int[] psi = new int[length];
            psi[0] = -1;
            int j = 1;

            foreach (var key in delta.Keys)
            {
                psi[j++] = Convert.ToInt32(delta[key][0]);

                for (int i = 1; i < delta[key].Count; i++)
                {
                    psi[j] = Convert.ToInt32(delta[key][i]) + psi[j - 1];
                    j++;
                }
            }

            return PsiFunction.PsiToSA(psi);
        }
    }
}
