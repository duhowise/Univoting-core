using Univoting.Akka.Models;

namespace Univoting.Akka.Messages;

public record VoterStatusUpdated(string VoterId, VotingStatus Status) : VotingEvent;