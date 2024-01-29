using CommandLine;

namespace FirmwareGen.CommandLine
{
    [Verb("generate-other-ffu", HelpText = "Generates a ffu from another Base VHDX")]
    public class GenerateOtherFFUOptions
    {
        [Option('o', "output", HelpText = "Todo", Required = true)]
        public string Output
        {
            get; set;
        }

        [Option('i', "input", HelpText = "Todo", Required = true)]
        public string Input
        {
            get; set;
        }

        [Option('v', "ver", HelpText = "Todo", Required = true)]
        public string Ver
        {
            get; set;
        }
    }
}
