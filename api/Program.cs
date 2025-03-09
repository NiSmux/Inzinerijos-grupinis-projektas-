var builder = WebApplication.CreateBuilder(args);

//CORS integracija
builder.Services.AddCors(options =>
{
    options.AddPolicy("DataPolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:5293",
                                "https://localhost:7066")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
        });
});

// Pridedame paslaugas į konteinerį
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Originalus weatherforecast endpoint'as
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Naujas endpoint'as: /api/tasks
app.MapGet("/api/tasks", () =>
{
    var tasks = new[]
    {
        new { Id = 1, Title = "Pirma užduotis", Description = "Trumpas aprašymas" },
        new { Id = 2, Title = "Antra užduotis", Description = "Kitas aprašymas" }
    };
    return Results.Ok(tasks);
})
.WithName("GetTasks");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
