namespace Reports.Core.Models;

public record StatusChange(int WorkItemId, DateTime When, string FromState, string ToState);