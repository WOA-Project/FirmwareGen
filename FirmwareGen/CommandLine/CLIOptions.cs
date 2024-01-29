using CommandLine;

namespace FirmwareGen.CommandLine
{
    public static class CLIOptions
    {
        public static void ParseOptionsAndTakeAction(string[] args)
        {
            if (!MainLogic.VerifyAllComponentsArePresent())
            {
                return;
            }

            _ = Parser.Default.ParseArguments<GenerateWindowsOptions, GenerateWindowsFFUOptions, GenerateOtherFFUOptions>(args)
                .WithParsed<GenerateWindowsOptions>(MainLogic.GenerateWindowsBaseVHDX)
                .WithParsed<GenerateWindowsFFUOptions>(MainLogic.GenerateWindowsFFU)
                .WithParsed<GenerateOtherFFUOptions>(MainLogic.GenerateOtherFFU);
        }
    }
}
