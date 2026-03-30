namespace Syon.TimeDashboard.Core.Models;

public record UserProfile
{
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string AvatarUrl { get; init; } = string.Empty;
}
