namespace Univoting.Akka.Messages;

public record GetElectionStatistics(Guid ElectionId) : VotingCommand;