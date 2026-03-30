using System.Net;
using System.Text.Json;
using NSubstitute;
using TimeTile.Core.Interfaces;
using TimeTile.Core.Models;
using TimeTile.Infrastructure.ApiClients.AzureDevOps;
using TimeTile.Infrastructure.ApiClients.SevenPace;
using TimeTile.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace TimeTile.Tests.Services;

public class WorkItemServiceTests
{
    private readonly IAzureDevOpsAuthService _authService;

    public WorkItemServiceTests()
    {
        _authService = Substitute.For<IAzureDevOpsAuthService>();
        _authService.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns("fake-token");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsWorkItem()
    {
        var handler = new FakeHttpHandler();
        handler.SetResponse("_apis/wit/workitems", new
        {
            value = new[]
            {
                new
                {
                    id = 42,
                    fields = new Dictionary<string, object>
                    {
                        ["System.Title"] = "Fix the bug",
                        ["System.State"] = "Active",
                        ["System.WorkItemType"] = "Task",
                        ["System.TeamProject"] = "MyProject"
                    }
                }
            }
        });

        var service = CreateService(handler);
        var item = await service.GetByIdAsync(42);

        Assert.NotNull(item);
        Assert.Equal(42, item.Id);
        Assert.Equal("Fix the bug", item.Title);
    }

    [Fact]
    public async Task GetProjectsAsync_ReturnsProjectNames()
    {
        var handler = new FakeHttpHandler();
        handler.SetResponse("_apis/projects", new
        {
            value = new[]
            {
                new { name = "ProjectA" },
                new { name = "ProjectB" }
            }
        });

        var service = CreateService(handler);
        var projects = await service.GetProjectsAsync();

        Assert.Equal(2, projects.Count);
        Assert.Contains("ProjectA", projects);
        Assert.Contains("ProjectB", projects);
    }

    [Fact]
    public async Task GetTeamsAsync_ReturnsTeams()
    {
        var handler = new FakeHttpHandler();
        handler.SetResponse("_apis/projects/MyProject/teams", new
        {
            value = new[]
            {
                new { id = "t1", name = "Team Alpha" },
                new { id = "t2", name = "Team Beta" }
            }
        });

        var service = CreateService(handler);
        var teams = await service.GetTeamsAsync("MyProject");

        Assert.Equal(2, teams.Count);
        Assert.Equal("Team Alpha", teams[0].Name);
    }

    [Fact]
    public async Task GetSprintsAsync_ReturnsSprints()
    {
        var handler = new FakeHttpHandler();
        handler.SetResponse("_apis/work/teamsettings/iterations", new
        {
            value = new[]
            {
                new
                {
                    id = "s1",
                    name = "Sprint 1",
                    path = "MyProject\\Sprint 1",
                    attributes = new
                    {
                        startDate = "2026-03-23T00:00:00Z",
                        finishDate = "2026-04-05T00:00:00Z"
                    }
                }
            }
        });

        var service = CreateService(handler);
        var sprints = await service.GetSprintsAsync("MyProject", "MyTeam");

        Assert.Single(sprints);
        Assert.Equal("Sprint 1", sprints[0].Name);
    }

    [Fact]
    public async Task SearchAsync_WithNumericQuery_SearchesByWiql()
    {
        // 7pace throws, so it falls through to AzDO WIQL
        var sevenPaceHandler = new FakeHttpHandler();
        sevenPaceHandler.SetStatusCode(HttpStatusCode.InternalServerError);

        var azDoHandler = new FakeHttpHandler();
        // WIQL query returns work item IDs
        azDoHandler.SetResponse("_apis/wit/wiql", new
        {
            workItems = new[] { new { id = 42 } }
        });
        // GetWorkItemsByIds returns full objects
        azDoHandler.SetResponse("_apis/wit/workitems", new
        {
            value = new[]
            {
                new
                {
                    id = 42,
                    fields = new Dictionary<string, object>
                    {
                        ["System.Title"] = "Found item",
                        ["System.State"] = "Active",
                        ["System.WorkItemType"] = "Task",
                        ["System.TeamProject"] = "TestProject"
                    }
                }
            }
        });

        var service = CreateService(azDoHandler, sevenPaceHandler);
        var results = await service.SearchAsync("42");

        Assert.Single(results);
        Assert.Equal(42, results[0].Id);
    }

    private WorkItemService CreateService(FakeHttpHandler azDoHandler, FakeHttpHandler? sevenPaceHandler = null)
    {
        var azDoClient = new AzureDevOpsApiClient(
            new HttpClient(azDoHandler) { BaseAddress = new Uri("https://dev.azure.com/test/") },
            _authService);

        var spHandler = sevenPaceHandler ?? new FakeHttpHandler();
        spHandler.SetStatusCode(HttpStatusCode.NotFound); // Default to not found for 7pace
        var logger = Substitute.For<ILogger<SevenPaceApiClient>>();
        var sevenPaceClient = new SevenPaceApiClient(
            new HttpClient(spHandler) { BaseAddress = new Uri("https://api.7pace.com/") },
            logger);

        return new WorkItemService(sevenPaceClient, azDoClient);
    }
}

/// <summary>
/// Test HTTP handler that returns preconfigured responses based on URL path matching.
/// </summary>
internal sealed class FakeHttpHandler : HttpMessageHandler
{
    private readonly Dictionary<string, string> _responses = new(StringComparer.OrdinalIgnoreCase);
    private HttpStatusCode _defaultStatusCode = HttpStatusCode.OK;

    public void SetResponse(string urlContains, object responseBody)
    {
        _responses[urlContains] = JsonSerializer.Serialize(responseBody);
    }

    public void SetStatusCode(HttpStatusCode statusCode)
    {
        _defaultStatusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.ToString() ?? "";
        foreach (var (key, json) in _responses)
        {
            if (url.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                });
            }
        }

        return Task.FromResult(new HttpResponseMessage(_defaultStatusCode)
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        });
    }
}
