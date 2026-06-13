using Microsoft.Extensions.Options;

namespace Yarp.LoadBalancer.LoadBalancing;

public sealed class DestinationHealthMonitor(
    DestinationHealthStore destinationHealthStore,
    DynamicProxyConfigProvider proxyConfigProvider,
    IHttpClientFactory httpClientFactory,
    IOptionsMonitor<DynamicLoadBalancerOptions> optionsMonitor,
    ILogger<DestinationHealthMonitor> logger) : BackgroundService
{
    private int _lastConfigHash;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = optionsMonitor.CurrentValue;

            try
            {
                var changed = SyncConfiguredDestinations(options);
                changed |= await ProbeDestinationsAsync(options, stoppingToken);

                if (changed)
                {
                    proxyConfigProvider.Reload(options);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while probing load balancer destinations.");
            }

            await Task.Delay(GetProbeInterval(options), stoppingToken);
        }
    }

    private bool SyncConfiguredDestinations(DynamicLoadBalancerOptions options)
    {
        destinationHealthStore.SyncConfiguredDestinations(options.Destinations);

        var configHash = GetConfigHash(options);
        if (configHash == _lastConfigHash)
        {
            return false;
        }

        _lastConfigHash = configHash;
        return true;
    }

    private async Task<bool> ProbeDestinationsAsync(DynamicLoadBalancerOptions options, CancellationToken stoppingToken)
    {
        var changed = false;
        var destinations = destinationHealthStore.GetAll();

        foreach (var destination in destinations)
        {
            var live = await ProbeAsync(destination.Address, options.LivenessPath, options, stoppingToken);
            var ready = live.Success
                ? await ProbeAsync(destination.Address, options.ReadinessPath, options, stoppingToken)
                : ProbeResult.Failed("Readiness was skipped because liveness failed.");

            var error = live.Success
                ? ready.Error
                : live.Error;

            changed |= destinationHealthStore.ApplyProbeResult(
                destination.Id,
                live.Success,
                ready.Success,
                options.HealthyThreshold,
                options.UnhealthyThreshold,
                error);
        }

        return changed;
    }

    private async Task<ProbeResult> ProbeAsync(
        string destinationAddress,
        string path,
        DynamicLoadBalancerOptions options,
        CancellationToken stoppingToken)
    {
        var client = httpClientFactory.CreateClient("load-balancer-health");
        client.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.ProbeTimeoutSeconds));

        try
        {
            var response = await client.GetAsync(new Uri(new Uri(destinationAddress), path.TrimStart('/')), stoppingToken);
            if (response.IsSuccessStatusCode)
            {
                return ProbeResult.Successful();
            }

            return ProbeResult.Failed($"{path} returned {(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return ProbeResult.Failed($"{path} failed: {ex.Message}");
        }
    }

    private static TimeSpan GetProbeInterval(DynamicLoadBalancerOptions options)
    {
        return TimeSpan.FromSeconds(Math.Max(1, options.ProbeIntervalSeconds));
    }

    private static int GetConfigHash(DynamicLoadBalancerOptions options)
    {
        var hash = new HashCode();
        hash.Add(options.RouteId, StringComparer.OrdinalIgnoreCase);
        hash.Add(options.ClusterId, StringComparer.OrdinalIgnoreCase);
        hash.Add(options.RoutePath, StringComparer.OrdinalIgnoreCase);
        hash.Add(options.PathPattern, StringComparer.OrdinalIgnoreCase);
        hash.Add(options.LoadBalancingPolicy, StringComparer.OrdinalIgnoreCase);
        hash.Add(options.LivenessPath, StringComparer.OrdinalIgnoreCase);
        hash.Add(options.ReadinessPath, StringComparer.OrdinalIgnoreCase);

        foreach (var destination in options.Destinations.OrderBy(destination => destination.Id, StringComparer.OrdinalIgnoreCase))
        {
            hash.Add(destination.Id, StringComparer.OrdinalIgnoreCase);
            hash.Add(destination.Address, StringComparer.OrdinalIgnoreCase);
        }

        return hash.ToHashCode();
    }

    private readonly record struct ProbeResult(bool Success, string? Error)
    {
        public static ProbeResult Successful() => new(true, null);

        public static ProbeResult Failed(string error) => new(false, error);

        public static implicit operator bool(ProbeResult result) => result.Success;
    }
}
