using TimeTile.Core.Models;

namespace TimeTile.Tests.Models;

public class TrackingSessionTests
{
    [Fact]
    public void IsActive_WhenStoppedAtIsNull_ReturnsTrue()
    {
        var session = new TrackingSession
        {
            WorkItemId = 42,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-30)
        };

        Assert.True(session.IsActive);
    }

    [Fact]
    public void IsActive_WhenStoppedAtHasValue_ReturnsFalse()
    {
        var session = new TrackingSession
        {
            WorkItemId = 42,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-30),
            StoppedAt = DateTimeOffset.UtcNow
        };

        Assert.False(session.IsActive);
    }

    [Fact]
    public void Elapsed_WhenStopped_ReturnsCorrectDuration()
    {
        var start = new DateTimeOffset(2026, 3, 30, 10, 0, 0, TimeSpan.Zero);
        var stop = new DateTimeOffset(2026, 3, 30, 11, 30, 0, TimeSpan.Zero);

        var session = new TrackingSession
        {
            WorkItemId = 42,
            StartedAt = start,
            StoppedAt = stop
        };

        Assert.Equal(TimeSpan.FromMinutes(90), session.Elapsed);
    }

    [Fact]
    public void Elapsed_WhenActive_ReturnsRunningDuration()
    {
        var session = new TrackingSession
        {
            WorkItemId = 42,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
        };

        // Should be approximately 10 minutes (allowing 2 second tolerance)
        Assert.InRange(session.Elapsed.TotalMinutes, 9.9, 10.1);
    }

    [Fact]
    public void Defaults_AreCorrect()
    {
        var session = new TrackingSession();

        Assert.Equal(0, session.WorkItemId);
        Assert.Equal(string.Empty, session.WorkItemTitle);
        Assert.Null(session.StoppedAt);
    }
}
