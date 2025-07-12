namespace Univoting.Akka.Messages;

public record RegisterVoter(Guid ElectionId, string VoterId, string Name, string IdentificationNumber) : VotingCommand;