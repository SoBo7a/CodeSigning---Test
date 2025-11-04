using System;
using System.IO;
using System.Text;

namespace CodeSigningDemo
{
    internal static class Motw
    {
        internal static void Print(string path)
        {
            ConsoleUi.Section("Mark-of-the-Web");
            var (present, content) = TryRead(path);
            ConsoleUi.KeyValue("Present", present ? "yes" : "no", present ? ConsoleColor.Yellow : (ConsoleColor?)null);
            if (present && !string.IsNullOrWhiteSpace(content))
                ConsoleUi.SubBlock("Zone.Identifier", content.Trim());
        }

        internal static (bool present, string content) TryRead(string path)
        {
            var adsPath = path + ":Zone.Identifier";
            try
            {
                using (var sr = new StreamReader(
                    new FileStream(adsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                    Encoding.UTF8))
                {
                    return (true, sr.ReadToEnd());
                }
            }
            catch
            {
                return (false, string.Empty);
            }
        }
    }
}
