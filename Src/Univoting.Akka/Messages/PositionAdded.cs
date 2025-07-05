namespace Univoting.Akka.Messages;

public record PositionAdded(string ElectionId, string PositionId, string Name, int Priority) : VotingEvent;