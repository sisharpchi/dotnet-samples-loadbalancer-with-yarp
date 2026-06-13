using MarketplaceSample.Api.Extensions;
using MarketplaceSample.Api.Health;
using MarketplaceSample.Application;
using MarketplaceSample.Infrastructure;
using MarketplaceSample.Infrastructure.Database;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;

namespace MarketplaceSample.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddMarketplaceOpenIddict();
        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddControllers();
        builder.Services.AddAuthorization();
        builder.Services.AddHealthChecks()
            .AddCheck<ConfigurableLivenessHealthCheck>("live", tags: ["live"])
            .AddCheck<ConfigurableReadinessHealthCheck>("ready", tags: ["ready"]);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(s =>
        {
            s.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI", Version = "v1" });
            s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "OpenIddict access token using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            s.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme
                    },
                    []
                }
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI v1"));
            app.UseHsts();
        }

        await app.InitializeDatabaseAsync();

        app.UseHealthChecks("/health");
        app.UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live")
        });
        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready")
        });

        //app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
