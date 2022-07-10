using System;
using Comirva.Audio.Feature;

namespace Soundfingerprinting
{
    /// <summary>
    ///     Description of DummyAudioFeature.
    /// </summary>
    public class DummyAudioFeature : AudioFeature
    {
        public override string Name { get; set; }

        public override byte[] ToBytes()
        {
            throw new NotImplementedException();
        }

        public override double GetDistance(AudioFeature f, DistanceType t)
        {
            throw new NotImplementedException();
        }

        public override double GetDistance(AudioFeature f)
        {
            throw new NotImplementedException();
        }
    }
}