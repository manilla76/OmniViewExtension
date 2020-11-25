using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace ThermoPileModelLibrary
{
    public class PileModel
    {
        private double stackerPosition;     //0-360 degree designation of the position of the stacker head
        private double stackerHeadPosition; //0-360 degree designation of the position of the very tip of the head of the pile
        private double stackerSpeed;        //Speed of the stacker in some unit (meters/sec, meters/min, or deg/min)
        private double stackerAdvanceSpeed; //Speed of the stacker advancing around the circle
        private double stackerSwingDegrees; //Number of degrees the stacker can swing ahead of the stackerHeadPosition before going back to the stackerHeadPosition

        private double pileRadius;          //Distance from center to the stacker
        private IChunkDataModel[,] pileAnalysis;
        private Random random;
        private double timePerAnalysis;                //time between stacker moves in minutes
        private bool isForward;
        private double tph;

        private ISegmentModel[] segments;
        private uint currentSegment;        //0-359 representing the individual 1 degree segments around the circular stockpile
        private uint currentChunk;          //0-99 representing the 100 chunks from the bottom(0) of the pile to the top(99)
        private readonly IAnalyzerService _analyzerService;
        private readonly IServiceProvider _serviceProvider;

        public uint CurrentSegment
        {
            get { return currentSegment; }
            set { currentSegment = value; }
        }

        public PileModel(IAnalyzerService analyzerService, IServiceProvider serviceProvider)
        {
            stackerPosition = 0f;
            stackerHeadPosition = 0f;
            stackerSpeed = 1.0;             // 0.15 degrees per minute
            stackerSwingDegrees = 10;
            //pileAnalysis = new IChunkDataModel[360, 100];   //Row,Column representation of pile.  Row = vertical chunk, Column = 1 degree segment
            segments = new ISegmentModel[360];
            stackerAdvanceSpeed = 0.12;      // 0.12 degrees per min = 7.2 deg / hr ~ 180deg per day ~ full rotation in 2 days
            random = new Random();
            pileRadius = 61;                // Radius of the stockpile in meters.
            timePerAnalysis = 1;                       // 1 minute between analysis
            isForward = true;               // stacker will start moving forward from the head.
            currentSegment = 0;
            currentChunk = 0;
            _analyzerService = analyzerService;
            _serviceProvider = serviceProvider;
            segments[currentSegment] = _serviceProvider.GetRequiredService<ISegmentModel>();
        }

        public void MoveStacker(double time)
        {
            //Move stackerPosition in designated direction.  If exceeds stackerSwingDegrees, reverse direction, if goes past stackerHeadPosition, reverse direction
            stackerPosition += (isForward ? 1 : -1) * stackerSpeed * time / 60;    // If forward, add to stacker position, if not, reverese direction
            //Move stackerHeadPosition by stackerAdvanceSpeed * tick
            stackerHeadPosition += (stackerAdvanceSpeed * time / 60);
            CheckPositon();
        }

        /// <summary>
        /// Check the current position to see if the direction needs to change
        /// </summary>
        private void CheckPositon()
        {
            if (stackerPosition > stackerHeadPosition + stackerSwingDegrees)
            {
                isForward = false;
            }
            if (stackerPosition < stackerHeadPosition)
            {
                isForward = true;
            }
            currentSegment = (uint) stackerPosition == currentSegment ? currentSegment : NextSegment(currentSegment);   // If segment is new, create new segment in the array as needed.  
        }

        private uint NextSegment(uint currentSegment)
        {
            if (isForward)
            {
                currentSegment = currentSegment == 359 ? 0 : currentSegment + 1;   // advance to the next segment.  if at the end of the array, cycle back to the beginning
            }
            else
            {
                currentSegment = currentSegment == 0 ? 359 : currentSegment - 1;   // go back to previous segment.  if at the beginning of the array, cycle back to the end
            }
            segments[currentSegment] ??= _serviceProvider.GetRequiredService<ISegmentModel>();
            return currentSegment;
        }

        public void AddTons(double time)
        {
            tph = GetTph();
            double tons = tph / (60 * 60) * time;                       // calculate tons for the time given  tons/hr / (60 min/hr * 60sec/min) = tons / sec
            double remainingTons = tons;
            segments[currentSegment].AddTonsToChunk(remainingTons, AddAnalysis());
        }

        private double GetTph()
        {
            return 1000 + 100 * (0.5 - random.NextDouble());            // 1000 +/- 50
        }

        private Dictionary<string, double> AddAnalysis()
        {
            //When the new analysis is ready, add the analysis to the segments based on the column pile model
            //Split the anlaysis to multiple segments if necessary.  
            return _analyzerService.GetAnalysis();
        }
    }
}
