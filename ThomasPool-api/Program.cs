using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Text;
using System.Text.Json.Nodes;
using ThomasPool.Application.Services;
using ThomasPool.Domain.Interfaces;
using ThomasPool.Infra.Authentication;
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

builder.Services.AddTransient<IDbInitializer, MongoAdminInitializer>();
builder.Services.AddTransient<IDbInitializer, MongoProfileInitializer>();
builder.Services.AddTransient<IDbInitializer, MongoProfileFormInitializer>();

builder.Services.AddSingleton<IJwtProvider, JwtProvider>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<ProfileService>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    });

builder.Services.AddAuthorization();

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

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Healthy");

app.MapControllers();


app.Run();
