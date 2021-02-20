using System;
using System.Threading;
using CommandLine;
using Confluent.Kafka;
using money_laundering_service;
using Newtonsoft.Json;

Parser.Default.ParseArguments<CommandLineOptions>(args)
    .WithParsed(o =>
    {
        var conf = new ConsumerConfig
        {
            GroupId = "money-laundering-service-consumer-group",
            BootstrapServers = o.BootStrapServers,
            AutoOffsetReset = AutoOffsetReset.Latest
        };

        using var c = new ConsumerBuilder<string, string>(conf).Build();
        c.Subscribe("payments");

        CancellationTokenSource cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // prevent the process from terminating.
            cts.Cancel();
        };

        try
        {
            while (true)
            {
                try
                {
                    var cr = c.Consume(cts.Token);

                    var transaction = JsonConvert.DeserializeObject<Transaction>(cr.Message.Value);

                    var launderyCheck = new LaunderyCheck(transaction.Id,
                        transaction.Value > 1000 ? "declined" : "ok");
            
                    var config = new ProducerConfig {BootstrapServers = o.BootStrapServers};

                    using var p = new ProducerBuilder<string, string>(config).Build();
        
                    try
                    {
                        var message = new Message<string, string>
                        {
                            Key = launderyCheck.TransactionId,
                            Value = JsonConvert.SerializeObject(launderyCheck)
                        };
            
                        p.Produce("laundery-check", message);

                        Console.WriteLine($"Delivered Laundery Check for transaction '{transaction.Id}'");

                        p.Flush(TimeSpan.FromSeconds(10));
                    }
                    catch (ProduceException<string, string> e)
                    {
                        Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Error occured: {e.Error.Reason}");
                }
                catch (JsonReaderException)
                {
                    Console.WriteLine($"Unable to parse transaction");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ensure the consumer leaves the group cleanly and final offsets are committed.
            c.Close();
        }
    })
    .WithNotParsed(_ => Console.WriteLine("Invalid arguments"));

namespace money_laundering_service
{
    public record Transaction(string Id, string Sender, string Recipient, int Value);

    public record LaunderyCheck(string TransactionId, string Status);
}
