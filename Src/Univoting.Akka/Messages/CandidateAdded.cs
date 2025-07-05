namespace Univoting.Akka.Messages;

public record CandidateAdded(string PositionId, string CandidateId, string FirstName, string LastName, byte[]? Picture, int Priority) : VotingEvent;