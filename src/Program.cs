﻿using NServiceBus;
using NServiceBus.Logging;
using System.Configuration;
using System.Diagnostics;
using System.Runtime;

public class Program
{
    static readonly ILog Log = LogManager.GetLogger<Program>();
    static bool IsDebug = false;

    [Conditional("DEBUG")]
    static void SetDebugBuild()
    {
        IsDebug = true;
    }

    public static async Task Main(string[] args)
    {
        SetDebugBuild();

        Log.InfoFormat("                   IsServerGC = {0}", GCSettings.IsServerGC);
        Log.InfoFormat("                  LatencyMode = {0}", GCSettings.LatencyMode);
        Log.InfoFormat("LargeObjectHeapCompactionMode = {0}", GCSettings.LargeObjectHeapCompactionMode);
        Log.InfoFormat("                    OSVersion = {0}", Environment.OSVersion);
        Log.InfoFormat("                      Version = {0}", Environment.Version);
        Log.InfoFormat("               Is64BitProcess = {0}", Environment.Is64BitProcess);
        Log.InfoFormat("                      IsDebug = {0}", IsDebug);

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

