﻿using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Comirva.Audio.Util.Maths;
using CommonUtils;
using NDtw;

namespace Comirva.Audio.Feature
{
    /// <summary>
    ///     CoMIRVA: Collection of Music Information Retrieval and Visualization Applications
    ///     Ported from Java to C# by perivar@nerseth.com
    /// </summary>
    public class MandelEllis : AudioFeature
    {
        private GmmMe gmmMe;

        public MandelEllis(GmmMe gmmMe) : this()
        {
            this.gmmMe = gmmMe;
        }

        protected internal MandelEllis()
        {
        }

        /// the feature
        public override string Name { get; set; }

        /// <summary>Get Distance</summary>
        /// <seealso cref="">comirva.audio.feature.AudioFeature#GetDistance(comirva.audio.feature.AudioFeature)</seealso>
        public override double GetDistance(AudioFeature f)
        {
            if (!(f is MandelEllis))
            {
                new Exception("Can only handle AudioFeatures of type Mandel Ellis, not of: " + f);
                return -1;
            }

            var other = (MandelEllis)f;
            return KullbackLeibler(gmmMe, other.gmmMe) + KullbackLeibler(other.gmmMe, gmmMe);
        }

        /// <summary>Get Distance</summary>
        /// <seealso cref="">comirva.audio.feature.AudioFeature#GetDistance(comirva.audio.feature.AudioFeature)</seealso>
        public override double GetDistance(AudioFeature f, DistanceType t)
        {
            if (!(f is MandelEllis))
            {
                new Exception("Can only handle AudioFeatures of type Mandel Ellis, not of: " + f);
                return -1;
            }

            var other = (MandelEllis)f;

            var distanceMeasure = DistanceMeasure.Euclidean;
            switch (t)
            {
                case DistanceType.Dtw_Euclidean:
                    distanceMeasure = DistanceMeasure.Euclidean;
                    break;
                case DistanceType.Dtw_SquaredEuclidean:
                    distanceMeasure = DistanceMeasure.SquaredEuclidean;
                    break;
                case DistanceType.Dtw_Manhattan:
                    distanceMeasure = DistanceMeasure.Manhattan;
                    break;
                case DistanceType.Dtw_Maximum:
                    distanceMeasure = DistanceMeasure.Maximum;
                    break;
                case DistanceType.KullbackLeiblerDivergence:
                default:
                    return KullbackLeibler(gmmMe, other.gmmMe) + KullbackLeibler(other.gmmMe, gmmMe);
            }

            var dtw = new Dtw(GetArray(), other.GetArray(), distanceMeasure);
            return dtw.GetCost();
        }

        public double[] GetArray()
        {
            var mean = gmmMe.mean;
            var covarMatrix = gmmMe.covarMatrix;
            var covarMatrixInv = gmmMe.covarMatrixInv;

            var m = mean.GetColumnPackedCopy();
            var cov = covarMatrix.GetColumnPackedCopy();
            var icov = covarMatrixInv.GetColumnPackedCopy();

            var d = new double[m.Length + cov.Length + icov.Length];

            var start = 0;
            for (var i = 0; i < m.Length; i++) d[start + i] = mean[i];

            start += m.Length;
            for (var i = 0; i < cov.Length; i++) d[start + i] = cov[i];

            start += cov.Length;
            for (var i = 0; i < icov.Length; i++) d[start + i] = icov[i];

            return d;
        }

        /// <summary>
        ///     Calculate the Kullback-Leibler (KL) distance between the two GmmMe. (Also
        ///     known as relative entropy) To make the measure symmetric (ie. to obtain a
        ///     divergence), the KL distance should be called twice, with exchanged
        ///     parameters, and the result be added.
        ///     Implementation according to the submission to the MIREX'05 by Mandel and Ellis.
        /// </summary>
        /// <param name="gmmMe1">ME features of song 1</param>
        /// <param name="gmmMe2">ME features of song 2</param>
        /// <returns>the KL distance from gmmMe1 to gmmMe2</returns>
        private float KullbackLeibler(GmmMe gmmMe1, GmmMe gmmMe2)
        {
            var dim = gmmMe1.covarMatrix.GetColumnDimension();

            /// calculate the trace-term:
            var tr1 = gmmMe2.covarMatrixInv.Times(gmmMe1.covarMatrix);
            var tr2 = gmmMe1.covarMatrixInv.Times(gmmMe2.covarMatrix);
            var sum = tr1.Plus(tr2);
            var trace = (float)sum.Trace();

            /// "distance" between the two mean vectors:
            var dist = gmmMe1.mean.Minus(gmmMe2.mean);

            /// calculate the second brace:
            var secBra = gmmMe2.covarMatrixInv.Plus(gmmMe1.covarMatrixInv);

            var tmp1 = dist.Transpose().Times(secBra);

            /// finally, the whole term:
            return 0.5f * (trace - 2 * dim + (float)tmp1.Times(dist).Get(0, 0));
        }

        /// <summary>
        ///     Writes the xml representation of this object to the xml ouput stream.
        ///     <br>
        ///         <br>
        ///             There is the convetion, that each call to a <code>writeXML()</code> method
        ///             results in one xml element in the output stream.
        /// </summary>
        /// <param name="writer">XMLStreamWriter the xml output stream</param>
        /// <example>
        ///     mandelEllis.WriteXML(new XmlTextWriter("mandelellis.xml", null));
        /// </example>
        public void WriteXML(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("feature");
            xmlWriter.WriteAttributeString("type", GetType().ToString());
            gmmMe.mean.WriteXML(xmlWriter, "mean");
            gmmMe.covarMatrix.WriteXML(xmlWriter, "cov");
            gmmMe.covarMatrixInv.WriteXML(xmlWriter, "icov");
            xmlWriter.WriteEndElement();
            xmlWriter.Close();
        }

        /// <summary>
        ///     Reads the xml representation of an object form the xml input stream.<br>
        /// </summary>
        /// <param name="parser">XMLStreamReader the xml input stream</param>
        /// <example>
        ///     mandelEllis.ReadXML(new XmlTextReader("mandelellis.xml"));
        /// </example>
        public void ReadXML(XmlTextReader xmlTextReader)
        {
            var xdoc = XDocument.Load(xmlTextReader);

            var feature = xdoc.Element("feature");
            if (feature == null) throw new MissingFieldException("Could not find feature section - no GmmMe Loaded!");

            var mean = new Matrix(0, 0);
            mean.ReadXML(xdoc, "mean");

            var covarMatrix = new Matrix(0, 0);
            covarMatrix.ReadXML(xdoc, "cov");

            var covarMatrixInv = new Matrix(0, 0);
            covarMatrixInv.ReadXML(xdoc, "icov");

            gmmMe = new GmmMe(mean, covarMatrix, covarMatrixInv);
            xmlTextReader.Close();
        }

        public byte[] ToBytesCompressed()
        {
            using (var stream = new MemoryStream())
            {
                gmmMe.mean.WriteBinary(stream);
                gmmMe.covarMatrix.WriteBinary(stream);
                gmmMe.covarMatrixInv.WriteBinary(stream);
                stream.Flush();
                return stream.ToArray();
            }
        }

        public byte[] ToBytesXML()
        {
            //XmlWriterSettings settings = new XmlWriterSettings();
            //settings.Indent = true;
            var builder = new StringBuilder();
            using (var writer = XmlWriter.Create(builder))
            {
                WriteXML(writer);
            }

            return StringUtils.GetBytes(builder.ToString());
        }

        public static AudioFeature FromBytesCompressed(byte[] byteArray)
        {
            using (Stream stream = new MemoryStream(byteArray))
            {
                var mean = Matrix.LoadBinary(stream);
                var covarMatrix = Matrix.LoadBinary(stream);
                var covarMatrixInv = Matrix.LoadBinary(stream);
                stream.Flush();

                var gmmme = new GmmMe(mean, covarMatrix, covarMatrixInv);
                var mandelEllis = new MandelEllis(gmmme);
                return mandelEllis;
            }
        }

        public static AudioFeature FromBytesXML(byte[] buf)
        {
            var xmlData = StringUtils.GetString(buf);
            var xmlTextReader = new XmlTextReader(new StringReader(xmlData));

            var mandelEllis = new MandelEllis();
            mandelEllis.ReadXML(xmlTextReader);
            return mandelEllis;
        }

        /// <summary>
        ///     Manual serialization of a AudioFeature object to a byte array
        /// </summary>
        /// <returns>byte array</returns>
        public override byte[] ToBytes()
        {
            //return ToBytesXML();
            return ToBytesCompressed();
        }

        /// <summary>
        ///     Manual deserialization of an AudioFeature from a LittleEndian byte array
        /// </summary>
        /// <param name="buf">byte array</param>
        public static AudioFeature FromBytes(byte[] buf)
        {
            //return FromBytesXML(buf);
            return FromBytesCompressed(buf);
        }

        /// <summary>
        ///     "Gaussian Mixture Model for Mandel / Ellis algorithm" This class holds
        ///     the features needed for the Mandel Ellis algorithm: One full covariance
        ///     matrix, and the mean of all MFCCS.
        ///     @author tim
        /// </summary>
        public class GmmMe
        {
            // the covariance matrix
            internal Matrix covarMatrix;

            // the inverted covarMatrix, stored for computational efficiency
            internal Matrix covarMatrixInv;

            // a row vector
            internal Matrix mean;

            public GmmMe(Matrix mean, Matrix covarMatrix, Matrix covarMatrixInv)
            {
                this.mean = mean;
                this.covarMatrix = covarMatrix;
                this.covarMatrixInv = covarMatrixInv;
            }
        }
    }
}