namespace SeatFinder.Api.Models;

public record Theater(
    int Id,
    string Name,
    string Address,
    double Latitude,
    double Longitude,
    IReadOnlyList<Auditorium> Auditoriums
);
