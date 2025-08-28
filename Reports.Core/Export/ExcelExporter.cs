using Reports.Core.Models;
using ClosedXML.Excel;

namespace Reports.Core.Export;

public static class ExcelExporter
{
    public static void SaveReport(ReportData report, string filePath)
    {
        using var wb = new XLWorkbook();
        // Summary
        var s = wb.Worksheets.Add("Summary");
        s.Cell(2, 1).Value = "Sprint";
        s.Cell(2, 2).Value = report.Sprint.Name;
        s.Cell(3, 1).Value = "Sprint Window";
        s.Cell(3, 2).Value = $"{report.Sprint.Start:yyyy-MM-dd} → {report.Sprint.End:yyyy-MM-dd}";
        s.Cell(5, 1).Value = "New Work Items Today";
        s.Cell(5, 2).Value = report.NewItemsToday;
        s.Cell(6, 1).Value = "New Work Items In Sprint";
        s.Cell(6, 2).Value = report.NewItemsInSprint;

        // Current state counts
        s.Cell(8, 1).Value = "Current State";
        s.Cell(8, 2).Value = "Count";
        int r = 9;
        foreach (var kv in report.CurrentStateCounts.OrderBy(k => k.Key))
        {
            s.Cell(r, 1).Value = kv.Key;
            s.Cell(r, 2).Value = kv.Value;
            r++;
        }

        // Status updates today
        r += 2;
        s.Cell(r, 1).Value = "Status Updates Today (into state)"; r++;
        s.Cell(r, 1).Value = "To State"; s.Cell(r, 2).Value = "Count"; r++;
        foreach (var kv in report.StatusUpdatesIntoStateToday.OrderByDescending(k => k.Value))
        {
            s.Cell(r, 1).Value = kv.Key;
            s.Cell(r, 2).Value = kv.Value;
            r++;
        }

        // Work items detail
        var d = wb.Worksheets.Add("WorkItems");
        d.Cell(1, 1).Value = "ID";
        d.Cell(1, 2).Value = "Title";
        d.Cell(1, 3).Value = "State";
        d.Cell(1, 4).Value = "CreatedDate";
        d.Cell(1, 5).Value = "AssignedTo";
        d.Cell(1, 6).Value = "AssignedTo (unique)";

        int i = 2;
        foreach (var wi in report.WorkItems.OrderBy(w => w.Id))
        {
            d.Cell(i, 1).Value = wi.Id;
            d.Cell(i, 2).Value = wi.Title;
            d.Cell(i, 3).Value = wi.State;
            d.Cell(i, 4).Value = wi.CreatedDate;
            d.Cell(i, 5).Value = wi.AssignedToDisplayName ?? "Unassigned";
            d.Cell(i, 6).Value = wi.AssignedToUniqueName ?? string.Empty;
            i++;
        }

        // Status change detail
        var c = wb.Worksheets.Add("StatusChangesToday");
        c.Cell(1, 1).Value = "WorkItemId";
        c.Cell(1, 2).Value = "When";
        c.Cell(1, 3).Value = "From";
        c.Cell(1, 4).Value = "To";
        int j = 2;
        foreach (var sc in report.StatusChangesToday.OrderBy(x => x.When))
        {
            c.Cell(j, 1).Value = sc.WorkItemId;
            c.Cell(j, 2).Value = sc.When;
            c.Cell(j, 3).Value = sc.FromState;
            c.Cell(j, 4).Value = sc.ToState;
            j++;
        }

        wb.SaveAs(filePath);
    }
}