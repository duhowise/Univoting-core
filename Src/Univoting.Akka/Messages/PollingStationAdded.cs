namespace Univoting.Akka.Messages;

public record PollingStationAdded(string ElectionId, string PollingStationId, string Name) : VotingEvent;