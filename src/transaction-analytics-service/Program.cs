using System;
using System.Threading;
using CommandLine;
using Confluent.Kafka;
using Newtonsoft.Json;
using transaction_analytics_service;

Parser.Default.ParseArguments<CommandLineOptions>(args)
    .WithParsed(o =>
    {
        var conf = new ConsumerConfig
        {
            GroupId = "transaction-analytics-service-consumer-group",
            BootstrapServers = o.BootStrapServers,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        using var c = new ConsumerBuilder<string, string>(conf).Build();
        c.Subscribe("laundery-check");
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // prevent the process from terminating.
            cts.Cancel();
        };
        
        Console.WriteLine("Consuming topic from " + o.BootStrapServers);
        
        try
        {
            while (true)
            {
                try
                {
                    var cr = c.Consume(cts.Token);

                    var message = cr.Message;

                    var launderyCheck = JsonConvert.DeserializeObject<LaunderyCheck>(message.Value);

                    Console.WriteLine(
                        $"Consumed laundery check for transaction '{launderyCheck.TransactionId}' with status '{launderyCheck.Status}'.");
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error occured: {e.Error.Reason}");
                }
                catch (JsonReaderException)
                {
                    Console.WriteLine("Unable to parse laundery check");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ensure the consumer leaves the group cleanly and final offsets are committed.
            c.Close();
            Console.WriteLine("Shutdown");
        }
    })
    .WithNotParsed(_ => Console.WriteLine("Invalid arguments"));
    
namespace transaction_analytics_service
{
    public record LaunderyCheck(string TransactionId, string Status);
}