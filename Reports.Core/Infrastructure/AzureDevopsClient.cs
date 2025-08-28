using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Reports.Core.Infrastructure;

internal class AzureDevOpsClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly string? _org;
    private readonly string? _project;
    private readonly string? _team;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private string BaseUrl => $"https://dev.azure.com/{_org}/{_project}";

    public AzureDevOpsClient(string organization, string project, string team, string pat)
    {
        _org = organization;
        _project = project;
        _team = team;
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}"))
        );
    }

    public async Task<(DateTime start, DateTime end, string id, string name, string path)> GetCurrentSprintAsync()
    {
        var url = $"{BaseUrl}/{_team}/_apis/work/teamsettings/iterations?$timeframe=current&api-version=7.0";
        using var resp = await _http.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        await using var stream = await resp.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(stream);
        var value = doc.RootElement.GetProperty("value");
        if (value.GetArrayLength() == 0) throw new InvalidOperationException("No current sprint found.");
        var first = value[0];
        var attributes = first.GetProperty("attributes");
        var start = attributes.GetProperty("startDate").GetDateTime();
        var finish = attributes.GetProperty("finishDate").GetDateTime();
        var id = first.GetProperty("id").GetString()!;
        var name = first.GetProperty("name").GetString()!;
        var path = first.GetProperty("path").GetString()!;

        return (start, finish, id, name, path);
    }

    public async Task<IReadOnlyList<int>> QueryWorkItemIdsByIterationPathAsync(string iterationPath)
    {
        var wiql = new
        {
            query = $@"SELECT [System.Id]
                    FROM workitems
                    WHERE [System.TeamProject] = @project
                    AND [System.IterationPath] = '{iterationPath.Replace("'", "''")}'
                    ORDER BY [System.CreatedDate] ASC"
        };
        using var content = new StringContent(JsonSerializer.Serialize(wiql), Encoding.UTF8, "application/json");
        using var resp = await _http.PostAsync($"{BaseUrl}/_apis/wit/wiql?api-version=7.0", content);
        resp.EnsureSuccessStatusCode();
        await using var stream = await resp.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(stream);
        var arr = doc.RootElement.GetProperty("workItems");
        var ids = new List<int>(arr.GetArrayLength());

        foreach (var el in arr.EnumerateArray()) ids.Add(el.GetProperty("id").GetInt32());

        return ids;
    }

    public async Task<IReadOnlyList<JsonDocument>> GetWorkItemsInBatchesAsync(IEnumerable<int> ids)
    {
        var results = new List<JsonDocument>();
        const int batchSize = 200;
        var idBatches = ids
        .Select((id, idx) => new { id, idx })
        .GroupBy(x => x.idx / batchSize)
        .Select(g => g.Select(x => x.id));

        foreach (var batch in idBatches)
        {
            var joined = string.Join(',', batch);
            var url = $"{BaseUrl}/_apis/wit/workitems?ids={joined}&$expand=fields&api-version=7.0";
            using var resp = await _http.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            // Keep whole JSON so we can read complex identity fields
            var json = await resp.Content.ReadAsStringAsync();
            results.Add(JsonDocument.Parse(json));
        }

        return results;
    }

    public async Task<JsonDocument> GetRevisionsAsync(int workItemId)
    {
        var url = $"{BaseUrl}/_apis/wit/workitems/{workItemId}/revisions?api-version=7.0";
        using var resp = await _http.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
    
    public void Dispose() => _http?.Dispose();
}