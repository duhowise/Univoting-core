namespace Univoting.Akka.Messages;

public record VoterRegistered(Guid ElectionId, string VoterId, string Name, string IdentificationNumber) : VotingEvent;