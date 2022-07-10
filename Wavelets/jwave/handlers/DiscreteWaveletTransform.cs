using System;
using CommonUtils;
using math.transform.jwave.handlers.wavelets;

namespace math.transform.jwave.handlers
{
    ///
    // * Base class for the forward and reverse Discret Wavelet Transform in 1-D, 2-D,
    // * and 3-D using a specified Wavelet by inheriting class.
    // *
    // * @date 23 juin 2011 15:55:33
    // * @author Pol Kennel
    public class DiscreteWaveletTransform : WaveletTransform
    {
        //   * Constructor receiving a WaveletI object.
        //   *
        //   * @date 23 juin 2011 15:54:54
        //   * @author Pol Kennel
        //   * @param wavelet
        public DiscreteWaveletTransform(WaveletInterface wavelet) : base(wavelet)
        {
        }

        //   * Constructor receiving a WaveletI object and a iteration level for forward reverse methods
        //   *
        //   * @date 23 juin 2011 15:54:54
        //   * @author Pol Kennel
        //   * @param wavelet
        public DiscreteWaveletTransform(WaveletInterface wavelet, int iteration) : base(wavelet, iteration)
        {
        }

        //   * Performs the 1-D forward transform for arrays of dim N from time domain to
        //   * Hilbert domain for the given array using the Discrete Wavelet Transform
        //   * (DWT) algorithm (identical to the Fast Wavelet Transform (FWT) in 1-D).
        //   * 
        //   * @date 24 juin 2011 13:16:00
        //   * @author Pol Kennel
        //   * @date 10.02.2010 08:23:24
        //   * @author Christian Scheiblich
        //   * @param arrTime
        //   *          coefficients of 1-D Time domain
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
        }

        //   * Performs the 1-D reverse transform for arrays of dim N from Hilbert domain
        //   * to time domain for the given array using the Discrete Wavelet Transform
        //   * (DWT) algorithm and the selected wavelet (identical to the Fast Wavelet
        //   * Transform (FWT) in 1-D).
        //   * 
        //   * @date 24 juin 2011 13:16:18
        //   * @author Pol Kennel
        //   * @date 10.02.2010 08:23:24
        //   * @author Christian Scheiblich
        //   * @param arrHilb
        //   *          coefficients of 1-D Hilbert domain
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
        //   * Hilbert domain for the given array using the Discrete Wavelet Transform
        //   * (DWT) algorithm. The number of transformation levels applied is limited by
        //   * threshold (identical to the Fast Wavelet Transform (FWT) in 1-D).
        //   * 
        //   * @date 24 juin 2011 13:18:38
        //   * @author Pol Kennel
        //   * @date 15.07.2010 13:26:26
        //   * @author Thomas Haider
        //   * @date 15.08.2010 00:31:36
        //   * @author Christian Scheiblich
        //   * @param arrTime
        //   *          coefficients of 1-D Time domain
        //   * @param toLevel
        //   *          iteration number
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
        }

        //   * Performs the 1-D reverse transform for arrays of dim N from Hilbert domain
        //   * to time domain for the given array using the Discrete Wavelet Transform
        //   * (DWT) algorithm and the selected wavelet (identical to the Fast Wavelet
        //   * Transform (FWT) in 1-D). The number of transformation levels applied is
        //   * limited by threshold.
        //   * 
        //   * @author Pol Kennel
        //   * @date 24 juin 2011 13:43:05
        //   * @author Thomas Haider
        //   * @date 15.08.2010 00:31:09
        //   * @author Christian Scheiblich
        //   * @date 20.06.2011 13:03:27
        //   * @param arrHilb
        //   *          coefficients of 1-D Hilbert domain
        //   * @param fromLevel
        //   *          iteration number
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
        }

        //   * Performs the 2-D forward transform from time domain to frequency or Hilbert
        //   * domain for a given array using the Discrete Wavelet Transform (DWT). It
        //   * corresponds to the non standard decomposition (alternates between
        //   * operations on rows and columns, in opposition of the standard
        //   * decomposition).
        //   * 
        //   * @date 24 juin 2011 13:44:51
        //   * @author Pol Kennel
        //   * @param matTime
        //   *          coefficients of 2-D time domain
        //   * @see math.transform.jwave.handlers.DiscreteWaveletTransform#forward(double[][])
        public virtual double[][] forwardWavelet(double[][] matTime)
        {
            Console.WriteLine("use bad one!");

            try
            {
                throw new Exception("dfd");
            }
            catch (Exception e)
            {
                // TODO pol should implement this try n catch
                Console.Error.WriteLine(e.StackTrace);
            }

            var noOfRows = matTime.Length;
            var noOfCols = matTime[0].Length;
            //double[][] matHilb = new double[noOfRows][noOfCols];
            var matHilb = MathUtils.CreateJaggedArray<double[][]>(noOfRows, noOfCols);
            for (var i = 0; i < matHilb[0].Length; i++)
            for (var j = 0; j < matHilb.Length; j++)
                matHilb[i][j] = matTime[i][j];

            var h = Math.Min(noOfRows, noOfCols);
            var minWaveLength = _wavelet.getWaveLength();

            if (h >= minWaveLength)
                while (h >= minWaveLength) // dimension limit
                {
                    for (var i = 0; i < h; i++) // rows processing
                    {
                        var arrTime = new double[h];
                        for (var j = 0; j < h; j++)
                            arrTime[j] = matHilb[i][j];

                        var arrHilb = _wavelet.forward(arrTime);

                        for (var j = 0; j < h; j++)
                            matHilb[i][j] = arrHilb[j];
                    }

                    for (var j = 0; j < h; j++) // columns processing
                    {
                        var arrTime = new double[h];
                        for (var i = 0; i < h; i++)
                            arrTime[i] = matHilb[i][j];

                        var arrHilb = _wavelet.forward(arrTime);

                        for (var i = 0; i < h; i++)
                            matHilb[i][j] = arrHilb[i];
                    }

                    h = h >> 1;
                }

            return matHilb;
        }

        //   * Performs the 2-D reverse transform from frequency or Hilbert or time domain
        //   * to time domain for a given array using the Discrete Wavelet Transform
        //   * (DWT). It corresponds to the non standard decomposition (alternates between
        //   * operations on rows and columns, in opposition of the standard
        //   * decomposition).
        //   * 
        //   * @date 24 juin 2011 13:51:05
        //   * @author Pol Kennel
        //   * @param matHilb
        //   *          coefficients of 2-D Hilbert domain
        //   * @see math.transform.jwave.handlers.DiscreteWaveletTransform#reverse(double[][])
        public virtual double[][] reverseWavelet(double[][] matHilb)
        {
            var noOfRows = matHilb.Length;
            var noOfCols = matHilb[0].Length;
            //double[][] matTime = new double[noOfRows][noOfCols];
            var matTime = MathUtils.CreateJaggedArray<double[][]>(noOfRows, noOfCols);
            for (var i = 0; i < matHilb[0].Length; i++)
            for (var j = 0; j < matHilb.Length; j++)
                matTime[i][j] = matHilb[i][j];

            var dim = Math.Min(noOfRows, noOfCols);
            var minWaveLength = _wavelet.getWaveLength();
            var h = minWaveLength;

            if (h >= minWaveLength)
                while (h <= dim && h >= minWaveLength) // dimension limit
                {
                    for (var i = 0; i < h; i++) // rows processing
                    {
                        var arrHilb = new double[h];
                        for (var j = 0; j < h; j++)
                            arrHilb[j] = matTime[i][j];

                        var arrTime = _wavelet.reverse(arrHilb);

                        for (var j = 0; j < h; j++)
                            matTime[i][j] = arrTime[j];
                    }

                    for (var j = 0; j < h; j++) // columns processing
                    {
                        var arrHilb = new double[h];
                        for (var i = 0; i < h; i++)
                            arrHilb[i] = matTime[i][j];

                        var arrTime = _wavelet.reverse(arrHilb);

                        for (var i = 0; i < h; i++)
                            matTime[i][j] = arrTime[i];
                    }

                    h = h << 1;
                }

            return matTime;
        }

        //   * Performs the 2-D forward transform from time domain to frequency or Hilbert
        //   * domain for a given array using the Discrete Wavelet Transform (DWT). It
        //   * corresponds to the non standard decomposition (alternates between
        //   * operations on rows and columns, in opposition of the standard
        //   * decomposition). The number of transformation levels applied is limited by
        //   * threshold.
        //   * 
        //   * @date 24 juin 2011 13:44:51
        //   * @author Pol Kennel
        //   * @param matTime
        //   *          coefficients of 2-D time domain
        //   * @param toLevel
        //   *          iteration number
        //   * @see math.transform.jwave.handlers.DiscreteWaveletTransform#forward(double[][])
        public override double[][] forwardWavelet(double[][] matTime, int toLevel)
        {
            Console.WriteLine("use good one!");

            var noOfRows = matTime.Length;
            var noOfCols = matTime[0].Length;
            //double[][] matHilb = new double[noOfRows][noOfCols];
            var matHilb = MathUtils.CreateJaggedArray<double[][]>(noOfRows, noOfCols);
            for (var i = 0; i < matHilb[0].Length; i++)
            for (var j = 0; j < matHilb.Length; j++)
                matHilb[i][j] = matTime[i][j];

            var level = 0;
            var h = Math.Min(noOfRows, noOfCols);
            var minWaveLength = _wavelet.getWaveLength();

            if (h >= minWaveLength)
                while (h >= minWaveLength && level < toLevel) // levels
                {
                    for (var i = 0; i < h; i++) // rows processing
                    {
                        var arrTime = new double[h];
                        for (var j = 0; j < h; j++)
                            arrTime[j] = matHilb[i][j];

                        var arrHilb = _wavelet.forward(arrTime);

                        for (var j = 0; j < h; j++)
                            matHilb[i][j] = arrHilb[j];
                    }

                    for (var j = 0; j < h; j++) // columns processing
                    {
                        var arrTime = new double[h];
                        for (var i = 0; i < h; i++)
                            arrTime[i] = matHilb[i][j];

                        var arrHilb = _wavelet.forward(arrTime);

                        for (var i = 0; i < h; i++)
                            matHilb[i][j] = arrHilb[i];
                    }

                    h = h >> 1;
                    level++;
                }

            return matHilb;
        }

        //   * Performs the 2-D reverse transform from frequency or Hilbert or time domain
        //   * to time domain for a given array using the Discrete Wavelet Transform
        //   * (DWT). It corresponds to the non standard decomposition (alternates between
        //   * operations on rows and columns, in opposition of the standard
        //   * decomposition). The number of transformation levels applied is limited by
        //   * threshold.
        //   * 
        //   * @date 24 juin 2011 13:51:05
        //   * @author Pol Kennel
        //   * @param matHilb
        //   *          coefficients of 2-D Hilbert domain
        //   * @param fromLevel
        //   *          iteration number
        //   * @see math.transform.jwave.handlers.DiscreteWaveletTransform#reverse(double[][])
        public override double[][] reverseWavelet(double[][] matHilb, int fromLevel)
        {
            var noOfRows = matHilb.Length;
            var noOfCols = matHilb[0].Length;
            //double[][] matTime = new double[noOfRows][noOfCols];
            var matTime = MathUtils.CreateJaggedArray<double[][]>(noOfRows, noOfCols);

            for (var i = 0; i < matHilb[0].Length; i++)
            for (var j = 0; j < matHilb.Length; j++)
                matTime[i][j] = matHilb[i][j];

            var level = 0;
            var dim = Math.Min(noOfRows, noOfCols);
            var minWaveLength = _wavelet.getWaveLength();
            var h = (int)(dim / Math.Pow(2, fromLevel - 1));

            if (h >= minWaveLength)
                while (h <= dim && h >= minWaveLength && level < fromLevel) // levels
                {
                    for (var i = 0; i < h; i++) // rows processing
                    {
                        var arrHilb = new double[h];
                        for (var j = 0; j < h; j++)
                            arrHilb[j] = matTime[i][j];

                        var arrTime = _wavelet.reverse(arrHilb);

                        for (var j = 0; j < h; j++)
                            matTime[i][j] = arrTime[j];
                    }

                    for (var j = 0; j < h; j++) // columns processing
                    {
                        var arrHilb = new double[h];
                        for (var i = 0; i < h; i++)
                            arrHilb[i] = matTime[i][j];

                        var arrTime = _wavelet.reverse(arrHilb);

                        for (var i = 0; i < h; i++)
                            matTime[i][j] = arrTime[i];
                    }

                    h = h << 1;
                    level++;
                }

            return matTime;
        }

        //   * 
        //   * TODO pol explainMeShortly
        //   *
        //   * @date 24 juin 2011 14:04:00
        //   * @author pol
        //   * @see math.transform.jwave.handlers.BasicTransform#forward(double[][][])
        public virtual double[][][] forwardWavelet(double[][][] spcTime)
        {
            // TODO
            return spcTime;
        }

        //   * 
        //   * TODO pol explainMeShortly
        //   *
        //   * @date 24 juin 2011 14:04:18
        //   * @author pol
        //   * @see math.transform.jwave.handlers.BasicTransform#reverse(double[][][])
        public virtual double[][][] reverseWavelet(double[][][] spcHilb)
        {
            // TODO
            return spcHilb;
        }
    }
}