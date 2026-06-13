using System.Collections.Concurrent;

namespace Yarp.LoadBalancer.LoadBalancing;

public sealed class DestinationHealthStore
{
    private readonly ConcurrentDictionary<string, DestinationHealthState> _destinations = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<DestinationHealthSnapshot> SyncConfiguredDestinations(IEnumerable<LoadBalancerDestinationOptions> configuredDestinations)
    {
        var configured = configuredDestinations
            .Where(destination => !string.IsNullOrWhiteSpace(destination.Id) && !string.IsNullOrWhiteSpace(destination.Address))
            .ToDictionary(destination => destination.Id, StringComparer.OrdinalIgnoreCase);

        foreach (var destination in configured.Values)
        {
            _destinations.AddOrUpdate(
                destination.Id,
                _ => new DestinationHealthState(destination.Id, NormalizeAddress(destination.Address)),
                (_, existing) =>
                {
                    existing.UpdateAddress(NormalizeAddress(destination.Address));
                    return existing;
                });
        }

        foreach (var destinationId in _destinations.Keys)
        {
            if (!configured.ContainsKey(destinationId))
            {
                _destinations.TryRemove(destinationId, out _);
            }
        }

        return GetAll();
    }

    public IReadOnlyList<DestinationHealthSnapshot> GetAll()
    {
        return _destinations.Values
            .OrderBy(destination => destination.Id, StringComparer.OrdinalIgnoreCase)
            .Select(destination => destination.ToSnapshot())
            .ToArray();
    }

    public IReadOnlyList<DestinationHealthSnapshot> GetRoutableDestinations()
    {
        return _destinations.Values
            .Where(destination => destination.IsLive && destination.IsReady)
            .OrderBy(destination => destination.Id, StringComparer.OrdinalIgnoreCase)
            .Select(destination => destination.ToSnapshot())
            .ToArray();
    }

    public bool ApplyProbeResult(string id, bool live, bool ready, int healthyThreshold, int unhealthyThreshold, string? error)
    {
        return _destinations.TryGetValue(id, out var destination)
            && destination.ApplyProbeResult(live, ready, healthyThreshold, unhealthyThreshold, error);
    }

    private static string NormalizeAddress(string address)
    {
        return address.EndsWith('/') ? address : address + "/";
    }
}
