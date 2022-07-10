namespace math.transform.jwave.handlers.wavelets
{
    ///
    // * Basic class for one wavelet keeping coefficients of the wavelet function, the
    // * scaling function, the base wavelength, the forward transform method, and the
    // * reverse transform method.
    // * 
    // * @date 10.02.2010 08:54:48
    // * @author Christian Scheiblich
    public abstract class Wavelet : WaveletInterface
    {
        //   * coefficients of the wavelet; wavelet function
        protected internal double[] _coeffs;

        //   * coefficients of the scales; scaling function
        protected internal double[] _scales;

        //   * minimal wavelength of the used wavelet and scaling coefficients
        protected internal int _waveLength;

        //   * Constructor; predefine members to init values
        //   * 
        //   * @date 10.02.2010 08:54:48
        //   * @author Christian Scheiblich
        public Wavelet()
        {
            _waveLength = 0;
            _coeffs = null;
            _scales = null;
        } // Wavelet

        //   * Returns the number of coeffs (and scales).
        //   * 
        //   * @date 08.02.2010 13:11:47
        //   * @author Christian Scheiblich
        //   * @return integer representing the number of coeffs.
        public virtual int Length => _coeffs.Length; // getLength

        //   * Performs the forward transform for the given array from time domain to
        //   * Hilbert domain and returns a new array of the same size keeping
        //   * coefficients of Hilbert domain and should be of length 2 to the power of p
        //   * -- length = 2^p where p is a positive integer.
        //   * 
        //   * @date 10.02.2010 08:18:02
        //   * @author Christian Scheiblich
        //   * @param arrTime
        //   *          array keeping time domain coefficients
        //   * @return coefficients represented by frequency domain
        public virtual double[] forward(double[] arrTime)
        {
            var arrHilb = new double[arrTime.Length];

            var k = 0;
            var h = arrTime.Length >> 1;

            for (var i = 0; i < h; i++)
            for (var j = 0; j < _waveLength; j++)
            {
                k = (i << 1) + j;
                while (k >= arrTime.Length)
                    k -= arrTime.Length;

                arrHilb[i] += arrTime[k] * _scales[j]; // low pass filter - energy (approximation)
                arrHilb[i + h] += arrTime[k] * _coeffs[j]; // high pass filter - details
            } // wavelet

            return arrHilb;
        } // forward

        //   * Performs the reverse transform for the given array from Hilbert domain to
        //   * time domain and returns a new array of the same size keeping coefficients
        //   * of time domain and should be of length 2 to the power of p -- length = 2^p
        //   * where p is a positive integer.
        //   * 
        //   * @date 10.02.2010 08:19:24
        //   * @author Christian Scheiblich
        //   * @param arrHilb
        //   *          array keeping frequency domain coefficients
        //   * @return coefficients represented by time domain
        public virtual double[] reverse(double[] arrHilb)
        {
            var arrTime = new double[arrHilb.Length];

            var k = 0;
            var h = arrHilb.Length >> 1;
            for (var i = 0; i < h; i++)
            for (var j = 0; j < _waveLength; j++)
            {
                k = (i << 1) + j;
                while (k >= arrHilb.Length)
                    k -= arrHilb.Length;

                arrTime[k] +=
                    arrHilb[i] * _scales[j] +
                    arrHilb[i + h] * _coeffs[j]; // adding up details times energy (approximation)
            } // wavelet

            return arrTime;
        } // reverse

        //   * Returns the minimal wavelength for the used wavelet.
        //   * 
        //   * @date 10.02.2010 08:13:59
        //   * @author Christian Scheiblich
        //   * @return the minimal wavelength for this basic wave
        public virtual int getWaveLength()
        {
            return _waveLength;
        } // getWaveLength

        //   * Returns a double array with the coeffs.
        //   * 
        //   * @date 08.02.2010 13:14:54
        //   * @author Christian Scheiblich
        //   * @return double array keeping the coeffs.
        public virtual double[] getCoeffs()
        {
            var coeffs = new double[_coeffs.Length];
            for (var c = 0; c < _coeffs.Length; c++)
                coeffs[c] = _coeffs[c];
            return coeffs;
        } // getCoeffs

        //   * Returns a double array with the scales (of a wavelet).
        //   * 
        //   * @date 08.02.2010 13:15:25
        //   * @author Christian Scheiblich
        //   * @return double array keeping the scales.
        public virtual double[] getScales()
        {
            var scales = new double[_scales.Length];
            for (var s = 0; s < _scales.Length; s++)
                scales[s] = _scales[s];
            return scales;
        } // getScales
    } // class
}