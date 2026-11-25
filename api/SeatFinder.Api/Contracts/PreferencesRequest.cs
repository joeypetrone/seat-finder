using System.ComponentModel.DataAnnotations;

namespace SeatFinder.Api.Contracts;

/// <summary>
/// The body of PUT /preferences. Single-field rules live here as attributes, so [ApiController]
/// rejects them before the controller runs. Rules that compare fields or need theater data live in
/// <see cref="Validation.PreferenceRules"/>.
/// </summary>
/// <remarks>
/// Value types are nullable so a missing field is a "required" error instead of silently becoming 0 or false.
/// </remarks>
public record PreferencesRequest(
    [property: Required] string? PreferredRowFrom,
    [property: Required] string? PreferredRowTo,
    [property: Required] int? CenterSeatFrom,
    [property: Required] int? CenterSeatTo,
    [property: Required] bool? AvoidAisles,
    [property: Required, Range(1, 8)] int? PartySize,
    [property: Required, Range(1, 50)] int? SearchRadiusMiles,
    int? HomeTheaterId
);
