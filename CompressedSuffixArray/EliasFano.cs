using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedSuffixArray
{
    /// <summary>
    /// Class which contains methods used for compression and decompression of a suffix array.
    /// Compression and decompression are based on Elias-Fano algorithm.
    /// </summary>

    public class EliasFano
    {
        /// <summary>
        /// Method used for compression of a suffix array based on Elias-Fano algorithm.
        /// </summary>
        /// <param name="input">Original input.</param>
        /// <param name="bitLengthOfSeq">List of values that represent length of resulting bit sequences by increasing sequences.</param>
        /// <param name="bitLengthOfOrigParts">List of integer arrays containing information about length of leading and lower bit sequences.</param>
        /// <returns>List of bytes which represent compressed values.</returns>

        public static List<byte[]> EliasFanoCompression(string input, List<int> bitLengthOfSeq, List<int[]> bitLengthOfOrigParts)
        {
            Dictionary<char, List<int>> incSequences = PsiFunction.GetIncreasingSequences(input);

            List<bool[]> eliasFano = new List<bool[]>();
            int eliasIndex = 0;
            int m;
            int noOfBits = 0;
            int noOfLeadingBits;
            int noOfLowerBits;
            int[] bucket;
            bool[] lowerBits;
            bool[] leadingBits;
            bool[] number;
            bool[] lowerEliasFano;
            bool[] leadingEliasFano;

            foreach (var val in incSequences.Values)
            {
                int lowerPointer = 0;
                m = val[val.Count - 1];
                noOfBits = BitsToExpressNumber(val[val.Count - 1]);
                noOfLeadingBits = GetNumberOfLeadingBits(val.Count);

                bucket = new int[(int)Math.Pow(2, noOfLeadingBits)];
                noOfLowerBits = noOfBits - noOfLeadingBits;
                int[] bits = new int[2];
                bits[0] = noOfLeadingBits;
                bits[1] = noOfLowerBits;
                bitLengthOfOrigParts.Insert(eliasIndex, bits);
                lowerEliasFano = new bool[val.Count * noOfLowerBits];

                foreach (var item in val)
                {
                    number = new bool[noOfBits];
                    lowerBits = new bool[noOfLowerBits];
                    leadingBits = new bool[noOfLeadingBits];

                    number = NumberToBits(item, noOfBits);

                    int j = 0;

                    for (int i = 0; i < noOfBits; i++)
                    {
                        if(i < noOfLeadingBits)
                        {
                            leadingBits[i] = number[i];
                        }
                        else
                        {
                            lowerBits[j++] = number[i];
                        }
                    }

                    bucket[GetNumberFromBits(leadingBits)]++;

                    for (int i = 0; i < noOfLowerBits; i++)
                    {
                        lowerEliasFano[lowerPointer++] = lowerBits[i];
                    }
                }

                int sizeOfLeading = 0;
                foreach (var counter in bucket)
                {
                    sizeOfLeading += counter + 1;
                }
                leadingEliasFano = new bool[sizeOfLeading];

                GenerateLeadingSequence(bucket, leadingEliasFano);

                bool[] resultVector = GenerateResultVectorOfBits(sizeOfLeading + val.Count * noOfLowerBits,
                    leadingEliasFano, lowerEliasFano);
                
                eliasFano.Insert(eliasIndex, resultVector);
                eliasIndex++;
            }

            int f = 0;
            foreach (var seq in eliasFano)
            {
                bitLengthOfSeq.Insert(f, seq.Length);
                f++;
            }

            return EncodeToBytes(eliasFano);
        }

        /// <summary>
        /// Calculates how many bits are necessary to write the biggest value in a sequence.
        /// </summary>
        /// <param name="biggestNumber">The biggest value in a sequence.</param>
        /// <returns>Number of bits used for writing the given value.</returns>

        private static int BitsToExpressNumber(int biggestNumber)
        {
            for (int i = 1; ; i++)
            {
                if (Math.Pow(2, i) > biggestNumber)
                {
                    return i;
                }
            }
        }

        /// <summary>
        /// Calculates number of leading bits based on number of elements in a sequence.
        /// </summary>
        /// <param name="numberOfElements">Number of elements in a sequence.</param>
        /// <returns>Number of leading bits.</returns>

        private static int GetNumberOfLeadingBits(int numberOfElements)
        {
            for (int i = 1; ; i++)
            {
                if (Math.Pow(2, i) > numberOfElements)
                {
                    return i;
                }
            }
        }

        /// <summary>
        /// Converts integer to bits (array of booleans).
        /// </summary>
        /// <param name="number">Value to be converted to bits.</param>
        /// <param name="noOfBits">Number of bits to write a value.</param>
        /// <returns>Array of booleans which represent bits.</returns>

        public static bool[] NumberToBits(int number, int noOfBits)
        {
            bool[] bits = new bool[noOfBits];

            for (int i = noOfBits - 1; i >= 0; i--)
            {
                if (number % 2 == 1)
                {
                    bits[i] = true;
                }
                number /= 2;
            }

            return bits;
        }

        /// <summary>
        /// Converts bits to integer.
        /// </summary>
        /// <param name="bits">Array of booleans which represent bits.</param>
        /// <returns>Converted value.</returns>

        public static int GetNumberFromBits(bool[] bits)
        {
            double number = 0;
            for (int i = bits.Length - 1, j = 0; i >= 0; i--, j++)
            {
                if (bits[i] == true)
                {
                    number += Math.Pow(2, j);
                }
            }
            return (int)number;
        }

        /// <summary>
        /// Generates a sequence of bits from leading bits of each element in a sequence.
        /// </summary>
        /// <param name="bucket">Array of integers used for counting occurences of bit combinations.</param>
        /// <param name="leadingEliasFano">Resulting sequence of leading bits.</param>

        private static void GenerateLeadingSequence(int[] bucket, bool[] leadingEliasFano)
        {
            for (int i = bucket.Length - 1, j = leadingEliasFano.Length - 1; i >= 0; i--)
            {
                leadingEliasFano[j--] = false;
                for (int k = 0; k < bucket[i]; k++)
                {
                    leadingEliasFano[j--] = true;
                }
            }
        }

        /// <summary>
        /// Generates bit sequence from leading and lower bit sequences.
        /// </summary>
        /// <param name="size">Length of resulting bit sequence.</param>
        /// <param name="leadingEliasFano">Leading bit sequence.</param>
        /// <param name="lowerEliasFano">Lower bit sequence.</param>
        /// <returns>Resulting bit sequence.</returns>

        private static bool[] GenerateResultVectorOfBits(int size, bool[] leadingEliasFano, bool[] lowerEliasFano)
        {
            bool[] result = new bool[size];
            int j = 0;
            for (int i = 0; i < result.Length; i++)
            {
                if (i < leadingEliasFano.Length)
                {
                    result[i] = leadingEliasFano[i];
                }
                else
                {
                    result[i] = lowerEliasFano[j++];
                }
            }

            return result;
        }

        /// <summary>
        /// Converts bits to list of bytes.
        /// </summary>
        /// <param name="eliasFano">List of bits. This list is a result of Elias-Fano algorithm.</param>
        /// <returns></returns>

        public static List<byte[]> EncodeToBytes(List<bool[]> eliasFano)
        {
            bool[] eliasByte = new bool[8];
            List<byte[]> encodedEliasFano = new List<byte[]>();
            int m = 0;

            foreach (var seq in eliasFano)
            {
                int l = 0;
                int noOfFullBytes = seq.Length / 8;
                int noOfLeftoverBits = seq.Length % 8;
                int noOfFullBits = seq.Length - noOfLeftoverBits;
                byte[] encodedSeq = new byte[noOfFullBytes + 1];

                for (int i = 0; i < seq.Length; i += 8)
                {
                    if(i < noOfFullBits)
                    {
                        for (int k = 0, j = i; k < 8; k++, j++)
                        {
                            eliasByte[k] = seq[j];
                        }
                        encodedSeq[l++] = GetByteFromBits(eliasByte);
                    }
                    else
                    {
                        for (int k = 0, j = i; k < 8; k++, j++)
                        {
                            if(j < seq.Length)
                            {
                                eliasByte[k] = seq[j];
                            }
                            else
                            {
                                eliasByte[k] = false;
                            }
                        }
                        encodedSeq[l++] = GetByteFromBits(eliasByte);
                    }
                }
                encodedEliasFano.Insert(m, encodedSeq);
                m++;
            }
            return encodedEliasFano;
        }

        /// <summary>
        /// Converts 8 bits to a byte.
        /// </summary>
        /// <param name="bits">Array of bits.</param>
        /// <returns>Returns a byte.</returns>

        public static byte GetByteFromBits(bool[] bits)
        {
            byte[] bytes = BitConverter.GetBytes(GetNumberFromBits(bits));
            return bytes[0];
        }

        /// <summary>
        /// Method used for decompression of list of byte arrays based on Elias-Fano algorithm.
        /// </summary>
        /// <param name="eliasFano">List of byte arrays. Result of Elias-Fano compression.</param>
        /// <param name="bitLengthOfSeq">List of values that represent length of resulting bit sequences by increasing sequences.</param>
        /// <param name="bitLengthOfOrigParts">List of integer arrays containing information about length of leading and lower bit sequences.</param>
        /// <returns>Suffix array.</returns>

        public static int[] DecompressEliasFano(List<byte[]> eliasFano, List<int> bitLengthOfSeq, List<int[]> bitLengthOfOrigParts)
        {
            int highLowBitsSeam = 0;
            List<int> sequence;
            List<int> psi = new List<int>();
            psi.Insert(0, -1);
            int k = 1;

            for (int i = 0; i < eliasFano.Count; i++)
            {
                bool[] bitArray = ByteToBit(eliasFano[i], bitLengthOfSeq[i]);
                highLowBitsSeam = GetHigherLowerBitsSeam(bitArray, bitLengthOfOrigParts[i][0]);
                List<bool[]> leadingBits = GenerateLeadingBits(bitArray, highLowBitsSeam, bitLengthOfOrigParts[i][0]);
                List<bool[]> lowerBits = GenerateLowerBits(bitArray, highLowBitsSeam, bitLengthOfOrigParts[i][1]);
                if(bitLengthOfOrigParts[i][1] == 0)
                {
                    sequence = new List<int>();
                    int index = 0;
                    foreach (var item in leadingBits)
                    {
                        sequence.Insert(index, GetNumberFromBits(item));
                        index++;
                    }
                } else
                {
                    sequence = GenerateSequence(leadingBits, lowerBits);
                }
                foreach (var el in sequence)
                {
                    psi.Insert(k, el);
                    k++;
                }
            }
            
            return PsiFunction.PsiToSA(psi.ToArray());
        }

        /// <summary>
        /// Converts bytes to bits.
        /// </summary>
        /// <param name="byteArray">Byte array.</param>
        /// <param name="seqLength">First n bits that form Elias-Fano bit sequence.</param>
        /// <returns>Bit array.</returns>

        public static bool[] ByteToBit(byte[] byteArray, int seqLength)
        {
            bool[] result = new bool[seqLength];
            bool[] oneByte = new bool[8];
            int k = 0;

            for (int i = 0; i < byteArray.Length; i++)
            {
                if (k >= seqLength)
                {
                    break;
                }
                oneByte = NumberToBits(byteArray[i], 8);
                for (int j = 0; j < 8; j++)
                {
                    if (k >= seqLength)
                    {
                        break;
                    }
                    result[k++] = oneByte[j];
                }
            }

            return result;
        }

        /// <summary>
        /// Gets index of first bit that is a part of lower bit sequence.
        /// </summary>
        /// <param name="bitArray">Bit array.</param>
        /// <param name="noOfLeadingBits">Number of leading bits in one value.</param>
        /// <returns></returns>

        public static int GetHigherLowerBitsSeam(bool[] bitArray, int noOfLeadingBits)
        {
            int k = 0;
            int seam = 0;

            for (int i = 0; i < bitArray.Length; i++)
            {
                if (k == Math.Pow(2, noOfLeadingBits))
                {
                    seam = i;
                    break;
                }
                if (bitArray[i] == false)
                {
                    k++;
                }
            }

            if (seam == 0)
            {
                seam = bitArray.Length;
            }

            return seam;
        }

        /// <summary>
        /// Generates leading bits for every value of the original increasing sequence.
        /// </summary>
        /// <param name="bitArray">Bit array.</param>
        /// <param name="leadingLength">Length of leading bits sequence.</param>
        /// <param name="noOfLeadingBits">Number of leading bits.</param>
        /// <returns>List of leading bits.</returns>

        public static List<bool[]> GenerateLeadingBits(bool[] bitArray, int leadingLength, int noOfLeadingBits)
        {
            int iter = (int)Math.Pow(2, noOfLeadingBits);
            int[] buckets = new int[iter];
            int i = 0, counter = 0, bucketIndex = 0;
            List<bool[]> leadingBits = new List<bool[]>();
            bool[] bits = new bool[noOfLeadingBits];

            while (i < leadingLength)
            {
                if (bitArray[i] == true)
                {
                    counter++;
                }
                else
                {
                    buckets[bucketIndex] = counter;
                    bucketIndex++;
                    counter = 0;
                }
                i++;
            }

            i = 0;
            for (int j = 0; j < buckets.Length; j++)
            {
                bits = NumberToBits(j, noOfLeadingBits);
                while (buckets[j] > 0)
                {
                    leadingBits.Insert(i, bits);
                    i++;
                    buckets[j]--;
                }
            }

            return leadingBits;
        }

        /// <summary>
        /// Generates lower bits for every value of the original increasing sequence.
        /// </summary>
        /// <param name="bitArray">Bit array.</param>
        /// <param name="seam">Starting index of lower bits sequence in bit array.</param>
        /// <param name="noOfLowerBits">Number of lower bits.</param>
        /// <returns>List of lower bits.</returns>

        public static List<bool[]> GenerateLowerBits(bool[] bitArray, int seam, int noOfLowerBits)
        {
            List<bool[]> lowerBits = new List<bool[]>();
            bool[] bits;
            int k = 0;

            for (int i = seam; i < bitArray.Length; i += noOfLowerBits)
            {
                bits = new bool[noOfLowerBits];
                for (int j = 0; j < noOfLowerBits; j++)
                {
                    bits[j] = bitArray[i + j];
                }
                lowerBits.Insert(k, bits);
                k++;
            }

            return lowerBits;
        }

        /// <summary>
        /// Constructs elements of the original increasing sequence using leading and lower bits.
        /// </summary>
        /// <param name="leadingBits">List of leading bits.</param>
        /// <param name="lowerBits">List of lower bits.</param>
        /// <returns>Increasing sequence.</returns>

        public static List<int> GenerateSequence(List<bool[]> leadingBits, List<bool[]> lowerBits)
        {
            List<int> sequence = new List<int>();
            bool[] bits = new bool[leadingBits[0].Length + lowerBits[0].Length];
            int k = 0;

            for (int i = 0; i < leadingBits.Count; i++)
            {
                for (int j = 0; j < leadingBits[i].Length; j++)
                {
                    bits[k++] = leadingBits[i][j];
                }
                for (int j = 0; j < lowerBits[i].Length; j++)
                {
                    bits[k++] = lowerBits[i][j];
                }
                sequence.Insert(i, GetNumberFromBits(bits));
                k = 0;
            }

            return sequence;
        }
    }
}
