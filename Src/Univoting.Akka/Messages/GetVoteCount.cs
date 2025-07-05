namespace Univoting.Akka.Messages;

public record GetVoteCount(string PositionId) : VotingCommand;