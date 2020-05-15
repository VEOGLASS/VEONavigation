using System;

//TODO: POI type as enum; serializable country&postaladdress

namespace XploriaAR
{
    [Serializable]
    public class NavigationApiResponse
    {
        public Trip trip;                               //desired waypoints
        public NAVData navData;
        public NMEAData data;                           //all NMEA yacht data       

        public PointOfInterest[] pointsOfInterest;      //all available POIs in range
    }

    [Serializable]
    public class PointOfInterest
    {
        public string name;
        public string phone;
        public string website;

        public string type; 

        public Address address;
    }

    [Serializable]
    public struct Waypoint
    {
        public string title;

        public Location location;
    }

    [Serializable]
    public struct Trip
    {
        public Waypoint[] waypoints;
    }

    [Serializable]
    public struct NMEAData
    {
        public float speed;

        public float windDirection;
        public float windStrength;

        public float currentCourse;
        public float averageCourse;
        public float desiredCourse;

        public Location location;
    }

    [Serializable]
    public struct NAVData
    {
        public Location location;
    }

    [Serializable]
    public struct Address
    {
        public Location location;
        public PostalAddress postalAddress;

        //[Serializable]
        public struct PostalAddress
        {
            public string street;
            public string postalCode;
            public string region;
            public string locality;

            public Country country;

            //[Serializable]
            public struct Country
            {
                public string name;
                public string code;

                public Location locaiton;
            }
        }
    }
}