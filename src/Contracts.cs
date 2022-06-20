using NServiceBus;

public class Ping : ICommand
{
}

public class Pong : IMessage
{
    public DateTime OriginalSentTime { get; set; }
    public double SendDurationMs { get; set; }
    public double ResponseDurationMs { get; set; }
}
