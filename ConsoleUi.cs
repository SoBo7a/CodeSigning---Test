using System;

namespace CodeSigningDemo
{
    internal static class ConsoleUi
    {
        private static int Width => Math.Max(50, Math.Min(Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 80, 100));

        internal static void Header(string title)
        {
            var w = Width;
            var top = "╔" + new string('═', w - 2) + "╗";
            var bot = "╚" + new string('═', w - 2) + "╝";
            var pad = Math.Max(0, w - 2 - title.Length);
            var left = pad / 2;
            var right = pad - left;
            var mid = "║" + new string(' ', left) + title + new string(' ', right) + "║";

            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(top);
            Console.WriteLine(mid);
            Console.WriteLine(bot);
            Console.ForegroundColor = prev;
        }

        internal static void Section(string title)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;
            var w = Width;
            var label = $"[ {title} ]";
            var dashes = Math.Max(0, w - label.Length - 1);
            Console.WriteLine(label + " " + new string('─', dashes));
            Console.ForegroundColor = prev;
        }

        internal static void KeyValue(string key, string value, ConsoleColor? valueColor = null)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  " + key + ": ");
            Console.ForegroundColor = valueColor ?? prev;
            Console.WriteLine(value);
            Console.ForegroundColor = prev;
        }

        internal static void Status(string label, bool ok)
        {
            var prev = Console.ForegroundColor;
            Console.Write("  " + label + ": ");
            Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine(ok ? "TRUSTED" : "NOT TRUSTED");
            Console.ForegroundColor = prev;
        }

        internal static void SubBlock(string title, string content)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  --- " + title + " ---");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(content);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('-', Math.Min(content.Length, 40)));
            Console.ForegroundColor = prev;
        }
    }
}
