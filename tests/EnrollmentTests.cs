using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MergingtonHighSchool.Tests;

public class EnrollmentTests
{
    [Fact]
    public async Task UserCannotEnrollMoreThanOnce()
    {
        await using var app = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>();
        using var client = app.CreateClient();

        var activityName = "Chess Club";
        var email = "repeat@mergington.edu";

        var payload = new { email };

        // First signup
        var first = await client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);
        Assert.True(first.IsSuccessStatusCode);

        // Second signup should be blocked
        var second = await client.PostAsJsonAsync($"/api/activities/{activityName}/signup", payload);
        Assert.Equal(System.Net.HttpStatusCode.Conflict, second.StatusCode);

        // Verify participant appears only once
        var activities = await client.GetFromJsonAsync<Dictionary<string, Models.Activity>>("/api/activities");
        Assert.NotNull(activities);
        var activity = activities![activityName];
        var occurrences = activity.Participants.Count(p => p == email);
        Assert.Equal(1, occurrences);
    }
}
