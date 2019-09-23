using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace CompressedSuffixArray
{
    /// <summary>
    /// Class which contains main method and methods for writing data structures.
    /// </summary>

    public class Program
    {
        public static int alphabetSize = 5;

        /// <summary>
        /// Method which is automatically called when starting this application.
        /// Contains generating suffix array and calls of different methods of compression.
        /// </summary>
        /// <param name="args"></param>

        public static void Main(string[] args)
        {
            Console.Write("Unesite apsolutnu putanju do datoteke: ");
            string path = Console.ReadLine();
            string input = "";
            try
            {
                input = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Console.WriteLine("\nDatoteka koju želite čitati ne postoji ili ju nije moguće otvoriti.");
                Console.ReadKey();
                Environment.Exit(-1);
            }
            int[] suffixArray = new int[input.Length];
            var watch = Stopwatch.StartNew();


            SuffixArray.SAIS.sufsort(input, suffixArray, input.Length);
            watch.Stop();
            Console.Write("SA: ");
            WriteArray(suffixArray);
            Console.WriteLine("Izgradnja SA se izvodila " + watch.ElapsedMilliseconds + " ms.");
            Console.WriteLine("SA zauzima " + GetMemorySizeOfObject(suffixArray) + " bajtova.");
            Console.WriteLine();

            //compress SA with Re-Pair
            watch.Restart();
            int[] csa = RePair.RePairMethod(suffixArray).ToArray();
            watch.Stop();
            Console.Write("CSA: ");
            WriteArray(csa);
            Console.WriteLine("Izgradnja CSA preko Re-Pair se izvodila " + watch.ElapsedMilliseconds + " ms.");
            Console.WriteLine("CSA zauzima " + GetMemorySizeOfObject(csa) + " bajtova.");
            Console.WriteLine();

            //decompress Re-Pair to SA
            watch.Restart();
            RePair.DecompressRePair(csa.ToList(), input.Length);
            watch.Stop();
            Console.WriteLine("Dekompresija Re-Pair CSA se izvodila " + watch.ElapsedMilliseconds + " ms.");
            Console.WriteLine();

            //compress Psi with Delta encoding
            watch.Restart();
            PsiFunction.GeneratePsiFunction(suffixArray, input);
            Console.Write("Psi: ");
            WriteArray(PsiFunction.psi);
            Dictionary<char, List<Int16>> delta = DeltaEncoding.DeltaEncodingCompression(input);
            Console.WriteLine("Delta:");
            WriteDictionary(delta);

            watch.Stop();
            Console.WriteLine("Izgradnja CSA preko Delta encoding se izvodila " + watch.ElapsedMilliseconds + " ms.");
            Console.WriteLine("CSA zauzima " + GetMemorySizeOfObject(delta) + " bajtova.");
            Console.WriteLine();


            //decompress Delta encoding to SA
            watch.Restart();
            int[] SA = DeltaEncoding.DecompressDeltaEncoding(delta, input.Length);
            watch.Stop();
            Console.WriteLine("Dekompresija Delta encoding CSA se izvodila " + watch.ElapsedMilliseconds + " ms.");
            Console.WriteLine();
            WriteArray(SA);

            //compress Psi with Elias-Fano
            watch.Restart();
            PsiFunction.GeneratePsiFunction(suffixArray, input);
            Console.Write("Psi: ");
            WriteArray(PsiFunction.psi);
            List<int> bitLengthOfSeq = new List<int>();
            List<int[]> bitLengthOfOrigParts = new List<int[]>();
            List<byte[]> eliasFano = EliasFano.EliasFanoCompression(input, bitLengthOfSeq, bitLengthOfOrigParts);
            watch.Stop();
            Console.WriteLine("Elias-Fano:");
            WriteListOfArrays(eliasFano);
            Console.WriteLine("Duljine bitova:");
            WriteListOfArrays(bitLengthOfOrigParts);

            Console.WriteLine("Izgradnja CSA preko Elias-Fano se izvodila " + watch.ElapsedMilliseconds + " ms.");
            Console.WriteLine("CSA zauzima " + GetMemorySizeOfObject(eliasFano) + " bajtova.");
            Console.WriteLine();


            //decompress Elias-Fano to SA
            watch.Restart();
            int[] SAElias = EliasFano.DecompressEliasFano(eliasFano, bitLengthOfSeq, bitLengthOfOrigParts);
            watch.Stop();
            Console.WriteLine("Dekompresija Elias-Fano CSA se izvodila " + watch.ElapsedMilliseconds + " ms.");
            Console.WriteLine();
            WriteArray(SAElias);
        }

        /// <summary>
        /// Writes list to console.
        /// </summary>
        /// <typeparam name="T">A type used for making this method reusable for every type of data.</typeparam>
        /// <param name="list">List we want to write to console.</param>

        public static void WriteList<T>(List<T> list)
        {
            Console.Write("[");
            foreach (var i in list)
            {
                Console.Write(i + " ");
            }
            Console.Write("]");
            Console.WriteLine();
        }

        /// <summary>
        /// Writes array to console.
        /// </summary>
        /// <typeparam name="T">A type used for making this method reusable for every type of data.</typeparam>
        /// <param name="array">Array we want to write to console.</param>

        public static void WriteArray<T>(T[] array)
        {
            Console.Write("[");
            foreach (var i in array)
            {
                Console.Write(i + " ");
            }
            Console.Write("]");
            Console.WriteLine();
        }

        /// <summary>
        /// Writes dictionary to console.
        /// </summary>
        /// <typeparam name="T">A type used for making this method reusable for every type of data. Represents key type.</typeparam>
        /// <typeparam name="U">A type used for making this method reusable for every type of data. Represents value type.</typeparam>
        /// <param name="dict">Dictionary we want to write to console.</param>

        public static void WriteDictionary<T, U>(Dictionary<T, List<U>> dict)
        {
            foreach (var key in dict.Keys)
            {
                Console.Write(key + " -> ");
                WriteList(dict[key]);
            }
        }

        /// <summary>
        /// Writes list of arrays to console.
        /// </summary>
        /// <typeparam name="T">A type used for making this method reusable for every type of data.</typeparam>
        /// <param name="list">List we want to write.</param>

        public static void WriteListOfArrays<T>(List<T[]> list)
        {
            foreach (var i in list)
            {
                foreach (var j in i)
                {
                    Console.Write(j + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Calculates memory which is used by a specific object.
        /// </summary>
        /// <param name="o">Object</param>
        /// <returns>Memory size of the object.</returns>

        public static long GetMemorySizeOfObject(object o)
        {
            long size = 0;
            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, o);
                size = s.Length;
            }

            return size;
        }
    }
}
