namespace Univoting.Akka.Actors.States;

public class CandidateState
{
    public string CandidateId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public byte[]? Picture { get; set; }
    public int Priority { get; set; }
}
