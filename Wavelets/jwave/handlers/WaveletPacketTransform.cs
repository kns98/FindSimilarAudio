using System;
using math.transform.jwave.handlers.wavelets;

namespace math.transform.jwave.handlers
{
    ///
    // * Base class for the forward and reverse Wavelet Packet Transform (WPT) also
    // * called Wavelet Packet Decomposition (WPD) using a specified Wavelet by
    // * inheriting class.
    // * 
    // * @date 23.02.2010 13:44:05
    // * @author Christian Scheiblich
    // 
    public class WaveletPacketTransform : WaveletTransform
    {
        //   * Constructor receiving a Wavelet object.
        //   * 
        //   * @date 23.02.2010 13:44:05
        //   * @author Christian Scheiblich
        //   * @param wavelet
        //   *          object of type Wavelet; Haar02, Daub02, Coif06, ...
        public WaveletPacketTransform(WaveletInterface wavelet) : base(wavelet)
        {
        } // WaveletPacketTransform

        //   * Constructor receiving a Wavelet object.
        //   * 
        //   * @date 23.02.2010 13:44:05
        //   * @author Christian Scheiblich
        //   * @param wavelet
        //   *          object of type Wavelet; Haar02, Daub02, Coif06, ...
        public WaveletPacketTransform(WaveletInterface wavelet, int iteration) : base(wavelet, iteration)
        {
        } // WaveletPacketTransform

        //   * Implementation of the 1-D forward wavelet packet transform for arrays of
        //   * dim N by filtering with the longest wavelet first and then always with both
        //   * sub bands -- low and high (approximation and details) -- by the next
        //   * smaller wavelet.
        //   * 
        //   * @date 23.02.2010 13:44:05
        //   * @author Christian Scheiblich
        //   * @see math.transform.jwave.handlers.BasicTransform#forward(double[])
        public override double[] forwardWavelet(double[] arrTime)
        {
            var arrHilb = new double[arrTime.Length];
            for (var i = 0; i < arrTime.Length; i++)
                arrHilb[i] = arrTime[i];

            var level = 0;
            var k = arrTime.Length;
            var h = arrTime.Length;
            var minWaveLength = _wavelet.getWaveLength();
            if (h >= minWaveLength)
                while (h >= minWaveLength)
                {
                    var g = k / h; // 1 -> 2 -> 4 -> 8 ->...

                    for (var p = 0; p < g; p++)
                    {
                        var iBuf = new double[h];

                        for (var i = 0; i < h; i++)
                            iBuf[i] = arrHilb[i + p * h];

                        var oBuf = _wavelet.forward(iBuf);

                        for (var i = 0; i < h; i++)
                            arrHilb[i + p * h] = oBuf[i];
                    } // packets

                    h = h >> 1;

                    level++;
                } // levels

            return arrHilb;
        } // forward

        //   * Implementation of the 1-D reverse wavelet packet transform for arrays of
        //   * dim N by filtering with the smallest wavelet for all sub bands -- low and
        //   * high bands (approximation and details) -- and the by the next greater
        //   * wavelet combining two smaller and all other sub bands.
        //   * 
        //   * @date 23.02.2010 13:44:05
        //   * @author Christian Scheiblich
        //   * @see math.transform.jwave.handlers.BasicTransform#reverse(double[])
        public override double[] reverseWavelet(double[] arrHilb)
        {
            var arrTime = new double[arrHilb.Length];

            for (var i = 0; i < arrHilb.Length; i++)
                arrTime[i] = arrHilb[i];

            var level = 0;
            var minWaveLength = _wavelet.getWaveLength();
            var k = arrTime.Length;
            var h = minWaveLength;
            if (arrHilb.Length >= minWaveLength)
                while (h <= arrTime.Length && h >= minWaveLength)
                {
                    var g = k / h; //... -> 8 -> 4 -> 2 -> 1

                    for (var p = 0; p < g; p++)
                    {
                        var iBuf = new double[h];

                        for (var i = 0; i < h; i++)
                            iBuf[i] = arrTime[i + p * h];

                        var oBuf = _wavelet.reverse(iBuf);

                        for (var i = 0; i < h; i++)
                            arrTime[i + p * h] = oBuf[i];
                    } // packets

                    h = h << 1;

                    level++;
                } // levels

            return arrTime;
        } // reverse

        //   * Implementation of the 1-D forward wavelet packet transform for arrays of
        //   * dim N by filtering with the longest wavelet first and then always with both
        //   * sub bands -- low and high (approximation and details) -- by the next
        //   * smaller wavelet. Stopping at the given level.
        //   * 
        //   * @date 15.07.2010 13:43:44
        //   * @author Christian Scheiblich
        //   * @see math.transform.jwave.handlers.BasicTransform#forward(double[], int)
        public override double[] forwardWavelet(double[] arrTime, int toLevel)
        {
            var arrHilb = new double[arrTime.Length];
            for (var i = 0; i < arrTime.Length; i++)
                arrHilb[i] = arrTime[i];

            var level = 0;
            var k = arrTime.Length;
            var h = arrTime.Length;
            var minWaveLength = _wavelet.getWaveLength();
            if (h >= minWaveLength)
                while (h >= minWaveLength && level < toLevel)
                {
                    var g = k / h; // 1 -> 2 -> 4 -> 8 ->...

                    for (var p = 0; p < g; p++)
                    {
                        var iBuf = new double[h];

                        for (var i = 0; i < h; i++)
                            iBuf[i] = arrHilb[i + p * h];

                        var oBuf = _wavelet.forward(iBuf);

                        for (var i = 0; i < h; i++)
                            arrHilb[i + p * h] = oBuf[i];
                    } // packets

                    h = h >> 1;

                    level++;
                } // levels

            return arrHilb;
        } // forward

        //   * Implementation of the 1-D reverse wavelet packet transform for arrays of
        //   * dim N by filtering with the smallest wavelet for all sub bands -- low and
        //   * high bands (approximation and details) -- and the by the next greater
        //   * wavelet combining two smaller and all other sub bands. Starting from a
        //   * given level.
        //   * 
        //   * @date 15.07.2010 13:44:03
        //   * @author Christian Scheiblich
        //   * @date 20.06.2011 13:05:15
        //   * @author Pol Kennel
        //   * @see math.transform.jwave.handlers.BasicTransform#reverse(double[], int)
        public override double[] reverseWavelet(double[] arrHilb, int fromLevel)
        {
            var arrTime = new double[arrHilb.Length];

            for (var i = 0; i < arrHilb.Length; i++)
                arrTime[i] = arrHilb[i];

            var level = 0;

            var minWaveLength = _wavelet.getWaveLength();

            var k = arrTime.Length;

            // int h = minWaveLength; // bug ... 20110620
            var h = (int)(arrHilb.Length / Math.Pow(2, fromLevel - 1)); // added by Pol

            if (arrHilb.Length >= minWaveLength)
                while (h <= arrTime.Length && h >= minWaveLength && level < fromLevel)
                {
                    var g = k / h; //... -> 8 -> 4 -> 2 -> 1

                    for (var p = 0; p < g; p++)
                    {
                        var iBuf = new double[h];

                        for (var i = 0; i < h; i++)
                            iBuf[i] = arrTime[i + p * h];

                        var oBuf = _wavelet.reverse(iBuf);

                        for (var i = 0; i < h; i++)
                            arrTime[i + p * h] = oBuf[i];
                    } // packets

                    h = h << 1;

                    level++;
                } // levels

            return arrTime;
        } // reverse
    } // class
}