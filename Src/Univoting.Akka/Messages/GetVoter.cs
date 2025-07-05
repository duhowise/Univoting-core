namespace Univoting.Akka.Messages;

public record GetVoter(string VoterId) : VotingCommand;