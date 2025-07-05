using Univoting.Models;

namespace Univoting.Akka.Messages;

public record UpdateVoterStatus(string VoterId, VotingStatus Status) : VotingCommand;