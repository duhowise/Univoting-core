namespace Univoting.Akka.Messages;

public record GetSkippedVoteCount(string PositionId, Guid ElectionId) : VotingCommand;