using Univoting.Akka.SharedModels;

namespace Univoting.Akka.Actors.States;

public class VoterState
{
    public string VoterId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IdentificationNumber { get; set; } = string.Empty;
    public VotingStatus Status { get; set; } = VotingStatus.Pending;
}
