using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeatFinder.Api.Contracts;
using SeatFinder.Api.Data;
using SeatFinder.Api.Providers;
using SeatFinder.Api.Validation;

namespace SeatFinder.Api.Controllers;

[ApiController]
[Route("preferences")]
public class PreferencesController(SeatFinderDbContext db, IShowtimeProvider provider) : ControllerBase
{
    /// <summary>The saved preferences, or the defaults if nothing has been saved yet.</summary>
    [HttpGet]
    [ProducesResponseType<PreferencesResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PreferencesResponse>> Get(CancellationToken cancellationToken)
    {
        // AsNoTracking: this is read-only, so EF doesn't need to watch the row for changes.
        var saved = await db.SeatPreferences.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == SeatPreference.SingletonId, cancellationToken);

        return ToResponse(saved ?? SeatPreference.CreateDefault());
    }

    /// <summary>Replaces the saved preferences.</summary>
    /// <response code="400">A field is missing or out of range, or the fields don't fit together or match the theaters.</response>
    [HttpPut]
    [ProducesResponseType<PreferencesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PreferencesResponse>> Put(PreferencesRequest request, CancellationToken cancellationToken)
    {
        // Missing fields and out-of-range numbers never get here: [ApiController] answers those with a 400.
        var candidate = new SeatPreference
        {
            Id = SeatPreference.SingletonId,
            PreferredRowFrom = request.PreferredRowFrom!.Trim().ToUpperInvariant(),
            PreferredRowTo = request.PreferredRowTo!.Trim().ToUpperInvariant(),
            CenterSeatFrom = request.CenterSeatFrom!.Value,
            CenterSeatTo = request.CenterSeatTo!.Value,
            AvoidAisles = request.AvoidAisles!.Value,
            PartySize = request.PartySize!.Value,
            SearchRadiusMiles = request.SearchRadiusMiles!.Value,
            HomeTheaterId = request.HomeTheaterId,
        };

        var theaters = await provider.GetTheatersAsync(cancellationToken);
        var errors = PreferenceRules.Validate(candidate, theaters);
        if (errors.Count > 0)
        {
            foreach (var (field, messages) in errors)
            {
                foreach (var message in messages)
                {
                    ModelState.AddModelError(field, message);
                }
            }
            // Same response shape as the automatic 400s, so the front end handles one format.
            return ValidationProblem(ModelState);
        }

        // Insert the first time, update after that. The loaded row is tracked, so EF writes only changed columns.
        var saved = await db.SeatPreferences.FindAsync([SeatPreference.SingletonId], cancellationToken);
        if (saved is null)
        {
            db.SeatPreferences.Add(candidate);
            saved = candidate;
        }
        else
        {
            db.Entry(saved).CurrentValues.SetValues(candidate);
        }
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(saved);
    }

    private static PreferencesResponse ToResponse(SeatPreference p) => new(
        p.PreferredRowFrom,
        p.PreferredRowTo,
        p.CenterSeatFrom,
        p.CenterSeatTo,
        p.AvoidAisles,
        p.PartySize,
        p.SearchRadiusMiles,
        p.HomeTheaterId);
}
