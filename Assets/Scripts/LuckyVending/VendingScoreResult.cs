using System.Collections.Generic;

public sealed class VendingScoreResult
{
    public int Total { get; }
    public List<string> LogLines { get; }
    public HashSet<int> HighlightSlots { get; }

    public VendingScoreResult(int total, List<string> logLines, HashSet<int> highlightSlots)
    {
        Total = total;
        LogLines = logLines;
        HighlightSlots = highlightSlots;
    }
}
