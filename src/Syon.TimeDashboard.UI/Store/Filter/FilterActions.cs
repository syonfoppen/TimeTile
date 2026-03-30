using Syon.TimeDashboard.Core.Models;

namespace Syon.TimeDashboard.UI.Store.Filter;

public record LoadProjectsAction;
public record ProjectsLoadedAction(IReadOnlyList<string> Projects);

public record SelectProjectAction(string? Project);
public record LoadTeamsAction(string Project);
public record TeamsLoadedAction(IReadOnlyList<Team> Teams);

public record SelectTeamAction(string? Team);
public record LoadSprintsAction(string Project, string Team);
public record SprintsLoadedAction(IReadOnlyList<Sprint> Sprints);

public record SelectIterationAction(string? Iteration);
public record ClearFiltersAction;
