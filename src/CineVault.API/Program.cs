using System.Reflection;
using Asp.Versioning;
using CineVault.API.Entities;
using CineVault.API.Extensions;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
[assembly: ApiController]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMapster();
TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

builder.Services.AddCineVaultDbContext(builder.Configuration);
builder.Services.AddScoped<RequestMonitorMiddleware>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CineVault API", Version = "1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "CineVault API", Version = "2" });
});

builder.Services.AddSerilog(configuration =>
{
    string? logLevel = builder.Configuration["Logging:LogLevel:Default"];

    bool tryParse = Enum.TryParse<LogEventLevel>(logLevel, out var logLevelEnum);

    if (!tryParse)
    {
        throw new LoggingFailedException($"Invalid log level: {logLevel}");
    }

    configuration.MinimumLevel.Is(logLevelEnum)
        .WriteTo.Console()
        .WriteTo.File("log.db");
});

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

var app = builder.Build();

// TODO 3 ���������� ����� ������������� ��������� ���� ����� �� ���������, �������������� EnsureCreated()
var dbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<CineVaultDbContext>();
dbContext.Database.EnsureCreated();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "v1");
        options.SwaggerEndpoint("v2/swagger.json", "v2");
    });
}

if (app.Environment.IsLocal())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<RequestMonitorMiddleware>();

app.MapControllers();

Console.WriteLine($"Launch Environment: {app.Environment.EnvironmentName}");

await app.RunAsync();
