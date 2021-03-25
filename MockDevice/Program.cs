using System;

namespace MockDevice
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("[Device] Wrong Input Args!\nExiting...");
                Console.ReadKey();
                Environment.Exit(-1);
            }

            var device = new Device(args[0], args[1]);

            device.Connect();

            Console.ReadKey();
        }
    }
}