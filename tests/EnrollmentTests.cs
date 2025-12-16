using System.Net.Http.Json;
using Xunit;

namespace MergingtonHighSchool.Tests;

public class EnrollmentTests
{
    [Fact]
    public async Task UserCanEnrollMoreThanOnce()
    {
        await using var app = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>();
        using var client = app.CreateClient();

        var activityName = "Chess Club";
        var email = "repeat@mergington.edu";

        var payload = new { email };

        // First signup
        var first = await client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);
        Assert.True(first.IsSuccessStatusCode);

        // Second signup (same user again)
        var second = await client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);
        Assert.True(second.IsSuccessStatusCode);

        // Verify participants include duplicates by counting occurrences
        var activities = await client.GetFromJsonAsync<Dictionary<string, Models.Activity>>("/api/activities");
        Assert.NotNull(activities);
        var activity = activities![activityName];
        var occurrences = activity.Participants.Count(p => p == email);
        Assert.Equal(2, occurrences);
    }
}
