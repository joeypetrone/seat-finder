namespace SeatFinder.Api.Data;

/// <summary>
/// The saved seat preferences. A class with settable properties rather than a record, because
/// EF Core tracks changes to an entity it loaded and writes only what changed.
/// </summary>
public class SeatPreference
{
    // v1 has no accounts, so there is exactly one row and it always has this id.
    public const int SingletonId = 1;

    public int Id { get; set; }

    /// <summary>Front-most preferred row, e.g. "E". Rows are lettered from the screen back.</summary>
    public string PreferredRowFrom { get; set; } = "";

    /// <summary>Back-most preferred row, e.g. "J".</summary>
    public string PreferredRowTo { get; set; } = "";

    /// <summary>First seat number of the preferred middle stretch of a row, counted from the left.</summary>
    public int CenterSeatFrom { get; set; }

    /// <summary>Last seat number of the preferred middle stretch of a row.</summary>
    public int CenterSeatTo { get; set; }

    public bool AvoidAisles { get; set; }

    /// <summary>How many seats together.</summary>
    public int PartySize { get; set; }

    public int SearchRadiusMiles { get; set; }

    /// <summary>A theater from the showtime provider, or null for "any theater in range".</summary>
    public int? HomeTheaterId { get; set; }

    /// <summary>What GET returns before anything has been saved, so the form always has values to show.</summary>
    public static SeatPreference CreateDefault() => new()
    {
        Id = SingletonId,
        PreferredRowFrom = "E",
        PreferredRowTo = "H",
        CenterSeatFrom = 7,
        CenterSeatTo = 14,
        AvoidAisles = false,
        PartySize = 2,
        SearchRadiusMiles = 25,
        HomeTheaterId = null,
    };
}
