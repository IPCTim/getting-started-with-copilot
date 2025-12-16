using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MergingtonHighSchool.Tests;

public class ActivityApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ActivityApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetActivities_ReturnsAllActivities()
    {
        // Act
        var response = await _client.GetAsync("/api/activities");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var activities = await response.Content.ReadFromJsonAsync<Dictionary<string, Models.Activity>>();
        
        Assert.NotNull(activities);
        Assert.NotEmpty(activities);
        Assert.True(activities.Count >= 9, "Should have at least 9 activities");
        Assert.Contains("Chess Club", activities.Keys);
        Assert.Contains("Programming Class", activities.Keys);
        Assert.Contains("Gym Class", activities.Keys);
    }

    [Fact]
    public async Task GetActivities_ReturnsCorrectActivityDetails()
    {
        // Act
        var activities = await _client.GetFromJsonAsync<Dictionary<string, Models.Activity>>("/api/activities");
        
        // Assert
        Assert.NotNull(activities);
        var chessClub = activities["Chess Club"];
        Assert.NotNull(chessClub);
        Assert.Equal("Learn strategies and compete in chess tournaments", chessClub.Description);
        Assert.Equal("Fridays, 3:30 PM - 5:00 PM", chessClub.Schedule);
        Assert.Equal(12, chessClub.MaxParticipants);
        Assert.NotEmpty(chessClub.Participants);
    }

    [Fact]
    public async Task GetActivities_ReturnsOkStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/activities");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetActivities_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/activities");
        
        // Assert
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
    }
}
