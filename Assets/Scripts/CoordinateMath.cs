using System;
using Domain;
using UnityEngine;

public static class CoordinateMath
{
    private const double EARTH_RADIUS_KILOMETERS = 6378.137;

    /**
    Returns distance in meters between two points.
    Math uses Haversine Formula
     */
    public static double Distance(Coordinates origin, Coordinates target) {
        double deltaLatitude = ToRadian(target.Latitude - origin.Latitude);
        double deltaLongitude = ToRadian(target.Longitude - origin.Longitude);
        double a = Math.Sin(deltaLatitude / 2) * Math.Sin(deltaLatitude / 2) +
                Math.Cos(ToRadian(origin.Latitude)) * Math.Cos(ToRadian(target.Latitude)) *
                Math.Sin(deltaLongitude / 2) * Math.Sin(deltaLongitude / 2);
        double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        double d = EARTH_RADIUS_KILOMETERS * c;
        return d * 1000;
    }

    private static double ToRadian(double degrees)
    {
        return (Math.PI / 180) * degrees;
    }

    /**
    Returns bearing in radians between two points, assuming north is 0.
     */
    public static double BearingRadian(Coordinates origin, Coordinates target) {
        double deltaLongitude = target.Longitude - origin.Longitude;
        double y = Math.Sin(deltaLongitude) * Math.Cos(target.Latitude);
        double x = Math.Cos(origin.Latitude) * Math.Sin(target.Latitude) -
            Math.Sin(origin.Latitude) * Math.Cos(target.Latitude) * Math.Cos(deltaLongitude);
        
        double direction = Math.Atan2(y, x);
        return direction;
    }

    public static Vector3 CoordinatesToWorldPosition(Coordinates target) {
        Coordinates origin = MapObjectPlacementManager.Instance.ProjectionOrigin;
        double x = (target.Longitude - origin.Longitude) * (ToRadian(EARTH_RADIUS_KILOMETERS) * Math.Cos(ToRadian(target.Latitude))) * 1000;
        double y = (target.Latitude - origin.Latitude) * ToRadian(EARTH_RADIUS_KILOMETERS) * 1000;

        return new Vector3((float)x, 0f, (float)y);
    }
}
