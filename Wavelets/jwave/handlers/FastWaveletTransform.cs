using System;
using math.transform.jwave.handlers.wavelets;

namespace math.transform.jwave.handlers
{
    ///
    // * Base class for the forward and reverse Fast Wavelet Transform in 1-D, 2-D,
    // * and 3-D using a specified Wavelet by inheriting class.
    // * 
    // * @date 10.02.2010 08:10:42
    // * @author Christian Scheiblich
    public class FastWaveletTransform : WaveletTransform
    {
        //   * Constructor receiving a Wavelet object.
        //   * 
        //   * @date 10.02.2010 08:10:42
        //   * @author Christian Scheiblich
        //   * @param wavelet
        //   *          object of type Wavelet; Haar02, Daub02, Coif06, ...
        public FastWaveletTransform(WaveletInterface wavelet) : base(wavelet)
        {
        } // FastWaveletTransform

        //   * Constructor receiving a Wavelet object.
        //   * 
        //   * @date 10.02.2010 08:10:42
        //   * @author Christian Scheiblich
        //   * @param wavelet
        //   *          object of type Wavelet; Haar02, Daub02, Coif06, ...
        public FastWaveletTransform(WaveletInterface wavelet, int iteration) : base(wavelet, iteration)
        {
        } // FastWaveletTransform

        //   * Performs the 1-D forward transform for arrays of dim N from time domain to
        //   * Hilbert domain for the given array using the Fast Wavelet Transform (FWT)
        //   * algorithm.
        //   * 
        //   * @date 10.02.2010 08:23:24
        //   * @author Christian Scheiblich
        //   * @see math.transform.jwave.handlers.BasicTransform#forward(double[])
        public override double[] forwardWavelet(double[] arrTime)
        {
            var arrHilb = new double[arrTime.Length];
            for (var i = 0; i < arrTime.Length; i++)
                arrHilb[i] = arrTime[i];

            var level = 0;
            var h = arrTime.Length;
            var minWaveLength = _wavelet.getWaveLength();
            if (h >= minWaveLength)
                while (h >= minWaveLength)
                {
                    var iBuf = new double[h];

                    for (var i = 0; i < h; i++)
                        iBuf[i] = arrHilb[i];

                    var oBuf = _wavelet.forward(iBuf);

                    for (var i = 0; i < h; i++)
                        arrHilb[i] = oBuf[i];

                    h = h >> 1;

                    level++;
                } // levels

            return arrHilb;
        } // forward

        //   * Performs the 1-D reverse transform for arrays of dim N from Hilbert domain
        //   * to time domain for the given array using the Fast Wavelet Transform (FWT)
        //   * algorithm and the selected wavelet.
        //   * 
        //   * @date 10.02.2010 08:23:24
        //   * @author Christian Scheiblich
        //   * @see math.transform.jwave.handlers.BasicTransform#reverse(double[])
        public override double[] reverseWavelet(double[] arrHilb)
        {
            var arrTime = new double[arrHilb.Length];

            for (var i = 0; i < arrHilb.Length; i++)
                arrTime[i] = arrHilb[i];

            var level = 0;
            var minWaveLength = _wavelet.getWaveLength();
            var h = minWaveLength;
            if (arrHilb.Length >= minWaveLength)
                while (h <= arrTime.Length && h >= minWaveLength)
                {
                    var iBuf = new double[h];

                    for (var i = 0; i < h; i++)
                        iBuf[i] = arrTime[i];

                    var oBuf = _wavelet.reverse(iBuf);

                    for (var i = 0; i < h; i++)
                        arrTime[i] = oBuf[i];

                    h = h << 1;

                    level++;
                } // levels

            return arrTime;
        } // reverse

        //   * Performs the 1-D forward transform for arrays of dim N from time domain to
        //   * Hilbert domain for the given array using the Fast Wavelet Transform (FWT)
        //   * algorithm. The number of transformation levels applied is limited by
        //   * threshold.
        //   * 
        //   * @date 15.07.2010 13:26:26
        //   * @author Thomas Haider
        //   * @date 15.08.2010 00:31:36
        //   * @author Christian Scheiblich
        //   * @see math.transform.jwave.handlers.BasicTransform#forward(double[], int)
        public override double[] forwardWavelet(double[] arrTime, int toLevel)
        {
            var arrHilb = new double[arrTime.Length];
            for (var i = 0; i < arrTime.Length; i++)
                arrHilb[i] = arrTime[i];

            var level = 0;
            var h = arrTime.Length;
            var minWaveLength = _wavelet.getWaveLength();
            if (h >= minWaveLength)
                while (h >= minWaveLength && level < toLevel)
                {
                    var iBuf = new double[h];

                    for (var i = 0; i < h; i++)
                        iBuf[i] = arrHilb[i];

                    var oBuf = _wavelet.forward(iBuf);

                    for (var i = 0; i < h; i++)
                        arrHilb[i] = oBuf[i];

                    h = h >> 1;

                    level++;
                } // levels

            return arrHilb;
        } // forward

        //   * Performs the 1-D reverse transform for arrays of dim N from Hilbert domain
        //   * to time domain for the given array using the Fast Wavelet Transform (FWT)
        //   * algorithm and the selected wavelet. The number of transformation levels
        //   * applied is limited by threshold.
        //   * 
        //   * @date 15.07.2010 13:28:06
        //   * @author Thomas Haider
        //   * @date 15.08.2010 00:31:09
        //   * @author Christian Scheiblich
        //   * @date 20.06.2011 13:03:27
        //   * @author Pol Kennel
        //   * @see math.transform.jwave.handlers.BasicTransform#reverse(double[], int)
        public override double[] reverseWavelet(double[] arrHilb, int fromLevel)
        {
            var arrTime = new double[arrHilb.Length];

            for (var i = 0; i < arrHilb.Length; i++)
                arrTime[i] = arrHilb[i];

            var level = 0;

            var minWaveLength = _wavelet.getWaveLength();

            // int h = minWaveLength; // bug ... 20110620
            var h = (int)(arrHilb.Length / Math.Pow(2, fromLevel - 1)); // added by Pol

            if (arrHilb.Length >= minWaveLength)
                while (h <= arrTime.Length && h >= minWaveLength && level < fromLevel)
                {
                    var iBuf = new double[h];

                    for (var i = 0; i < h; i++)
                        iBuf[i] = arrTime[i];

                    var oBuf = _wavelet.reverse(iBuf);

                    for (var i = 0; i < h; i++)
                        arrTime[i] = oBuf[i];

                    h = h << 1;

                    level++;
                } // levels

            return arrTime;
        } // reverse
    } // class
}