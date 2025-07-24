using Univoting.Akka.SharedModels;

namespace Univoting.Akka.Messages;

public record UpdateVoterStatus(string VoterId, VotingStatus Status) : VotingCommand;