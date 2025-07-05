namespace Univoting.Akka.Messages;

public record GetPosition(string PositionId, string ElectionId) : VotingCommand;