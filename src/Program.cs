using MergingtonHighSchool.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JSON serialization to use camelCase
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

// In-memory activity database
var activities = new Dictionary<string, Activity>
{
    ["Chess Club"] = new Activity
    {
        Description = "Learn strategies and compete in chess tournaments",
        Schedule = "Fridays, 3:30 PM - 5:00 PM",
        MaxParticipants = 12,
        Participants = new List<string> { "michael@mergington.edu", "daniel@mergington.edu" }
    },
    ["Programming Class"] = new Activity
    {
        Description = "Learn programming fundamentals and build software projects",
        Schedule = "Tuesdays and Thursdays, 3:30 PM - 4:30 PM",
        MaxParticipants = 20,
        Participants = new List<string> { "emma@mergington.edu", "sophia@mergington.edu" }
    },
    ["Gym Class"] = new Activity
    {
        Description = "Physical education and sports activities",
        Schedule = "Mondays, Wednesdays, Fridays, 2:00 PM - 3:00 PM",
        MaxParticipants = 30,
        Participants = new List<string> { "john@mergington.edu", "olivia@mergington.edu" }
    },
    // Artistic activities
    ["Art Workshop"] = new Activity
    {
        Description = "Explore painting, drawing, and mixed media techniques",
        Schedule = "Wednesdays, 3:30 PM - 5:00 PM",
        MaxParticipants = 15,
        Participants = new List<string> { "amelia@mergington.edu" }
    },
    ["Drama Club"] = new Activity
    {
        Description = "Acting, improvisation, and stage production",
        Schedule = "Mondays, 3:30 PM - 5:00 PM",
        MaxParticipants = 25,
        Participants = new List<string> { "liam@mergington.edu" }
    },
    // Intellectual activities
    ["Math Circle"] = new Activity
    {
        Description = "Problem solving and mathematical exploration",
        Schedule = "Thursdays, 4:00 PM - 5:30 PM",
        MaxParticipants = 20,
        Participants = new List<string> { "noah@mergington.edu" }
    },
    ["Science Club"] = new Activity
    {
        Description = "Hands-on experiments and scientific inquiry",
        Schedule = "Tuesdays, 3:30 PM - 5:00 PM",
        MaxParticipants = 18,
        Participants = new List<string> { "ava@mergington.edu" }
    },
    // Sports activities
    ["Soccer Team"] = new Activity
    {
        Description = "Competitive soccer training and league matches",
        Schedule = "Tuesdays and Thursdays, 4:00 PM - 5:30 PM",
        MaxParticipants = 22,
        Participants = new List<string> { "jackson@mergington.edu", "mia@mergington.edu" }
    },
    ["Swimming Club"] = new Activity
    {
        Description = "Swimming technique development and water safety",
        Schedule = "Mondays and Wednesdays, 4:00 PM - 5:00 PM",
        MaxParticipants = 20,
        Participants = new List<string> { "ethan@mergington.edu" }
    }
};

// API Endpoints
app.MapGet("/api/activities", () => Results.Ok(activities))
    .WithName("GetActivities");

app.MapPost("/api/activities/{activityName}/signup", (string activityName, SignupRequest request) =>
{
    // Validate activity exists
    if (!activities.ContainsKey(activityName))
    {
        return Results.NotFound(new { detail = "Activity not found" });
    }

    var activity = activities[activityName];

    // Prevent duplicate enrollment
    if (activity.Participants.Contains(request.Email))
    {
        return Results.Conflict(new { detail = $"{request.Email} is already enrolled in {activityName}" });
    }

    // Add student
    activity.Participants.Add(request.Email);
    return Results.Ok(new { message = $"Signed up {request.Email} for {activityName}" });
})
    .WithName("SignupForActivity");

app.MapDelete("/api/activities/{activityName}/signup/{email}", (string activityName, string email) =>
{
    // Validate activity exists
    if (!activities.ContainsKey(activityName))
    {
        return Results.NotFound(new { detail = "Activity not found" });
    }

    var activity = activities[activityName];

    // Remove participant
    if (activity.Participants.Remove(email))
    {
        return Results.Ok(new { message = $"{email} has been unregistered from {activityName}" });
    }

    return Results.NotFound(new { detail = $"{email} is not enrolled in {activityName}" });
})
    .WithName("UnregisterFromActivity");

// SPA fallback - serve index.html for client-side routes
app.MapFallbackToFile("index.html");

app.Run();

// Allow tests to reference the entry point with WebApplicationFactory
public partial class Program { }
