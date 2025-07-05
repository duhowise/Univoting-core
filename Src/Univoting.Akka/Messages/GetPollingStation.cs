namespace Univoting.Akka.Messages;

public record GetPollingStation(string PollingStationId) : VotingCommand;