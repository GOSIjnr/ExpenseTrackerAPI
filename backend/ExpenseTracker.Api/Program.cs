using System.Text.Json;
using System.Text.Json.Serialization;
using ExpenseTracker.Api.Authentication;
using ExpenseTracker.Api.Constants.Auth;
using ExpenseTracker.Api.Extensions.Claims;
using ExpenseTracker.Api.Extensions.Cors;
using ExpenseTracker.Api.Extensions.Endpoints;
using ExpenseTracker.Api.Extensions.OpenApi;
using ExpenseTracker.Api.Extensions.RateLimiting;
using ExpenseTracker.Api.Hosting;
using ExpenseTracker.Api.Middleware;
using ExpenseTracker.Application;
using ExpenseTracker.Application.CQRS.Messaging;
using ExpenseTracker.Infrastructure;
using ExpenseTracker.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Json;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomCors(builder.Configuration);
builder.Services.AddEndpointRateLimiting(builder.Configuration);

builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices([typeof(IMediator).Assembly]);

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointModules();

builder.Services.AddAuthentication(AuthenticationSchemes.Session)
    .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(
        AuthenticationSchemes.Session,
        options => { options.ClaimsIssuer = "ExpenseTracker"; }
    );

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthenticationResultHandler>();

builder.Services.AddAuthorizationBuilder().AddCustomPolicies();

builder.Services.Configure<JsonOptions>(opts =>
{
    JsonSerializerOptions serializer = opts.SerializerOptions;

    serializer.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    serializer.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    serializer.WriteIndented = true;
    serializer.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi(options => { options.AddCustomOpenApiTransformer(); });

builder.Services.AddHostedService<StartupTasksHostedService>();

WebApplication app = builder.Build();

app.UseCustomCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "ExpenseTracker API";
        options.DefaultHttpClient = new(ScalarTarget.Node, ScalarClient.Fetch);
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<TraceIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapEndpointModules();

app.Run();
