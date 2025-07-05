using Univoting.Models;

namespace Univoting.Akka.Messages;

public record ModeratorAdded(string ElectionId, string ModeratorId, string Name, Badge Badge) : VotingEvent;