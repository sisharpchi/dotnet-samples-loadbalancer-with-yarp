namespace Yarp.LoadBalancer.LoadBalancing;

public sealed record DestinationHealthSnapshot(
    string Id,
    string Address,
    bool IsLive,
    bool IsReady,
    int ConsecutiveLiveFailures,
    int ConsecutiveReadySuccesses,
    int ConsecutiveReadyFailures,
    DateTimeOffset? LastCheckedAt,
    string? LastError);

internal sealed class DestinationHealthState
{
    public DestinationHealthState(string id, string address)
    {
        Id = id;
        Address = address;
    }

    public string Id { get; }

    public string Address { get; private set; }

    public bool IsLive { get; private set; } = true;

    public bool IsReady { get; private set; }

    public int ConsecutiveLiveFailures { get; private set; }

    public int ConsecutiveReadySuccesses { get; private set; }

    public int ConsecutiveReadyFailures { get; private set; }

    public DateTimeOffset? LastCheckedAt { get; private set; }

    public string? LastError { get; private set; }

    public void UpdateAddress(string address)
    {
        Address = address;
    }

    public bool ApplyProbeResult(bool live, bool ready, int healthyThreshold, int unhealthyThreshold, string? error)
    {
        var previousLive = IsLive;
        var previousReady = IsReady;

        LastCheckedAt = DateTimeOffset.UtcNow;
        LastError = error;

        ConsecutiveLiveFailures = live ? 0 : ConsecutiveLiveFailures + 1;
        if (ConsecutiveLiveFailures >= Math.Max(1, unhealthyThreshold))
        {
            IsLive = false;
        }
        else if (live)
        {
            IsLive = true;
        }

        if (ready)
        {
            ConsecutiveReadySuccesses++;
            ConsecutiveReadyFailures = 0;
        }
        else
        {
            ConsecutiveReadySuccesses = 0;
            ConsecutiveReadyFailures++;
        }

        if (ConsecutiveReadySuccesses >= Math.Max(1, healthyThreshold))
        {
            IsReady = true;
        }
        else if (ConsecutiveReadyFailures >= Math.Max(1, unhealthyThreshold))
        {
            IsReady = false;
        }

        return previousLive != IsLive || previousReady != IsReady;
    }

    public DestinationHealthSnapshot ToSnapshot()
    {
        return new DestinationHealthSnapshot(
            Id,
            Address,
            IsLive,
            IsReady,
            ConsecutiveLiveFailures,
            ConsecutiveReadySuccesses,
            ConsecutiveReadyFailures,
            LastCheckedAt,
            LastError);
    }
}
