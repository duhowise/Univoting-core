namespace Univoting.Akka.Messages;

public record GetVotingResults(string PositionId, string ElectionId) : VotingCommand;