namespace SeatFinder.Api.Models;

public record Auditorium(
    int Id,
    string Name,
    IReadOnlyList<Seat> Seats
);
