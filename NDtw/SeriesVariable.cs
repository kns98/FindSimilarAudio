using NDtw.Preprocessing;

namespace NDtw
{
    public class SeriesVariable
    {
        private readonly IPreprocessor _preprocessor;

        public SeriesVariable(double[] x, double[] y, string variableName = null, IPreprocessor preprocessor = null,
            double weight = 1)
        {
            OriginalXSeries = x;
            OriginalYSeries = y;
            VariableName = variableName;
            _preprocessor = preprocessor;
            Weight = weight;
        }

        public string VariableName { get; }

        public double Weight { get; }

        public double[] OriginalXSeries { get; }

        public double[] OriginalYSeries { get; }

        public double[] GetPreprocessedXSeries()
        {
            if (_preprocessor == null)
                return OriginalXSeries;

            return _preprocessor.Preprocess(OriginalXSeries);
        }

        public double[] GetPreprocessedYSeries()
        {
            if (_preprocessor == null)
                return OriginalYSeries;

            return _preprocessor.Preprocess(OriginalYSeries);
        }
    }
}