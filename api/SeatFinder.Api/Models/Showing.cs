namespace SeatFinder.Api.Models;

public record Showing(
    int Id,
    int TheaterId,
    int AuditoriumId,
    string MovieTitle,
    string? Rating,
    DateTimeOffset StartsAt,
    IReadOnlyList<ShowingSeat> Seats
);
