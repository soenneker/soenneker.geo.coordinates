[![](https://img.shields.io/nuget/v/soenneker.geo.coordinates.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.geo.coordinates/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.geo.coordinates/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.geo.coordinates/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.geo.coordinates.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.geo.coordinates/)

# Soenneker.Geo.Coordinates

High-performance geographic coordinate utilities.

## Install

```bash
dotnet add package Soenneker.Geo.Coordinates
```

## Quick start

```csharp
using Soenneker.Geo.Coordinates;

var result = CoordinateUtil.IsValid(1, 1);
```

Determines whether latitude and longitude are finite and within WGS84 bounds.

## What you get

- `CoordinateUtil` — High-performance geographic coordinate utilities.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `CoordinateUtil.IsValid(latitude, longitude)` | Determines whether latitude and longitude are finite and within WGS84 bounds. | true if latitude and longitude are finite and within WGS84 bounds; otherwise, false. |
| `CoordinateUtil.IsValid(coordinate)` | Determines whether the coordinate is finite and within WGS84 bounds. | true if the coordinate is finite and within WGS84 bounds; otherwise, false. |
| `CoordinateUtil.ClampLatitude(latitude)` | Clamps latitude to [-90, 90]. | The resulting value. |
| `CoordinateUtil.NormalizeLongitude(longitude)` | Normalizes longitude to [-180, 180]. | The resulting value. |
| `CoordinateUtil.Normalize(coordinate)` | Clamps latitude and normalizes longitude. | The resulting coordinate. |
| `CoordinateUtil.TryParse(value, coordinate)` | Attempts to parse an invariant "latitude, longitude" value. | true if the requested update was applied; otherwise, false. |
| `CoordinateUtil.Parse(value)` | Parses an invariant "latitude, longitude" value. | The resulting coordinate. |
| `CoordinateUtil.ToRadians(degrees)` | Converts degrees to radians. | The resulting value. |
| `CoordinateUtil.ToDegrees(radians)` | Converts radians to degrees. | The resulting value. |
| `CoordinateUtil.GetDistanceMeters(from, to)` | Calculates the great-circle distance in meters using the haversine formula. | The resulting value. |
| `CoordinateUtil.GetDistanceKilometers(from, to)` | Calculates the great-circle distance in kilometers using the haversine formula. | The resulting value. |
| `CoordinateUtil.GetDistanceMiles(from, to)` | Calculates the great-circle distance in miles using the haversine formula. | The resulting value. |
| `CoordinateUtil.GetInitialBearingDegrees(from, to)` | Calculates the initial bearing from one coordinate to another in degrees, normalized to [0, 360). | The resulting value. |
| `CoordinateUtil.GetDestination(origin, distanceMeters, bearingDegrees)` | Calculates the coordinate reached from the origin by travelling a distance at a bearing. | The resulting coordinate. |
| `CoordinateUtil.GetMidpoint(first, second)` | Calculates the geographic midpoint between two coordinates. | The resulting coordinate. |
| `CoordinateUtil.GetBoundingBox(center, radiusMeters)` | Calculates a bounding box around a center coordinate and radius. | The resulting (Coordinate Southwest, Coordinate Northeast). |
| `CoordinateUtil.GetClosest(origin, candidates)` | Gets the closest coordinate to an origin from a sequence of candidates. | The requested coordinate. |
| `CoordinateUtil.GetClosest(origin, candidates, coordinateSelector)` | Gets the item with the closest coordinate to an origin from a sequence of candidates. | The requested value. |
