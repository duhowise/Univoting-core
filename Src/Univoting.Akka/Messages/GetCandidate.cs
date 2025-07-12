namespace Univoting.Akka.Messages;

public record GetCandidate(string CandidateId, Guid ElectionId) : VotingCommand;