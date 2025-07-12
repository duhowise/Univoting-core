namespace Univoting.Akka.Messages;

public record GetCandidatesForPosition(string PositionId, Guid ElectionId) : VotingCommand;