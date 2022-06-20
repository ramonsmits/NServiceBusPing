using NServiceBus;

class PingHandler :
    IHandleMessages<Ping>,
    IHandleMessages<Pong>
{
    public static PingTest Test { get; set; }

    public Task Handle(Ping msg, IMessageHandlerContext context)
    {
        var now = DateTime.UtcNow;
        var sentAt = DateTimeExtensions.ToUtcDateTime(context.MessageHeaders[Headers.TimeSent]);
        var latency = now.Subtract(sentAt).TotalMilliseconds;
        var response = new Pong { 
            OriginalSentTime = sentAt, 
            SendDurationMs = latency
        };
        return context.Reply(response);
    }

    public Task Handle(Pong msg, IMessageHandlerContext context)
    {
        var now = DateTime.UtcNow;
        if (Test is null) return Task.CompletedTask; //From a previous iteration perhaps
        
        Interlocked.Increment(ref Test.Count);
        
        if (Test.Count < 4) return Task.CompletedTask; //warmup

        var sentAt = DateTimeExtensions.ToUtcDateTime(context.MessageHeaders[Headers.TimeSent]);
        var pongLatency = now - sentAt;
        var pingLatency = now - msg.OriginalSentTime;

        Test.Samples.Add(pingLatency.TotalMilliseconds);

        _ = Console.Out.WriteLineAsync($"TotalMilliseconds = {pingLatency.TotalMilliseconds,8:N2}ms");

        return Task.CompletedTask;
    }
}
