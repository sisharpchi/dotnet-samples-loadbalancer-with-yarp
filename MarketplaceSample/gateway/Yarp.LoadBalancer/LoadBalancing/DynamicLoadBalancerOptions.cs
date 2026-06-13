namespace Yarp.LoadBalancer.LoadBalancing;

public sealed class DynamicLoadBalancerOptions
{
    public const string SectionName = "LoadBalancer";

    public string RouteId { get; set; } = "marketplace-route";

    public string ClusterId { get; set; } = "marketplace-cluster";

    public string RoutePath { get; set; } = "/marketplace/{**catch-all}";

    public string PathPattern { get; set; } = "{**catch-all}";

    public string LoadBalancingPolicy { get; set; } = "RoundRobin";

    public string LivenessPath { get; set; } = "/health/live";

    public string ReadinessPath { get; set; } = "/health/ready";

    public int ProbeIntervalSeconds { get; set; } = 5;

    public int ProbeTimeoutSeconds { get; set; } = 2;

    public int HealthyThreshold { get; set; } = 1;

    public int UnhealthyThreshold { get; set; } = 2;

    public List<LoadBalancerDestinationOptions> Destinations { get; set; } = [];
}

public sealed class LoadBalancerDestinationOptions
{
    public string Id { get; set; } = "";

    public string Address { get; set; } = "";
}
