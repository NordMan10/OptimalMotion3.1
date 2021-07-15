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

        private readonly InputDataGenerator inputDataGenerator = new InputDataGenerator(ModellingParameters.FirstTakingOffMoment, ModellingParameters.TakingOffMomentStep);

        public event Func<TableRow> AddTakingOffAircraft;

        public void InvokeAddTakingOffAircraft()
        {
            var tableRow =  AddTakingOffAircraft?.Invoke();

            table.AddRow(tableRow);
        }

        private TableRow AddTakingOffAircraftHandler()
        {
            return GetOutputData();
        }

        public TableRow GetOutputData()
        {
            var inputData = inputDataGenerator.GetInputData();
            var takingOffAircraft = GetTakingOffAircraft(inputData);
            var thisRunway = runways[takingOffAircraft.RunwayId - 1];
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

            var newTakingOffMoment = takingOffAircraft.Moments.PlannedTakingOff + delay;

            return new TableRow(takingOffAircraft.Id.ToString(), newTakingOffMoment.ToString(), takingOffAircraft.Moments.PlannedTakingOff.ToString(), 
                takingOffAircraft.ProcessingIsNeeded, takingOffAircraft.RunwayId.ToString(), takingOffAircraft.SpecialPlaceId.ToString());
        }

        private void InitRunways(int runwayCount)
        {
            for (var i = 0; i < runwayCount; i++)
            {
                var runway = new Runway(i);
                runways.Add(i, runway);
            }
        }

        private void InitSpecialPlaces(int specPlatformCount)
        {
            for (var i = 1; i < specPlatformCount + 1; i++)
            {
                var specPlatform = new SpecialPlace(i);
                specialPlaces.Add(i, specPlatform);
            }
        }

        private TakingOffAircraft GetTakingOffAircraft(InputData inputData)
        {
            var specialPlaceId = DataRandomizer.GetRandomizedValue(ModellingParameters.StartIdValue, ModellingParameters.SpecialPlaceCount + 1);

            return aircraftGenerator.GetTakingOffAircraft(inputData, Enums.AircraftTypes.Medium, specialPlaceId);
        }


    }
}
