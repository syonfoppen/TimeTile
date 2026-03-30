using Fluxor;
using Microsoft.Extensions.Logging;
using TimeTile.Core.Interfaces;

namespace TimeTile.UI.Store.Filter;

public class FilterEffects
{
    private readonly IWorkItemService _workItemService;
    private readonly ILogger<FilterEffects> _logger;

    public FilterEffects(IWorkItemService workItemService, ILogger<FilterEffects> logger)
    {
        _workItemService = workItemService;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadProjects(LoadProjectsAction _, IDispatcher dispatcher)
    {
        try
        {
            var projects = await _workItemService.GetProjectsAsync();
            dispatcher.Dispatch(new ProjectsLoadedAction(projects));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load projects");
        }
    }

    [EffectMethod]
    public async Task HandleSelectProject(SelectProjectAction action, IDispatcher dispatcher)
    {
        if (action.Project is null) return;

        try
        {
            var teams = await _workItemService.GetTeamsAsync(action.Project);
            dispatcher.Dispatch(new TeamsLoadedAction(teams));

            dispatcher.Dispatch(new LoadTeamsAction(action.Project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load teams for project {Project}", action.Project);
        }
    }

    [EffectMethod]
    public async Task HandleSelectTeam(SelectTeamAction action, IDispatcher dispatcher)
    {
        if (action.Team is null) return;

        try
        {
            // Need current project from filter state - this will be handled by component
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle team selection");
        }
    }

    [EffectMethod]
    public async Task HandleLoadSprints(LoadSprintsAction action, IDispatcher dispatcher)
    {
        try
        {
            var sprints = await _workItemService.GetSprintsAsync(action.Project, action.Team);
            dispatcher.Dispatch(new SprintsLoadedAction(sprints));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load sprints for {Project}/{Team}", action.Project, action.Team);
        }
    }
}
