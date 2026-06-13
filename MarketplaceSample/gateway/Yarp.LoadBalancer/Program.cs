using Yarp.LoadBalancer.Extensions;
using Yarp.LoadBalancer.LoadBalancing;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.LoadBalancing;

namespace Yarp.LoadBalancer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<ILoadBalancingPolicy, CustomLoadBalancingPolicy>();
        builder.Services.Configure<DynamicLoadBalancerOptions>(
            builder.Configuration.GetSection(DynamicLoadBalancerOptions.SectionName));
        builder.Services.AddSingleton<DestinationHealthStore>();
        builder.Services.AddSingleton<DynamicProxyConfigProvider>();
        builder.Services.AddSingleton<IProxyConfigProvider>(sp => sp.GetRequiredService<DynamicProxyConfigProvider>());
        builder.Services.AddHttpClient("load-balancer-health");
        builder.Services.AddHostedService<DestinationHealthMonitor>();
        builder.Services.AddReverseProxy();

        builder.Services.AddHealthChecks();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.MapReverseProxy();
        app.MapHealthChecks("/health");
        app.MapControllers();
       
        app.Run();
    }
}
