using System.Text.Json;
using SeatFinder.Api.Data;
using SeatFinder.Api.Models;

namespace SeatFinder.Api.Validation;

/// <summary>
/// Rules for saved preferences that depend on other fields or on the theaters. Takes the entity
/// rather than the request so the agent's tools can check preferences the same way later.
/// </summary>
public static class PreferenceRules
{
    /// <summary>
    /// Checks rows and seats against every auditorium the provider knows. Layouts differ, so a row
    /// or seat counts as real if at least one auditorium has it.
    /// </summary>
    /// <returns>Errors keyed by the property they belong to; empty when the preferences are valid.</returns>
    public static Dictionary<string, string[]> Validate(SeatPreference preference, IReadOnlyList<Theater> theaters)
    {
        var errors = new Dictionary<string, string[]>();
        var seats = theaters.SelectMany(t => t.Auditoriums).SelectMany(a => a.Seats).ToList();

        // Front to back: "B" before "M", and a longer label like "AA" behind all single letters.
        var rows = seats.Select(s => s.Row).Distinct()
            .OrderBy(r => r.Length).ThenBy(r => r, StringComparer.Ordinal)
            .ToList();
        var rowFromIndex = rows.IndexOf(preference.PreferredRowFrom);
        var rowToIndex = rows.IndexOf(preference.PreferredRowTo);
        var rowList = rows.Count > 0 ? $"{rows[0]}–{rows[^1]}" : "none";

        if (rowFromIndex < 0)
        {
            errors[Key(nameof(SeatPreference.PreferredRowFrom))] = [$"Row '{preference.PreferredRowFrom}' doesn't exist in any auditorium (rows are {rowList})."];
        }
        if (rowToIndex < 0)
        {
            errors[Key(nameof(SeatPreference.PreferredRowTo))] = [$"Row '{preference.PreferredRowTo}' doesn't exist in any auditorium (rows are {rowList})."];
        }
        else if (rowFromIndex >= 0 && rowFromIndex > rowToIndex)
        {
            errors[Key(nameof(SeatPreference.PreferredRowTo))] = ["'preferredRowTo' must be the same row as 'preferredRowFrom' or behind it."];
        }

        var maxSeat = seats.Count > 0 ? seats.Max(s => s.Number) : 0;
        if (preference.CenterSeatFrom < 1 || preference.CenterSeatFrom > maxSeat)
        {
            errors[Key(nameof(SeatPreference.CenterSeatFrom))] = [$"Seat numbers run from 1 to {maxSeat}."];
        }
        if (preference.CenterSeatTo < 1 || preference.CenterSeatTo > maxSeat)
        {
            errors[Key(nameof(SeatPreference.CenterSeatTo))] = [$"Seat numbers run from 1 to {maxSeat}."];
        }
        else if (preference.CenterSeatFrom > preference.CenterSeatTo)
        {
            errors[Key(nameof(SeatPreference.CenterSeatTo))] = ["'centerSeatTo' must be on or after 'centerSeatFrom'."];
        }

        if (preference.HomeTheaterId is int theaterId && theaters.All(t => t.Id != theaterId))
        {
            errors[Key(nameof(SeatPreference.HomeTheaterId))] = [$"Theater {theaterId} doesn't exist."];
        }

        return errors;
    }

    // Matches the JSON field names, so these errors use the same keys as the automatic 400s.
    private static string Key(string propertyName) => JsonNamingPolicy.CamelCase.ConvertName(propertyName);
}
