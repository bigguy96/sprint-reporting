using Reports.Core.Export;
using Reports.Core.Services;

// Usage:
// dotnet run --project Reports.Cli -- --org your-org --project YourProject --team YourTeam
/* 
    dotnet run --project Reports.Cli -- `
    --org your-org `
    --project YourProject `
    --team YourTeam
*/

var pat = Environment.GetEnvironmentVariable("AZDO_PAT");
if (string.IsNullOrWhiteSpace(pat))
{
    Console.Error.WriteLine("ERROR: AZDO_PAT environment variable is not set.");
    return 1;
}

var org = GetArg("--org") ?? throw new ArgumentException("--org is required");
var project = GetArg("--project") ?? throw new ArgumentException("--project is required");
var team = GetArg("--team") ?? throw new ArgumentException("--team is required");
var output = GetArg("--output") ?? $"DailySummary_{DateTime.Today:yyyyMMdd}.xlsx";

var svc = new AzureDevOpsService(org, project, team, pat);
Console.WriteLine($"Fetching current sprint for {org}/{project}/{team}…");
var report = await svc.BuildDailyReportAsync();

Console.WriteLine($"Found sprint '{report.Sprint.Name}' {report.Sprint.Start:yyyy-MM-dd} → {report.Sprint.End:yyyy-MM-dd}");
Console.WriteLine($"Work items: {report.WorkItems.Count}");
Console.WriteLine($"New today: {report.NewItemsToday}, New in sprint: {report.NewItemsInSprint}");

ExcelExporter.SaveReport(report, output);
Console.WriteLine($"Excel saved → {output}");

return 0;

static string? GetArg(string name)
{
    var args = Environment.GetCommandLineArgs();
    for (var i = 0; i < args.Length; i++)
    {
        if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
        {
            return (i + 1 < args.Length) ? args[i + 1] : null;
        }
    }
    return null;
}