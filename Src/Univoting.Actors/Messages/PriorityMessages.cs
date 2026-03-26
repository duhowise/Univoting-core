using System;

namespace Univoting.Actors.Messages
{
    // Commands
    public record CreatePriority(Guid PriorityId, int Number);
    // Events
    public record PriorityCreated(Guid PriorityId, int Number);
    // Queries
    public record GetPriority(Guid PriorityId);
    // Responses
    public record PriorityDetails(Guid PriorityId, int Number);
}
