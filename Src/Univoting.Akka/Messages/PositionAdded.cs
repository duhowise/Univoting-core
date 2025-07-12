namespace Univoting.Akka.Messages;

public record PositionAdded(Guid ElectionId, string PositionId, string Name, int Priority) : VotingEvent;