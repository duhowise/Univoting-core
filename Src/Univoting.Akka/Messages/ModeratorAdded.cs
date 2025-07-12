using Univoting.Models;

namespace Univoting.Akka.Messages;

public record ModeratorAdded(Guid ElectionId, string ModeratorId, string Name, Badge Badge) : VotingEvent;