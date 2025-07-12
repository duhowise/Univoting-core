namespace Univoting.Akka.Messages;

public record GetAllVotesForElection(Guid ElectionId) : VotingCommand;
