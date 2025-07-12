namespace Univoting.Akka.Messages;

public record GetVotesForPosition(string PositionId, Guid ElectionId) : VotingCommand;