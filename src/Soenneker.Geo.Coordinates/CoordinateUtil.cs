using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using Soenneker.Dtos.Coordinates;

namespace Soenneker.Geo.Coordinates;

/// <summary>
/// High-performance geographic coordinate utilities.
/// </summary>
public static class CoordinateUtil
{
    private const double _earthRadiusMeters = 6_371_008.8;
    private const double _metersPerMile = 1_609.344;
    private const double _degreesToRadians = Math.PI / 180D;
    private const double _radiansToDegrees = 180D / Math.PI;

    /// <summary>
    /// Determines whether latitude and longitude are finite and within WGS84 bounds.
    /// </summary>
    public static bool IsValid(double latitude, double longitude)
    {
        return double.IsFinite(latitude) && double.IsFinite(longitude) && latitude is >= -90D and <= 90D &&
               longitude is >= -180D and <= 180D;
    }

    /// <summary>
    /// Determines whether the coordinate is finite and within WGS84 bounds.
    /// </summary>
    public static bool IsValid(Coordinate coordinate)
    {
        return IsValid(coordinate.Latitude, coordinate.Longitude);
    }

    /// <summary>
    /// Clamps latitude to [-90, 90].
    /// </summary>
    public static double ClampLatitude(double latitude)
    {
        if (!double.IsFinite(latitude))
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be finite.");

        return Math.Clamp(latitude, -90D, 90D);
    }

    /// <summary>
    /// Normalizes longitude to [-180, 180].
    /// </summary>
    public static double NormalizeLongitude(double longitude)
    {
        if (!double.IsFinite(longitude))
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be finite.");

        longitude = ((longitude + 180D) % 360D + 360D) % 360D - 180D;

        return longitude == -180D ? 180D : longitude;
    }

    /// <summary>
    /// Clamps latitude and normalizes longitude.
    /// </summary>
    public static Coordinate Normalize(Coordinate coordinate)
    {
        return new Coordinate(ClampLatitude(coordinate.Latitude), NormalizeLongitude(coordinate.Longitude));
    }

    /// <summary>
    /// Attempts to parse an invariant "latitude, longitude" value.
    /// </summary>
    public static bool TryParse(string? value, out Coordinate coordinate)
    {
        coordinate = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        ReadOnlySpan<char> span = value.AsSpan().Trim();
        int commaIndex = span.IndexOf(',');

        if (commaIndex <= 0 || commaIndex == span.Length - 1)
            return false;

        ReadOnlySpan<char> latitudeSpan = span[..commaIndex].Trim();
        ReadOnlySpan<char> longitudeSpan = span[(commaIndex + 1)..].Trim();

        if (!double.TryParse(latitudeSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude) ||
            !double.TryParse(longitudeSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude) ||
            !IsValid(latitude, longitude))
        {
            return false;
        }

        coordinate = new Coordinate(latitude, longitude);
        return true;
    }

    /// <summary>
    /// Parses an invariant "latitude, longitude" value.
    /// </summary>
    public static Coordinate Parse(string value)
    {
        if (TryParse(value, out Coordinate coordinate))
            return coordinate;

        throw new FormatException("Coordinate must be an invariant 'latitude, longitude' value within WGS84 bounds.");
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    public static double ToRadians(double degrees)
    {
        return degrees * _degreesToRadians;
    }

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    public static double ToDegrees(double radians)
    {
        return radians * _radiansToDegrees;
    }

    /// <summary>
    /// Calculates the great-circle distance in meters using the haversine formula.
    /// </summary>
    public static double GetDistanceMeters(Coordinate from, Coordinate to)
    {
        ThrowIfInvalid(from, nameof(from));
        ThrowIfInvalid(to, nameof(to));

        double fromLatitude = ToRadians(from.Latitude);
        double a = GetHaversineValue(fromLatitude, Math.Cos(fromLatitude), from, to);
        double c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));

        return _earthRadiusMeters * c;
    }

    /// <summary>
    /// Calculates the great-circle distance in kilometers using the haversine formula.
    /// </summary>
    public static double GetDistanceKilometers(Coordinate from, Coordinate to)
    {
        return GetDistanceMeters(from, to) / 1_000D;
    }

    /// <summary>
    /// Calculates the great-circle distance in miles using the haversine formula.
    /// </summary>
    public static double GetDistanceMiles(Coordinate from, Coordinate to)
    {
        return GetDistanceMeters(from, to) / _metersPerMile;
    }

    /// <summary>
    /// Calculates the initial bearing from one coordinate to another in degrees, normalized to [0, 360).
    /// </summary>
    public static double GetInitialBearingDegrees(Coordinate from, Coordinate to)
    {
        ThrowIfInvalid(from, nameof(from));
        ThrowIfInvalid(to, nameof(to));

        double fromLatitude = ToRadians(from.Latitude);
        double toLatitude = ToRadians(to.Latitude);
        double deltaLongitude = ToRadians(to.Longitude - from.Longitude);

        double y = Math.Sin(deltaLongitude) * Math.Cos(toLatitude);
        double x = Math.Cos(fromLatitude) * Math.Sin(toLatitude) -
                   Math.Sin(fromLatitude) * Math.Cos(toLatitude) * Math.Cos(deltaLongitude);

        return NormalizeDegrees(ToDegrees(Math.Atan2(y, x)));
    }

    /// <summary>
    /// Calculates the coordinate reached from the origin by travelling a distance at a bearing.
    /// </summary>
    public static Coordinate GetDestination(Coordinate origin, double distanceMeters, double bearingDegrees)
    {
        ThrowIfInvalid(origin, nameof(origin));

        if (!double.IsFinite(distanceMeters) || distanceMeters < 0D)
            throw new ArgumentOutOfRangeException(nameof(distanceMeters), "Distance must be finite and non-negative.");

        if (!double.IsFinite(bearingDegrees))
            throw new ArgumentOutOfRangeException(nameof(bearingDegrees), "Bearing must be finite.");

        double angularDistance = distanceMeters / _earthRadiusMeters;
        double bearing = ToRadians(bearingDegrees);
        double latitude = ToRadians(origin.Latitude);
        double longitude = ToRadians(origin.Longitude);

        double destinationLatitude = Math.Asin(Math.Sin(latitude) * Math.Cos(angularDistance) +
                                               Math.Cos(latitude) * Math.Sin(angularDistance) * Math.Cos(bearing));
        double destinationLongitude = longitude +
                                      Math.Atan2(Math.Sin(bearing) * Math.Sin(angularDistance) * Math.Cos(latitude),
                                          Math.Cos(angularDistance) -
                                          Math.Sin(latitude) * Math.Sin(destinationLatitude));

        return new Coordinate(ToDegrees(destinationLatitude), NormalizeLongitude(ToDegrees(destinationLongitude)));
    }

    /// <summary>
    /// Calculates the geographic midpoint between two coordinates.
    /// </summary>
    public static Coordinate GetMidpoint(Coordinate first, Coordinate second)
    {
        ThrowIfInvalid(first, nameof(first));
        ThrowIfInvalid(second, nameof(second));

        double firstLatitude = ToRadians(first.Latitude);
        double firstLongitude = ToRadians(first.Longitude);
        double secondLatitude = ToRadians(second.Latitude);
        double deltaLongitude = ToRadians(second.Longitude - first.Longitude);

        double bx = Math.Cos(secondLatitude) * Math.Cos(deltaLongitude);
        double by = Math.Cos(secondLatitude) * Math.Sin(deltaLongitude);

        double latitude = Math.Atan2(Math.Sin(firstLatitude) + Math.Sin(secondLatitude),
            Math.Sqrt((Math.Cos(firstLatitude) + bx) * (Math.Cos(firstLatitude) + bx) + by * by));
        double longitude = firstLongitude + Math.Atan2(by, Math.Cos(firstLatitude) + bx);

        return new Coordinate(ToDegrees(latitude), NormalizeLongitude(ToDegrees(longitude)));
    }

    /// <summary>
    /// Calculates a bounding box around a center coordinate and radius.
    /// </summary>
    public static (Coordinate Southwest, Coordinate Northeast) GetBoundingBox(Coordinate center, double radiusMeters)
    {
        ThrowIfInvalid(center, nameof(center));

        if (!double.IsFinite(radiusMeters) || radiusMeters < 0D)
            throw new ArgumentOutOfRangeException(nameof(radiusMeters), "Radius must be finite and non-negative.");

        double latitude = ToRadians(center.Latitude);
        double longitude = ToRadians(center.Longitude);
        double angularDistance = radiusMeters / _earthRadiusMeters;

        double minLatitude = latitude - angularDistance;
        double maxLatitude = latitude + angularDistance;

        if (minLatitude <= -Math.PI / 2D || maxLatitude >= Math.PI / 2D)
        {
            minLatitude = Math.Max(minLatitude, -Math.PI / 2D);
            maxLatitude = Math.Min(maxLatitude, Math.PI / 2D);

            return (new Coordinate(ToDegrees(minLatitude), -180D), new Coordinate(ToDegrees(maxLatitude), 180D));
        }

        double deltaLongitude = Math.Asin(Math.Sin(angularDistance) / Math.Cos(latitude));
        double minLongitude = longitude - deltaLongitude;
        double maxLongitude = longitude + deltaLongitude;

        return (new Coordinate(ToDegrees(minLatitude), NormalizeLongitude(ToDegrees(minLongitude))),
            new Coordinate(ToDegrees(maxLatitude), NormalizeLongitude(ToDegrees(maxLongitude))));
    }

    /// <summary>
    /// Gets the closest coordinate to an origin from a sequence of candidates.
    /// </summary>
    public static Coordinate? GetClosest(Coordinate origin, IEnumerable<Coordinate> candidates)
    {
        return GetByHaversineValue(origin, candidates, findClosest: true);
    }

    /// <summary>
    /// Gets the item with the closest coordinate to an origin from a sequence of candidates.
    /// </summary>
    public static T? GetClosest<T>(Coordinate origin, IEnumerable<T> candidates, Func<T, Coordinate> coordinateSelector)
    {
        return GetByHaversineValue(origin, candidates, coordinateSelector, findClosest: true);
    }

    /// <summary>
    /// Gets the closest coordinate to an origin from a span of candidates.
    /// </summary>
    public static Coordinate? GetClosest(Coordinate origin, ReadOnlySpan<Coordinate> candidates)
    {
        return GetByHaversineValue(origin, candidates, findClosest: true);
    }

    /// <summary>
    /// Gets the item with the closest coordinate to an origin from a span of candidates.
    /// </summary>
    public static T? GetClosest<T>(Coordinate origin, ReadOnlySpan<T> candidates, Func<T, Coordinate> coordinateSelector)
    {
        return GetByHaversineValue(origin, candidates, coordinateSelector, findClosest: true);
    }

    /// <summary>
    /// Gets the furthest coordinate from an origin from a sequence of candidates.
    /// </summary>
    public static Coordinate? GetFurthest(Coordinate origin, IEnumerable<Coordinate> candidates)
    {
        return GetByHaversineValue(origin, candidates, findClosest: false);
    }

    /// <summary>
    /// Gets the item with the furthest coordinate from an origin from a sequence of candidates.
    /// </summary>
    public static T? GetFurthest<T>(Coordinate origin, IEnumerable<T> candidates, Func<T, Coordinate> coordinateSelector)
    {
        return GetByHaversineValue(origin, candidates, coordinateSelector, findClosest: false);
    }

    /// <summary>
    /// Gets the furthest coordinate from an origin from a span of candidates.
    /// </summary>
    public static Coordinate? GetFurthest(Coordinate origin, ReadOnlySpan<Coordinate> candidates)
    {
        return GetByHaversineValue(origin, candidates, findClosest: false);
    }

    /// <summary>
    /// Gets the item with the furthest coordinate from an origin from a span of candidates.
    /// </summary>
    public static T? GetFurthest<T>(Coordinate origin, ReadOnlySpan<T> candidates, Func<T, Coordinate> coordinateSelector)
    {
        return GetByHaversineValue(origin, candidates, coordinateSelector, findClosest: false);
    }

    private static Coordinate? GetByHaversineValue(Coordinate origin, IEnumerable<Coordinate> candidates,
        bool findClosest)
    {
        ThrowIfInvalid(origin, nameof(origin));

        ArgumentNullException.ThrowIfNull(candidates);

        if (candidates is Coordinate[] array)
            return GetByHaversineValue(origin, array.AsSpan(), findClosest);

        if (candidates is List<Coordinate> list)
            return GetByHaversineValue(origin, CollectionsMarshal.AsSpan(list), findClosest);

        Coordinate? result = null;
        double selectedValue = findClosest ? double.MaxValue : double.MinValue;
        double originLatitude = ToRadians(origin.Latitude);
        double originLatitudeCos = Math.Cos(originLatitude);

        foreach (Coordinate candidate in candidates)
        {
            ThrowIfInvalid(candidate, nameof(candidates));

            double value = GetHaversineValue(originLatitude, originLatitudeCos, origin, candidate);

            if (findClosest ? value < selectedValue : value > selectedValue)
            {
                selectedValue = value;
                result = candidate;
            }
        }

        return result;
    }

    private static Coordinate? GetByHaversineValue(Coordinate origin, ReadOnlySpan<Coordinate> candidates,
        bool findClosest)
    {
        ThrowIfInvalid(origin, nameof(origin));

        Coordinate? result = null;
        double selectedValue = findClosest ? double.MaxValue : double.MinValue;
        double originLatitude = ToRadians(origin.Latitude);
        double originLatitudeCos = Math.Cos(originLatitude);

        for (var i = 0; i < candidates.Length; i++)
        {
            Coordinate candidate = candidates[i];
            ThrowIfInvalid(candidate, nameof(candidates));

            double value = GetHaversineValue(originLatitude, originLatitudeCos, origin, candidate);

            if (findClosest ? value < selectedValue : value > selectedValue)
            {
                selectedValue = value;
                result = candidate;
            }
        }

        return result;
    }

    private static T? GetByHaversineValue<T>(Coordinate origin, IEnumerable<T> candidates, Func<T, Coordinate> coordinateSelector,
        bool findClosest)
    {
        ThrowIfInvalid(origin, nameof(origin));
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(coordinateSelector);

        if (candidates is T[] array)
            return GetByHaversineValue(origin, array.AsSpan(), coordinateSelector, findClosest);

        if (candidates is List<T> list)
            return GetByHaversineValue(origin, CollectionsMarshal.AsSpan(list), coordinateSelector, findClosest);

        T? result = default;
        var hasResult = false;
        double selectedValue = findClosest ? double.MaxValue : double.MinValue;
        double originLatitude = ToRadians(origin.Latitude);
        double originLatitudeCos = Math.Cos(originLatitude);

        foreach (T candidate in candidates)
        {
            Coordinate coordinate = coordinateSelector(candidate);
            ThrowIfInvalid(coordinate, nameof(candidates));

            double value = GetHaversineValue(originLatitude, originLatitudeCos, origin, coordinate);

            if (findClosest ? value < selectedValue : value > selectedValue)
            {
                selectedValue = value;
                result = candidate;
                hasResult = true;
            }
        }

        return hasResult ? result : default;
    }

    private static T? GetByHaversineValue<T>(Coordinate origin, ReadOnlySpan<T> candidates, Func<T, Coordinate> coordinateSelector,
        bool findClosest)
    {
        ThrowIfInvalid(origin, nameof(origin));
        ArgumentNullException.ThrowIfNull(coordinateSelector);

        T? result = default;
        var hasResult = false;
        double selectedValue = findClosest ? double.MaxValue : double.MinValue;
        double originLatitude = ToRadians(origin.Latitude);
        double originLatitudeCos = Math.Cos(originLatitude);

        for (var i = 0; i < candidates.Length; i++)
        {
            T candidate = candidates[i];
            Coordinate coordinate = coordinateSelector(candidate);
            ThrowIfInvalid(coordinate, nameof(candidates));

            double value = GetHaversineValue(originLatitude, originLatitudeCos, origin, coordinate);

            if (findClosest ? value < selectedValue : value > selectedValue)
            {
                selectedValue = value;
                result = candidate;
                hasResult = true;
            }
        }

        return hasResult ? result : default;
    }

    private static double GetHaversineValue(double fromLatitudeRadians, double fromLatitudeCos, Coordinate from,
        Coordinate to)
    {
        double toLatitude = ToRadians(to.Latitude);
        double deltaLatitude = toLatitude - fromLatitudeRadians;
        double deltaLongitude = ToRadians(to.Longitude - from.Longitude);

        double sinLatitude = Math.Sin(deltaLatitude * 0.5D);
        double sinLongitude = Math.Sin(deltaLongitude * 0.5D);

        double value = sinLatitude * sinLatitude + fromLatitudeCos * Math.Cos(toLatitude) * sinLongitude * sinLongitude;

        return Math.Clamp(value, 0D, 1D);
    }

    private static void ThrowIfInvalid(Coordinate coordinate, string parameterName)
    {
        if (!IsValid(coordinate))
            throw new ArgumentOutOfRangeException(parameterName, "Coordinate must be finite and within WGS84 bounds.");
    }

    private static double NormalizeDegrees(double degrees)
    {
        degrees %= 360D;

        if (degrees < 0D)
            degrees += 360D;

        return degrees;
    }
}
