namespace Univoting.Akka.Messages;

public record GetModerator(string ModeratorId) : VotingCommand;