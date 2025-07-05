namespace Univoting.Akka.Messages;

public record GetElection(string ElectionId) : VotingCommand;