namespace Univoting.Akka.Messages;

public record GetCandidatesForPosition(string PositionId) : VotingCommand;