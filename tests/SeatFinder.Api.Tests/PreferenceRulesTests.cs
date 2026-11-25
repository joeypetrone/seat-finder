using SeatFinder.Api.Data;
using SeatFinder.Api.Models;
using SeatFinder.Api.Providers;
using SeatFinder.Api.Validation;

namespace SeatFinder.Api.Tests;

public class PreferenceRulesTests
{
    // The mock's real layouts: rows A–M in the large rooms, seats 1–20 at most.
    private static readonly IReadOnlyList<Theater> Theaters =
        new MockShowtimeProvider().GetTheatersAsync().GetAwaiter().GetResult();

    private static SeatPreference Valid() => new()
    {
        PreferredRowFrom = "E",
        PreferredRowTo = "J",
        CenterSeatFrom = 7,
        CenterSeatTo = 14,
        PartySize = 2,
        SearchRadiusMiles = 25,
        HomeTheaterId = 1,
    };

    [Fact]
    public void ValidPreferences_HaveNoErrors()
    {
        Assert.Empty(PreferenceRules.Validate(Valid(), Theaters));
    }

    [Fact]
    public void NoHomeTheater_IsAllowed()
    {
        var preference = Valid();
        preference.HomeTheaterId = null;

        Assert.Empty(PreferenceRules.Validate(preference, Theaters));
    }

    [Fact]
    public void SingleRowAndSingleSeat_AreAllowed()
    {
        var preference = Valid();
        preference.PreferredRowTo = preference.PreferredRowFrom;
        preference.CenterSeatTo = preference.CenterSeatFrom;

        Assert.Empty(PreferenceRules.Validate(preference, Theaters));
    }

    [Fact]
    public void RowOnlyInLargeAuditoriums_IsAllowed()
    {
        // Small rooms stop at row I; M exists only in the large ones.
        var preference = Valid();
        preference.PreferredRowTo = "M";

        Assert.Empty(PreferenceRules.Validate(preference, Theaters));
    }

    [Theory]
    [InlineData("Z", "J", "preferredRowFrom")]
    [InlineData("E", "Z", "preferredRowTo")]
    [InlineData("J", "E", "preferredRowTo")]
    public void BadRows_AreRejected(string from, string to, string field)
    {
        var preference = Valid();
        preference.PreferredRowFrom = from;
        preference.PreferredRowTo = to;

        var errors = PreferenceRules.Validate(preference, Theaters);

        Assert.Equal([field], errors.Keys);
    }

    [Theory]
    [InlineData(0, 14, "centerSeatFrom")]
    [InlineData(7, 21, "centerSeatTo")]
    [InlineData(14, 7, "centerSeatTo")]
    public void BadCenterRange_IsRejected(int from, int to, string field)
    {
        var preference = Valid();
        preference.CenterSeatFrom = from;
        preference.CenterSeatTo = to;

        var errors = PreferenceRules.Validate(preference, Theaters);

        Assert.Equal([field], errors.Keys);
    }

    [Fact]
    public void UnknownHomeTheater_IsRejected()
    {
        var preference = Valid();
        preference.HomeTheaterId = 99;

        var errors = PreferenceRules.Validate(preference, Theaters);

        Assert.Equal(["homeTheaterId"], errors.Keys);
    }
}
