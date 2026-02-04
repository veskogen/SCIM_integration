using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using ScimProvisioningApp.Services;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add logging for debugging purposes
builder.Logging.AddConsole();
builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Trace);


// Retrieve the secret token from your appsettings.json
var scimSecret = builder.Configuration["ScimProvisioning:SecretToken"];

// Add services to the container.

//Ensure JSON does not default to camelCase properties
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ScimService>(); // register service
builder.Services.AddAuthorization();

var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();



//Routing and Endpoints
app.UseHttpsRedirection(); // This is commented out when HTTP testing

app.Use(async (context, next) =>
{
    // Check if the request is for your SCIM endpoint (but allow ServiceProviderConfig and Schemas anonymously)
    if (context.Request.Path.StartsWithSegments("/scim/v2"))
    {
        var path = context.Request.Path.Value ?? string.Empty;
        // Allow Entra to fetch ServiceProviderConfig and Schemas without a secret
        if (path.Equals("/scim/v2/ServiceProviderConfig", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/scim/v2/Schemas", StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        // For all other SCIM endpoints require the secret
        var authHeader = context.Request.Headers["Authorization"].ToString();
        var expectedToken = $"Bearer {builder.Configuration["ScimProvisioning:SecretToken"]}";

        if (string.IsNullOrEmpty(authHeader) || authHeader != expectedToken)
        {
            context.Response.StatusCode = 401; // Unauthorized
            // Add WWW-Authenticate header so Entra can understand the auth challenge
            //context.Response.Headers["WWW-Authenticate"] = "Bearer realm=\"scim\", error=\"invalid_token\", error_description=\"Invalid or missing Secret Token\"";
            await context.Response.WriteAsync("Invalid or missing Secret Token.");
            return;
        }
    }
    await next();
});

// Use Authorization middleware
app.UseAuthorization();
app.MapControllers();
app.Run();

