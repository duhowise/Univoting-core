namespace Univoting.Akka.Messages;

public record PollingStationAdded(Guid ElectionId, string PollingStationId, string Name) : VotingEvent;