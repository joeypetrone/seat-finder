using SeatFinder.Api.Validation;

namespace SeatFinder.Api.Tests;

public class DateRangeRulesTests
{
    private static readonly DateOnly Today = new(2026, 12, 1);

    [Theory]
    [InlineData(0, 0)]   // just today
    [InlineData(0, 13)]  // exactly 14 days, counting both ends
    [InlineData(5, 9)]   // entirely in the future
    public void ValidRange_HasNoErrors(int fromOffset, int toOffset)
    {
        var errors = DateRangeRules.Validate(Today.AddDays(fromOffset), Today.AddDays(toOffset), Today);

        Assert.Empty(errors);
    }

    [Fact]
    public void FromAfterTo_IsRejectedOnTo()
    {
        var errors = DateRangeRules.Validate(Today.AddDays(3), Today.AddDays(2), Today);

        Assert.Equal(["to"], errors.Keys);
    }

    [Fact]
    public void FifteenDays_IsRejectedOnTo()
    {
        var errors = DateRangeRules.Validate(Today, Today.AddDays(14), Today);

        Assert.Equal(["to"], errors.Keys);
    }

    [Fact]
    public void FromInThePast_IsRejectedOnFrom()
    {
        var errors = DateRangeRules.Validate(Today.AddDays(-1), Today.AddDays(1), Today);

        Assert.Equal(["from"], errors.Keys);
    }

    [Fact]
    public void SeveralProblems_AreAllReported()
    {
        var errors = DateRangeRules.Validate(Today.AddDays(-1), Today.AddDays(-5), Today);

        Assert.Equal(2, errors.Count);
        Assert.Contains("from", errors.Keys);
        Assert.Contains("to", errors.Keys);
    }
}
