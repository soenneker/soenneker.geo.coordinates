using System;
using AwesomeAssertions;
using Soenneker.Dtos.Coordinates;
using Soenneker.Tests.Unit;

namespace Soenneker.Geo.Coordinates.Tests;

public sealed class CoordinateUtilTests : UnitTest
{
    [Test]
    public void IsValid_should_validate_wgs84_bounds()
    {
        CoordinateUtil.IsValid(new Coordinate(90, 180)).Should().BeTrue();
        CoordinateUtil.IsValid(new Coordinate(90.1, 0)).Should().BeFalse();
        CoordinateUtil.IsValid(new Coordinate(0, 180.1)).Should().BeFalse();
        CoordinateUtil.IsValid(new Coordinate(double.NaN, 0)).Should().BeFalse();
    }

    [Test]
    public void Normalize_should_clamp_latitude_and_wrap_longitude()
    {
        Coordinate result = CoordinateUtil.Normalize(new Coordinate(95, 181));

        result.Latitude.Should().Be(90);
        result.Longitude.Should().BeApproximately(-179, 0.000001);
    }

    [Test]
    public void TryParse_should_parse_invariant_coordinate()
    {
        bool result = CoordinateUtil.TryParse("40.7128, -74.0060", out Coordinate coordinate);

        result.Should().BeTrue();
        coordinate.Latitude.Should().Be(40.7128);
        coordinate.Longitude.Should().Be(-74.0060);
    }

    [Test]
    public void TryParse_should_reject_out_of_range_values()
    {
        bool result = CoordinateUtil.TryParse("91, 0", out Coordinate coordinate);

        result.Should().BeFalse();
        coordinate.Should().Be(default(Coordinate));
    }

    [Test]
    public void GetDistanceMeters_should_calculate_haversine_distance()
    {
        var newYork = new Coordinate(40.7128, -74.0060);
        var losAngeles = new Coordinate(34.0522, -118.2437);

        double result = CoordinateUtil.GetDistanceMeters(newYork, losAngeles);

        result.Should().BeApproximately(3_935_746, 1_000);
    }

    [Test]
    public void GetInitialBearingDegrees_should_calculate_bearing()
    {
        var newYork = new Coordinate(40.7128, -74.0060);
        var losAngeles = new Coordinate(34.0522, -118.2437);

        double result = CoordinateUtil.GetInitialBearingDegrees(newYork, losAngeles);

        result.Should().BeApproximately(273.69, 0.1);
    }

    [Test]
    public void GetDestination_should_calculate_destination_point()
    {
        var origin = new Coordinate(0, 0);

        Coordinate result = CoordinateUtil.GetDestination(origin, 111_195, 90);

        result.Latitude.Should().BeApproximately(0, 0.001);
        result.Longitude.Should().BeApproximately(1, 0.001);
    }

    [Test]
    public void GetMidpoint_should_calculate_geographic_midpoint()
    {
        var first = new Coordinate(0, 0);
        var second = new Coordinate(0, 2);

        Coordinate result = CoordinateUtil.GetMidpoint(first, second);

        result.Latitude.Should().BeApproximately(0, 0.000001);
        result.Longitude.Should().BeApproximately(1, 0.000001);
    }

    [Test]
    public void GetBoundingBox_should_calculate_radius_bounds()
    {
        var center = new Coordinate(0, 0);

        (Coordinate southwest, Coordinate northeast) = CoordinateUtil.GetBoundingBox(center, 111_195);

        southwest.Latitude.Should().BeApproximately(-1, 0.001);
        southwest.Longitude.Should().BeApproximately(-1, 0.001);
        northeast.Latitude.Should().BeApproximately(1, 0.001);
        northeast.Longitude.Should().BeApproximately(1, 0.001);
    }

    [Test]
    public void GetDistanceMeters_should_throw_for_invalid_coordinate()
    {
        Action action = () => CoordinateUtil.GetDistanceMeters(new Coordinate(91, 0), new Coordinate(0, 0));

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GetClosest_should_return_nearest_candidate()
    {
        var origin = new Coordinate(40.7128, -74.0060);
        var newark = new Coordinate(40.7357, -74.1724);
        var losAngeles = new Coordinate(34.0522, -118.2437);
        var chicago = new Coordinate(41.8781, -87.6298);

        Coordinate? result = CoordinateUtil.GetClosest(origin, [losAngeles, newark, chicago]);

        result.Should().Be(newark);
    }

    [Test]
    public void GetFurthest_should_return_furthest_candidate()
    {
        var origin = new Coordinate(40.7128, -74.0060);
        var newark = new Coordinate(40.7357, -74.1724);
        var losAngeles = new Coordinate(34.0522, -118.2437);
        var chicago = new Coordinate(41.8781, -87.6298);

        Coordinate? result = CoordinateUtil.GetFurthest(origin, [newark, chicago, losAngeles]);

        result.Should().Be(losAngeles);
    }

    [Test]
    public void Closest_and_furthest_should_return_null_for_empty_candidates()
    {
        var origin = new Coordinate(40.7128, -74.0060);
        var candidates = Array.Empty<Coordinate>();

        CoordinateUtil.GetClosest(origin, candidates).Should().BeNull();
        CoordinateUtil.GetFurthest(origin, candidates).Should().BeNull();
    }

    [Test]
    public void GetClosest_should_return_original_object_from_selector()
    {
        var origin = new Coordinate(40.7128, -74.0060);
        var newark = new CoordinateCandidate("newark", new Coordinate(40.7357, -74.1724));
        var losAngeles = new CoordinateCandidate("los-angeles", new Coordinate(34.0522, -118.2437));
        var chicago = new CoordinateCandidate("chicago", new Coordinate(41.8781, -87.6298));

        CoordinateCandidate? result = CoordinateUtil.GetClosest(origin, [losAngeles, newark, chicago], static x => x.Coordinate);

        result.Should().BeSameAs(newark);
    }

    [Test]
    public void GetFurthest_should_return_original_object_from_selector()
    {
        var origin = new Coordinate(40.7128, -74.0060);
        var newark = new CoordinateCandidate("newark", new Coordinate(40.7357, -74.1724));
        var losAngeles = new CoordinateCandidate("los-angeles", new Coordinate(34.0522, -118.2437));
        var chicago = new CoordinateCandidate("chicago", new Coordinate(41.8781, -87.6298));

        CoordinateCandidate? result = CoordinateUtil.GetFurthest(origin, [newark, chicago, losAngeles], static x => x.Coordinate);

        result.Should().BeSameAs(losAngeles);
    }

    private sealed record CoordinateCandidate(string Name, Coordinate Coordinate);
}
