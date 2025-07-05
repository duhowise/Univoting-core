namespace Univoting.Akka.Messages;

public record CheckVoterEligibility(string VoterId, string PositionId) : VotingCommand;