using System.Collections.Generic;

namespace ThermoPileModelLibrary
{
    public interface ISegmentModel
    {
        uint CurrentChunk { get; }

        void SetChunk(uint chunk);
        double AddTonsToChunk(double remainingTons, Dictionary<string, double> analysis);
    }
}