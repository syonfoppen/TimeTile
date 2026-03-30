using Fluxor;

namespace Syon.TimeDashboard.UI.Store.Filter;

public static class FilterReducers
{
    [ReducerMethod]
    public static FilterState OnLoadProjects(FilterState state, LoadProjectsAction _) =>
        state with { IsLoadingFilters = true };

    [ReducerMethod]
    public static FilterState OnProjectsLoaded(FilterState state, ProjectsLoadedAction action) =>
        state with { Projects = action.Projects, IsLoadingFilters = false };

    [ReducerMethod]
    public static FilterState OnSelectProject(FilterState state, SelectProjectAction action) =>
        state with { SelectedProject = action.Project, SelectedTeam = null, SelectedIteration = null, Teams = [], Sprints = [] };

    [ReducerMethod]
    public static FilterState OnTeamsLoaded(FilterState state, TeamsLoadedAction action) =>
        state with { Teams = action.Teams };

    [ReducerMethod]
    public static FilterState OnSelectTeam(FilterState state, SelectTeamAction action) =>
        state with { SelectedTeam = action.Team, SelectedIteration = null, Sprints = [] };

    [ReducerMethod]
    public static FilterState OnSprintsLoaded(FilterState state, SprintsLoadedAction action) =>
        state with { Sprints = action.Sprints };

    [ReducerMethod]
    public static FilterState OnSelectIteration(FilterState state, SelectIterationAction action) =>
        state with { SelectedIteration = action.Iteration };

    [ReducerMethod]
    public static FilterState OnClearFilters(FilterState state, ClearFiltersAction _) =>
        state with { SelectedProject = null, SelectedTeam = null, SelectedIteration = null };
}
