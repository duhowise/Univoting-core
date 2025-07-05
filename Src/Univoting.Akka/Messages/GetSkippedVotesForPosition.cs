namespace Univoting.Akka.Messages;

public record GetSkippedVotesForPosition(string PositionId) : VotingCommand;