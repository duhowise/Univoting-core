namespace Univoting.Akka.Models;

public class VoterVotingHistory
{
    public string VoterId { get; set; } = string.Empty;
    public string VoterName { get; set; } = string.Empty;
    public VotingStatus Status { get; set; }
    public List<string> VotedPositions { get; set; } = new();
    public List<string> SkippedPositions { get; set; } = new();
    public Dictionary<string, DateTime> VotingTimestamps { get; set; } = new();
}