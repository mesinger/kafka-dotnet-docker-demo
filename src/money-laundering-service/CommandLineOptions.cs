using CommandLine;

namespace money_laundering_service
{
    public class CommandLineOptions
    {
        [Option('b', "bootstrap-servers", Required = true)]
        public string BootStrapServers { get; set; }
    }
}