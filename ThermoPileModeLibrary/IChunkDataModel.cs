using System.Collections.Generic;

namespace ThermoPileModelLibrary
{
    public interface IChunkDataModel
    {
        Dictionary<string, double> Analysis { get; }
        double Tons { get; }
        //double TonsRemaining { get; }

        double AddTonsToChunk(double tons, Dictionary<string, double> analysis);
    }
}