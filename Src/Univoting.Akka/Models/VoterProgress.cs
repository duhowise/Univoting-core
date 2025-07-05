namespace Univoting.Akka.Models;

public class VoterProgress
{
    public string VoterId { get; set; } = string.Empty;
    public int TotalPositions { get; set; }
    public int CompletedPositions { get; set; }
    public int VotedPositions { get; set; }
    public int SkippedPositions { get; set; }
    public double ProgressPercentage { get; set; }
    public bool IsComplete { get; set; }
}