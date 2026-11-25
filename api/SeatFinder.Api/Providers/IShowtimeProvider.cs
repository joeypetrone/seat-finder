using SeatFinder.Api.Models;

namespace SeatFinder.Api.Providers;

/// <summary>
/// Where theaters and showtimes come from. The rest of the app depends only on this,
/// so a mock source and a real one are interchangeable.
/// </summary>
public interface IShowtimeProvider
{
    // Async because a real source would be an HTTP call, even though the mock answers instantly.
    Task<IReadOnlyList<Theater>> GetTheatersAsync(CancellationToken cancellationToken = default);

    /// <summary>Showings that start on any day from <paramref name="from"/> through <paramref name="to"/>, inclusive.</summary>
    Task<IReadOnlyList<Showing>> GetShowingsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
