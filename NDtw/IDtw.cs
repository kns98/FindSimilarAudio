using System;

namespace NDtw
{
    public interface IDtw
    {
        int XLength { get; }
        int YLength { get; }
        SeriesVariable[] SeriesVariables { get; }
        double GetCost();
        Tuple<int, int>[] GetPath();
        double[][] GetDistanceMatrix();
        double[][] GetCostMatrix();
    }
}