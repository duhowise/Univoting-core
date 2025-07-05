namespace Univoting.Akka.Messages;

public record GetModeratorsForElection(string ElectionId) : VotingCommand;