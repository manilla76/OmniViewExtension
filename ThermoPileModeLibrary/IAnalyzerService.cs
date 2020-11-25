using System.Collections.Generic;

namespace ThermoPileModelLibrary
{
    public interface IAnalyzerService
    {
        double Tph { get; set; }
        Dictionary<string, double> analysis { get; }

        Dictionary<string, double> GetAnalysis();
    }
}