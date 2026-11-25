using SeatFinder.Api.Models;

namespace SeatFinder.Api.Providers;

/// <summary>
/// Where theaters and showtimes come from. The rest of the app depends only on this,
/// so a mock source and a real one are interchangeable.
/// </summary>
public interface IShowtimeProvider
{
    /// <summary>
    /// The time zone the showtimes are local to. "Today" and date ranges are judged in this zone,
    /// not the server's, so a 9 PM showing never lands on the next day.
    /// </summary>
    TimeZoneInfo TimeZone { get; }

    // Async because a real source would be an HTTP call, even though the mock answers instantly.
    Task<IReadOnlyList<Theater>> GetTheatersAsync(CancellationToken cancellationToken = default);

    /// <summary>Showings that start on any local day from <paramref name="from"/> through <paramref name="to"/>, inclusive.</summary>
    Task<IReadOnlyList<Showing>> GetShowingsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
