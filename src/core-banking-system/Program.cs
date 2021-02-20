using System;
using CommandLine;
using Confluent.Kafka;
using core_banking_system;
using Newtonsoft.Json;

Parser.Default.ParseArguments<CommandLineOptions>(args)
    .WithParsed(o =>
    {
        var config = new ProducerConfig {BootstrapServers = o.BootStrapServers};

        using var p = new ProducerBuilder<string, string>(config).Build();
        
        try
        {
            var transaction = new Transaction(Guid.NewGuid().ToString(), o.PaymentSender, o.PaymentRecipient, o.Amount);
            
            var message = new Message<string, string>
            {
                Key = transaction.Id,
                Value = JsonConvert.SerializeObject(transaction)
            };
            
            p.Produce("payments", message);

            Console.WriteLine($"Delivered transaction {transaction.Id}");

            p.Flush(TimeSpan.FromSeconds(10));
        }
        catch (ProduceException<string, string> e)
        {
            Console.WriteLine($"Delivery failed: {e.Error.Reason}");
        }
    })
    .WithNotParsed(_ => Console.WriteLine("Invalid arguments"));

namespace core_banking_system
{
    public record Transaction(string Id, string Sender, string Recipient, int Value);
}
