using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;

namespace Yarp.LoadBalancer.Extensions;

public class CustomLoadBalancingPolicy : ILoadBalancingPolicy
{
    public string Name => "CustomLogic";

    public DestinationState? PickDestination(HttpContext context, ClusterState cluster, IReadOnlyList<DestinationState> availableDestinations)
    {
        if (availableDestinations.Count == 0)
        {
            return null;
        }

        context.Request.Headers.TryGetValue("destination", out var destination);
        if (availableDestinations.Any(d => d.DestinationId == destination))
        {
            return availableDestinations.First(d => d.DestinationId == destination);
        }

        return availableDestinations[0];
    }
}
