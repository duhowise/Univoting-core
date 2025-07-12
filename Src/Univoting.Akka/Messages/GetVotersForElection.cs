namespace Univoting.Akka.Messages;

public record GetVotersForElection(Guid ElectionId) : VotingCommand;