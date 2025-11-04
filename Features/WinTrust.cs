using System;
using System.Runtime.InteropServices;

namespace CodeSigningDemo
{
    internal static class WinTrust
    {
        private static readonly Guid WINTRUST_ACTION_GENERIC_VERIFY_V2 =
            new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

        [DllImport("wintrust.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern int WinVerifyTrust(IntPtr hWnd, ref Guid pgActionID, IntPtr pWVTData);

        private enum WinTrustDataUIChoice : uint { All = 1, None = 2, NoBad = 3, NoGood = 4 }
        private enum WinTrustDataRevocationChecks : uint { None = 0x00000000, WholeChain = 0x00000001 }
        private enum WinTrustDataChoice : uint { File = 1 }
        private enum WinTrustDataStateAction : uint { Ignore = 0x00000000, Verify = 0x00000001, Close = 0x00000002 }

        [Flags]
        private enum WinTrustDataProvFlags : uint
        {
            None = 0x00000000,
            DisableMD2andMD4 = 0x00002000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WINTRUST_FILE_INFO
        {
            public uint cbStruct;
            public string pcwszFilePath;
            public IntPtr hFile;
            public IntPtr pgKnownSubject;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WINTRUST_DATA
        {
            public uint cbStruct;
            public IntPtr pPolicyCallbackData;
            public IntPtr pSIPClientData;
            public WinTrustDataUIChoice dwUIChoice;
            public WinTrustDataRevocationChecks fdwRevocationChecks;
            public WinTrustDataChoice dwUnionChoice;
            public IntPtr pFile; // WINTRUST_FILE_INFO*
            public WinTrustDataStateAction dwStateAction;
            public IntPtr hWVTStateData;
            public IntPtr pwszURLReference;
            public WinTrustDataProvFlags dwProvFlags;
            public uint dwUIContext;
        }

        internal static void PrintTrust(string path)
        {
            ConsoleUi.Section("Windows Trust Check (WinVerifyTrust)");
            int hr = VerifyFile(path);
            ConsoleUi.Status("Status", hr == 0);

            if (hr != 0)
            {
                var info = $"0x{(uint)hr:X8} {DescribeHResult(hr)}";
                ConsoleUi.KeyValue("HRESULT", info, ConsoleColor.Yellow);
            }
        }

        internal static int VerifyFile(string filePath)
        {
            var fileInfo = new WINTRUST_FILE_INFO
            {
                cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_FILE_INFO)),
                pcwszFilePath = filePath,
                hFile = IntPtr.Zero,
                pgKnownSubject = IntPtr.Zero
            };

            IntPtr pFileInfo = IntPtr.Zero;
            IntPtr pWinTrustData = IntPtr.Zero;

            try
            {
                pFileInfo = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_FILE_INFO)));
                Marshal.StructureToPtr(fileInfo, pFileInfo, false);

                var data = new WINTRUST_DATA
                {
                    cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_DATA)),
                    pPolicyCallbackData = IntPtr.Zero,
                    pSIPClientData = IntPtr.Zero,
                    dwUIChoice = WinTrustDataUIChoice.None,
                    fdwRevocationChecks = WinTrustDataRevocationChecks.WholeChain,
                    dwUnionChoice = WinTrustDataChoice.File,
                    pFile = pFileInfo,
                    dwStateAction = WinTrustDataStateAction.Verify,
                    hWVTStateData = IntPtr.Zero,
                    pwszURLReference = IntPtr.Zero,
                    dwProvFlags = WinTrustDataProvFlags.DisableMD2andMD4, // or .None
                    dwUIContext = 0
                };

                pWinTrustData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_DATA)));
                Marshal.StructureToPtr(data, pWinTrustData, false);

                var action = WINTRUST_ACTION_GENERIC_VERIFY_V2;
                int hr = WinVerifyTrust(IntPtr.Zero, ref action, pWinTrustData);

                data.dwStateAction = WinTrustDataStateAction.Close;
                Marshal.StructureToPtr(data, pWinTrustData, false);
                WinVerifyTrust(IntPtr.Zero, ref action, pWinTrustData);

                return hr; // 0 == TRUSTED
            }
            finally
            {
                if (pFileInfo != IntPtr.Zero) Marshal.FreeHGlobal(pFileInfo);
                if (pWinTrustData != IntPtr.Zero) Marshal.FreeHGlobal(pWinTrustData);
            }
        }

        internal static string DescribeHResult(int hr)
        {
            switch ((uint)hr)
            {
                // TRUST_*
                case 0x800B0100: return "TRUST_E_NOSIGNATURE: No signature present.";
                case 0x80096010: return "TRUST_E_BAD_DIGEST: Signature did not verify (bad digest).";
                case 0x80096004: return "TRUST_E_SUBJECT_FORM_UNKNOWN: Unsupported subject type.";
                case 0x800B0004: return "TRUST_E_SUBJECT_NOT_TRUSTED: Subject not trusted for the action.";

                // CERT_*
                case 0x800B0101: return "CERT_E_EXPIRED: Certificate outside its validity period.";
                case 0x800B0102: return "CERT_E_VALIDITYPERIODNESTING: Chain validity periods don't nest.";
                case 0x800B0103: return "CERT_E_ROLE: Certificate used in an invalid role.";
                case 0x800B0104: return "CERT_E_PATHLENCONSTRAINT: Path length constraint violated.";
                case 0x800B0105: return "CERT_E_CRITICAL: Unknown/unsupported critical extension.";
                case 0x800B0106: return "CERT_E_PURPOSE: Certificate used for a purpose not allowed.";
                case 0x800B0107: return "CERT_E_ISSUERCHAINING: Parent did not issue child certificate.";
                case 0x800B0108: return "CERT_E_MALFORMED: Malformed certificate.";
                case 0x800B0109: return "CERT_E_UNTRUSTEDROOT: Chain terminates in an untrusted root.";
                case 0x800B010A: return "CERT_E_CHAINING: Could not build a chain to a trusted root.";
                case 0x800B010B: return "CERT_E_UNTRUSTEDTESTROOT: Root is a test root (not trusted).";

                // CRYPT_* (revocation)
                case 0x80092010: return "CRYPT_E_REVOKED: A certificate in the chain is revoked.";
                case 0x80092011: return "CRYPT_E_NO_REVOCATION_DLL: Revocation DLL not found.";
                case 0x80092012: return "CRYPT_E_NO_REVOCATION_CHECK: Revocation check not possible.";
                case 0x80092013: return "CRYPT_E_REVOCATION_OFFLINE: Revocation server offline/unreachable.";

                default: return "Unrecognized HRESULT.";
            }
        }
    }
}
