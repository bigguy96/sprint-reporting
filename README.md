## Quick Start

1. **Build & Run**
```bash
dotnet build
dotnet run --project Reports.Cli -- \
--org your-org --project YourProject --team YourTeam \
--output DailySummary.xlsx
```

2. **What it does**
- Detects **current sprint** (start/end, name, path)
- Fetches all work items in that sprint
- Computes:
- New items **today**
- New items **within sprint window**
- Current **state counts**
- **Status transitions today** (with details)
- Exports to **Excel** (Summary, WorkItems, StatusChangesToday)