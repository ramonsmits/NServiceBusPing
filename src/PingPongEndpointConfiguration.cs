﻿using Newtonsoft.Json;
using NServiceBus;
using NServiceBus.Logging;

class PingPongEndpointConfiguration : EndpointConfiguration
{
    public static JsonSerializerSettings IgnoreNullJsonSerializerSettings => new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

    public static readonly Dictionary<string, List<double>> Samples = new Dictionary<string, List<double>>();

    public PingPongEndpointConfiguration(string endpointName, string connectionstring, int concurrencyLimit) : base(endpointName)
    {
        var serialization = this.UseSerialization<NewtonsoftSerializer>();
        serialization.Settings(IgnoreNullJsonSerializerSettings);

        this.LimitMessageProcessingConcurrencyTo(concurrencyLimit);
        
        this.EnableInstallers();
        this.UsePersistence<InMemoryPersistence>();
        this.SendFailedMessagesTo("error");

        var transport = this.UseTransport<RabbitMQTransport>();
        transport.UseConventionalRoutingTopology();
        transport.ConnectionString(connectionstring);
        transport.PrefetchCount(0);

        var durationsLog = LogManager.GetLogger("Durations");
        var signalsLog = LogManager.GetLogger("Signals");

        var metrics = this.EnableMetrics();
        metrics.RegisterObservers(
            register: context =>
            {
                //Critical Time = 00:00:00.0563710
                //Processing Time = 00:00:00.0295209
                foreach (var duration in context.Durations)
                {
                    var samples = Samples[duration.Name] = new List<double>();
                    duration.Register(
                        observer: (ref DurationEvent @event) =>
                        {
                            samples.Add((long)@event.Duration.TotalMilliseconds);
                        });
                }
                //# of msgs pulled from the input queue /sec = Pong, NsbPing, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
                //# of msgs successfully processed / sec = Pong, NsbPing, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
                //foreach (var signal in context.Signals)
                //{
                //    signal.Register(
                //        observer: (ref SignalEvent @event) =>
                //        {
                //            signalsLog.InfoFormat("{0} = {1}", signal.Name, @event.MessageType);
                //        });
                //}
            });

        //this.LimitMessageProcessingConcurrencyTo(50);

        var recoverability = this.Recoverability();
        // https://docs.particular.net/nservicebus/recoverability/#immediate-retries  //Transport transactions must be enabled to support retries.
        recoverability
            .Immediate(cfg => cfg.NumberOfRetries(0))
            .Delayed(cfg => cfg.NumberOfRetries(0));
    }
}