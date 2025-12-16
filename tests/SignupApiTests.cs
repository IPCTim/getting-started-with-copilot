using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MergingtonHighSchool.Tests;

public class SignupApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SignupApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SignupForActivity_SuccessfulSignup_ReturnsOk()
    {
        // Arrange
        var activityName = "Chess Club";
        var email = "newstudent@mergington.edu";
        var payload = new { email };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SignupForActivity_NonExistentActivity_ReturnsNotFound()
    {
        // Arrange
        var activityName = "NonExistent Activity";
        var email = "student@mergington.edu";
        var payload = new { email };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SignupForActivity_DuplicateEnrollment_ReturnsConflict()
    {
        // Arrange
        var activityName = "Programming Class";
        var email = "duplicate@mergington.edu";
        var payload = new { email };

        // Act - First signup
        var firstResponse = await _client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);
        Assert.True(firstResponse.IsSuccessStatusCode);

        // Act - Second signup attempt
        var secondResponse = await _client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);
        
        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task SignupForActivity_AddsParticipantToActivity()
    {
        // Arrange
        var activityName = "Gym Class";
        var email = "newgymstudent@mergington.edu";
        var payload = new { email };

        // Act
        await _client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);
        
        // Verify participant was added
        var activities = await _client.GetFromJsonAsync<Dictionary<string, Models.Activity>>("/api/activities");
        
        // Assert
        Assert.NotNull(activities);
        var activity = activities[activityName];
        Assert.Contains(email, activity.Participants);
    }

    [Theory]
    [InlineData("Art Workshop", "artist1@mergington.edu")]
    [InlineData("Drama Club", "actor1@mergington.edu")]
    [InlineData("Math Circle", "mathematician1@mergington.edu")]
    public async Task SignupForActivity_MultipleActivities_AllSucceed(string activityName, string email)
    {
        // Arrange
        var payload = new { email };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);
        
        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task SignupForActivity_AlreadyEnrolledStudent_ReturnsConflictMessage()
    {
        // Arrange
        var activityName = "Science Club";
        var email = "existingstudent@mergington.edu";
        var payload = new { email };

        // First signup
        await _client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);

        // Act - Second signup
        var response = await _client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(content);
        Assert.Contains("already enrolled", content["detail"], StringComparison.OrdinalIgnoreCase);
    }
}
