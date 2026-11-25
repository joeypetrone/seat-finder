using System.ComponentModel.DataAnnotations;

namespace SeatFinder.Api.Contracts;

/// <summary>
/// The body of PUT /preferences. Single-field rules live here as attributes, so [ApiController]
/// rejects them before the controller runs. Rules that compare fields or need theater data live in
/// <see cref="Validation.PreferenceRules"/>.
/// </summary>
/// <remarks>
/// Attributes go on the constructor parameters (no "property:" target): that is where MVC looks for them on a record.
/// Value types are nullable so a missing field is a "required" error instead of silently becoming 0 or false.
/// </remarks>
public record PreferencesRequest(
    [Required] string? PreferredRowFrom,
    [Required] string? PreferredRowTo,
    [Required] int? CenterSeatFrom,
    [Required] int? CenterSeatTo,
    [Required] bool? AvoidAisles,
    [Required, Range(1, 8)] int? PartySize,
    [Required, Range(1, 50)] int? SearchRadiusMiles,
    int? HomeTheaterId
);
