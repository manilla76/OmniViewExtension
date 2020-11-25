using System;
using System.Collections.Generic;
using System.Text;

namespace ThermoPileModelLibrary
{
    public class AnalyzerService : IAnalyzerService
    {
        private Random random = new Random();
        public double Tph { get; set; }
        public Dictionary<string, double> analysis { get; } = new Dictionary<string, double>();

        private string ipAddress = "127.0.0.1";
        //private Thermo.Datapool.Datapool datapool;                // Eventually add connection to datapool

        public Dictionary<string, double> GetAnalysis()
        {
            return new Dictionary<string, double> { { "LSF", 100 + 10 *( .5 - random.NextDouble()) } };
        }

    }
}
