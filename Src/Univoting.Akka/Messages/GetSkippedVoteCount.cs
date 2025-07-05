namespace Univoting.Akka.Messages;

public record GetSkippedVoteCount(string PositionId) : VotingCommand;