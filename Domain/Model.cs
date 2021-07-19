using OptimalMotion3._1.Domain.Static;
using OptimalMotion3._1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OptimalMotion3._1.Domain
{
    public class Model
    {
        public Model(int runwayCount, int specialPlaceCount, ITable table)
        {
            InitRunways(runwayCount);
            InitSpecialPlaces(specialPlaceCount);
            this.table = table;

            AddTakingOffAircraft += AddTakingOffAircraftHandler;
        }

        private Dictionary<int, Runway> runways = new Dictionary<int, Runway>();
        private Dictionary<int, SpecialPlace> specialPlaces = new Dictionary<int, SpecialPlace>();
        private readonly ITable table;

        private static AircraftIdGenerator idGenerator = AircraftIdGenerator.GetInstance(ModellingParameters.StartIdValue);

        private AircraftGenerator aircraftGenerator = AircraftGenerator.GetInstance(idGenerator);

        private readonly InputDataGenerator inputDataGenerator = new InputDataGenerator(ModellingParameters.FirstTakingOffMoment);

        public event Func<TableRow> AddTakingOffAircraft;


        public void UpdateModel(int runwayCount, int specialPlaceCount)
        {
            UpdateRunways(runwayCount);
            UpdateSpecialPlaces(specialPlaceCount);
        }

        public void InvokeAddTakingOffAircraft()
        {
            var tableRow = AddTakingOffAircraft?.Invoke();
            table.AddRow(tableRow);
        }

        private TableRow AddTakingOffAircraftHandler()
        {
            return GetOutputData();
        }

        public List<int> GetActualTakingOffMoments(List<int> plannedTakingOffMoments)
        {
            var actualTakingOffMoments = new List<int>();

            for (var i = 0; i < plannedTakingOffMoments.Count; i++)
            {
                var inputData = inputDataGenerator.GetInputData(plannedTakingOffMoments[i]);
                var takingOffAircraft = aircraftGenerator.GetTakingOffAircraft(inputData);

                var thisRunway = runways[takingOffAircraft.RunwayId];
                int startMoment;

                var takingOffInterval = new Interval(inputData.PlannedTakingOffMoment - takingOffAircraft.Intervals.TakingOff, inputData.PlannedTakingOffMoment);
                var freeRunwayInterval = thisRunway.GetFreeInterval(takingOffInterval);

                var delay = freeRunwayInterval.FirstMoment - takingOffInterval.FirstMoment;

                thisRunway.AddAircraftInterval(takingOffAircraft.Id, freeRunwayInterval);

                if (takingOffAircraft.ProcessingIsNeeded)
                {
                    var thisSpecialPlace = specialPlaces[takingOffAircraft.SpecialPlaceId];

                    var SPArriveMoment = takingOffInterval.FirstMoment - takingOffAircraft.Intervals.MotionFromPSToES -
                        takingOffAircraft.Intervals.MotionFromSPToPS - takingOffAircraft.Intervals.Processing;

                    var processingInterval = new Interval(SPArriveMoment, SPArriveMoment + takingOffAircraft.Intervals.Processing);
                    var freeSPInterval = thisSpecialPlace.GetFreeInterval(processingInterval);

                    delay += freeSPInterval.FirstMoment - processingInterval.FirstMoment;

                    thisSpecialPlace.AddAircraftInterval(takingOffAircraft.Id, freeSPInterval);

                    startMoment = SPArriveMoment - takingOffAircraft.Intervals.MotionFromParkingToSP + delay;
                }
                else
                {
                    startMoment = takingOffInterval.FirstMoment - takingOffAircraft.Intervals.MotionFromPSToES - takingOffAircraft.Intervals.MotionFromParkingToPS +
                        delay;
                }

                takingOffAircraft.Moments.Start = startMoment;

                actualTakingOffMoments.Add(takingOffAircraft.Moments.PlannedTakingOff + delay);
            }

            return actualTakingOffMoments;
        }



        //public TableRow GetOutputData()
        //{
        //    var inputData = inputDataGenerator.GetInputData(1);
        //    var takingOffAircraft = aircraftGenerator.GetTakingOffAircraft(inputData, Enums.AircraftTypes.Medium);
        //    var thisRunway = runways[takingOffAircraft.RunwayId];
        //    int startMoment;

        //    var takingOffInterval = new Interval(inputData.PlannedTakingOffMoment - takingOffAircraft.Intervals.TakingOff, inputData.PlannedTakingOffMoment);
        //    var freeRunwayInterval = thisRunway.GetFreeInterval(takingOffInterval);
            
        //    var delay = freeRunwayInterval.FirstMoment - takingOffInterval.FirstMoment;

        //    thisRunway.AddAircraftInterval(takingOffAircraft.Id, freeRunwayInterval);

        //    if (takingOffAircraft.ProcessingIsNeeded)
        //    {
        //        var thisSpecialPlace = specialPlaces[takingOffAircraft.SpecialPlaceId];

        //        var SPArriveMoment = takingOffInterval.FirstMoment - takingOffAircraft.Intervals.MotionFromPSToES -
        //            takingOffAircraft.Intervals.MotionFromSPToPS - takingOffAircraft.Intervals.Processing;

        //        var processingInterval = new Interval(SPArriveMoment, SPArriveMoment + takingOffAircraft.Intervals.Processing);
        //        var freeSPInterval = thisSpecialPlace.GetFreeInterval(processingInterval);

        //        delay += freeSPInterval.FirstMoment - processingInterval.FirstMoment;

        //        thisSpecialPlace.AddAircraftInterval(takingOffAircraft.Id, freeSPInterval);

        //        startMoment = SPArriveMoment - takingOffAircraft.Intervals.MotionFromParkingToSP + delay;
        //    }
        //    else
        //    {
        //        startMoment = takingOffInterval.FirstMoment - takingOffAircraft.Intervals.MotionFromPSToES - takingOffAircraft.Intervals.MotionFromParkingToPS +
        //            delay;
        //    }

        //    takingOffAircraft.Moments.Start = startMoment;

        //    var actualTakingOffMoment = takingOffAircraft.Moments.PlannedTakingOff + delay;

        //    return new TableRow(takingOffAircraft.Id.ToString(), actualTakingOffMoment.ToString(), takingOffAircraft.Moments.PlannedTakingOff.ToString(), 
        //        takingOffAircraft.ProcessingIsNeeded, takingOffAircraft.RunwayId.ToString(), takingOffAircraft.SpecialPlaceId.ToString());
        //}

        private void InitRunways(int runwayCount)
        {
            for (var i = ModellingParameters.StartIdValue; i < runwayCount + ModellingParameters.StartIdValue; i++)
            {
                var runway = new Runway(i);
                runways.Add(i, runway);
            }
        }

        private void InitSpecialPlaces(int specPlatformCount)
        {
            for (var i = ModellingParameters.StartIdValue; i < specPlatformCount + ModellingParameters.StartIdValue; i++)
            {
                var specialPlace = new SpecialPlace(i);
                specialPlaces.Add(i, specialPlace);
            }
        }

        private void UpdateRunways(int runwayCount)
        {
            for (var i = runways.Count; i < runwayCount; i++)
            {
                var runway = new Runway(i + ModellingParameters.StartIdValue);
                runways.Add(i + ModellingParameters.StartIdValue, runway);
            }
        }

        private void UpdateSpecialPlaces(int specialPlaceCount)
        {
            for (var i = specialPlaces.Count; i < specialPlaceCount; i++)
            {
                var specialPlace = new SpecialPlace(i + ModellingParameters.StartIdValue);
                specialPlaces.Add(i + ModellingParameters.StartIdValue, specialPlace);
            }
        }
    }
}
