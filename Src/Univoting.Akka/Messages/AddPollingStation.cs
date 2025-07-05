namespace Univoting.Akka.Messages;

public record AddPollingStation(string ElectionId, string PollingStationId, string Name) : VotingCommand;