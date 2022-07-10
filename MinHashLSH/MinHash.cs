using System;
using System.Collections.Generic;
using System.Diagnostics;

// http://blogs.msdn.com/b/spt/archive/2008/06/10/set-similarity-and-min-hash.aspx
// http://mymagnadata.wordpress.com/2011/01/04/minhash-java-implementation/
namespace SetSimilarity
{
    internal class MinHash
    {
        private const int m_numHashFunctions = 100; //Modify this parameter
        private readonly Hash[] m_hashFunctions;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="universeSize">
        ///     The number of items that can possibly exist in the universe. If you were using single RGB
        ///     pixels, the universe would be 255 * 255 * 255 = 16,581,375
        /// </param>
        /// <remarks>
        ///     The basic idea in the Min Hashing scheme is to randomly permute the rows and for each
        ///     column c(i) compute its hash value h(c(i)) as the index of the first row under the permutation that has a 1 in that
        ///     column.
        /// </remarks>
        public MinHash(int universeSize)
        {
            Debug.Assert(universeSize > 0);

            m_hashFunctions = new Hash[m_numHashFunctions];

            var r = new Random(11);
            for (var i = 0; i < m_numHashFunctions; i++)
            {
                var a = (uint)r.Next(universeSize);
                var b = (uint)r.Next(universeSize);
                var c = (uint)r.Next(universeSize);
                m_hashFunctions[i] = x => QHash((uint)x, a, b, c, (uint)universeSize);
            }
        }

        /// <summary>
        ///     Compute Similiarity
        /// </summary>
        /// <param name="set1">HashSet</param>
        /// <param name="set2">HashSet</param>
        /// <returns>A value determining how similar the sets are (1 = identical)</returns>
        /// <example>
        ///     HashSet
        ///     <String>
        ///         set1 = new HashSet
        ///         <String>
        ///             ();
        ///             set1.Add("FRANCISCO");
        ///             set1.Add("MISSION");
        ///             set1.Add("SAN");
        ///             HashSet
        ///             <String>
        ///                 set2 = new HashSet
        ///                 <String>
        ///                     ();
        ///                     set2.Add("FRANCISCO");
        ///                     set2.Add("MISSION");
        ///                     set2.Add("SAN");
        ///                     set2.Add("USA");
        ///                     MinHash minHash = new MinHash(set1.Count+set2.Count);
        ///                     Console.Out.WriteLine(minHash.Similarity(set1, set2));
        /// </example>
        public double Similarity<T>(HashSet<T> set1, HashSet<T> set2)
        {
            Debug.Assert(set1.Count > 0 && set2.Count > 0);

            var numSets = 2;
            var bitMap = BuildBitMap(set1, set2);

            var minHashValues = GetMinHashSlots(numSets, m_numHashFunctions);

            ComputeMinHashForSet(set1, 0, minHashValues, bitMap);
            ComputeMinHashForSet(set2, 1, minHashValues, bitMap);

            return ComputeSimilarityFromSignatures(minHashValues, m_numHashFunctions);
        }

        private void ComputeMinHashForSet<T>(HashSet<T> set, short setIndex, int[,] minHashValues,
            Dictionary<T, bool[]> bitArray)
        {
            var index = 0;
            foreach (var element in bitArray.Keys)
            {
                for (var i = 0; i < m_numHashFunctions; i++)
                    if (set.Contains(element))
                    {
                        var hindex = m_hashFunctions[i](index);

                        //if (hindex < minHashValues[setIndex, i])
                        if (hindex < minHashValues[setIndex, index])
                            // if current hash is smaller than the existing hash in the slot then replace with the smaller hash value
                            //minHashValues[setIndex, i] = hindex;
                            minHashValues[setIndex, index] = hindex;
                    }

                index++;
            }
        }

        private static int[,] GetMinHashSlots(int numSets, int numHashFunctions)
        {
            var minHashValues = new int[numSets, numHashFunctions];

            for (var i = 0; i < numSets; i++)
            for (var j = 0; j < numHashFunctions; j++)
                minHashValues[i, j] = int.MaxValue;
            return minHashValues;
        }

        private static int QHash(uint x, uint a, uint b, uint c, uint bound)
        {
            //Modify the hash family as per the size of possible elements in a Set
            var hashValue = (int)((a * (x >> 4) + b * x + c) & 131071);
            return Math.Abs(hashValue);
        }

        private static Dictionary<T, bool[]> BuildBitMap<T>(HashSet<T> set1, HashSet<T> set2)
        {
            var bitArray = new Dictionary<T, bool[]>();
            foreach (var item in set1) bitArray.Add(item, new bool[2] { true, false });

            foreach (var item in set2)
            {
                bool[] value;
                if (bitArray.TryGetValue(item, out value))
                    //item is present in set1
                    bitArray[item] = new bool[2] { true, true };
                else
                    //item is not present in set1
                    bitArray.Add(item, new bool[2] { false, true });
            }

            return bitArray;
        }

        private static double ComputeSimilarityFromSignatures(int[,] minHashValues, int numHashFunctions)
        {
            var identicalMinHashes = 0;
            for (var i = 0; i < numHashFunctions; i++)
                if (minHashValues[0, i] == minHashValues[1, i])
                    identicalMinHashes++;
            return 1.0 * identicalMinHashes / numHashFunctions;
        }

        public static void Test()
        {
            var set1 = new HashSet<string>();
            set1.Add("FRANCISCO");
            set1.Add("MISSION");
            set1.Add("SAN");

            var set2 = new HashSet<string>();
            set2.Add("SAN");
            set2.Add("FRANCISCO");
            set2.Add("MISSION");
            set2.Add("USA");

            var minHash = new MinHash(set1.Count + set2.Count);
            Console.Out.WriteLine(minHash.Similarity(set1, set2));
        }

        private delegate int Hash(int index);
    }
}