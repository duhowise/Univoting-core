namespace Univoting.Akka.Messages;

public record VoterRegistered(string ElectionId, string VoterId, string Name, string IdentificationNumber) : VotingEvent;