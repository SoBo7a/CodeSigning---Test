using System;
using System.Security.Cryptography.X509Certificates;

namespace CodeSigningDemo
{
    internal static class Signer
    {
        internal static void Print(string path)
        {
            ConsoleUi.Section("Signature");
            try
            {
                var raw = X509Certificate.CreateFromSignedFile(path);
                var cert = new X509Certificate2(raw);

                ConsoleUi.KeyValue("Subject (Publisher)", cert.Subject, ConsoleColor.White);
                ConsoleUi.KeyValue("Issuer", cert.Issuer);
                ConsoleUi.KeyValue("Valid From", cert.NotBefore.ToString("u"));
                ConsoleUi.KeyValue("Valid To", cert.NotAfter.ToString("u"));
                ConsoleUi.KeyValue("Thumbprint", cert.Thumbprint);
            }
            catch (Exception ex)
            {
                ConsoleUi.KeyValue("Info", $"No Authenticode certificate found ({ex.Message})", ConsoleColor.Yellow);
            }
        }
    }
}
