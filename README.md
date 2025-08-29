# Reports CLI

This .NET 9 CLI application fetches Azure DevOps work item summaries for a sprint and exports the data to an Excel file.

The application supports **layered configuration**:

1. **User Secrets** (local development)
2. **Environment Variables**
3. **Windows Credential Manager** (production / end-users on Windows)

---

## Prerequisites

* Windows machine
* .NET 9 SDK installed
* Azure DevOps Personal Access Token (PAT)

---

## 1. Configuration

### Option A: User Secrets (local dev)

Initialize user secrets in your CLI project folder:

```bash
cd Reports.Cli
dotnet user-secrets init
```

Then set your Azure DevOps settings:

```bash
dotnet user-secrets set "AzDo:Org" "your-org"
dotnet user-secrets set "AzDo:Project" "YourProject"
dotnet user-secrets set "AzDo:Team" "YourTeam"
dotnet user-secrets set "AzDo:Token" "<PAT>"
```

### Option B: Environment Variables

Set the following environment variables (Windows example using PowerShell):

```powershell
setx AZDO_ORG your-org
setx AZDO_PROJECT YourProject
setx AZDO_TEAM YourTeam
setx AZDO_TOKEN <PAT>
```

> These persist across sessions. Open a new terminal to use them.

### Option C: Windows Credential Manager (secure)

Store credentials securely in Windows Credential Manager:

```powershell
cmdkey /generic:AzDo:Org     /user:ignored /pass:your-org
cmdkey /generic:AzDo:Project /user:ignored /pass:YourProject
cmdkey /generic:AzDo:Team    /user:ignored /pass:YourTeam
cmdkey /generic:AzDo:Token   /user:ignored /pass:<PAT>
```

> `User` field is ignored. Values are encrypted and accessible only by your user account.

---

## 2. Running the CLI

Build and run the project:

```powershell
dotnet run --project src/AzDoReports.Cli
```

* The app automatically loads configuration from User Secrets, Env Vars, or Windows Credential Manager.
* The Excel report will be saved to your **My Documents** folder by default:

```
C:\Users\<YourUser>\Documents\DailySummary_YYYYMMDD.xlsx
```

* You can optionally override the output path by editing the code or later adding a CLI argument.

---

## 3. Output

The Excel file includes:

* All work items in the current sprint
* Daily status updates
* `AssignedTo` field
* Count of new items today and during the sprint

---

## Notes

* This CLI is **Windows-only** due to Credential Manager integration.
* Fully compatible with **.NET 9**.
* Supports layered configuration for flexibility and security.

---

## Future Enhancements

* Optional GUI using WPF or Blazor.
* Cross-platform credential storage (Keychain / SecretService) for Linux/macOS.
* Automatic scheduling via Task Scheduler or Windows Service.