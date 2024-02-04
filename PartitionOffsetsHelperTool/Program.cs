using PartitionOffsetsHelperTool.GPT;

namespace PartitionOffsetsHelperTool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Partition Offsets Helper Tool");
            Console.WriteLine("Copyright (c) 2019-2024, Gustave Monce - gus33000.me - @gus33000");
            Console.WriteLine("Released under the MIT license at github.com/WOA-Project/FirmwareGen");
            Console.WriteLine("");

            Console.WriteLine("This tool is designed to help you get the right commands to");
            Console.WriteLine("achieve the storage allocation you desire between Windows and Android.");
            Console.WriteLine();
            Console.WriteLine("Using this tool is pretty simple, but requires consulting the guide");
            Console.WriteLine("before first usage. Please ensure you understood the guide, or you may");
            Console.WriteLine("easily get invalid commands.");
            Console.WriteLine();
            Console.WriteLine("This tool is currently only suited for Surface Duo (1st Gen).");
            Console.WriteLine("It will not work for Surface Duo 2 or any other device.");
            Console.WriteLine();

            ConsoleColor ogColor = Console.ForegroundColor;

            bool Is128GBModel;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("Is your Surface Duo a 128GB model? [Y/N]: ");
                Console.ForegroundColor = ogColor;
                ConsoleKeyInfo key = Console.ReadKey();
                Console.WriteLine();
                Console.WriteLine();
                if (key.Key == ConsoleKey.Y)
                {
                    Console.WriteLine("You have a 128GB model.");
                    Console.WriteLine();
                    Is128GBModel = true;
                    break;
                }
                else if (key.Key == ConsoleKey.N)
                {
                    Console.WriteLine("You have a 256GB model.");
                    Console.WriteLine();
                    Is128GBModel = false;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input, please try again.");
                    Console.WriteLine();
                    continue;
                }
            }

            ulong requestedAndroidTotalCapacity;

            Console.WriteLine("How much space do you want to allocate for Android?");
            Console.WriteLine("The remaining space will be given to Windows.");
            Console.WriteLine();
            Console.WriteLine("Please note you cannot go beyond your total storage capacity,");
            Console.WriteLine("and Windows requires a minimum storage requirement of 64GB for itself,");
            Console.WriteLine("while Android Compatibility specifications require at least 4GB of storage available.");
            Console.WriteLine();
            Console.WriteLine("How much space do you want to allocate for Android?");
            Console.WriteLine("Please provide the amount in GB, ie, 4 results in 4GB, 20, results in 20GB.");
            Console.WriteLine();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("Your choice in GB: ");
                Console.ForegroundColor = ogColor;
                string? input = Console.ReadLine();
                Console.WriteLine();

                if (input == null)
                {
                    Console.WriteLine("Invalid input, please try again.");
                    Console.WriteLine();
                    continue;
                }

                if (!ulong.TryParse(input, out requestedAndroidTotalCapacity))
                {
                    Console.WriteLine("Invalid input, please try again.");
                    Console.WriteLine();
                    continue;
                }

                if (requestedAndroidTotalCapacity < 4)
                {
                    Console.WriteLine("Invalid input, please try again.");
                    Console.WriteLine("The Minimum value should be: 4GB");
                    Console.WriteLine();
                    continue;
                }

                if (Is128GBModel && requestedAndroidTotalCapacity > 128 - 64 - 24)
                {
                    Console.WriteLine("Invalid input, please try again.");
                    Console.WriteLine("The Maximum value should be: " + (128 - 64 - 24) + "GB");
                    Console.WriteLine();
                    continue;
                }

                if (!Is128GBModel && requestedAndroidTotalCapacity > 256 - 64 - 34)
                {
                    Console.WriteLine("Invalid input, please try again.");
                    Console.WriteLine("The Maximum value should be: " + (256 - 64 - 34) + "GB");
                    Console.WriteLine();
                    continue;
                }

                break;
            }

            ulong requestedAndroidTotalCapacityInBytes = requestedAndroidTotalCapacity * 1024 * 1024 * 1024;
            ulong deviceCapacityInBytes = Is128GBModel ? 111_723_675_648ul : 239_651_758_080ul;
            ulong supposedWindowsCapacityInBytes = deviceCapacityInBytes - requestedAndroidTotalCapacityInBytes;

            Console.WriteLine("User Choice:");
            Console.WriteLine();
            Console.WriteLine($"Android: {requestedAndroidTotalCapacity}GB ({Math.Round((double)requestedAndroidTotalCapacityInBytes / (1000 * 1000 * 1000), 2)}GiB)");
            Console.WriteLine($"Windows: {Math.Round((double)supposedWindowsCapacityInBytes / (1024 * 1024 * 1024), 2)}GB ({Math.Round((double)supposedWindowsCapacityInBytes / (1000 * 1000 * 1000), 2)}GiB)");
            Console.WriteLine();

            Console.WriteLine("Calculating offsets...");
            Console.WriteLine();

            GPTUtils.MakeGPT(deviceCapacityInBytes, 4096, DeviceProfiles.Constants.OEMEP_UFS_LUN_0_PARTITIONS, requestedAndroidTotalCapacityInBytes);

            Console.WriteLine("");
            Console.WriteLine("Done! You can now use the commands provided above according to the");
            Console.WriteLine("instructions provided in the guide to achieve the desired storage allocation.");
            Console.WriteLine("");
            Console.Write("Press any key to exit . . . ");
            _ = Console.ReadKey();
        }
    }
}
