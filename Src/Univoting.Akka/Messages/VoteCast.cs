namespace Univoting.Akka.Messages;

public record VoteCast(string VoterId, string CandidateId, string PositionId, DateTime Time) : VotingEvent;