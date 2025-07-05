namespace Univoting.Akka.Actors.States;

public class PositionState
{
    public string PositionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; }
    public Dictionary<string, CandidateState> Candidates { get; set; } = new();
    public Dictionary<string, VoteState> Votes { get; set; } = new();
    public Dictionary<string, SkippedVoteState> SkippedVotes { get; set; } = new();
}
