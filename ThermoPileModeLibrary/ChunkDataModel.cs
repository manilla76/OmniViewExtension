using System;
using System.Collections.Generic;
using System.Text;

namespace ThermoPileModelLibrary
{
    public class ChunkDataModel : IChunkDataModel
    {
        public Dictionary<string, double> Analysis { get; } = new Dictionary<string, double> { { "LSF", 0f } };
        public double Tons { get; private set; } = 0;
        private double maxTons = 2.0;
        //public double TonsRemaining { get; private set; }
        
        
        /// <summary>
        /// Adds tons to chunk until the limit is hit, then returns the remainder to be added into the next chunk.
        /// </summary>
        /// <param name="tons">Tons to be added</param>
        /// <returns>Remaining tons if tons fills this chunk</returns>
        public double AddTonsToChunk(double tons, Dictionary<string, double> analysis)
        {
            double tonsRemaining = maxTons - Tons;
            if (tonsRemaining > tons)
            {
                UpdateAnalysis(tons, analysis);
                Tons += tons;
                return 0;
            }
            UpdateAnalysis(tonsRemaining, analysis);
            Tons = maxTons;
            
            return tons - tonsRemaining;
        }

        private void UpdateAnalysis(double tons, Dictionary<string, double> analysis)
        {
            Analysis["LSF"] = (Tons + tons == 0) ? 0 : (Analysis["LSF"] * Tons + analysis["LSF"] * tons) / (Tons + tons);
        }
    }
}
