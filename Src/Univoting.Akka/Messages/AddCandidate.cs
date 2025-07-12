namespace Univoting.Akka.Messages;

public record AddCandidate(string PositionId, string CandidateId, string FirstName, string LastName, byte[]? Picture, int Priority, Guid ElectionId) : VotingCommand;