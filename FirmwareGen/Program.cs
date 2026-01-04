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
            using DiscUtils.VirtualDisk outDisk = DiscUtils.VirtualDisk.OpenDisk(@"C:\Users\Gus\Downloads\OEMEP_WCOSCDG_MaximizedForWindows\Store0_VenHw(860845C1-BE09-4355-8BC1-30D64FF8E63A).vhdx", FileAccess.ReadWrite);

            DiscUtils.Streams.SparseStream ds = outDisk.Content;

            _ = ds.Seek(4347396096, SeekOrigin.Begin);
            DateTime startTime = DateTime.Now;
            using (BinaryReader br = new(File.OpenRead(@"C:\Users\Gus\Downloads\Telegram Desktop\LUNs_OEMSK.EV3.rs_prerelease_cfga.18236.1000.180905-1440\_NEW\BS_EFIESP.img")))
            {
                int sectors = 272629760 / chunkSize;//(int)(br.BaseStream.Length / chunkSize);
                for (int i = 0; i < sectors; i++)
                {
                    byte[] buff = br.ReadBytes(chunkSize);
                    ds.Write(buff, 0, chunkSize);
                    Logging.ShowProgress((ulong)br.BaseStream.Length, startTime, (ulong)((i + 1) * chunkSize), (ulong)((i + 1) * chunkSize), false);
                }
            }
            Logging.Log("");

            _ = ds.Seek(4620025856, SeekOrigin.Begin);
            startTime = DateTime.Now;
            using (BinaryReader br = new(File.OpenRead(@"C:\Users\Gus\Downloads\Telegram Desktop\LUNs_OEMSK.EV3.rs_prerelease_cfga.18236.1000.180905-1440\_NEW\750d8f0bec814fb2a5e54019f9cc64cc.img")))
            {
                int sectors = (int)(49066016768 / chunkSize);//(int)(br.BaseStream.Length / chunkSize);
                for (int i = 0; i < sectors; i++)
                {
                    byte[] buff = br.ReadBytes(chunkSize);
                    ds.Write(buff, 0, chunkSize);
                    //var bytesRead = (ulong)((i + 1) * chunkSize);
                    //var sourcePos = (ulong)((i + 1) * chunkSize);
                    //Logging.ShowProgress((ulong)br.BaseStream.Length, startTime, bytesRead, sourcePos, false);
                    Logging.ShowProgress((ulong)sectors, startTime, (ulong)i, (ulong)i, false);
                }
            }
            Logging.Log("");*/

            const int chunkSize = 4096;

            DiscUtils.Setup.SetupHelper.RegisterAssembly(typeof(Disk).Assembly);
            using DiscUtils.VirtualDisk outDisk = DiscUtils.VirtualDisk.OpenDisk(@"C:\Users\Gus\Documents\Andromeda\18259.1002.rs_onecore_stack_mc5.181010-1731\PreInstalledDisk.vhdx", FileAccess.ReadWrite);

            DiscUtils.Streams.SparseStream ds = outDisk.Content;

            _ = ds.Seek(0x100000, SeekOrigin.Begin);
            DateTime startTime = DateTime.Now;
            using (BinaryReader br = new(File.OpenRead(@"C:\Users\Gus\Documents\Andromeda\18259.1002.rs_onecore_stack_mc5.181010-1731\PreInstalled.img")))
            {
                int sectors = (int)(53685997568 / chunkSize);//(int)(br.BaseStream.Length / chunkSize);
                for (int i = 0; i < sectors; i++)
                {
                    byte[] buff = br.ReadBytes(chunkSize);
                    ds.Write(buff, 0, chunkSize);
                    Logging.ShowProgress((ulong)br.BaseStream.Length, startTime, ((ulong)i + 1ul) * chunkSize, ((ulong)i + 1) * chunkSize, false);
                }
            }
            Logging.Log("");

            /*Logging.Log($@"-i ""C:\Users\Gus\Downloads\OEMEP_WCOSCDG_MaximizedForWindows\Store0_VenHw(860845C1-BE09-4355-8BC1-30D64FF8E63A).vhdx"" -f ""C:\a\adaptationkits\CDG\Output\{new EpsilonHalfSplit128GB().FFUFileName("18236.1000", "mu-mu", "ANDOS")}"" -c 16384 -s 4096 -p ""{new EpsilonHalfSplit128GB().PlatformID()}"" -o 10.0.18236.1000 -b 4000");

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
            }*/
        }
    }
}