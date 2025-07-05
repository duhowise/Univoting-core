namespace Univoting.Akka.Messages;

public record AddPosition(string ElectionId, string PositionId, string Name, int Priority) : VotingCommand;