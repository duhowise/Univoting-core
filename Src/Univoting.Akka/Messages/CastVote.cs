namespace Univoting.Akka.Messages;

public record CastVote(string VoterId, string CandidateId, string PositionId, string ElectionId) : VotingCommand;