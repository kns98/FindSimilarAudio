/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 * 
 * Copyright (C) 2007 Dominik Schnitzer <dominik@schnitzer.at>
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

using System;

namespace Mirage
{
    public interface IWindowFunction
    {
        void Initialize(int winsize);

        void Apply(ref float[] data, float[] audiodata, int offset);

        double[] GetWindow();
    }


    public class HammingWindow : IWindowFunction
    {
        private double[] win;
        private int winsize;

        public HammingWindow()
        {
        }

        // Initialize and setup the window
        public HammingWindow(int winsize)
        {
            Initialize(winsize);
        }

        public double[] GetWindow()
        {
            return win;
        }

        public void Initialize(int winsize)
        {
            this.winsize = winsize;
            win = new double[winsize];

            for (var i = 0; i < winsize; i++) win[i] = 0.54 - 0.46 * Math.Cos(2 * Math.PI * (i / (double)winsize));
        }

        public void Apply(ref float[] data, float[] audiodata, int offset)
        {
            for (var i = 0; i < winsize; i++) data[i] = (float)win[i] * audiodata[i + offset];
        }
    }

    public class HannWindow : IWindowFunction
    {
        private double[] win;
        private int winsize;

        public HannWindow()
        {
        }

        // Initialize and setup the window
        public HannWindow(int winsize)
        {
            Initialize(winsize);
        }

        public double[] GetWindow()
        {
            return win;
        }

        public void Initialize(int winsize)
        {
            this.winsize = winsize;
            win = new double[winsize];

            for (var i = 0; i < winsize; i++) win[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (winsize - 1)));
        }

        public void Apply(ref float[] data, float[] audiodata, int offset)
        {
            for (var i = 0; i < winsize; i++) data[i] = (float)win[i] * audiodata[i + offset];
        }
    }
}