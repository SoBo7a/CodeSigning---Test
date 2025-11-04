# Who Signed Me?

Small Windows **console app** that inspects the **digital signature** of the running EXE, shows whether the file has a **Mark-of-the-Web (MOTW)**, and performs a **Windows trust check** via `WinVerifyTrust` using the Authenticode policy (the same decision UAC uses for *Verified publisher* vs *Unknown publisher*).

---

## What it does

- **Signature info** (from the embedded Authenticode signer): Subject/Issuer, validity, thumbprint.  
  *Note: this just reads the signer – it does not decide trust.*
- **MOTW**: prints the `Zone.Identifier` ADS contents if present (i.e., downloaded from the internet).
- **Windows trust decision**: calls `WinVerifyTrust` (Authenticode policy) and prints:
  - `TRUSTED` → would show **Verified publisher** in UAC.
  - `NOT TRUSTED` → prints the **HRESULT** in hex and a short description (e.g., `0x800B0109 CERT_E_UNTRUSTEDROOT`).

> **SmartScreen** is separate from signature trust (reputation-based). To trigger SmartScreen, host the EXE and download it with a modern browser so the file keeps its MOTW.

---

## Build

- Open the solution in **Visual Studio** (Console App, .NET Framework).
- Build **Release**.

Output EXE: `.\bin\Release\CodeSigning - Test.exe`

---

## Run

Just start the EXE.  
It verifies **itself** (no arguments needed).  
Press **any key** to exit.

---

## Signing the EXE

You can sign the built EXE with either a **self-signed test certificate** or a **public trusted token-based certificate**.

### A) Sign with a self-signed test certificate (GUI via Visual Studio)

1. **Create the test PFX in Visual Studio**
   - `Project → Properties → Signing`
   - Tick **Sign the ClickOnce manifests**
   - Click **Create Test Certificate…**, set a password  
   - A file like `CodeSigning - Test_…_TemporaryKey.pfx` appears in your project

2. **Sign with SignTool (cmd.exe)**
   ```cmd
   signtool sign /fd SHA256 /f "<full path>\CodeSigning - Test_…_TemporaryKey.pfx" /p "<YOUR_PASSWORD>" /tr http://timestamp.globalsign.com/tsa/r6advanced1 /td SHA256 ".\bin\Release\CodeSigning - Test.exe"
   ```

---

### B) Sign with a **token/HSM certificate**

**Sign (auto-select certificate)**
```cmd
signtool sign /a /tr http://timestamp.globalsign.com/tsa/r6advanced1 /td SHA256 /fd SHA256 ".\bin\Release\CodeSigning - Test.exe"
```

**Sign (explicit certificate by thumbprint)**
```cmd
signtool sign /fd SHA256 /sha1 <YOUR_CERT_THUMBPRINT_NO_SPACES> /tr http://timestamp.globalsign.com/tsa/r6advanced1 /td SHA256 ".\bin\Release\CodeSigning - Test.exe"
```

### C) Verify the Signature using SignTool
```cmd
signtool verify /pa /v /all ".\bin\Release\CodeSigning - Test.exe"
```

---

## Triggering SmartScreen (optional demo)

1. Host the signed EXE on any HTTPS location (SharePoint/OneDrive/website/GitHub release).  
2. **Download with Microsoft Edge** and run from **Downloads** so the file keeps **MOTW**.  
3. Expect a SmartScreen prompt for new publishers. If you continue, UAC will show **Verified publisher** (if the trust check passes), matching the app’s `TRUSTED`.
