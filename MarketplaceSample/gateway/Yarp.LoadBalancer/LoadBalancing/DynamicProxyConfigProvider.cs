using Yarp.ReverseProxy.Configuration;

namespace Yarp.LoadBalancer.LoadBalancing;

public sealed class DynamicProxyConfigProvider : IProxyConfigProvider
{
    private readonly InMemoryConfigProvider _inner;
    private readonly DestinationHealthStore _destinationHealthStore;
    private readonly object _reloadLock = new();
    private int _revision;

    public DynamicProxyConfigProvider(DestinationHealthStore destinationHealthStore)
    {
        _destinationHealthStore = destinationHealthStore;
        _inner = new InMemoryConfigProvider([], []);
    }

    public IProxyConfig GetConfig()
    {
        return _inner.GetConfig();
    }

    public void Reload(DynamicLoadBalancerOptions options)
    {
        var routes = BuildRoutes(options);
        var clusters = BuildClusters(options);

        lock (_reloadLock)
        {
            _revision++;
            _inner.Update(routes, clusters, _revision.ToString());
        }
    }

    private static IReadOnlyList<RouteConfig> BuildRoutes(DynamicLoadBalancerOptions options)
    {
        return
        [
            new RouteConfig
            {
                RouteId = options.RouteId,
                ClusterId = options.ClusterId,
                Match = new RouteMatch
                {
                    Path = options.RoutePath
                },
                Transforms =
                [
                    new Dictionary<string, string>
                    {
                        ["PathPattern"] = options.PathPattern
                    }
                ]
            }
        ];
    }

    private IReadOnlyList<ClusterConfig> BuildClusters(DynamicLoadBalancerOptions options)
    {
        var destinations = _destinationHealthStore.GetRoutableDestinations()
            .ToDictionary(
                destination => destination.Id,
                destination => new DestinationConfig
                {
                    Address = destination.Address
                },
                StringComparer.OrdinalIgnoreCase);

        return
        [
            new ClusterConfig
            {
                ClusterId = options.ClusterId,
                LoadBalancingPolicy = options.LoadBalancingPolicy,
                Destinations = destinations
            }
        ];
    }
}
