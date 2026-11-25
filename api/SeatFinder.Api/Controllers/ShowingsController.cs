using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SeatFinder.Api.Contracts;
using SeatFinder.Api.Models;
using SeatFinder.Api.Providers;
using SeatFinder.Api.Validation;

namespace SeatFinder.Api.Controllers;

[ApiController]
[Route("showings")]
public class ShowingsController(IShowtimeProvider provider, TimeProvider clock) : ControllerBase
{
    /// <summary>Showings starting on any day from <paramref name="from"/> through <paramref name="to"/> (yyyy-MM-dd, inclusive).</summary>
    /// <response code="400">A date is missing or malformed, or the range breaks a rule.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ShowingSummary>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ShowingSummary>>> Get(
        [FromQuery, Required] DateOnly? from,
        [FromQuery, Required] DateOnly? to,
        CancellationToken cancellationToken)
    {
        // Missing or malformed dates never get here: [ApiController] answers those with a 400 itself.
        // Nullable plus [Required] is what makes a missing date an error instead of 0001-01-01.
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), provider.TimeZone).DateTime);
        var errors = DateRangeRules.Validate(from!.Value, to!.Value, today);
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

        var theaters = await provider.GetTheatersAsync(cancellationToken);
        var showings = await provider.GetShowingsAsync(from.Value, to.Value, cancellationToken);

        var theaterById = theaters.ToDictionary(t => t.Id);
        var auditoriumById = theaters.SelectMany(t => t.Auditoriums).ToDictionary(a => a.Id);

        return showings
            .Select(s => new ShowingSummary(
                s.Id,
                s.TheaterId,
                theaterById[s.TheaterId].Name,
                s.AuditoriumId,
                auditoriumById[s.AuditoriumId].Name,
                s.MovieTitle,
                s.Rating,
                s.StartsAt,
                s.Seats.Count(seat => seat.Status == SeatStatus.Open),
                s.Seats.Count))
            .ToList();
    }
}
