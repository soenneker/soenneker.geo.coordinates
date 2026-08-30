[![](https://img.shields.io/nuget/v/soenneker.geo.coordinates.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.geo.coordinates/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.geo.coordinates/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.geo.coordinates/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.geo.coordinates.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.geo.coordinates/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.geo.coordinates/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.geo.coordinates/actions/workflows/codeql.yml)

# Soenneker.Geo.Coordinates

High-performance geographic coordinate utilities.

## Install

```bash
dotnet add package Soenneker.Geo.Coordinates
```

## Parse and normalize

```csharp
using Soenneker.Geo.Coordinates;
using Soenneker.Dtos.Coordinates;

if (CoordinateUtil.TryParse("40.7128, -74.0060", out Coordinate newYork))
{
    bool valid = CoordinateUtil.IsValid(newYork);
}

Coordinate normalized = CoordinateUtil.Normalize(new Coordinate(95, 181));
// 90, -179
```

Parsing uses invariant culture and requires `latitude, longitude` in WGS84 bounds. `Normalize` clamps latitude to `[-90, 90]` and wraps finite longitude to `(-180, 180]`; it is not a validation substitute when rejecting bad input matters.

## Distance and navigation

```csharp
var newYork = new Coordinate(40.7128, -74.0060);
var losAngeles = new Coordinate(34.0522, -118.2437);

double kilometers = CoordinateUtil.GetDistanceKilometers(newYork, losAngeles);
double initialBearing = CoordinateUtil.GetInitialBearingDegrees(newYork, losAngeles);

Coordinate destination = CoordinateUtil.GetDestination(
    origin: newYork,
    distanceMeters: 10_000,
    bearingDegrees: 90);
```

Distance, bearing, midpoint, destination, and bounding-box calculations use a spherical Earth with a mean radius of 6,371,008.8 meters. They are appropriate for general geographic work, not survey-grade ellipsoidal calculations.

## Find nearest or furthest

```csharp
Coordinate? closest = CoordinateUtil.GetClosest(newYork,
[
    new Coordinate(40.7357, -74.1724),
    losAngeles
]);
```

Sequence and `ReadOnlySpan<T>` overloads are available, including selector overloads that return the original item. Empty inputs return `null` for `Coordinate` and `default` for generic results; ties keep the first candidate.

## API summary

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
| `CoordinateUtil.GetFurthest(...)` | Gets the coordinate or selected item furthest from the origin. | `null`/`default` for an empty input. |

## Important behavior

- Calculation methods reject non-finite or out-of-range coordinates with `ArgumentOutOfRangeException`.
- Negative or non-finite distances/radii and non-finite bearings are rejected.
- A bounding box that crosses the antimeridian can have a southwest longitude numerically greater than its northeast longitude; split that range when querying stores that do not understand wrapped longitude intervals.
- The initial bearing for identical points is returned as `0`, although a bearing is geometrically undefined in that case.
