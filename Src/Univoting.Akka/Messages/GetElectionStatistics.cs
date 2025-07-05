namespace Univoting.Akka.Messages;

public record GetElectionStatistics(string ElectionId) : VotingCommand;