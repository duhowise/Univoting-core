namespace Univoting.Akka.Messages;

public record RegisterVoter(string ElectionId, string VoterId, string Name, string IdentificationNumber) : VotingCommand;