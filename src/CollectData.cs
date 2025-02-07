using System;
using System.Collections.Generic;
using VTOLVR;
namespace vtolvrtelemetry
{
    public class CollectData
    {

        public string Location { get; set; }
        public Int32 unixTimestamp { get; set; }

        public VehicleClass Vehicle { get; set; } = new VehicleClass();
        public PhysicsClass Physics { get; set; } = new PhysicsClass();

        public class PhysicsClass
        {
            public float XAccel { get; set; }
            public float YAccel { get; set; }
            public float ZAccel { get; set; }
            public string PlayerGs { get; set; }
        }

        public class VehicleClass
        {
            public float Airspeed { get; set; }
            public float VerticalSpeed { get; set; }
            public float AltitudeASL { get; set; }
            public float Heading { get; set; }
            public float Pitch { get; set; }
            public float AoA { get; set; }
            public float Roll { get; set; }
            public string TailHook { get; set; }
            public string BatteryLevel { get; set; }
            public List<Dictionary<string, string>> Engines { get; set; }
            public string EjectionState { get; set; }
            public Dictionary<string, string> Lights { get; set; }
            public FuelClass Fuel { get; set; } = new FuelClass();
            public AvionicsClass Avionics { get; set; } = new AvionicsClass();

            public class AvionicsClass
            {
                public string StallDetector { get; set; }
                public string MissileDetected { get; set; }
                public string RadarState { get; set; }
                public List<Dictionary<string, string>> RWRContacts { get; set; }
                public string masterArm { get; set; }

            }

            public class FuelClass
            {
                public string FuelLevel { get; set; }
                public string FuelBurnRate { get; set; }
                public string FuelDensity { get; set; }
            }


        }
    }
}