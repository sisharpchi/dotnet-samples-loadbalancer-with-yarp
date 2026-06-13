using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Yarp.LoadBalancer.LoadBalancing;

namespace Yarp.LoadBalancer.Controllers;

[ApiController]
[Route("load-balancer")]
public class LoadBalancerController(
    DestinationHealthStore destinationHealthStore,
    DynamicProxyConfigProvider proxyConfigProvider,
    IOptionsMonitor<DynamicLoadBalancerOptions> optionsMonitor) : ControllerBase
{
    [HttpGet("state")]
    public IActionResult GetState()
    {
        var options = optionsMonitor.CurrentValue;
        return Ok(new
        {
            options.RouteId,
            options.ClusterId,
            options.RoutePath,
            options.PathPattern,
            options.LoadBalancingPolicy,
            options.LivenessPath,
            options.ReadinessPath,
            options.ProbeIntervalSeconds,
            options.ProbeTimeoutSeconds,
            options.HealthyThreshold,
            options.UnhealthyThreshold,
            Destinations = destinationHealthStore.GetAll(),
            ActiveDestinations = destinationHealthStore.GetRoutableDestinations()
        });
    }

    [HttpPost("reload")]
    public IActionResult Reload()
    {
        proxyConfigProvider.Reload(optionsMonitor.CurrentValue);
        return Accepted(new
        {
            Message = "Proxy configuration reload was triggered.",
            ActiveDestinations = destinationHealthStore.GetRoutableDestinations()
        });
    }
}
