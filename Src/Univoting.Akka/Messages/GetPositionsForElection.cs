namespace Univoting.Akka.Messages;

public record GetPositionsForElection(string ElectionId) : VotingCommand;