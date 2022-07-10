namespace Soundfingerprinting.Audio.Strides
{
    /// <summary>
    ///     Incremental stride
    /// </summary>
    public class IncrementalStaticStride : IStride
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IncrementalStaticStride" /> class.
        /// </summary>
        /// <param name="incrementBy">
        ///     Increment by parameter in audio samples
        /// </param>
        /// <param name="samplesInFingerprint">
        ///     Number of samples in one signature [normally 8192]
        /// </param>
        public IncrementalStaticStride(int incrementBy, int samplesInFingerprint)
        {
            this.StrideSize =
                -samplesInFingerprint +
                incrementBy; /*Negative stride will guarantee that the signal is incremented by the parameter specified*/
            FirstStrideSize = 0;
        }

        public IncrementalStaticStride(int incrementBy, int samplesInFingerprint, int firstStride) : this(incrementBy,
            samplesInFingerprint)
        {
            this.FirstStrideSize = firstStride;
        }

        #region IStride Members

        /// <summary>
        ///     Increment by parameter (usually negative)
        /// </summary>
        public int StrideSize { get; }

        public int FirstStrideSize { get; }

        #endregion
    }
}