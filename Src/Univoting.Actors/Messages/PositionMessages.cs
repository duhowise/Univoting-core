using System;

namespace Univoting.Actors.Messages
{
    // Commands
    public record CreatePosition(Guid PositionId, string Name, Guid RankId, Guid ElectionId, Guid? PriorityId);
    // Events
    public record PositionCreated(Guid PositionId, string Name, Guid RankId, Guid ElectionId, Guid? PriorityId);
    // Queries
    public record GetPosition(Guid PositionId);
    // Responses
    public record PositionDetails(Guid PositionId, string Name, Guid RankId, Guid ElectionId, Guid? PriorityId);
}
