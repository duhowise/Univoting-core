namespace Univoting.Akka.Messages;

public record GetVoterHistory(string VoterId) : VotingCommand;