namespace Reports.Core.Models;

public class ReportData
{
    public required DateTime Today { get; init; }
    public required SprintInfo Sprint { get; init; }
    public required IReadOnlyList<WorkItemInfo> WorkItems { get; init; }
    public required IReadOnlyList<StatusChange> StatusChangesToday { get; init; }
    public int NewItemsToday => WorkItems.Count(w => w.CreatedDate.Date == Today.Date);
    public int NewItemsInSprint => WorkItems.Count(w => w.CreatedDate.Date >= Sprint.Start.Date && w.CreatedDate.Date <= Sprint.End.Date);
    public IReadOnlyDictionary<string, int> CurrentStateCounts => WorkItems
    .GroupBy(w => w.State)
    .ToDictionary(g => g.Key, g => g.Count());
    public IReadOnlyDictionary<string, int> StatusUpdatesIntoStateToday => StatusChangesToday
    .GroupBy(sc => sc.ToState)
    .ToDictionary(g => g.Key, g => g.Count());
}