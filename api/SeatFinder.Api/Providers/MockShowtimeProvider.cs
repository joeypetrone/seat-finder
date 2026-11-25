using SeatFinder.Api.Models;

namespace SeatFinder.Api.Providers;

/// <summary>
/// A made-up chain with four Nashville-area theaters. Showings are generated for any requested day,
/// and taken seats come from a random generator seeded per showing, so a showing always has the
/// same taken seats no matter which date range it was requested in.
/// </summary>
/// <remarks>
/// The chain, the street numbers, and the movies are fictional. Coordinates are real points in
/// each area so the distance filter gives believable results.
/// </remarks>
public sealed class MockShowtimeProvider : IShowtimeProvider
{
    // Changing this reshuffles every showing's taken seats.
    public const int BaseSeed = 20261125;

    private static readonly (string Title, string? Rating)[] Movies =
    [
        ("The Last Encore", "PG-13"),
        ("Midnight Rails", "R"),
        ("Paper Planets", "PG"),
        ("Harbor Lights", "PG-13"),
        ("Signal Lost", "R"),
        ("The Quiet Season", null), // new release, rating not announced yet
    ];

    private static readonly Layout Large = new(Rows: 13, Sections: [4, 12, 4], AccessibleRow: 'F');
    private static readonly Layout Small = new(Rows: 9, Sections: [3, 10, 3], AccessibleRow: 'E');
    private static readonly TimeOnly[] EarlySlots = [new(13, 0), new(16, 15), new(19, 30)];
    private static readonly TimeOnly[] LateSlots = [new(14, 0), new(17, 30), new(21, 0)];

    private readonly List<Theater> _theaters = [];
    private readonly List<ScheduledRoom> _schedule = [];

    public MockShowtimeProvider()
    {
        AddTheater(1, "Seatline Cinemas Gulch", "620 12th Ave S, Nashville, TN 37203", 36.1516, -86.7840,
            new Room("Auditorium 1", Large, EarlySlots), new Room("Auditorium 2", Small, LateSlots));
        AddTheater(2, "Seatline Cinemas Brentwood", "7020 Maryland Way, Brentwood, TN 37027", 36.0335, -86.8020,
            new Room("Auditorium 1", Large, LateSlots), new Room("Auditorium 2", Small, EarlySlots));
        AddTheater(3, "Seatline Cinemas Hendersonville", "300 Indian Lake Blvd, Hendersonville, TN 37075", 36.3046, -86.6150,
            new Room("Auditorium 1", Large, EarlySlots));
        AddTheater(4, "Seatline Cinemas Murfreesboro", "2615 Medical Center Pkwy, Murfreesboro, TN 37129", 35.8530, -86.4300,
            new Room("Auditorium 1", Large, LateSlots), new Room("Auditorium 2", Small, EarlySlots));
    }

    // IANA id works on Windows too (.NET uses ICU), so the same code runs on any OS.
    public TimeZoneInfo TimeZone { get; } = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");

    public Task<IReadOnlyList<Theater>> GetTheatersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Theater>>(_theaters);

    public Task<IReadOnlyList<Showing>> GetShowingsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var showings = new List<Showing>();
        for (var day = from; day <= to; day = day.AddDays(1))
        {
            foreach (var room in _schedule)
            {
                for (var slot = 0; slot < room.Times.Length; slot++)
                {
                    showings.Add(BuildShowing(room, day, slot));
                }
            }
        }

        IReadOnlyList<Showing> sorted = showings.OrderBy(s => s.StartsAt).ThenBy(s => s.AuditoriumId).ToList();
        return Task.FromResult(sorted);
    }

    private void AddTheater(int id, string name, string address, double latitude, double longitude, params Room[] rooms)
    {
        // Auditorium ids are unique across theaters: theater 1 has 11 and 12, theater 2 has 21 and 22.
        var auditoriums = rooms
            .Select((room, i) => new Auditorium(id * 10 + i + 1, room.Name, BuildSeats(room.Layout)))
            .ToList();

        _theaters.Add(new Theater(id, name, address, latitude, longitude, auditoriums));
        for (var i = 0; i < rooms.Length; i++)
        {
            _schedule.Add(new ScheduledRoom(id, auditoriums[i], rooms[i].Layout, rooms[i].Times));
        }
    }

    private Showing BuildShowing(ScheduledRoom room, DateOnly day, int slot)
    {
        // The id comes from the day, auditorium, and slot so it (and the seed) is the same on every request.
        // Day numbers are about 740,000 for current dates, so times 1,000 still fits in an int.
        var id = day.DayNumber * 1000 + room.Auditorium.Id * 10 + slot;
        var movie = Movies[(day.DayNumber + room.Auditorium.Id + slot) % Movies.Length];

        var local = day.ToDateTime(room.Times[slot]);
        var startsAt = new DateTimeOffset(local, TimeZone.GetUtcOffset(local));

        var random = new Random(BaseSeed + id);
        var fill = FillRate(day, room.Times[slot]);
        var seatsPerRow = room.Layout.Sections.Sum();

        var seats = room.Auditorium.Seats
            .Select(seat =>
            {
                // Seats near the middle of the room sell first, so the best seats are the scarce ones
                // and the scoring has a real choice to make.
                var across = Math.Abs(seat.Number - (seatsPerRow + 1) / 2.0) / (seatsPerRow / 2.0);
                var depth = Math.Abs(seat.Row[0] - 'A' - room.Layout.Rows * 0.6) / room.Layout.Rows;
                var chance = fill * (1.3 - 0.5 * Math.Min(across + depth, 1.2));
                var status = random.NextDouble() < chance ? SeatStatus.Taken : SeatStatus.Open;
                return new ShowingSeat(seat, status);
            })
            .ToList();

        return new Showing(id, room.TheaterId, room.Auditorium.Id, movie.Title, movie.Rating, startsAt, seats);
    }

    // Weekend evenings are busiest, weekday matinees quietest.
    private static double FillRate(DateOnly day, TimeOnly time)
    {
        var weekend = day.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday or DayOfWeek.Sunday;
        var evening = time.Hour >= 18;
        return (weekend, evening) switch
        {
            (true, true) => 0.7,
            (true, false) or (false, true) => 0.45,
            (false, false) => 0.2,
        };
    }

    private static List<Seat> BuildSeats(Layout layout)
    {
        var seats = new List<Seat>();
        for (var r = 0; r < layout.Rows; r++)
        {
            var row = (char)('A' + r);
            var number = 1;
            foreach (var size in layout.Sections)
            {
                for (var i = 0; i < size; i++, number++)
                {
                    // Every section has an aisle on both sides; the outer ones run along the walls.
                    var isAisle = i == 0 || i == size - 1;
                    var isAccessible = row == layout.AccessibleRow && isAisle;
                    seats.Add(new Seat(row.ToString(), number, isAisle, isAccessible));
                }
            }
        }
        return seats;
    }

    // Rows are lettered from the screen back. Sections are seat counts between aisles, left to right.
    private sealed record Layout(int Rows, int[] Sections, char AccessibleRow);

    private sealed record Room(string Name, Layout Layout, TimeOnly[] Times);

    private sealed record ScheduledRoom(int TheaterId, Auditorium Auditorium, Layout Layout, TimeOnly[] Times);
}
