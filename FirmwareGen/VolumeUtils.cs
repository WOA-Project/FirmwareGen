using System.Diagnostics;

namespace FirmwareGen
{
    public static class VolumeUtils
    {
        public static void RunProgram(string Program, string Arguments)
        {
            Process proc = new()
            {
                StartInfo = new ProcessStartInfo(Program, Arguments)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false
                }
            };
            _ = proc.Start();
            proc.WaitForExit();
        }
    }
}
