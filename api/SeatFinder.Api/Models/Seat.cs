namespace SeatFinder.Api.Models;

public record Seat(
    string Row,
    int Number,
    bool IsAisle,
    bool IsAccessible
);
