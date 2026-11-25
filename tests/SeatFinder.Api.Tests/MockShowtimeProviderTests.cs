using SeatFinder.Api.Models;
using SeatFinder.Api.Providers;

namespace SeatFinder.Api.Tests;

public class MockShowtimeProviderTests
{
    // A Friday, so the range below covers weekday and weekend fill rates.
    private static readonly DateOnly Friday = new(2026, 12, 4);

    [Fact]
    public async Task SameShowing_FromTwoProviders_HasSameTakenSeats()
    {
        var first = await new MockShowtimeProvider().GetShowingsAsync(Friday, Friday);
        var second = await new MockShowtimeProvider().GetShowingsAsync(Friday, Friday);

        Assert.Equal(first.Count, second.Count);
        for (var i = 0; i < first.Count; i++)
        {
            Assert.Equal(first[i].Id, second[i].Id);
            // Lists inside records compare by reference, so compare the seats themselves.
            Assert.Equal(first[i].Seats, second[i].Seats);
        }
    }

    [Fact]
    public async Task Showing_HasSameSeats_WhicheverRangeItWasRequestedIn()
    {
        var provider = new MockShowtimeProvider();

        var oneDay = await provider.GetShowingsAsync(Friday, Friday);
        var week = await provider.GetShowingsAsync(Friday.AddDays(-3), Friday.AddDays(3));

        foreach (var showing in oneDay)
        {
            var same = Assert.Single(week, s => s.Id == showing.Id);
            Assert.Equal(showing.Seats, same.Seats);
        }
    }

    [Fact]
    public async Task DifferentShowings_HaveDifferentTakenSeats()
    {
        var showings = await new MockShowtimeProvider().GetShowingsAsync(Friday, Friday);

        // Same auditorium, different times: the seed differs, so the pattern should too.
        var inOneRoom = showings.Where(s => s.AuditoriumId == 11).ToList();
        Assert.True(inOneRoom.Count >= 2);
        Assert.NotEqual(inOneRoom[0].Seats, inOneRoom[1].Seats);
    }

    [Fact]
    public async Task GetShowings_ReturnsOnlyShowingsInsideTheRange()
    {
        var provider = new MockShowtimeProvider();
        var from = Friday;
        var to = Friday.AddDays(2);

        var showings = await provider.GetShowingsAsync(from, to);

        Assert.NotEmpty(showings);
        Assert.All(showings, s =>
        {
            var localDay = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(s.StartsAt, provider.TimeZone).DateTime);
            Assert.InRange(localDay, from, to);
        });
        var days = showings.Select(s => s.StartsAt.Date).Distinct().Count();
        Assert.Equal(3, days);
    }

    [Fact]
    public async Task Showings_AreSortedByStartTime()
    {
        var showings = await new MockShowtimeProvider().GetShowingsAsync(Friday, Friday.AddDays(1));

        Assert.Equal(showings.OrderBy(s => s.StartsAt), showings);
    }

    [Fact]
    public async Task Theaters_HaveUniqueAuditoriumIds_AndShowingsPointAtThem()
    {
        var provider = new MockShowtimeProvider();
        var theaters = await provider.GetTheatersAsync();
        var showings = await provider.GetShowingsAsync(Friday, Friday);

        Assert.Equal(4, theaters.Count);
        var auditoriumIds = theaters.SelectMany(t => t.Auditoriums).Select(a => a.Id).ToList();
        Assert.Equal(auditoriumIds.Count, auditoriumIds.Distinct().Count());

        Assert.All(showings, s =>
        {
            var theater = Assert.Single(theaters, t => t.Id == s.TheaterId);
            Assert.Contains(theater.Auditoriums, a => a.Id == s.AuditoriumId);
        });
    }

    [Fact]
    public async Task Seats_AreMarkedAisleAtSectionEdges()
    {
        var theaters = await new MockShowtimeProvider().GetTheatersAsync();
        // The large layout is 4 | 12 | 4: aisles after seat 4 and after seat 16.
        var rowA = theaters[0].Auditoriums[0].Seats.Where(s => s.Row == "A").ToList();

        var aisleNumbers = rowA.Where(s => s.IsAisle).Select(s => s.Number);

        Assert.Equal(20, rowA.Count);
        Assert.Equal([1, 4, 5, 16, 17, 20], aisleNumbers);
    }

    [Fact]
    public async Task EveryShowing_HasSomeOpenAndSomeTakenSeats()
    {
        var showings = await new MockShowtimeProvider().GetShowingsAsync(Friday, Friday.AddDays(6));

        // Not a law of theaters, but if the generator ever gave empty or sold-out rooms everywhere,
        // the seat search and scoring would have nothing to show.
        Assert.All(showings, s =>
        {
            Assert.Contains(s.Seats, seat => seat.Status == SeatStatus.Open);
            Assert.Contains(s.Seats, seat => seat.Status == SeatStatus.Taken);
        });
    }
}
