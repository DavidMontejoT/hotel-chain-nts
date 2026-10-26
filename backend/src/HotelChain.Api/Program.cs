using HotelChain.Api.Extensions;
using HotelChain.Api.Middleware;
using HotelChain.Application;
using HotelChain.Infrastructure;
using HotelChain.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration, builder.Environment)
    .AddFrontendCors(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var swaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled");
if (swaggerEnabled) builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(ServiceCollectionExtensions.CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await app.Services.InitializeDatabaseAsync();

app.Run();

// Exposed for the integration tests (WebApplicationFactory).
public partial class Program;
