namespace Univoting.Akka.Messages;

public record GetVotingResults(string PositionId) : VotingCommand;