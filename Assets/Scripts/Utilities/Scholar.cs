using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace XploriaAR
{
    public static class Scholar
    {
        #region Const fields

        private const double equatorialRadius = 6371137.0;
        private const double inverseFlattening = 298.257223563;

        #endregion

        public static Vector2 GPSCoorToPoint(Location location)
        {
            return GPSCoorToPoint(location.lat, location.lng);
        }

        public static Vector2 GPSCoorToPoint(double lat, double lng)
        {
            int zone = (int)Math.Floor(lng / 6 + 31);

            double easting = 0;
            double northing = 0;

            double f = 1.0 / inverseFlattening;
            double b = equatorialRadius * (1 - f);                       // polar radius

            double e = Math.Sqrt(1 - Math.Pow(b, 2) / Math.Pow(equatorialRadius, 2));
            double e0 = e / Math.Sqrt(1 - Math.Pow(e, 1));

            double drad = Math.PI / 180;
            double k0 = 0.9996;

            double latRadians = lat * drad;                              // convert latitude to radians
            double lngRadians = lng * drad;                              // convert longitude to radians
            double utmz = 1 + Math.Floor((lng + 180) / 6.0);             // longitude to utm zone
            double zcm = 3 + 6.0 * (utmz - 1) - 180;                     // central meridian of a zone
                                                                         // this gives us zone A-B for below 80S
            double esq = (1 - (b / equatorialRadius) * (b / equatorialRadius));
            double e0sq = e * e / (1 - Math.Pow(e, 2));
            double M = 0;

            double N = equatorialRadius / Math.Sqrt(1 - Math.Pow(e * Math.Sin(latRadians), 2));
            double T = Math.Pow(Math.Tan(latRadians), 2);
            double C = e0sq * Math.Pow(Math.Cos(latRadians), 2);
            double A = (lng - zcm) * drad * Math.Cos(latRadians);

            //calculate M (USGS style)
            M = latRadians * (1 - esq * (1.0 / 4.0 + esq * (3.0 / 64.0 + 5.0 * esq / 256.0)));
            M = M - Math.Sin(2.0 * latRadians) * (esq * (3.0 / 8.0 + esq * (3.0 / 32.0 + 45.0 * esq / 1024.0)));
            M = M + Math.Sin(4.0 * latRadians) * (esq * esq * (15.0 / 256.0 + esq * 45.0 / 1024.0));
            M = M - Math.Sin(6.0 * latRadians) * (esq * esq * esq * (35.0 / 3072.0));
            M = M * equatorialRadius;
            // if another point of origin is used than the equator
            var M0 = 0;
            //calculate easting(relative to CM)
            var x = k0 * N * A * (1 + A * A * ((1 - T + C) / 6 + A * A * (5 - 18 * T + T * T + 72.0 * C - 58 * e0sq) / 120.0));
            //standard easting
            x = x + 500000;
            //calculate northing(first from the equator)
            var y = k0 * (M - M0 + N * Math.Tan(latRadians) * (A * A * (1 / 2.0 + A * A * ((5 - T + 9 * C + 4 * C * C) / 24.0 + A * A * (61 - 58 * T + T * T + 600 * C - 330 * e0sq) / 720.0))));
            //yg = y global, from S. Pole  //yg = y global, from S. Pole
            var yg = y + 10000000;
            //add in false northing if south of the equator
            if (y < 0) y = 10000000 + y;

            easting = Math.Round(10 * x) / 10.0;
            northing = Math.Round(10 * y) / 10.0;

            return new Vector2((float)easting, (float)northing);
        }

        /// <summary>
        /// Distance between points given in decimal degrees 
        ///    unit = the unit you desire for results                              
        ///           where: 'M' is statute miles (default)                        
        ///                  'K' is kilometers                                     
        ///                  'N' is nautical miles  
        ///                  'm' is meters  
        /// </summary>
        /// <param name="deviceLocation"></param>
        /// <param name="placeLocation"></param>
        /// <param name="unit"></param>
        /// <returns></returns> 
        public static double Distance(Location deviceLocation, Location placeLocation, char unit)
        {
            if ((deviceLocation.lat == placeLocation.lat) && (deviceLocation.lng == placeLocation.lng))
            {
                return 0;
            }
            else
            {
                double theta = deviceLocation.lng - placeLocation.lng;
                double dist = Math.Sin(Deg2Rad(deviceLocation.lat)) * Math.Sin(Deg2Rad(placeLocation.lat)) + Math.Cos(Deg2Rad(deviceLocation.lat)) * Math.Cos(Deg2Rad(placeLocation.lat)) * Math.Cos(Deg2Rad(theta));
                dist = Math.Acos(dist);
                dist = Rad2Deg(dist);
                dist = dist * 60 * 1.1515;
                if (unit == 'K')
                {
                    dist = dist * 1.609344;
                }
                else if (unit == 'N')
                {
                    dist = dist * 0.8684;
                }
                else if (unit == 'm')
                {
                    dist = dist * 1609.344;
                }
                return (dist);
            }
        }
       
        // Rhumb line bearing
        public static double RhumbBearingTo(Location deviceLocation, Location placeLocation)
        {
            double lat1 = Deg2Rad(deviceLocation.lat);
            double lat2 = Deg2Rad(placeLocation.lat);
            double dLon = Deg2Rad(placeLocation.lng - deviceLocation.lng);

            double dPhi = Math.Log(Math.Tan(lat2 / 2 + Math.PI / 4) / Math.Tan(lat1 / 2 + Math.PI / 4));
            if (Math.Abs(dLon) > Math.PI) dLon = (dLon > 0) ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
            double brng = Math.Atan2(dLon, dPhi);

            return (Rad2Deg(brng) + 360) % 360;
        } 

        // Calculates bearing from device location to POIs. Bearing in degrees
        public static double BearingTo(Location deviceLocation, Location placeLocation)
        {
            double lat1 = Deg2Rad(deviceLocation.lat);
            double lat2 = Deg2Rad(placeLocation.lat);
            double dLon = Deg2Rad(placeLocation.lng) - Deg2Rad(deviceLocation.lng);

            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
            double brng = Math.Atan2(y, x);

            return (Rad2Deg(brng) + 360) % 360;
        }

        //  Converts decimal degrees to radians
        public static double Deg2Rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        // Converts radians to decimal degrees
        public static double Rad2Deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
    }

    [Serializable]
    public struct Location
    {
        public double lat;
        public double lng;

        public Location(double lat, double lng)
        {
            this.lat = lat;
            this.lng = lng;
        }

        public Location(LocationInfo info)
        {
            lat = double.Parse(info.latitude.ToString("R"));
            lng = double.Parse(info.longitude.ToString("R"));
        }

        public override string ToString()
        {
            var lat = this.lat.ToString().Replace(',', '.');
            var lng = this.lng.ToString().Replace(',', '.');
            return lat + "," + lng;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object o)
        {
            if (((Location)o).lat == this.lat && ((Location)o).lng == this.lng)
                return true;
            else
                return false;
        }

        #region Operators overloading

        public static bool operator ==(Location a, Location b)
        {
            return a.lat == b.lat && a.lng == b.lng;
        }

        public static bool operator !=(Location a, Location b)
        {
            return a.lat != b.lat || a.lng != b.lng;
        }

        #endregion
    }
}