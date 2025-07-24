using Univoting.Akka.SharedModels;

namespace Univoting.Akka.Messages;

public record AddModerator(Guid ElectionId, string ModeratorId, string Name, Badge Badge) : VotingCommand;