namespace Univoting.Akka.Messages;

public record GetElection(Guid ElectionId) : VotingCommand;