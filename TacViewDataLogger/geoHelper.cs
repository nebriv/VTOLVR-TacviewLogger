using System;

namespace TacViewDataLogger
{
    public class geoHelper
    {


        public static GeoLocation FindPointAtDistanceFrom2(GeoLocation source, double bearing, double range)
        {
            const double EarthRadius = 6378137.0;
            const double DegreesToRadians = 0.0174532925;
            const double RadiansToDegrees = 57.2957795;

            double latA = source.Latitude * DegreesToRadians;
            double lonA = source.Longitude * DegreesToRadians;
            double angularDistance = range * 1000 / EarthRadius;
            double trueCourse = bearing * DegreesToRadians;

            double lat = Math.Asin(Math.Sin(latA) * Math.Cos(angularDistance) + Math.Cos(latA) * Math.Sin(angularDistance) * Math.Cos(trueCourse));

            double dlon = Math.Atan2(Math.Sin(trueCourse) * Math.Sin(angularDistance) * Math.Cos(latA), Math.Cos(angularDistance) - Math.Sin(latA) * Math.Sin(lat));
            double lon = ((lonA + dlon + Math.PI) % (Math.PI * 2)) - Math.PI;

            return new GeoLocation
            {
                Latitude = lat * RadiansToDegrees,
                Longitude = lon * RadiansToDegrees
            };

        }


        public struct GeoLocation
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }

            public override string ToString()
            {
                return ($"{Latitude}, {Longitude}");
            }
        }

        public static double DegreesToRadians(double degrees)
        {
            const double degToRadFactor = Math.PI / 180;
            return degrees * degToRadFactor;
        }

        public static double RadiansToDegrees(double radians)
        {
            const double radToDegFactor = 180 / Math.PI;
            return radians * radToDegFactor;
        }
    }
}
