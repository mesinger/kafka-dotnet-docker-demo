using CommandLine;

namespace transaction_analytics_service
{
    public class CommandLineOptions
    {
        [Option('b', "bootstrap-servers", Required = true)]
        public string BootStrapServers { get; set; }
    }
}