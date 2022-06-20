using MathNet.Numerics.Statistics;
using NServiceBus;
using NServiceBus.Logging;

class PingTest
{
    readonly ILog Log = LogManager.GetLogger<PingTest>();
    public IEndpointInstance EndpointInstance { private get; set; }
    public List<double> Samples { get; set; }
    public TimeSpan Duration { get; set; }
    public int Count = 0;

    public async void Launch(string destination, int intervalms, int numPings, CancellationToken cancellationToken)
    {
        var step = TimeSpan.FromMilliseconds(intervalms);

        Samples = new List<double>(numPings);
        PingHandler.Test = this;
        Count = 0;

        try
        {
            var next = DateTime.UtcNow;
            next = RoundUp(next, step); // Roundup so that the sends are pretty much aligned with the clock

            while (!cancellationToken.IsCancellationRequested)
            {
                var delay = next - DateTime.UtcNow;
                if (delay.Ticks > 0) await Task.Delay(delay, cancellationToken);
                next += step;

                // Required to form this otherwise the configured "intervalms" value might be missed, important with tiny delays
                _ = EndpointInstance.Send(destination, new Ping());

                if (numPings != 0 && Count > numPings) break;
            }

            LogResults();
        }
        catch (Exception e)
        {
            Log.Fatal("Start", e);
        }
    }

    DateTime RoundUp(DateTime dt, TimeSpan d)
    {
        return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
    }

    public void LogResults()
    {
        if (Samples.Count == 0) { Log.Error("No samples recorded."); return; }

        var hist = new Histogram(Samples, 10);

        Log.InfoFormat("PingCount: {0}", Count);
        Log.InfoFormat("HistCount: {0}", (int)hist.DataCount);
        Log.InfoFormat("Histogram: {0}", hist.ToString().Replace("(", "\r\n\t("));
    }
}
