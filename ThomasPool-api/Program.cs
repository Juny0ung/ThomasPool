using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Text.Json.Nodes;
using ThomasPool.Domain.Interfaces;
using ThomasPool.Infra.Persistence;

BsonSerializer.RegisterSerializer(typeof(JsonNode), JsonNodeSerializer.Instance);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new MongoClient(config["MongoDB:ConnectionString"]);
});

builder.Services.AddScoped<IAdminRepository, MongoAdminRepository>();
builder.Services.AddScoped<IProfileRepository, MongoProfileRepository>();
builder.Services.AddScoped<IProfileFormRepository, MongoProfileFormRepository>();

builder.Services.AddTransient<IDbInitializer, MongoProfileInitializer>();
builder.Services.AddTransient<IDbInitializer, MongoProfileFormInitializer>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var initializers = services.GetServices<IDbInitializer>();

    foreach (var initializer in initializers)
    {
        await initializer.EnsureIndexesAsync(services, builder.Configuration);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
