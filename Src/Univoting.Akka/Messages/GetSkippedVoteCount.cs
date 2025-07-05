namespace Univoting.Akka.Messages;

public record GetSkippedVoteCount(string PositionId, string ElectionId) : VotingCommand;