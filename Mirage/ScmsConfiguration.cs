/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 *
 * Copyright (C) 2007-2008 Dominik Schnitzer <dominik@schnitzer.at>
 * Changed and enhanced by Per Ivar Nerseth <perivar@nerseth.com>
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA  02110-1301, USA.
 */

namespace Mirage
{
    /// <summary>
    ///     Utility class storing a cache and Configuration variables for the Scms
    ///     distance computation.
    /// </summary>
    public class ScmsConfiguration
    {
        public ScmsConfiguration(int dimension)
        {
            Dimension = dimension;
            CovarianceLength = (Dimension * Dimension + Dimension) / 2;
            MeanDiff = new float[Dimension];
            AddInverseCovariance = new float[CovarianceLength];
        }

        public int Dimension { get; }

        public int CovarianceLength { get; }

        public float[] AddInverseCovariance { get; }

        public float[] MeanDiff { get; }
    }
}