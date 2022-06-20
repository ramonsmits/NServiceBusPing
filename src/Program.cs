using NServiceBus;
using System.Configuration;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.CursorVisible = false;
        var endpointName = args[0];
        var destination = args[1];
        var interval = int.Parse(args[2]);
        var count = int.Parse(args[3]);

        var endpointInstance = await CreateEndpontInstance(endpointName);

        var test = new PingTest() { EndpointInstance = endpointInstance };

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, ea) => { ea.Cancel = true; cts.Cancel(); };

        test.Launch(destination, interval, count, cts.Token);

        Console.WriteLine("Ctrl+C to quit...");
        await Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);

        await endpointInstance.Stop();
        return;
    }

    static Task<IEndpointInstance> CreateEndpontInstance(string endpointName)
    {
        string brokerAddress = ConfigurationManager.AppSettings["BrokerAddress"];
        if (string.IsNullOrEmpty(brokerAddress)) throw new InvalidOperationException("Please specify the broker connection string in `*.config`");
        return Endpoint.Start(new PingPongEndpointConfiguration(endpointName, brokerAddress));
    }
}

