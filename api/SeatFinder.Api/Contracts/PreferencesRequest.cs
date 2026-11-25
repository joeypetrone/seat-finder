using System.ComponentModel.DataAnnotations;

namespace SeatFinder.Api.Contracts;

/// <summary>
/// The body of PUT /preferences. Single-field rules live here as attributes, so [ApiController]
/// rejects them before the controller runs. Rules that compare fields or need theater data live in
/// <see cref="Validation.PreferenceRules"/>.
/// </summary>
/// <remarks>
/// Init properties rather than a positional record: on a positional record, MVC names validation
/// errors after the constructor parameter ("PartySize") instead of the JSON field ("partySize").
/// Value types are nullable so a missing field is a "required" error instead of silently becoming 0 or false.
/// </remarks>
public record PreferencesRequest
{
    [Required] public string? PreferredRowFrom { get; init; }
    [Required] public string? PreferredRowTo { get; init; }
    [Required] public int? CenterSeatFrom { get; init; }
    [Required] public int? CenterSeatTo { get; init; }
    [Required] public bool? AvoidAisles { get; init; }
    [Required, Range(1, 8)] public int? PartySize { get; init; }
    [Required, Range(1, 50)] public int? SearchRadiusMiles { get; init; }
    public int? HomeTheaterId { get; init; }
}
