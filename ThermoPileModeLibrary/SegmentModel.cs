using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace ThermoPileModelLibrary
{
    public class SegmentModel : ISegmentModel
    {
        public uint CurrentChunk { get; private set; }
        public IChunkDataModel[] chunks;
        private IServiceProvider _serviceProvider;
        public SegmentModel(IServiceProvider serviceProvider)
        {
            CurrentChunk = 0;
            chunks = new IChunkDataModel[100];
            _serviceProvider = serviceProvider;
            chunks[CurrentChunk] = _serviceProvider.GetRequiredService<IChunkDataModel>();
        }

        public void SetChunk(uint chunk)
        {
            if (chunk >= 100)
            {
                throw new ArgumentOutOfRangeException("CurrentChunk", "filled more than 100 chunks in this segment");
            }
            CurrentChunk = chunk;
        }

        public double AddTonsToChunk(double remainingTons, Dictionary<string, double> analysis)
        {
            chunks[CurrentChunk] ??= _serviceProvider.GetRequiredService<IChunkDataModel>();
            while (remainingTons != 0)
            {
                remainingTons = chunks[CurrentChunk].AddTonsToChunk(remainingTons, analysis);
                SetChunk(CurrentChunk + (uint)((remainingTons == 0) ? 0 : 1));     // If Tons remain after adding to the current chunk, advance to the next chunk and add again.
                chunks[CurrentChunk] ??= _serviceProvider.GetRequiredService<IChunkDataModel>();
            }
            return 0;
        }
    }
}
