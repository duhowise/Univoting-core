namespace Univoting.Akka.Messages;

public record GetPositionsForElection(Guid ElectionId) : VotingCommand;