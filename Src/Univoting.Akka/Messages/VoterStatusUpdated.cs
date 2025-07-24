using Univoting.Akka.SharedModels;

namespace Univoting.Akka.Messages;

public record VoterStatusUpdated(string VoterId, VotingStatus Status) : VotingEvent;