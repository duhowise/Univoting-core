namespace Univoting.Akka.Messages;

public record GetCandidate(string CandidateId) : VotingCommand;