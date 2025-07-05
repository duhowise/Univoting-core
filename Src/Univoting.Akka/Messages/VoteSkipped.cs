namespace Univoting.Akka.Messages;

public record VoteSkipped(string VoterId, string PositionId, DateTime Time) : VotingEvent;