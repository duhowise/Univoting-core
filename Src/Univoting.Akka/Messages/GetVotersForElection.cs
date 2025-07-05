namespace Univoting.Akka.Messages;

public record GetVotersForElection(string ElectionId) : VotingCommand;