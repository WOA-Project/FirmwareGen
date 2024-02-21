using DiscUtils.Vhdx;
using FirmwareGen.CommandLine;
using FirmwareGen.DeviceProfiles;
using System;
using System.IO;

namespace FirmwareGen
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Logging.Log("FirmwareGen");
            Logging.Log("Copyright (c) 2019-2024, Gustave Monce - gus33000.me - @gus33000");
            Logging.Log("Released under the MIT license at github.com/WOA-Project/FirmwareGen");
            Logging.Log("");


            //string TmpVHD = new EpsilonMaximizedForWindows().GetBlankVHD();
            //Logging.Log($"VHD: {TmpVHD}");


            /*const int chunkSize = 4096;

            DiscUtils.Setup.SetupHelper.RegisterAssembly(typeof(Disk).Assembly);
            using DiscUtils.VirtualDisk outDisk = DiscUtils.VirtualDisk.OpenDisk(@"C:\a\adaptationkits\CDG\Output\EpsilonHalfSplit128GB.vhdx", FileAccess.ReadWrite);

            DiscUtils.Streams.SparseStream ds = outDisk.Content;

            _ = ds.Seek(38248726528, SeekOrigin.Begin);
            DateTime startTime = DateTime.Now;
            using (BinaryReader br = new(File.OpenRead(@"C:\a\adaptationkits\CDG\Output\OSPool.img")))
            {
                int sectors = (int)(br.BaseStream.Length / chunkSize);
                for (int i = 0; i < sectors; i++)
                {
                    byte[] buff = br.ReadBytes(chunkSize);
                    ds.Write(buff, 0, chunkSize);
                    Logging.ShowProgress((ulong)br.BaseStream.Length, startTime, (ulong)((i + 1) * chunkSize), (ulong)((i + 1) * chunkSize), false);
                }
            }
            Logging.Log("");*/

            Logging.Log($@"-i EpsilonHalfSplit128GB.vhdx -f ""C:\a\adaptationkits\CDG\Output\{new EpsilonHalfSplit128GB().FFUFileName("20279.1002", "mu-mu", "WCOSCDG")}"" -c 16384 -s 4096 -p ""{new EpsilonHalfSplit128GB().PlatformID()}"" -o 10.0.20279.1002 -b 4000");

            try
            {
                CLIOptions.ParseOptionsAndTakeAction(args);
            }
            catch (Exception ex)
            {
                Logging.Log("Something happened.", Logging.LoggingLevel.Error);
                Logging.Log(ex.Message, Logging.LoggingLevel.Error);
                Logging.Log(ex.StackTrace, Logging.LoggingLevel.Error);
                Environment.Exit(1);
            }
        }
    }
}