namespace Univoting.Akka.Messages;

public record GetPosition(string PositionId, Guid ElectionId) : VotingCommand;