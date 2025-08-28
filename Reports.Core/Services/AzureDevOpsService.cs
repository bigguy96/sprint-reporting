using System.Text.Json;
using Reports.Core.Infrastructure;
using Reports.Core.Models;

namespace Reports.Core.Services;

public class AzureDevOpsService
{
    private readonly string _org;
    private readonly string _project;
    private readonly string _team;
    private readonly string _pat;

    public AzureDevOpsService(string organization, string project, string team, string pat)
    {
        _org = organization;
        _project = project;
        _team = team;
        _pat = pat;
    }

    public async Task<SprintInfo> GetCurrentSprintAsync()
    {
        using var client = new AzureDevOpsClient(_org, _project, _team, _pat);
        var (start, end, id, name, path) = await client.GetCurrentSprintAsync();
        return new SprintInfo(id, name, start, end, path);
    }

    public async Task<IReadOnlyList<WorkItemInfo>> GetWorkItemsForIterationAsync(string iterationPath)
    {
        using var client = new AzureDevOpsClient(_org, _project, _team, _pat);
        var ids = await client.QueryWorkItemIdsByIterationPathAsync(iterationPath);
        if (ids.Count == 0) return Array.Empty<WorkItemInfo>();

        var docs = await client.GetWorkItemsInBatchesAsync(ids);
        var items = new List<WorkItemInfo>();
        foreach (var doc in docs)
        {
            foreach (var el in doc.RootElement.GetProperty("value").EnumerateArray())
            {
                var fields = el.GetProperty("fields");
                var wi = new WorkItemInfo(
                Id: el.GetProperty("id").GetInt32(),
                Title: fields.GetStringField("System.Title"),
                State: fields.GetStringField("System.State"),
                CreatedDate: fields.GetDateField("System.CreatedDate"),
                AssignedToDisplayName: fields.GetIdentityDisplayName(),
                AssignedToUniqueName: fields.GetIdentityUniqueName()
                );
                items.Add(wi);
            }
        }
        return items.OrderBy(w => w.Id).ToList();
    }

    public async Task<IReadOnlyList<StatusChange>> GetStatusChangesTodayAsync(IEnumerable<int> workItemIds, DateTime today)
    {
        using var client = new AzureDevOpsClient(_org, _project, _team, _pat);
        var list = new List<StatusChange>();
        foreach (var id in workItemIds)
        {
            using var revDoc = await client.GetRevisionsAsync(id);
            var arr = revDoc.RootElement.GetProperty("value").EnumerateArray().ToList();
            if (arr.Count == 0) continue;


            string currentState = arr[0].GetProperty("fields").GetProperty("System.State").GetString() ?? "";
            for (int i = 1; i < arr.Count; i++)
            {
                var prevState = currentState;
                var fields = arr[i].GetProperty("fields");
                if (!fields.TryGetProperty("System.State", out var newStateEl)) continue; // state unchanged in this revision
                var newState = newStateEl.GetString() ?? prevState;


                // ChangedDate lives in fields["System.ChangedDate"] (ISO string)
                DateTime changed = fields.TryGetProperty("System.ChangedDate", out var changedEl) && changedEl.ValueKind == JsonValueKind.String
                ? DateTime.Parse(changedEl.GetString()!)
                : DateTime.MinValue;


                if (!string.Equals(prevState, newState, StringComparison.OrdinalIgnoreCase))
                {
                    if (changed.Date == today.Date)
                    {
                        list.Add(new StatusChange(id, changed, prevState, newState));
                    }
                    currentState = newState;
                }
            }
        }
        return list;
    }

    public async Task<ReportData> BuildDailyReportAsync()
    {
        var sprint = await GetCurrentSprintAsync();
        var items = await GetWorkItemsForIterationAsync(sprint.IterationPath);
        var changes = await GetStatusChangesTodayAsync(items.Select(i => i.Id), DateTime.Today);
        return new ReportData
        {
            Today = DateTime.Today,
            Sprint = sprint,
            WorkItems = items,
            StatusChangesToday = changes
        };
    }
}