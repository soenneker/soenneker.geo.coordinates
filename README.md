[![](https://img.shields.io/nuget/v/soenneker.geo.coordinates.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.geo.coordinates/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.geo.coordinates/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.geo.coordinates/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.geo.coordinates.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.geo.coordinates/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Geo.Coordinates
### High-performance geographic coordinate utilities.

## Installation

```
dotnet add package Soenneker.Geo.Coordinates
```

## Usage

```csharp
using Soenneker.Dtos.Coordinates;
using Soenneker.Geo.Coordinates;

var newYork = new Coordinate(40.7128, -74.0060);
var losAngeles = new Coordinate(34.0522, -118.2437);

double meters = CoordinateUtil.GetDistanceMeters(newYork, losAngeles);
double bearing = CoordinateUtil.GetInitialBearingDegrees(newYork, losAngeles);
Coordinate midpoint = CoordinateUtil.GetMidpoint(newYork, losAngeles);
Coordinate? closest = CoordinateUtil.GetClosest(newYork, [losAngeles, midpoint]);
```

Return the original object when candidates contain coordinates:

```csharp
public sealed record Store(string Name, Coordinate Coordinate);

Store? closestStore = CoordinateUtil.GetClosest(
    newYork,
    stores,
    static store => store.Coordinate);
```
