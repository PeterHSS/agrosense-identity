using Api;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependecyInjection(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.Services.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseAuthentication();

app.UseExceptionHandler();

app.MapControllers();

app.UseHealthChecks("/health", new HealthCheckOptions { ResponseWriter = async (context, report) => { context.Response.ContentType = "text/plain"; await context.Response.WriteAsync("OK"); } });

app.Run();
