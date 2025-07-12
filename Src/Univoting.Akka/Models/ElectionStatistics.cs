namespace Univoting.Akka.Models;

public class ElectionStatistics
{
    public Guid ElectionId { get; set; } =Guid.Empty;
    public string ElectionName { get; set; } = string.Empty;
    public int TotalVoters { get; set; }
    public int TotalPositions { get; set; }
    public List<PositionStatistics> PositionStatistics { get; set; } = new();
    public double OverallParticipationRate { get; set; }
}