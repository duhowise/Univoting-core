using Univoting.Models;

namespace Univoting.Akka.Messages;

public record AddModerator(Guid ElectionId, string ModeratorId, string Name, Badge Badge) : VotingCommand;