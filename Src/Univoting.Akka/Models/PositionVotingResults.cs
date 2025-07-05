namespace Univoting.Akka.Models;

public class PositionVotingResults
{
    public string PositionId { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public int TotalSkipped { get; set; }
    public List<VotingResult> CandidateResults { get; set; } = new();
}