using System;
using System.Collections.Generic;

namespace Aquila
{
    /**
     * @file Dtw.cpp
     * 
     * DTW distance calculation - implementation.
     * 
     * @author Zbigniew Siciarz
     * @date 2007-2010
     * @version 2.5.0
     * @since 0.5.7
     */
    public class Dtw
    {
        /**
		 * Distance function prototype.
		 */
        public delegate double distanceFunction(double[] NamelessParameter1, double[] NamelessParameter2);

        /**
		 * Normalization type of result distance.
		 */
        public enum NormalizationType
        {
            NoNormalization,
            Diagonal,
            SumOfSides
        }

        /**
		 * Type of lowest-cost passes between points.
		 */
        public enum PassType
        {
            Neighbors = 0,
            Diagonals = 1
        }

        /**
		 * Library version.
		 */
        public const string VERSION = "2.5.3";

        /**
		 * Total count of Mel frequency scale filters.
		 */
        public const int MELFILTERS = 24;

        /**
		 * An array of pointers to distance functions.
		 */
        private static readonly distanceFunction[] distanceFunctions =
        {
            Functions.euclideanDistance,
            Functions.manhattanDistance,
            Functions.chebyshevDistance,
            Functions.minkowskiDistance
        };

        /**
		 * Extractor object for an input signal.
		 */
        private readonly Extractor from;

        /**
		 * Pointer to currently used distance function.
		 */
        private distanceFunction distanceFn;

        /**
		 * Current type of passes between points.
		 */
        private PassType passType;

        /**
		 * DTW point array.
		 */
        private readonly DtwPoint[][] points;

        /**
         * Creates the DTW object and sets signal feature object.
         * 
         * @param signal feature extractor object
         */
        public Dtw(Extractor signal)
        {
            from = signal;
            points = new DtwPoint[signal.GetFramesCount()][];
            distanceFn = Functions.euclideanDistance;
            passType = PassType.Neighbors;
        }

        /**
         * Computes the DTW distance between signal and pattern.
         * 
         * @param pattern feature extractor object for the pattern
         * @param normalization normalization type (default by diagonal)
         * @return double DTW distance
         */
        public double GetDistance(Extractor pattern)
        {
            return GetDistance(pattern, NormalizationType.Diagonal);
        }

        public double GetDistance(Extractor pattern, NormalizationType normalization)
        {
            CalculateLocalDistances(pattern);
            var signalSize = from.GetFramesCount();
            var patternSize = pattern.GetFramesCount();

            var top = new DtwPoint();
            var center = new DtwPoint();
            var bottom = new DtwPoint();
            var previous = new DtwPoint();

            for (var i = 1; i < signalSize; ++i)
            for (var j = 1; j < patternSize; ++j)
            {
                center = points[i - 1][j - 1];
                if (PassType.Neighbors == passType)
                {
                    top = points[i - 1][j];
                    bottom = points[i][j - 1];
                }
                else // Diagonals
                {
                    if (i > 1 && j > 1)
                    {
                        top = points[i - 2][j - 1];
                        bottom = points[i - 1][j - 2];
                    }
                    else
                    {
                        top = points[i - 1][j];
                        bottom = points[i][j - 1];
                    }
                }

                if (top.dAccumulated < center.dAccumulated)
                    previous = top;
                else
                    previous = center;

                if (bottom.dAccumulated < previous.dAccumulated)
                    previous = bottom;

                points[i][j].dAccumulated = points[i][j].dLocal + previous.dAccumulated;
                points[i][j].previous = previous;
            }

            var distance = points[signalSize - 1][patternSize - 1].dAccumulated;

            switch (normalization)
            {
                case NormalizationType.Diagonal:
                    distance /= Math.Sqrt(signalSize * signalSize + patternSize * patternSize);
                    break;
                case NormalizationType.SumOfSides:
                    distance /= signalSize + patternSize;
                    break;
                case NormalizationType.NoNormalization:
                default:
                    break;
            }

            return distance;
        }

        /**
         * Returns a const reference to DTW point array.
         * 
         * @return DTW point array
         */
        public DtwPoint[][] GetPoints()
        {
            return points;
        }

        /**
         * Chooses a distance function.
         * 
         * @param index function index in the distanceFunctions array
         */
        public void SetDistanceFunction(int index)
        {
            var functionsCount = distanceFunctions.Length;
            if (index > 0 && index < functionsCount) distanceFn = distanceFunctions[index];
        }

        /**
         * Sets the pass type.
         * 
         * @param type pass type
         */
        public void SetPassType(PassType type)
        {
            passType = type;
        }

        /**
         * Returns the lowest-cost path in the DTW array.
         * 
         * @return path
         */
        public LinkedList<KeyValuePair<int, int>> GetPath()
        {
            var path = new LinkedList<KeyValuePair<int, int>>();

            var width = points.Length;
            var height = points[0].Length;

            //C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on this variable, so pointers on this variable are left unchanged.
            var point = points[width - 1][height - 1];

            while (point.previous != null)
            {
                path.AddLast(new KeyValuePair<int, int>(point.x, point.y));

                //C++ TO C# CONVERTER WARNING: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created if it does not yet exist:
                //ORIGINAL LINE: point = point->previous;
                //point.CopyFrom(point.previous);
                point = point.previous;
            }

            return path;
        }

        /**
         * Calculates local distances array.
         * 
         * @param pattern feature extractor object of the pattern
         */
        private void CalculateLocalDistances(Extractor pattern)
        {
            var patternSize = pattern.GetFramesCount();
            for (var i = 0; i < from.GetFramesCount(); ++i)
            {
                Array.Resize(ref points[i], patternSize);
                for (var j = 0; j < patternSize; j++)
                    points[i][j] = new DtwPoint(i, j, distanceFn(from.GetVector(i), pattern.GetVector(j)));
            }
        }
    }
}