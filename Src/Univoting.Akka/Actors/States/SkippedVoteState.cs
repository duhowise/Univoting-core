namespace Univoting.Akka.Actors.States;

public class SkippedVoteState
{
    public string SkippedVoteId { get; set; } = string.Empty;
    public string VoterId { get; set; } = string.Empty;
    public DateTime Time { get; set; }
}
