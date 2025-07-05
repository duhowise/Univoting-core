namespace Univoting.Akka.Messages;

public record GetCandidatesForPosition(string PositionId, string ElectionId) : VotingCommand;