using Reports.Core.Export;
using Reports.Core.Services;
using Reports.Cli;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var config = new ConfigurationBuilder()
        .AddUserSecrets(typeof(Program).Assembly, optional: true)
        .AddEnvironmentVariables(prefix: "AZDO_")
        .AddWindowsCredentialManager(["AzDo:Org", "AzDo:Project", "AzDo:Team", "AzDo:Token"])
        .Build();

        string? org = config["AzDo:Org"];
        string? project = config["AzDo:Project"];
        string? team = config["AzDo:Team"];
        string? token = config["AzDo:Token"];
        string output = config["Output"] ?? Path.Combine(myDocuments, $"DailySummary_{DateTime.Today:yyyyMMdd}.xlsx");

        var svc = new AzureDevOpsService(org!, project!, team!, token!);
        Console.WriteLine($"Fetching sprint for {org}/{project}/{team}...");
        var report = await svc.BuildDailyReportAsync();

        Console.WriteLine($"Sprint: '{report.Sprint.Name}' {report.Sprint.Start:yyyy-MM-dd} → {report.Sprint.End:yyyy-MM-dd}");
        Console.WriteLine($"Work items: {report.WorkItems.Count}");
        Console.WriteLine($"New today: {report.NewItemsToday}, New in sprint: {report.NewItemsInSprint}");

        ExcelExporter.SaveReport(report, output);
        Console.WriteLine($"Excel saved → {output}");

        return 0;
    }
}