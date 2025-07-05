namespace Univoting.Akka.Models;

public class VotingResult
{
    public string CandidateId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public int Priority { get; set; }
}