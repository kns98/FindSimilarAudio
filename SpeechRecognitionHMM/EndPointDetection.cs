using System;

//
//  Please feel free to use/modify this class.
//  If you give me credit by keeping this information or
//  by sending me an email before using it or by reporting bugs , i will be happy.
//  Email : gtiwari333@gmail.com,
//  Blog : http://ganeshtiwaridotcomdotnp.blogspot.com/
// 
namespace SpeechRecognitionHMM
{
    // @author Madhav Pandey, Ganesh Tiwari
    // @reference 'A New Silence Removal and Endpoint Detection Algorithm for Speech
    //            and Speaker Recognition Applications' by IIT, Khragpur
    public class EndPointDetection
    {
        private readonly int firstSamples;
        private readonly float[] originalSignal; // input
        private readonly int samplePerFrame;
        private readonly int samplingRate;
        private float[] silenceRemovedSignal; // output

        public EndPointDetection(float[] originalSignal, int samplingRate)
        {
            this.originalSignal = originalSignal;
            this.samplingRate = samplingRate;
            samplePerFrame = this.samplingRate / 1000;
            firstSamples = samplePerFrame * 200; // according to formula
        }

        public float[] DoEndPointDetection()
        {
            var voiced = new float[originalSignal.Length]; // for identifying
            // each sample whether it is voiced or unvoiced

            float sum = 0;
            var sd = 0.0;
            var m = 0.0;

            // 1. calculation of mean
            //for (int i = 0; i < firstSamples; i++)
            for (var i = 0; i < originalSignal.Length; i++) sum += originalSignal[i];
            // System.err.println("total sum :" + sum);
            m = sum / firstSamples; // mean
            sum = 0; // reuse var for S.D.

            // 2. calculation of Standard Deviation
            //for (int i = 0; i < firstSamples; i++)
            for (var i = 0; i < originalSignal.Length; i++) sum += (float)Math.Pow(originalSignal[i] - m, 2);
            sd = Math.Sqrt(sum / firstSamples);
            // System.err.println("summm sum :" + sum);
            // System.err.println("mew :" + m);
            // System.err.println("sigma :" + sd);

            // 3. identifying whether one-dimensional Mahalanobis distance function
            // i.e. |x-u|/s greater than ####3 or not,
            for (var i = 0; i < originalSignal.Length; i++)
                // Console.Out.WriteLine("x-u/SD  ="+(Math.abs(originalSignal[i] -u ) / sd));
                if (Math.Abs(originalSignal[i] - m) / sd > 2)
                    voiced[i] = 1;
                else
                    voiced[i] = 0;

            // 4. calculation of voiced and unvoiced signals
            // mark each frame to be voiced or unvoiced frame
            var frameCount = 0;
            var usefulFramesCount = 1;
            var count_voiced = 0;
            var count_unvoiced = 0;
            var voicedFrame = new int[originalSignal.Length / samplePerFrame];
            var loopCount = originalSignal.Length - originalSignal.Length % samplePerFrame; // skip the last
            for (var i = 0; i < loopCount; i += samplePerFrame)
            {
                count_voiced = 0;
                count_unvoiced = 0;
                for (var j = i; j < i + samplePerFrame; j++)
                    if (voiced[j] == 1)
                        count_voiced++;
                    else
                        count_unvoiced++;
                if (count_voiced > count_unvoiced)
                {
                    usefulFramesCount++;
                    voicedFrame[frameCount++] = 1;
                }
                else
                {
                    voicedFrame[frameCount++] = 0;
                }
            }

            // 5. silence removal
            silenceRemovedSignal = new float[usefulFramesCount * samplePerFrame];
            var k = 0;
            for (var i = 0; i < frameCount; i++)
                if (voicedFrame[i] == 1)
                    for (var j = i * samplePerFrame; j < i * samplePerFrame + samplePerFrame; j++)
                        silenceRemovedSignal[k++] = originalSignal[j];
            // end
            return silenceRemovedSignal;
        }
    }
}