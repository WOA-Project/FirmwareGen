using CommandLine;

namespace FirmwareGen
{
    public class CLIOptions
    {
        [Verb("generate-windows-base-vhdx", HelpText = "Generates a Windows Base VHDX")]
        public class GenerateWindowsOptions
        {
            [Option('o', "output", HelpText = "Todo", Required = true)]
            public string Output { get; set; }

            [Option('w', "windows-dvd", HelpText = "Todo", Required = true)]
            public string WindowsDVD { get; set; }

            [Option('i', "windows-index", HelpText = "Todo", Required = true)]
            public string WindowsIndex { get; set; }
        }

        [Verb("generate-windows-ffu", HelpText = "Generates a ffu from a Windows Base VHDX")]
        public class GenerateWindowsFFUOptions
        {
            [Option('d', "driver-pack", HelpText = @"Todo", Required = true)]
            public string DriverPack { get; set; }

            [Option('o', "output", HelpText = "Todo", Required = true)]
            public string Output { get; set; }

            [Option('i', "input", HelpText = "Todo", Required = true)]
            public string Input { get; set; }

            [Option('v', "windows-ver", HelpText = "Todo", Required = true)]
            public string WindowsVer { get; set; }
        }

        [Verb("generate-other-ffu", HelpText = "Generates a ffu from another Base VHDX")]
        public class GenerateOtherFFUOptions
        {
            [Option('o', "output", HelpText = "Todo", Required = true)]
            public string Output { get; set; }

            [Option('i', "input", HelpText = "Todo", Required = true)]
            public string Input { get; set; }

            [Option('v', "ver", HelpText = "Todo", Required = true)]
            public string Ver { get; set; }
        }

        public static void ParseOptionsAndTakeAction(string[] args)
        {
            if (!MainLogic.VerifyAllComponentsArePresent())
                return;

            Parser.Default.ParseArguments<GenerateWindowsOptions, GenerateWindowsFFUOptions, GenerateOtherFFUOptions>(args)
                .WithParsed<GenerateWindowsOptions>(opts => MainLogic.GenerateWindowsBaseVHDX(opts))
                .WithParsed<GenerateWindowsFFUOptions>(opts => MainLogic.GenerateWindowsFFU(opts))
                .WithParsed<GenerateOtherFFUOptions>(opts => MainLogic.GenerateOtherFFU(opts));
        }
    }
}
