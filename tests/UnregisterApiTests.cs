using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MergingtonHighSchool.Tests;

public class UnregisterApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UnregisterApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UnregisterFromActivity_ExistingParticipant_ReturnsOk()
    {
        // Arrange
        var activityName = "Soccer Team";
        var email = "newsoccer@mergington.edu";
        var payload = new { email };
        
        // First, sign up the student
        await _client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);

        // Act - Unregister the student
        var response = await _client.DeleteAsync($"/api/activities/{activityName}/signup/{email}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnregisterFromActivity_NonExistentActivity_ReturnsNotFound()
    {
        // Arrange
        var activityName = "NonExistent Activity";
        var email = "student@mergington.edu";

        // Act
        var response = await _client.DeleteAsync($"/api/activities/{activityName}/signup/{email}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnregisterFromActivity_NonEnrolledStudent_ReturnsNotFound()
    {
        // Arrange
        var activityName = "Swimming Club";
        var email = "notenrolled@mergington.edu";

        // Act
        var response = await _client.DeleteAsync($"/api/activities/{activityName}/signup/{email}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnregisterFromActivity_RemovesParticipantFromActivity()
    {
        // Arrange
        var activityName = "Chess Club";
        var email = "tempchess@mergington.edu";
        var payload = new { email };
        
        // Sign up
        await _client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);

        // Act - Unregister
        await _client.DeleteAsync($"/api/activities/{activityName}/signup/{email}");
        
        // Verify participant was removed
        var activities = await _client.GetFromJsonAsync<Dictionary<string, Models.Activity>>("/api/activities");
        
        // Assert
        Assert.NotNull(activities);
        var activity = activities[activityName];
        Assert.DoesNotContain(email, activity.Participants);
    }

    [Fact]
    public async Task UnregisterFromActivity_ReturnsSuccessMessage()
    {
        // Arrange
        var activityName = "Programming Class";
        var email = "tempdev@mergington.edu";
        var payload = new { email };
        
        // Sign up
        await _client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);

        // Act
        var response = await _client.DeleteAsync($"/api/activities/{activityName}/signup/{email}");
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.Contains("unregistered", content["message"], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnregisterFromActivity_NotFoundReturnsErrorDetail()
    {
        // Arrange
        var activityName = "Drama Club";
        var email = "notinclub@mergington.edu";

        // Act
        var response = await _client.DeleteAsync($"/api/activities/{activityName}/signup/{email}");
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(content);
        Assert.Contains("not enrolled", content["detail"], StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Art Workshop", "artist2@mergington.edu")]
    [InlineData("Math Circle", "mathematician2@mergington.edu")]
    [InlineData("Science Club", "scientist2@mergington.edu")]
    public async Task UnregisterFromActivity_MultipleActivities_AllSucceed(string activityName, string email)
    {
        // Arrange
        var payload = new { email };
        await _client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);

        // Act
        var response = await _client.DeleteAsync($"/api/activities/{activityName}/signup/{email}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
