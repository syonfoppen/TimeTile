using TimeTile.Core.Models;

namespace TimeTile.Tests.Models;

public class SprintTests
{
    [Fact]
    public void IsCurrent_WhenNowIsBetweenDates_ReturnsTrue()
    {
        var sprint = new Sprint
        {
            Id = "1",
            Name = "Sprint 1",
            StartDate = DateTimeOffset.UtcNow.AddDays(-5),
            FinishDate = DateTimeOffset.UtcNow.AddDays(5)
        };

        Assert.True(sprint.IsCurrent);
    }

    [Fact]
    public void IsCurrent_WhenSprintInPast_ReturnsFalse()
    {
        var sprint = new Sprint
        {
            Id = "1",
            Name = "Sprint 1",
            StartDate = DateTimeOffset.UtcNow.AddDays(-15),
            FinishDate = DateTimeOffset.UtcNow.AddDays(-5)
        };

        Assert.False(sprint.IsCurrent);
    }

    [Fact]
    public void IsCurrent_WhenSprintInFuture_ReturnsFalse()
    {
        var sprint = new Sprint
        {
            Id = "1",
            Name = "Sprint 1",
            StartDate = DateTimeOffset.UtcNow.AddDays(5),
            FinishDate = DateTimeOffset.UtcNow.AddDays(15)
        };

        Assert.False(sprint.IsCurrent);
    }

    [Fact]
    public void IsCurrent_WhenDatesAreNull_ReturnsFalse()
    {
        var sprint = new Sprint
        {
            Id = "1",
            Name = "Sprint 1"
        };

        Assert.False(sprint.IsCurrent);
    }

    [Fact]
    public void Defaults_AreEmptyStrings()
    {
        var sprint = new Sprint();

        Assert.Equal(string.Empty, sprint.Id);
        Assert.Equal(string.Empty, sprint.Name);
        Assert.Equal(string.Empty, sprint.Path);
        Assert.Null(sprint.StartDate);
        Assert.Null(sprint.FinishDate);
    }
}
