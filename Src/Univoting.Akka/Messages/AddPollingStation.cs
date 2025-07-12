namespace Univoting.Akka.Messages;

public record AddPollingStation(Guid ElectionId, string PollingStationId, string Name) : VotingCommand;