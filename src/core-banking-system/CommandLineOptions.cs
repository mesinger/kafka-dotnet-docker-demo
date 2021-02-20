using CommandLine;

namespace core_banking_system
{
    public class CommandLineOptions
    {
        [Option('b', "bootstrap-servers", Required = true)]
        public string BootStrapServers { get; set; }

        [Option('s', "sender", Required = true, HelpText = "The payment sender id")]
        public string PaymentSender { get; set; }

        [Option('r', "recipient", Required = true, HelpText = "The payment recipient id")]
        public string PaymentRecipient { get; set; }

        [Option('a', "amount", Required = true, HelpText = "The transaction amount in EUR")]
        public int Amount { get; set; }
    }
}