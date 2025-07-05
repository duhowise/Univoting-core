using Univoting.Models;

namespace Univoting.Akka.Messages;

public record AddModerator(string ElectionId, string ModeratorId, string Name, Badge Badge) : VotingCommand;