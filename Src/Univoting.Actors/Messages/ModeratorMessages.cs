using System;
using Univoting.Models;

namespace Univoting.Actors.Messages
{
    // Commands
    public record CreateModerator(Guid ModeratorId, string Name, Badge Badge, Guid ElectionId);
    // Events
    public record ModeratorCreated(Guid ModeratorId, string Name, Badge Badge, Guid ElectionId);
    // Queries
    public record GetModerator(Guid ModeratorId);
    // Responses
    public record ModeratorDetails(Guid ModeratorId, string Name, Badge Badge, Guid ElectionId);
}
