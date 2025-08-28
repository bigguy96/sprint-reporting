namespace Reports.Core.Models;

public record WorkItemInfo(int Id, string Title, string State, DateTime CreatedDate, string? AssignedToDisplayName, string? AssignedToUniqueName);