using NServiceBus;
using NServiceBus.Logging;

class PingHandler :
    IHandleMessages<Ping>,
    IHandleMessages<Pong>,
    IHandleMessages<Noop>
{
    static readonly ILog Log = LogManager.GetLogger<PingHandler>();
    public static PingTest Test { get; set; }

    public Task Handle(Ping msg, IMessageHandlerContext context)
    {
        var now = DateTime.UtcNow;
        var sentAt = DateTimeExtensions.ToUtcDateTime(context.MessageHeaders[Headers.TimeSent]);
        var latency = now.Subtract(sentAt).TotalMilliseconds;
        var response = new Pong
        {
            OriginalSentTime = sentAt,
            SendDurationMs = latency
        };
        return context.Reply(response);
    }

    public Task Handle(Pong msg, IMessageHandlerContext context)
    {
        var now = DateTime.UtcNow;
        if (Test is null) return Task.CompletedTask; //From a previous iteration perhaps

        var count = Interlocked.Increment(ref Test.Count);

        if (count < 10) return Task.CompletedTask; //warmup

        var sentAt = DateTimeExtensions.ToUtcDateTime(context.MessageHeaders[Headers.TimeSent]);
        var pongLatency = now - sentAt;
        var pingLatency = now - msg.OriginalSentTime;

        Test.Samples.Add(pingLatency.TotalMilliseconds);

        return Task.CompletedTask;
    }

    public Task Handle(Noop message, IMessageHandlerContext context)
    {
        var now = DateTime.UtcNow;
        var sentAt = DateTimeExtensions.ToUtcDateTime(context.MessageHeaders[Headers.TimeSent]);
        var latency = now - sentAt;

        var count = Interlocked.Increment(ref Test.Count);

        int latencyMs = (int)latency.TotalMilliseconds;

        var text = $"#{count,6} {sentAt:O} {latencyMs,5}ms";

        if (latencyMs > 1000)
            Log.Error(text);
        else if (latencyMs > 100)
            Log.Warn(text);
        else //if (latencyMs > 10)
            Log.Info(text);
        //else
        //    Log.Debug(text);

        return Task.CompletedTask;
    }
}