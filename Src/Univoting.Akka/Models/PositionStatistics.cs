namespace Univoting.Akka.Models;

public class PositionStatistics
{
    public string PositionId { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public int SkippedCount { get; set; }
    public int TotalParticipation { get; set; }
}