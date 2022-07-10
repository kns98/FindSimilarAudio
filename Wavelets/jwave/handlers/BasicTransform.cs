using CommonUtils;
using math.transform.jwave.types;

namespace math.transform.jwave.handlers
{
    ///
    // * Basic Wave for transformations like Fast Fourier Transform (FFT), Fast
    // * Wavelet Transform (FWT), Fast Wavelet Packet Transform (WPT), or Discrete
    // * Wavelet Transform (DWT). Naming of this class due to en.wikipedia.org; to
    // * write Fourier series in terms of the 'basic waves' of function: e^(2*pi*i*w).
    // * 
    // * @date 08.02.2010 11:11:59
    // * @author Christian Scheiblich
    // 
    public abstract class BasicTransform : TransformInterface
    {
        //   * Constructor; does nothing
        //   * 
        //   * @date 08.02.2010 11:11:59
        //   * @author Christian Scheiblich
        protected internal BasicTransform()
        {
        } // BasicTransform

        //   * Performs the forward transform from time domain to frequency or Hilbert
        //   * domain for a given array depending on the used transform algorithm by
        //   * inheritance.
        //   * 
        //   * @date 10.02.2010 08:23:24
        //   * @author Christian Scheiblich
        //   * @param arrTime
        //   *          coefficients of 1-D time domain
        //   * @return coefficients of 1-D frequency or Hilbert domain
        public abstract double[] forward(double[] arrTime);

        //   * Performs the reverse transform from frequency or Hilbert domain to time
        //   * domain for a given array depending on the used transform algorithm by
        //   * inheritance.
        //   * 
        //   * @date 10.02.2010 08:23:24
        //   * @author Christian Scheiblich
        //   * @param arrFreq
        //   *          coefficients of 1-D frequency or Hilbert domain
        //   * @return coefficients of 1-D time domain
        public abstract double[] reverse(double[] arrFreq);

        //   * Performs the 2-D forward transform from time domain to frequency or Hilbert
        //   * domain for a given array depending on the used transform algorithm by
        //   * inheritance.
        //   * 
        //   * @date 10.02.2010 11:00:29
        //   * @author Christian Scheiblich
        //   * @param matTime
        //   *          coefficients of 2-D time domain
        //   * @return coefficients of 2-D frequency or Hilbert domain
        public virtual double[][] forward(double[][] matTime)
        {
            var noOfRows = matTime.Length;
            var noOfCols = matTime[0].Length;

            //double[][] matHilb = new double[noOfRows][noOfCols];
            var matHilb = MathUtils.CreateJaggedArray<double[][]>(noOfRows, noOfCols);

            for (var i = 0; i < noOfRows; i++)
            {
                var arrTime = new double[noOfCols];

                for (var j = 0; j < noOfCols; j++)
                    arrTime[j] = matTime[i][j];

                var arrHilb = forward(arrTime);

                for (var j = 0; j < noOfCols; j++)
                    matHilb[i][j] = arrHilb[j];
            } // rows

            for (var j = 0; j < noOfCols; j++)
            {
                var arrTime = new double[noOfRows];

                for (var i = 0; i < noOfRows; i++)
                    arrTime[i] = matHilb[i][j];

                var arrHilb = forward(arrTime);

                for (var i = 0; i < noOfRows; i++)
                    matHilb[i][j] = arrHilb[i];
            } // cols

            return matHilb;
        } // forward

        //   * Performs the 2-D reverse transform from frequency or Hilbert or time domain
        //   * to time domain for a given array depending on the used transform algorithm
        //   * by inheritance.
        //   * 
        //   * @date 10.02.2010 11:01:38
        //   * @author Christian Scheiblich
        //   * @param matFreq
        //   *          coefficients of 2-D frequency or Hilbert domain
        //   * @return coefficients of 2-D time domain
        public virtual double[][] reverse(double[][] matFreq)
        {
            var noOfRows = matFreq.Length;
            var noOfCols = matFreq[0].Length;

            //double[][] matTime = new double[noOfRows][noOfCols];
            var matTime = MathUtils.CreateJaggedArray<double[][]>(noOfRows, noOfCols);

            for (var j = 0; j < noOfCols; j++)
            {
                var arrFreq = new double[noOfRows];

                for (var i = 0; i < noOfRows; i++)
                    arrFreq[i] = matFreq[i][j];

                var arrTime = reverse(arrFreq); // AED

                for (var i = 0; i < noOfRows; i++)
                    matTime[i][j] = arrTime[i];
            } // cols

            for (var i = 0; i < noOfRows; i++)
            {
                var arrFreq = new double[noOfCols];

                for (var j = 0; j < noOfCols; j++)
                    arrFreq[j] = matTime[i][j];

                var arrTime = reverse(arrFreq); // AED

                for (var j = 0; j < noOfCols; j++)
                    matTime[i][j] = arrTime[j];
            } // rows

            return matTime;
        } // reverse

        //   * Performs the 3-D forward transform from time domain to frequency or Hilbert
        //   * domain for a given array depending on the used transform algorithm by
        //   * inheritance.
        //   * 
        //   * @date 10.07.2010 18:08:17
        //   * @author Christian Scheiblich
        //   * @param spcTime
        //   *          coefficients of 3-D time domain domain
        //   * @return coefficients of 3-D frequency or Hilbert domain
        public virtual double[][][] forward(double[][][] spcTime)
        {
            var noOfRows = spcTime.Length; // first dimension
            var noOfCols = spcTime[0].Length; // second dimension
            var noOfHigh = spcTime[0][0].Length; // third dimension

            //double[][][] spcHilb = new double[noOfRows][noOfCols][noOfHigh];
            var spcHilb = MathUtils.CreateJaggedArray<double[][][]>(noOfRows, noOfCols, noOfHigh);

            for (var i = 0; i < noOfRows; i++)
            {
                //double[][] matTime = new double[noOfCols][noOfHigh];
                var matTime = MathUtils.CreateJaggedArray<double[][]>(noOfCols, noOfHigh);

                for (var j = 0; j < noOfCols; j++)
                for (var k = 0; k < noOfHigh; k++)
                    matTime[j][k] = spcTime[i][j][k];

                var matHilb = forward(matTime); // 2-D forward

                for (var j = 0; j < noOfCols; j++)
                for (var k = 0; k < noOfHigh; k++)
                    spcHilb[i][j][k] = matHilb[j][k];
            } // rows

            for (var j = 0; j < noOfCols; j++)
            for (var k = 0; k < noOfHigh; k++)
            {
                var arrTime = new double[noOfRows];

                for (var i = 0; i < noOfRows; i++)
                    arrTime[i] = spcHilb[i][j][k];

                var arrHilb = forward(arrTime); // 1-D forward

                for (var i = 0; i < noOfRows; i++)
                    spcHilb[i][j][k] = arrHilb[i];
            } // high

            return spcHilb;
        } // forward

        //   * Performs the 3-D reverse transform from frequency or Hilbert domain to time
        //   * domain for a given array depending on the used transform algorithm by
        //   * inheritance.
        //   * 
        //   * @date 10.07.2010 18:09:54
        //   * @author Christian Scheiblich
        //   * @param spcHilb
        //   *          coefficients of 3-D frequency or Hilbert domain
        //   * @return coefficients of 3-D time domain
        public virtual double[][][] reverse(double[][][] spcHilb)
        {
            var noOfRows = spcHilb.Length; // first dimension
            var noOfCols = spcHilb[0].Length; // second dimension
            var noOfHigh = spcHilb[0][0].Length; // third dimension

            //double[][][] spcTime = new double[noOfRows][noOfCols][noOfHigh];
            var spcTime = MathUtils.CreateJaggedArray<double[][][]>(noOfRows, noOfCols, noOfHigh);

            for (var i = 0; i < noOfRows; i++)
            {
                //double[][] matHilb = new double[noOfCols][noOfHigh];
                var matHilb = MathUtils.CreateJaggedArray<double[][]>(noOfCols, noOfHigh);

                for (var j = 0; j < noOfCols; j++)
                for (var k = 0; k < noOfHigh; k++)
                    matHilb[j][k] = spcHilb[i][j][k];

                var matTime = reverse(matHilb); // 2-D reverse

                for (var j = 0; j < noOfCols; j++)
                for (var k = 0; k < noOfHigh; k++)
                    spcTime[i][j][k] = matTime[j][k];
            } // rows

            for (var j = 0; j < noOfCols; j++)
            for (var k = 0; k < noOfHigh; k++)
            {
                var arrHilb = new double[noOfRows];

                for (var i = 0; i < noOfRows; i++)
                    arrHilb[i] = spcTime[i][j][k];

                var arrTime = reverse(arrHilb); // 1-D reverse

                for (var i = 0; i < noOfRows; i++)
                    spcTime[i][j][k] = arrTime[i];
            } // high

            return spcTime;
        } // reverse

        //   * Performs the forward transform from time domain to frequency or Hilbert
        //   * domain for a given array depending on the used transform algorithm by
        //   * inheritance.
        //   * 
        //   * @date 23.11.2010 19:17:46
        //   * @author Christian Scheiblich
        //   * @param arrTime
        //   *          coefficients of 1-D time domain
        //   * @return coefficients of 1-D frequency or Hilbert domain
        public virtual Complex[] forward(Complex[] arrTime)
        {
            return null;
        } // forward

        //   * Performs the reverse transform from frequency or Hilbert domain to time
        //   * domain for a given array depending on the used transform algorithm by
        //   * inheritance.
        //   * 
        //   * @date 23.11.2010 19:17:59
        //   * @author Christian Scheiblich
        //   * @param arrFreq
        //   *          coefficients of 1-D frequency or Hilbert domain
        //   * @return coefficients of 1-D time domain
        public virtual Complex[] reverse(Complex[] arrFreq)
        {
            return null;
        } // reverse
    } // class
}