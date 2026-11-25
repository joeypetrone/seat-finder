namespace SeatFinder.Api.Validation;

/// <summary>
/// Rules for a requested range of showtime days. Kept separate from the controller so the
/// rules can be tested directly, and reused by the ranked endpoint and the agent's tools later.
/// </summary>
public static class DateRangeRules
{
    public const int MaxDays = 14;

    /// <summary>
    /// Checks an inclusive range of days. <paramref name="today"/> is passed in rather than read
    /// from the clock so the rules give the same answer in a test on any date.
    /// </summary>
    /// <returns>Errors keyed by the query parameter they belong to; empty when the range is valid.</returns>
    public static Dictionary<string, string[]> Validate(DateOnly from, DateOnly to, DateOnly today)
    {
        var errors = new Dictionary<string, string[]>();

        if (from < today)
        {
            errors["from"] = [$"'from' can't be in the past (today is {today:yyyy-MM-dd})."];
        }

        if (from > to)
        {
            errors["to"] = ["'to' must be on or after 'from'."];
        }
        else if (to.DayNumber - from.DayNumber + 1 > MaxDays)
        {
            errors["to"] = [$"The range can cover at most {MaxDays} days, including both ends."];
        }

        return errors;
    }
}
