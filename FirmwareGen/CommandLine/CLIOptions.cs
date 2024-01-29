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

            _ = Parser.Default.ParseArguments<GenerateWindowsFFUOptions>(args).WithParsed(MainLogic.GenerateWindowsFFU);
        }
    }
}
