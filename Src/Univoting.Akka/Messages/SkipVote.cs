namespace Univoting.Akka.Messages;

public record SkipVote(string VoterId, string PositionId, Guid ElectionId) : VotingCommand;