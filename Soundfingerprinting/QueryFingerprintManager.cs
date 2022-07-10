using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FindSimilar;
using Soundfingerprinting.Dao.Entities;
using Soundfingerprinting.DbStorage;
using Soundfingerprinting.DbStorage.Entities;
using Soundfingerprinting.Hashing;

namespace Soundfingerprinting.SoundTools
{
    // for splash screen

    // for Thread.Sleep

    public static class QueryFingerprintManager
    {
        /// <summary>
        ///     Query one specific song using MinHash algorithm.
        /// </summary>
        /// <param name="signatures">Signature signatures from a song</param>
        /// <param name="dbService">DatabaseService used to query the underlying database</param>
        /// <param name="lshHashTables">Number of hash tables from the database</param>
        /// <param name="lshGroupsPerKey">Number of groups per hash table</param>
        /// <param name="thresholdTables">
        ///     Minimum number of hash tables that must be found for one signature to be considered a
        ///     candidate (0 = return all candidates, 2+ = return only exact matches)
        /// </param>
        /// <param name="queryTime">Set by the method, representing the query length</param>
        /// <param name="doSearchEverything">disregard the local sensitivity hashes and search the whole database</param>
        /// <param name="splashScreen">The "please wait" splash screen (or null)</param>
        /// <returns>Dictionary with Tracks ID's and the Query Statistics</returns>
        public static Dictionary<int, QueryStats> QueryOneSongMinHash(
            IEnumerable<bool[]> signatures,
            DatabaseService dbService,
            MinHash minHash,
            int lshHashTables,
            int lshGroupsPerKey,
            int thresholdTables,
            ref long queryTime,
            bool doSearchEverything = false,
            SplashSceenWaitingForm splashScreen = null)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var signatureCounter = 0;
            var signatureTotalCount = signatures.Count();
            var stats = new Dictionary<int, QueryStats>();
            foreach (var signature in signatures)
            {
                #region Please Wait Splash Screen Cancel Event

                // check if the user clicked cancel
                if (splashScreen.CancellationPending) break;

                #endregion

                if (signature == null) continue;

                IDictionary<int, IList<HashBinMinHash>> candidates = null;
                if (doSearchEverything)
                {
                    candidates = dbService.ReadAllFingerprints();
                }
                else
                {
                    // Compute Min Hash on randomly selected fingerprint
                    var bin = minHash.ComputeMinHashSignature(signature);

                    // Find all hashbuckets to care about
                    var hashes = minHash.GroupMinHashToLSHBuckets(bin, lshHashTables, lshGroupsPerKey);
                    var hashbuckets = hashes.Values.ToArray();

                    // Find all candidates by querying the database for those hashbuckets
                    candidates = dbService.ReadFingerprintsByHashBucketLsh(hashbuckets);
                }

                // Reduce the potential candidates list if the number of hash tables found for each signature are less than the threshold
                var potentialCandidates = SelectPotentialMatchesOutOfEntireDataset(candidates, thresholdTables);

                // get the final candidate list by only using the potential candidate list
                if (potentialCandidates.Count > 0)
                {
                    var fingerprints = dbService.ReadFingerprintById(potentialCandidates.Keys);
                    var finalCandidates = fingerprints.ToDictionary(finger => finger,
                        finger => potentialCandidates[finger.Id].Count);
                    ArrangeCandidatesAccordingToFingerprints(signature,
                        finalCandidates,
                        lshHashTables,
                        lshGroupsPerKey,
                        stats);
                }

                #region Please Wait Splash Screen Update

                // calculate a percentage between 5 and 90
                var percentage = (int)(signatureCounter / (float)signatureTotalCount * 85) + 5;
                if (splashScreen != null)
                    splashScreen.SetProgress(percentage,
                        string.Format("Searching for similar fingerprints.\n(Signature {0} of {1})",
                            signatureCounter + 1, signatureTotalCount));
                signatureCounter++;

                #endregion Updat
            }

            stopWatch.Stop();
            queryTime = stopWatch.ElapsedMilliseconds; /*Set the query Time parameter*/
            return stats;
        }

        /// <summary>
        ///     Arrange candidates according to the corresponding calculation between initial signature and actual signature
        /// </summary>
        /// <param name="f">Actual signature gathered from the song</param>
        /// <param name="potentialCandidates">Potential fingerprints returned from the database</param>
        /// <param name="lHashTables">Number of L Hash tables</param>
        /// <param name="kKeys">Number of keys per table</param>
        /// <param name="trackIdQueryStats">Result set</param>
        /// <returns>Result set</returns>
        private static Dictionary<int, QueryStats> ArrangeCandidatesAccordingToFingerprints(bool[] f,
            Dictionary<Fingerprint, int> potentialCandidates,
            int lHashTables, int kKeys, Dictionary<int, QueryStats> trackIdQueryStats)
        {
            // Most time consuming method while performing the necessary calculation
            foreach (var pair in potentialCandidates)
            {
                var fingerprint = pair.Key;
                var tableVotes = pair.Value;

                // Compute Hamming Distance of actual and read signature
                var hammingDistance = MinHash.CalculateHammingDistance(f, fingerprint.Signature) * tableVotes;
                var jaqSimilarity = MinHash.CalculateJaqSimilarity(f, fingerprint.Signature);

                // Add to sample set
                var trackId = fingerprint.TrackId;
                if (!trackIdQueryStats.ContainsKey(trackId))
                    trackIdQueryStats.Add(trackId,
                        new QueryStats(0, 0, 0, -1, -1, 0, int.MinValue, 0, int.MaxValue, int.MinValue, int.MinValue,
                            double.MaxValue));

                var stats = trackIdQueryStats[trackId];
                stats.HammingDistance += hammingDistance; // Sum hamming distance of each potential candidate
                stats.NumberOfTrackIdOccurences++; // Increment occurrence count
                stats.NumberOfTotalTableVotes += tableVotes; // Find total table votes
                stats.HammingDistanceByTrack +=
                    hammingDistance / tableVotes; // Find hamming distance by track id occurrence
                if (stats.MinHammingDistance >
                    hammingDistance / tableVotes) // Find minimal hamming distance over the entire set
                    stats.MinHammingDistance = hammingDistance / tableVotes;
                if (stats.MaxTableVote < tableVotes) // Find maximal table vote
                    stats.MaxTableVote = tableVotes;
                if (stats.Similarity > jaqSimilarity)
                    stats.Similarity = jaqSimilarity;
            }

            return trackIdQueryStats;
        }

        /// <summary>
        ///     Select potential matches out of the entire dataset
        /// </summary>
        /// <param name="dataset">Dataset to consider</param>
        /// <param name="thresholdTables">Threshold tables</param>
        /// <returns>Sub dictionary</returns>
        public static Dictionary<int, IList<HashBinMinHash>> SelectPotentialMatchesOutOfEntireDataset(
            IDictionary<int, IList<HashBinMinHash>> dataset, int thresholdTables)
        {
            var result = new Dictionary<int, IList<HashBinMinHash>>();
            if (dataset == null) return result;

            foreach (var item in dataset)
                if (item.Value.Count >= thresholdTables)
                {
                    var tables = new List<int>();
                    foreach (var hashes in item.Value)
                        if (!tables.Contains(hashes.HashTable))
                            tables.Add(hashes.HashTable);

                    if (tables.Count >= thresholdTables) result.Add(item.Key, item.Value);
                }

            return result;
        }
    }
}