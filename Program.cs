using System;
using System.Reflection;

namespace CodeSigningDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var self = Assembly.GetExecutingAssembly().Location;

            ConsoleUi.Header("WHO SIGNED ME?");
            Console.WriteLine($"Path: {self}");
            Console.WriteLine();

            Signer.Print(self);
            Console.WriteLine();

            WinTrust.PrintTrust(self);

            Console.WriteLine();
            Console.WriteLine("Hint: Download this EXE with a modern web browser to keep MOTW and trigger SmartScreen.");
            Console.WriteLine();
            Console.Write("Press any key to exit...");
            try { Console.ReadKey(true); } catch { }
        }
    }
}
