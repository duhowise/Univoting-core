namespace Univoting.Akka.Actors.States;

public class VoteState
{
    public string VoteId { get; set; } = string.Empty;
    public string VoterId { get; set; } = string.Empty;
    public string CandidateId { get; set; } = string.Empty;
    public DateTime Time { get; set; }
}
