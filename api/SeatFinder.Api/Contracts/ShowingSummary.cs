namespace SeatFinder.Api.Contracts;

/// <summary>
/// One showing as the list endpoint returns it. Leaves out the per-seat map: two weeks of
/// showings with every seat would be several megabytes, and the list only needs the counts.
/// </summary>
public record ShowingSummary(
    int Id,
    int TheaterId,
    string TheaterName,
    int AuditoriumId,
    string AuditoriumName,
    string MovieTitle,
    string? Rating,
    DateTimeOffset StartsAt,
    int OpenSeats,
    int TotalSeats
);
