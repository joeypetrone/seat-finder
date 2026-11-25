namespace SeatFinder.Api.Contracts;

/// <summary>The saved preferences as GET and PUT /preferences return them. Leaves out the internal row id.</summary>
public record PreferencesResponse(
    string PreferredRowFrom,
    string PreferredRowTo,
    int CenterSeatFrom,
    int CenterSeatTo,
    bool AvoidAisles,
    int PartySize,
    int SearchRadiusMiles,
    int? HomeTheaterId
);
