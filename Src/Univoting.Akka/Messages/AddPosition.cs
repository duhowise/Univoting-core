namespace Univoting.Akka.Messages;

public record AddPosition(Guid ElectionId, string PositionId, string Name, int Priority) : VotingCommand;